# WPF UI 速查表

## 五条前景/背景规则

1. 背景变了，前景必须一起变。
2. 禁用不用 `Opacity`；用 `Control.Disabled` + `Text.Disabled`。
3. 隐式 `TextBlock`/`AccessText` 前景绑定 `(TextElement.Foreground)`，不要硬编码。
4. 白字只压在 `Accent` / `Accent.Hover` / `Accent.Pressed` / `Danger*` / `Tooltip.Background`。
5. 选中+悬停用 `MultiTrigger`，不要依赖触发器顺序。

## 字体与字号（全局统一，不写本地字体）

| 令牌 | 值 |
|---|---|
| `Font.Ui` | `Microsoft YaHei`（唯一字体栈） |
| `FontSize.Title` / `Section` | 13 |
| `FontSize.Body` | 12 |
| `FontSize.Caption` | 11.5 |
| `FontSize.Code` | 11.5 |
| `FontSize.Display` | 16 |
| `FontSize.Micro` | 10 |
| `LineHeight.Body` / `Caption` | 18 / 17 |

- 业务 XAML 禁止出现 `FontFamily`，禁止写 `FontSize="13"` 字面量。
- 复选框 / 单选框不得小于 **16×16**（`Layout.CheckBox.Size`）。
- 字号不随密度变化；控件高度/行高由密度令牌控制：紧凑 26/24、中等 30/28、宽松 34/32。

## 三档密度（默认中等）

| 密度 | 入口 | 控件高 | 表格行 | Tab 内边距 | 按钮内边距 |
|---|---|---:|---:|---|---|
| 紧凑 | `Theme.Compact.xaml` | 26 | 24 | 10,4 | 10,0 |
| 中等 | `Theme.xaml` | **30** | **28** | **12,6** | **12,0** |
| 宽松 | `Theme.Relaxed.xaml` | 34 | 32 | 16,8 | 16,0 |

- 业务 XAML 用 `{StaticResource Layout.*}`；主题内部用 `{DynamicResource Layout.*}`（支持运行时换挡）。
- 同一窗口只用一档；颜色、字号、圆角、16px 勾选框不随密度变化。
- 运行时切换：替换应用级 `Density.*.xaml` 合并字典（见 `docs/06-density.md`）。

## 状态速查

| 状态 | 背景 | 前景 | 边框 |
|---|---|---|---|
| 次按钮 Normal | `Brush.Surface` | `Brush.Text.Primary` | `Brush.Border.Control` |
| 次按钮 Hover | `Brush.Control.Hover` | `Brush.Text.Primary` | `Brush.Accent` |
| 主按钮 Normal | `Brush.Accent` | `Brush.OnAccent` | `Brush.Accent` |
| 主按钮 Hover | `Brush.Accent.Hover` | `Brush.OnAccent` | `Brush.Accent.Hover` |
| 主按钮 Disabled | `Brush.Control.Disabled` | `Brush.Text.Disabled` | `Brush.Border` |
| 危险按钮 Hover | `Brush.Danger.Subtle` | `Brush.Danger.Hover` | `Brush.Danger.Hover` |
| 输入框 Focus | `Brush.Surface` | `Brush.Text.Primary` | `Brush.Focus` |
| 列表 Hover | `Brush.Control.Hover` | `Brush.Text.Primary` | 透明 |
| 列表 Selected | `Brush.Selection` | `Brush.Text.Primary` | 透明 |
| 列表 Selected+Hover | `Brush.Selection.Hover` | `Brush.Text.Primary` | 透明 |
| ComboBoxItem Selected | `Brush.Accent.Subtle` | `Brush.Text.Primary` | 透明 |
| 标签页 Normal | `Brush.SurfaceAlt` | `Brush.Text.Secondary` | `Brush.Border.Control` 四边 |
| 标签页 Selected | `Brush.Surface` | `Brush.Text.Primary` | `Brush.Border.Control`（无下边框，与内容区连通） |
| ToolTip | `Brush.Tooltip.Background` | `Brush.Tooltip.Foreground` | 同背景 |

## 多行文本高度与自适应

```xml
<!-- 方案 A：分栏/比对/日志（拉伸充满 * 行，无裁切） -->
<TextBox Style="{StaticResource TextBox.Multiline}"
         VerticalAlignment="Stretch" />

<!-- 方案 B：普通表单多行输入（自适应自然高度，无死锁） -->
<TextBox Style="{StaticResource TextBox.Multiline}"
         Height="Auto"
         VerticalAlignment="Top" />
```

