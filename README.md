# WPF UI Skills

面向桌面工具与工程插件的高质量 WPF 视觉规范、**三档密度（紧凑 / 中等 / 宽松）**布局体系与自动化校验工具。

目标是把**设计令牌、可切换密度、状态契约（前景/背景/悬停/选中/禁用）与防截断骨架**固定为可执行的标准规范，避免出现：

- 字体过大、内边距过厚、一屏展示信息量过少；
- 主按钮蓝底深灰字；
- 浅色悬停背景上仍然是白字；
- 禁用按钮用 `Opacity=0.45`，文字几乎看不见；
- 列表选中项白字压在浅蓝底上；
- 窗口多 tab 切换导致窗口高度跳变；
- ComboBox 弹出层还是系统默认白底黑字，和主界面割裂；
- 同一个项目里 28/32 两种控件高度混用；
- 输入框文字只占左半边，右侧点击不能继续输入；
- 标签页选中后只有一条细横线，看不出当前在哪一页；
- 白底按钮的描边太浅，在浅色窗口里几乎看不见按钮边界。

---

## 0. 作为 Skill 使用

整个 `wpf-styles` 目录本身就是一个即开即用的 Skill：

- 入口：`SKILL.md`（含完整规范、紧凑布局基准与自动化校验命令）。
- 速查：`references/cheatsheet.md`。
- 代码片段：`references/xaml-snippets.md`。
- 可执行主题：`Theme/Theme.xaml`。
- 校验工具：`tools/ContrastCheck.py`、`tools/LayoutCheck.py`（附 `.ps1` 薄包装）。
- 契约测试：`tests/`（`dotnet test tests/WpfStyleContractTests.csproj`）。

一键安装：

```powershell
# 查看本机检测到哪些客户端
pwsh ./install.ps1 -Detect

# 用户级：安装到本机检测到的所有客户端
pwsh ./install.ps1 -Scope Global -Clients auto

# 用户级：安装到全部支持的客户端
pwsh ./install.ps1 -Scope Global -Clients all

# 项目级：写入目标仓库，可提交给团队
pwsh ./install.ps1 -Scope Project -Root "D:/Demo/YourApp" -Clients all

# 清理已卸载客户端里的残留安装（只删本技能写入的文件）
pwsh ./install.ps1 -Cleanup
```

`-Clients auto` 只安装到“软件确实存在”的客户端：检测 PATH 上的可执行文件、仍含
exe 的程序安装目录、Windows 卸载注册表/MSIX 安装记录，以及配置目录近 90 天内
（`-MaxAgeDays` 可调）的真实写入。卸载后残留的旧配置目录、或早期 `-Clients all`
留下的本技能文件，会被视为残留并在安装时自动清理。

手动安装（任选其一）：

```text
# 全局
%USERPROFILE%\.agents\skills\wpf-ui-style\      ← 把整个 wpf-styles 目录内容放进去

# 项目级
<repo>\.zcode\skills\wpf-ui-style\              ← 同上
```

支持的客户端、路径、参数、卸载方式见 [INSTALL.md](INSTALL.md)。

安装后，在对话里直接说：

> 按 `wpf-ui-style` 规范修改这个窗口，重点检查前景/背景、悬停、选中和多行文本高度。

Skill 会先读 `SKILL.md`，再按需打开 `references/cheatsheet.md`、
`docs/02-control-states.md`、`docs/03-layout.md` 和 `docs/04-common-pitfalls.md`。

### 0.1 配套技能：Ribbon 线性图标生成器

`skills/ribbon-icon-generator/` 是一个**独立 skill**（安装脚本不会把它套娃复制进 `wpf-ui-style`）。
它提供 24×24 单色线性图标的 40 个成品、SVG / WPF `StreamGeometry` 生成器、深浅双主题预览页，
以及安全边距 / 复杂度 / 对比度校验，专门解决 Ribbon 工具条在 16px 下的辨识度问题：

```powershell
pwsh ./skills/ribbon-icon-generator/install.ps1            # 装到本机已存在的客户端
pwsh ./skills/ribbon-icon-generator/install.ps1 -All -Force
```

```bash
python skills/ribbon-icon-generator/tools/IconGen.py list
python skills/ribbon-icon-generator/tools/IconGen.py check --strict
python skills/ribbon-icon-generator/tools/IconGen.py xaml --out samples/RibbonIcons.xaml
```

