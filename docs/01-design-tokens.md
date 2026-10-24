# 01. 设计令牌（Design Tokens）

令牌是唯一色值来源。业务 XAML 只允许引用 `Brush.*`、`FontSize.*`、`Layout.*`、`Radius.*`，禁止直接写十六进制颜色。

---

## 1. 颜色令牌

### 1.1 表面与边框

| 令牌 | 色值 | 用途 | 备注 |
|---|---|---|---|
| `Color.WindowBackground` | `#E9EDF2` | 窗口底色 | 低饱和蓝灰，避免纯白刺眼 |
| `Color.Surface` | `#FAFBFC` | 卡片、输入框、列表、弹层 | 主内容表面 |
| `Color.SurfaceAlt` | `#F2F4F7` | 分组、表头、只读框、禁用底 | 比 Surface 深一档 |
| `Color.SurfaceSunken` | `#F7F8FA` | 代码框、日志、交替行 | 轻微下沉 |
| `Color.Border` | `#D0D5DB` | 普通边框、分隔线 | 装饰性，不强制 3:1 |
| `Color.BorderStrong` | `#AEB7C2` | 悬停边框、弹层边框 | 比普通边框更明显 |
| `Color.Focus` | `#2F6F9F` | 键盘焦点边框 | 与 Surface 约 5.2:1 |

### 1.2 文字

| 令牌 | 色值 | 用途 | 在 Surface 上对比度 |
|---|---|---|---|
| `Color.TextPrimary` | `#1F2933` | 正文、标题、主按钮外文字 | 14.24:1 |
| `Color.TextSecondary` | `#4B5563` | 说明、表头、次要文字 | 7.29:1 |
| `Color.TextDisabled` | `#667085` | 禁用文字 | 在 `ControlDisabled` 上 4.51:1 |
| `Color.OnAccent` | `#FFFFFF` | 品牌色/危险色上的文字 | 在 Accent 上 5.39:1 |

> `TextDisabled` 不使用 `#9CA3AF`。后者在 `#F2F4F7` 上只有约 2.2:1，
> 是“禁用按钮看不清”的主要原因。

### 1.3 品牌与语义

| 令牌 | 色值 | 用途 | 白字对比度 |
|---|---|---|---|
| `Color.Accent` | `#2F6F9F` | 主按钮、选中下划线、焦点 | 5.39:1 |
| `Color.AccentHover` | `#285F88` | 主按钮悬停 | 6.81:1 |
| `Color.AccentPressed` | `#22506F` | 主按钮按下 | 8.59:1 |
| `Color.AccentSubtle` | `#E4EEF5` | 选中项、轻量强调底 | 深字 12.54:1 |
| `Color.AccentSubtleHover` | `#D8E6F0` | 选中 + 悬停 | 深字 11.59:1 |
| `Color.Danger` | `#B43B32` | 危险按钮/错误文字 | 5.81:1 |
| `Color.DangerHover` | `#97302A` | 危险悬停 | 7.58:1 |
| `Color.DangerPressed` | `#7E2823` | 危险按下 | 9.48:1 |
| `Color.DangerSubtle` | `#FBEDEC` | 错误提示底、危险悬停底 | 深字 12.95:1 |
| `Color.Success` | `#2A704E` | 成功文字/图标 | 在 SuccessSubtle 上 5.29:1 |
| `Color.SuccessSubtle` | `#E9F4EE` | 成功提示底 | — |
| `Color.Warning` | `#7A5A1E` | 警告文字/图标 | 在 WarningSubtle 上 5.60:1 |
| `Color.WarningSubtle` | `#F7F0E3` | 警告提示底 | — |

> 不要用 `#2F7D57` / `#8C6A22` 作为语义文字色；它们在各自浅底上低于 4.5:1。
> 本规范已调整为 `#2A704E` / `#7A5A1E`。

### 1.4 交互态

| 令牌 | 色值 | 用途 | 在 Surface 上深字对比度 |
|---|---|---|---|
| `Color.ControlHover` | `#F0F3F6` | 次要按钮、列表项悬停 | 13.25:1 |
| `Color.ControlPressed` | `#E4E8EE` | 次要按钮按下 | 12.00:1 |
| `Color.ControlDisabled` | `#F2F4F7` | 禁用背景 | — |
| `Color.Selection` | `#E4EEF5` | 列表/表格选中 | 12.54:1 |
| `Color.SelectionHover` | `#D8E6F0` | 选中 + 悬停 | 11.59:1 |

### 1.5 Tooltip、滚动条、卡片