规则：
- `TextBox.Multiline` 默认不设全局 `MinHeight`（避免撑爆小窗体和内部滚动打架）。
- 对照视图/日志放在 `*` 行并设置 `VerticalAlignment="Stretch"`。
- 表单多行放在 `Auto` 行并设置 `VerticalAlignment="Top"`。
- 不要写 `Height="*"`。


## 输入占满与标签页（R16）

```xml
<!-- 输入区横向占满：列用 *，输入框不写 Width -->
<TextBox Grid.Column="1" Text="{Binding Name}" />

<!-- 标签页：原生方角页签，选中页签与内容区连成一体，无圆角/无底部强调条 -->
<TabControl>
    <TabItem Header="基本信息">...</TabItem>
    <TabItem Header="高级选项">...</TabItem>
</TabControl>
```

- 不要给 `TextBox` / `PasswordBox` / `ComboBox` 设 `HorizontalContentAlignment="Left"`。
- 标签页选中态不能只剩一条 1px 横线；仍禁止用 tab 承载整窗多任务导航。
- **内容左边缘必须一致**：`PART_ContentHost` / `PART_TextBox` 只吃 `Padding`，**不要再加 `Margin="{TemplateBinding Padding}"`**（TextBox 自己会把 Padding 交给宿主，叠加即双倍）；`ComboBox` 显示内容由 ContentPresenter 渲染、没有 `TextBoxView` 的约 2px 内缩，需要补 2px 才和输入框对齐。

## 复选框

- 对勾用**填充路径**（不是 1.6px 细描边），几何**从 (0,0) 起笔**（Path 的 DesiredSize 是“到原点的外包”，偏移起笔会导致右下偏移），偶数外包 10×8，居中后落在 16×16 视口的 3~13 / 4~12；
- 三态必须区分：`IsChecked="True"` 白勾、`{x:Null}` 白横杠、`False` 空框；
- 禁用态勾/横杠切 `Brush.Text.Disabled`，不留白色 glyph；
- 全选框用 `IsThreeState="True"` + `Focusable="False"`（避免启动时抢焦点框）。
- 无文字的独立复选框（表头全选框、单元格里的）会由隐式样式的 `HasContent=False` 触发器把 `Margin` 归零，和行内复选框落在同一条竖线上。
- 表格里用 `Style="{StaticResource DataGrid.Element.CheckBox}"` 时必须确保该 keyed 样式 `BasedOn="{StaticResource {x:Type CheckBox}}"`，否则会退回系统方角复选框。

## 表格单元格下拉框

- 用 `Style="{StaticResource DataGrid.Cell.ComboBox}"`（单次点击直接展开）；
- 高度取 `Layout.CellControlHeight`（= 行高 − 2），**不要用 `Layout.RowHeight`**：行底有 1px 网格线，撑满整行的盒子 hover/展开时描边会压在分隔线上；
- 数据源绑定别写死 `AncestorType=Window`：页面是 `UserControl` 时会解析成 null，单元格空白、下拉点开是一个空壳（模板里已加 `HasItems=False` 隐藏空壳兜底，但根因是绑定）。

## 树 / 列表选中态

- `TreeViewItem`、`ListBoxItem`、`ListViewItem`、`DataGridRow` **必须自带模板**：默认模板用 `SystemColors.HighlightBrushKey` 画选中（系统蓝 + 白字），样式触发器够不到；
- 自定义 `TreeViewItem` 模板要自己绑 `IsExpanded` 隐藏 `ItemsPresenter`，否则折叠节点仍画出子项；
- 展开箭头用 `HasItems` 控制可见性（不是 `IsExpanded`）；子项缩进 18px 与箭头对齐；
- `ListViewItem` 直接 `BasedOn="{StaticResource {x:Type ListBoxItem}}"`，别复制触发器——否则残留系统焦点虚线框。

## 批量操作条（Zone 3 工具条）

