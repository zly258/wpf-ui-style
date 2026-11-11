# 04. 常见错误与修复

本页记录实际项目中反复出现的 WPF 样式问题。每条都给出：症状、根因、修复、验证方法。

---

## 1. 主按钮蓝底深灰字

**症状**

`Button.Primary` 背景是品牌蓝，但文字是深灰，不是白色。

**根因**

全局隐式 `TextBlock` 样式把 `Foreground` 硬编码成正文色：

```xml
<Style TargetType="TextBlock">
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Primary}" />
</Style>
```

隐式样式（Setter）优先级高于属性继承。`ContentPresenter` 为字符串内容生成的
`TextBlock` 会被这条样式命中，于是按钮自己的 `Foreground="White"` 被覆盖。

**修复**

让默认前景跟随最近的内容宿主：

```xml
<Style TargetType="TextBlock">
    <Setter Property="Foreground"
            Value="{Binding Path=(TextElement.Foreground),
                    RelativeSource={RelativeSource AncestorType={x:Type ContentPresenter}},
                    FallbackValue={StaticResource Brush.Text.Primary}}" />
</Style>

<Style TargetType="AccessText">
    <Setter Property="Foreground"
            Value="{Binding Path=(TextElement.Foreground),
                    RelativeSource={RelativeSource AncestorType={x:Type ContentPresenter}},
                    FallbackValue={StaticResource Brush.Text.Primary}}" />
</Style>
```

同时确保按钮模板的 `ContentPresenter` 传递前景：

```xml
<ContentPresenter TextElement.Foreground="{TemplateBinding Foreground}"
                  RecognizesAccessKey="True" />
```

**验证**

- 写一个契约测试：隐式 `TextBlock` 的 `Foreground` 必须包含 `(TextElement.Foreground)`。
- 运行 `tests/ThemeContractTests.cs`。

---

## 2. 禁用按钮几乎看不见

**症状**

主按钮禁用后整体变淡，白字几乎消失。

**根因**

用透明度表示禁用：

```xml
<Trigger Property="IsEnabled" Value="False">
    <Setter Property="Opacity" Value="0.45" />
</Trigger>
```

蓝底和白字一起变成 45% 透明，叠在白色/浅灰背景上，对比度极低。

**修复**

显式设置禁用背景、前景、边框：

```xml
<Trigger Property="IsEnabled" Value="False">
    <Setter Property="Background" Value="{StaticResource Brush.Control.Disabled}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Disabled}" />
    <Setter Property="BorderBrush" Value="{StaticResource Brush.Border}" />
    <Setter Property="Cursor" Value="Arrow" />
</Trigger>
```

本规范 `TextDisabled=#667085` 在 `ControlDisabled=#F2F4F7` 上约 4.51:1。

**验证**

- 对比度脚本包含 `TextDisabled / ControlDisabled`，最低要求 3:1。
- 截图检查禁用主按钮仍能读出文字。

---

## 3. 悬停后文字消失

**症状**

鼠标移到按钮/列表项上，背景变浅，但文字还是白色，几乎看不见。

**根因**

只改背景，不改前景：

```xml
<Trigger Property="IsMouseOver" Value="True">
    <Setter Property="Background" Value="{StaticResource Brush.Control.Hover}" />
</Trigger>
```

**修复**

同一个触发器内同时设置前景：

```xml
<Trigger Property="IsMouseOver" Value="True">
    <Setter Property="Background" Value="{StaticResource Brush.Control.Hover}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Primary}" />
</Trigger>
```

**验证**

- 检查每个 `Background` 触发器是否有对应 `Foreground`。
- 用 `rg` 搜索 `Property="Background"`，逐条核对。

---

## 4. 选中项白字压浅蓝底

**症状**

ListBox/ListView 选中项是浅蓝背景，但文字是白色，读不清。

**根因**

选中态设置了白色前景，悬停/选中背景却是浅色：

```xml
<Trigger Property="IsSelected" Value="True">
    <Setter Property="Background" Value="{StaticResource Brush.Selection}" />
    <Setter Property="Foreground" Value="White" />
</Trigger>
```

**修复**

浅底深字：

```xml
<Trigger Property="IsSelected" Value="True">
    <Setter Property="Background" Value="{StaticResource Brush.Selection}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Primary}" />
</Trigger>
```

如果确实要品牌蓝底白字，必须同时定义：

```xml
<Trigger Property="IsSelected" Value="True">
    <Setter Property="Background" Value="{StaticResource Brush.Accent}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.OnAccent}" />
</Trigger>
<MultiTrigger>
    <MultiTrigger.Conditions>
        <Condition Property="IsSelected" Value="True" />
        <Condition Property="IsMouseOver" Value="True" />
    </MultiTrigger.Conditions>
    <Setter Property="Background" Value="{StaticResource Brush.Accent.Hover}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.OnAccent}" />
</MultiTrigger>
```

**验证**

- 选中、悬停、选中+悬停三种状态截图。
- 检查 `Selection` 背景上的前景必须是 `Text.Primary`。

---

## 5. 选中 + 悬停组合失效

**症状**

单独选中正常，单独悬停正常，但“选中后鼠标移上去”文字突然看不清。

**根因**

两个独立 `Trigger` 的顺序覆盖：

```xml
<Trigger Property="IsMouseOver" Value="True">
    <Setter Property="Background" Value="{StaticResource Brush.Control.Hover}" />
</Trigger>
<Trigger Property="IsSelected" Value="True">
    <Setter Property="Foreground" Value="White" />
</Trigger>
```

后声明的 `IsSelected` 只改前景，不改背景；或者反过来，背景被悬停覆盖，前景仍是选中态的白字。

**修复**

用 `MultiTrigger` 明确组合状态：

```xml
<MultiTrigger>
    <MultiTrigger.Conditions>
        <Condition Property="IsSelected" Value="True" />
        <Condition Property="IsMouseOver" Value="True" />
    </MultiTrigger.Conditions>
    <Setter Property="Background" Value="{StaticResource Brush.Selection.Hover}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Primary}" />
</MultiTrigger>
```

**验证**

- 每个列表/表格/下拉项都测试“选中后悬停”。

---

## 6. ComboBox 弹出层还是系统默认样式 / IsEditable 无法输入

### 6.1 弹出层样式割裂

**症状**

主界面是蓝灰主题，点开下拉框却出现白色/灰色系统弹层，字体和颜色不一致。

**根因**

只给 `ComboBox` 设置了 `Background`/`Foreground`，没有替换模板；WPF 默认模板的 `Popup` 使用系统颜色。

**修复**

使用 `ComboBox.Base` 模板，并显式设置弹层：

