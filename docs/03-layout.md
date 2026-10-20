# 03. 布局规范

布局目标：在 AutoCAD / Inventor 的宿主窗口里保持紧凑、可读、可缩放，不抢宿主焦点，也不因为 DPI 或长文本崩坏。

---

## 1. 窗口尺寸

| 类型 | 建议尺寸 | 最小尺寸 | 说明 |
|---|---|---|---|
| 设置窗口 | 640–720 × 560–680 | 580 × 480 | 内容多时用 ScrollViewer |
| 批量/任务窗口 | 700–900 × 480–650 | 560 × 360 | 列表 + 日志 |
| 报告窗口 | 900–1100 × 540–700 | 680 × 380 | 表格为主 |
| 关于窗口 | 420–480 × 320–380 | — | 不缩放 |
| 进度窗口 | 420–520 × 160–220 | — | 不显示任务栏 |

规则：

- 可随内容增长的窗口使用 `MinWidth`/`MinHeight`，不要写死 `Width`/`Height`。
- 固定流程的小窗口（关于、进度）可以固定尺寸，但必须设置 `ResizeMode`。
- 所有窗口 `WindowStartupLocation="CenterScreen"` 或 `CenterOwner`。
- 插件窗口 `ShowInTaskbar="False"`，并设置 Owner 为 CAD 主窗口（如果可行）。

---

## 2. 窗口内容骨架

### 2.1 标准设置窗口

```xml
<Grid Margin="{StaticResource Layout.WindowPadding}">
    <Grid.RowDefinitions>
        <RowDefinition Height="*" />
        <RowDefinition Height="Auto" />
    </Grid.RowDefinitions>

    <ScrollViewer Grid.Row="0"
                  VerticalScrollBarVisibility="Auto"
                  HorizontalScrollBarVisibility="Disabled">
        <StackPanel>
            <GroupBox Header="翻译服务">
                <!-- 表单 -->
            </GroupBox>
            <GroupBox Header="翻译选项">
                <!-- 表单 -->
            </GroupBox>
        </StackPanel>
    </ScrollViewer>

    <Grid Grid.Row="1" Margin="0,12,0,0">
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
            <Button Content="取消" IsCancel="True" Margin="0,0,8,0" />
            <Button Content="保存" Style="{StaticResource Button.Primary}" IsDefault="True" />
        </StackPanel>
    </Grid>
</Grid>
```

- 只允许**一层**垂直滚动。不要在 `ScrollViewer` 里再放一个 `ScrollViewer`。
- 底部操作栏固定在 `Grid.Row=1`，不随内容滚动。
- 按钮顺序：取消在左，主操作在右。
- 主操作使用 `Button.Primary`，`IsDefault="True"`；取消使用 `IsCancel="True"`。

### 2.2 列表 + 日志（批量窗口）

```xml
<Grid Margin="{StaticResource Layout.WindowPadding}">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="*" />
        <RowDefinition Height="Auto" />
        <RowDefinition Height="Auto" />
    </Grid.RowDefinitions>

    <TextBlock Grid.Row="0" Style="{StaticResource Text.Hint}"
               Text="添加要处理的图纸，或直接拖放文件。" Margin="0,0,0,8" />

    <ListView Grid.Row="1" MinHeight="180" />

    <TextBlock Grid.Row="2" Style="{StaticResource Text.Hint}"
               Text="就绪" Margin="0,8,0,8" />

    <StackPanel Grid.Row="3" Orientation="Horizontal" HorizontalAlignment="Right">
        <Button Content="设置" Margin="0,0,8,0" />
        <Button Content="开始" Style="{StaticResource Button.Primary}" Margin="0,0,8,0" />
        <Button Content="取消" IsCancel="True" />
    </StackPanel>
</Grid>
```

日志区：

- 使用 `TextBox` + `TextBox.Code` 或 `TextBox.Multiline`。
- `MinHeight=140`、`MaxHeight=240`。
- `IsReadOnly=True`、`TextWrapping=Wrap`、`VerticalScrollBarVisibility=Auto`。
- 不要用 `RichTextBox` 做日志，除非需要富文本；普通日志用 TextBox 更稳。

