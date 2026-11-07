# 07. 测试与验证

样式问题不能只靠“看起来还行”。本规范提供三层验证：

1. **对比度脚本**：自动检查颜色组合。
2. **主题契约测试**：防止关键 XAML 被改回错误写法。
3. **人工视觉矩阵**：真实窗口、真实 DPI、真实 CAD 宿主。

---

## 1. 对比度脚本

```powershell
pwsh ./tools/ContrastCheck.ps1
# 或直接运行 Python（CI / Linux / macOS）
python tools/ContrastCheck.py
```

`.ps1` 只是薄包装，会自动探测 `py -3` → `python3` → `python`；逻辑在 `ContrastCheck.py`（仅标准库）。

输出示例：

```text
Pair                                      Ratio   Min   Result
Text.Primary / Surface                    14.24   4.5   PASS
OnAccent / Accent                          5.39   4.5   PASS
Text.Disabled / Control.Disabled           4.51   3.0   PASS
Focus / Surface                            5.20   3.0   PASS
...
All contrast checks passed.
```

退出码：

- `0`：全部通过。
- `1`：有组合低于最低要求。
- `2`：仅 `.ps1` 包装——本机未找到 Python 3。

新增颜色时，必须在 `tools/ContrastCheck.py` 的 `PAIRS` 中登记组合。


---

## 1.1 布局检查

```powershell
pwsh ./tools/LayoutCheck.ps1 -Root "D:\Projects\YourApp"
# 或直接运行 Python
python tools/LayoutCheck.py --root . --strict
```

逻辑在 `LayoutCheck.py`（仅标准库），`.ps1` 同样只是薄包装。

检查项：

- 控件上的 `Height="*"`（只允许 `RowDefinition`/`ColumnDefinition`）。
- 只读日志/代码 `TextBox` 是否设置 `MaxHeight`（防止无限撑大）。
- 换行 `TextBlock` 是否误用 `Stretch`。
- 业务窗口是否嵌套 `ScrollViewer`。
- 多行 `TextBox` 不再强制要求 `MinHeight`（全局硬限制会导致小窗口被撑爆）。
- `TabControl` 是否缺少固定高度（会导致切换 tab 时窗口高度跳变）。

截断 / 遮挡检查（R14）：

- **错误**
  - 负 `Margin`（相邻控件重叠）。
  - `Canvas` 承载表单控件（绝对定位）。
  - 省略号截断（`TextTrimming` / `TextOverflow` 含 `Ellipsis`）却没有 `ToolTip`。
  - 换行 `TextBlock` 写死 `Height`（文字被纵向裁切）。
- **警告**
  - 标签列使用固定像素宽度（长标签被裁）。
  - `TextBox` 写死 `Width`（长值被截断）。
  - `Window` 写死 `Height` 且没有 `SizeToContent="Height"`。
  - `*` 行里放 `StackPanel` 但整窗没有 `ScrollViewer`（内容会被裁）。
  - `TabControl` 没有固定 `Height`（切换 tab 时窗口高度跳变）；
    在 `SizeToContent="Height"` 的窗口里这是**错误**。

默认只把“错误”作为失败；警告需要人工判断。CI 中可以使用 `-Strict`（或 `--strict`）让警告也失败。

---

## 1.2 UI 自动化渲染、点击交互测试与视觉自检 (UiTest)

为 AI Agent 与自动化流水线提供**真实控件点击、状态机断言、分步快照与外部 XAML 动态渲染**能力：

```powershell
# 1. 运行核心复杂界面的交互点击测试（模拟全选/批量审批/步骤机前进后退/输入回写）
pwsh ./tools/UiTest.ps1 -Window editable-table -TestClicks

# 2. 执行交互点击并保存每一步的对比状态快照（Before/After 对比图片）
pwsh ./tools/UiTest.ps1 -Window wizard -TestClicks -ShotDir shots/wizard

# 3. 动态加载渲染外部任意 XAML 界面（无需编译整个业务工程）并执行布局静态自检
pwsh ./tools/UiTest.ps1 -Xaml "D:\Projects\YourApp\MainWindow.xaml" -Shot "shots\preview.png"

# 4. 批量运行所有核心窗口的交互与点击测试，输出结构化 JSON 报表供 CI/Agent 自动化消费
pwsh ./tools/UiTest.ps1 -All -TestClicks -Json
```

支持特性：
- **模拟真实点击与联动**：自动触发 `ButtonBase.ClickEvent`、`Command`、`ToggleButton`、`ComboBox` 下拉项选择；
- **状态机断言**：断言点击后文字更新（如 `已选中 8 项`）、按钮启用禁用状态（`IsEnabled`）、行数变化、向导步骤推进；
- **无障碍与热区自检**：自动扫描整个可视树，检查可交互按钮尺寸（>=20px）、检查省略文本 `ToolTip` 配套；
- **动态解析渲染**：支持向 `-Xaml` 传入任意未编译的 XAML 窗口或用户控件，自动注入设计系统资源并导出高清截图。


---

## 2. 主题契约测试

`tests/` 是一个可直接运行的 xUnit 工程（`WpfStyleContractTests.csproj`）。
在仓库根目录执行 `dotnet test tests/WpfStyleContractTests.csproj` 即可；
也可以把 `ThemeContractTests.cs` 复制到业务项目的测试目录。

覆盖：

- 隐式 `TextBlock` 前景使用 `(TextElement.Foreground)` 绑定。
- 隐式 `AccessText` 前景使用同样规则。
- `Button.Primary` 是 `OnAccent` 压在 `Accent` 上。
- `Button.Primary` 禁用态设置显式 `Foreground`，且不使用 `Opacity`。
- 关键颜色组合的对比度 ≥ 4.5 / 3.0。

