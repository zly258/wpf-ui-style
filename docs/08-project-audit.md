# 08. 三个项目的现状审计

本页记录 `OpenCADTranslate`、`EngineeringAI`、`auto_inventor` 的样式现状、已发现的问题和迁移建议。

---

## 1. OpenCADTranslate

**主题文件**：`src/Shared/WindowTheme.xaml`

### 做得好的地方

- 配色低饱和、工业感强，窗口/面板/边框层次清楚。
- `Button.Primary`、`Button.Danger`、`ListBoxItem`、`DataGridRow` 的选中态基本正确。
- `ComboBoxItem` 选中使用 `SelectionBrush` + 主文字色，没有白字压浅底。
- `DataGrid` 交替行、表头、网格线配色统一。
- 图标与主题色一致。

### 风险点

#### 1. 隐式 TextBlock 硬编码前景

```xml
<Style TargetType="TextBlock">
    <Setter Property="Foreground" Value="{DynamicResource TextPrimaryBrush}"/>
</Style>
```

风险：

- 任何没有 `RecognizesAccessKey="True"` 的 `ContentPresenter` 生成的 `TextBlock`
  都会被强制成正文色。
- 主按钮如果使用 `TextBlock` 内容，会变成蓝底深灰字。
- 状态标签、选中 Tab、危险按钮的语义色可能被覆盖。

建议：改为 `(TextElement.Foreground)` 绑定 + `FallbackValue`，并增加 `AccessText` 样式。

#### 2. 禁用态使用 Opacity

```xml
<Trigger Property="IsEnabled" Value="False">
    <Setter TargetName="Bd" Property="Opacity" Value="0.45"/>
</Trigger>
```

风险：白字与蓝底一起变淡，禁用主按钮几乎不可读。

建议：显式 `Control.Disabled` + `Text.Disabled` + `Border`。

#### 3. CheckBox / RadioButton 内容前景未传递

模板里的 `ContentPresenter` 没有 `TextElement.Foreground="{TemplateBinding Foreground}"`。
禁用时文字可能仍是主色，悬停时也不会跟随状态。

建议：补上绑定，并在模板触发器里设置 `Foreground=Text.Disabled`。

#### 4. ToolTip 未显式样式化

WPF 默认 ToolTip 使用系统颜色，在 CAD 深色主题或高对比度模式下可能不可读。

建议：使用深底白字或浅底深字，并同时设置背景/前景/边框。

#### 5. 字体渲染模式

OpenCADTranslate 使用 `TextFormattingMode="Ideal"`；EngineeringAI 使用 `Display`。
小字号（12px）在 100% DPI 下 `Ideal` 可能发虚。

建议：统一 `Display`，字号提升到 13.5。

#### 6. 控件高度偏小

当前按钮/输入框高度约 26，EngineeringAI 是 34。
在 125%/150% DPI 下 26 高的按钮文字容易拥挤。

建议：统一 `Layout.ControlHeight=34`，小号 30。

---

## 2. EngineeringAI

**主题目录**：`src/Wisdri.EngAI.Core/UI/Themes/`

### 做得好的地方

- **正确解决了隐式 TextBlock 覆盖问题**：
  ```xml
  <Setter Property="Foreground"
          Value="{Binding Path=(TextElement.Foreground),
                  RelativeSource={RelativeSource AncestorType={x:Type ContentPresenter}},
                  FallbackValue={StaticResource Brush.Text.Primary}}" />
  ```
- 同时为 `AccessText` 定义了同样规则。
- `Button.Primary` 明确 `Foreground=White`，禁用态显式设置背景/前景。
- `ComboBoxItem` 选中使用 `Accent.Subtle` + `Text.Primary`。
- `ListBoxItem` 选中使用 `Accent.Subtle` + `Text.Primary`。
- 有 `ThemeForegroundTests` 契约测试，防止回退。
- 布局尺寸、圆角、滚动条、卡片体系完整。

### 风险点

#### 1. 禁用文字对比度不足

`Color.TextDisabled = #9CA3AF`，在 `Color.ControlDisabled = #F2F4F7` 上约 2.2:1。
禁用控件虽然 WCAG 豁免，但实际很难看清。

建议：改为 `#667085`（约 4.51:1），并同步更新对比度测试。

#### 2. 部分 keyed 文本样式带显式前景

`Text.Title` / `Text.Body` / `Text.Section` 都带 `Brush.Text.Primary`。
如果误用在按钮、TabItem、ListBoxItem 内部，会覆盖状态前景。

建议：文档中明确禁止；代码审查时搜索 `Style="{StaticResource Text.`。

#### 3. 隐式 CheckBox 样式只设置 Foreground

EngineeringAI 的隐式 `CheckBox` 样式没有自定义模板，依赖系统模板。
系统模板在高 DPI/不同 Windows 主题下可能和整体风格不一致。

建议：使用本规范的 CheckBox/RadioButton 模板，确保方框、勾选、禁用态可控。

#### 4. 字体大小 13.5 / 控件高度 34

这是当前三个项目里最成熟的尺寸体系，建议作为基线。
OpenCADTranslate 需要从 12/26 迁移过来。

---

## 3. auto_inventor

**主题文件**：`Src/Shared/Presentation/Themes/UnifiedTheme.xaml`

### 做得好的地方

- **刻意不定义隐式 TextBlock 样式**，让文本从父容器继承前景；
  这是避免“主按钮文字变深”的另一种正确做法。
