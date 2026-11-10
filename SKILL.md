---
name: wpf-ui-style
description: Apply and audit the WPF UI Skills style system (tokens, three-level density, control states, layout, contrast, automated checks). Use when creating or modifying WPF/XAML windows, styles, resource dictionaries, themes, buttons, inputs, lists, data grids, dialogs, tabs, or when auditing XAML layout, contrast, and truncation issues. 适用于：WPF 视觉与交互规范、紧凑/中等/宽松三档密度、默认 12px 正文 / 30px 控件高 / 28px 表格行高、防截断表单、无多Tab窗口高度跳变、前景背景状态契约、对比度与布局自动化检查脚本。
---

# WPF UI Skills

高质量 WPF/XAML 桌面与插件 UI 视觉与交互规范、紧凑布局系统及自动化校验流水线。

---

## 核心设计原则

1. **密度可切换，默认中等 (Density-aware, Medium by Default)**：紧凑 / 中等 / 宽松三档只改变尺寸与留白，不改变颜色和字号。默认采用中等密度，一屏展示更多数据；工具条/属性面板可切紧凑，演示/触屏可切宽松。
2. **状态契约 (Explicit States)**：改 `Background` 必须显式声明 `Foreground`；禁用态严禁使用 `Opacity`；浅底必须配深字。
3. **无截断与无遮挡 (No Truncation / No Occlusion)**：内容决定尺寸，弹性容器约束，防 DPI 缩放破坏；任何文本截断必须配 `ToolTip`。
4. **无要求不用图标 (No Gratuitous Icons)**：默认采用清晰纯文本。没有明确要求使用图标时，一律不添加装饰性图标；纯图标按钮必须提供 `ToolTip`。
5. **脚本自动化闭环 (Scripted Quality Gate)**：改动后必须运行 Python 静态检查、xUnit 契约测试与真实 WPF 离屏渲染截图验证。

---

## 资产与工具索引

- `Theme/Tokens.xaml`：全局唯一色值、字体、尺寸兜底值、圆角与间距令牌。
- `Theme/Density.Compact.xaml` / `Density.Medium.xaml` / `Density.Relaxed.xaml`：三档密度；控件用 `{DynamicResource Layout.*}` 读取，可运行时切换。
- `Theme/Controls.xaml`：基础控件样式（Button, TextBox, ComboBox, CheckBox, RadioButton, GroupBox, Expander, TabControl, ToolTip, ScrollBar）。
- `Theme/DataControls.xaml`：数据控件样式（DataGrid, ListBox, ListView, TreeView）。
- `Theme/Extras.xaml`：扩展控件（Slider, DatePicker, ProgressBar, Menu, ToolBar, Badge）。
- `Theme/Icons.xaml`：24×24 Google Material Symbols 矢量几何与图标按钮（可选）。
- `Theme/Theme.xaml`：默认入口（中等密度）；`Theme.Compact.xaml` / `Theme.Relaxed.xaml` 为整包换挡入口。
- `tools/LayoutCheck.py`：布局/截断/多行文本/多Tab跳变静态规则检查（Python 3 标准库）。
- `tools/ContrastCheck.py`：WCAG 颜色对比度自动化计算与检查。
- `tests/ThemeContractTests.cs`：xUnit 契约测试（`dotnet test tests/WpfStyleContractTests.csproj`）。
- `preview/`：WPF 案例中心（左侧分类导航 + 右侧 10 个 UserControl 案例页），支持 `-page` 选页、`-density` 换密度与 `-shot` 离屏截图。

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

### 2. 三档密度与尺寸阶梯

| 令牌名 | 紧凑 | 中等（默认） | 宽松 | 说明 |
|---|---:|---:|---:|---|
| `Layout.ControlHeight` | 26 | **30** | 34 | 按钮 / 单行输入 / 下拉 / 日期 |
| `Layout.ControlHeight.Small` | 24 | **26** | 30 | 工具条 / 表格行内控件 |
| `Layout.ControlHeight.Large` | 32 | **36** | 40 | 主操作 / 触屏 |
| `Layout.RowHeight` | 24 | **28** | 32 | DataGrid / ListBox 行高 |
| `Layout.HeaderHeight` | 26 | **30** | 34 | 表格列头 |
| `Layout.TabPadding` | 10,4 | **12,6** | 16,8 | 标签页内边距 |
| `Layout.ButtonPadding` | 10,0 | **12,0** | 16,0 | 按钮左右内边距（上下恒为 0） |
| `Layout.InputPadding` | 4,0 | **6,0** | 8,0 | 单行输入左右内边距（不设上下） |
| `Layout.WindowPadding` | 8 | **12** | 16 | 窗口根内边距 |
| `Layout.GroupPadding` | 8 | **10** | 14 | GroupBox 内容内边距 |
| `Layout.FormRowMargin` | 0,2 | **0,3** | 0,5 | 表单行上下间距 |
| `Layout.ListItemPadding` | 6,4 | **8,5** | 10,6 | 列表项内边距 |
| `Layout.ProgressBarHeight` | 6 | **8** | 10 | 进度条高度 |
| `Layout.CheckBox.Size` / `RadioButton.Size` | 16 | **16** | 16 | 固定靶心，不随密度缩小 |
| `Layout.ButtonMinWidth` | 56 | **64** | 72 | 标准按钮最小宽度 |
| `Layout.ButtonMinWidth.Small` | 40 | **48** | 56 | 小号按钮最小宽度 |
| `Layout.FormLabelMinWidth` | 80 | **80** | 80 | 标签列最小宽度（配合 `Auto`） |
| `Layout.FormLabelMaxWidth` | 116 | **132** | 148 | 长标签换行上限（加在 TextBlock 上，不加在列上） |
| `Layout.InputMinWidth` | 140 | **140** | 140 | 输入列最小宽度（配合 `*`） |
| `Layout.WindowMinWidth` | 520 | **520** | 520 | 窗口最小宽度 |
| `Layout.LogMaxHeight` | 220 | **220** | 220 | 日志框默认最大高度 |

