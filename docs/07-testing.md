# 07. 测试与验证

样式问题不能只靠“看起来还行”。本规范提供三层验证：

1. **对比度脚本**：自动检查颜色组合。
2. **主题契约测试**：防止关键 XAML 被改回错误写法。
3. **人工视觉矩阵**：真实窗口、真实 DPI、真实 CAD 宿主。

---

## 1. 对比度脚本

```powershell
pwsh ./tools/ContrastCheck.ps1
```

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

新增颜色时，必须在脚本的 `$Pairs` 中登记组合。

---

## 1.1 布局检查

```powershell
pwsh ./tools/LayoutCheck.ps1 -Root "D:\workspace\inventor\OpenCADTranslate"
```

检查项：

- 控件上的 `Height="*"`（只允许 `RowDefinition`/`ColumnDefinition`）。
- 多行 `TextBox`（`AcceptsReturn` / `TextBox.Multiline` / `TextBox.Code`）是否设置 `MinHeight`/`MaxHeight`。
- 表单多行输入是否误用 `VerticalAlignment="Stretch"`。
- 换行 `TextBlock` 是否误用 `Stretch`。
- 业务窗口是否嵌套 `ScrollViewer`。

默认只把“错误”作为失败；警告需要人工判断。CI 中可以使用 `-Strict` 让警告也失败。

---

## 2. 主题契约测试

`tests/ThemeContractTests.cs` 是可复制到 xUnit 项目的模板。

覆盖：

- 隐式 `TextBlock` 前景使用 `(TextElement.Foreground)` 绑定。
- 隐式 `AccessText` 前景使用同样规则。
- `Button.Primary` 是 `OnAccent` 压在 `Accent` 上。
- `Button.Primary` 禁用态设置显式 `Foreground`，且不使用 `Opacity`。
- 关键颜色组合的对比度 ≥ 4.5 / 3.0。

使用：

1. 把文件复制到项目的测试目录。
2. 确认 `StylesRoot` 能找到 `wpf-styles` 目录：
   - 默认从测试输出目录向上查找；
   - 或设置环境变量 `WPF_STYLES_ROOT`。
3. `dotnet test`。

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

AutoCAD / Inventor 的 WPF 宿主与普通 WPF 应用不同：

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
