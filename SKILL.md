---
name: wpf-ui-style
description: Apply and audit the WPF UI Skills style system (tokens, control states, layout, contrast). Use when creating or modifying WPF/XAML windows, styles, resource dictionaries, themes, buttons, inputs, lists, data grids, dialogs, or when fixing unreadable text caused by foreground/background/hover/selected/disabled conflicts. 也适用于中文场景：WPF 样式规范、布局规范、前景与背景冲突、悬停背景看不清、选中项白字、禁用按钮不可读、多行文本框撑满布局、对比度检查。
---

# WPF UI Skills

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
- `tools/ContrastCheck.py` — 对比度检查逻辑（Python 3 标准库）。
- `tools/LayoutCheck.py` — 多行文本/布局反模式检查逻辑（Python 3 标准库）。
- `tools/ContrastCheck.ps1` / `tools/LayoutCheck.ps1` — 薄包装，自动探测 `py -3` / `python3` / `python` 后转调 `.py`。
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
- Never a local `FontFamily`; the Window style provides `Font.Ui` (Microsoft YaHei).
- Never a literal `FontSize="13"` in a window; use `FontSize.Body` / `Caption` / `Title`.

### R7. One control height

Use `Layout.ControlHeight` (36) or `Layout.ControlHeight.Small` (32).
Do not mix 30 / 32 / 36 in the same dialog.
Row height is `Layout.RowHeight` (30); table header is `Layout.HeaderHeight` (32).

### R8. One primary action per view

Only one `Button.Primary` per dialog/section. Everything else is
`Button.Secondary`, `Button.Text`, or `Button.Danger`.


### R9. Multiline text adaptability & no forced MinHeight

- **No forced global MinHeight**: `TextBox.Multiline` defaults to `MinHeight="0"`, `Height="Auto"`. Never impose hardcoded min-height in theme styles that causes window overflow or scrollbar fighting.
- **Form multiline input**: place in `Grid` `Height="Auto"` rows with `VerticalAlignment="Top"`. Let content or view dictate height.
- **Dedicated comparison / log / content panels**: place in dedicated `Grid` `Height="*"` rows with `VerticalAlignment="Stretch"` so it naturally fills available viewport space without clipping.
- **Never use `Height="*"` on a control**: only `RowDefinition`/`ColumnDefinition` can take `*`.


### R10. Icon contracts: Google Material Symbols & dynamic foreground

- **Official Material Symbols standard**: all icons are derived from Google Material Symbols (Apache 2.0 license, commercially free), designed on a standard 24x24 optical viewport (`StreamGeometry Icon.Geo.*`).
- **Balanced weight**: use **Outlined / Weight 400 / Grade 0 / Optical Size 24**. Never mix Filled with Outlined, and never use 200 (too thin, breaks at 16px) or 700 (too heavy, fights the text).
- **Uniform optical size**: every geometry ends with the zero-area anchor `M0,0 L24,24` so `Stretch="Uniform"` always maps the full 24x24 viewport. Without it, chevrons get blown up to the size of a gear.
- **Line-control strokes**: checkbox tick 1.6px, dropdown/expander chevrons 1.5px, round caps and joins. Keep them visually equal to icon weight.
- **No bitmap/PNG icons**: strictly vector geometry to avoid DPI blur across 100%–200% displays.
- **Dynamic foreground inheritance**: icon `Fill` binds to `(TextElement.Foreground)` with `Stretch="Uniform"`. Never hardcode `Fill="#..."`.
- **Icon size steps**: only use `Icon.Size.Micro` (12), `Icon.Size.Small` (14), `Icon.Size.Regular` (16 - default), `Icon.Size.Medium` (20), `Icon.Size.Large` (24), `Icon.Size.Hero` (32).
- **Icon buttons**: use `Button.Icon` (30x30), `Button.Icon.Small` (26x26), or `Button.Icon.Large` (36x36).
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

### R13. Unified typography & hit-target sizes

- **Single font stack**: `Font.Ui` is defined once in `Tokens.xaml` as `Microsoft YaHei` (fallbacks YaHei UI / Segoe UI). Never declare a different `FontFamily` anywhere in business XAML.
- **Global font scale**: Title/Section `15.5`, Body `14.5`, Caption `14`, Code `13.5`; line heights `23` / `22`. Do not override per-window.
- **Checkbox / RadioButton**: must render at least `Layout.CheckBox.Size` / `Layout.RadioButton.Size` (16×16). Never shrink to 12–14px; check glyph uses round caps and ≥1.8px stroke.
- **When font size changes, re-check**: control heights, row heights, tab/menu padding, tooltip padding, and DataGrid cell padding in the same pass.





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
