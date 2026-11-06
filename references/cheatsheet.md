# WPF UI 速查表

## 五条前景/背景规则

1. 背景变了，前景必须一起变。
2. 禁用不用 `Opacity`；用 `Control.Disabled` + `Text.Disabled`。
3. 隐式 `TextBlock`/`AccessText` 前景绑定 `(TextElement.Foreground)`，不要硬编码。
4. 白字只压在 `Accent` / `Accent.Hover` / `Accent.Pressed` / `Danger*` / `Tooltip.Background`。
5. 选中+悬停用 `MultiTrigger`，不要依赖触发器顺序。

## 字体与字号（全局统一，不写本地字体）

| 令牌 | 值 |
|---|---|
| `Font.Ui` | `Microsoft YaHei`（唯一字体栈） |
| `FontSize.Title` / `Section` | 13 |
| `FontSize.Body` | 12 |
| `FontSize.Caption` | 11.5 |
| `FontSize.Code` | 11.5 |
| `FontSize.Display` | 16 |
| `FontSize.Micro` | 10 |
| `LineHeight.Body` / `Caption` | 18 / 17 |

- 业务 XAML 禁止出现 `FontFamily`，禁止写 `FontSize="13"` 字面量。
- 复选框 / 单选框不得小于 **16×16**（`Layout.CheckBox.Size`）。
- 字号变化后必须同步检查：控件高度（32/28）、行高（26）、表头（28）、Tab/Menu/ToolTip/DataGrid 内边距。

## 状态速查

| 状态 | 背景 | 前景 |
|---|---|---|
| 主按钮 Normal | `Brush.Accent` | `Brush.OnAccent` |
| 主按钮 Hover | `Brush.Accent.Hover` | `Brush.OnAccent` |
| 主按钮 Disabled | `Brush.Control.Disabled` | `Brush.Text.Disabled` |
| 次按钮 Hover | `Brush.Control.Hover` | `Brush.Text.Primary` |
| 危险按钮 Hover | `Brush.Danger.Subtle` | `Brush.Danger.Hover` |
| 输入框 Focus | `Brush.Surface` | `Brush.Text.Primary` |
| 列表 Hover | `Brush.Control.Hover` | `Brush.Text.Primary` |
| 列表 Selected | `Brush.Selection` | `Brush.Text.Primary` |
| 列表 Selected+Hover | `Brush.Selection.Hover` | `Brush.Text.Primary` |
| ComboBoxItem Selected | `Brush.Accent.Subtle` | `Brush.Text.Primary` |
| ToolTip | `Brush.Tooltip.Background` | `Brush.Tooltip.Foreground` |

## 多行文本高度与自适应

```xml
<!-- 方案 A：分栏/比对/日志（拉伸充满 * 行，无裁切） -->
<TextBox Style="{StaticResource TextBox.Multiline}"
         VerticalAlignment="Stretch" />

<!-- 方案 B：普通表单多行输入（自适应自然高度，无死锁） -->
<TextBox Style="{StaticResource TextBox.Multiline}"
         Height="Auto"
         VerticalAlignment="Top" />
```

规则：
- `TextBox.Multiline` 默认不设全局 `MinHeight`（避免撑爆小窗体和内部滚动打架）。
- 对照视图/日志放在 `*` 行并设置 `VerticalAlignment="Stretch"`。
- 表单多行放在 `Auto` 行并设置 `VerticalAlignment="Top"`。
- 不要写 `Height="*"`。


## 防截断 / 防遮挡（显示不全与重叠）

1. 标签列 `Width="Auto"` + `MinWidth="{StaticResource Layout.FormLabelMinWidth}"`（80）——
   **禁止**固定像素列宽，长标签会被裁掉。
2. 输入列 `Width="*"` + `MinWidth="{StaticResource Layout.InputMinWidth}"`（140）——
   输入框本身不写死 `Width`。
3. 省略号（`TextTrimming="CharacterEllipsis"`）必须配 `ToolTip`，否则内容永远看不到。
4. 窗口不写死 `Height`：用 `SizeToContent="Height"` + `MinHeight`，内容区包一层
   `ScrollViewer`（`VerticalScrollBarVisibility="Auto"`），整窗只允许一层垂直滚动。
5. 底部按钮栏独占一个 `Auto` 行；状态/路径/错误文案另起一行，不要和按钮同行。
6. 禁止负 `Margin`、禁止 `Canvas` 绝对定位排表单、禁止给含换行文本的控件写死 `Height`。
7. 表格/列表文字列 `Width="*"` + `MinWidth`，长值配 `ToolTip`。
8. 窗口内不要用多 tab 承载并排任务；确需 `TabControl` 时**固定它的 `Height`**，
   否则切到内容少的 tab 时窗口会缩矮（内容多了在 tab 内部滚动）。

## 图标行对齐

- 混排 24/28/32 三档图标按钮的行，用**水平 `StackPanel`**；`WrapPanel` 忽略 `VerticalAlignment`，会把图标排得高低错开。
- 主题已给按钮/图标按钮设 `VerticalAlignment="Center"`，别改回 `Stretch`。

