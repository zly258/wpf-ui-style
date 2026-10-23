# 02. 控件状态矩阵

这是本规范最重要的一页。绝大多数“文字看不清”都来自状态组合没有同时定义背景和前景。

---

## 1. 状态定义

| 状态 | WPF 属性 | 说明 |
|---|---|---|
| Normal | — | 默认 |
| Hover | `IsMouseOver=True` | 鼠标悬停 |
| Pressed | `IsPressed=True` | 鼠标按下 |
| Focused | `IsKeyboardFocused=True` / `IsKeyboardFocusWithin=True` | 键盘焦点 |
| Selected | `IsSelected=True` | 列表/表格/下拉项选中 |
| Selected+Hover | `IsSelected=True` + `IsMouseOver=True` | 必须用 `MultiTrigger` |
| Checked | `IsChecked=True` | 复选框/单选框 |
| Disabled | `IsEnabled=False` | 禁用 |
| ReadOnly | `IsReadOnly=True` | 只读输入框 |
| Error | 业务状态（Tag/附加属性） | 校验失败 |
| Warning / Success | 业务状态 | 提示 |

---

## 2. 五条状态规则

### 规则 1：背景变了，前景必须跟着变

```xml
<!-- 错误：只改背景 -->
<Trigger Property="IsMouseOver" Value="True">
    <Setter Property="Background" Value="{StaticResource Brush.Control.Hover}" />
</Trigger>

<!-- 正确 -->
<Trigger Property="IsMouseOver" Value="True">
    <Setter Property="Background" Value="{StaticResource Brush.Control.Hover}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Primary}" />
</Trigger>
```

### 规则 2：触发器顺序不可依赖

WPF 会按声明顺序应用 `Trigger`，后声明的覆盖前面的。不要靠“我把选中放在悬停后面”来保证正确；
组合状态必须用 `MultiTrigger`。

### 规则 3：禁用不用透明度

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

### 规则 4：浅底深字，深底白字

白字只允许出现在：
`Accent` / `Accent.Hover` / `Accent.Pressed` /
`Danger` / `Danger.Hover` / `Danger.Pressed` /
`Tooltip.Background`。

浅底（Surface / SurfaceAlt / Control.Hover / Selection / Accent.Subtle / Danger.Subtle）
一律使用 `Text.Primary` 或对应的深色语义色。

### 规则 5：内容宿主必须传递前景

```xml
<ContentPresenter
    TextElement.Foreground="{TemplateBinding Foreground}" />
```

`RecognizesAccessKey="True"` 生成的是 `AccessText`，不是 `TextBlock`；
两者都要有跟随 `(TextElement.Foreground)` 的隐式样式。

---

## 3. 按钮

### 3.1 Button.Secondary（默认按钮）

| 状态 | Background | Foreground | BorderBrush |
|---|---|---|---|
| Normal | `Brush.Surface` | `Brush.Text.Primary` | `Brush.Border` |
| Hover | `Brush.Control.Hover` | `Brush.Text.Primary` | `Brush.Border.Strong` |
| Pressed | `Brush.Control.Pressed` | `Brush.Text.Primary` | `Brush.Accent` |
| Focused | 不变 | 不变 | `Brush.Focus` |
| Disabled | `Brush.Control.Disabled` | `Brush.Text.Disabled` | `Brush.Border` |

### 3.2 Button.Primary（主按钮）

| 状态 | Background | Foreground | BorderBrush |
|---|---|---|---|
| Normal | `Brush.Accent` | `Brush.OnAccent` | `Brush.Accent` |
| Hover | `Brush.Accent.Hover` | `Brush.OnAccent` | `Brush.Accent.Hover` |
| Pressed | `Brush.Accent.Pressed` | `Brush.OnAccent` | `Brush.Accent.Pressed` |
| Focused | 不变 | 不变 | `Brush.Accent.Pressed` |
| Disabled | `Brush.Control.Disabled` | `Brush.Text.Disabled` | `Brush.Border` |

> **绝对不要**在 Hover/Pressed 时把背景改成浅色却保留白字。
> 主按钮的 Hover/Pressed 必须是更深的蓝，白字才成立。

### 3.3 Button.Danger（危险按钮）

