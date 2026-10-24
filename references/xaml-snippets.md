# 可复制 XAML 片段

## 1. 合并主题

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary
                Source="pack://application:,,,/YourAssembly;component/Theme/Theme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

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
<ComboBox SelectedIndex="0">
    <ComboBoxItem Content="替换原文" />
    <ComboBoxItem Content="追加译文" />
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