## 交互态自检（R15）

| 交互态 | 必查项 |
|---|---|
| 表格单元格编辑 | 编辑器高度 ≤ 行高（`RowHeight` ≥ `Layout.ControlHeight`），文字上下居中 |
| 折叠面板展开长内容 | 内容区出现滚动条，窗口不被撑高，底部按钮栏不被顶出 |
| 日期/下拉弹层打开 | 弹层有内容；按钮不压住输入内容（`DatePickerTextBox` 要 `Stretch`） |
| 长列表滚动 | 竖向滚动条可见，滑块方向正确（`Track.Orientation` 绑定 `TemplateBinding Orientation`） |

预览工程自带状态探针：`dotnet run --project preview -- -shot out/probe.png -window probe`

## 布局骨架

```xml
<Grid Margin="{StaticResource Layout.WindowPadding}">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" /> <!-- 标题 -->
        <RowDefinition Height="*" />    <!-- 主内容/滚动 -->
        <RowDefinition Height="Auto" /> <!-- 按钮 -->
    </Grid.RowDefinitions>

    <ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto">
        <StackPanel>
            <GroupBox Header="选项">...</GroupBox>
        </StackPanel>
    </ScrollViewer>

    <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Right">
        <Button Content="取消" IsCancel="True" Margin="0,0,8,0" />
        <Button Content="保存" Style="{StaticResource Button.Primary}" IsDefault="True" />
    </StackPanel>
</Grid>
```

## 安全表单行（R14）

```xml
<Grid Style="{StaticResource FormRow}">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" MinWidth="{StaticResource Layout.FormLabelMinWidth}" />
        <ColumnDefinition Width="*" MinWidth="{StaticResource Layout.InputMinWidth}" />
    </Grid.ColumnDefinitions>
    <TextBlock Text="起始编号" Style="{StaticResource Text.Label}" />
    <TextBox Grid.Column="1" Style="{StaticResource TextBox.Form}" />
</Grid>
```

## 对比度

| 内容 | 最低 |
|---|---:|
| 正文 | 4.5:1 |
| 大号文字 | 3:1 |
| 焦点/非文字 | 3:1 |
| 禁用文字 | 3:1（建议 4.5:1） |

## 常用命令

```powershell
pwsh ./tools/ContrastCheck.ps1
pwsh ./tools/LayoutCheck.ps1 -Root "D:\your\project"
```

## Top 10 错误

1. 隐式 TextBlock 硬编码前景 → 主按钮蓝底深灰字。
2. 禁用 `Opacity=0.45` → 文字看不清。
3. 只改背景不改前景 → 悬停后文字消失。
4. 选中项白字压浅蓝底。
5. 选中+悬停没有 `MultiTrigger`。
6. ComboBox 弹层还是系统默认样式。
7. DataGridCell 选中背景盖住行背景。
8. 多行 TextBox 默认 `Stretch` 撑满 `*` 行。
9. 框中框反模式：外层 Border 加大 Padding 套内层 TextBox，产生双重框与巨大空白。
10. 工具栏全员次要按钮：破坏性操作（如删除）混在常规操作中无危险警示色。
11. 标签列写死像素宽度 → 长标签被裁成 "起始流…"。
12. 窗口写死 `Height` + 无滚动 → 底部按钮被内容盖住（150% DPI 更明显）。


---

## 图标系统速查 (Google Material Symbols 规范)

### 官方规范基准
- **数据来源**：Google Material Symbols（Apache 2.0 开源许可，自由商用）。
- **视觉重量**：统一 **Outlined / Weight 400 / Grade 0 / Optical Size 24**——不过粗（700）也不过细（200）。
- **坐标视口**：统一 24×24 视口网格，每条几何末尾带 `M0,0 L24,24` 锚点，`Stretch="Uniform"`，`SnapsToDevicePixels="True"`。
- **线型控件**：复选框勾 1.6px，下拉/折叠箭头 1.5px，圆头端点。

### 图标尺寸阶梯
- `Icon.Size.Micro` (12px): 状态徽章、角标
- `Icon.Size.Small` (14px): 表格小操作、单行输入框尾缀
- `Icon.Size.Regular` (16px): **默认尺寸**（按钮、菜单、Tab）
- `Icon.Size.Medium` (20px): 工具栏图标、面板操作
- `Icon.Size.Large` (24px): 提示卡片、大号入口
- `Icon.Size.Hero` (32px): 空状态插画、引导视窗

### 图标着色黄金法则
- **严禁写死 Fill="#..."**。
- 默认继承宿主 `(TextElement.Foreground)`，主按钮自动纯白，禁用自动变灰。
- 纯图标按钮必带 `ToolTip`。
- 图标按钮尺寸矩阵：`Button.Icon.Small` (24px) / `Button.Icon` (28px) / `Button.Icon.Large` (32px)。