---

## 3. 表单布局

### 3.1 两列表单（推荐）

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="96" />
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="16" />
        <ColumnDefinition Width="96" />
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="Auto" />
    </Grid.RowDefinitions>

    <TextBlock Text="服务地址" Style="{StaticResource Text.Label}" />
    <TextBox Grid.Column="1" Text="{Binding Endpoint}" Margin="0,4" />

    <TextBlock Grid.Column="3" Text="模型名称" Style="{StaticResource Text.Label}" />
    <TextBox Grid.Column="4" Text="{Binding Model}" Margin="0,4" />

    <TextBlock Grid.Row="1" Text="访问令牌" Style="{StaticResource Text.Label}" />
    <PasswordBox Grid.Row="1" Grid.Column="1" Margin="0,4" />
</Grid>
```

规则：

- 标签列宽 88–104，推荐 96；同屏多个表单保持一致。
- 输入列 `*`，不要在输入框上写死宽度。
- 两列之间留 12–16。
- 标签左对齐，垂直居中。
- 同一行的控件高度一致（34 或 30）。
- 长文本说明放在输入框下方，使用 `Text.Hint`，左对齐到输入列。

### 3.2 单列长表单

```xml
<StackPanel>
    <TextBlock Text="名称列标题" Style="{StaticResource Text.Hint}" />
    <TextBox Margin="0,4,0,10" Text="{Binding BomNameColumnTitles}" />
    <TextBlock Text="跳过列标题" Style="{StaticResource Text.Hint}" />
    <TextBox Margin="0,4,0,0" Text="{Binding BomSkipColumnTitles}" />
</StackPanel>
```

单列表单适合：
- 文本域、说明多；
- 设置项少；
- 窗口宽度 < 600。

### 3.3 复选框组

```xml
<GroupBox Header="翻译范围">
    <WrapPanel>
        <CheckBox Content="单行文本" Width="130" />
        <CheckBox Content="多行文本" Width="130" />
        <CheckBox Content="引线文本" Width="130" />
        <CheckBox Content="块属性" Width="130" />
        <CheckBox Content="表格单元格" Width="130" />
    </WrapPanel>
</GroupBox>
```

- 用 `WrapPanel` 自动换行，不要用固定列数的 Grid 把标签挤断。
- 每个复选框宽度按最长标签设定，保持网格感。
- 复选框之间水平间距 8–16，垂直间距 4–8。

---

## 4. 分组与层级

- 一个窗口最多 3 层视觉层级：窗口 → GroupBox/Expander → 表单行。
- 不要 `GroupBox` 套 `GroupBox` 再套 `Border`；视觉噪音大。
- 分组标题使用 `Text.Section` 或 GroupBox Header，字号 14.5。
- 分组之间间距 12–16。
- 高级选项使用 `Expander` 收起，默认折叠；不要把低频参数全部铺开。

---

## 5. 按钮与操作

- 一个对话框只允许一个 `Button.Primary`。
- 危险操作使用 `Button.Danger`，并与普通操作拉开 12–16 间距。
- 图标按钮最小 28×28；不要小于 24×24。
- 按钮高度统一 34；工具条可用 30。
- 按钮文字用动词：保存、取消、开始翻译、加载失败项。
- 不要用“确定/是/否”以外的模糊词；危险操作必须写清楚对象。

---

## 6. 表格与列表

### 6.1 列表

- 行高 26（`Layout.RowHeight`）。
- 表头 28（`Layout.HeaderHeight`）。
- 列宽：短序号 40–56，名称/路径 `*`，状态 80–100。
- 长文本列使用 `TextTrimming="CharacterEllipsis"` 或 `ToolTip` 显示完整内容。
- 不要在列表项里放多层嵌套控件；一行最多一个主文本 + 一个次要文本 + 一个状态标签。

### 6.2 DataGrid

- `HeadersVisibility="Column"`。
- `GridLinesVisibility="Horizontal"`。
- `CanUserResizeRows="False"`。
- 交替行使用 `SurfaceSunken`，不要用高饱和色。
- 选中行使用 `Selection` + `Text.Primary`。
- 可编辑单元格的编辑器继承输入框样式；不要在单元格里再套一层自定义 TextBox。

---

## 7. 进度与日志

- 进度条高度 8，放在状态文字上方或下方。
- 百分比文字右对齐，状态文字左对齐，中间留空。
- 日志使用只读 TextBox，自动滚动到底部。
- 不要在日志里输出 ANSI 颜色码；WPF TextBox 不解析。
- 进度窗口的取消按钮始终可用（除非任务已经完成）。
- 长任务显示“当前文件 (3/20)”和“当前步骤”。

---

## 8. 空状态、加载、错误

### 空状态

```xml
<TextBlock Text="暂无术语。点击“新增词条”或“导入 CSV”。"
           Style="{StaticResource Text.Hint}"
           HorizontalAlignment="Center"
           VerticalAlignment="Center" />