---

## 1. 文件结构

```
wpf-styles/
├─ README.md                         ← 本文件
├─ INSTALL.md                        ← 安装/配置/卸载说明
├─ install.ps1                       ← 一键安装到主流客户端
├─ SKILL.md                          ← 给 AI/编码代理使用的执行规范
├─ Theme/
│  ├─ Tokens.xaml                    ← 唯一色值、字体、尺寸兜底值、圆角、文本样式
│  ├─ Density.Compact.xaml           ← 紧凑密度（26px 控件 / 24px 行高）
│  ├─ Density.Medium.xaml            ← 中等密度（默认，30px 控件 / 28px 行高）
│  ├─ Density.Relaxed.xaml           ← 宽松密度（34px 控件 / 32px 行高）
│  ├─ Controls.xaml                  ← 按钮、输入框、下拉框、复选、分组、Tab、ToolTip…
│  ├─ DataControls.xaml              ← ListBox、ListView、DataGrid、TreeView
│  ├─ Icons.xaml                     ← 24x24 图标几何、尺寸阶梯、图标按钮
│  ├─ Extras.xaml                    ← Slider、DatePicker、Menu、ToolBar、Card、Badge…
│  ├─ Theme.xaml                     ← 默认入口（中等密度）
│  ├─ Theme.Compact.xaml             ← 整包入口（紧凑密度）
│  ├─ Theme.Relaxed.xaml             ← 整包入口（宽松密度）
│  └─ README.md                      ← 主题文件使用说明
├─ docs/
│  ├─ README.md                      ← 文档索引
│  ├─ 01-design-tokens.md            ← 设计令牌
│  ├─ 02-control-states.md           ← 控件状态矩阵（核心）
│  ├─ 03-layout.md                   ← 布局规范（含防截断与密度）
│  ├─ 04-common-pitfalls.md          ← 常见错误与修复
│  ├─ 05-accessibility-contrast.md   ← 对比度与可访问性
│  ├─ 06-density.md                  ← 三档密度说明与切换方式
│  ├─ 07-testing.md                  ← 自动化与人工验证
│  └─ 09-icon-system.md              ← 矢量图标系统与契约
├─ references/
│  ├─ cheatsheet.md                  ← 一页速查
│  └─ xaml-snippets.md               ← 可复制 XAML 片段
├─ samples/                          ← 按用途分类的可复制 XAML
│  ├─ forms/
│  │  ├─ SafeFormWindow.xaml         ← 安全表单骨架
│  │  ├─ TabsSettingsWindow.xaml     ← 标签页 + 输入占满
│  │  └─ StackedFormWindow.xaml      ← 窄面板单列表单
│  ├─ tables/EditableTableWindow.xaml
│  ├─ layouts/WorkbenchWindow.xaml
│  ├─ dialogs/WizardWindow.xaml
│  ├─ icons/IconGalleryWindow.xaml
│  └─ README.md
├─ preview/                          ← 可运行案例中心（侧边导航 + 10 个案例页 + 密度切换 + 截图）
├─ skills/
│  └─ ribbon-icon-generator/         ← 配套独立技能：24x24 Ribbon 线性图标生成器
│     ├─ SKILL.md                    ← 图标规范、工作流、WPF 集成
│     ├─ catalog/icons.json          ← 40 个图标几何（唯一数据源）
│     ├─ tools/IconGen.py            ← SVG / XAML / 预览 / 校验 / 提示词
│     └─ samples/                    ← 生成的 SVG 与 RibbonIcons.xaml
├─ tools/
│  ├─ ContrastCheck.py               ← 对比度自动检查（Python 3 标准库）
│  ├─ LayoutCheck.py                 ← 布局/防截断/TabControl 规则自动检查
│  ├─ ContrastCheck.ps1              ← 薄包装（自动探测 Python）
│  ├─ LayoutCheck.ps1                ← 薄包装（自动探测 Python）
│  └─ README.md                      ← 脚本分工与调用方式
└─ tests/
   ├─ ThemeContractTests.cs          ← xUnit 契约测试
   └─ WpfStyleContractTests.csproj   ← 可直接 dotnet test
```

---

## 2. 快速接入

### 2.1 App.xaml 合并

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary
                Source="pack://application:,,,/YourAssembly;component/Theme/Theme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

工程插件项目通常没有 `App.xaml`，在窗口构造时合并：

