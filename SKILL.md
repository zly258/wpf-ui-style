---
name: wpf-ui-style
description: Apply and audit the OpenCAD WPF UI style system (tokens, control states, layout, contrast). Use when creating or modifying WPF/XAML windows, styles, resource dictionaries, themes, buttons, inputs, lists, data grids, dialogs, or when fixing unreadable text caused by foreground/background/hover/selected/disabled conflicts. 也适用于中文场景：WPF 样式规范、布局规范、前景与背景冲突、悬停背景看不清、选中项白字、禁用按钮不可读、多行文本框撑满布局、对比度检查。
---

# WPF UI Style Skill

## When to use

Use this skill whenever you touch WPF/XAML in these projects:

- `OpenCADTranslate`
- `EngineeringAI`
- `auto_inventor`
- any new AutoCAD / Inventor WPF plugin that should share the same look

Use it especially when:

- adding a new window, dialog, panel, toolbar or settings page;
- changing colors, fonts, spacing, control templates or resource dictionaries;
- fixing “文字看不清”“蓝底深灰字”“悬停后字没了”“禁用按钮几乎不可见”；
- reviewing a PR that contains XAML.

## Install

```powershell
# detect clients on this machine
pwsh ./install.ps1 -Detect

# install for the current user (detected clients)
pwsh ./install.ps1 -Scope Global -Clients auto

# install for every supported client
pwsh ./install.ps1 -Scope Global -Clients all

# install into a repository (shareable with the team)
pwsh ./install.ps1 -Scope Project -Root "D:\path\to\repo" -Clients all
```

See `INSTALL.md` for the full client table, parameters, verification and uninstall steps.

## Skill assets

- `INSTALL.md` — 安装/配置/卸载说明，含各客户端路径。
- `install.ps1` — 一键安装到 ZCode、Cursor、Copilot、Claude、WorkBuddy、Trae、Codex、DSH 等。
- `references/cheatsheet.md` — 一页速查：硬规则、状态矩阵、布局约束。
- `references/xaml-snippets.md` — 可直接复制的正确 XAML 片段。
- `Theme/Tokens.xaml` — 颜色、字体、尺寸、圆角。
- `Theme/Controls.xaml` / `Theme/DataControls.xaml` — 控件样式。
- `Theme/Icons.xaml` — 24x24 矢量几何、尺寸与状态样式、图标按钮。
- `docs/09-icon-system.md` — 图标设计与使用规范。
- `tools/ContrastCheck.ps1` — 对比度检查。
- `tools/LayoutCheck.ps1` — 多行文本/布局反模式检查。
- `tests/ThemeContractTests.cs` — xUnit 契约测试模板。

## Source of truth

Read these files before writing XAML:

1. `Theme/Tokens.xaml` — colors, brushes, typography, spacing, radii.
2. `Theme/Controls.xaml` — buttons, inputs, ComboBox, CheckBox, RadioButton, GroupBox, Expander, Tab, ToolTip, ProgressBar, ScrollBar.
3. `Theme/DataControls.xaml` — ListBox, ListView, DataGrid, TreeView.
4. `Theme/Icons.xaml` — 24x24 矢量几何图形、尺寸阶梯、图标按钮。
5. `docs/02-control-states.md` — state matrix.
6. `docs/03-layout.md` — layout and multiline height constraints.
7. `docs/04-common-pitfalls.md` — known failure modes.
8. `docs/09-icon-system.md` — 图标系统尺寸与着色契约。

Do not invent a new palette or a new control template if an existing style fits.
Extend `Tokens.xaml` or the relevant theme file instead.

## Mandatory rules

### R1. Every state declares both background and foreground

For every `Trigger` / `MultiTrigger` that changes `Background`, also set
`Foreground` (and `BorderBrush` if the border is visible).

```xml
<Trigger Property="IsMouseOver" Value="True">
    <Setter Property="Background" Value="{StaticResource Brush.Control.Hover}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Primary}" />
</Trigger>
```

### R2. Never use opacity for disabled

```xml
<!-- Wrong -->
<Trigger Property="IsEnabled" Value="False">
    <Setter Property="Opacity" Value="0.45" />
</Trigger>

<!-- Correct -->
<Trigger Property="IsEnabled" Value="False">
    <Setter Property="Background" Value="{StaticResource Brush.Control.Disabled}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Disabled}" />
    <Setter Property="BorderBrush" Value="{StaticResource Brush.Border}" />
</Trigger>
```

### R3. The implicit TextBlock foreground must follow the content host

The global `TextBlock` / `AccessText` styles must bind to
`(TextElement.Foreground)` of the nearest `ContentPresenter` with a fallback
to `Brush.Text.Primary`. Never hardcode `Brush.Text.Primary` in the implicit
style. Otherwise `Button.Primary` becomes blue background + dark gray text.

### R4. White text only on dark surfaces

Allowed white-text surfaces:

- `Brush.Accent`, `Brush.Accent.Hover`, `Brush.Accent.Pressed`
- `Brush.Danger`, `Brush.Danger.Hover`, `Brush.Danger.Pressed`
- `Brush.Tooltip.Background`