| 状态 | Background | Foreground | BorderBrush |
|---|---|---|---|
| Normal | `Brush.Surface` | `Brush.Danger` | `Brush.Danger` |
| Hover | `Brush.Danger.Subtle` | `Brush.Danger.Hover` | `Brush.Danger.Hover` |
| Pressed | `Brush.Danger.Subtle` | `Brush.Danger.Pressed` | `Brush.Danger.Pressed` |
| Focused | 不变 | 不变 | `Brush.Danger.Pressed` |
| Disabled | `Brush.Control.Disabled` | `Brush.Text.Disabled` | `Brush.Border` |

> 危险按钮的悬停是**浅红底 + 深红字**，不是红底白字。
> 只有实心红底才允许白字；本规范的危险按钮默认是描边风格。

### 3.4 Button.Text（文本/标题按钮）

| 状态 | Background | Foreground | Border |
|---|---|---|---|
| Normal | `Transparent` | `Brush.Text.Secondary` | 无 |
| Hover | `Brush.Control.Hover` | `Brush.Text.Primary` | 无 |
| Pressed | `Brush.Control.Pressed` | `Brush.Text.Primary` | 无 |
| Focused | 不变 | 不变 | `Brush.Focus` 1px |
| Disabled | `Transparent` | `Brush.Text.Disabled` | 无 |

---

## 4. 输入框 / 密码框

| 状态 | Background | Foreground | BorderBrush |
|---|---|---|---|
| Normal | `Brush.Surface` | `Brush.Text.Primary` | `Brush.Border` |
| Hover | `Brush.Surface` | `Brush.Text.Primary` | `Brush.Border.Strong` |
| Focused | `Brush.Surface` | `Brush.Text.Primary` | `Brush.Focus` |
| ReadOnly | `Brush.SurfaceAlt` | `Brush.Text.Secondary` | `Brush.Border` |
| Disabled | `Brush.Control.Disabled` | `Brush.Text.Disabled` | `Brush.Border` |
| Error | `Brush.Surface` | `Brush.Text.Primary` | `Brush.Danger` |

**注意：**

- 错误态只改边框和下方提示文字，不要把输入框背景改成浅红；浅红底 + 深色文字虽然可读，
  但会与“危险按钮悬停”混淆。
- 错误提示文字使用 `Text.Danger`，放在输入框下方，不要只靠颜色。
- 多行输入框的 `VerticalContentAlignment=Top`，单行输入框 `Center`。
- 密码框与输入框视觉完全一致。

---

## 5. ComboBox

### 5.1 ComboBox 本体

| 状态 | Background | Foreground | BorderBrush |
|---|---|---|---|
| Normal | `Brush.Surface` | `Brush.Text.Primary` | `Brush.Border` |
| Hover | `Brush.Surface` | `Brush.Text.Primary` | `Brush.Border.Strong` |
| Focused / Open | `Brush.Surface` | `Brush.Text.Primary` | `Brush.Focus` |
| Disabled | `Brush.Control.Disabled` | `Brush.Text.Disabled` | `Brush.Border` |

弹出层：

- Background `Brush.Surface`
- BorderBrush `Brush.Border.Strong`
- 文本 `Brush.Text.Primary`
- 不要使用系统默认的白色/灰色弹层，否则会和主题割裂。

### 5.2 ComboBoxItem

| 状态 | Background | Foreground |
|---|---|---|
| Normal | `Transparent` | `Brush.Text.Primary` |
| Hover | `Brush.Control.Hover` | `Brush.Text.Primary` |
| Selected | `Brush.Accent.Subtle` | `Brush.Text.Primary` |
| Selected+Hover | `Brush.Accent.SubtleHover` | `Brush.Text.Primary` |
| Disabled | `Transparent` | `Brush.Text.Disabled` |

> 下拉项选中**不要用品牌蓝底 + 白字**。浅蓝底 + 深字更稳，
> 也不会在“选中+悬停”时出现白字压浅底。

---

## 6. CheckBox / RadioButton

