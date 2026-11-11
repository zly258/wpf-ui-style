---
name: ribbon-icon-generator
description: Generate, extend, and audit 24x24 monoline ribbon icons for desktop industrial software, CAD/BIM plugins, and IDE toolbars. Ships a 40-icon catalog, a generator for SVG / WPF StreamGeometry / dual-theme preview sheets, a safety-margin + contrast validator, and AI image prompts. Use when a ribbon command needs a new icon, when icons must look consistent at 16-32px in both dark and light themes, or when auditing stroke width, safe area, dashed selection/range marks, and WPF Stroke binding. 适用于：CAD/工业软件 Ribbon 菜单与工具栏极简线性图标、框选/批量/筛选等操作概念、深浅双主题 SVG、WPF 线性图标资源、AI 绘图提示词、图标复杂度与安全边距校验。
---

# Ribbon Icon Generator · 极简线性图标

面向桌面端工业软件 / CAD / BIM 插件 / IDE Ribbon 的单色线性图标体系。
本技能用**一份 JSON 目录 + 一个生成脚本**保证 40 个图标在 16–32px、深色与浅色主题下完全同构：

- 目录：`catalog/icons.json`（唯一数据源，只描述几何，不含颜色）。
- 生成器：`tools/IconGen.py`（SVG / WPF XAML / 预览页 / 校验 / 提示词，仅标准库）。
- 产物：`samples/svg/*.svg`、`samples/RibbonIcons.xaml`、`preview/icons.html`。

> 图标是**工具**，不是插画。任何在 24px 下糊成一团、或在 16px 下丢失主特征的细节，都必须删除。

---

## 1. 何时使用本技能

- 给 Ribbon 组、工具条、右键菜单、属性面板增加一个新的命令图标。
- 现有图标缩小到 16px 后发糊、笔画粗细不一、深浅主题下对比度不足。
- 需要「框选 / 批量 / 筛选 / 阵列 / 约束」这类**软件操作概念**的图形表达，而不是照抄实物。
- 需要把线性图标接入 WPF（`Stroke` 继承前景、`Icon.Size.*` 尺寸阶梯、深色/浅色主题）。
- 需要先让 AI 出一张位图找构图灵感，再落成矢量。

不适用：品牌 Logo、彩色插画、照片式渲染图标、需要多色语义分层的状态图（用语义画刷 + 单色图标解决）。

---

## 2. 视觉规范（硬约束）

| 维度 | 规范 | 校验方式 |
|---|---|---|
| 画布 | `viewBox="0 0 24 24"`，24×24 逻辑网格 | `check` 断言 `grid == 24` |
| 安全边距 | 几何留在 **3..21**（硬底线 2..22，留给 stroke 的 1px 外扩） | `check` 报错/警告 |
| 描边 | 固定 `stroke-width="2"`，等宽，禁止逐形变粗细 | 生成器写死 |
| 端点/转角 | `stroke-linecap="round"`、`stroke-linejoin="round"` | 生成器写死 |
| 圆角 | 矩形 `rx="1"` 或 `rx="2"`，不做半圆角 | 目录值 |
| 颜色 | 单色，仅 `currentColor` 或主题前景色 | 源码扫描 |
| 主题 | 深色 `#2D323A` 底 + `#FFFFFF` 线；浅色 `#F3F3F3` 底 + `#4A4A4A` 线 | `check` 断言对比度 ≥ 3:1 |
| 复杂度 | 独立几何元素 ≤ 4；虚线仅用于「框选 / 范围 / 剖切面」 | `check` 报错/警告 |
| 虚线 | 只允许 `stroke-dasharray="2 3"`（紧凑版 `2 2`） | `check` 断言 |
| 禁止 | 渐变、阴影、立体、多色、文字标签、填充块、写死 HEX | 生成器不提供对应能力 |

