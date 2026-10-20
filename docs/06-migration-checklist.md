# 06. 现有项目迁移清单

适用于把 `OpenCADTranslate`、`EngineeringAI`、`auto_inventor` 或新插件迁移到本规范。

---

## 1. 迁移顺序

1. **盘点**
   - 找出所有 `.xaml`、资源字典、窗口基类、ThemeLoader。
   - 找出所有十六进制颜色、局部 `FontFamily`、局部 `Height`。
2. **接入主题**
   - 在 App 或窗口基类合并 `Theme/Theme.xaml`。
   - 删除窗口内重复的颜色/按钮/输入框模板。
3. **替换令牌**
   - 按下方映射表替换旧画刷。
4. **修前景契约**
   - 隐式 `TextBlock` / `AccessText` 改为 `(TextElement.Foreground)` 绑定。
5. **修状态**
   - 所有 `Background` 触发器补 `Foreground`。
   - 所有选中/悬停组合补 `MultiTrigger`。
   - 所有 `Opacity` 禁用改为显式禁用色。
6. **修内容宿主**
   - 按钮、复选、单选、列表项、TabItem、ComboBoxItem、Expander 的
     `ContentPresenter` 绑定 `TextElement.Foreground`。
7. **修数据控件**
   - ComboBox 弹层、DataGrid 单元格、ListBox 选中态。
8. **修 ToolTip**
   - 深底白字或浅底深字，二者必须成对。
9. **统一字体与字号**
   - 删除所有局部 `FontFamily`，只保留 `Tokens.xaml` 的 `Font.Ui`（Microsoft YaHei）。
   - 删除所有 `FontSize="…"` 字面量，改用 `FontSize.Title/Section/Body/Caption/Code`。
   - 复选框/单选框不得小于 16×16；字号放大后同步检查控件高度（36/32）、行高（30）、表头（32）与内边距。
10. **验证**
   - 运行 `ContrastCheck.ps1`。
   - 运行 `ThemeContractTests.cs`。
   - 100%/125%/150% DPI 截图。
   - CAD 实机打开窗口。

---

## 2. 旧画刷映射

### 2.1 OpenCADTranslate

文件：`src/Shared/WindowTheme.xaml`

| 旧资源 | 新资源 |
|---|---|
| `WindowBackgroundBrush` | `Brush.WindowBackground` |
| `SurfaceBrush` | `Brush.Surface` |
| `SurfaceAltBrush` | `Brush.SurfaceAlt` |
| `BorderBrush` | `Brush.Border` |
| `BorderStrongBrush` | `Brush.Border.Strong` |
| `TextPrimaryBrush` | `Brush.Text.Primary` |
| `TextSecondaryBrush` | `Brush.Text.Secondary` |
| `TextDisabledBrush` | `Brush.Text.Disabled` |
| `AccentBrush` | `Brush.Accent` |
| `AccentHoverBrush` | `Brush.Accent.Hover` |
| `AccentPressedBrush` | `Brush.Accent.Pressed` |
| `AccentSubtleBrush` | `Brush.Accent.Subtle` |
| `DangerBrush` | `Brush.Danger` |
| `DangerHoverBrush` | `Brush.Danger.Hover` |
| `DangerSubtleBrush` | `Brush.Danger.Subtle` |
| `SuccessBrush` | `Brush.Success` |
| `SuccessSubtleBrush` | `Brush.Success.Subtle` |
| `InputBackgroundBrush` | `Brush.Surface` |
| `GridHeaderBrush` | `Brush.SurfaceAlt` |
| `RowAltBrush` | `Brush.SurfaceSunken` |
| `SelectionBrush` | `Brush.Selection` |
| `SelectionTextBrush` | `Brush.Text.Primary` |
| `FocusBorderBrush` | `Brush.Focus` |

### 2.2 EngineeringAI

文件：`src/Wisdri.EngAI.Core/UI/Themes/Colors.xaml`

| 旧资源 | 新资源 |
|---|---|
| `Brush.WindowBackground` | `Brush.WindowBackground` |
| `Brush.Panel` | `Brush.Surface` |
| `Brush.MutedPanel` | `Brush.SurfaceAlt` |
| `Brush.Border` | `Brush.Border` |
| `Brush.Border.Strong` | `Brush.Border.Strong` |
| `Brush.Text.Primary` | `Brush.Text.Primary` |
| `Brush.Text.Secondary` | `Brush.Text.Secondary` |
| `Brush.Text.Disabled` | `Brush.Text.Disabled` |
| `Brush.Accent` | `Brush.Accent` |
| `Brush.Accent.Hover` | `Brush.Accent.Hover` |
| `Brush.Accent.Pressed` | `Brush.Accent.Pressed` |
| `Brush.Accent.Subtle` | `Brush.Accent.Subtle` |
| `Brush.Danger` | `Brush.Danger` |
| `Brush.Danger.Hover` | `Brush.Danger.Hover` |
| `Brush.Danger.Subtle` | `Brush.Danger.Subtle` |
| `Brush.Control.Hover` | `Brush.Control.Hover` |
| `Brush.Control.Pressed` | `Brush.Control.Pressed` |
| `Brush.Control.Disabled` | `Brush.Control.Disabled` |
| `Brush.Code.Background` | `Brush.SurfaceSunken` |
| `Brush.ScrollThumb` | `Brush.ScrollThumb` |
| `Brush.ScrollThumb.Hover` | `Brush.ScrollThumb.Hover` |