Forbidden:

- white text on `Brush.Selection` / `Brush.Selection.Hover`
- white text on `Brush.Control.Hover` / `Brush.Control.Pressed`
- white text on `Brush.Accent.Subtle` / `Brush.Accent.SubtleHover`
- white text on `Brush.Surface` / `Brush.SurfaceAlt`

### R5. Handle selected + hover explicitly

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

### R6. Use tokens, not literals

- Business XAML: only `Brush.*`, `FontSize.*`, `Layout.*`, `Radius.*`.
- Never `#FF...` in a window.
- Never a local `FontFamily`; the Window style provides `Font.Ui`.

### R7. One control height

Use `Layout.ControlHeight` (34) or `Layout.ControlHeight.Small` (30).
Do not mix 26 / 28 / 34 in the same dialog.

### R8. One primary action per view

Only one `Button.Primary` per dialog/section. Everything else is
`Button.Secondary`, `Button.Text`, or `Button.Danger`.

### R9. Multiline text must not stretch by default

A multiline `TextBox` defaults to `VerticalAlignment="Stretch"`. Inside a
`Grid` row with `Height="*"` it fills the whole row and breaks the layout.
Inside a `StackPanel` the stretch has no effect, but a large `MinHeight`
still makes the form too tall.

Rules:

- Form multiline input:
  `Height="Auto"`, `MinHeight` 80–110, `MaxHeight` 140–180,
  `VerticalAlignment="Top"`, `VerticalContentAlignment="Top"`.
- Read-only log/code box:
  place it in a dedicated `Grid` row `Height="*"`;
  `MinHeight` 120–240; `VerticalAlignment="Stretch"` is allowed there.
- Wrapped `TextBlock`: `VerticalAlignment="Top"`; no fixed `Height`.
- Never use `Height="*"` on a control.
- Only list/table/log/ScrollViewer should occupy a `*` row.

```xml
<!-- form input -->
<TextBox Style="{StaticResource TextBox.Multiline}"
         Height="Auto" MinHeight="90" MaxHeight="160"
         VerticalAlignment="Top" />

<!-- log -->
<TextBox Grid.Row="1" Style="{StaticResource TextBox.Code}"
         MinHeight="120" MaxHeight="240"
         VerticalAlignment="Stretch" />
```

### R10. Icon contracts: Google Material Symbols & dynamic foreground

- **Official Material Symbols standard**: all icons are derived from Google Material Symbols (Apache 2.0 license, commercially free), designed on a standard 24x24 optical viewport (`StreamGeometry Icon.Geo.*`).
- **No bitmap/PNG icons**: strictly vector geometry to avoid DPI blur across 100%–200% displays.
- **Dynamic foreground inheritance**: icon `Fill` binds to `(TextElement.Foreground)` with `Stretch="Uniform"`. Never hardcode `Fill="#..."`.
- **Icon size steps**: only use `Icon.Size.Micro` (12), `Icon.Size.Small` (14), `Icon.Size.Regular` (16 - default), `Icon.Size.Medium` (20), `Icon.Size.Large` (24), `Icon.Size.Hero` (32).
- **Icon buttons**: use `Button.Icon` (28x28), `Button.Icon.Small` (24x24), or `Button.Icon.Large` (34x34).
- **ToolTip required**: all icon-only buttons must provide a descriptive `ToolTip`.
- **Icon-text gap**: use 6px horizontal gap between icon and text (`Margin="0,0,6,0"`).


### R11. Layout hierarchy & toolbar rules

- **No nested borders (box-in-a-box)**: avoid wrapping a `TextBox` inside a heavily padded `Border`. Use `GroupBox` or let `TextBox Style="{StaticResource TextBox.Multiline}"` stretch naturally.
- **Action proximity**: bind actions close to their subjects. In translation/comparison views, keep target controls (e.g. language selection + translate button) between panels or aligned with flow, and place result tools (e.g. copy) in the result header.
- **Toolbar hierarchy**:
  - One primary action (`Button.Primary` + icon).
  - Destructive actions must use `Button.Danger` and be separated by at least 12–16px.
  - Search box should feature an icon prefix (`Icon.Geo.Search`) and clear action (`Button.Icon.Small`).

### R12. Padding bounds & text clipping prevention

- **Button horizontal padding**: `Layout.ButtonPadding` must not exceed `10,0` (compact `8,0`). Never use 14px+ on buttons, which squeezes multilingual labels and triggers truncation.
- **Single-line input vertical padding**: single-line `TextBox` / `PasswordBox` must use `Layout.InputPadding` (`6,0`) with `VerticalContentAlignment="Center"`. Avoid top/bottom padding to prevent clipping descenders (`g`, `j`, `y`).
- **ComboBox padding**: use `6,0,22,0` to leave room for the dropdown arrow without squashing the selected text.
- **Multiline padding**: keep multiline `TextBox` padding at `6,6` (`Layout.InputPadding.Multiline`) for clean text flow.




## Workflow

