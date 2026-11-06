# 可运行预览工程

把仓库里的 `Theme/` 接到一个真实 WPF 工程里，用多个窗口演示规范效果。主题文件通过
`Link` 直接引用仓库根目录的 `Theme/`，不复制副本，改主题即时生效。

## 四个窗口

| 窗口 | `-window` 值 | 内容 |
|---|---|---|
| `ShowcaseWindow.xaml` | `showcase` | 综合业务窗口：按钮层级、表单、搜索框、多行文本、列表/表格、图标阶梯、语义提示、折叠与选项卡 |
| `ControlsGalleryWindow.xaml` | `controls` | 控件全览：按钮家族、开关、滑杆、日期、进度、富文本、菜单、工具栏、树、列表、徽章、空状态 |
| `LayoutsWindow.xaml` | `layouts` | 布局模板：两列表单、双栏对照、主-细节、仪表盘网格、设置页、拆分视图 |
| `SafeFormWindow.xaml` | `safeform` | 安全表单骨架（规则 R14）：标签不裁、值不藏、状态文案与按钮栏分行、高 DPI 可滚动 |
| `AntiPatternWindow.xaml` | `anti` | 反模式对照（故意保留旧的文档属性设置写法）：固定标签列宽、固定窗口高度、状态与按钮同行 |
| `ProbeWindow.xaml` | `probe` | 交互态探针：表格编辑态、折叠面板长内容（滚动条）、日期控件（自绘模板 + 日历弹层），并打印自检结果 |
| `CaseSettingsWindow.xaml` | `case-settings` | 案例 1 · 密集设置表单：三列并排骨架、行距 3px、控件高 32px，一屏看到更多字段 |
| `CaseTableWindow.xaml` | `case-table` | 案例 2 · 数据表格：工具栏 + 26px 行高表格 + 分页条 |
| `CaseCompareWindow.xaml` | `case-compare` | 案例 3 · 双栏对照 + 日志：主内容占 * 行、日志固定高度、底部操作栏不被顶出 |

## 运行

交互窗口：

```bash
dotnet run --project preview -- -window showcase
dotnet run --project preview -- -window controls
dotnet run --project preview -- -window layouts
dotnet run --project preview -- -window safeform
dotnet run --project preview -- -window anti
dotnet run --project preview -- -window probe
dotnet run --project preview -- -window case-settings
dotnet run --project preview -- -window case-table
dotnet run --project preview -- -window case-compare
```

静默离屏渲染成 PNG（不弹窗口、不进消息循环）：

```bash
# 单个窗口
dotnet run --project preview -- -shot out/showcase.png -window showcase -scale 1.5

# 全部窗口各出一张
dotnet run --project preview -- -shot out/all.png -all -scale 1.0

# 展开菜单弹层（验证 MenuItem 模板）
dotnet run --project preview -- -shot out/menu.png -menu
```

| 参数 | 说明 |
|---|---|
| `-shot <path>` | 输出 PNG 路径；`-all` 时按 `<目录>/<窗口名>.png` 输出全部窗口 |
| `-autoclose <秒>` | 实机冒烟：窗口渲染完成后自动关闭、进程退出（不留残余窗口） |
| `-window <name>` | `showcase` / `controls` / `layouts` / `safeform` / `anti` / `probe` / `case-settings` / `case-table` / `case-compare` |
| `-scale <n>` | 渲染倍率，`1.5` 等价于 150% DPI 观感 |
| `-all` | 全部窗口各出一张 |
| `-menu` | 展开控件全览窗口的菜单，把弹层合成到图上 |

离屏渲染会先把窗口内容撑到自然高度再截图，所以整页内容都在一张图里，而不是只截一屏。

## 已知限制

`Popup` 类内容（菜单下拉、ComboBox 弹层、ToolTip）需要真实窗口句柄才会填充
`IsItemsHost` 面板，离屏模式下不会生成子项。这是 WPF 的固有行为，不是模板缺陷；
要看弹层样式请跑交互窗口。

## 文件说明

- `App.xaml` — 只合并 `Theme/Theme.xaml`，全程序集生效。
- `../Theme/Extras.xaml` — 补充控件样式：`Slider`、`DatePicker`、`RichTextBox`、
  `ToggleButton`、`ToggleSwitch`、`Menu`/`MenuItem`、`ToolBar`、`Card`、`Badge`。
  已由 `Theme.xaml` 合并，业务项目无需单独引用。
- `App.xaml.cs` — 入口点与离屏渲染器（`-shot` / `-all` / `-menu`）。

## 校验

```bash
python tools/LayoutCheck.py --root preview   # 0 错误
python tools/ContrastCheck.py                # 全部通过
```