| 状态 | 文本 Foreground | 方框/圆环 Background | 方框/圆环 BorderBrush |
|---|---|---|---|
| Normal | `Brush.Text.Primary` | `Brush.Surface` | `Brush.Border` |
| Hover | `Brush.Text.Primary` | `Brush.Surface` | `Brush.Border.Strong` |
| Focused | `Brush.Text.Primary` | `Brush.Surface` | `Brush.Focus` |
| Checked | `Brush.Text.Primary` | `Brush.Accent` | `Brush.Accent` |
| Checked+Hover | `Brush.Text.Primary` | `Brush.Accent.Hover` | `Brush.Accent.Hover` |
| Disabled | `Brush.Text.Disabled` | `Brush.Control.Disabled` | `Brush.Border` |

- 勾选符号/圆点：`Brush.OnAccent`（白）。
- 禁用时勾选符号改为 `Brush.Text.Disabled`，不要保留白色。
- 文本通过 `ContentPresenter TextElement.Foreground="{TemplateBinding Foreground}"` 传递。
- 不要用 `Opacity` 表示禁用。
- **方框/圆环尺寸固定为 `Layout.CheckBox.Size` / `Layout.RadioButton.Size`（16×16），不得缩小**；勾选符号使用圆头端点、描边 1.6px，方框与文字间距 8px。


---

## 7. ListBox / ListView

| 状态 | Background | Foreground |
|---|---|---|
| Normal | `Transparent` | `Brush.Text.Primary` |
| Hover | `Brush.Control.Hover` | `Brush.Text.Primary` |
| Selected | `Brush.Selection` | `Brush.Text.Primary` |
| Selected+Hover | `Brush.Selection.Hover` | `Brush.Text.Primary` |
| Disabled | `Transparent` | `Brush.Text.Disabled` |

容器：

- ListBox/ListView Background `Brush.Surface`
- BorderBrush `Brush.Border`
- 行高/内边距使用 `Layout.ListItemPadding`。

> 禁止“品牌蓝底 + 白字”的选中项，除非整个应用明确使用深色选中风格，
> 并且为 Hover、Selected+Hover、Disabled 都定义了对应前景。
> 本规范统一使用浅蓝底 + 深字。

---

## 8. DataGrid

| 元素/状态 | Background | Foreground |
|---|---|---|
| DataGrid | `Brush.Surface` | `Brush.Text.Primary` |
| 行 Normal | `Brush.Surface` | `Brush.Text.Primary` |
| 交替行 | `Brush.SurfaceSunken` | `Brush.Text.Primary` |
| 行 Hover | `Brush.Control.Hover` | `Brush.Text.Primary` |
| 行 Selected | `Brush.Selection` | `Brush.Text.Primary` |
| 行 Selected+Hover | `Brush.Selection.Hover` | `Brush.Text.Primary` |
| 单元格 Selected | `Transparent` | `Brush.Text.Primary` |
| 表头 | `Brush.SurfaceAlt` | `Brush.Text.Secondary` |
| 网格线 | `Brush.Border` | — |
| Disabled | `Brush.Control.Disabled` | `Brush.Text.Disabled` |

**关键：** `DataGridCell` 的默认选中会自己画蓝色背景。必须在 `DataGridCell`
的 `IsSelected` 触发器里把 `Background` 设为 `Transparent`，让行的选中背景透出来，
同时把 `Foreground` 固定为 `Text.Primary`。

---

## 9. TabItem

| 状态 | Background | Foreground | 下划线 |
|---|---|---|---|
| Normal | `Transparent` | `Brush.Text.Secondary` | 透明 |
| Hover | `Brush.Control.Hover` | `Brush.Text.Primary` | 透明 |
| Selected | `Transparent` | `Brush.Accent` | `Brush.Accent` 2px |
| Disabled | `Transparent` | `Brush.Text.Disabled` | 透明 |

---

## 10. Expander

| 状态 | Header Background | Header Foreground | 内容 |
|---|---|---|---|
| Normal | `Transparent` | `Brush.Text.Primary` | `Brush.Surface` |
| Hover | `Brush.Control.Hover` | `Brush.Text.Primary` | 不变 |
| Expanded | `Transparent` | `Brush.Text.Primary` | 显示，顶部 1px `Brush.Border` |
| Disabled | `Transparent` | `Brush.Text.Disabled` | 不变 |

---

## 11. GroupBox