1. **Inventory**
   - List the XAML files and resource dictionaries involved.
   - Find local color literals, local fonts, local control heights, and
     `Opacity` triggers.

2. **Map**
   - Replace literals with `Brush.*` / `FontSize.*` / `Layout.*`.
   - Remove duplicated button/input templates; use the theme styles.

3. **Fix states**
   - For each interactive control, enumerate:
     normal, hover, pressed, focused, selected, selected+hover, disabled,
     read-only, error.
   - Check the matrix in `docs/02-control-states.md`.
   - Add `Foreground` to every background-changing trigger.

4. **Fix content hosts**
   - `ContentPresenter` in buttons/check boxes/list items must set
     `TextElement.Foreground="{TemplateBinding Foreground}"`.
   - `ContentPresenter` with `RecognizesAccessKey="True"` generates
     `AccessText`; the implicit `AccessText` style must follow the same rule.

5. **Check contrast**
   - Run `pwsh ./tools/ContrastCheck.ps1`.
   - Copy/run `tests/ThemeContractTests.cs` in the project test suite.
   - Manually check the 100% / 125% / 150% DPI screenshots.

6. **Check layout**
   - Run `pwsh ./tools/LayoutCheck.ps1 -Root <project>`.
   - Fix all errors (controls with `Height="*"`, multiline inputs without
     `MinHeight`/`MaxHeight`).
   - Review warnings for wrapped text and nested scroll viewers.

7. **Report**
   - List the files changed.
   - List the state pairs checked.
   - List the layout checks performed.
   - List any intentional deviations and why.

## Review checklist

- [ ] No `#RRGGBB` / `#AARRGGBB` in business XAML.
- [ ] No local `FontFamily`.
- [ ] No `Opacity` used for disabled/hover/selected.
- [ ] Every `Background` trigger has a matching `Foreground`.
- [ ] Selected + hover uses `MultiTrigger`.
- [ ] White text only on Accent/Danger/Tooltip backgrounds.
- [ ] `TextBlock` / `AccessText` implicit styles use `(TextElement.Foreground)`.
- [ ] `ContentPresenter` in stateful controls binds `TextElement.Foreground`.
- [ ] ComboBox popup uses `Brush.Surface` + `Brush.Text.Primary`.
- [ ] List/DataGrid selected row uses `Brush.Selection` + `Brush.Text.Primary`.
- [ ] ToolTip uses dark background + white text (or the project’s chosen pair).
- [ ] Disabled text contrast ≥ 3:1.
- [ ] Focus indicator contrast ≥ 3:1.
- [ ] Control heights use `Layout.ControlHeight*`.
- [ ] Multiline `TextBox` uses `Height="Auto"` + `MinHeight`/`MaxHeight` + `VerticalAlignment="Top"`.
- [ ] Only list/table/log/ScrollViewer occupies a `Grid` `*` row.
- [ ] No `Height="*"` on a control.
- [ ] `ContrastCheck.ps1` passes.
- [ ] `LayoutCheck.ps1` has no errors.
- [ ] Theme contract tests pass.
- [ ] Window opens in a real WPF smoke test.

## Common failure → fix

| Symptom | Cause | Fix |
|---|---|---|
| Primary button text is dark gray | implicit `TextBlock` hardcodes foreground | bind to `(TextElement.Foreground)`; set `TextElement.Foreground` on ContentPresenter |
| Hover text disappears | hover changes background only | set `Foreground` in the same trigger |
| Selected item is white on light blue | selected sets white text; hover sets light background | selected uses `Selection` + `Text.Primary`; selected+hover uses `Selection.Hover` + `Text.Primary` |
| Disabled button unreadable | `Opacity=0.45` | explicit `Control.Disabled` + `Text.Disabled` |
| ComboBox popup looks foreign | default system template | use `ComboBox.Base` popup template |
| ToolTip black on black | default/system colors | set both `Background` and `Foreground` |
| CheckBox disabled text stays dark | ContentPresenter doesn’t bind foreground | `TextElement.Foreground="{TemplateBinding Foreground}"` |
| DataGrid selected cell is blue with dark text | cell selection overrides row | cell `IsSelected` → transparent background + `Text.Primary` |
| Multiline TextBox fills the whole window | default `Stretch` in a `*` row | `Height="Auto"` + `MinHeight`/`MaxHeight` + `VerticalAlignment="Top"` |
| Form becomes too tall after adding a text area | large `MinHeight` inside `StackPanel` | `MaxHeight` + dedicated `*` row for logs only |
| Theme works in one window, not another | merged dictionary order / assembly name | merge `Theme/Theme.xaml` once at app or window scope; verify pack URI |

## Definition of done

- Theme files compile in a WPF project.
- The window opens without XAML load exceptions.
- `ContrastCheck.ps1` exits 0.
- `LayoutCheck.ps1` reports no errors.
- Theme contract tests pass.
- Screenshots at 100% / 125% / 150% show readable text in:
  normal, hover, pressed, selected, selected+hover, disabled, read-only.
- Resizing the window does not make form text areas grow without bound.