- 按钮模板显式设置：
  ```xml
  TextElement.Foreground="{TemplateBinding Foreground}"
  ```
- `Button` 用 `Tag="Primary"` / `Tag="Danger"` 区分变体，状态完整。
- `ComboBoxItem` 对 Selected + Highlighted 使用 `MultiTrigger`，明确设置背景和前景。
- ToolTip 深底白字，对比度高。
- `TextBox` 的 ReadOnly/Disabled 显式设置背景/前景。
- `GroupBox` 头部显式设置前景。

### 风险点

#### 1. ListBoxItem 选中使用品牌蓝底 + 白字

```xml
<Trigger Property="IsSelected" Value="True">
    <Setter Property="Background" Value="{StaticResource AppBrandBrush}" />
    <Setter Property="Foreground" Value="{StaticResource AppOnBrandTextBrush}" />
</Trigger>
<Trigger Property="IsMouseOver" Value="True">
    <Setter Property="Background" Value="{StaticResource AppControlHoverBrush}" />
</Trigger>
```

如果 `IsMouseOver` 触发器声明在 `IsSelected` 之后，悬停会把背景改成浅蓝，
但前景仍是白字，出现浅底白字。当前顺序下选中触发器在后，暂时正确，
但非常依赖声明顺序。

建议：使用 `MultiTrigger` 明确“选中 + 悬停”，并优先采用浅底深字。
如果坚持品牌蓝底白字，必须同时定义 Selected+Hover 的深蓝底 + 白字。

#### 2. 品牌蓝 `#1D4ED8` 与 OpenCADTranslate `#2F6F9F` 不一致

两个项目视觉风格不同。如果未来合并 UI 库，需要统一 Accent。

建议：
- CAD 插件统一使用 `#2F6F9F`（低饱和工业蓝）。
- 如果 auto_inventor 面向网页/桌面消费级产品，可保留 `#1D4ED8`，
  但需要单独维护一套 token，不要和 CAD 插件混用。

#### 3. `AppMutedTextBrush` 与 `AppSecondaryTextBrush` 近似

`#64748B` 与 `#475569` 都能作为次要文字；同一界面混用会产生“灰得不一样”的观感。

建议：统一使用 `Text.Secondary`；只有禁用态使用 `Text.Disabled`。

#### 4. 部分隐式样式依赖系统模板

`CheckBox`、`ListBoxItem`、`TreeViewItem` 等只设置了颜色，没有替换模板。
系统模板的选中/悬停/禁用状态可能和主题不完全一致。

建议：逐步替换为本规范的模板。

---

## 4. 跨项目共性问题

| 问题 | 影响项目 | 建议 |
|---|---|---|
| 控件高度 26 / 28 / 34 混用 | 三个项目 | 统一 34，紧凑用 30 |
| 字号 12 / 13 / 13.5 混用 | 三个项目 | 统一 Body 13.5、Caption 13 |
| Accent 两种蓝 | OpenCADTranslate / auto_inventor | CAD 插件统一 `#2F6F9F` |
| 禁用文字过浅 | OpenCADTranslate / EngineeringAI | 统一 `#667085` |
| 隐式 TextBlock 硬编码 | OpenCADTranslate | 改为 `(TextElement.Foreground)` |
| 选中项白字压浅底 | auto_inventor 潜在 | `MultiTrigger` + 浅底深字 |
| ToolTip 未统一 | OpenCADTranslate | 深底白字 |
| 多行文本默认 Stretch / 缺少 MaxHeight | OpenCADTranslate / auto_inventor | 表单 Top + Min/Max；只有日志放在 `*` 行 |
| 主题入口不统一 | 三个项目 | 统一 `Theme/Theme.xaml` |

### 多行文本高度问题明细

`LayoutCheck.ps1` 在 OpenCADTranslate 中报告了以下位置：

- `BatchProgressWindow.xaml` 日志框：缺少 `MinHeight`/`MaxHeight`。
- `SettingsWindow.xaml`（AutoCAD/Inventor）系统提示词多行框：缺少 `MinHeight`/`MaxHeight`。
- `TextTranslationWindow.xaml` 输入/输出多行框：缺少 `MinHeight`/`MaxHeight`，
  且使用了 `VerticalAlignment="Stretch"`。
- `DrawingTranslationWindow.xaml` 日志框：缺少 `MinHeight`/`MaxHeight`。

建议：

- 表单输入（系统提示词、翻译输入）：`Height="Auto"` + `MinHeight`/`MaxHeight` + `VerticalAlignment="Top"`。
- 日志框：放在专用 `*` 行，设置 `MinHeight=120`、`MaxHeight=240`，允许 `Stretch`。
- 不要在 `StackPanel` 里给日志框设置很大的 `MinHeight`。

---

## 5. 推荐迁移优先级

1. **P0**：修隐式 TextBlock 前景契约（OpenCADTranslate）。
2. **P0**：修禁用态 Opacity（OpenCADTranslate）。
3. **P0**：修 CheckBox/RadioButton 内容前景（OpenCADTranslate）。
4. **P1**：统一控件高度/字号（三个项目）。
5. **P1**：统一禁用文字色（EngineeringAI）。
6. **P1**：ListBoxItem 选中+悬停 `MultiTrigger`（auto_inventor）。
7. **P2**：统一 Accent（如果合并 UI 库）。
8. **P2**：把 ToolTip、ComboBox 弹层、DataGrid 单元格纳入统一主题。
9. **P2**：增加契约测试与对比度脚本到 CI。
