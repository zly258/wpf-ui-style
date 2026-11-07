---
name: wpf-ui-style
description: Apply and audit the WPF UI Skills style system (tokens, control states, layout, contrast, automated checks). Use when creating or modifying WPF/XAML windows, styles, resource dictionaries, themes, buttons, inputs, lists, data grids, dialogs, or when auditing XAML layout, contrast, and truncation issues. 适用于：WPF 视觉与交互规范、紧凑布局（默认 12px 正文 / 32px 控件高 / 3px 行距）、防截断表单、无多Tab窗口高度跳变、前景背景状态契约、对比度与布局自动化检查脚本。
---

# WPF UI Skills

高质量 WPF/XAML 桌面与插件 UI 视觉与交互规范、紧凑布局系统及自动化校验流水线。

---

## 核心设计原则

1. **紧凑优先 (Compact by Default)**：默认采用紧凑排版，一屏展示更多数据与信息。拒绝大字号与空旷内边距。
2. **状态契约 (Explicit States)**：改 `Background` 必须显式声明 `Foreground`；禁用态严禁使用 `Opacity`；浅底必须配深字。
3. **无截断与无遮挡 (No Truncation / No Occlusion)**：内容决定尺寸，弹性容器约束，防 DPI 缩放破坏；任何文本截断必须配 `ToolTip`。
4. **无要求不用图标 (No Gratuitous Icons)**：默认采用清晰纯文本。没有明确要求使用图标时，一律不添加装饰性图标；纯图标按钮必须提供 `ToolTip`。
5. **脚本自动化闭环 (Scripted Quality Gate)**：改动后必须运行 Python 静态检查、xUnit 契约测试与真实 WPF 离屏渲染截图验证。

---

## 资产与工具索引

- `Theme/Tokens.xaml`：全局唯一色值、字体、尺寸阶梯、圆角与间距令牌。
- `Theme/Controls.xaml`：基础控件样式（Button, TextBox, ComboBox, CheckBox, RadioButton, GroupBox, Expander, TabControl, ToolTip, ScrollBar）。
- `Theme/DataControls.xaml`：数据控件样式（DataGrid, ListBox, ListView, TreeView）。
- `Theme/Extras.xaml`：扩展控件（Slider, DatePicker, ProgressBar, Menu, ToolBar, Badge）。
- `Theme/Icons.xaml`：24×24 Google Material Symbols 矢量几何与图标按钮（可选）。
- `Theme/Theme.xaml`：一站式合并入口字典。
- `tools/LayoutCheck.py`：布局/截断/多行文本/多Tab跳变静态规则检查（Python 3 标准库）。
- `tools/ContrastCheck.py`：WCAG 颜色对比度自动化计算与检查。
- `tests/ThemeContractTests.cs`：xUnit 契约测试（`dotnet test tests/WpfStyleContractTests.csproj`）。
- `preview/`：WPF 预览工程，支持交互运行与 `-shot` 离屏多倍率截图（`dotnet run --project preview -- -shot ...`）。

---

## 设计令牌与尺寸基准

### 1. 字体阶梯 (Microsoft YaHei)

| 令牌名 | 尺寸 (px) | 行高 | 适用场景 |
|---|---|---|---|
| `FontSize.Display` | 16 | 22 | 产品标题 / 极少使用的展示标题 |
| `FontSize.Title` | 13 | 18 | 窗口主标题 / 分组标题（加粗） |
| `FontSize.Section` | 13 | 18 | 次级分组标头 / 卡片头 |
| `FontSize.Body` | 12 | 18 | **正文默认**：标签、输入框、表格单元格、按钮文本 |
| `FontSize.Caption` | 11.5 | 17 | 辅助说明、状态栏文案、底部说明 |
| `FontSize.Code` | 11.5 | 17 | 日志、代码、路径、控制台等宽输出 |
| `FontSize.Micro` | 10 | 14 | 极少极小徽章或辅助角标 |

### 2. 控件尺寸与间距阶梯

| 令牌名 | 尺寸 (px) | 说明 |
|---|---|---|
| `Layout.ControlHeight` | 32 | **默认控件高度**：按钮、单行输入框、下拉框、密码框 |
| `Layout.ControlHeight.Small` | 28 | **紧凑/工具条控件高度**：工具条按钮、小型输入框 |
| `Layout.RowHeight` | 26 | 数据表格（DataGrid）/ 列表（ListBox）默认行高 |
| `Layout.HeaderHeight` | 28 | 表格列头（ColumnHeader）高度 |
| `Layout.CheckBox.Size` | 16×16 | 复选框与单选框靶心尺寸（不可缩放至 12~14px） |
| `Layout.WindowPadding` | 12 | 窗口根内边距（外边框到内容边距） |
| `Layout.GroupPadding` | 10 | GroupBox 内容内边距 |
| `Layout.FormRowMargin` | 0,3,0,3 | **表单行间距**（紧凑默认上下各 3px） |
| `Layout.ColumnGap` | 10 (`GridLength`) | 多列表单列间距 |
| `Layout.ButtonMinWidth` | 64 | 标准按钮最小宽度 |
| `Layout.ButtonMinWidth.Small`| 48 | 小号/文本按钮最小宽度 |
| `Layout.ButtonPadding` | 8,0 | 按钮内边距 |
| `Layout.InputPadding` | 5,0 | 单行输入框内边距 |
| `Layout.InputPadding.Multiline` | 6,5 | 多行输入框内边距 |
| `Layout.FormLabelMinWidth` | 80 | 标签列最小宽度（配合 `Auto` 弹性扩展） |
| `Layout.InputMinWidth` | 140 | 输入列最小宽度（配合 `*` 弹性填充） |
| `Layout.WindowMinWidth` | 520 | 窗口最小宽度 |
| `Layout.LogMaxHeight` | 220 | 日志框默认最大高度限制 |

