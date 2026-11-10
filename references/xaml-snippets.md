# 可复制 XAML 片段

## 1. 合并主题与选择密度

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- 中等（默认）：Theme.xaml -->
            <!-- 紧凑：Theme.Compact.xaml -->
            <!-- 宽松：Theme.Relaxed.xaml -->
            <ResourceDictionary
                Source="pack://application:,,,/YourAssembly;component/Theme/Theme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

运行时热切换（控件全部使用 `{DynamicResource Layout.*}`）：

```csharp
static void ApplyDensity(string profile) // Compact / Medium / Relaxed
{
    var dict = new ResourceDictionary
    {
        Source = new Uri("pack://application:,,,/YourAssembly;component/Theme/Density." +
                         profile + ".xaml", UriKind.Absolute)
    };
    var merged = Application.Current.Resources.MergedDictionaries;
    for (int i = merged.Count - 1; i >= 0; i--)
        if (merged[i].Source?.OriginalString.Contains("/Density.") == true)
            merged.RemoveAt(i);
    merged.Add(dict);
}
```

> 业务 XAML 用 `{StaticResource Layout.*}`，主题内部用 `{DynamicResource Layout.*}`。
> 三档键值与选型见 `docs/06-density.md`。

## 2. 按钮

```xml
<Button Content="取消" />
<Button Content="保存" Style="{StaticResource Button.Primary}" IsDefault="True" />
<Button Content="删除" Style="{StaticResource Button.Danger}" />
<Button Content="更多" Style="{StaticResource Button.Text}" />
<Button Content="小按钮" Style="{StaticResource Button.Primary.Small}" />
```

## 3. 输入框

```xml
<!-- 单行 -->
<TextBox Text="{Binding Name}" />

<!-- 多行表单：不要撑满 -->
<TextBox Style="{StaticResource TextBox.Multiline}"
         Height="Auto"
         VerticalAlignment="Top"
         Text="{Binding Notes}" />

<!-- 只读日志：允许填充 -->
<TextBox Grid.Row="1"
         Style="{StaticResource TextBox.Code}"
         MaxHeight="240"
         VerticalAlignment="Stretch"
         Text="{Binding Log}" />
```

## 4. 表单两列

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
    <TextBox Grid.Column="1" Margin="0,4" Text="{Binding Endpoint}" />

    <TextBlock Grid.Column="3" Text="模型名称" Style="{StaticResource Text.Label}" />
    <TextBox Grid.Column="4" Margin="0,4" Text="{Binding Model}" />

    <TextBlock Grid.Row="1" Text="访问令牌" Style="{StaticResource Text.Label}" />
    <PasswordBox Grid.Row="1" Grid.Column="1" Margin="0,4" />
</Grid>
```

## 5. ComboBox

```xml
<!-- 基础下拉框 -->
<ComboBox SelectedIndex="0">
    <ComboBoxItem Content="替换原文" />
    <ComboBoxItem Content="追加译文" />
</ComboBox>

<!-- 可编辑下拉框（支持键盘自由输入或选择现有项） -->
<ComboBox IsEditable="True" Text="可键入内容">
    <ComboBoxItem Content="推荐选项一" />
    <ComboBoxItem Content="推荐选项二" />
</ComboBox>
```

## 6. 复选框组

```xml
<GroupBox Header="翻译范围">
    <WrapPanel>
        <CheckBox Content="单行文本" Width="130" />
        <CheckBox Content="多行文本" Width="130" />
        <CheckBox Content="引线文本" Width="130" />
        <CheckBox Content="块属性" Width="130" />
    </WrapPanel>
</GroupBox>
```

## 7. ListBox

```xml
<ListBox>
    <ListBoxItem Content="第一项" />
    <ListBoxItem Content="第二项" IsSelected="True" />
</ListBox>
```

## 8. DataGrid

```xml
<DataGrid AutoGenerateColumns="False"
          HeadersVisibility="Column"
          GridLinesVisibility="Horizontal"
          CanUserResizeRows="False">
    <DataGrid.Columns>
        <DataGridTextColumn Header="原文" Binding="{Binding Source}" Width="*" />
        <DataGridTextColumn Header="译文" Binding="{Binding Target}" Width="*" />
    </DataGrid.Columns>