| 令牌 | 色值 | 用途 |
|---|---|---|
| `Color.TooltipBackground` | `#1F2933` | Tooltip 深底 |
| `Color.TooltipForeground` | `#FFFFFF` | Tooltip 白字（14.76:1） |
| `Color.ScrollThumb` | `#C3CAD3` | 滚动条滑块 |
| `Color.ScrollThumbHover` | `#A7B0BB` | 滚动条悬停 |
| `Color.CardSystem` | `#F2F4F7` | 系统消息卡片 |
| `Color.CardUser` | `#E4EEF5` | 用户消息卡片 |
| `Color.CardAi` | `#FAFBFC` | AI 消息卡片 |
| `Color.CardError` | `#FBEDEC` | 错误卡片 |
| `Color.Shadow` | `#1F2933` | 浮层投影颜色 |

### 1.6 状态标签

| 状态 | 背景 | 前景 |
|---|---|---|
| Idle | `Brush.Badge.Idle.Background` (`#F2F4F7`) | `Brush.Badge.Idle.Foreground` (`#4B5563`) |
| Active | `Brush.Badge.Active.Background` (`#E4EEF5`) | `Brush.Badge.Active.Foreground` (`#2F6F9F`) |
| Done | `Brush.Badge.Done.Background` (`#E9F4EE`) | `Brush.Badge.Done.Foreground` (`#2A704E`) |
| Stopped | `Brush.Badge.Stopped.Background` (`#F7F0E3`) | `Brush.Badge.Stopped.Foreground` (`#7A5A1E`) |
| Failed | `Brush.Badge.Failed.Background` (`#FBEDEC`) | `Brush.Badge.Failed.Foreground` (`#B43B32`) |

---

## 2. 画刷命名

| 类别 | 命名 | 示例 |
|---|---|---|
| 表面 | `Brush.Surface*` | `Brush.Surface`, `Brush.SurfaceAlt` |
| 边框 | `Brush.Border*` | `Brush.Border`, `Brush.Border.Strong` |
| 文字 | `Brush.Text.*` | `Brush.Text.Primary`, `Brush.Text.Disabled` |
| 品牌 | `Brush.Accent*` | `Brush.Accent`, `Brush.Accent.Hover` |
| 危险 | `Brush.Danger*` | `Brush.Danger`, `Brush.Danger.Subtle` |
| 语义 | `Brush.Success*`, `Brush.Warning*` | `Brush.Success`, `Brush.Warning.Subtle` |
| 交互 | `Brush.Control.*`, `Brush.Selection*` | `Brush.Control.Hover` |
| 其他 | `Brush.Tooltip.*`, `Brush.ScrollThumb*` | — |

**规则：**

- `Color.*` 只允许在 `Tokens.xaml` 内部被 `Brush.*` 引用。
- 业务 XAML 只引用 `Brush.*`。
- 不要在业务 XAML 里写 `Color="{StaticResource Color...}"`。

---

## 3. 字体与字号

| 令牌 | 值 | 用途 |
|---|---|---|
| `Font.Ui` | `Microsoft YaHei, Microsoft YaHei UI, Segoe UI` | 全局唯一字体栈（默认微软雅黑） |
| `FontSize.Title` | 15.5 | 窗口标题、页面标题 |
| `FontSize.Section` | 15.5 | 分组标题 |
| `FontSize.Body` | 14.5 | 正文、按钮、输入框 |
| `FontSize.Caption` | 14 | 说明、表头、Tooltip |
| `FontSize.Code` | 13.5 | 日志、代码、Schema 预览 |
| `LineHeight.Body` | 23 | 正文行高 |
| `LineHeight.Caption` | 22 | 说明行高 |

**规则：**

- **字体只定义一次**：仅在 `Tokens.xaml` 的 `Font.Ui` 声明；业务 XAML 禁止写任何 `FontFamily`。
- **字号只引用令牌**：业务 XAML 禁止写 `FontSize="13"` 之类的字面量。
- 一个窗口最多使用 3 种字号：Title/Section、Body、Caption。
- 不要用 `FontWeight="Bold"`；需要强调时用 `SemiBold`。
- 不要混用 Consolas、Arial、宋体；代码/日志也使用 `Font.Ui`，避免字体跳变。
- 字号提升后，控件高度、行高、内边距必须同步放大，避免文字被挤压或裁切。

---

## 4. 尺寸与间距

### 4.1 控件高度

| 令牌 | 值 | 用途 |
|---|---|---|
| `Layout.ControlHeight` | 36 | 按钮、输入框、下拉框、密码框 |
| `Layout.ControlHeight.Small` | 32 | 小号按钮、工具条 |
| `Layout.TagHeight` | 26 | 标签、徽章 |
| `Layout.IconButton.Micro` | 26 | 紧凑图标按钮 |
| `Layout.IconButton.Small` | 30 | 标准行内图标按钮 |
| `Layout.IconButton.Large` | 36 | 大号工具条图标按钮 |
| `Layout.RowHeight` | 30 | 列表/表格行 |
| `Layout.HeaderHeight` | 32 | 表格表头 |
| `Layout.CheckBox.Size` | 16 | 复选框方框 |
| `Layout.RadioButton.Size` | 16 | 单选框圆环 |

> 同一窗口内不要混用 30/32/36。需要紧凑时整组使用 Small。
> 复选框/单选框不得小于 16×16，否则 125% DPI 下难以辨认与点击。