```

- 空状态使用 `Text.Secondary`，居中，不要用大红/大图标。
- 空状态不要遮挡操作按钮。

### 加载

- 短任务：按钮禁用 + 状态文字“正在处理…”。
- 长任务：进度条 + 当前文件/步骤。
- 不要在 UI 线程做同步等待；使用 async/await。

### 错误

- 输入框边框 `Brush.Danger`。
- 下方提示文字 `Text.Danger`。
- 严重错误用 `MessageBox` 或错误面板，给出“发生了什么 + 怎么处理”。
- 不要把异常堆栈直接显示给最终用户；写入日志。

---

## 9. 字体与 DPI

- 窗口根设置 `TextOptions.TextFormattingMode="Display"`。
- `UseLayoutRounding="True"`、`SnapsToDevicePixels="True"`。
- 不要使用小于 12px 的字号。
- 100% / 125% / 150% / 200% DPI 都要检查：
  - 1px 边框是否发虚；
  - 按钮文字是否被裁切；
  - 表格行高是否够；
  - ComboBox 弹出层是否错位。

---

## 10. 键盘与焦点

- Tab 顺序与视觉顺序一致。
- 主操作 `IsDefault="True"`，取消 `IsCancel="True"`。
- 所有可交互控件有可见焦点（`Brush.Focus` 1px 边框）。
- 不要设置 `Focusable="False"` 给需要键盘操作的控件。
- 图标按钮必须有 `ToolTip` 和 `AutomationProperties.Name`。
- 表单标签使用 `Label Target="{Binding ElementName=...}"` 或 `AutomationProperties.LabeledBy`。

---

## 11. 多行文本高度与自适应原则 (Multiline Height Principles)

多行输入框（`TextBox.Multiline`）如果**强行硬编码全局 `MinHeight`**，会造成严重的可用性灾难：
1. **撑爆宿主小窗体**：在分辨率较低（如 1366x768）、高 DPI 缩放（150%）或嵌入式小面板中，强制 110px 以上的 `MinHeight` 会直接将下方的操作按钮、状态栏挤出可视区。
2. **产生双层滚动条与滚动死锁**：外层 `ScrollViewer` 与内层强制高度的 `TextBox` 滚动争抢，导致用户滚动滚轮时内容忽动忽停，误以为“输入框被冻结或显示不全”。
3. **内容较少时严重留白**：仅输入一两行字时，下方出现巨大僵死空白。

### 11.1 三项弹性自适应原则

1. **样式库默认解除 `MinHeight` / `MaxHeight` 硬限制**：
   - `TextBox.Multiline` 基础样式中，`MinHeight="0"`，`Height="Auto"`。让控件能够根据父容器（Grid 行、卡片）或者文本实际行数自然延展，绝不强行锁死尺寸。
2. **表单内多行输入（跟随行高或自然增高）**：
   - 放在 `Grid` 的 `Height="Auto"` 行中，依靠内部打字行数自然撑高，或者由业务根据弹窗尺寸局部决定是否设置 `Height`。
   - `VerticalAlignment="Top"`，`TextWrapping="Wrap"`，`AcceptsReturn="True"`。
3. **分栏/对照型全高文本区（如翻译原文/译文、日志）**：
   - 放在独立的 `Grid` `Height="*"` 行中；
   - 显式声明 `VerticalAlignment="Stretch"`，使其 100% 充满该分区的所有可用空间；
   - 随着主窗口拉伸/缩放，文本框完全同步拉大，内容一览无余，绝无多余僵死空白。

### 11.2 正确写法

```xml
<!-- 场景 A：对照视图 / 专用分栏（拉伸充满 * 行，最推荐） -->
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="*" />     <!-- 原文区自适应填充 -->
        <RowDefinition Height="Auto" />  <!-- 操作栏 -->
        <RowDefinition Height="*" />     <!-- 译文区自适应填充 -->
    </Grid.RowDefinitions>

    <TextBox Grid.Row="0"
             Style="{StaticResource TextBox.Multiline}"
             VerticalAlignment="Stretch" />

    <TextBox Grid.Row="2"
             Style="{StaticResource TextBox.Multiline}"
             IsReadOnly="True"
             VerticalAlignment="Stretch" />