```xml
<Border Background="{StaticResource Brush.Surface}"
        BorderBrush="{StaticResource Brush.Border.Strong}"
        BorderThickness="1"
        CornerRadius="{StaticResource Radius.Card}">
    <ScrollViewer>
        <StackPanel IsItemsHost="True" />
    </ScrollViewer>
</Border>
```

同时给 `ComboBoxItem` 定义 Normal/Hover/Selected/Selected+Hover/Disabled。

**验证**

- 点开下拉框，检查弹层背景、边框、项悬停、项选中。
- 在 125%/150% DPI 下检查弹层宽度是否与 ComboBox 对齐。

### 6.2 ComboBox 设置 `IsEditable="True"` 时无法输入与聚焦

**症状**

给 `ComboBox` 设置了 `IsEditable="True"`，但点击控件仍然只能展开下拉列表，鼠标无法激活光标，键盘无法输入文本，`Text` 双向绑定也无法同步用户输入。

**根因**

自定义 `ComboBox` 模板只提供了 `ContentPresenter x:Name="ContentSite"`，缺少 WPF 官方控件契约强制要求的命名部件 `TextBox x:Name="PART_EditableTextBox"`。

WPF 的 `ComboBox` 在内部实现中，仅当找到名为 `PART_EditableTextBox` 的文本框时才会挂载 `TextChanged`、键盘快捷键与焦点传递；若模板缺失此部件，WPF 将直接静默失败，无法支持任何编辑操作。

**修复**

在 `ComboBox.Base` 的控件模板中同时声明展示与编辑两个部件，并使用 `ComboBox.EditableTextBox` 专用去框样式：

```xml
<!-- 静态展示态文本宿主 -->
<ContentPresenter x:Name="ContentSite"
                  Margin="{TemplateBinding Padding}"
                  IsHitTestVisible="False"
                  Content="{TemplateBinding SelectionBoxItem}"
                  ... />

<!-- 可编辑态文本框宿主（WPF 契约要求 PART_EditableTextBox） -->
<TextBox x:Name="PART_EditableTextBox"
         Style="{StaticResource ComboBox.EditableTextBox}"
         Margin="{TemplateBinding Padding}"
         HorizontalAlignment="Stretch"
         VerticalAlignment="Stretch"
         Foreground="{TemplateBinding Foreground}"
         IsReadOnly="{TemplateBinding IsReadOnly}"
         Visibility="Collapsed" />
```

在模板触发器中按 `IsEditable` 切换：

```xml
<Trigger Property="IsEditable" Value="True">
    <Setter Property="IsTabStop" Value="False" />
    <Setter TargetName="PART_EditableTextBox" Property="Visibility" Value="Visible" />
    <Setter TargetName="ContentSite" Property="Visibility" Value="Collapsed" />
</Trigger>
```

**验证**

- 设置 `IsEditable="True"`，点击文本区域时光标正确呈 I 型，可自由键入与选择文本。
- 点击右侧下拉箭头时正常展开/收起下拉项列表。
- 设置 `IsReadOnly="True"` 时背景呈只读底色，允许选中文本但禁止键盘键入。

---

## 7. DataGrid 选中单元格是蓝底深字

**症状**

整行选中是浅蓝底，但某个单元格仍然有默认蓝色背景，文字是深色，看起来像“补丁”。

**根因**

`DataGridCell` 默认模板在 `IsSelected` 时会自己画选中背景，覆盖了行的背景。

**修复**

在 `DataGridCell` 的选中触发器里把背景设为透明，让行背景透出来：

```xml
<Trigger Property="IsSelected" Value="True">
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Primary}" />
</Trigger>
```

**验证**

- 选中整行，检查每个单元格背景一致。
- 检查编辑态单元格的前景/背景。

---

## 8. CheckBox / RadioButton 禁用后文字仍是主色

**症状**

复选框/单选框禁用后，方框变灰，但文字还是深色。

**根因**

模板里的 `ContentPresenter` 没有绑定 `TextElement.Foreground`，隐式 `TextBlock`
样式回退到了正文色；或者控件 `Foreground` 的禁用 setter 没有传递到内容。

**修复**

```xml
<ContentPresenter TextElement.Foreground="{TemplateBinding Foreground}" />
```

并在模板触发器中设置：

```xml
<Trigger Property="IsEnabled" Value="False">
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Disabled}" />
</Trigger>
```

**验证**

- 禁用复选框/单选框，文字应为 `Text.Disabled`。
- 勾选/未勾选、悬停、聚焦都检查。

---

## 9. ToolTip 黑底黑字 / 白底白字

**症状**

ToolTip 弹出后文字与背景同色，完全不可读。

**根因**

只设置了 `Background` 或只设置了 `Foreground`，另一个来自系统主题。

**修复**

同时设置：

```xml
<Style TargetType="ToolTip">
    <Setter Property="Background" Value="{StaticResource Brush.Tooltip.Background}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.Tooltip.Foreground}" />
    <Setter Property="BorderBrush" Value="{StaticResource Brush.Tooltip.Background}" />
</Style>
```

并在模板中传递：

```xml
<ContentPresenter TextElement.Foreground="{TemplateBinding Foreground}" />
```

**验证**

- 悬停任意 ToolTip，检查深底白字。
- 在深色 CAD 主题背景下也检查（ToolTip 独立于宿主主题）。

---

## 10. StaticResource 找不到 / 主题不生效

**症状**

窗口启动时抛 `XamlParseException: Cannot find resource`，或者部分控件没有样式。

**根因**

- 合并顺序错误：`Controls.xaml` 先于 `Tokens.xaml`。
- `pack://` URI 的程序集名写错。
- 只在某个窗口合并了主题，其他窗口没有。
- `Theme.xaml` 被重复合并，且其中一个路径错误。

**修复**

- 在 `App.xaml` 或窗口基类统一合并 `Theme/Theme.xaml`。
- 确认 URI：

```xml
pack://application:,,,/YourAssembly;component/Theme/Theme.xaml
```

- 不要在业务窗口里再合并 `Tokens.xaml`/`Controls.xaml`，只合并 `Theme.xaml`。
- 如果运行时切换主题，使用 `DynamicResource`；单主题使用 `StaticResource` 即可。

**验证**

- 新建一个最小窗口，只放一个 `Button.Primary`，确认样式生效。
- 检查输出目录中是否存在 `Theme/Theme.baml`。

---

## 11. 局部 Foreground 覆盖状态前景

**症状**

按钮/标签页/列表项在悬停或禁用时，文字颜色不变。

**根因**

在控件内容里写了局部前景：

```xml
<Button Content="保存">
    <TextBlock Text="保存" Foreground="White" />
</Button>
```

局部值优先级高于样式和触发器，状态切换无法覆盖。

**修复**

不要在状态控件内部写局部 `Foreground`。使用字符串内容，让 `ContentPresenter`
传递前景；或者绑定到 `TextElement.Foreground`。

