# tools 目录说明

## 1. 脚本分工

| 文件 | 语言 | 职责 |
|---|---|---|
| `ContrastCheck.py` | Python 3（仅标准库） | WCAG 对比度检查，逻辑唯一实现 |
| `LayoutCheck.py` | Python 3（仅标准库） | XAML 布局反模式扫描，逻辑唯一实现 |
| `ContrastCheck.ps1` | PowerShell | **薄包装**：探测 `py -3` → `python3` → `python` 并转调 `.py` |
| `LayoutCheck.ps1` | PowerShell | **薄包装**：同上 |
| `_fetch_material_icons.py` | Python 3 | 从 Google Fonts 抓取 Material Symbols Outlined 并转换到 24 视口 |
| `_splice_icons.py` | Python 3 | 将转换后的几何写入 `Theme/Icons.xaml` |
| `../install.ps1` | PowerShell | 唯一保留的安装脚本：复制 skill、写客户端目录，零依赖 |

## 2. 为什么不是“全部 Python”

- Windows 自带 PowerShell 5.1，但不保证有 Python；`install.ps1` 是新机器上的第一步，必须零依赖。
- 校验类脚本是 XML/正则/颜色计算，Python 更可靠、可测试、可跨平台复用。
- `.ps1` 只做参数转发，保证文档里的 `pwsh ./tools/*.ps1` 命令继续可用。

## 3. 使用方式

两种调用等价：

```powershell
# 推荐（与文档一致，自动找 Python）
pwsh ./tools/ContrastCheck.ps1
pwsh ./tools/LayoutCheck.ps1 -Root "D:\your\project" -Strict
```

```bash
# 直接运行（CI / Linux / macOS）
python tools/ContrastCheck.py
python tools/LayoutCheck.py --root . --strict
```

参数同时兼容 PowerShell 风格（`-Root`、`-Strict`、`-TokensPath`）和 Unix 风格（`--root`、`--strict`、`--tokens`）。

## 4. 依赖与退出码

- 仅使用 Python 3 标准库，无需 `pip install`。
- 退出码：`0` 通过；`1` 校验失败；`2`（仅 `.ps1` 包装）未找到 Python 3。

## 5. 扩展图标

```bash
python tools/_fetch_material_icons.py   # 抓取并转换到 24 视口，输出 icon_paths.json
python tools/_splice_icons.py           # 写入 Theme/Icons.xaml
```

新增图标必须保持 **Outlined / Weight 400 / Grade 0 / Optical Size 24**，并保留末尾 `M0,0 L24,24` 边界锚点。