</Grid>

<!-- 场景 B：嵌入表单中的普通多行输入（自然高度，无死锁） -->
<TextBox Style="{StaticResource TextBox.Multiline}"
         VerticalAlignment="Top"
         Height="Auto" />
```

```

`Layout.LogMinHeight` / `Layout.LogMaxHeight` 默认是 120 / 240。

### 11.3 错误写法

```xml
<!-- 错误 1：StackPanel 里 Stretch 不起作用，MinHeight 过大仍会撑高 -->
<StackPanel>
    <TextBox AcceptsReturn="True" VerticalAlignment="Stretch" MinHeight="300" />
</StackPanel>

<!-- 错误 2：Grid 的 * 行里放表单输入框，默认 Stretch 会填满整行 -->
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>
    <TextBox AcceptsReturn="True" />
</Grid>

<!-- 错误 3：Height 不支持 * -->
<TextBox Height="*" />
```

### 11.4 Grid 行分配原则

- 表单行用 `Auto`。
- 只给一个“真正需要填充”的区域用 `*`：列表、表格、日志、主内容 ScrollViewer。
- 不要同时给多个表单行 `*`；需要上下分栏时用 `GridSplitter`。
- 如果一个 `*` 行里放的不是列表/日志/滚动容器，而是普通输入框，几乎一定是布局错误。

### 11.5 自动检查

```powershell
pwsh ./tools/LayoutCheck.ps1 -Root "D:\your\project"
```

脚本会提示：

- 控件上的 `Height="*"`；
- 多行 `TextBox` 缺少 `MinHeight`/`MaxHeight`；
- `TextWrapping="Wrap"` 的 `TextBlock` 使用 `VerticalAlignment="Stretch"`；
- 嵌套 `ScrollViewer`。

---

## 12. 布局反模式

| 反模式 | 问题 | 替代 |
|---|---|---|
| 固定 `Width`/`Height` 包住长文本 | 高 DPI/长语言被裁切 | `MinWidth` + `*` 列 + `TextWrapping` |
| 嵌套 ScrollViewer | 滚轮争夺、双滚动条 | 只保留一层垂直滚动 |
| 表单标签右对齐且宽度不一 | 视觉锯齿 | 固定标签列宽，左对齐 |
| 按钮全部 `Button.Primary` | 没有主次 | 一个 Primary，其余 Secondary/Text |
| 用 `Margin` 堆间距 | 不一致、难维护 | 统一 `Layout.*` / 8 的倍数 |
| 表格里放长段落 | 行高爆炸 | 截断 + ToolTip/详情面板 |
| 用 `Opacity` 做禁用 | 文字看不清 | 显式禁用色 |
| 用颜色作为唯一状态 | 色盲/灰度不可辨 | 颜色 + 图标/文字/边框 |
| **多重卡片嵌套与双重边框** (`Border` 套 `TextBox`) | 灰色框套灰色框、外大内小、视觉层次混乱、空间利用率低下 | 去除外层冗余边框卡片，让多行输入控件自带标准边框并填充可用区域 |
| **操作行为与内容区域割裂** | 核心操作（如“翻译”）与结果处理（如“复制译文/关闭”）分散在不同极端角落，视线大幅跳跃 | 将上下文关联的操作紧随其对应的卡片（例如卡片顶部标题右侧放置工具按钮，底部统一放置流程主次操作） |
| **平铺式工具栏全为次要按钮** | 按钮权重扁平无主次、无视觉焦点，破坏性操作（如“删除选中”）混在常规操作中容易误触 | 确立主动作（如“+ 新增词条” Primary）、次动作（导入/导出）、危险动作（“删除选中”使用 `Button.Danger` 或与其它操作拉开间距），搜索栏前置放大镜图标并带清除键 |
| **上下对照型布局高度失衡/固定塌陷** | 原文区和译文区高度不同步、或内部内容没有垂直拉伸，产生大量难看的灰底空白 | 原文与译文在 Grid 中均分 `Height="*"` 行并保持联动滚动，或者两列左右分栏对照 |