**验证**

- 搜索业务 XAML 中 `Foreground="`，确认没有出现在按钮/列表项/TabItem 内部。

---

## 12. 用 `Text.Title` / `Text.Body` 放在按钮里

**症状**

按钮文字颜色不随状态变化，或者主按钮文字不是白色。

**根因**

`Text.Title` / `Text.Body` 等 keyed 样式带显式 `Foreground`，优先级高于控件前景。

**修复**

- 按钮、TabItem、ListBoxItem、ComboBoxItem 内部使用隐式 `TextBlock`。
- 需要特定字号时，只设置 `FontSize`/`FontWeight`，不要设置 `Foreground`。

**验证**

- 搜索 `Style="{StaticResource Text.` 在按钮/列表项内部的使用。

---

## 13. 用 `Opacity` 做悬停

**症状**

悬停时整个控件变淡，文字和背景一起变浅，对比度下降。

**根因**

```xml
<Trigger Property="IsMouseOver" Value="True">
    <Setter Property="Opacity" Value="0.8" />
</Trigger>
```

**修复**

用背景/边框变化表达悬停：

```xml
<Trigger Property="IsMouseOver" Value="True">
    <Setter Property="Background" Value="{StaticResource Brush.Control.Hover}" />
    <Setter Property="BorderBrush" Value="{StaticResource Brush.Border.Strong}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Primary}" />
</Trigger>
```

**验证**

- 搜索 `Opacity`，只允许用于阴影/浮层淡入，不用于控件状态。

---

## 14. 焦点不可见

**症状**

键盘 Tab 到按钮/输入框，看不出焦点在哪里。

**根因**

- 移除了 `FocusVisualStyle` 但没有提供替代边框。
- 焦点边框颜色与背景对比度不足。

**修复**

在控件状态中增加：

```xml
<Trigger Property="IsKeyboardFocused" Value="True">
    <Setter Property="BorderBrush" Value="{StaticResource Brush.Focus}" />
</Trigger>
```

焦点色 `#2F6F9F` 在 `#FAFBFC` 上约 5.2:1，满足非文字 3:1。

**验证**

- 只用键盘 Tab 遍历窗口，焦点必须可见。
- 截图检查焦点边框在浅色/深色背景上都清楚。

---

## 15. 双重嵌套 Border / 框中框反模式

**症状**

文本输入框外面还套了一层浅灰色的卡片 Border 或 GroupBox，内部 TextBox 又没有填满，留出难看的环形空白，造成“大框套小框”、“双重边框”的糟糕视觉。

**根因**

开发者想给区域加卡片包裹，但外层 `Border` 给了大的 `Padding`，而内层 `TextBox` 又写了固定的宽高或没有设置 `VerticalAlignment="Stretch"`。

**修复**

1. 多行输入区域直接使用 `TextBox Style="{StaticResource TextBox.Multiline}"` 并在外层用带有标准 Header 的 `GroupBox`，或者直接将 `TextBox` 作为主要容器，避免不必要的 `Border` 包装。
2. 内部 `TextBox` 设置 `VerticalAlignment="Stretch"` 和 `Margin="0"`，让输入框边框成为唯一的自然边界。

---

## 16. 工具栏按钮权重扁平与危险操作无警示

**症状**

工具栏一排按钮全都是无色次要按钮（`Button.Secondary`），例如“新增词条”、“删除选中”、“导入 CSV”、“导出 CSV”并排且长相完全一样。用户无法一眼找到核心主操作，且极易误触“删除”。

**根因**

缺乏视觉主次分级意识，所有按钮直接套用默认 `Button`。

**修复**

1. 确立唯一核心主动作，使用 `Button.Primary`（例如“+ 新增词条”）。
2. 破坏性操作使用 `Button.Danger`（红边红字，悬停浅红底深红字），且在布局上与其他操作隔开至少 12–16px。
3. 批量次要操作保留 `Button.Secondary`，且统一搭配矢量图标（加号、上传、下载、垃圾桶）。

---

## 17. 控件 Padding 过大导致文字截断与视效局促

**症状**

单行输入框、按钮或表格单元格文字垂直方向被切掉上下部分（如字母 g、j、y 底部被吃掉），或左右稍长一点文字末尾就变成点点点，甚至连中文也显示不完整。

**根因**

- 控件固定了高度（如 30px / 34px），但给内部 `Padding` 设得太大（如上下设了 6px–8px，或左右设了 14px 以上）。
- 内边距将内容呈现区域挤压得过小，导致 `PART_ContentHost` 视口容纳不下正常字号及行高。

**修复**

- 单行 `TextBox` / `PasswordBox` 垂直方向不要设置大 padding，统一使用 `Layout.InputPadding`（设为 `6,0`），垂直对齐使用 `VerticalContentAlignment="Center"`。
- 按钮左右 Padding 设为 `10,0`（紧凑小按钮设为 `8,0`），为文本与图标预留充足横向空间。
- ComboBox 右侧留出 22px 给下拉箭头，左侧留 6px 即可（`Padding="6,0,22,0"`）。

---

## 18. 字体混用导致文字发虚


**症状**

同一窗口里有的文字清晰，有的发糊；弹层和主窗口字体不一致。

**根因**

- 局部设置了 `FontFamily`（Consolas、Arial、宋体）。
- `TextFormattingMode` 在不同层级不一致（Ideal/Display 混用）。
- 使用了非整数 `FontSize`。

**修复**

- 只在 `Window` 根设置 `FontFamily` 和 `TextOptions.TextFormattingMode="Display"`。
- 子元素不要设置 `FontFamily`。
- 字号使用 `FontSize.*` 令牌。
- 代码/日志也用 `Font.Ui`，不要用 Consolas。

**验证**

- 搜索 `FontFamily="`，只允许出现在 `Tokens.xaml` 和 `Window` 样式中。
- 100%/125%/150% DPI 截图。

---

## 19. 主题文件重复合并导致覆盖

**症状**

某个窗口的按钮样式和其他窗口不一样，或者后打开窗口把前一个窗口的样式改掉。

**根因**

- 每个窗口各自合并了一套不同的主题字典。
- 重复合并 `Tokens.xaml`，后合并的旧版本覆盖新版本。
- 在 `Window.Resources` 里合并后，又在 `App.Resources` 合并了另一套。

**修复**

- 全应用只合并一次 `Theme/Theme.xaml`。
- 插件项目在窗口基类/ThemeLoader 中统一合并。
- 不要在业务窗口里复制主题字典。

**验证**

- 搜索 `MergedDictionaries`，确认只有统一入口。
- 打开多个窗口，检查样式一致。

---

## 20. 长文本把布局撑破


**症状**

英文/俄文译文变长后，按钮文字被裁切、表格列被撑开、对话框出现横向滚动。