**为什么是 2px / 24px**：在 16px 渲染时实际笔画 ≈ 1.33px，与 12px 正文、1.5px 复选框勾视觉等重；
在 32px 渲染时 ≈ 2.67px，仍不抢文字。粗细在两者之间都成立，所以不要为某个尺寸单独出图。

---

## 3. 执行工作流

1. **意象提炼**：把功能拆成「1 个核心几何 + 1 个操作修饰符」。
   - 核心几何：文档 / 方框 / 圆形 / 立方体 / 箭头 / 折线 / 漏斗。
   - 修饰符：加号（新建）、虚线框（框选）、双层错位（批量/复制）、切向箭头（旋转/延伸）、斜线（倒角/修剪）。
2. **先查目录**：`python tools/IconGen.py search 批量`——能复用就复用，语义不同才新建。
3. **起草几何**：只允许 `path` / `rect` / `circle` / `line` 四种基元；坐标取整数或 0.5。
4. **校验**：`python tools/IconGen.py check --strict`，消除越界、贴边、复杂度过高。
5. **双主题目检**：`python tools/IconGen.py preview` 后看 `preview/icons.html`，**必须同时看 32px 和 16px**。
6. **交付**：`svg` 给前端/网页，`xaml` 给 WPF，`prompt` 给 AI 出参考图。

新增一个图标的最小改动只有一处——往 `catalog/icons.json` 里加一条，然后重跑生成器（见 §5）。

---

## 4. 常用命令

```bash
cd skills/ribbon-icon-generator

python tools/IconGen.py list                 # 按分类列出 40 个图标
python tools/IconGen.py search 批量           # 中/英/关键字/意象 搜索
python tools/IconGen.py svg box-select        # 单个 SVG 打印到终端
python tools/IconGen.py svg box-select --theme dark --size 24 --out out/
python tools/IconGen.py all --out samples/svg --size 32     # 批量导出
python tools/IconGen.py xaml --out samples/RibbonIcons.xaml  # WPF 资源字典
python tools/IconGen.py preview --out preview/icons.html      # 深浅双主题对照页
python tools/IconGen.py check --strict        # 安全边距/复杂度/对比度校验，CI 可用
python tools/IconGen.py prompt "Batch export" --theme dark --negative
```

退出码：`0` 通过；`1` 有错误；`2` `--strict` 下有警告。适合直接挂进提交前检查。

---

## 5. 目录数据格式

```json
{
  "id": "box-select",            // kebab-case，唯一
  "zh": "框选",
  "en": "Box Select",
  "category": "selection",       // 必须存在于 categories
  "keywords": ["框选", "窗口选择", "box", "crossing"],
  "concept": "外围虚线框 + 内部实体对象",
  "shapes": [
    { "kind": "rect", "x": 3, "y": 3, "w": 18, "h": 18, "r": 2, "dash": "2 3" },
    { "kind": "rect", "x": 8, "y": 8, "w": 8, "h": 8, "r": 1 }
  ]
}
```

| 基元 | 字段 | 备注 |
|---|---|---|
| `path` | `d` | 支持 `M L H V C S Q T A Z` 及相对命令；用于勾、折线、圆角、复合轮廓 |
| `rect` | `x` `y` `w` `h` `r` | `r` 省略即直角；`r=1` 常规、`r=2` 外框 |
| `circle` | `cx` `cy` `r` | 整圆；半圆/弧用 `path` |
| `line` | `x1` `y1` `x2` `y2` | 仅直段，端点由全局 round cap 处理 |

可选 `"dash": "2 3"` 只加在该修饰符上，不要整图虚线。

---

## 6. 概念 → 几何对照（可直接抄的配方）

