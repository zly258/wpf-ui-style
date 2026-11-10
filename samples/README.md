# 示例与布局模板索引

工程提供六个核心业务与布局模板，全部遵循紧凑排版、无截断、状态契约原则，开箱即用：

1. `EditableTableWindow.xaml`：**复杂可编辑数据表格模板**（全选联动、可编辑单元格、下拉单元格、复选框单元格、状态徽章、展开详情行、分页与汇总统计）。
2. `WorkbenchWindow.xaml`：**工程工作台三栏布局模板**（装配特征树 + 3D/2D 视口卡片 + 属性检查器 Property Grid + 底部状态条）。
3. `WizardWindow.xaml`：**分步向导对话框模板**（顶部步骤条指示器 + 动态步进单层容器 + 流程控制栏）。
4. `SampleWindow.xaml`：完整的业务设置与批量处理窗口示例。
5. `SafeFormWindow.xaml`：**安全表单骨架**（防截断 / 防遮挡，规则 R11/R14）。
6. `IconGalleryWindow.xaml`：**图标系统全场景测试与矩阵展示窗口**。

---

## 1. EditableTableWindow.xaml (复杂可编辑数据表格)

演示工业与企业软件中极高频的复杂表格体系：
- **表头三态联动**：表头 `CheckBox` 具备全选 / 反选 / 部分选中三态，与所有行复选框双向绑定联动。
- **可编辑文本单元格**：`DataGridTextColumn` 双击就地编辑，聚焦蓝框高亮，Tab 键平滑切换，文字垂直居中不溢出。
- **下拉单元格 (ComboBox Column)**：`DataGridComboBoxColumn` 材质/状态下拉切换，无多余边距，完美适配单元格高度。
- **带复选框单元格**：`DataGridTemplateColumn` 居中 CheckBox，支持一键点击勾选标记（关键部件、质检状态等）。
- **数值与金额列**：文字靠右对齐（`DataGrid.Element.TextRight` 与 `DataGrid.Editing.TextBoxRight`）。
- **状态徽章列**：结合主题语义色彩（`Brush.Success.Subtle`、`Brush.Accent.Subtle` 等）展示审核状态。
- **展开详情行 (RowDetails)**：嵌套展示加工工艺、公差配合与更新时间，展开内容内部适配。
- **工具条与分页**：顶部复合过滤与批量操作（批量审核、批量删除），底部分页与金额数量统计汇总。

---

## 2. WorkbenchWindow.xaml (工程工作台三栏布局)

演示 CAD/CAM/CAE 插件及专业桌面软件经典的三栏架构：
- **左栏**：装配构件与特征导航树（带搜索过滤框）。
- **中栏**：交互视口画板卡片 + 下半部分特征几何参数表，支持可拖动 `GridSplitter`。
- **右栏**：分组折叠式属性检查器（Property Grid），采用 `Expander` + 两列表单紧凑排版。
- **底部**：系统运行状态与视口坐标信息条。

---

## 3. WizardWindow.xaml (分步向导对话框)

演示配置向导、图纸导入导出向导的标准化骨架：
- **顶部步骤条**：带编号徽章与流程连接线的高亮步骤指示器（Step 1/2/3）。
- **单层自适应容器**：规避多 TabControl 造成的高度跳变事故，内容在单一面板中平滑切换。
- **标准化步进操作**：底部独立的“上一步”、“下一步 / 开始批处理”、“取消”按钮组。
- **状态说明保护**：引导说明与操作按钮分行/分列排布，长文案绝不挤压按钮。

---

## 4. IconGalleryWindow.xaml (图标系统测试矩阵)