**根因**

- 固定宽度 + 不换行。
- 按钮 `MinWidth` 太小。
- 表格列使用 `Auto` 且没有最大宽度。

**修复**

- 按钮使用 `MinWidth` + `Padding`，允许内容撑宽；关键按钮设置 `MinWidth`。
- 表单输入列使用 `*`。
- 表格长文本列使用 `*` + `TextTrimming="CharacterEllipsis"` + ToolTip。
- 对话框设置 `MinWidth`，不要写死 `Width`。
- 必要时使用 `ScrollViewer.HorizontalScrollBarVisibility="Disabled"` + 换行。

**验证**

- 用最长目标语言（德语/俄语）测试。
- 150% DPI 下检查按钮和表格。

---

## 18. 多行文本框撑满布局

**症状**

多行 TextBox/日志框把整个窗口撑满，按钮被挤到很下面，或者表单里的多行输入框高度异常大；
有时 `StackPanel` 里的 `VerticalAlignment="Stretch"` 完全不起作用，但 `MinHeight` 又把布局撑坏。

**根因**

- `TextBox` 默认 `VerticalAlignment="Stretch"`。放在 `Grid` 的 `Height="*"` 行里会填满整行。
- `StackPanel` 给子元素无限高度，`Stretch` 没有意义；此时 `MinHeight` 会直接决定高度。
- 把表单输入框和“需要填充”的日志框用了同一种布局策略。
- 误写 `Height="*"`（`Height` 是 double，不支持星号）。

**修复**

表单多行输入：

```xml
<TextBox Style="{StaticResource TextBox.Multiline}"
         Height="Auto"
         VerticalAlignment="Top" />
```

日志框（确实需要填充）：

```xml
<Grid.RowDefinitions>
    <RowDefinition Height="Auto" />
    <RowDefinition Height="*" />
    <RowDefinition Height="Auto" />
</Grid.RowDefinitions>

<TextBox Grid.Row="1"
         Style="{StaticResource TextBox.Code}"
         MaxHeight="240"
         VerticalAlignment="Stretch" />
```

规则：

- 表单行 `Auto`，日志/列表行 `*`。
- 只有列表、表格、日志、主内容 ScrollViewer 适合放在 `*` 行。
- 多行输入框默认 `Top`，不要 `Stretch`。
- 不要写 `Height="*"`。

**验证**

- 运行 `pwsh ./tools/LayoutCheck.ps1 -Root <项目目录>`。
- 缩放窗口，确认表单高度不随窗口无限增长。
- 150% DPI 下检查日志区没有把按钮挤出窗口。

---

## 18.5 给 DataGrid、Grid 或弹性 RowDefinition 设置过大 MinHeight 导致布局截断

**症状**

在普通 1080p（125%/150% DPI 缩放）或 1366x768 屏幕上，表格页面底部的分页栏、统计汇总栏或保存/取消操作按钮被截断、挤出屏幕看不见；缩放窗口时，表格不触发内部滚动条，而是整体溢出。

**根因**

1. 在 `DataGrid`、`Grid` 或 `RowDefinition Height="*"` 上硬编码了过大的 `MinHeight`（例如 `MinHeight="280"` 或 `MinHeight="240"`）。
2. 在 WPF 网格布局中，`MinHeight` 具有强制最高优先级。当可用视口高度不足时，`*` 行被 `MinHeight` 锁死无法收缩，强制将位于下方的 `Auto` 行（分页条、操作栏）挤出窗口可视边界。
3. 试图通过给数据表格窗口加 `SizeToContent="Height"` 来解决，反而导致 DataGrid 失去虚拟化、以无限高度测量所有行，窗口瞬间被撑爆并延伸到屏幕下方。

**修复**

1. **去除 `*` 行内控件的硬编码 MinHeight**：
   让 `DataGrid`、`Grid` 占 `Height="*"` 行，设置 `MinHeight="0"` 或极小保底值（如 <=100）。当窗口变小时，`DataGrid` 自然缩小并唤起自带的垂直滚动条，绝不挤压底部操作栏。
2. **区分窗口类型**：
   - 视图型表格窗口使用确定初始宽高 `Width="1180" Height="680"` 与低保底 `MinHeight="360"`，禁止使用 `SizeToContent="Height"`。
   - 底部的操作栏/分页器单独占 `RowDefinition Height="Auto"` 行。

```xml
<Grid.RowDefinitions>
    <RowDefinition Height="Auto" />  <!-- 顶部筛选 -->
    <RowDefinition Height="*" />     <!-- 表格：弹性自适应，内部自滚动，不锁死 MinHeight -->
    <RowDefinition Height="Auto" />  <!-- 底部分页：永远可见 -->
</Grid.RowDefinitions>

<DataGrid Grid.Row="1" ... />
<Border Grid.Row="2" ... />
```

**验证**

- 运行 `pwsh ./tools/LayoutCheck.ps1 -Root <项目目录>`，确认无 `rigid MinHeight` 警告。
- 将窗口垂直高度缩小到 480px，检查底部的分页器和操作按钮是否仍然清晰可见、完整不被裁切。

---

## 19. 颜色作为唯一状态信号

**症状**

色盲用户或灰度打印时分不清成功/失败/警告。

**根因**

只用红/绿/黄背景，没有图标或文字。

**修复**

- 状态标签同时使用文字（成功/失败/警告）和颜色。
- 图标 + 颜色 + 文本三重编码。
- 错误提示写在输入框下方，不只改边框颜色。

**验证**

- 灰度截图检查是否仍能区分状态。
- 使用颜色盲模拟工具检查。

---

## 20. AI 典型 UI 坏味道：入口混乱到处是、文案冗余复读、功能交叉重叠与过度密集

**症状**

1. **满屏都是“导出”与“保存”**：页头有“导出”，表格上方工具栏有“导出 (CSV)”，底栏有“导出日志”，甚至单行右键还有“导出”。用户完全不知道各个导出有何区别、改动后到底点哪个保存才入库。
2. **文字车轱辘话反复复读**：标题叫“物料管理”，副标题叫“物料管理与工艺”，下面还配两行“演示复选框联动、您可以管理物料...”。标签叫“材质种类”，下拉框默认选“选择材质种类”。
3. **功能交叉重叠**：筛选栏里塞了“添加行”按钮，工具栏里既有针对选中行的“批量删除”，又混着针对全局的“保存到数据库”和“折叠/展开”视图控制。
4. **过度密集缺乏呼吸感**：一行横向塞进 8 个输入控件，没有分组，没有间距，视觉疲劳。

**根因**

AI 模型的通病是“增量贪心堆叠”：收到“增加导出功能”时就顺手放一个导出按钮，收到“支持编辑”就塞一个保存按钮，收到“增加说明”就生成一段客套废话。缺乏对界面整体权责边界和信息架构的审视。

