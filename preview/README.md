# 可运行预览工程

把仓库里的 `Theme/` 接到一个真实 WPF 工程里，用三个窗口演示规范效果。主题文件通过
`Link` 直接引用仓库根目录的 `Theme/`，不复制副本，改主题即时生效。

## 三个窗口

| 窗口 | `-window` 值 | 内容 |
|---|---|---|
| `ShowcaseWindow.xaml` | `showcase` | 综合业务窗口：按钮层级、表单、搜索框、多行文本、列表/表格、图标阶梯、语义提示、折叠与选项卡 |
| `ControlsGalleryWindow.xaml` | `controls` | 控件全览：按钮家族、开关、滑杆、日期、进度、富文本、菜单、工具栏、树、列表、徽章、空状态 |
| `LayoutsWindow.xaml` | `layouts` | 布局模板：两列表单、双栏对照、主-细节、仪表盘网格、设置页、拆分视图 |

## 运行

交互窗口：

```bash
dotnet run --project preview -- -window showcase
dotnet run --project preview -- -window controls
dotnet run --project preview -- -window layouts
```

静默离屏渲染成 PNG（不弹窗口、不进消息循环）：

```bash
# 单个窗口
dotnet run --project preview -- -shot out/showcase.png -window showcase -scale 1.5

# 三个窗口一起
dotnet run --project preview -- -shot out/all.png -all -scale 1.0

# 展开菜单弹层（验证 MenuItem 模板）
dotnet run --project preview -- -shot out/menu.png -menu
```

| 参数 | 说明 |
|---|---|
| `-shot <path>` | 输出 PNG 路径；`-all` 时按 `<目录>/<窗口名>.png` 输出三张 |
| `-window <name>` | `showcase` / `controls` / `layouts` |
| `-scale <n>` | 渲染倍率，`1.5` 等价于 150% DPI 观感 |
| `-all` | 三个窗口各出一张 |
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