- Header：`Brush.Text.Primary`，字号 `FontSize.Section`，常规字重。
- 内容：`Brush.Surface`，边框 `Brush.Border`，圆角 `Radius.Card`，内边距 `Layout.GroupPadding`。
- 不要在 GroupBox 内部再套一层同色卡片；一层边框足够。
- GroupBox 不是交互控件，不需要 Hover/Selected。

---

## 12. ToolTip

| 属性 | 值 |
|---|---|
| Background | `Brush.Tooltip.Background` (`#1F2933`) |
| Foreground | `Brush.Tooltip.Foreground` (`#FFFFFF`) |
| BorderBrush | `Brush.Tooltip.Background` |
| Padding | `8,5` |
| FontSize | `FontSize.Caption` |

深底白字，对比度 14.76:1。
不要让 ToolTip 使用系统默认白底黑字或黑底黑字。

---

## 13. ProgressBar / StatusBar

**ProgressBar**

- Track：`Brush.SurfaceAlt`
- Fill：`Brush.Accent`
- Border：`Brush.Border`
- 高度 8。
- 文字（如果有）使用 `Brush.Text.Secondary`。

**StatusBar**

- Background：`Brush.Surface`
- Foreground：`Brush.Text.Secondary`
- 顶部边框：`Brush.Border`
- `StatusBarItem` 的前景绑定到父级 `StatusBar.Foreground`。

---

## 14. MenuItem

| 状态 | Background | Foreground |
|---|---|---|
| Normal | `Transparent` | `Brush.Text.Primary` |
| Highlighted/Hover | `Brush.Control.Hover` | `Brush.Text.Primary` |
| Disabled | `Transparent` | `Brush.Text.Disabled` |

> 如果使用复杂的子菜单/图标/勾选菜单，需要单独测试键盘导航和模板；
> 本主题只提供基础颜色契约，不重写完整 MenuItem 模板。

---

## 15. 组合状态速查

| 组合 | 必须设置的属性 | 推荐值 |
|---|---|---|
| Selected + Hover | Background, Foreground | `Selection.Hover` + `Text.Primary` |
| Checked + Hover | Background, BorderBrush | `Accent.Hover` + `Accent.Hover` |
| Focused + Hover | BorderBrush | `Focus` |
| Disabled + Selected | Background, Foreground | `Control.Disabled` + `Text.Disabled` |
| ReadOnly + Focus | Background, Foreground, Border | `SurfaceAlt` + `Text.Secondary` + `Focus` |

---

## 16. 错误示例

### 16.1 浅底白字

```xml
<!-- 错误：悬停浅底，但选中态仍是白字 -->
<Trigger Property="IsMouseOver" Value="True">
    <Setter Property="Background" Value="{StaticResource Brush.Control.Hover}" />
</Trigger>
<Trigger Property="IsSelected" Value="True">
    <Setter Property="Foreground" Value="White" />
</Trigger>
```

修复：选中使用 `Selection` + `Text.Primary`；选中+悬停用 `MultiTrigger`。

### 16.2 只改背景

```xml
<!-- 错误：悬停后文字可能仍是白字 -->
<Trigger Property="IsMouseOver" Value="True">
    <Setter Property="Background" Value="{StaticResource Brush.Control.Hover}" />
</Trigger>
```

修复：同一触发器内加 `Foreground=Text.Primary`。

### 16.3 透明度禁用

```xml
<!-- 错误：白字和蓝底一起变淡 -->
<Trigger Property="IsEnabled" Value="False">
    <Setter Property="Opacity" Value="0.45" />
</Trigger>
```

修复：显式禁用背景/前景/边框。

### 16.4 隐式 TextBlock 覆盖按钮前景

```xml
<!-- 错误：所有 TextBlock 都被强制成正文色 -->
<Style TargetType="TextBlock">
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Primary}" />
</Style>
```

修复：使用 `(TextElement.Foreground)` 绑定 + Fallback。

### 16.5 ContentPresenter 不传前景

```xml
<!-- 错误：CheckBox 禁用时文本仍可能是主色 -->
<ContentPresenter />
```

修复：

```xml
<ContentPresenter TextElement.Foreground="{TemplateBinding Foreground}" />
```
