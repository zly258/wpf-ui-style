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
| `FontSize.Title` / `Section` | 15.5 |
| `FontSize.Body` | 14.5 |
| `FontSize.Caption` | 14 |
| `FontSize.Code` | 13.5 |
| `LineHeight.Body` / `Caption` | 23 / 22 |

- 业务 XAML 禁止出现 `FontFamily`，禁止写 `FontSize="13"` 字面量。
- 复选框 / 单选框不得小于 **16×16**（`Layout.CheckBox.Size`）。
- 字号变化后必须同步检查：控件高度（36/32）、行高（30）、表头（32）、Tab/Menu/ToolTip/DataGrid 内边距。

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
- 图标按钮尺寸矩阵：`Button.Icon.Small` (26px) / `Button.Icon` (30px) / `Button.Icon.Large` (36px)。