**修复**

严格遵循四大功能分区（Four-Zone）与单一动作入口（Single Action Entry）：

1. **入口收敛到单一真理位置**：
   - 全局动作（保存全单、导出整表）：**只在页头右侧出现一次**。
   - 局部批量动作（批量审核、批量删除）：**放在表格工具栏左侧**，无选中项时灰显，配 `已选中 N 项` 状态。
   - 单行操作（编辑、单行删除）：放在表格最后一列。
2. **剔除所有 AI 教学废话**：
   - 彻底删除标题下方的“演示...”、“您可以在这里...”等客套文字，只保留业务标题和上下文对象标签（如 `装配体: DOC-0482`）。
3. **严格分离筛选与操作**：
   - 筛选栏只放条件输入和“重置筛选”，绝不放业务修改动作。
   - 工具栏左侧放数据操作，右侧放视图切换。

```xml
<!-- Correct: 结构清爽、职责分明的表格头部与工具栏 -->
<!-- Zone 1: 页头主标题与唯一全局操作 -->
<Grid Grid.Row="0">
    <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
        <TextBlock Text="BOM 物料与制造工艺清单" Style="{StaticResource Text.Title}" FontWeight="SemiBold" />
        <Border Margin="12,0,0,0" Background="{StaticResource Brush.SurfaceAlt}" Padding="8,2" CornerRadius="{StaticResource Radius.Small}">
            <TextBlock Text="装配体: DOC-0001_示例装配体" Style="{StaticResource Text.Caption}" />
        </Border>
    </StackPanel>
    <StackPanel Grid.Column="1" Orientation="Horizontal">
        <Button Content="导出清单 (CSV)" Style="{StaticResource Button.Secondary}" Margin="0,0,8,0" />
        <Button Content="保存变更" Style="{StaticResource Button.Primary}" />
    </StackPanel>
</Grid>

<!-- Zone 2: 纯净筛选栏（至多 3 组常用筛选） -->
<Grid Grid.Row="1">
    <TextBlock Text="编码/品名" Style="{StaticResource Text.Label}" />
    <TextBox x:Name="TxtSearch" ... />
    <TextBlock Text="材质" Style="{StaticResource Text.Label}" />
    <ComboBox x:Name="CmbMaterial" ... />
    <Button Content="重置筛选" Style="{StaticResource Button.Secondary}" />
</Grid>

<!-- Zone 3: 表格工具栏（左侧批量，右侧视图） -->
<Grid Grid.Row="2">
    <StackPanel Orientation="Horizontal">
        <Button Content="添加物料" Style="{StaticResource Button.Secondary}" Margin="0,0,8,0" />
        <Button Content="批量审核" IsEnabled="False" Style="{StaticResource Button.Secondary}" Margin="0,0,8,0" />
        <Button Content="批量删除" IsEnabled="False" Style="{StaticResource Button.Danger}" Margin="0,0,16,0" />
        <TextBlock Text="未选择项" Style="{StaticResource Text.Caption}" />
    </StackPanel>
    <StackPanel Grid.Column="1" Orientation="Horizontal">
        <Button Content="展开全部明细" Style="{StaticResource Button.Secondary}" />
    </StackPanel>
</Grid>
```

**验证**

- 运行 `python tools/LayoutCheck.py --root .`，确保无 `Duplicate action entry points` 或 `Redundant AI demo chatter` 警告。
- 检查界面上每个按钮的职责范围（全局 vs 批量 vs 单行），确认不存在同名动作互相掐架的情况。

---

## 21. 标签被固定列宽裁掉一半

**症状**

"起始编号（自动）" 只显示 "起始…"，或者标签右边被输入框压住。

**根因**

- 标签列写死像素宽度（`ColumnDefinition Width="104"` / `"96"`）。
- 标签使用 `TextTrimming` 但没有换行或 ToolTip。

**修复**

```xml
<!-- Wrong -->
<ColumnDefinition Width="104" />

<!-- Correct -->
<ColumnDefinition Width="Auto" MinWidth="{StaticResource Layout.FormLabelMinWidth}" />
```

- 标签列改 `Auto` + `MinWidth`（token `Layout.FormLabelMinWidth` = 88）。
- 标签用 `Text.Label`（默认 `TextTrimming="None"`）；太长就换行或缩短文案。
- `LayoutCheck` 会把固定像素的标签列报为 WARN。

**验证**

- 用最长的标签名检查；150% DPI 下再检查一次。

---

## 22. 输入框里的内容看不到

**症状**

输入框有值，但显示不全；用户以为是空的。

**根因**

- 输入框写死 `Width`，或所在列是固定宽度。
- 没有 ToolTip，也没有横向滚动。

**修复**

```xml
<ColumnDefinition Width="*" MinWidth="{StaticResource Layout.InputMinWidth}" />
<TextBox Style="{StaticResource TextBox.Form}" ToolTip="{Binding Path}" />
```

- 输入列 `*` + `MinWidth`；输入框本身不写死 `Width`。
- 长值用 `ToolTip` 显示全文；多行输入加 `HorizontalScrollBarVisibility="Auto"`。

**验证**

- 输入超长字符串，确认可见或有 ToolTip。

---

## 23. 底部按钮被内容遮挡 / 挤出窗口

**症状**

窗口底部看不到"保存"按钮，或者按钮被上面的输入区盖住。

**根因**

- 窗口写死 `Height`，内容高度超过窗口。
- 内容区没有 `ScrollViewer`。
- 按钮栏和长内容挤在同一行 / 同一 `*` 行。

**修复**

- 窗口 `SizeToContent="Height"` + `MinHeight`，不要写死 `Height`。
- 内容区包一层 `ScrollViewer`（`VerticalScrollBarVisibility="Auto"`）。
- 按钮栏独占最后一个 `Height="Auto"` 行。

**验证**

- 150% DPI + 内容最多的场景下，确认按钮可见可点。

---

## 24. 状态文案把按钮挤变形

**症状**

"已载入: C:/…/DocumentConfig.xml" 换行后，按钮被压成竖排或换行成两行。

**根因**

状态文案和按钮放在同一个 `Grid` 行 / 同一个 `StackPanel`。

**修复**

- 状态文案单独一行（`Grid.Row="0"`），按钮栏单独一行（`Grid.Row="1"`）。
- 状态文案用 `Text.Caption` + `TextWrapping="Wrap"`。

**验证**

- 用很长的路径填入状态栏，确认按钮不变形。

---

## 25. 高 DPI（125% / 150%）下内容溢出

**症状**

换到 150% 缩放后，底部内容被裁、控件之间互相重叠。

**根因**

- 固定高度 / 固定列宽。
- 整窗没有滚动容器。

**修复**