---

## 13. 对照翻译与工具栏经典模式规范

### 13.1 双区域对照输入（翻译、比对）布局规范

对于类似 CAD 文本翻译、图纸前后对比等典型“原文/译文”双文本交互：
1. **统一容器层次**：
   - 使用统一的外层分块（或 Tab/左右分栏），禁止出现 `Border` (外灰框) 内套带有大白底 Padding 的二次 `TextBox` 形成的“框中框”。
   - 原文与译文区域在纵向拉伸时应等比例（`RowDefinition Height="*"`），文本框自身设置 `VerticalAlignment="Stretch"`。
2. **操作锚定契约**：
   - 原文转换操作（如“翻译”按钮）与目标语言下拉框组成统一的控制条，居中或紧随原文下方。
   - 结果区域（译文）的专属操作（如“复制译文”）应直接内置在译文卡片的右上角快捷栏（`Button.Icon` + ToolTip 或文字按钮），而不是扔到远离窗口的底部角落与“关闭”按钮混在一起。

```xml
<!-- 推荐的双区域对照布局骨架 -->
<Grid Margin="{StaticResource Layout.WindowPadding}">
    <Grid.RowDefinitions>
        <RowDefinition Height="*" />     <!-- 原文区 -->
        <RowDefinition Height="Auto" />  <!-- 中间操作工具栏 -->
        <RowDefinition Height="*" />     <!-- 译文区 -->
        <RowDefinition Height="Auto" />  <!-- 底部关闭栏 -->
    </Grid.RowDefinitions>

    <!-- 1. 原文卡片 -->
    <GroupBox Grid.Row="0" Header="原文">
        <TextBox Style="{StaticResource TextBox.Multiline}"
                 VerticalAlignment="Stretch"
                 AcceptsReturn="True"
                 TextWrapping="Wrap" />
    </GroupBox>

    <!-- 2. 中间流向操作栏 -->
    <StackPanel Grid.Row="1" Orientation="Horizontal" HorizontalAlignment="Center" Margin="0,8">
        <TextBlock Text="翻译为" Style="{StaticResource Text.Label}" Margin="0,0,8,0" VerticalAlignment="Center" />
        <ComboBox Width="130" SelectedIndex="0" Margin="0,0,12,0" />
        <Button Style="{StaticResource Button.Primary}" Width="100" Margin="0,0,8,0">
            <StackPanel Orientation="Horizontal">
                <Path Data="{StaticResource Icon.Geo.Refresh}" Style="{StaticResource Icon.Regular}" Margin="0,0,6,0" />
                <TextBlock Text="开始翻译" VerticalAlignment="Center" />
            </StackPanel>
        </Button>
    </StackPanel>

    <!-- 3. 译文卡片 (右上角自带复制工具) -->
    <GroupBox Grid.Row="2" Header="译文">
        <Grid>
            <TextBox Style="{StaticResource TextBox.Multiline}"
                     IsReadOnly="True"
                     VerticalAlignment="Stretch"
                     TextWrapping="Wrap" />
            <Button Style="{StaticResource Button.Icon}"
                    HorizontalAlignment="Right" VerticalAlignment="Top"
                    Margin="0,6,6,0"
                    ToolTip="复制译文到剪切板">
                <Path Data="{StaticResource Icon.Geo.Copy}" Style="{StaticResource Icon.Regular}" />
            </Button>
        </Grid>
    </GroupBox>

    <!-- 4. 底部右下角退出 -->
    <StackPanel Grid.Row="3" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,12,0,0">
        <Button Content="关闭" IsCancel="True" MinWidth="72" />
    </StackPanel>
</Grid>
```