</DataGrid>
```

## 9. 日志区

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="*" />
        <RowDefinition Height="Auto" />
    </Grid.RowDefinitions>

    <TextBlock Grid.Row="0" Style="{StaticResource Text.Caption}"
               Text="处理日志" Margin="0,0,0,6" />

    <TextBox Grid.Row="1"
             Style="{StaticResource TextBox.Code}"
             MaxHeight="240"
             VerticalAlignment="Stretch" />

    <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Right"
                Margin="0,12,0,0">
        <Button Content="取消" IsCancel="True" Margin="0,0,8,0" />
        <Button Content="开始" Style="{StaticResource Button.Primary}" />
    </StackPanel>
</Grid>
```

## 10. 高级设置 Expander

```xml
<Expander Header="高级设置" IsExpanded="False">
    <StackPanel>
        <TextBlock Style="{StaticResource Text.Caption}"
                   Text="以下参数一般保持默认即可。" Margin="0,0,0,8" />
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="104" />
                <ColumnDefinition Width="*" />
            </Grid.ColumnDefinitions>
            <TextBlock Text="并发请求数" Style="{StaticResource Text.Label}" />
            <TextBox Grid.Column="1" Margin="0,4" Text="4" />
        </Grid>
    </StackPanel>
</Expander>
```

## 11. 正确的内容宿主前景

```xml
<ContentPresenter TextElement.Foreground="{TemplateBinding Foreground}"
                  RecognizesAccessKey="True" />
```

## 12. 隐式文本前景契约

```xml
<Style TargetType="{x:Type TextBlock}">
    <Setter Property="Foreground"
            Value="{Binding Path=(TextElement.Foreground),
                    RelativeSource={RelativeSource AncestorType={x:Type ContentPresenter}},
                    FallbackValue={StaticResource Brush.Text.Primary}}" />
</Style>

<Style TargetType="{x:Type AccessText}">
    <Setter Property="Foreground"
            Value="{Binding Path=(TextElement.Foreground),
                    RelativeSource={RelativeSource AncestorType={x:Type ContentPresenter}},
                    FallbackValue={StaticResource Brush.Text.Primary}}" />
</Style>
```

## 13. 选中 + 悬停

```xml
<MultiTrigger>
    <MultiTrigger.Conditions>
        <Condition Property="IsSelected" Value="True" />
        <Condition Property="IsMouseOver" Value="True" />
    </MultiTrigger.Conditions>
    <Setter Property="Background" Value="{StaticResource Brush.Selection.Hover}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Primary}" />
</MultiTrigger>
```

## 14. 禁用态

```xml
<Trigger Property="IsEnabled" Value="False">
    <Setter Property="Background" Value="{StaticResource Brush.Control.Disabled}" />
    <Setter Property="Foreground" Value="{StaticResource Brush.Text.Disabled}" />
    <Setter Property="BorderBrush" Value="{StaticResource Brush.Border}" />
    <Setter Property="Cursor" Value="Arrow" />
</Trigger>
```

## 15. 图标与图标按钮规范片段

### 15.1 基础图标使用（继承上下文前景）

```xml
<!-- 标准 16px 图标，自动继承按钮/父级前景 -->
<Path Data="{StaticResource Icon.Geo.Settings}" Style="{StaticResource Icon.Regular}" />
```

### 15.2 图标+文字按钮（留出 6px 间隙）

```xml
<Button Style="{StaticResource Button.Primary}">
    <StackPanel Orientation="Horizontal">
        <Path Data="{StaticResource Icon.Geo.Download}"
              Style="{StaticResource Icon.Regular}"
              Margin="0,0,6,0" />
        <TextBlock Text="导出 DXF 图纸" VerticalAlignment="Center" />
    </StackPanel>
</Button>
```

### 15.3 纯图标操作按钮（必配 ToolTip）