- 行高用 `Auto`，高度交给内容；需要滚动的地方只留一层 `ScrollViewer`。
- 标签列 `Auto` + `MinWidth`，输入列 `*` + `MinWidth`。
- 逐条走 `docs/03-layout.md` §14.7 自查清单。

**验证**

- 100% / 125% / 150% 三档各截一次图对比。

---

## 26. 负 Margin / Canvas 绝对定位造成重叠

**症状**

两个控件叠在一起，或控件边框被吃掉一块。

**根因**

- 用负 `Margin` 做"贴边微调"。
- 用 `Canvas` + 绝对坐标排表单。

**修复**

- 去掉负 `Margin`，改用 `Padding` 或对齐属性。
- 表单一律用 `Grid` 的 `Auto`/`*` 列行。

**验证**

- `LayoutCheck` 会把负 `Margin` 与 `Canvas` 布局报为 ERROR。

---

## 27. `ColumnDefinition.Width` 绑定到 Double 令牌直接崩

**症状**

窗口一打开就抛 `XamlParseException`："设置属性 ColumnDefinition.Width 时引发了异常"，
内层是 `System.ArgumentException: "12" 不是属性 "Width" 的有效值`。

**根因**

`ColumnDefinition.Width` 的类型是 `GridLength`，而 `StaticResource` 解析出的是
`sys:Double`，无法隐式转换。

**修复**

- 列间距令牌用 `GridLength` 类型定义：

```xml
<GridLength x:Key="Layout.ColumnGap">12</GridLength>
```

- 宽度类令牌（`MinWidth`）用 `sys:Double` 即可，因为 `MinWidth` 是 `Double` 属性。

**验证**

- 打开窗口，确认没有 `XamlParseException`；预览工程 `preview/` 可直接跑。

---

## 28. DataGrid 单元格内容与表头错位 / 压住网格线

**症状**

- 单元格文字紧贴竖向网格线，甚至压在线上；
- 单元格文字与表头文字左右差一截（通常差 10px）；
- 窄列的内容会盖到相邻列的文字上（"总装图" 和 "42" 叠在一起）。

**根因**

WPF 默认的 `DataGridCell` 模板里 `ContentPresenter` **不绑定** `Padding`，
所以给 `DataGridCell` 设 `Padding` 完全没有效果：内容从列边界 x=0 开始画。
而 `DataGridColumnHeader` 的模板会消费 `Padding`（10px），
于是表头缩进 10px、单元格不缩进，两边错位；列宽不够时文字还会溢出到相邻列。

**修复**

主题里显式给模板（见 `Theme/DataControls.xaml`）：

```xml
<ControlTemplate x:Key="DataGridCell.Template" TargetType="{x:Type DataGridCell}">
    <Border Background="{TemplateBinding Background}"
            BorderBrush="{TemplateBinding BorderBrush}"
            BorderThickness="{TemplateBinding BorderThickness}">
        <ContentPresenter Margin="{TemplateBinding Padding}"
                          HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                          VerticalAlignment="{TemplateBinding VerticalContentAlignment}" />
    </Border>
</ControlTemplate>
```

单元格样式里 `Padding="10,0"`（与表头 `Padding="10,6"` 的左值一致）并挂上该模板。

**验证**

- 主题契约测试 `DataGridCell_CellAndHeaderShareTheSameLeftInset` 会守住这条。
- 目视：单元格文字与表头文字左边缘对齐，且不压竖线。

---

## 29. 折叠面板展开后把底部按钮栏顶出窗口

**症状**

展开"高级参数"后，窗口底部的"保存/关闭"按钮看不见了，或者按钮被内容压住。

**根因**

Expander 的内容直接放在 `ContentPresenter` 里，没有滚动容器：
内容有多长，Expander 就多高，整窗被撑高，按钮栏被顶到可视区外。

**修复**

主题的 Expander 模板已把内容包进 `ScrollViewer`：

```xml
<ScrollViewer VerticalScrollBarVisibility="Auto"
              HorizontalScrollBarVisibility="Disabled"
              MaxHeight="{StaticResource Layout.ExpanderMaxHeight}">
    <ContentPresenter />
</ScrollViewer>
```

业务侧不要再自己套 `ScrollViewer`（会变成双滚动条）。

**验证**

- 展开长内容，确认内容区出现滚动条、窗口高度不跳变。

---

## 30. 表格进入编辑态后文字上下不居中 / 底边框被裁

**症状**

双击单元格进入编辑后，输入框比行高、底部被切掉一行，文字偏上。

**根因**

单元格编辑器是 TextBox（高度 = `Layout.ControlHeight`，中等 30px），而行高小于它时：
行高小于编辑器高度时，编辑器被行裁掉。

**修复**

- `DataGrid.RowHeight` 必须 ≥ 编辑器实际高度，主题已设为 `Layout.RowHeight`（中等 28，紧凑 24，宽松 32）。
- `DataGridRow.MinHeight` 同步。

**验证**

- 主题契约测试 `DataGrid_RowHeightFitsTheCellEditor` 守住这条。

---

## 31. 日期控件的日历按钮压住日期文字

**症状**

`2026/11/5` 显示成 `2026/11:`，或者日期文字被蓝色日历图标盖住。

**根因**

- WPF 系统 DatePicker 模板把日历按钮浮在文本框上；
- 主题的隐式 `ToggleButton` 带 `MinWidth=72`，把输入区挤窄；
- `DatePickerTextBox` 默认按内容宽度收缩，文字视口窄到裁掉最后一个字。

**修复**

- 用主题的 `DatePicker.Template`：文本列 + 按钮列两列布局，按钮 `MinWidth="0"`；
- 日期文本框 `HorizontalAlignment="Stretch"`；
- 弹层里放 `Calendar`（否则点开空白）。

**验证**

- 主题契约测试 `DatePicker_UsesOwnTemplateWithCalendarInPopup`。

---

## 32. 竖向滚动条几乎看不见

**症状**

内容溢出窗口，但右侧找不到滚动条（或滑块是横着的）。

**根因**

滚动条模板里的 `Track` 没有绑定 `ScrollBar.Orientation`，
`Track` 默认是横向，竖向滚动条的滑块被横着排，几乎看不见。

**修复**

```xml
<Track x:Name="PART_Track"
       Orientation="{TemplateBinding Orientation}"
       IsDirectionReversed="True">
```

**验证**

- 主题契约测试 `ScrollBar_TrackFollowsBarOrientation`。

---

## 33. 日期弹层塌成一条竖线

**症状**

点开日期控件，弹出的日历面板极窄，星期标题（"一二三四五六日"）被挤成一排细竖线。

**根因**