### 4.1.1 图标尺寸阶梯 (Icon Sizing)

| 令牌 | 尺寸 (px) | 用途 |
|---|---|---|
| `Icon.Size.Micro` | 12 | 极小徽章、树节点收折标记 |
| `Icon.Size.Small` | 14 | 表格行内小操作、单行输入框辅助图标 |
| `Icon.Size.Regular` | 16 | **默认尺寸**：按钮、菜单项、Tab 标头 |
| `Icon.Size.Medium` | 20 | 工具栏大图标、分组标题栏操作按钮 |
| `Icon.Size.Large` | 24 | 通知卡片、对话框提示图标 |
| `Icon.Size.Hero` | 32 | 空状态插画、主引导图标 |

### 4.1.2 图标按钮尺寸 (IconButton)

| 令牌 | 按钮宽高 (px) | 适用图标尺寸 | 用途 |
|---|---|---|---|
| `Layout.IconButton.Micro` | 26 | 14 (`Icon.Size.Small`) | 紧凑表格操作、搜索框清除按钮 |
| `Layout.IconButton.Small` | 30 | 16 (`Icon.Size.Regular`) | 标准工具条按钮、面板小操作 |
| `Layout.IconButton.Large` | 36 | 20 (`Icon.Size.Medium`) | 大号主工具条按钮 |


### 4.2 间距

| 令牌 | 值 | 用途 |
|---|---|---|
| `Layout.WindowPadding` | 16 | 窗口内容边距 |
| `Layout.GroupPadding` | 12 | GroupBox 内边距 |
| `Layout.FormRowMargin` | `0,4,0,4` | 表单行上下间距 |
| `Layout.ButtonMinWidth` / `Layout.ButtonMinWidth.Small` | `72` / `52` | 按钮最小宽度（由主题样式引用） |
| `Layout.ButtonPadding` | `10,0` | 按钮左右内边距（防长文挤压） |
| `Layout.LogMaxHeight` | `240` | 日志/代码 TextBox 可选高度上限（见 R9，不强制 MinHeight） |
| `Layout.InputPadding` | `6,0` | 单行输入框（预留充分文字空间） |
| `Layout.InputPadding.Multiline` | `6,6` | 多行输入框 |
| `Layout.ListItemPadding` | `10,6` | 列表项 |


> 提示：按钮、输入框内部 Padding 严格控制在 6–10px 以内，严禁设置过大（如 14px 以上或过大上下 Padding），避免在高 DPI 或中英双语下文字截断、字符末尾丢失。


间距梯度：**2 / 4 / 6 / 8 / 12 / 16 / 24 / 32**。
不要使用 5、7、9、13、17 等奇数间距（图标微调除外）。

### 4.3 圆角

| 令牌 | 值 | 用途 |
|---|---|---|
| `Radius.Small` | 2 | 按钮、输入框、下拉框 |
| `Radius.Card` | 4 | 卡片、分组、弹层 |
| `Radius.Input` | 4 | 多行输入框 |
| `Radius.Pill` | 10 | 状态标签胶囊 |

> 不要把按钮做成大圆角/胶囊；工业桌面工具保持 2–4px。
> 状态标签可以用胶囊，其他控件不要。

---

## 5. 文本样式

| 样式 | 用途 | 注意 |
|---|---|---|
| `Text.Title` | 页面/窗口标题 | 带显式前景，只用于静态文本 |
| `Text.Section` | 分组标题 | 带显式前景，只用于静态文本 |
| `Text.Body` | 正文 | 带显式前景 |
| `Text.Caption` | 说明 | 带次要前景 |
| `Text.Label` | 表单标签 | 带主前景 |
| `Text.Danger` / `Text.Success` / `Text.Warning` | 语义文字 | 带语义色 |
| `Text.Code` | 代码/日志 | 使用 UI 字体，不用等宽 |
| `Text.RoleLabel` | 角色/状态小标签 | 带浅底 |

**禁止：** 在 `Button`、`CheckBox`、`TabItem`、`ListBoxItem`、`ComboBoxItem` 内部使用
`Text.Title` / `Text.Body` 等带显式前景的样式。它们会把控件的状态前景覆盖掉。
这些位置应使用隐式 `TextBlock`（跟随内容宿主）或直接绑定 `TextElement.Foreground`。

---

## 6. 新增令牌的规则

1. 先判断是否能用现有令牌组合表达；能复用就不要新增。
2. 新颜色必须同时提供 `Color.*` 和 `Brush.*`，并说明用途。
3. 新颜色必须给出在至少一个背景上的对比度。
4. 不要新增“近似重复”的颜色（例如 `#2F6F9F` 与 `#2E6E9E`）。
5. 新增后运行 `tools/ContrastCheck.ps1`。
6. 在 `docs/01-design-tokens.md` 和 `docs/05-accessibility-contrast.md` 同步登记。
