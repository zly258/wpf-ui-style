# WPF UI 速查表

## 五条前景/背景规则

1. 背景变了，前景必须一起变。
2. 禁用不用 `Opacity`；用 `Control.Disabled` + `Text.Disabled`。
3. 隐式 `TextBlock`/`AccessText` 前景绑定 `(TextElement.Foreground)`，不要硬编码。
4. 白字只压在 `Accent` / `Accent.Hover` / `Accent.Pressed` / `Danger*` / `Tooltip.Background`。
5. 选中+悬停用 `MultiTrigger`，不要依赖触发器顺序。

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

## 多行文本高度

```xml
<!-- 表单多行输入：不要撑满 -->
<TextBox Style="{StaticResource TextBox.Multiline}"
         Height="Auto" MinHeight="90" MaxHeight="160"
         VerticalAlignment="Top" />

<!-- 日志：允许填充 -->
<TextBox Grid.Row="1" Style="{StaticResource TextBox.Code}"
         MinHeight="120" MaxHeight="240"
         VerticalAlignment="Stretch" />
```

规则：

- 表单行 `Auto`，日志/列表/表格行 `*`。
- 多行输入 `Top`，日志 `Stretch`。
- 不要写 `Height="*"`。
- 不要在 `StackPanel` 里放一个要“填满”的日志框。

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


---

## 图标系统速查 (Google Material Symbols 规范)

### 官方规范基准
- **数据来源**：Google Material Symbols（Apache 2.0 开源许可，自由商用）。
- **坐标视口**：统一 24×24 视口网格，`Stretch="Uniform"`，`SnapsToDevicePixels="True"`。

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
- 图标按钮尺寸矩阵：`Button.Icon.Small` (24px) / `Button.Icon` (28px) / `Button.Icon.Large` (34px)。