```xml
<Button Style="{StaticResource Button.Icon}" ToolTip="刷新数据 (F5)">
    <Path Data="{StaticResource Icon.Geo.Refresh}" Style="{StaticResource Icon.Regular}" />
</Button>

<!-- 紧凑小号（表格行操作） -->
<Button Style="{StaticResource Button.Icon.Small}" ToolTip="删除此项">
    <Path Data="{StaticResource Icon.Geo.Trash}" Style="{StaticResource Icon.Small}" />
</Button>
```

### 15.4 语义反馈图标

```xml
<!-- 成功提示 -->
<Path Data="{StaticResource Icon.Geo.SuccessCircle}" Style="{StaticResource Icon.Medium.Success}" />
<!-- 警告提示 -->
<Path Data="{StaticResource Icon.Geo.WarningTriangle}" Style="{StaticResource Icon.Medium.Warning}" />
<!-- 错误提示 -->
<Path Data="{StaticResource Icon.Geo.ErrorCircle}" Style="{StaticResource Icon.Medium.Danger}" />
```

## 16. 双栏/对照型窗口标准重构模板（解决大框套小框与操作割裂）

```xml
<Grid Margin="{StaticResource Layout.WindowPadding}">
    <Grid.RowDefinitions>
        <RowDefinition Height="*" />     <!-- 原文区 -->
        <RowDefinition Height="Auto" />  <!-- 中间操作工具条 -->
        <RowDefinition Height="*" />     <!-- 译文区 -->
        <RowDefinition Height="Auto" />  <!-- 底部关闭栏 -->
    </Grid.RowDefinitions>

    <!-- 原文卡片 -->
    <GroupBox Grid.Row="0" Header="原文">
        <TextBox Style="{StaticResource TextBox.Multiline}"
                 VerticalAlignment="Stretch"
                 AcceptsReturn="True"
                 TextWrapping="Wrap" />
    </GroupBox>

    <!-- 中间流向操作栏 (居中、语言选择+主翻译操作紧密一体) -->
    <StackPanel Grid.Row="1" Orientation="Horizontal" HorizontalAlignment="Center" Margin="0,8">
        <TextBlock Text="翻译为" Style="{StaticResource Text.Label}" Margin="0,0,8,0" VerticalAlignment="Center" />
        <ComboBox Width="130" SelectedIndex="0" Margin="0,0,12,0">
            <ComboBoxItem Content="英语 (en)" />
            <ComboBoxItem Content="中文 (zh)" />
        </ComboBox>
        <Button Style="{StaticResource Button.Primary}" Width="100">
            <StackPanel Orientation="Horizontal">
                <Path Data="{StaticResource Icon.Geo.Refresh}" Style="{StaticResource Icon.Regular}" Margin="0,0,6,0" />
                <TextBlock Text="翻译" VerticalAlignment="Center" />
            </StackPanel>
        </Button>
    </StackPanel>

    <!-- 译文卡片 (右上角自带复制工具) -->
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

    <!-- 底部栏 -->
    <StackPanel Grid.Row="3" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,12,0,0">
        <Button Content="关闭" IsCancel="True" />
    </StackPanel>
</Grid>
```

## 17. 列表顶部工具栏标准重构模板（解决全次要按钮与危险操作无警示）

```xml
<Grid Margin="0,0,0,8">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" />
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="Auto" />
    </Grid.ColumnDefinitions>

    <!-- 左侧搜索 (带前缀图标与清除按钮) -->
    <Grid Width="260">
        <TextBox Padding="26,0,26,0" />
        <Path Data="{StaticResource Icon.Geo.Search}" Style="{StaticResource Icon.Small}"
              HorizontalAlignment="Left" Margin="8,0,0,0" IsHitTestVisible="False" Opacity="0.5" />
        <Button Style="{StaticResource Button.Icon.Small}" HorizontalAlignment="Right" Margin="0,0,2,0" ToolTip="清空搜索">
            <Path Data="{StaticResource Icon.Geo.Close}" Style="{StaticResource Icon.Small}" />
        </Button>
    </Grid>

    <!-- 右侧操作栏 (主操作高亮、次操作带标、危险操作红标隔离) -->
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

        <!-- 危险操作隔离与高危警示 -->
        <Button Style="{StaticResource Button.Danger}">
            <StackPanel Orientation="Horizontal">
                <Path Data="{StaticResource Icon.Geo.Trash}" Style="{StaticResource Icon.Regular}" Margin="0,0,6,0" />
                <TextBlock Text="删除选中" VerticalAlignment="Center" />
            </StackPanel>
        </Button>
    </StackPanel>
</Grid>
```