```xml
<Border Style="{StaticResource ToolStrip}" Margin="0,0,0,8">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="Auto" />
        </Grid.ColumnDefinitions>

        <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
            <Button Content="添加物料" Margin="0,0,8,0" />
            <Button Content="批量审核通过" Margin="0,0,8,0" IsEnabled="False" />
            <Button Content="批量删除" Style="{StaticResource Button.Danger}"
                    Margin="0,0,10,0" IsEnabled="False" />
            <Border Background="{StaticResource Brush.SurfaceAlt}"
                    CornerRadius="{StaticResource Radius.Pill}"
                    Padding="{DynamicResource Layout.BadgePadding}"
                    VerticalAlignment="Center">
                <TextBlock Text="未选择项" FontSize="{StaticResource FontSize.Caption}"
                           Foreground="{StaticResource Brush.Text.Secondary}" />
            </Border>
        </StackPanel>

        <Button Grid.Column="1" Content="展开全部明细" />
    </Grid>
</Border>
```

- 工具条用 `ToolStrip` 包住，左边缘与表格/筛选卡对齐；
- 批量动作未勾选时必须 `IsEnabled="False"`；
- 选中数用胶囊徽章，选中后切 `Accent.Subtle + Accent`；
- 禁用态的危险按钮必须是灰底灰字，不能保留红色。

## 表单对齐（宽面板 / 窄面板）

| 场景 | 骨架 | 规则 |
|---|---|---|
| 宽面板 ≥ 520 DIP | 一个 Grid：标签 `Auto+MinWidth` / 输入 `*+MinWidth` | 所有行共享列定义，禁止每行一个 Grid |
| 窄面板 < 520 DIP | `FormStack` + `Text.FieldLabel` | 标签在上、控件在下，共享左基线 |
| 控件内容 | `TextBox` / `ComboBox` 左对齐 | 禁止 `Center`；数值列可 `TextAlignment="Right"` |
| GroupBox | 标题与内容同一内边距 | 不要用负 `Margin` 手工对位 |

```xml
<!-- 窄面板单列 -->
<StackPanel Style="{StaticResource FormStack}">
    <TextBlock Text="服务地址" Style="{StaticResource Text.FieldLabel}" />
    <TextBox Text="{Binding Url}" />
</StackPanel>
```


## 防截断 / 防遮挡（显示不全与重叠）

1. 标签列 `Width="Auto"` + `MinWidth="{StaticResource Layout.FormLabelMinWidth}"`（80）——
   **禁止**固定像素列宽，长标签会被裁掉。
2. 输入列 `Width="*"` + `MinWidth="{StaticResource Layout.InputMinWidth}"`（140）——
   输入框本身不写死 `Width`。
3. 省略号（`TextTrimming="CharacterEllipsis"`）必须配 `ToolTip`，否则内容永远看不到。
4. 窗口不写死 `Height`：用 `SizeToContent="Height"` + `MinHeight`，内容区包一层
   `ScrollViewer`（`VerticalScrollBarVisibility="Auto"`），整窗只允许一层垂直滚动。
5. 底部按钮栏独占一个 `Auto` 行；状态/路径/错误文案另起一行，不要和按钮同行。
6. 禁止负 `Margin`、禁止 `Canvas` 绝对定位排表单、禁止给含换行文本的控件写死 `Height`。
7. 表格/列表文字列 `Width="*"` + `MinWidth`，长值配 `ToolTip`。
8. 窗口内不要用多 tab 承载并排任务；确需 `TabControl` 时**固定它的 `Height`**，
   否则切到内容少的 tab 时窗口会缩矮（内容多了在 tab 内部滚动）。

## 图标行对齐

- 混排 24/28/32 三档图标按钮的行，用**水平 `StackPanel`**；`WrapPanel` 忽略 `VerticalAlignment`，会把图标排得高低错开。
- 主题已给按钮/图标按钮设 `VerticalAlignment="Center"`，别改回 `Stretch`。

## 交互态自检（R15）

| 交互态 | 必查项 |
|---|---|
| 表格单元格编辑 | 行高取 `Layout.RowHeight`；编辑器 `Height=Auto` 跟随行高，文字上下居中 |
| 折叠面板展开长内容 | 内容区出现滚动条，窗口不被撑高，底部按钮栏不被顶出 |
| 日期/下拉弹层打开 | 弹层有内容；按钮不压住输入内容（`DatePickerTextBox` 要 `Stretch`） |
| 长列表滚动 | 竖向滚动条可见，滑块方向正确（`Track.Orientation` 绑定 `TemplateBinding Orientation`） |

预览工程自带状态探针：`dotnet run --project preview -- -shot out/probe.png -window probe`

## 布局骨架

