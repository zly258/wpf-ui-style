import json, re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
paths = json.load(open(ROOT / "icon_paths.json", encoding="utf-8"))

GROUPS = [
    ("1. 常用业务操作 (Actions)", [
        ("Search", "search", "搜索"),
        ("Close", "close", "关闭 / 清除"),
        ("Check", "check", "勾选 / 确认"),
        ("Plus", "add", "新增 / 添加"),
        ("Edit", "edit", "编辑 / 修改"),
        ("Trash", "delete", "删除 / 清空"),
        ("Refresh", "refresh", "刷新 / 重新执行"),
        ("Settings", "settings", "设置 / 首选项"),
        ("Filter", "filter_alt", "筛选 / 过滤"),
        ("Download", "download", "下载 / 导出"),
        ("Upload", "upload", "上传 / 导入"),
        ("Copy", "content_copy", "复制 / 剪切板"),
        ("Folder", "folder", "文件夹"),
        ("File", "description", "文档 / 图纸"),
        ("Save", "save", "保存"),
    ]),
    ("2. 媒体与视图控制 (Media & View)", [
        ("Play", "play_arrow", "开始 / 运行"),
        ("Pause", "pause", "暂停"),
        ("Stop", "stop", "停止"),
        ("ZoomIn", "zoom_in", "放大"),
        ("ZoomOut", "zoom_out", "缩小"),
        ("Visibility", "visibility", "显示 / 预览"),
        ("Translate", "translate", "翻译 / 语言"),
    ]),
    ("3. 方向与拖拽 (Navigation & Drag)", [
        ("ChevronDown", "expand_more", "展开 / 向下"),
        ("ChevronUp", "expand_less", "收起 / 向上"),
        ("ChevronLeft", "chevron_left", "上一页 / 向左"),
        ("ChevronRight", "chevron_right", "下一页 / 向右"),
        ("MoreVert", "more_vert", "更多操作"),
        ("DragIndicator", "drag_indicator", "拖拽手柄"),
    ]),
    ("4. 语义与状态提示 (Semantic Status)", [
        ("Info", "info", "信息提示"),
        ("SuccessCircle", "check_circle", "成功 / 校验通过"),
        ("WarningTriangle", "warning", "警告 / 注意"),
        ("ErrorCircle", "cancel", "错误 / 阻断"),
    ]),
]

lines = []
lines.append("    <!-- ==============================================================================")
lines.append("         Google Material Symbols — Outlined / Weight 400 / Grade 0 / Optical Size 24")
lines.append("         许可证：Apache License 2.0（可自由商用）")
lines.append("")
lines.append("         统一视觉重量：采用官方 Outlined 400 字重（非 200 细体、非 700 粗体），")
lines.append("         笔画粗细与 14.5px 正文、16px 复选框保持均衡。")
lines.append("")
lines.append("         坐标说明：官方 24px SVG 使用 viewBox=\"0 -960 960 960\"，已线性映射为")
lines.append("         0..24 的正向坐标。每条几何末尾附加零面积锚点 \"M0,0 L24,24\"，")
lines.append("         使 Stretch=\"Uniform\" 始终按 24x24 视口等比缩放，保证所有图标的")
lines.append("         光学尺寸一致（不会把 Chevron 这类小图标放大到与 Settings 同宽）。")
lines.append("         ============================================================================== -->")
lines.append("")

for title, items in GROUPS:
    lines.append(f"    <!-- {title} -->")
    lines.append("")
    for key, official, desc in items:
        d = paths[key].strip() + " M0,0 L24,24"
        lines.append(f"    <!-- {official} ({desc}) -->")
        lines.append(f'    <StreamGeometry x:Key="Icon.Geo.{key}">{d}</StreamGeometry>')
        lines.append("")
    lines.append("")

block = "\n".join(lines)

src = open(ROOT / "Theme" / "Icons.xaml", encoding="utf-8").read()
start_marker = "    <!-- =============================================================================="
end_marker = "    <!-- ============================================================\n         4. 图标基础样式"
si = src.index(start_marker)
ei = src.index(end_marker)
new = src[:si] + block + "\n" + src[ei:]
open(ROOT / "Theme" / "Icons.xaml", "w", encoding="utf-8", newline="\n").write(new)
print("spliced; icons:", len(paths))
