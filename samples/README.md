# 参考 XAML 目录

按用途分类的**可复制**窗口骨架，全部遵循：三档密度（默认中等）、状态契约、
无截断 / 无遮挡。`preview/` 里有对应的可运行版本；本目录只保留可复制片段，
不参与编译，`x:Class` 是占位符。

```text
samples/
├─ forms/
│  ├─ SafeFormWindow.xaml        ← 安全表单：长标签/长值/状态行/底部栏
│  ├─ TabsSettingsWindow.xaml    ← 标签页设置：可见选中态 + 输入占满 + 防跳变
│  └─ StackedFormWindow.xaml     ← 窄面板单列表单：标签在上、控件在下
├─ tables/
│  └─ EditableTableWindow.xaml   ← 可编辑表格：全选联动/单元格编辑/分页
├─ layouts/
│  └─ WorkbenchWindow.xaml       ← 三栏工作台：树 / 视口 / 属性检查器
├─ dialogs/
│  └─ WizardWindow.xaml          ← 分步向导：步骤条 + 单层面板 + 流程控制
└─ icons/
   └─ IconGalleryWindow.xaml     ← 图标矩阵：尺寸阶梯 / 着色契约 / 图标按钮
```

## 使用

1. 复制需要的 `.xaml` 到业务项目。
2. 把 `x:Class` 改成你的命名空间与类名（或删除 `x:Class` 作为纯设计稿）。
3. 确保已合并 `Theme/Theme.xaml`（或 `Theme.Compact.xaml` / `Theme.Relaxed.xaml`）。
4. 按下面的清单检查状态与布局。

## 文件说明

### forms/SafeFormWindow.xaml

对应规则 R11 / R14，专门演示最容易出问题的场景：

- 标签列 `Auto` + `MinWidth`；长标签在 `TextBlock` 上设 `MaxWidth` 并换行，既不被裁、也不会把输入列挤出可视区；
- 输入列 `*` + `MinWidth`，输入框不写死 `Width`；长值配 `ToolTip`；
- 窗口 `SizeToContent="Height"` + `MinHeight`，内容区一层 `ScrollViewer`；
- 状态文案独占一行，按钮栏独占一行；
- 无负 `Margin`、无 `Canvas` 绝对定位、无省略号缺 `ToolTip`。

### forms/StackedFormWindow.xaml

- 窄面板（< 520 DIP）单列布局：标签在上、控件在下；
- `FormStack` + `Text.FieldLabel`，标签与输入框共享左基线；
- 不使用左右两列，避免输入列被 `MinWidth` 挤出可视区；
- 适合停靠窗、属性面板、侧边设置栏。

### forms/TabsSettingsWindow.xaml

- `TabItem` 选中态 = Accent 实心底 + 白字 + 3px 加深条 + SemiBold；
- 输入区横向占满，点击右侧空白可继续输入；
- `SizeToContent` 窗口里的 `TabControl` 显式 `Height`，切换标签页不改变窗口高度；
- 禁用标签页灰字、不参与命中测试。

### tables/EditableTableWindow.xaml

- 表头三态全选与行勾选联动；
- 文本 / 下拉 / 复选框单元格；
- 数值右对齐、状态徽章、展开明细行；
- 工具栏（左筛选/批量、右视图控制）与底部分页汇总；
- 批量操作条用 `ToolStrip` 容器 + 选中数胶囊徽章；未勾选时批量按钮禁用（含危险按钮中性化）；
- 行高取 `Layout.RowHeight`，编辑器随行高伸展。

### layouts/WorkbenchWindow.xaml

- 左：装配特征树 + 搜索过滤；
- 中：视口卡片 + 特征参数表，支持 `GridSplitter`；
- 右：`Expander` + 两列表单的属性检查器；
- 底部系统状态条。

### dialogs/WizardWindow.xaml

- 顶部步骤条（编号徽章 + 连接线）；
- 单层内容容器切换，规避多 `TabControl` 高度跳变；
- 底部上一步 / 下一步 / 取消标准化控制；
- 步骤说明独立排布，长文案不挤压按钮。

### icons/IconGalleryWindow.xaml

- 尺寸阶梯：Micro 12 / Small 14 / Regular 16 / Medium 20 / Large 24 / Hero 32；
- 同一 Path 在主按钮/次按钮/禁用/危险按钮中自动继承前景色；
- `Button.Icon` / `.Small` / `.Large` / `.Danger` 覆盖正常、悬停、禁用与 ToolTip；
- 带清除按钮的搜索框、图标标签页、工具条组合。

## 状态检查清单

- [ ] 主按钮：正常、悬停、按下、禁用，文字始终可读。
- [ ] 次按钮：白底 + `Border.Control` 描边；悬停浅灰底 + Accent 描边。
- [ ] 危险按钮：悬停浅红底 + 深红字。
- [ ] 输入框：整块区域可聚焦，悬停 Accent 描边，聚焦 Focus 描边。
- [ ] ComboBox：弹层浅色主题，选中项浅蓝底深字。
- [ ] CheckBox：勾选蓝底白勾，禁用灰底灰字。
- [ ] ListBox：选中浅蓝底深字，选中 + 悬停背景略深。
- [ ] 标签页：选中态是 Accent 实心面 + 白字 + 3px 加深条。
- [ ] Expander：展开/收起箭头旋转，头部悬停浅底。
- [ ] 日志框：只读、可滚动、字体与其他文字一致。
- [ ] ToolTip：深底白字。
- [ ] 125% / 150% DPI 下无裁切、无发虚。

## 截断与遮挡检查清单

- [ ] 标签列 `Auto` + `MinWidth`，没有固定像素列宽。
- [ ] 输入列 `*` + `MinWidth`，输入框没有写死 `Width`。
- [ ] 所有省略号都有 `ToolTip`。
- [ ] 窗口没有写死 `Height`（`SizeToContent="Height"` + `MinHeight`）。
- [ ] 内容区有且只有一层 `ScrollViewer`。
- [ ] 状态文案与按钮栏分行。
- [ ] 没有负 `Margin`，没有 `Canvas` 绝对定位。

## 注意

示例中的 `x:Class` 只是占位符，直接编译会找不到代码隐藏文件。
请复制到实际项目后修改命名空间，或删除 `x:Class` 作为纯设计稿预览。