### 13.2 列表/表格顶部工具栏排版规范

对于类似术语词条、图纸列表上方的综合工具栏：
1. **左侧搜索、右侧操作**：
   - 左侧为筛选/搜索区：搜索框集成搜索图标（`Icon.Geo.Search`）及清空按钮，宽度 240–320px。
   - 右侧为动作区（Action Bar）：主次分明，严禁清一色 Secondary 按钮。
2. **按钮视觉层级划分**：
   - **主动作（1 个）**：如“新增词条”，使用 `Button.Primary`（搭配 `Icon.Geo.Plus`）。
   - **次要批量操作**：如“导入 CSV”、“导出 CSV”，使用 `Button.Secondary`（搭配导入/导出图标）。
   - **破坏性/危险操作**：如“删除选中”，使用 `Button.Danger`（悬停浅红底深红字，搭配 `Icon.Geo.Trash`），且应与常规操作保持至少 12–16px 间隔或单独分组，防止误点。

```xml
<!-- 推荐的标准工具栏骨架 -->
<Grid Margin="0,0,0,8">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" /> <!-- 搜索框 -->
        <ColumnDefinition Width="*" />    <!-- 弹性间隔 -->
        <ColumnDefinition Width="Auto" /> <!-- 操作按钮群 -->
    </Grid.ColumnDefinitions>

    <!-- 左侧搜索 -->
    <Grid Width="260">
        <TextBox Padding="26,0,26,0" />
        <Path Data="{StaticResource Icon.Geo.Search}" Style="{StaticResource Icon.Small}"
              HorizontalAlignment="Left" Margin="8,0,0,0" IsHitTestVisible="False" Opacity="0.5" />
        <Button Style="{StaticResource Button.Icon.Small}" HorizontalAlignment="Right" Margin="0,0,2,0" ToolTip="清空搜索">
            <Path Data="{StaticResource Icon.Geo.Close}" Style="{StaticResource Icon.Small}" />
        </Button>
    </Grid>

    <!-- 右侧操作栏 (主次明确，危险隔离) -->
    <StackPanel Grid.Column="2" Orientation="Horizontal">
        <Button Style="{StaticResource Button.Primary}" Margin="0,0,8,0">
            <StackPanel Orientation="Horizontal">
                <Path Data="{StaticResource Icon.Geo.Plus}" Style="{StaticResource Icon.Regular}" Margin="0,0,6,0" />
                <TextBlock Text="新增词条" VerticalAlignment="Center" />
            </StackPanel>
        </Button>
        <Button Style="{StaticResource Button.Secondary}" Margin="0,0,8,0">
            <StackPanel Orientation="Horizontal">
                <Path Data="{StaticResource Icon.Geo.Upload}" Style="{StaticResource Icon.Regular}" Margin="0,0,6,0" />
                <TextBlock Text="导入 CSV" VerticalAlignment="Center" />
            </StackPanel>
        </Button>
        <Button Style="{StaticResource Button.Secondary}" Margin="0,0,16,0">
            <StackPanel Orientation="Horizontal">
                <Path Data="{StaticResource Icon.Geo.Download}" Style="{StaticResource Icon.Regular}" Margin="0,0,6,0" />
                <TextBlock Text="导出 CSV" VerticalAlignment="Center" />
            </StackPanel>
        </Button>

        <!-- 危险操作隔离 -->
        <Button Style="{StaticResource Button.Danger}">
            <StackPanel Orientation="Horizontal">
                <Path Data="{StaticResource Icon.Geo.Trash}" Style="{StaticResource Icon.Regular}" Margin="0,0,6,0" />
                <TextBlock Text="删除选中" VerticalAlignment="Center" />
            </StackPanel>
        </Button>
    </StackPanel>
</Grid>
```

