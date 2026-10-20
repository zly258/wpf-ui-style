# 09. 图标设计与使用规范 (Google Material Symbols Standard)

在工业 CAD 插件及工程桌面应用（如 OpenCADTranslate、EngineeringAI、auto_inventor 等）中，图标用于提供直观的功能认知、状态指示和工具交互。

本系统**全面采用 Google Material Symbols 官方图标体系**（开源许可证：**Apache License 2.0**，允许自由商用、修改与分发）：
- 统一基于 **24×24 视口网格（Optical Size 24）**；
- 统一采用 **Outlined / Weight 400 / Grade 0**：既不是 200 的细体（小尺寸下发虚、看不清），也不是 700 的粗体（视觉过重、抢文字焦点）；
- 图标通过矢量 `StreamGeometry` 引入，并通过 WPF 动态画刷绑定，保证在明暗主题、主按钮（蓝底纯白）、禁用态（自动置灰）以及悬停态下 100% 正确着色。

### 视觉重量基线（Weight 400）

| 维度 | 取值 | 说明 |
|---|---|---|
| 风格 | Outlined | 与工程软件界面克制、清晰的气质一致；避免 Filled 变体在小尺寸下糊成一团 |
| 字重 | 400 | 中间档，笔画与 14.5px 正文、16px 复选框边框等粗 |
| Grade | 0 | 不额外加粗/减细 |
| Optical Size | 24 | 与 16/20/24px 三档渲染尺寸对齐 |

> 禁止在同一界面混用 Outlined 与 Filled、混用不同字重，或引入 200/700 的图标变体。
> 线型控件（复选框勾、下拉箭头、折叠箭头）的描边统一为 **1.5–1.6px**，与图标字重视觉等重。



---

## 1. 图标尺寸规范 (Icon Sizing Hierarchy)

所有图标元素必须基于严格的固定尺寸阶梯，禁止任意书写宽高（如 13、17、23 等奇偶不齐尺寸）。

| 令牌名 (Token Key) | 尺寸 (px) | 适用场景 | 说明 / 对应宿主 |
|---|---|---|---|
| `Icon.Size.Micro` | 12×12 | 徽章标记、树节点展开折叠点、行内极小状态指示点 | 极小空间，不表达复杂几何 |
| `Icon.Size.Small` | 14×14 | 行内标签、表格行紧凑操作、单行输入框辅助图标 | 配合 Caption 字体 |
| `Icon.Size.Regular` | 16×16 | **默认尺寸**：标准按钮图标、菜单项、Tab 标头、树节点 | 配合 14.5px Body 字体 |
| `Icon.Size.Medium` | 20×20 | 工具栏大图标、分组标题栏操作按钮、快捷操作条 | 搭配 32px / 36px 高度控件 |
| `Icon.Size.Large` | 24×24 | 大尺寸功能入口、通知卡片、重要警告提示框 | 对齐 24px 网格视口 |
| `Icon.Size.Hero` | 32×32 | 引导对话框、空状态 (Empty State)、首页导航卡片 | 视觉焦点中心 |

---

## 2. 几何坐标系与网格约束 (Coordinate Grid & Vector Constraint)

