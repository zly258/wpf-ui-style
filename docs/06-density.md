# 06. 三档密度 (Compact / Medium / Relaxed)

同一套颜色、同一套状态、同一套结构，只换尺寸与留白。密度解决的是“同一个业务在不同
宿主里的呼吸感不同”，不是“做三套不同的主题”。

---

## 1. 设计约束

| 随密度变化 | 不随密度变化 |
|---|---|
| 控件高度（按钮 / 输入 / 下拉 / 日期 / 图标按钮） | 所有颜色与 `Brush.*` |
| 表格行高、表头高度 | 字号（Body 12 / Caption 11.5 / Title 13） |
| 按钮、输入、列表、标签页的内边距 | 圆角 `Radius.*` |
| 表单行间距、区块间距、窗口根边距 | 复选框/单选框靶心（固定 16px） |
| 工具条高度、进度条高度 | 日志框 / Expander 的最大高度约束 |

这样做的原因：

1. 字号随密度变化会破坏对比度实测值，也会让同一窗口的层级忽大忽小；
2. 16px 是复选框的可访问性下限，不参与缩放；
3. `MaxHeight` 属于“防撑爆”的安全阀，不应随密度放大。

---

## 2. 三档取值

| Token | 紧凑 Compact | 中等 Medium（默认） | 宽松 Relaxed |
|---|---:|---:|---:|
| `Layout.ControlHeight` | 26 | **30** | 34 |
| `Layout.ControlHeight.Small` | 24 | **26** | 30 |
| `Layout.ControlHeight.Large` | 32 | **36** | 40 |
| `Layout.RowHeight` | 24 | **28** | 32 |
| `Layout.HeaderHeight` | 26 | **30** | 34 |
| `Layout.ButtonMinWidth` | 56 | **64** | 72 |
| `Layout.ButtonMinWidth.Small` | 40 | **48** | 56 |
| `Layout.ButtonPadding` | 10,0 | **12,0** | 16,0 |
| `Layout.ButtonPadding.Small` | 6,0 | **8,0** | 10,0 |
| `Layout.InputPadding` | 6,0 | **8,0** | 10,0 |
| `Layout.InputPadding.Multiline` | 6,4 | **8,6** | 10,8 |
| `Layout.ComboBoxPadding` | 6,0,22,0 | **8,0,24,0** | 10,0,26,0 |
| `Layout.ComboBoxItemPadding` | 6,3 | **8,4** | 10,5 |
| `Layout.DatePickerPadding` | 6,0,28,0 | **8,0,30,0** | 10,0,32,0 |
| `Layout.ListItemPadding` | 6,4 | **8,5** | 10,6 |
| `Layout.ListViewItemPadding` | 2,1 | **3,2** | 5,3 |
| `Layout.MenuItemPadding` | 7,4 | **8,5** | 10,6 |
| `Layout.WindowPadding` | 8 | **12** | 16 |
| `Layout.GroupPadding` | 8 | **10** | 14 |
| `Layout.CardPadding` | 8 | **10** | 14 |
| `Layout.FormRowMargin` | 0,2,0,2 | **0,3,0,3** | 0,5,0,5 |
| `Layout.SectionMargin` | 0,0,0,6 | **0,0,0,10** | 0,0,0,14 |
| `Layout.TabPadding` | 10,4 | **12,6** | 16,8 |
| `Layout.TabMargin` | 0,0,2,0 | **0,0,4,0** | 0,0,6,0 |
| `Layout.TooltipPadding` | 6,3 | **8,4** | 10,5 |
| `Layout.BadgePadding` | 6,1 | **8,2** | 10,3 |
| `Layout.ExpanderHeaderPadding` | 8,4 | **10,6** | 12,8 |
| `Layout.ExpanderContentPadding` | 8,5 | **8,6** | 12,8 |
| `Layout.ToolBarPadding` | 4,2 | **5,3** | 6,4 |
| `Layout.ToolBarHeight` | 38 | **44** | 52 |
| `Layout.ProgressBarHeight` | 6 | **8** | 10 |
| `Layout.IconButton.Micro` | 22 | **24** | 28 |
| `Layout.IconButton.Small` | 24 | **28** | 32 |
| `Layout.IconButton.Large` | 28 | **32** | 36 |
| `Layout.TagHeight` | 22 | **24** | 26 |

`Density.Profile` 是字符串令牌（`compact` / `medium` / `relaxed`），便于调试和截图脚本识别当前档位。

---

## 3. 切换方式

### 3.1 静态切换（推荐）

只换入口字典，业务 XAML 不动：