> 颜色、字号、圆角、CheckBox 靶心尺寸与最大高度**不随密度变化**。
> 完整键集与切换方式见 `docs/06-density.md`。

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

### R7. 三档密度统一，同界面禁止混用
- 全项目提供 **紧凑 / 中等 / 宽松** 三档密度，默认 **中等**。同一窗口只允许一档，禁止 26px 与 34px 控件随机混排。
- 密度只改尺寸、内边距、行高；**颜色、字号、圆角、CheckBox 靶心（16px）不随密度变化**。
- 业务 XAML 禁止写死控件 `Height` / `Padding`（如 `Height="32"`、`Padding="8,0"`）；一律引用 `Layout.*` 令牌。
- 主题控件内部必须用 `{DynamicResource Layout.*}` 读取密度 token，才能在运行时换挡；业务 XAML 用 `{StaticResource Layout.*}` 即可（合并字典时已解析到当前档位）。

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
1. **表格编辑态**：行高取 `Layout.RowHeight`；单元格编辑器使用 `Height="Auto"` 跟随行高伸展，文字上下居中，底边框完整。
2. **折叠面板展开**：展开长内容时内容在面板内滚动，不撑爆宿主窗口，不把底部操作栏挤出可视区。
3. **下拉/日期弹层**：弹层展开后内容完整，不遮挡关键输入，日历按钮不覆盖文字。

### R14. 功能分区与单一动作入口，严禁入口混乱 (Four-Zone & Single Action Entry)
- **严格四大功能分区**：
  1. **Zone 1 页头区**：主标题 + 关联对象标签 + 全局文档操作（`保存变更`、`导出清单`）；
  2. **Zone 2 范围筛选区**：纯粹的范围收窄输入（搜索、材质、状态、重置），**严禁在筛选区放置新增/审批等业务修改动作**；常用筛选控制在 3~4 组以内，超长必须折叠；
  3. **Zone 3 工作区工具栏**：左侧为局部/批量操作（**有选中项时才点亮**，并跟随 `已选中 N 项` 状态），右侧为表格视图控制（`展开/折叠明细`），**严禁在此处重复放置全局“保存”或“导出”入口**；
     - **必须有容器**：批量操作条统一用 `ToolStrip`（Surface 底 + 1px 描边 + `Layout.ToolBarPadding`）包住按钮组，不要直接浮在窗口背景上；容器左边缘与下方表格、上方筛选卡对齐；
     - **选中数用胶囊徽章**：未选中 `SurfaceAlt + Text.Secondary`，选中后切 `Accent.Subtle + Accent`，不要只留一行灰色小字；
     - **禁用态的危险动作必须中性化**：`Button.Danger` 禁用时用 `Control.Disabled / Text.Disabled / Border`，**不得保留红色**，否则零选中时“批量删除”看起来仍可用；
  4. **Zone 4 底部汇总与结算区**：负责统计指标、分页控制器或全局确定/取消。
- **单一真理入口**：同一语义的动作（如“导出”、“保存”）全窗口只允许出现一处权威入口，严禁页头放导出、工具栏放导出、底栏又放导出造成入口混乱。

### R15. 界面文案零冗余，严禁 AI 教学废话 (Zero-Redundancy Content)
- **剔除 AI 客套与教学废话**：严禁在标题下方放置“演示...”、“您可以在这里查看与管理...”等无意义说明段落；
- **精炼标签不复读**：字段 Label（“材质”）、下拉项（“全部材质”）、表头（“状态”）与单元格徽章（“已审核”）保持高度凝练，严禁在徽章中重复拼接“状态: 已审核”。