DatePicker 的弹层里放的是 **DatePicker 自己创建的 Calendar 实例**，
模板里给 `<Calendar>` 写的属性不会作用到它；而且 DatePicker 会按文本框宽度去推算日历宽度，
布局早期文本框还没量出宽度时，日历宽度会算成 0 —— 面板就塌成一条线。

**修复**

- 用**隐式样式**给 Calendar 设属性（隐式样式才会作用到 DatePicker 内部的实例）；
- 必须给 `MinWidth`（MinWidth 能盖住被算成 0 的 Width）：

```xml
<Style TargetType="{x:Type Calendar}">
    <Setter Property="Background" Value="{StaticResource Brush.Surface}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Primary}" />
    <Setter Property="MinWidth" Value="240" />
</Style>
```

**验证**

- 主题契约测试 `Calendar_HasAMinWidthSoThePopupCannotCollapse`。
- 实机点开日期控件，确认面板宽度正常、星期标题是一排正常文字。

---

## 34. 一排图标高低错开（看起来"没对齐"）

**症状**

工具栏里一排图标按钮，图标一个高一个低，视觉上很乱。

**根因**

图标按钮有三档尺寸（24 / 28 / 32 px）。它们默认 `VerticalAlignment=Stretch`
（带固定 `Height` 时等同顶对齐），而 **`WrapPanel` 会忽略子元素的 `VerticalAlignment`**，
每个按钮按自己的高度顶对齐排布 → 图标中心线错开。

**修复**

- 主题已给按钮与图标按钮加 `VerticalAlignment="Center"`；
- 混排尺寸的行改用**水平 `StackPanel`**（会尊重对齐），不要用 `WrapPanel`；
  要自动换行就换成 `Grid` + `UniformGrid`，或统一用同一档尺寸。

**验证**

- 离屏渲染控件全览窗口，检查一排图标的垂直中心是否一致。

---

## 35. 切换 tab 时窗口高度跳变（窗口忽高忽矮）

**症状**

窗口里放了一个 `TabControl`。点到内容少的那个 tab，窗口明显缩矮，
底部按钮栏跟着往上跳；点回内容多的 tab，窗口又拉高。用户体验很糟。

**根因**

窗口按规范使用 `SizeToContent="Height"`，高度由内容自然高度决定。
`TabControl` 没有固定高度时，内容区高度等于当前选中 tab 的内容高度——
切到内容少的 tab，内容自然高度变小，整个窗口就被压矮。

**修复**

- 不要用多 tab 在一个窗口里承载并排任务；拆成多个窗口，或用
  `GroupBox` / `Expander` 分组、主从布局。
- 确需 `TabControl` 时给它**固定高度**，内容多了在 tab 内部滚动：

```xml
<TabControl Height="140">
    <TabItem Header="处理日志">
        <ScrollViewer VerticalScrollBarVisibility="Auto">…</ScrollViewer>
    </TabItem>
    <TabItem Header="转换参数">…</TabItem>
</TabControl>
```

**验证**

- `LayoutCheck` 会报 ERROR（`SizeToContent` 窗口内 `TabControl` 无固定高度）。
- 手工点一遍每个 tab，确认窗口高度不变、底部按钮栏不移动。

---

## 18. 输入框只显示左半边，右侧点不进去

**症状**

`TextBox` 看起来占满整行，但文字只靠左，点击右侧空白没有反应，
光标不会跳到行尾；长值看起来像被截断。

**根因**

模板里的 `PART_ContentHost` 跟随了 `HorizontalContentAlignment="Left"`，
`ScrollViewer` 按内容宽度收缩，右侧空白不属于文本宿主。

**修复**

- 主题已把 `TextBox.Base` 的 `HorizontalContentAlignment` 设为 `Stretch`，
  模板里的 `ScrollViewer` 固定 `HorizontalAlignment="Stretch"`。业务 XAML 不要再改回 `Left`。
- 输入列用 `Width="*"`，输入框不写 `Width`；需要上限时用 `MaxWidth`。

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" MinWidth="{StaticResource Layout.FormLabelMinWidth}" />
        <ColumnDefinition Width="*" MinWidth="{StaticResource Layout.InputMinWidth}" />
    </Grid.ColumnDefinitions>
    <TextBlock Text="工程名称" Style="{StaticResource Text.Label}" />
    <TextBox Grid.Column="1" Text="{Binding Name}" />
</Grid>
```

---

## 19. 标签页选中后只有一条细横线

**症状**

点 tab 后只有一条 1px 蓝色横线，分不清当前页；多个 tab 时尤其明显。

**根因**

模板只画了一条下划线指示器，标签页本身没有可见的边框/底色，看起来不像可点击的页签。

**修复**

- 使用主题的默认 `TabItem`：原生方角页签，每个页签都有 1px `Brush.Border.Tab` 边框、
  未选中 `SurfaceAlt` 底；选中页签铺 `Brush.Surface` 并去掉下边框，与内容区连成一体。
- 不要重新引入圆角或底部强调条；不要只保留一条下划线。
- 仍然遵守 R12：窗口内不要用 tab 承载并排任务。

---

## 20. 白底按钮的边框太浅，看不见边界

**症状**

白色按钮放在浅灰窗口上，按钮形状几乎消失，只有文字浮着。

**根因**

按钮边框使用了 `Brush.Border`（`#D0D5DB`，对白底约 1.4:1），
或直接用了 `BorderThickness="0"`。

**修复**

- 默认按钮使用 `Brush.Border.Control`（对 Surface 3.52:1，对窗口底 3.10:1）。
- 悬停时描边切到 `Brush.Accent`，按下切到 `Brush.Accent.Pressed`。
- 不要用 `Opacity` 弱化按钮；禁用态用 `Control.Disabled + Text.Disabled`。

---

## 21. 同一窗口混用 26 / 30 / 34 三档尺寸

**症状**

同一行里有的按钮 34px、有的输入框 26px，基线不齐，看起来像贴错。

**根因**

从不同来源复制 XAML，局部写死了 `Height="32"`、`Padding="10,6"`，
或部分控件用了 `.Small` 变体、部分用默认样式。

**修复**

- 选定一档密度后，所有控件引用同一组 `Layout.*`，不要在业务 XAML 写死高度。
- 工具条/表格行内统一用 `Layout.ControlHeight.Small`，主表单统一用 `Layout.ControlHeight`。
- `LayoutCheck` 会检查 `Padding`/`Height` 硬编码；`dotnet test` 会检查三档键集与递增关系。

---

## 22. 标签与输入框/下拉框不对齐

**症状**

表单里标签靠左，输入框和下拉框却离得很远；窄停靠窗里甚至只看到标签，输入控件被挤到右侧或裁掉。

**根因**