> EngineeringAI 的 `Brush.Text.Disabled=#9CA3AF` 需要改为 `#667085`。
> `#9CA3AF` 在 `#F2F4F7` 上只有约 2.2:1。

### 2.3 auto_inventor

文件：`Src/Shared/Presentation/Themes/UnifiedTheme.xaml`

| 旧资源 | 新资源 |
|---|---|
| `AppSurfaceBrush` | `Brush.Surface` |
| `AppSurfaceMutedBrush` | `Brush.SurfaceAlt` |
| `AppTextBrush` | `Brush.Text.Primary` |
| `AppSecondaryTextBrush` | `Brush.Text.Secondary` |
| `AppMutedTextBrush` | `Brush.Text.Secondary` |
| `AppDisabledTextBrush` | `Brush.Text.Disabled` |
| `AppBorderBrush` | `Brush.Border` |
| `AppBorderStrongBrush` | `Brush.Border.Strong` |
| `AppBrandBrush` | `Brush.Accent` |
| `AppBrandHoverBrush` | `Brush.Accent.Hover` |
| `AppBrandPressedBrush` | `Brush.Accent.Pressed` |
| `AppOnBrandTextBrush` | `Brush.OnAccent` |
| `AppFocusBrush` | `Brush.Focus` |
| `AppControlHoverBrush` | `Brush.Control.Hover` |
| `AppControlPressedBrush` | `Brush.Control.Pressed` |
| `AppSelectionBrush` | `Brush.Selection` |
| `AppDisabledBackgroundBrush` | `Brush.Control.Disabled` |
| `AppDangerBrush` | `Brush.Danger` |
| `AppDangerHoverBrush` | `Brush.Danger.Hover` |
| `AppDangerPressedBrush` | `Brush.Danger.Pressed` |
| `AppDangerBackgroundBrush` | `Brush.Danger.Subtle` |
| `AppSuccessBackgroundBrush` | `Brush.Success.Subtle` |
| `AppSuccessTextBrush` | `Brush.Success` |
| `AppInfoBackgroundBrush` | `Brush.Accent.Subtle` |
| `AppInfoBorderBrush` | `Brush.Accent` |
| `AppInfoTextBrush` | `Brush.Accent.Pressed` |

---

## 3. 逐文件检查

### 3.1 全局主题

- [ ] 删除窗口级重复的 `WindowTheme.xaml` / `Styles.xaml` / `UnifiedTheme.xaml`。
- [ ] 只保留一个 `Theme/Theme.xaml` 入口。
- [ ] 合并顺序：Tokens → Controls → DataControls（Theme.xaml 已处理）。
- [ ] 确认 `pack://` 程序集名正确。

### 3.2 文本

- [ ] 隐式 `TextBlock` 不再硬编码 `Foreground`。
- [ ] 增加 `AccessText` 隐式样式。
- [ ] 业务 XAML 中没有局部 `FontFamily`。
- [ ] 业务 XAML 中没有 `#RRGGBB`。
- [ ] 按钮/列表项/TabItem 内部没有带显式前景的 `Text.*` 样式。

### 3.3 按钮

- [ ] 主按钮 Hover/Pressed 仍保持白字。
- [ ] 危险按钮 Hover 是浅红底 + 深红字。
- [ ] 禁用态使用显式颜色，不用 `Opacity`。
- [ ] 焦点边框可见。
- [ ] 一个视图只有一个 `Button.Primary`。

### 3.4 输入控件

- [ ] TextBox/PasswordBox 的 Hover/Focus/ReadOnly/Disabled 四态完整。
- [ ] ComboBox 弹层使用 `Brush.Surface` + `Brush.Text.Primary`。
- [ ] ComboBoxItem 有 Selected+Hover `MultiTrigger`。
- [ ] CheckBox/RadioButton 的 ContentPresenter 绑定前景。

### 3.5 列表/表格

- [ ] ListBox/ListView 选中使用 `Selection` + `Text.Primary`。
- [ ] 选中+悬停使用 `Selection.Hover` + `Text.Primary`。
- [ ] DataGridCell 选中背景透明，避免盖住行背景。
- [ ] 表头使用 `SurfaceAlt` + `Text.Secondary`。
- [ ] 交替行使用 `SurfaceSunken`。

### 3.6 其他

- [ ] ToolTip 背景/前景成对设置。
- [ ] TabItem 选中使用 Accent + 2px 下划线。
- [ ] Expander 头部 Hover 有浅底。
- [ ] ScrollBar 细窄，不抢视觉。
- [ ] StatusBar 顶部边框和前景正确。

---

## 4. 验证命令

```powershell
# 1. 对比度
pwsh ./tools/ContrastCheck.ps1

# 2. 主题契约测试（复制到项目测试目录后）
dotnet test

# 3. 编译
dotnet build -c Release

# 4. 启动冒烟
# 打开设置、批量、报告、术语库窗口，逐个检查状态
```

---

## 5. 回滚策略

- 迁移前先提交一个干净的 commit。
- 先在一个窗口试点（推荐设置窗口），验证通过后再推广。
- 如果主题加载失败，保留旧窗口的资源合并作为临时回退，但不要长期并存两套主题。
- 每完成一类控件（按钮/输入/列表）提交一次，便于定位问题。
