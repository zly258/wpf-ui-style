# Ribbon Icon Generator

24×24 极简单色线性图标体系，用于桌面工业软件、CAD/BIM 插件与 IDE 的 Ribbon 菜单。

- **一套几何，双主题**：深色 `#2D323A`/`#FFFFFF`，浅色 `#F3F3F3`/`#4A4A4A`。
- **40 个开箱图标**：文件、撤销重做、选择筛选、编辑变换、绘图标注、视图捕捉、系统设置。
- **四种产物**：SVG、WPF `StreamGeometry` + `Style`、双主题预览页、AI 绘图提示词。
- **可执行质量门**：安全边距、复杂度、虚线规范、WCAG 非文本对比度（≥ 3:1）。

## 快速开始

```bash
# 列出 / 搜索
python tools/IconGen.py list
python tools/IconGen.py search 框选

# 单个 SVG（currentColor，跟随前景）
python tools/IconGen.py svg box-select

# 批量导出 + WPF 资源字典 + 预览页
python tools/IconGen.py all --out samples/svg --size 32
python tools/IconGen.py xaml --out samples/RibbonIcons.xaml
python tools/IconGen.py preview --out preview/icons.html

# 校验（CI 可用，错误返回 1，--strict 警告返回 2）
python tools/IconGen.py check --strict

# AI 绘图提示词
python tools/IconGen.py prompt "Batch export" --theme dark --negative
```

## 新增图标

只改一处：往 `catalog/icons.json` 的 `icons` 里加一条（`kind` 支持 `path` / `rect` / `circle` / `line`），
然后重跑 `all`、`xaml`、`preview`。形状配方与反面案例见 `references/geometry-cookbook.md`。

## 安装

```powershell
pwsh ./install.ps1                        # 装到本机已存在的 AI 客户端
pwsh ./install.ps1 -Scope Project -Root "D:/YourApp"
pwsh ./install.ps1 -All -Force            # 强制装到全部客户端
```

## 目录

| 路径 | 内容 |
|---|---|
| `SKILL.md` | 规范、工作流、命令、WPF 集成、验收清单 |
| `catalog/icons.json` | 图标几何（唯一数据源） |
| `tools/IconGen.py` | 生成与校验 CLI（Python 3 标准库） |
| `samples/` | SVG 成品与 WPF 资源字典 |
| `preview/icons.html` | 深浅双主题对照页 |
| `references/` | 几何配方、提示词库 |

样式基线：同仓库 `docs/09-icon-system.md`。