### 2.1 统一 24×24 原生设计栅格
- 所有矢量图标的 Path 几何图形（`Geometry`）必须基于 **24×24** 的逻辑坐标系进行绘制。
- 官方 SVG 使用 `viewBox="0 -960 960 960"`，已统一线性映射为 0..24 正向坐标。
- 每条几何末尾附加**零面积锚点** `M0,0 L24,24`，确保 `Stretch="Uniform"` 始终按完整 24×24 视口等比缩放；否则 Chevron 这类小图形会被放大到与 Settings 同宽，造成光学尺寸不一致。
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
- 线型控件（非图标几何）的描边标准为 **1.5px（小尺寸）或 1.6px（16px 复选框勾）**，端点与拐角必须平滑：
  ```xml
  StrokeThickness="1.5"
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
| `Button.Icon.Small` | 26×26 | 14×14 | 5 | 2px | 表格行内小操作、搜索框尾部清除 |
| `Button.Icon` (默认) | 30×30 | 16×16 | 7 | 4px | 标准工具条按钮、属性面板操作 |
| `Button.Icon.Large` | 36×36 | 20×20 | 8 | 4px | 顶部大工具栏、侧边栏导航入口 |

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

## 6. 官方图标资源映射清单 (Google Material Symbols Registry)

所有几何 Path 提取自官方 Google Material Symbols (Apache 2.0)，保存在 `Theme/Icons.xaml`，以 `Icon.Geo.*` 为统一 XAML 键名：

| XAML 资源键名 | Material Symbol 官方原名 | 分类 | 用途 |
|---|---|---|---|
| `Icon.Geo.Search` | `search` | Action | 搜索框前缀、查找 |
| `Icon.Geo.Close` | `close` | Action | 弹窗关闭、输入框一键清除 |
| `Icon.Geo.Check` | `check` | Action | 确认、已选完成 |
| `Icon.Geo.Plus` | `add` | Action | 新增词条、新建图纸 |
| `Icon.Geo.Edit` | `edit` | Action | 编辑详情、修改行 |
| `Icon.Geo.Trash` | `delete` | Action | 删除记录、清空 |
| `Icon.Geo.Refresh` | `refresh` | Action | 刷新数据、重新翻译 |
| `Icon.Geo.Settings` | `settings` | Action | 系统设置、首选项 |
| `Icon.Geo.Filter` | `filter_alt` | Action | 列表数据过滤 |
| `Icon.Geo.Download` | `download` | Action | 导出 CSV、保存文件 |
| `Icon.Geo.Upload` | `upload` | Action | 导入 CSV、上传图纸 |
| `Icon.Geo.Copy` | `content_copy` | Action | 一键复制译文/文本 |
| `Icon.Geo.Folder` | `folder` | File | 打开文件夹/目录选择 |
| `Icon.Geo.File` | `description` | File | 图纸文件、单条文档 |
| `Icon.Geo.Save` | `save` | Action | 保存设置/导出结果 |
| `Icon.Geo.Translate` | `translate` | Action | 翻译/语言切换 |
| `Icon.Geo.Play` | `play_arrow` | Media | 开始/运行任务 |
| `Icon.Geo.Pause` | `pause` | Media | 暂停任务 |
| `Icon.Geo.Stop` | `stop` | Media | 停止任务 |
| `Icon.Geo.ZoomIn` | `zoom_in` | View | 放大视图/预览 |
| `Icon.Geo.ZoomOut` | `zoom_out` | View | 缩小视图/预览 |
| `Icon.Geo.Visibility` | `visibility` | View | 显示/预览密码或图层 |
| `Icon.Geo.ChevronDown` | `expand_more` | Navigation | 树节点/折叠面板展开 |
| `Icon.Geo.ChevronUp` | `expand_less` | Navigation | 树节点/折叠面板收起 |
| `Icon.Geo.ChevronLeft` | `chevron_left` | Navigation | 上一页、后退 |
| `Icon.Geo.ChevronRight` | `chevron_right` | Navigation | 下一页、前进 |
| `Icon.Geo.MoreVert` | `more_vert` | Navigation | 更多操作菜单 |
| `Icon.Geo.DragIndicator` | `drag_indicator` | Navigation | 列表拖拽排序手柄 |
| `Icon.Geo.Info` | `info` | Semantic | 规则说明、版本信息提示 |
| `Icon.Geo.SuccessCircle` | `check_circle` | Semantic | 校验成功、导出完成 |
| `Icon.Geo.WarningTriangle` | `warning` | Semantic | 命名冲突、参数警告 |
| `Icon.Geo.ErrorCircle` | `cancel` | Semantic | 解析失败、严重错误阻断 |

### 6.1 如何扩展引入新的 Material Symbols 图标？
1. 访问官方图标库：[Google Fonts / Material Symbols](https://fonts.google.com/icons)；
2. 固定选择 **Outlined / Weight 400 / Grade 0 / Optical Size 24**（保持与现有图标等重）；
3. 复制其 24px SVG 中 `<path d="..." />` 的路径数据；
4. 使用 `tools/_fetch_material_icons.py` 的坐标转换（viewBox `0 -960 960 960` → 0..24），并在末尾追加 `M0,0 L24,24` 锚点；
5. 在 `Theme/Icons.xaml` 中声明 `<StreamGeometry x:Key="Icon.Geo.新名称">…</StreamGeometry>` 即可全工程复用。



---

## 7. 违规反模式检查清单 (Pitfalls to Avoid)

- ❌ **直接使用 PNG/Bitmap 作为小图标**：在 125%/150%/200% Windows 系统缩放下必定发虚或产生黑边。统一使用矢量 `StreamGeometry`。
- ❌ **硬编码 Fill="#..."**：导致主题切换或按钮反色时图标直接“隐形”。
- ❌ **没有设置 Stretch="Uniform"**：导致长宽比例不一的路径拉伸走样。
- ❌ **纯图标按钮没有悬停反馈或无 ToolTip**：用户无法得知功能与点击状态。
- ❌ **图标与文字紧贴（Margin="0"）**：视觉粘连，显得拥挤粗糙。
- ❌ **混用 Filled 与 Outlined、或混用 200/700 字重**：同一界面出现“有的粗、有的细”，破坏节奏。统一 Outlined / 400。
- ❌ **图标过粗**：粗体图标在小尺寸下糊成一团，并抢走文字焦点；主按钮中尤其明显。
- ❌ **图标过细**：细体（200）在 16px 与 125% DPI 下笔画断裂、发灰难辨。
- ❌ **缺少 0..24 边界锚点**：`Stretch="Uniform"` 会把 Chevron 等小图形放大，导致同一行的图标大小不一。
- ❌ **线型控件描边随意**：复选框勾、下拉箭头、折叠箭头必须统一 1.5–1.6px，避免与图标字重不匹配。

