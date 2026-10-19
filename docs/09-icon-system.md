# 09. 图标设计与使用规范 (Icon System Specification)

在工业 CAD 插件及工程桌面应用（如 OpenCADTranslate、EngineeringAI、auto_inventor 等）中，图标用于提供直观的功能认知、状态指示和工具交互。

本规范规定图标的统一尺寸阶梯、栅格比例、几何路径标准、动态着色机制以及交互控件封装规范，彻底杜绝图标变形、边缘发虚、明暗主题失控、按钮白底白字或深底黑标等可用性问题。

---

## 1. 图标尺寸规范 (Icon Sizing Hierarchy)

所有图标元素必须基于严格的固定尺寸阶梯，禁止任意书写宽高（如 13、17、23 等奇偶不齐尺寸）。

| 令牌名 (Token Key) | 尺寸 (px) | 适用场景 | 说明 / 对应宿主 |
|---|---|---|---|
| `Icon.Size.Micro` | 12×12 | 徽章标记、树节点展开折叠点、行内极小状态指示点 | 极小空间，不表达复杂几何 |
| `Icon.Size.Small` | 14×14 | 行内标签、表格行紧凑操作、单行输入框辅助图标 | 配合 Caption 字体 |
| `Icon.Size.Regular` | 16×16 | **默认尺寸**：标准按钮图标、菜单项、Tab 标头、树节点 | 配合标准 13.5px Body 字体 |
| `Icon.Size.Medium` | 20×20 | 工具栏大图标、分组标题栏操作按钮、快捷操作条 | 搭配 30px / 34px 高度控件 |
| `Icon.Size.Large` | 24×24 | 大尺寸功能入口、通知卡片、重要警告提示框 | 对齐 24px 网格视口 |
| `Icon.Size.Hero` | 32×32 | 引导对话框、空状态 (Empty State)、首页导航卡片 | 视觉焦点中心 |

---

## 2. 几何坐标系与网格约束 (Coordinate Grid & Vector Constraint)

### 2.1 统一 24×24 原生设计栅格
- 所有矢量图标的 Path 几何图形（`Geometry`）必须基于 **24×24** 的逻辑坐标系进行绘制。
- 在通过 WPF 呈现时，外层控件统一设置：
  ```xml
  <Viewbox Width="{StaticResource Icon.Size.Regular}"
           Height="{StaticResource Icon.Size.Regular}">
      <Path Data="{StaticResource Icon.Geo.Settings}"
            Fill="{Binding RelativeSource={RelativeSource AncestorType=TextElement}, Path=(TextElement.Foreground)}" />
  </Viewbox>
  ```
  或者直接使用封装好的 `Style="{StaticResource Icon.Base}"`：
  ```xml
  <Path Data="{StaticResource Icon.Geo.Settings}" Style="{StaticResource Icon.Regular}" />
  ```

### 2.2 抗锯齿与像素对齐 (Pixel Snapping)
- 图标渲染必须强制开启像素对齐和布局舍入，防止在高 DPI 或常规屏下边缘发虚（Subpixel Blurring）：
  ```xml
  UseLayoutRounding="True"
  SnapsToDevicePixels="True"
  ```
- 图标如果使用轮廓线（Stroke），描边粗细标准为 **1.5px 或 2px**，端点与拐角必须设为平滑/圆形：
  ```xml
  StrokeThickness="2"
  StrokeStartLineCap="Round"
  StrokeEndLineCap="Round"
  StrokeLineJoin="Round"
  ```
- 优先推荐 **闭合填充矢量图形（Fill-based Paths）**，其在 Viewbox 缩放时具备最佳跨分辨率保真度。

---

## 3. 图标着色规则 (Icon Coloring & Token Binding)

### 3.1 核心原则：图标颜色严禁写死 HEX
在任何 XAML 业务代码或控件模板中，**禁止出现 `<Path Fill="#333333" />` 或 `Fill="Black"`**！

### 3.2 动态前景继承契约 (R-ICON-1)
所有无固有语义的常规单色图标，其 `Fill` 必须动态绑定到最近父级上下文的 `TextElement.Foreground`：
```xml
<!-- 图标样式默认绑定规则 -->
<Setter Property="Fill"
        Value="{Binding Path=(TextElement.Foreground),
                        RelativeSource={RelativeSource Self},
                        FallbackValue={StaticResource Brush.Text.Primary}}" />
```
**继承效果**：
- 放在**主按钮（Button.Primary）**中时，由于按钮前景是 `Brush.OnAccent`（纯白），图标自动变为纯白色，绝不出现“蓝底黑标”；
- 放在**次要按钮（Button.Secondary）**中时，悬停变为深色前景，禁用变为 `Brush.Text.Disabled`；
- 放在**列表选中项（DataGrid / ListBox Selected）**中时，自动随选中前景切换。