```csharp
public static void Apply(Window window)
{
    window.Resources.MergedDictionaries.Add(new ResourceDictionary
    {
        Source = new Uri(
            "pack://application:,,,/YourAssembly;component/Theme/Theme.xaml",
            UriKind.Absolute)
    });
}
```

> 注意：`YourAssembly` 必须是**实际包含 Theme 文件的程序集名**。
> 如果主题放在共享库 `YourApp.UI` 中，就写
> `pack://application:,,,/YourApp.UI;component/Theme/Theme.xaml`。

### 2.2 使用样式

```xml
<Button Content="保存" Style="{StaticResource Button.Primary}" />
<Button Content="取消" />
<TextBox Text="{Binding Name}" />
<GroupBox Header="翻译选项">
    <StackPanel>
        <CheckBox Content="启用术语库" />
        <ComboBox SelectedIndex="0">
            <ComboBoxItem Content="替换原文" />
            <ComboBoxItem Content="追加译文" />
        </ComboBox>
    </StackPanel>
</GroupBox>
```

- `Button`、`TextBox`、`ComboBox`、`CheckBox`、`RadioButton`、`ListBox`、`DataGrid` 等都有隐式样式，**不需要逐个指定**。
- 只有变体需要显式指定：`Button.Primary`、`Button.Danger`、`Button.Text`、`TextBox.Multiline`、`TextBox.Code`、`Button.Primary.Small`、`Button.Secondary.Large` 等。
- `TextBox` / `PasswordBox` / `ComboBox` 的内容区默认横向占满，点击右侧空白也能继续输入。
- `TabItem` 是原生方角页签：每个页签都有 1px `Brush.Border.Tab` 边框（未选中可见、可点击），选中页签去掉下边框与内容区连成一体；无圆角、无底部强调条。方框比按钮/输入框的 `Border.Control` 浅一档，整块框线不压内容。

### 2.3 选择密度（紧凑 / 中等 / 宽松）

默认是 **中等**；换密度只需更换入口文件名：

```xml
<!-- 紧凑：工程插件工具条、属性面板、大表格 -->
<ResourceDictionary Source="pack://application:,,,/YourAssembly;component/Theme/Theme.Compact.xaml" />

<!-- 中等（默认） -->
<ResourceDictionary Source="pack://application:,,,/YourAssembly;component/Theme/Theme.xaml" />

<!-- 宽松：触屏 / 演示 / 培训 -->
<ResourceDictionary Source="pack://application:,,,/YourAssembly;component/Theme/Theme.Relaxed.xaml" />
```

运行时热切换（控件全部使用 `{DynamicResource Layout.*}`，可即时生效）：

```csharp
Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
{
    Source = new Uri(
        "pack://application:,,,/YourAssembly;component/Theme/Density.Compact.xaml",
        UriKind.Absolute)
});
```

| 令牌 | 紧凑 | 中等（默认） | 宽松 |
|---|---:|---:|---:|
| `Layout.ControlHeight`（按钮/输入/下拉） | 26 | **30** | 34 |
| `Layout.ControlHeight.Small`（工具条） | 24 | **26** | 30 |
| `Layout.RowHeight`（表格行） | 24 | **28** | 32 |
| `Layout.HeaderHeight`（表头） | 26 | **30** | 34 |
| `Layout.TabPadding` | 10,4 | **12,6** | 16,8 |
| `Layout.ButtonPadding` | 10,0 | **12,0** | 16,0 |
| `Layout.WindowPadding` | 8 | **12** | 16 |
| `Layout.FormRowMargin` | 0,2 | **0,3** | 0,5 |

字号与颜色**不随密度变化**；只在 `Density.*.xaml` 中改尺寸、内边距和行高。
完整说明见 [docs/06-density.md](docs/06-density.md)。

---

## 3. 核心硬规则

1. **每个状态必须同时声明背景和前景。**
   只改 `Background` 不改 `Foreground`，是“看不清文字”的头号原因。

2. **禁止用 `Opacity` 表达禁用。**
   禁用态使用 `Brush.Control.Disabled` + `Brush.Text.Disabled`。
   透明度会让白字和浅底一起变淡，对比度直接崩掉。

3. **隐式 `TextBlock` 不得硬编码前景色。**
   必须使用“跟随最近 `ContentPresenter`”的绑定（见 `Theme/Controls.xaml`）。
   否则 `Button.Primary` 的白字会被全局样式覆盖成正文深灰。