---

## 18. 安全表单窗口骨架（防截断 / 防遮挡）

把「标签不裁、值不藏、按钮不被盖」一次性做对的完整窗口模板。
直接复制改文案即可。

```xml
<Window x:Class="YourApp.Ui.SettingsWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="设置"
        SizeToContent="Height"
        Width="720"
        MinWidth="{StaticResource Layout.WindowMinWidth}"
        MinHeight="320"
        WindowStartupLocation="CenterScreen">

    <Window.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="/YourApp;component/Ui/Theme/Theme.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Window.Resources>

    <Grid Margin="{StaticResource Layout.WindowPadding}">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />  <!-- 说明 -->
            <RowDefinition Height="*" />     <!-- 内容（可滚动） -->
            <RowDefinition Height="Auto" />  <!-- 状态 -->
            <RowDefinition Height="Auto" />  <!-- 按钮 -->
        </Grid.RowDefinitions>

        <TextBlock Grid.Row="0" Style="{StaticResource Text.Caption}"
                   Text="这些值会保存到配置文件。"
                   TextWrapping="Wrap" Margin="0,0,0,10" />

        <!-- 内容区：整窗唯一的一层垂直滚动 -->
        <ScrollViewer Grid.Row="1"
                      VerticalScrollBarVisibility="Auto"
                      HorizontalScrollBarVisibility="Disabled">
            <Grid>
                <Grid.ColumnDefinitions>
                    <!-- 标签列自适应 + 最小宽度，长标签不会被裁 -->
                    <ColumnDefinition Width="Auto" MinWidth="{StaticResource Layout.FormLabelMinWidth}" />
                    <ColumnDefinition Width="*" MinWidth="{StaticResource Layout.InputMinWidth}" />
                    <ColumnDefinition Width="{StaticResource Layout.ColumnGap}" />
                    <ColumnDefinition Width="Auto" MinWidth="{StaticResource Layout.FormLabelMinWidth}" />
                    <ColumnDefinition Width="*" MinWidth="{StaticResource Layout.InputMinWidth}" />
                </Grid.ColumnDefinitions>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                </Grid.RowDefinitions>

                <!-- 左列 -->
                <TextBlock Grid.Row="0" Text="文档编号" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="0" Grid.Column="1" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <TextBlock Grid.Row="1" Text="文档版本" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="1" Grid.Column="1" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <TextBlock Grid.Row="2" Text="所属项目" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="2" Grid.Column="1" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <TextBlock Grid.Row="3" Text="编制" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="3" Grid.Column="1" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <TextBlock Grid.Row="4" Text="审核" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="4" Grid.Column="1" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <TextBlock Grid.Row="5" Text="设计" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="5" Grid.Column="1" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <TextBlock Grid.Row="6" Text="制图" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="6" Grid.Column="1" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <TextBlock Grid.Row="7" Text="日期" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="7" Grid.Column="1" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <!-- 长标签：换行展示，不裁切 -->
                <TextBlock Grid.Row="8" Text="文档标题（用于换行演示）"
                           Style="{StaticResource Text.Label}" TextWrapping="Wrap" />
                <TextBox Grid.Row="8" Grid.Column="1" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <!-- 右列 -->
                <TextBlock Grid.Row="0" Grid.Column="3" Text="附件1" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="0" Grid.Column="4" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <TextBlock Grid.Row="1" Grid.Column="3" Text="附件2" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="1" Grid.Column="4" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <TextBlock Grid.Row="2" Grid.Column="3" Text="附件3" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="2" Grid.Column="4" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <TextBlock Grid.Row="3" Grid.Column="3" Text="附件4" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="3" Grid.Column="4" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <TextBlock Grid.Row="4" Grid.Column="3" Text="附件5" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="4" Grid.Column="4" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <TextBlock Grid.Row="5" Grid.Column="3" Text="附件6" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="5" Grid.Column="4" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <TextBlock Grid.Row="6" Grid.Column="3" Text="起始编号" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="6" Grid.Column="4" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8"
                         ToolTip="整数；用于连续编号" />

                <TextBlock Grid.Row="7" Grid.Column="3" Text="质量等级" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="7" Grid.Column="4" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />

                <TextBlock Grid.Row="8" Grid.Column="3" Text="保密等级" Style="{StaticResource Text.Label}" />
                <TextBox Grid.Row="8" Grid.Column="4" Style="{StaticResource TextBox.Form}"
                         Margin="0,0,0,8" />
            </Grid>
        </ScrollViewer>

        <!-- 状态文案：独占一行，可换行，不挤按钮 -->
        <TextBlock Grid.Row="2" Style="{StaticResource Text.Caption}"
                   Foreground="{StaticResource Brush.Text.Secondary}"
                   Text="已载入：配置文件"
                   TextWrapping="Wrap" Margin="0,10,0,0" />

        <!-- 按钮栏：独占一行 -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" HorizontalAlignment="Right"
                    Margin="0,10,0,0">
            <Button Content="刷新" Margin="0,0,8,0" />
            <Button Content="保存" Style="{StaticResource Button.Primary}" Margin="0,0,8,0"
                    IsDefault="True" />
            <Button Content="关闭" IsCancel="True" />
        </StackPanel>
    </Grid>
</Window>
```

