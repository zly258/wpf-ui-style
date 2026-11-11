# AI 绘图提示词库与「位图 → 矢量」流程

提示词只负责**构图探索**。Ribbon 图标最终必须落到 `catalog/icons.json`，
否则会得到边缘发虚、粗细不均、无法随主题变色的位图。

---

## 1. 标准公式

```text
Minimalist flat line icon of {主体}, CAD software ribbon UI style, 2px uniform monoline
stroke, rounded corners, extremely simple geometric silhouette, high contrast monochrome,
{palette}, centered, 2D vector, no gradients, no shadows, clean negative space.
```

| 槽位 | 深色模式 | 浅色模式 |
|---|---|---|
| `{palette}` | `pure white lines on solid dark slate gray background (#2D323A)` | `dark charcoal gray lines (#4A4A4A) on solid light gray background (#F3F3F3)` |

负面提示词（每次都带）：

```text
complex details, 3D, isometric, gradients, shadows, textures, multiple colors, thin lines,
realistic, cluttered, filled shapes, text, watermark
```

> `isometric` 在负面里，是因为三维立体感会破坏「平面线性」；如果主体本身就是三维视图图标
> （如 `cube-iso`），则从负面里删掉这一项。

CLI 直接生成：

```bash
python tools/IconGen.py prompt "批量导出" --theme dark --negative
python tools/IconGen.py prompt "剖切平面" --theme light --concept "立方体 + 虚线切面"
```

---

## 2. 现成提示词

| 功能 | 提示词主体 | 建议构图 |
|---|---|---|
| 框选 | `dashed selection rectangle enclosing a small solid square` | 外虚线、内实线 |
| 批量选择 | `two overlapping selection rectangles, back one dashed` | 错位 5px |
| 批量导出 | `two stacked document sheets with a single outward arrow` | 只用一个箭头 |
| 图层管理 | `three stacked parallelogram layers` | 上小下大 |
| 参数约束 | `padlock with a rounded lock body` | 锁体 r2 |
| 三维剖切 | `isometric cube with a dashed cutting plane` | 虚线贯穿 |
| 捕捉 | `crosshair with a small square at the center` | 短线不触边 |
| 正交 | `right angle line with a small square at the corner` | 直角标记 |
| 视图缩放 | `four corner brackets framing a small square` | 不闭合 |
| 批量打印 | `stacked pages above a printer` | 最多 4 元素 |

生成多张后，选**元素最少、16px 下仍可辨**的那张作为构图参考。

---

## 3. 位图 → 矢量重绘

1. 缩到 16px 看是否还能认出主特征；认不出直接放弃这张构图。
2. 在 24 网格上重画：只保留位图里的主轮廓和 1 个修饰符，删掉所有渲染细节。
3. 用 `rect` / `circle` / `line` / `path` 四种基元落成 JSON。
4. `python tools/IconGen.py check --strict`。
5. `python tools/IconGen.py preview` 与现有图标并排比对视觉重量。
6. 需要更「像」参考图时，调坐标不调描边——`stroke` 永远保持 2。

---

## 4. 反面提示词组合

| 组合 | 结果 |
|---|---|
| `detailed`, `intricate`, `realistic` | 出现材质、阴影，无法线性化 |
| `3d icon`, `isometric`, `clay` | 与平面线性体系冲突 |
| `colorful`, `gradient` | 无法双主题适配 |
| `glow`, `neon`, `glassmorphism` | 深色主题下过亮，浅色主题失效 |
| `logo`, `badge`, `emblem` | 会生成文字/装饰边框，24px 下糊 |

---

## 5. 验收

- 位图只作为参考，**不得直接进产品**。
- 矢量必须通过 `check --strict` 与 16px 目检。
- 深浅两种背景下都截图确认，不靠「看起来差不多」。