4. **浅色背景只用深色文字，深色背景只用白色文字。**
   白字只允许出现在：
   `Accent` / `Accent.Hover` / `Accent.Pressed` /
   `Danger` / `Danger.Hover` / `Danger.Pressed` / `Tooltip.Background`。
   选中列表项、ComboBox 项、悬停项一律使用浅底深字。

5. **组合状态必须用 `MultiTrigger` 显式处理。**
   典型是“选中 + 悬停”。只写两个独立 `Trigger` 会依赖声明顺序，
   后声明的触发器覆盖前面的，容易出现浅底白字。

6. **多行文本不要默认撑满。**
   表单多行输入：`Height="Auto"` + `VerticalAlignment="Top"`，不要强制 `MinHeight`。
   只有日志/列表/表格/主内容 ScrollViewer 才放在 `Grid` 的 `*` 行。
   禁止在控件上写 `Height="*"`。

7. **不截断、不遮挡。**
   标签列 `Auto` + `MinWidth`（长标签换行，不裁）；输入列 `*` + `MinWidth`（不写死 `Width`）；
   省略号必须配 `ToolTip`；窗口用 `SizeToContent="Height"` + 一层 `ScrollViewer`；
   底部按钮栏独占一行，状态文案另起一行；禁止负 `Margin` 与 `Canvas` 绝对定位。
   详见 `SKILL.md` 的 R14、`docs/03-layout.md` §14 与 `samples/forms/SafeFormWindow.xaml`。

8. **密度只改变尺寸，不改变颜色与字号。**
   控件内部通过 `{DynamicResource Layout.*}` 读取高度、内边距与行高；
   业务 XAML 不要写死 `Height="32"` 或 `Padding="8,0"`。
   紧凑 / 中等 / 宽松三档由 `Theme/Density.*.xaml` 提供，默认中等。

---

## 4. 状态速查

| 控件/状态 | 背景 | 前景 | 边框 |
|---|---|---|---|
| 次要按钮 正常 | `Brush.Surface` | `Brush.Text.Primary` | `Brush.Border.Control` |
| 次要按钮 悬停 | `Brush.Control.Hover` | `Brush.Text.Primary` | `Brush.Accent` |
| 次要按钮 按下 | `Brush.Control.Pressed` | `Brush.Text.Primary` | `Brush.Accent.Pressed` |
| 主按钮 正常 | `Brush.Accent` | `Brush.OnAccent` | `Brush.Accent` |
| 主按钮 悬停 | `Brush.Accent.Hover` | `Brush.OnAccent` | `Brush.Accent.Hover` |
| 主按钮 禁用 | `Brush.Control.Disabled` | `Brush.Text.Disabled` | `Brush.Border` |
| 输入框 正常 | `Brush.Surface` | `Brush.Text.Primary` | `Brush.Border.Control` |
| 输入框 聚焦 | `Brush.Surface` | `Brush.Text.Primary` | `Brush.Focus` |
| 标签页 悬停 | `Brush.Control.Hover` | `Brush.Text.Primary` | `Brush.Accent` |
| 标签页 未选中 | `Brush.SurfaceAlt` | `Brush.Text.Secondary` | `Brush.Border.Tab`（四边） |
| 标签页 选中 | `Brush.Surface` | `Brush.Text.Primary`（SemiBold） | `Brush.Border.Tab`（去掉下边框） |
| 列表项 悬停 | `Brush.Control.Hover` | `Brush.Text.Primary` | 透明 |
| 列表项 选中 | `Brush.Selection` | `Brush.Text.Primary` | 透明 |
| 列表项 选中+悬停 | `Brush.Selection.Hover` | `Brush.Text.Primary` | 透明 |
| 危险按钮 悬停 | `Brush.Danger.Subtle` | `Brush.Danger.Hover` | `Brush.Danger.Hover` |
| ToolTip | `Brush.Tooltip.Background` | `Brush.Tooltip.Foreground` | 同背景 |

完整矩阵见 [docs/02-control-states.md](docs/02-control-states.md)。

---

## 5. 对比度要求

- 正文文字：≥ **4.5:1**（WCAG AA）。
- 大号文字（≥ 18.66px 粗体或 ≥ 24px）：≥ **3:1**。
- 焦点边框、图标、输入框/按钮边框等非文字元素：≥ **3:1**。
  按钮与输入框描边使用 `Brush.Border.Control`（对白底 3.52:1、对窗口底 3.10:1）。