演示与测试图标系统的全场景约束：
- **尺寸阶梯**：Micro (12px) / Small (14px) / Regular (16px) / Medium (20px) / Large (24px) / Hero (32px)。
- **动态着色契约**：同一矢量 Path 放入主按钮（自动白标）、次按钮（深色标）、禁用按钮（浅灰标）、危险按钮（红标）。
- **纯图标按钮矩阵**：`Button.Icon` (28px)、`Button.Icon.Small` (24px)、`Button.Icon.Large` (32px)、`Button.Icon.Danger`，覆盖正常、悬停、禁用态与 ToolTip 提示。
- **语义状态提示**：成功 (Success)、警告 (Warning)、危险 (Danger)、信息 (Info) 的通知卡片图标组合。
- **业务集成**：带清除按钮的搜索框复合控件、TabControl 图标选项卡、工具条组合。

---

## 2. SampleWindow.xaml (通用业务表单)

演示：
- 标准窗口边距、滚动区域、底部操作栏；
- 两列表单（标签列 `Auto` + `MinWidth`，输入列 `*` + `MinWidth`）；
- `GroupBox` + `WrapPanel` 复选框组；
- `Expander` 高级设置；
- `ListBox` 选中态；
- 只读日志框；
- 主/次/文本按钮；
- ToolTip。

---

## 3. SafeFormWindow.xaml (安全表单骨架 / 防截断防遮挡)

专门演示最容易出问题的场景，对应规则 R14：

- 标签列 `Width="Auto"` + `MinWidth="{StaticResource Layout.FormLabelMinWidth}"`：
  长标签（如"文档标题（用于换行演示）"）把列撑宽，**不会被裁成一半**；
- 输入列 `Width="*"` + `MinWidth="{StaticResource Layout.InputMinWidth}"`，
  输入框用 `TextBox.Form`：窄窗口下仍有可用宽度，长值靠 `ToolTip` 查看；
- 窗口 `SizeToContent="Height"` + `MinHeight="420"`：内容变多时窗口跟着长；
- 内容区一层 `ScrollViewer`：150% DPI 下可滚动，不会被裁；
- 状态文案（长路径）独占一行，按钮栏独占一行，互不影响；
- 无负 `Margin`、无 `Canvas` 绝对定位、无省略号缺 `ToolTip`。

对照反例见 `references/xaml-snippets.md` §18.1。


## 使用

1. 复制 `SampleWindow.xaml` 到你的项目。
2. 修改 `x:Class` 为你的命名空间和类名。
3. 确保 `App.xaml` 或 `ThemeLoader` 已合并 `Theme/Theme.xaml`。
4. 运行，按下面的清单检查状态。

## 状态检查清单

- [ ] 主按钮：正常、悬停、按下、禁用，文字始终可读。
- [ ] 次按钮：悬停浅灰底，文字仍为主色。
- [ ] 危险按钮：悬停浅红底 + 深红字。
- [ ] 输入框：悬停边框变深、聚焦边框变蓝。
- [ ] ComboBox：展开后弹层为浅色主题，选中项浅蓝底深字。
- [ ] CheckBox：勾选蓝底白勾，禁用灰底灰字。
- [ ] ListBox：选中浅蓝底深字，选中+悬停背景略深，文字仍深色。
- [ ] Expander：展开/收起箭头旋转，头部悬停浅底。
- [ ] 日志框：只读、可滚动、字体与其他文字一致。
- [ ] ToolTip：深底白字。
- [ ] 125%/150% DPI 下无裁切、无发虚。

## 截断与遮挡检查清单（R14）

- [ ] 标签列是 `Auto` + `MinWidth`，没有固定像素列宽。
- [ ] 输入列是 `*` + `MinWidth`，输入框没有写死 `Width`。
- [ ] 所有省略号都有 `ToolTip`。
- [ ] 窗口没有写死 `Height`（`SizeToContent="Height"` + `MinHeight`）。
- [ ] 内容区有且只有一层 `ScrollViewer`。
- [ ] 状态文案与按钮栏分行。
- [ ] 没有负 `Margin`，没有 `Canvas` 绝对定位。

## 注意

示例中的 `x:Class` 只是占位符。直接编译会找不到对应的代码隐藏文件；
请复制到实际项目后修改命名空间，或删除 `x:Class` 作为纯设计稿预览。