```xml
<Grid Margin="{StaticResource Layout.WindowPadding}">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" /> <!-- 标题 -->
        <RowDefinition Height="*" />    <!-- 主内容/滚动 -->
        <RowDefinition Height="Auto" /> <!-- 按钮 -->
    </Grid.RowDefinitions>

    <ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto">
        <StackPanel>
            <GroupBox Header="选项">...</GroupBox>
        </StackPanel>
    </ScrollViewer>

    <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Right">
        <Button Content="取消" IsCancel="True" Margin="0,0,8,0" />
        <Button Content="保存" Style="{StaticResource Button.Primary}" IsDefault="True" />
    </StackPanel>
</Grid>
```

## 安全表单行（R14）

```xml
<Grid Style="{StaticResource FormRow}">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" MinWidth="{StaticResource Layout.FormLabelMinWidth}" />
        <ColumnDefinition Width="*" MinWidth="{StaticResource Layout.InputMinWidth}" />
    </Grid.ColumnDefinitions>
    <TextBlock Text="起始编号" Style="{StaticResource Text.Label}" />
    <TextBox Grid.Column="1" Style="{StaticResource TextBox.Form}" />
</Grid>
```

## 对比度

| 内容 | 最低 |
|---|---:|
| 正文 | 4.5:1 |
| 大号文字 | 3:1 |
| 焦点/非文字 | 3:1 |
| 禁用文字 | 3:1（建议 4.5:1） |

## 常用命令

```powershell
pwsh ./tools/ContrastCheck.ps1
pwsh ./tools/LayoutCheck.ps1 -Root "D:\your\project"
```

## Top 10 错误

1. 隐式 TextBlock 硬编码前景 → 主按钮蓝底深灰字。
2. 禁用 `Opacity=0.45` → 文字看不清。
3. 只改背景不改前景 → 悬停后文字消失。
4. 选中项白字压浅蓝底。
5. 选中+悬停没有 `MultiTrigger`。
6. ComboBox 弹层还是系统默认样式。
7. DataGridCell 选中背景盖住行背景。
8. 多行 TextBox 默认 `Stretch` 撑满 `*` 行。
9. 框中框反模式：外层 Border 加大 Padding 套内层 TextBox，产生双重框与巨大空白。
10. 工具栏全员次要按钮：破坏性操作（如删除）混在常规操作中无危险警示色。
11. 标签列写死像素宽度 → 长标签被裁成 "起始流…"。
12. 窗口写死 `Height` + 无滚动 → 底部按钮被内容盖住（150% DPI 更明显）。
13. 输入框内容宿主 `Left` 对齐 → 右侧点不进去，长值像被截断。
14. 标签页选中只画一条细线 → 分不清当前页。
15. 白底按钮用浅色描边 → 按钮边界消失；应使用 `Brush.Border.Control`。
16. 同一窗口混用 26/30/34 三档高度 → 基线不齐。


---

## 图标系统速查 (Google Material Symbols 规范)

### 官方规范基准
- **数据来源**：Google Material Symbols（Apache 2.0 开源许可，自由商用）。
- **视觉重量**：统一 **Outlined / Weight 400 / Grade 0 / Optical Size 24**——不过粗（700）也不过细（200）。
- **坐标视口**：统一 24×24 视口网格，每条几何末尾带 `M0,0 L24,24` 锚点，`Stretch="Uniform"`，`SnapsToDevicePixels="True"`。
- **线型控件**：复选框勾 1.6px，下拉/折叠箭头 1.5px，圆头端点。

### 图标尺寸阶梯
- `Icon.Size.Micro` (12px): 状态徽章、角标
- `Icon.Size.Small` (14px): 表格小操作、单行输入框尾缀
- `Icon.Size.Regular` (16px): **默认尺寸**（按钮、菜单、Tab）
- `Icon.Size.Medium` (20px): 工具栏图标、面板操作
- `Icon.Size.Large` (24px): 提示卡片、大号入口
- `Icon.Size.Hero` (32px): 空状态插画、引导视窗

### 图标着色黄金法则
- **严禁写死 Fill="#..."**。
- 默认继承宿主 `(TextElement.Foreground)`，主按钮自动纯白，禁用自动变灰。
- 纯图标按钮必带 `ToolTip`。
- 图标按钮尺寸矩阵（中等密度）：`Button.Icon.Small` (24px) / `Button.Icon` (28px) / `Button.Icon.Large` (32px)；紧凑/宽松按 `Layout.IconButton.*` 自动换挡。