- 禁用文字：WCAG 不强制，但本规范要求 ≥ **3:1**，建议 ≥ 4.5:1。
- 白字压品牌色：本规范所有 Accent/Danger 组合均 ≥ 5.3:1。

运行（`.ps1` 是薄包装，会自动探测 `py -3` / `python3` / `python`）：

```powershell
pwsh ./tools/ContrastCheck.ps1
pwsh ./tools/LayoutCheck.ps1 -Root "D:/Demo/YourApp"
```

也可以直接调用 Python（CI / Linux / macOS 推荐）：

```bash
python tools/ContrastCheck.py
python tools/LayoutCheck.py --root . --strict
```

`LayoutCheck` 会检查多行文本撑满、控件 `Height="*"`、换行文本 `Stretch`、
嵌套 `ScrollViewer`、`TabControl` 缺固定高度等布局反模式。脚本分工见 [tools/README.md](tools/README.md)。

完整色值与实测比值见 [docs/05-accessibility-contrast.md](docs/05-accessibility-contrast.md)，
布局约束见 [docs/03-layout.md](docs/03-layout.md)。

---

## 6. 兼容性

- 适用于 .NET Framework 4.8 与 .NET 8+ 的 WPF。
- 不使用第三方控件库、动画库或 Windows 11 专属 API。
- 主题文件只依赖 WPF 基础控件，工程插件可直接链接。
- 推荐 `TextOptions.TextFormattingMode="Display"`（已在 `Window` 样式中设置），
  小字号更清晰；不要在子元素上改成 `Ideal`。
- 高 DPI 下保持 `UseLayoutRounding="True"`、`SnapsToDevicePixels="True"`，
  避免 1px 边框发虚。

---

## 7. 不建议做的事

- 不要在每个窗口里复制一套颜色和按钮模板。
- 不要直接写 `#FF...`，一律引用 `Brush.*`。
- 不要给 `TextBlock` 设置局部 `Foreground` 去“修”颜色；
  先确认它所在的 `ContentPresenter` / 控件前景是否正确。
- 不要用 `Opacity` 做禁用、悬停或选中。
- 不要在浅色悬停背景上保留白字。
- 未明确要求时不要滥用图标；默认纯文本更清晰整洁。

---

## 8. 使用说明

安装后，在支持 skill 的客户端里直接描述任务即可，例如：

```text
按 wpf-ui-style 规范检查这个 WPF 窗口
把 SettingsWindow 迁移到 wpf-ui-style，修前景/背景、悬停、选中和禁用态
检查这个 XAML 的多行文本高度，避免 TextBox 撑满 Grid 的 * 行
用 wpf-ui-style 的 Token 重做这个对话框的配色和间距
```

Skill 会按以下顺序工作：

1. 读取 `SKILL.md` 的硬规则。
2. 按需打开 `references/cheatsheet.md`、`docs/02-control-states.md`、
   `docs/03-layout.md`、`docs/04-common-pitfalls.md`。
3. 用 `Theme/Theme.xaml` 替换局部样式。
4. 运行 `tools/ContrastCheck.ps1` 和 `tools/LayoutCheck.ps1`。
5. 用 `dotnet test tests/WpfStyleContractTests.csproj` 做回归检查。

如果客户端只支持规则文件（Windsurf、Cline、Roo、Kilo、Continue、JetBrains 等），
安装器会写入精简规则并指向已安装的 `SKILL.md`；如果只选了规则型客户端，
还会自动补装一份通用 `agents` skill，保证路径有效。

完整安装参数、客户端路径、卸载和排错见 [INSTALL.md](INSTALL.md)。

---

## 9. 相关文档

- [文档索引](docs/README.md)
- [设计令牌](docs/01-design-tokens.md)
- [控件状态矩阵](docs/02-control-states.md)
- [布局规范](docs/03-layout.md)
- [三档密度](docs/06-density.md)
- [常见错误](docs/04-common-pitfalls.md)
- [对比度与可访问性](docs/05-accessibility-contrast.md)
- [测试与验证](docs/07-testing.md)
- [矢量图标系统](docs/09-icon-system.md)
- [速查表](references/cheatsheet.md)
- [可复制 XAML 片段](references/xaml-snippets.md)
