# 参考 XAML 目录与标准案例分类

按工程用途严谨分类的**可复用**窗口骨架，全部遵循：三档密度（默认中等）、状态契约、无截断 / 无遮挡。`preview/` 目录中提供对应开箱即用的交互版本；本目录保留纯 XAML 片段，不参与构建，`x:Class` 仅为占位符。

```text
samples/
├── README.md                      # 案例中心索引与最佳实践指南
├── controls/                      # 基础控件库与图标矩阵
│   ├── ControlsGalleryWindow.xaml # 全套基础控件交互矩阵（按钮/输入/下拉/复选/单选/滑块/日历/标签页）
│   └── IconGalleryWindow.xaml     # 矢量图标系统与尺寸着色契约
├── forms/                         # 各类工程表单规范骨架
│   ├── SafeFormWindow.xaml        # 防截断安全表单（长标签换行/长值ToolTip/单层滚动/独立按钮行）
│   ├── FormLayoutWindow.xaml      # 表单基线对齐（宽面板双列对齐 / 窄面板单列垂直堆叠）
│   ├── StackedFormWindow.xaml     # 窄面板侧栏属性表单（标签在上、控件在下）
│   ├── SettingsWindow.xaml        # 密集参数配置表单（三列网格 / 一屏多视 / 滚动区解耦）
│   └── TabsSettingsWindow.xaml    # 标签页配置面板（多方向原生方角页签 / 高度防跳变）
├── tables/                        # 工程数据表格与批量交互
│   └── EditableTableWindow.xaml   # 复杂可编辑表格（三态全选/行勾选联动/单元格下拉/数值右对齐/分页汇总）
├── layouts/                       # 复杂多栏工作区布局
│   ├── WorkbenchWindow.xaml       # 三栏工程工作台（特征树 / 3D模型视口 / 属性检查器 / 状态栏）
│   ├── CompareWindow.xaml         # 双栏差异对比与日志流（左右分栏独立滚动 / 顶部工具条 / 底部固定）
│   └── DashboardWindow.xaml       # 工况监控与生产看板（KPI 卡片矩阵 / 任务流水线 / 质检告警表格）
└── dialogs/                       # 流程引导与弹框规范
    └── WizardWindow.xaml          # 多步骤向导对话框（步骤指示器 / 步骤状态机 / 标准化流程操作条）
```

## 使用说明

1. 复制需要的 `.xaml` 到业务工程对应模块；
2. 将 `x:Class` 调整为你项目的命名空间与类名（或移除 `x:Class` 作为纯 XAML 视图引用）；
3. 确保在 `App.xaml` 或主窗口资源中已合并 `Theme/Theme.xaml`（或切换为紧凑 `Theme.Compact.xaml` / 宽松 `Theme.Relaxed.xaml`）；
4. 对照下方清单进行状态契约与防截断校验。

---

## 案例详细说明

### 1. 基础控件与图标库 (`controls/`)

- **`controls/ControlsGalleryWindow.xaml`**：
  - 核心控件全景：次按钮、主操作、文本按钮、危险按钮、小号变体、禁用态全覆盖；
  - 文本框与密码框左边缘内边距严格对其，带水印占位与焦点光圈；
  - 下拉框（ComboBox）浅色悬停与键盘导航；日期控件（DatePicker & Calendar）统一主题化；
  - CheckBox 居中矢量无偏移对齐，Slider 刻度与垂直模式，TabControl 原生方角页签与多方向挂载。
- **`controls/IconGalleryWindow.xaml`**：
  - 包含 Google Material Symbols 官方矢量图标与尺寸阶梯（Micro 12px ~ Hero 32px）；
  - 图标前景自动继承 `TextElement.Foreground`，主按钮白标、次按钮深标、危险按钮红标。

### 2. 工程表单体系 (`forms/`)

- **`forms/SafeFormWindow.xaml`**：
  - 对应防截断规范，专门演示长标签换行（`Auto` + `MinWidth` + `MaxWidth`）；
  - 输入列 `*` 弹性延展，长文字配备 ToolTip；
  - 独占单层垂直滚动容器 `ScrollViewer`，状态提示与操作按钮独立分行，杜绝推挤溢出。