| 功能概念 | 构成 | 目录 id |
|---|---|---|
| 单选/点选 | 光标多边形箭头 | `select` |
| 框选 | 虚线外框 + 内部实线小方框 | `box-select` |
| 批量/多选 | 第一次实线框 + 第二次虚线框错位叠加 | `batch-select` |
| 筛选/范围 | 漏斗（宽口收窄到导管） | `filter` |
| 批量处理 | 前层实体 + 后层 L 型错位轮廓 | `batch`（见 `copy`） |
| 复制/派生 | 两个错位圆角矩形 | `copy` |
| 阵列/重复 | 源对象 + 向右、向下的虚线副本 | `array` |
| 分解/炸开 | 2×2 四块拉开间距 | `explode` |
| 旋转 | 带缺口的 3/4 圆环 + 切向箭头 | `rotate` |
| 镜像 | 虚线对称轴 + 左右对称几何 | `mirror` |
| 延伸 | 边界竖线 + 指向边界的箭头 | `extend` |
| 圆角 / 倒角 | 直角被圆弧 / 斜线替换 | `fillet` / `chamfer` |
| 约束/锁定 | 锁体圆角矩形 + 锁梁弧 | `lock` |
| 三维视图 | 等轴测六边形 + 三条内棱线 | `cube-iso` |
| 剖切 | 立方体 + 虚线剖切面 | `section` |
| 捕捉 | 四向准星短线 + 中心方块 | `snap` |
| 正交 | 直角折线 + 直角标记小方块 | `ortho` |

更多配方、反面案例与扩展检查清单见 [`references/geometry-cookbook.md`](references/geometry-cookbook.md)。

---

## 7. SVG 输出模板

生成器产出的标准结构（`--theme none` 时描边为 `currentColor`）：

```xml
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="24" height="24"
     fill="none" stroke="currentColor" stroke-width="2"
     stroke-linecap="round" stroke-linejoin="round">
  <rect x="3" y="3" width="18" height="18" rx="2" stroke-dasharray="2 3" />
  <rect x="8" y="8" width="8" height="8" rx="1" />
</svg>
```

- 用 `currentColor` 让图标跟随按钮文字色，**永远不要写死 `stroke="#333"`**。
- 需要位图/预览时才用 `--theme dark|light` 注入背景，交付给业务代码的 SVG 保持 `none`。
- 同一图标内所有元素共用一个 `stroke-width`，没有例外。

---

## 8. WPF 集成（本仓库 token 体系）

`python tools/IconGen.py xaml --out samples/RibbonIcons.xaml` 生成：

- `Icon.Ribbon.<PascalId>`：合并后的 `StreamGeometry`（实线部分）。
- `Icon.Ribbon.<PascalId>.Dash`：虚线修饰层（如有）。
- `Style x:Key="Icon.Ribbon"`：`Stroke` 绑定 `(TextElement.Foreground)`、`StrokeThickness=2`、round cap/join、固定 24×24 画布。
- `Style x:Key="Icon.Ribbon.Dash"`：`StrokeDashArray="1 1.5"`（= 设计稿的 `2 3` ÷ stroke 2）。

```xml
<ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="Theme/Theme.xaml" />
        <ResourceDictionary Source="samples/RibbonIcons.xaml" />
    </ResourceDictionary.MergedDictionaries>
</ResourceDictionary>
```

```xml
<!-- 16px 工具条按钮：Viewbox 负责尺寸，描边自动继承按钮前景 -->
<Button Style="{StaticResource Button.Icon}" ToolTip="框选 (Esc)">
    <Viewbox Width="{StaticResource Icon.Size.Regular}"
             Height="{StaticResource Icon.Size.Regular}">
        <Path Data="{StaticResource Icon.Ribbon.BoxSelect}"
              Style="{StaticResource Icon.Ribbon}" />
    </Viewbox>
</Button>

<!-- 带虚线修饰：两层同视口叠放 -->
<Viewbox Width="{StaticResource Icon.Size.Regular}"
         Height="{StaticResource Icon.Size.Regular}">
    <Grid>
        <Path Data="{StaticResource Icon.Ribbon.BoxSelect.Dash}"
              Style="{StaticResource Icon.Ribbon.Dash}" />
        <Path Data="{StaticResource Icon.Ribbon.BoxSelect}"
              Style="{StaticResource Icon.Ribbon}" />
    </Grid>
</Viewbox>
```