### R16. 输入区占满、标签页可识别 (Fill & Visible Tabs)
- **输入区横向占满**：`TextBox` / `PasswordBox` / `ComboBox` 的文本宿主必须横向 `Stretch`。严禁在业务 XAML 里把 `HorizontalContentAlignment` 改成 `Left`，否则右侧会是无法点击的空白，长值也更容易被截。
- **标签页必须是「可见的方框」**：默认 `TabItem` 用原生方角样式——未选中也有 1px `Brush.Border.Control` 边框 + `SurfaceAlt` 底；选中页签铺 `Brush.Surface`、去掉下边框，与内容区连成一体。**禁止圆角、禁止底部强调条/下划线指示器**；浅色卡片上只画一条线无法看出这是可切换的页签。
- **标签页仍是局部切换**：R12 的「窗口内禁止多 Tab 导航」继续生效；本样式用于分组设置、属性分类、画廊演示等局部区域。
- **按钮边界可见**：默认按钮是「白底 + `Brush.Border.Control` 描边」。严禁把按钮描边改成 `Brush.Border` 或更浅的颜色，也不要用 `Opacity` 弱化。

### R17. 表单对齐与窄面板 (Form Alignment & Narrow Panels)
1. **宽面板（≥ 520 DIP）用两列**：标签列 `Width="Auto"` + `MinWidth="{StaticResource Layout.FormLabelMinWidth}"`，输入列 `Width="*"` + `MinWidth="{StaticResource Layout.InputMinWidth}"`；同一个表单**只定义一个 Grid**，所有行共享列定义。禁止每行一个 Grid、各自算标签宽度。
2. **窄面板（< 520 DIP）用单列**：标签在上、控件在下，统一使用 `FormStack` + `Text.FieldLabel`：
   ```xml
   <StackPanel Style="{StaticResource FormStack}">
       <TextBlock Text="服务地址" Style="{StaticResource Text.FieldLabel}" />
       <TextBox Text="{Binding Url}" />
   </StackPanel>
   ```
   宿主可能是停靠窗、属性面板或可缩放侧栏时，优先用单列，避免输入列被 `MinWidth` 挤出可视区。
3. **控件内容左对齐**：`TextBox` / `PasswordBox` / `ComboBox` 内部文字默认左对齐；禁止设置 `HorizontalContentAlignment="Center"` 或 `TextAlignment="Center"`（数值列需要右对齐时用 `TextAlignment="Right"` 并在表头说明）。
4. **GroupBox 标题与内容同基线**：主题模板把标题和内容放在同一内边距容器里，标题、标签、控件共享左基线；业务 XAML 不要再用负 `Margin` 手工对位。
5. **不要用空白占位**：禁止用 `Margin` / 固定 `Width` / 空 `TextBlock` 把标签或控件"推到"期望位置；对齐应由 Grid 列或 `FormStack` 决定。

---

## 自动化验证流水线 (Verification Pipeline)

AI 与开发者在交付任何 WPF 改动前，必须依次执行以下自动化检查：

```bash
# 1. 静态布局与反模式检查（必须 0 Error；同时校验密度档案完整性与 DynamicResource 接线）
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
python tools/UiTest.py --xaml preview/Views/SafeFormView.xaml --shot .tmp-shots/verify.png
```

---

## 交付自查清单 (Review Checklist)

- [ ] 界面没有使用硬编码 `#HEX` 颜色，全部引用 `Brush.*`。
- [ ] 默认排版为中等密度（Body 12px, ControlHeight 30px, 表格行 28px, 表单行距 3px, 根边距 12px），未混用其它档位。
- [ ] 控件尺寸/内边距引用 `Layout.*`，没有写死 `Height="32"` / `Padding="8,0"`；主题内部密度 token 使用 `DynamicResource`。
- [ ] 单行/多行输入与下拉框内容区横向占满，右侧空白可点击聚焦。
- [ ] 标签页是方角页签：每个页签边框可见，选中页签与内容区连通，无圆角/无底部强调条。
- [ ] 表单标签与控件共享左基线：宽面板单 Grid 两列，窄面板 `FormStack` 单列。
- [ ] 输入框/下拉框内部文字左对齐（数值列允许右对齐），没有居中。
- [ ] 复选框对勾是填充路径且居中；三态可区分（`IsChecked=null` 显示横杠，不是空框）。
- [ ] 默认按钮使用 `Brush.Border.Control` 描边（≥3:1），悬停切到 Accent 描边。
- [ ] 未要求使用图标的按钮均为纯文本按钮，无多余装饰性矢量 Path。
- [ ] 纯图标按钮均包含有效 `ToolTip`。
- [ ] 标签列 `Auto` + `MinWidth`，输入列 `*` + `MinWidth`，无硬编码输入框宽度。
- [ ] 窗口高度由 `SizeToContent="Height"` 弹性驱动，内容区带单一滚动保护。
- [ ] 底部操作栏独占一行，长状态文本独占操作栏上方一行。
- [ ] 窗口无未固定高度的多 Tab 切换。
- [ ] 禁用态无 `Opacity`，改背景必配前景，浅底深字无白字压浅蓝。
- [ ] `LayoutCheck.py` 0 错误；`ContrastCheck.py` 全部通过；契约测试全部通过。