1. 每一行用独立的 `Grid` / `StackPanel`，标签列宽各行不同；
2. 标签列和输入列没有共享一个 Grid，或者标签上加了局部 `Margin` / `HorizontalAlignment`；
3. 宿主宽度小于两列的最小宽度（标签 `MinWidth` + 输入 `MinWidth`），输入列被挤出可视区；
4. 长标签只给 `ColumnDefinition` 设了 `MaxWidth`，文本仍按单行测量，直接溢出到输入框上；
5. 控件被局部设置了 `HorizontalContentAlignment="Center"` 或 `TextAlignment="Center"`。

**修复**

宽面板（≥ 520 DIP）：

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" MinWidth="{StaticResource Layout.FormLabelMinWidth}" />
        <ColumnDefinition Width="*" MinWidth="{StaticResource Layout.InputMinWidth}" />
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="Auto" />
    </Grid.RowDefinitions>
    <TextBlock Text="服务地址" Style="{StaticResource Text.Label}" />
    <TextBox Grid.Column="1" />
    <TextBlock Grid.Row="1" Text="模型名称" Style="{StaticResource Text.Label}" />
    <TextBox Grid.Row="1" Grid.Column="1" />
</Grid>
```

窄面板（< 520 DIP）改为单列：

```xml
<StackPanel Style="{StaticResource FormStack}">
    <TextBlock Text="服务地址" Style="{StaticResource Text.FieldLabel}" />
    <TextBox />
</StackPanel>
```

长标签要这样写：

```xml
<TextBlock Text="文档标题（用于换行演示）"
           Style="{StaticResource Text.Label}"
           TextWrapping="Wrap" TextTrimming="None"
           MaxWidth="{StaticResource Layout.FormLabelMaxWidth}" />
```

> `MaxWidth` 放在 `TextBlock` 上，Auto 列才会按换行后的宽度测量；放在 `ColumnDefinition` 上无效。

**验证**

- `LayoutCheck --strict`：0 错误；居中输入内容会报 WARN；
- 在 150% DPI 与宿主最小宽度下，输入框仍完整可见；
- 目视确认标签与控件共享同一条左基线。

---

## 36. 树/列表选中项变成系统蓝底白字

**症状**

`TreeView` 里点中一个节点，背景是 Windows 主题蓝（`#0078D7`）配白字，和旁边的浅蓝 `Brush.Selection` 完全不是一套；鼠标移开也不消失，某些主题下白字还会和浅色背景撞在一起看不清。

**根因**

样式里给 `TreeViewItem` 写了 `IsSelected` 触发器，但没有替换 `Template`。WPF 默认的 `TreeViewItem` 模板在选中时直接用 `SystemColors.HighlightBrushKey` / `HighlightTextBrushKey` 画，**模板内部硬编码，样式触发器根本够不到**。`ListBoxItem`、`ListViewItem`、`DataGridRow` 的默认模板同理。

**修复**

这几类 item 容器必须自带模板（或 `BasedOn` 到带模板的样式）：

```xml
<Style TargetType="{x:Type TreeViewItem}">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type TreeViewItem}">
                <StackPanel>
                    <Border x:Name="Bd" Background="{TemplateBinding Background}"
                            Padding="{TemplateBinding Padding}">
                        <Grid> ... 展开箭头 + ContentPresenter ... </Grid>
                    </Border>
                    <Border x:Name="ItemsHost" Margin="18,0,0,0" Visibility="Collapsed">
                        <ItemsPresenter />
                    </Border>
                </StackPanel>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsSelected" Value="True">
                        <Setter TargetName="Bd" Property="Background"
                                Value="{StaticResource Brush.Selection}" />
                        <Setter Property="Foreground"
                                Value="{StaticResource Brush.Text.Primary}" />
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

三个容易漏的点：

1. **自定义模板不会自动处理折叠**：默认模板里有隐藏 `ItemsPresenter` 的逻辑，自己写就得绑 `IsExpanded`，否则折叠的节点仍然把子项全部画出来。
2. **箭头用 `HasItems` 控制可见性**，不要用 `IsExpanded`——折叠的父节点也需要箭头。
3. **`ListViewItem` 直接 `BasedOn` 到 `ListBoxItem`**，不要复制一遍触发器；否则默认模板残留的焦点虚线框会留在选中项外面。

**验证**

- 选中节点后取背景色，应是 `Brush.Selection`（`#E4EEF5`），不是 `#0078D7`；
- 折叠一个有子项的节点，子项必须消失；
- 选中项外围不应有虚线焦点框。

---

## 37. 输入框文字比下拉框靠右一大截

**症状**

同一个表单里，`TextBox` 的文字比 `ComboBox` 缩进多出 6~8px；换成 `DatePicker` 又是另一个位置。左边缘对不齐一整列。

**根因**

`TextBox` 会把 `Padding` 交给 `PART_ContentHost`（滚动宿主）。模板里再写一层
`Margin="{TemplateBinding Padding}"` 就等于把内边距算了两遍。另外 `DatePicker` 的
`PART_TextBox` 模板上多写了一个 `Margin="6,0,0,0"`，同样叠在 `Padding` 之上。

**修复**

模板里不要给 `PART_ContentHost` / `PART_TextBox` 再加 Margin，只保留 `Padding`：

```xml
<ScrollViewer x:Name="PART_ContentHost" Focusable="False" Background="Transparent"
              HorizontalAlignment="Stretch" VerticalAlignment="Stretch" ... />
```

`ComboBox` 的显示内容不是 TextBox 渲染的（没有 `TextBoxView` 自带的约 2px 内缩），
所以内容区要补 2px 才能和输入框落在同一条竖线上。

**验证**

- 并排渲染 `TextBox` / `ComboBox` / 可编辑 `ComboBox` / `DatePicker`，量首字墨迹的 x：
  四者应当一致（当前 Medium 档都是 30px 处起笔）；
- 契约测试 `TextBox_ContentHostDoesNotDoubleApplyPadding` 守住模板不再叠 Margin。

---

## 38. 表头全选框比下面的行复选框偏左

**症状**

`DataGrid` 表头列里的全选复选框，和下面每一行的行复选框不在一条竖线上，通常偏左 3px 左右。

**根因**

隐式 `CheckBox` / `RadioButton` 样式带 `Margin="0,2,12,2"`，右边那 12px 是给
“复选框 + 文字”留的间距。表头全选框、单元格里的独立复选框没有文字，这 12px
就变成纯偏移，把控件整体推出中心。

**修复**

无文字的复选框不要继承文字间距——隐式样式里按 `HasContent` 收掉：

```xml
<Style.Triggers>
    <Trigger Property="HasContent" Value="False">
        <Setter Property="Margin" Value="0" />
    </Trigger>
</Style.Triggers>
```

**验证**

- 渲染表格，量表头框与行框的墨迹中心 x 应为同一条线；
- 契约测试 `CheckBox_WithoutContentHasNoLabelMargin` 守住这条触发器存在。