### 3.3 语义图标着色契约 (R-ICON-2)
状态通知、校验提示等具有固定业务含义的图标，使用对应的语义画刷：
- **Success (成功)**: `Brush.Success` (#15803D)
- **Warning (警告)**: `Brush.Warning` (#B45309)
- **Danger / Error (错误/危险)**: `Brush.Danger` (#B91C1C)
- **Info / Accent (提示/品牌)**: `Brush.Accent` (#1765AD)
- **Muted (弱化辅助)**: `Brush.Text.Tertiary` (#9CA3AF)

---

## 4. 图标交互控件规范 (Interactive Icon Controls)

### 4.1 图标按钮尺寸矩阵 (IconButton Sizes)
独立图标按钮（如窗口关闭按钮、工具栏操作、搜索栏清除按钮）须严格遵守以下正方形尺寸规格：

| 样式名 | 按钮尺寸 | 图标尺寸 | 内边距 (Padding) | 圆角 | 适用场景 |
|---|---|---|---|---|---|
| `Button.Icon.Small` | 24×24 | 14×14 | 5 | 2px | 表格行内小操作、搜索框尾部清除 |
| `Button.Icon` (默认) | 28×28 | 16×16 | 6 | 4px | 标准工具条按钮、属性面板操作 |
| `Button.Icon.Large` | 34×34 | 20×20 | 7 | 4px | 顶部大工具栏、侧边栏导航入口 |

### 4.2 图标按钮交互状态规范 (R-ICON-3)
图标按钮默认应为“幽灵形态”（Ghost/Transparent），当鼠标悬停或激活时才显示底色微反馈：
- **Normal (默认态)**: 背景 `Transparent`，前景 `Brush.Text.Secondary`，边框 `Transparent`。
- **Hover (悬停态)**: 背景 `Brush.Control.Hover` (#EAEAEA)，前景 `Brush.Text.Primary`。
- **Pressed (按下态)**: 背景 `Brush.Control.Pressed` (#DFDFDF)，前景 `Brush.Text.Primary`。
- **Disabled (禁用态)**: 背景 `Transparent`，前景 `Brush.Text.Disabled` (#9CA3AF)，光标 `Arrow`。
- **Checked (切换激活态)**: 背景 `Brush.Accent.Subtle` (#E8F2FA)，前景 `Brush.Accent`，边框 `Brush.Accent`。

### 4.3 必须提供 ToolTip 无障碍访问 (R-ICON-4)
纯图标按钮（没有文字说明）**必须显式提供 `ToolTip`**，严禁出现没有任何文字提示的纯图标交互：
```xml
<!-- 正确示范 -->
<Button Style="{StaticResource Button.Icon}" ToolTip="刷新当前模型 (F5)">
    <Path Data="{StaticResource Icon.Geo.Refresh}" Style="{StaticResource Icon.Regular}" />
</Button>
```

---

## 5. 图标与文本排版间距 (Icon-Text Layout Rhythm)

当图标与文本同时出现在按钮、菜单项或状态条中时：
1. **水平间距（Gap）**：
   - 14px 图标搭配正文：间隙 `4px` 或 `6px`（`Margin="0,0,6,0"`）。
   - 16px 图标搭配正文：标准间隙 **`6px`**。
   - 20px+ 图标搭配标题：间距 **`8px`**。
2. **垂直对齐（Alignment）**：
   - 图标与文本必须显式设置 `VerticalAlignment="Center"`。
   - 文本块设置 `VerticalAlignment="Center"` 确保基线对齐。

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

---

## 6. 图标资源清单 (Icon Geometry Registry)

所有几何 Path 保存在 `Theme/Icons.xaml`，以 `Icon.Geo.*` 为唯一前缀。包含但不限于以下通用几何：

1. **通用操作**:
   - `Icon.Geo.Search` (搜索)
   - `Icon.Geo.Close` (关闭/清除)
   - `Icon.Geo.Check` (勾选/成功)
   - `Icon.Geo.Plus` (添加/新建)
   - `Icon.Geo.Edit` (编辑/修改)
   - `Icon.Geo.Trash` (删除/清空)
   - `Icon.Geo.Refresh` (刷新/同步)
   - `Icon.Geo.Settings` (设置/配置)
   - `Icon.Geo.Filter` (筛选/过滤)
   - `Icon.Geo.Download` (下载/导出)
   - `Icon.Geo.Upload` (上传/导入)
   - `Icon.Geo.Copy` (复制/剪切板)
   - `Icon.Geo.Folder` (文件夹/打开)
   - `Icon.Geo.File` (单文件)

2. **方向与折叠**:
   - `Icon.Geo.ChevronDown` / `ChevronUp` / `ChevronLeft` / `ChevronRight`
   - `Icon.Geo.ArrowLeft` / `ArrowRight`

3. **状态反馈**:
   - `Icon.Geo.Info` (信息提示 - 圆圈 i)
   - `Icon.Geo.SuccessCircle` (成功完成 - 圆圈勾)
   - `Icon.Geo.WarningTriangle` (警告注意 - 三角叹号)
   - `Icon.Geo.ErrorCircle` (错误阻断 - 圆圈叉)

---

## 7. 违规反模式检查清单 (Pitfalls to Avoid)

- ❌ **直接使用 PNG/Bitmap 作为小图标**：在 125%/150%/200% Windows 系统缩放下必定发虚或产生黑边。统一使用矢量 `StreamGeometry`。
- ❌ **硬编码 Fill="#..."**：导致主题切换或按钮反色时图标直接“隐形”。
- ❌ **没有设置 Stretch="Uniform"**：导致长宽比例不一的路径拉伸走样。
- ❌ **纯图标按钮没有悬停反馈或无 ToolTip**：用户无法得知功能与点击状态。
- ❌ **图标与文字紧贴（Margin="0"）**：视觉粘连，显得拥挤粗糙。