---

## 强制规则 (Mandatory Rules)

### R1. 改 Background 必须改 Foreground
在控件样式与触发器中，一旦触发器改变了 `Background`，必须在同一触发器显式声明 `Foreground`（以及需要的 `BorderBrush`）。
严禁出现只改背景导致前景色不可控的问题。

### R2. 禁用态严禁使用 Opacity
```xml
<!-- 错误 -->
<Trigger Property="IsEnabled" Value="False">
    <Setter Property="Opacity" Value="0.45" />
</Trigger>

<!-- 正确 -->
<Trigger Property="IsEnabled" Value="False">
    <Setter Property="Background" Value="{StaticResource Brush.Control.Disabled}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Disabled}" />
    <Setter Property="BorderBrush" Value="{StaticResource Brush.Border}" />
</Trigger>
```

### R3. 隐式 TextBlock 必须动态继承 Foreground
全局隐式 `TextBlock` 与 `AccessText` 样式必须绑定到最近父级上下文的 `(TextElement.Foreground)`，`FallbackValue` 为 `Brush.Text.Primary`。
严禁硬编码 `Foreground` 为固定色，否则主按钮（蓝底白字）内的 TextBlock 会被强制置为深灰字。

### R4. 白字只能出现在深色背景上
- **允许白字**：`Brush.Accent` 系列（主按钮）、`Brush.Danger` 系列（危险操作）、`Brush.Tooltip.Background`。
- **严禁白字**：`Brush.Selection`（浅蓝底）、`Brush.Control.Hover`（浅灰底）、`Brush.Surface`（面板白底）。

### R5. 选中 + 悬停必须使用 MultiTrigger
列表与表格的 `IsSelected=True` 与 `IsMouseOver=True` 必须由 `MultiTrigger` 明确设置背景为 `Brush.Selection.Hover`、文字为 `Brush.Text.Primary`，严禁产生白字压浅底。

### R6. 业务 XAML 必须使用 Token，严禁字面量
业务界面中严禁硬编码 `#RRGGBB`、硬编码 `FontFamily`、硬编码字体字面量（如 `FontSize="13"`）或任意内边距；统一引用 `Theme/Tokens.xaml`。

### R7. 统一控件高度，同界面禁止混用
标准对话框统一使用 32px 高度（`Layout.ControlHeight`）；工具条或极窄嵌入式面板统一使用 28px 高度（`Layout.ControlHeight.Small`）。同一视图内严禁 28px 与 32px 控件随机混排。

### R8. 单一主按钮原则
每个对话框或主区域**最多允许一个** `Button.Primary`（主强调操作，回车默认）；其余操作均为 `Button.Secondary`、`Button.Text` 或 `Button.Danger`。

### R9. 多行文本框与网格容器弹性自适应，严禁强制过大 MinHeight
- 表单中的多行输入框（`TextBox.Multiline`）放在 `Grid` 的 `Height="Auto"` 行中，设 `VerticalAlignment="Top"`，由内容自然伸展；严禁在样式中写死全局 `MinHeight="100"` 导致界面被强行撑高。
- 对照视图、日志视图、数据表格放在 `Grid` 的 `Height="*"` 行，设 `VerticalAlignment="Stretch"` 配合内部自带滚动条；**严禁在 `DataGrid`、`Grid` 或 `*` 弹性 `RowDefinition` 上硬编码过大的 `MinHeight`（>=180px）**，避免在低分辨率（1366x768 / 150% DPI）下弹性行无法收缩，将底部分页栏或操作按钮挤出可视区。
- 严禁在控件上写 `Height="*"`（`*` 只能用于 RowDefinition / ColumnDefinition）。

### R10. 没有明确要求不用图标 (No Gratuitous Icons)
- **默认纯文本**：按钮与界面控件默认使用清晰凝练的纯文本（如 `Content="导出"`、`Content="保存"`），严禁在未经用户明确要求时自作主张为按钮嵌套 `<StackPanel><Path .../><TextBlock .../></StackPanel>` 等装饰图标。
- **允许图标场景**：
  1. 用户明确提出使用图标。
  2. 工具栏或表格行内的**纯图标按钮**（使用 `Button.Icon` / `Button.Icon.Small`，且**必须设置 ToolTip**）。