### 18.1 错误骨架对照

```xml
<!-- Wrong 1：标签列写死宽度 → 长标签被裁 -->
<ColumnDefinition Width="104" />

<!-- Wrong 2：输入框写死宽度 → 窄窗口下被裁/溢出 -->
<TextBox Width="220" />

<!-- Wrong 3：窗口写死高度 + 内容无滚动 → 底部按钮被盖住 -->
<Window Height="470">
    <Grid><StackPanel /></Grid>
</Window>

<!-- Wrong 4：状态文案与按钮同行 → 长路径把按钮挤变形 -->
<Grid>
    <TextBlock Text="已载入：配置文件" />
    <StackPanel Orientation="Horizontal"><!-- 按钮 --></StackPanel>
</Grid>

<!-- Wrong 5：省略号没有 ToolTip → 用户永远看不到全文 -->
<TextBlock Text="{Binding Path}" TextTrimming="CharacterEllipsis" />

<!-- Wrong 6：负 Margin / Canvas 绝对定位 → 控件重叠 -->
<TextBox Margin="-4,0,0,0" />
<Canvas>
    <TextBox Canvas.Left="20" Canvas.Top="12" />
</Canvas>
```

---

## 19. 输入区占满整行（R16）

```xml
<!-- 列用 * + MinWidth；输入框不写 Width -->
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" MinWidth="{StaticResource Layout.FormLabelMinWidth}" />
        <ColumnDefinition Width="*" MinWidth="{StaticResource Layout.InputMinWidth}" />
    </Grid.ColumnDefinitions>
    <TextBlock Text="工程名称" Style="{StaticResource Text.Label}" />
    <TextBox Grid.Column="1" Text="{Binding Name}" />
</Grid>
```

要点：

- 主题模板已把 `PART_ContentHost` 设为 `HorizontalAlignment="Stretch"`；
- 不要设置 `HorizontalContentAlignment="Left"`，否则右侧空白点不进去；
- 需要最大宽度时用 `MaxWidth`，不要写死 `Width`。

---

## 20. 标签页（可见选中态）