使用：

1. 在仓库根目录运行 `dotnet test tests/WpfStyleContractTests.csproj`。
2. 如果复制到业务项目，确认 `StylesRoot` 能找到 `wpf-styles` 目录：
   - 默认从测试输出目录向上查找；
   - 或设置环境变量 `WPF_STYLES_ROOT`。
3. CI 中执行同样的命令。

> 建议把契约测试放在 CI 中。任何把 `TextBlock.Foreground` 改回静态色、
> 把禁用态改回 `Opacity`、或把 `Button.Primary` 改成非白字的提交都会失败。

---

## 3. 人工视觉矩阵

### 3.1 每个控件至少检查

| 控件 | 状态 |
|---|---|
| Button.Secondary | Normal / Hover / Pressed / Focused / Disabled |
| Button.Primary | Normal / Hover / Pressed / Focused / Disabled |
| Button.Danger | Normal / Hover / Pressed / Focused / Disabled |
| Button.Text | Normal / Hover / Pressed / Disabled |
| TextBox | Normal / Hover / Focused / ReadOnly / Disabled |
| PasswordBox | Normal / Focused / Disabled |
| ComboBox | Normal / Hover / Open / Disabled |
| ComboBoxItem | Normal / Hover / Selected / Selected+Hover / Disabled |
| CheckBox | Unchecked / Checked / Hover / Focused / Disabled |
| RadioButton | Unchecked / Checked / Hover / Focused / Disabled |
| ListBoxItem | Normal / Hover / Selected / Selected+Hover / Disabled |
| DataGridRow | Normal / Hover / Selected / Selected+Hover / Disabled |
| TabItem | Normal / Hover / Selected / Disabled |
| Expander | Collapsed / Expanded / Hover / Disabled |
| ToolTip | 显示 |
| ProgressBar | 0% / 50% / 100% |
| StatusBar | Normal |

### 3.2 DPI

- 100%
- 125%
- 150%
- 200%（如果支持）

检查：

- 1px 边框是否发虚；
- 文字是否被裁切；
- ComboBox 弹层是否对齐；
- 表格行高是否够；
- 按钮最小宽度是否够。

### 3.2.0 交互态自检（R15）

`preview` 工程里的 `ProbeWindow` 会进入编辑态、展开长内容、打开日历弹层，
离屏渲染成 PNG，并在控制台打印自检结果：

```bash
dotnet run --project preview -- -shot out/probe.png -window probe
```

看到这些输出即表示交互态正常：

```
DATEPOPUP child=System.Windows.Controls.Calendar   # 日历弹层有内容
DATEPICKER selected=2026-12-01                      # 选日期能回写
ROW h=32.0                                          # 行高容得下编辑器
```

人工再确认：编辑态文字居中、折叠面板内容区有滚动条、滚动条滑块是竖的。

### 3.2.1 截断与遮挡专项（R14）

在 100% / 150% DPI 和窗口 `MinWidth` 三种条件下各看一遍：

- [ ] 最长的表单标签没有被裁掉一半（标签列是 `Auto` + `MinWidth`）。
- [ ] 最长的输入值要么完整可见，要么有 `ToolTip` / 可横向滚动。
- [ ] 底部按钮栏没有被内容顶出窗口，也没有被状态文案压变形。
- [ ] 表格列里的长值没有被截断（`*` + `MinWidth` + `ToolTip`）。
- [ ] 省略号文本都能通过悬停看到全文。
- [ ] 内容超出时出现滚动条，而不是被直接裁掉。

### 3.3 语言

- 中文
- 英文
- 长英文（德语/俄语）用于压力测试

检查：

- 按钮文字是否被裁切；
- 表单标签是否换行；
- 表格列是否撑破；
- 对话框是否出现横向滚动。

---

## 4. CAD 宿主冒烟

工程插件与宿主窗口下的 WPF 与普通独立桌面应用不同：

- 可能没有 `App.xaml`；
- 主题要在窗口构造时合并；
- 宿主的 DPI/主题可能影响字体和弹层；
- 模态/非模态窗口的 Owner 行为不同。

检查清单：

- [ ] 设置窗口能打开、关闭、再次打开。
- [ ] 批量窗口的列表、日志、进度条正常。
- [ ] 报告窗口的表格选中/悬停正常。
- [ ] 术语库窗口的 DataGrid 编辑正常。
- [ ] ComboBox 弹层在 CAD 窗口上方正确显示。
- [ ] ToolTip 在 CAD 深色主题下仍可读。
- [ ] 高 DPI 下窗口没有裁切。
- [ ] 关闭窗口后没有资源泄漏/异常。

---

## 5. 截图建议

每个窗口至少保存：

1. 默认状态；
2. 鼠标悬停在主按钮；
3. 主按钮禁用；
4. 列表项选中；
5. 列表项选中+悬停；
6. 输入框聚焦；
7. ComboBox 展开；
8. ToolTip 显示。

截图用于 PR 评审和回归对比。

---

## 6. 失败处理

| 失败 | 处理 |
|---|---|
| 对比度脚本失败 | 调整前景或背景令牌；不要降低阈值 |
| 契约测试失败 | 检查是否把前景改回静态色/Opacity |
| 窗口打不开 | 检查 pack URI、合并顺序、资源键 |
| 某些控件没样式 | 检查隐式样式是否被局部样式覆盖 |
| 高 DPI 裁切 | 去掉固定 Height/Width，使用 MinHeight + `*` |
| ComboBox 弹层错位 | 检查 `Popup` 的 `Placement` 和 `MinWidth` 绑定 |
| 文字发虚 | 检查 `TextFormattingMode` 和局部 FontFamily |