- **`forms/FormLayoutWindow.xaml`**：
  - 宽面板（≥ 520 DIP）：双列网格，标签居左、输入居右，共用左对齐基线；
  - 窄面板 / 侧栏（< 520 DIP）：垂直单列堆叠，标签在上、控件在下，天然免疫宽度压缩截断。
- **`forms/StackedFormWindow.xaml`**：
  - 侧边停靠栏属性检查器典型骨架，标签与输入框共享左基线。
- **`forms/SettingsWindow.xaml`**：
  - 密集型配置骨架：三列并排排布，行距与垂直空间紧凑高效，一屏呈现多项配置。
- **`forms/TabsSettingsWindow.xaml`**：
  - 原生方角页签：页签与内容区连为一体，显式指定 `Height` 防止切换标签页导致的窗口高度跳变。

### 3. 数据表格 (`tables/`)

- **`tables/EditableTableWindow.xaml`**：
  - 表头全选/部分选中与数据行勾选双向联动；
  - 单元格单次点击激活下拉框（`DataGrid.Cell.ComboBox`）；
  - 数值统一靠右对齐、状态徽章、展开嵌套参数明细行；
  - 批量操作工具条自带选中计数胶囊徽章，未勾选时按钮中性置灰；
  - 底部集成汇总统计与分页控制。

### 4. 复杂多栏工作区布局 (`layouts/`)

- **`layouts/WorkbenchWindow.xaml`**：
  - 左侧：工程特征树与快速过滤；
  - 中间：三维模型交互视口模拟与参数明细表，带 `GridSplitter` 弹性分隔；
  - 右侧：可折叠属性面板与两列表单；
  - 底部：系统状态栏。
- **`layouts/CompareWindow.xaml`**：
  - 左右双栏对称对照面板，各自独立滚动；
  - 底部 120 DIP 固定高度只读日志流，不争抢主视口弹性空间。
- **`layouts/DashboardWindow.xaml`**：
  - 顶部 4 组 KPI 指标卡片矩阵（吞吐量、通过率、待干预数、平均耗时）；
  - 实时任务流水线进度与质检告警表格协同联动；
  - 底部运维状态与引擎资源指标监控。

### 5. 流程引导与向导 (`dialogs/`)

- **`dialogs/WizardWindow.xaml`**：
  - 顶部步骤条（数字徽章 + 连接线）；
  - 单层内容容器配合状态机切换，无多级 Tab 跳变隐患；
  - 底部“上一步 / 下一步 / 完成”标准化操作栏。

---

## 状态检查清单

- [ ] 主按钮：正常、悬停、按下、禁用，文字始终保持高对比可读。
- [ ] 次按钮：白底 + `Border.Control` 描边；悬停浅灰底 + Accent 描边。
- [ ] 危险按钮：悬停浅红底 + 深红字，禁用时中性置灰。
- [ ] 输入框：整块区域可聚焦，悬停 Accent 描边，聚焦 Focus 描边。
- [ ] ComboBox：浅色弹层，选中项浅蓝底深字。
- [ ] CheckBox：勾选蓝底白勾，禁用灰底灰字。
- [ ] ListBox：选中浅蓝底深字，选中 + 悬停背景略深。
- [ ] 标签页：每个页签边框可见，选中页签与内容区连通。
- [ ] Expander：展开/收起箭头旋转，头部悬停浅底。
- [ ] 日志框：只读、可滚动、字体与其他文字一致。
- [ ] ToolTip：深底白字。
- [ ] 125% / 150% 高 DPI 下无裁切、无发虚。

## 截断与遮挡检查清单

- [ ] 标签列 `Auto` + `MinWidth`，没有固定像素列宽。
- [ ] 输入列 `*` + `MinWidth`，输入框没有写死 `Width`。
- [ ] 所有省略号都有 `ToolTip`。
- [ ] 窗口没有写死 `Height`（采用 `SizeToContent="Height"` + `MinHeight`）。
- [ ] 内容区有且只有一层 `ScrollViewer`。
- [ ] 状态文案与按钮栏分行。
- [ ] 没有负 `Margin`，没有 `Canvas` 绝对定位。