- 若使用图标，必须基于 Google Material Symbols 24×24 Outlined 矢量 Path，通过 `Theme/Icons.xaml` 引用，并确保其颜色绑定自 `(TextElement.Foreground)`。

### R11. 无截断、无遮挡表单骨架 (Safe Form Layout)
1. **标签列**：使用 `Width="Auto"` + `MinWidth="{StaticResource Layout.FormLabelMinWidth}"`。严禁写死固定像素列宽导致长标签被裁切。
2. **输入列**：使用 `Width="*"` + `MinWidth="{StaticResource Layout.InputMinWidth}"`。严禁在输入框上写死固定 `Width`。
3. **截断必须配 ToolTip**：凡使用 `TextTrimming="CharacterEllipsis"` 或 `WordEllipsis` 的文本，必须同时绑定 `ToolTip`。
4. **窗口类型与自适应**：
   - **纯表单/弹窗**：窗口使用 `SizeToContent="Height"` + 低阈值 `MinHeight="320"`；内容区包裹在单一垂直 `ScrollViewer`（`VerticalScrollBarVisibility="Auto"`）内，禁止嵌套多层滚动。
   - **数据表格/工作台等视图型窗口**：严禁使用 `SizeToContent="Height"`（避免破坏 DataGrid 虚拟化并将窗口撑爆）；指定建议初始宽高（如 1180×680）配合低保底 `MinHeight="360"`~`420"`，由 `*` 弹性行内的控件内置滚动条自适应。
5. **状态文案与按钮分行**：底部按钮栏固定在独立的底行；路径、处理状态等可能很长的状态文本必须单独占按钮栏上方的一行，严禁与按钮抢同行。
6. **严禁遮挡写法**：严禁负 `Margin`（如 `Margin="-4,0,0,0"`），严禁使用 `Canvas` 绝对坐标排列表单。

### R12. 窗口内禁止多 Tab 导航；Tab 切换不得让窗口高度跳变
- 窗口内禁止使用 `TabControl` 做多任务页面导航（并排任务应拆分独立窗口或使用主从/折叠面板）。
- **画廊/展示类例外**：演示控件的窗口若包含 `TabControl`，**必须显式指定固定 `Height`**（并在 Tab 内容区内部滚动），严禁在 `SizeToContent="Height"` 窗口中使用无固定高度的 `TabControl`，防止切换 Tab 时窗口产生上下跳变。

### R13. 交互态必须验证完整
改动控件或表格后必须验证：
1. **表格编辑态**：单元格进入编辑状态时，内置编辑器高度必须 ≤ 行高（26px/32px），文字上下居中，底边框完整。
2. **折叠面板展开**：展开长内容时内容在面板内滚动，不撑爆宿主窗口，不把底部操作栏挤出可视区。
3. **下拉/日期弹层**：弹层展开后内容完整，不遮挡关键输入，日历按钮不覆盖文字。

---

## 自动化验证流水线 (Verification Pipeline)

AI 与开发者在交付任何 WPF 改动前，必须依次执行以下自动化检查：

```bash
# 1. 静态布局与反模式检查（必须 0 Error）
python tools/LayoutCheck.py --root . --theme-dir Theme

# 2. 颜色对比度自动化检查（必须全部通过）
python tools/ContrastCheck.py

# 3. 契约测试（验证令牌与断言）
dotnet test tests/WpfStyleContractTests.csproj

# 4. 预览工程编译验证
dotnet build preview/WpfStylePreview.csproj

# 5. UI 自动化交互点击、状态机断言与分步快照（测试真实按钮/复选/下拉联动）
python tools/UiTest.py --all --test-clicks

# 6. 动态渲染指定 XAML 界面并执行规范自检
python tools/UiTest.py --xaml preview/SafeFormWindow.xaml --shot .tmp-shots/verify.png
```

---

## 交付自查清单 (Review Checklist)

- [ ] 界面没有使用硬编码 `#HEX` 颜色，全部引用 `Brush.*`。
- [ ] 默认排版紧凑（Body 12px, ControlHeight 32px, 行间距 3px, 根边距 12px）。
- [ ] 未要求使用图标的按钮均为纯文本按钮，无多余装饰性矢量 Path。
- [ ] 纯图标按钮均包含有效 `ToolTip`。
- [ ] 标签列 `Auto` + `MinWidth`，输入列 `*` + `MinWidth`，无硬编码输入框宽度。
- [ ] 窗口高度由 `SizeToContent="Height"` 弹性驱动，内容区带单一滚动保护。
- [ ] 底部操作栏独占一行，长状态文本独占操作栏上方一行。
- [ ] 窗口无未固定高度的多 Tab 切换。
- [ ] 禁用态无 `Opacity`，改背景必配前景，浅底深字无白字压浅蓝。
- [ ] `LayoutCheck.py` 0 错误；`ContrastCheck.py` 全部通过；契约测试全部通过。
