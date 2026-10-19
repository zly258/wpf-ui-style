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

## 6. ComboBox 弹出层还是系统默认样式

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
         MinHeight="90"
         MaxHeight="160"
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
         MinHeight="120"
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
