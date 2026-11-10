#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
WPF UI 自动化渲染、交互点击测试与视觉规范自检工具。

支持能力：
1. 离屏/动态渲染任意 XAML 窗口或 UserControl 为高清 PNG 图片；
2. 自动化模拟用户交互点击（按钮、复选框、下拉框、选项卡、折叠面板等）；
3. 验证点击后的状态迁移与数据联动，并支持保存分步交互快照；
4. 静态检查 UI 元素规范（触控点击热区、文本截断 ToolTip 配套等）；
5. 同时支持人类可读的控制台报表与 CI/Agent 自动化的结构化 JSON 输出。

使用示例：
  python tools/UiTest.py --window editable-table --test-clicks
  python tools/UiTest.py --window wizard --test-clicks --shot-dir shots/wizard
  python tools/UiTest.py --xaml preview/Views/SafeFormView.xaml --shot shots/safeform.png
  python tools/UiTest.py --all --test-clicks
  python tools/UiTest.py --list
"""

import argparse
import json
import os
import shutil
import subprocess
import sys
from pathlib import Path

# 支持的内置案例与测试窗口列表
BUILTIN_WINDOWS = [
    ("overview", "案例中心首页（侧边导航 + 规范摘要 + 全部案例入口）"),
    ("controls", "基础控件全览（按钮、输入、下拉、复选、标签页、密度）"),
    ("safeform", "安全表单（防截断/防遮挡骨架）"),
    ("settings", "密集设置表单（三列并排、防截断）"),
    ("form-layout", "表单对齐（宽面板两列 / 窄面板单列，标签与控件左对齐）"),
    ("editable-table", "可编辑数据表格（全选联动、单元格编辑、行增删）"),
    ("compare", "双栏对照与日志（弹性主区 + 固定日志区）"),
    ("workbench", "工程工作台（特征树/视口/属性检查器）"),
    ("wizard", "分步向导（上一步/下一步状态机）"),
    ("dashboard", "监控看板（KPI、任务队列、告警表格）"),
    ("probe", "交互状态专项校验（日历面板、可编辑下拉框、折叠展开）"),
]


def find_dotnet():
    dotnet = shutil.which("dotnet")
    if not dotnet:
        # 尝试常见 Windows 默认路径
        default_path = Path(r"C:\Program Files\dotnet\dotnet.exe")
        if default_path.exists():
            return str(default_path)
    return dotnet


def find_project_file():
    current_dir = Path(__file__).resolve().parent
    repo_root = current_dir.parent
    candidate = repo_root / "preview" / "WpfStylePreview.csproj"
    if candidate.exists():
        return candidate
    return None


def run_test(args):
    dotnet = find_dotnet()
    if not dotnet:
        print("ERROR: 未找到 .NET SDK (dotnet)。UI 渲染与交互测试需要 .NET 8 SDK。", file=sys.stderr)
        return 2

    project_file = find_project_file()
    if not project_file:
        print("ERROR: 未找到预览项目文件 preview/WpfStylePreview.csproj。", file=sys.stderr)
        return 2

    if args.list:
        print("================================================================================")
        print("WPF UI 可用预览与测试窗口列表：")
        print("================================================================================")
        for name, desc in BUILTIN_WINDOWS:
            print(f"  - {name:<22} : {desc}")
        print("================================================================================")
        return 0

    # 构造传递给 WpfStylePreview 的 CLI 参数
    cmd = [dotnet, "run", "--project", str(project_file), "--"]

    if args.scale:
        cmd.extend(["-scale", str(args.scale)])

    if args.json:
        cmd.append("-json")

    if args.xaml:
        cmd.extend(["-xaml", str(args.xaml)])
        if args.shot:
            cmd.extend(["-shot", str(args.shot)])
        res = subprocess.run(cmd)
        return res.returncode

    if args.all and args.test_clicks:
        all_passed = True
        test_targets = ["overview", "editable-table", "wizard", "workbench", "probe"]
        print(f"开始批量运行 {len(test_targets)} 个核心业务窗口交互测试...\n")
        for win in test_targets:
            sub_cmd = list(cmd)
            sub_cmd.extend(["-window", win, "-test-clicks"])
            if args.shot_dir:
                sub_cmd.extend(["-shot-dir", str(Path(args.shot_dir) / win)])
            res = subprocess.run(sub_cmd)
            if res.returncode != 0:
                all_passed = False
            print()
        return 0 if all_passed else 1

    if args.all:
        cmd.append("-all")
        if args.shot:
            cmd.extend(["-shot", str(args.shot)])
        res = subprocess.run(cmd)
        return res.returncode

    # 单窗口模式
    target_window = args.window or "overview"
    cmd.extend(["-window", target_window])

    if args.test_clicks:
        cmd.append("-test-clicks")
    if args.verify:
        cmd.append("-verify")
    if args.shot:
        cmd.extend(["-shot", str(args.shot)])
    if args.shot_dir:
        cmd.extend(["-shot-dir", str(args.shot_dir)])

    res = subprocess.run(cmd)
    return res.returncode


def main():
    parser = argparse.ArgumentParser(
        description="WPF UI 自动化渲染、交互点击测试与视觉规范自检工具",
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )

    parser.add_argument("-w", "--window", "-Window", dest="window", type=str, help="指定待测窗口名称（参见 --list）")
    parser.add_argument("-c", "--test-clicks", "-TestClicks", dest="test_clicks", action="store_true", help="执行交互点击测试，模拟按钮/复选/下拉并断言状态迁移")
    parser.add_argument("-v", "--verify", "-Verify", dest="verify", action="store_true", help="执行视觉树布局规范与可访问性静态检查")
    parser.add_argument("-x", "--xaml", "-Xaml", dest="xaml", type=str, help="动态加载并渲染外部任意 XAML 文件（Window/UserControl）")
    parser.add_argument("-s", "--shot", "-Shot", dest="shot", type=str, help="指定最终渲染截图输出路径（如 shots/out.png）")
    parser.add_argument("-d", "--shot-dir", "-ShotDir", dest="shot_dir", type=str, help="指定分步交互快照目录（每次点击保存一张状态对比图）")
    parser.add_argument("--scale", "-Scale", dest="scale", type=float, default=1.0, help="渲染缩放比例（默认 1.0，如 1.5 或 2.0 高清输出）")
    parser.add_argument("-a", "--all", "-All", dest="all", action="store_true", help="测试或批量导出全部内置窗口")
    parser.add_argument("--json", "-Json", dest="json", action="store_true", help="输出结构化 JSON 报表（便于 CI 或 Agent 自动化集成）")
    parser.add_argument("-l", "--list", "-List", dest="list", action="store_true", help="列出所有可用的内置预览与测试窗口")

    args = parser.parse_args()
    sys.exit(run_test(args))


if __name__ == "__main__":
    main()