```xml
<TabControl>
    <TabItem Header="基本信息">
        <Grid Margin="0,8,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto" MinWidth="{StaticResource Layout.FormLabelMinWidth}" />
                <ColumnDefinition Width="*" MinWidth="{StaticResource Layout.InputMinWidth}" />
            </Grid.ColumnDefinitions>
            <TextBlock Text="工程名称" Style="{StaticResource Text.Label}" />
            <TextBox Grid.Column="1" Text="{Binding Name}" />
        </Grid>
    </TabItem>
    <TabItem Header="高级选项">
        <StackPanel Margin="0,8,0,0">
            <CheckBox Content="启用自动保存" IsChecked="True" />
            <CheckBox Content="记录调试日志" />
        </StackPanel>
    </TabItem>
</TabControl>
```

要点：

- 选中态 = `Brush.Accent` 实心底 + `Brush.OnAccent` 白字 + 1px Accent 描边 + 3px `Brush.Accent.Pressed` 底条 + SemiBold；
- 未选中悬停 = `Brush.Control.Hover` 底 + `Brush.Border` 描边；
- 标签页只做局部切换；整窗多任务仍按 R12 拆窗口。

---

## 20.5 批量操作条（ToolStrip + 选中数徽章）

```xml
<Border Style="{StaticResource ToolStrip}" Margin="0,0,0,8">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="Auto" />
        </Grid.ColumnDefinitions>

        <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
            <Button Content="添加物料" Margin="0,0,8,0" Click="Add_Click" />
            <Button Content="批量审核通过" Margin="0,0,8,0"
                    IsEnabled="False" Click="Approve_Click" />
            <Button Content="批量删除" Style="{StaticResource Button.Danger}"
                    Margin="0,0,10,0" IsEnabled="False" Click="Delete_Click" />

            <Border x:Name="SelectionBadge"
                    Background="{StaticResource Brush.SurfaceAlt}"
                    CornerRadius="{StaticResource Radius.Pill}"
                    Padding="{DynamicResource Layout.BadgePadding}"
                    VerticalAlignment="Center">
                <TextBlock x:Name="SelectedSummary" Text="未选择项"
                           FontSize="{StaticResource FontSize.Caption}"
                           Foreground="{StaticResource Brush.Text.Secondary}" />
            </Border>
        </StackPanel>

        <Button Grid.Column="1" Content="展开全部明细" Click="Toggle_Click" />
    </Grid>
</Border>
```

```csharp
// 勾选数量变化时：点亮按钮 + 徽章换色
bool any = selectedCount > 0;
BtnBatchApprove.IsEnabled = any;
BtnBatchDelete.IsEnabled = any;
SelectedSummary.Text = any ? $"已选中 {selectedCount} 项" : "未选择项";
SelectionBadge.Background = (Brush)FindResource(any ? "Brush.Accent.Subtle" : "Brush.SurfaceAlt");
SelectedSummary.Foreground = (Brush)FindResource(any ? "Brush.Accent" : "Brush.Text.Secondary");
```

## 21. 窄面板单列表单（标签与控件左对齐）

```xml
<ScrollViewer VerticalScrollBarVisibility="Auto"
              HorizontalScrollBarVisibility="Disabled">
    <StackPanel Margin="{StaticResource Layout.WindowPadding}">
        <TextBlock Text="服务连接" Style="{StaticResource Text.Title}" FontWeight="SemiBold" />

        <StackPanel Style="{StaticResource FormStack}">
            <TextBlock Text="服务地址" Style="{StaticResource Text.FieldLabel}" />
            <TextBox Text="{Binding Url}" />
        </StackPanel>

        <StackPanel Style="{StaticResource FormStack}">
            <TextBlock Text="模型名称" Style="{StaticResource Text.FieldLabel}" />
            <TextBox Text="{Binding Model}" />
        </StackPanel>

        <StackPanel Style="{StaticResource FormStack}">
            <TextBlock Text="超时（秒）" Style="{StaticResource Text.FieldLabel}" />
            <TextBox Text="{Binding Timeout}" />
        </StackPanel>
    </StackPanel>
</ScrollViewer>
```

要点：

- 标签与输入框都从左边开始，不依赖左右列宽；
- 适用宿主宽度可能小于 520 DIP 的停靠窗、属性面板、侧栏；
- 宽面板仍用单 Grid 两列，见 `docs/03-layout.md` §17。
