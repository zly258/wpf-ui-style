# 示例

工程提供两个核心示例窗口：

1. `SampleWindow.xaml`：完整的业务设置/批量窗口示例。
2. `IconGalleryWindow.xaml`：**图标系统全场景测试与矩阵展示窗口**。

---

## 1. IconGalleryWindow.xaml (图标系统测试矩阵)

演示与测试图标系统的全场景约束：
- **尺寸阶梯**：Micro (12px) / Small (14px) / Regular (16px) / Medium (20px) / Large (24px) / Hero (32px)。
- **动态着色契约**：同一矢量 Path 放入主按钮（自动白标）、次按钮（深色标）、禁用按钮（浅灰标）、危险按钮（红标）。
- **纯图标按钮矩阵**：`Button.Icon` (28px)、`Button.Icon.Small` (24px)、`Button.Icon.Large` (34px)、`Button.Icon.Danger`，覆盖正常、悬停、禁用态与 ToolTip 提示。
- **语义状态提示**：成功 (Success)、警告 (Warning)、危险 (Danger)、信息 (Info) 的通知卡片图标组合。
- **业务集成**：带清除按钮的搜索框复合控件、TabControl 图标选项卡、工具条组合。

---

## 2. SampleWindow.xaml (通用业务表单)

演示：
- 标准窗口边距、滚动区域、底部操作栏；
- 两列表单；
- `GroupBox` + `WrapPanel` 复选框组；
- `Expander` 高级设置；
- `ListBox` 选中态；
- 只读日志框；
- 主/次/文本按钮；
- ToolTip。


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

## 注意

示例中的 `x:Class` 只是占位符。直接编译会找不到对应的代码隐藏文件；
请复制到实际项目后修改命名空间，或删除 `x:Class` 作为纯设计稿预览。