接入要点：

1. **尺寸只走 `Icon.Size.*`**（12/14/16/20/24/32），不要写 13、17、23。默认 16px 配 12px 正文。
2. **描边继承前景**，不要在 Path 上写 `Stroke="#4A4A4A"`；主按钮里会自动变白，禁用态自动置灰。
3. **固定 24 画布**：`Icon.Ribbon` 的 `Stretch=None + Width/Height=24` 保证小图形（如直线）不会被放大到和立方体同宽，Viewbox 只做等比缩放。
4. 纯图标按钮必须给 `ToolTip`（无障碍要求，见 `docs/09-icon-system.md`）。
5. 深色主题下 `#FFFFFF` 对 `#2D323A` 实测 12.89:1；浅色 `#4A4A4A` 对 `#F3F3F3` 为 7.99:1，都远超图标所需的 3:1。

---

## 9. AI 绘图提示词（只用于找构图，不直接交付位图）

```bash
python tools/IconGen.py prompt "Batch export" --theme dark --negative
python tools/IconGen.py prompt box-select --theme light --concept "dashed frame with one solid square inside"
```

正向公式：

```text
Minimalist flat line icon of {核心几何意象}, CAD software ribbon UI style, 2px uniform
monoline stroke, rounded corners, extremely simple geometric silhouette, high contrast
monochrome, pure white lines on solid dark slate gray background (#2D323A), centered,
2D vector, no gradients, no shadows, clean negative space.
```

浅色模式把调色短语替换为：`dark charcoal gray lines (#4A4A4A) on solid light gray background (#F3F3F3)`。

负面提示词：

```text
complex details, 3D, isometric, gradients, shadows, textures, multiple colors, thin lines,
realistic, cluttered, filled shapes, text, watermark
```

拿到位图后**必须重绘为矢量**：位图只提供构图灵感，直接缩放会得到模糊、粗细不均的描边。
完整提示词库与重绘流程见 [`references/prompt-library.md`](references/prompt-library.md)。

---

## 10. 质量门（交付前逐条过）

1. `python tools/IconGen.py check --strict` → 0 error / 0 warning。
2. 在 `preview/icons.html` 里**同时**看 32px 与 16px：16px 下仍能认出主特征才算通过。
3. 深浅两色下都看：浅色不是「深色图标的简单反色」，而是各自达到对比度要求。
4. 同屏混排时，图标视觉重量一致：细碎图形（剪刀、手）要缩一点，方框类要撑满一点。
5. 图标与 12px 正文并排时，间距 6px、垂直居中；纯图标按钮必须有 `ToolTip`。
6. 不要为了凑数画 5 个元素；`check` 的复杂度上限是 4，超过就拆成语义更准的图标。

---

## 11. 文件索引

| 路径 | 内容 |
|---|---|
| `catalog/icons.json` | 40 个图标几何 + 分类 + 主题色（唯一数据源） |
| `tools/IconGen.py` | SVG / XAML / 预览 / 校验 / 提示词 CLI（Python 3 标准库） |
| `samples/svg/` | 全部图标的 32px SVG 成品 |
| `samples/RibbonIcons.xaml` | WPF 资源字典 + 用法片段（生成物） |
| `preview/icons.html` | 深浅双主题对照页（生成物） |
| `references/geometry-cookbook.md` | 形状配方、反面案例、新增图标检查清单 |
| `references/prompt-library.md` | 提示词库与「位图 → 矢量」重绘流程 |
| `install.ps1` | 安装到本机 AI 客户端的 skills 目录 |

样式基线来自同仓库的 WPF UI 技能：`docs/09-icon-system.md`（尺寸阶梯、着色契约、无障碍）。