```xml
<!-- 紧凑 -->
<ResourceDictionary Source="pack://application:,,,/YourAssembly;component/Theme/Theme.Compact.xaml" />
<!-- 中等（默认，等同于 Theme.xaml） -->
<ResourceDictionary Source="pack://application:,,,/YourAssembly;component/Theme/Theme.xaml" />
<!-- 宽松 -->
<ResourceDictionary Source="pack://application:,,,/YourAssembly;component/Theme/Theme.Relaxed.xaml" />
```

`Theme.Compact.xaml` / `Theme.Relaxed.xaml` 内部顺序固定：

```text
Tokens.xaml → Controls → DataControls → Icons → Extras → Density.<档位>.xaml
```

> 密度档案必须**最后**合并。`Controls/DataControls/Icons/Extras` 内部各自合并了
> `Tokens.xaml`（含中等档兜底值）；WPF 按合并字典的逆序查找，只有最后合并的密度档案
> 才会优先命中。把 Density 放在最前面会被 Tokens 的中等值盖住。

### 3.2 运行时热切换

控件内部全部使用 `{DynamicResource Layout.*}`，所以可以在运行中替换应用级字典：

```csharp
static void ApplyDensity(string profile) // "Compact" / "Medium" / "Relaxed"
{
    var dict = new ResourceDictionary
    {
        Source = new Uri(
            "pack://application:,,,/YourAssembly;component/Theme/Density." + profile + ".xaml",
            UriKind.Absolute)
    };

    var merged = Application.Current.Resources.MergedDictionaries;
    for (int i = merged.Count - 1; i >= 0; i--)
    {
        var src = merged[i].Source;
        if (src != null && src.OriginalString.Contains("/Density."))
            merged.RemoveAt(i);
    }
    merged.Add(dict); // 追加在末尾：查找时优先命中
}
```

`preview` 案例中心右上角的「密度」下拉就是这段代码的实际演示（`Shell/MainWindow`）。

> 为什么必须是 `DynamicResource`？
> `StaticResource` 在字典加载时就把值固化到 Setter 里，之后再合并新档案不会重新求值；
> 只有 `DynamicResource` 会在运行时沿资源链重新解析。

---

## 4. 编写业务 XAML 的规则

1. **不要写死尺寸**：`Height="32"`、`Padding="8,0"`、`RowHeight="26"` 一律换成 `{StaticResource Layout.*}`。
2. **不要给控件设 `Width`/`Height` 来"对齐"**：用网格列、`MinWidth`、`Margin` 解决。
3. **同一窗口只选一档**：混用不同档位看起来像对齐事故，而不是层级。
4. **自定义控件要接 token**：新样式里凡是高度、内边距、行高，优先引用 `Layout.*`，并在 `Density.*.xaml` 三份档案里同时登记。
5. **字号不要跟着密度变**：需要强调时用 `FontWeight`、`Foreground`，不要加大字号。

---

## 5. 与布局规则的关系

- R7（密度统一）与 R9（多行文本弹性）不冲突：`TextBox.Multiline` 仍然 `Height="Auto"`，只是内边距取自 `Layout.InputPadding.Multiline`。
- R11（防截断）里的 `Layout.FormLabelMinWidth` / `Layout.InputMinWidth` **不随密度变化**，保证窄窗口下仍可读。
- R12（禁止整窗多 Tab 导航）不变；`TabItem` 的选中态增强只影响局部切换的可识别性。
- R16（输入占满 / 标签页可识别）是本次新增的强制项。

---

## 6. 自动校验

```bash
# 密度档案键集一致性 + 控件 DynamicResource 接线 + 业务 XAML 反模式
python tools/LayoutCheck.py --root . --theme-dir Theme

# 新增的 Border.Control 描边对比度
python tools/ContrastCheck.py

# 契约测试（三档键集相同、严格递增、Tokens 兜底、DynamicResource 接线）
dotnet test tests/WpfStyleContractTests.csproj
```

`LayoutCheck` 在 Theme 目录内会额外检查：

- `Density.Compact/Medium/Relaxed` 三份档案键集必须完全一致；
- `Tokens.xaml` 必须为每个密度键保留兜底值；
- 控件文件里密度键不得使用 `{StaticResource ...}`，也不得引用未定义的 `{DynamicResource ...}`。

---

## 7. 选型建议

| 场景 | 推荐档位 | 理由 |
|---|---|---|
| 工程插件停靠窗、属性面板 | 紧凑 | 同屏信息量优先 |
| 常规设置窗口、业务对话框、向导 | 中等 | 默认平衡 |
| 主工作台、数据表格页 | 中等 / 紧凑 | 由行数决定，同一窗口保持一致 |
| 触屏、培训演示、低密度看板 | 宽松 | 更大点击靶心与留白 |
