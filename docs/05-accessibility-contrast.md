# 05. 对比度与可访问性

本规范以 WCAG 2.1 AA 为最低线，并针对桌面 CAD 插件增加两条可读性要求。

---

## 1. 最低要求

| 内容 | 最低对比度 | 说明 |
|---|---:|---|
| 正文文字（< 18.66px 粗体 / < 24px 常规） | **4.5:1** | 本规范正文 13.5px |
| 大号文字（≥ 18.66px 粗体 或 ≥ 24px） | **3:1** | 标题 14.5px 仍按 4.5:1 |
| 图标、焦点环、输入框边框等非文字 | **3:1** | 焦点环必须 3:1 |
| 禁用文字 | WCAG 豁免 | 本规范要求 ≥ 3:1，建议 ≥ 4.5:1 |
| 装饰性边框/分隔线 | 不强制 | 但不能是唯一的状态信号 |

> 14.5px 的标题不算“大号文字”，仍按 4.5:1。

---

## 2. 实测对比度

以下比值按 WCAG 相对亮度公式计算（sRGB，`L = 0.2126R + 0.7152G + 0.0722B`）。

### 2.1 正文与次要文字

| 前景 | 背景 | 比值 | 结论 |
|---|---|---:|---|
| `Text.Primary` `#1F2933` | `WindowBackground` `#E9EDF2` | 12.55:1 | ✅ |
| `Text.Primary` | `Surface` `#FAFBFC` | 14.24:1 | ✅ |
| `Text.Primary` | `SurfaceAlt` `#F2F4F7` | 13.39:1 | ✅ |
| `Text.Primary` | `Accent.Subtle` `#E4EEF5` | 12.54:1 | ✅ |
| `Text.Primary` | `Accent.SubtleHover` `#D8E6F0` | 11.59:1 | ✅ |
| `Text.Primary` | `Control.Hover` `#F0F3F6` | 13.25:1 | ✅ |
| `Text.Primary` | `Control.Pressed` `#E4E8EE` | 12.00:1 | ✅ |
| `Text.Primary` | `Selection` `#E4EEF5` | 12.54:1 | ✅ |
| `Text.Primary` | `Selection.Hover` `#D8E6F0` | 11.59:1 | ✅ |
| `Text.Secondary` `#4B5563` | `WindowBackground` | 6.43:1 | ✅ |
| `Text.Secondary` | `Surface` | 7.29:1 | ✅ |
| `Text.Secondary` | `SurfaceAlt` | 6.86:1 | ✅ |

### 2.2 白字压在品牌/危险色上

| 前景 | 背景 | 比值 | 结论 |
|---|---|---:|---|
| `OnAccent` `#FFFFFF` | `Accent` `#2F6F9F` | 5.39:1 | ✅ |
| `OnAccent` | `Accent.Hover` `#285F88` | 6.81:1 | ✅ |
| `OnAccent` | `Accent.Pressed` `#22506F` | 8.59:1 | ✅ |
| `OnAccent` | `Danger` `#B43B32` | 5.81:1 | ✅ |
| `OnAccent` | `Danger.Hover` `#97302A` | 7.58:1 | ✅ |
| `OnAccent` | `Danger.Pressed` `#7E2823` | 9.48:1 | ✅ |
| `Tooltip.Foreground` `#FFFFFF` | `Tooltip.Background` `#1F2933` | 14.76:1 | ✅ |

### 2.3 语义色与浅底

| 前景 | 背景 | 比值 | 结论 |
|---|---|---:|---|
| `Text.Primary` | `Danger.Subtle` `#FBEDEC` | 12.95:1 | ✅ |
| `Text.Primary` | `Success.Subtle` `#E9F4EE` | 13.10:1 | ✅ |
| `Text.Primary` | `Warning.Subtle` `#F7F0E3` | 13.02:1 | ✅ |
| `Danger` `#B43B32` | `Danger.Subtle` | 5.10:1 | ✅ |
| `Success` `#2A704E` | `Success.Subtle` | 5.29:1 | ✅ |
| `Warning` `#7A5A1E` | `Warning.Subtle` | 5.60:1 | ✅ |

### 2.4 禁用与焦点

| 前景/元素 | 背景 | 比值 | 结论 |
|---|---|---:|---|
| `Text.Disabled` `#667085` | `Control.Disabled` `#F2F4F7` | 4.51:1 | ✅（超过 3:1 目标） |
| `Focus` `#2F6F9F` | `Surface` `#FAFBFC` | 5.20:1 | ✅（非文字 3:1） |
| `Border.Strong` `#AEB7C2` | `Surface` | 1.96:1 | ⚠️ 装饰性边框，不作为唯一状态信号 |

> `Border`/`Border.Strong` 是装饰性边框，不满足 3:1 是允许的；
> 但**焦点环、错误边框、选中指示**必须使用 `Focus`/`Danger`/`Accent`。

---

## 3. 禁用态

WCAG 允许禁用控件低于 4.5:1，但本规范要求：

- 禁用文字 ≥ 3:1；推荐 4.5:1。
- 禁用控件必须仍然可辨认“这是一个控件”。
- 不要用 `Opacity`，因为透明度会同时降低文字和背景的对比度。
- 禁用态使用：
  - Background `Brush.Control.Disabled`
  - Foreground `Brush.Text.Disabled`
  - Border `Brush.Border`
- 禁用态不响应悬停/按下视觉变化（`Cursor=Arrow`）。

---

## 4. 焦点指示

- 焦点环使用 `Brush.Focus`（`#2F6F9F`），在 `Surface` 上 5.2:1。
- 焦点环至少 1px；建议 1px 边框 + 1px 内阴影（如果模板允许）。
- 不要只靠系统虚线焦点框；它在高 DPI 下可能发虚。
- 输入框使用 `IsKeyboardFocusWithin`；按钮使用 `IsKeyboardFocused`。
- 焦点顺序必须与视觉顺序一致。

---

## 5. 颜色不是唯一信号

状态必须至少有两种编码：

| 状态 | 颜色 | 辅助信号 |
|---|---|---|
| 成功 | 绿色文字/图标 | “成功”文字、对勾图标 |
| 警告 | 棕黄文字/图标 | “警告”文字、感叹号图标 |
| 失败 | 红色文字/图标 | “失败”文字、叉号图标 |
| 选中 | 浅蓝底 | 边框、加粗、勾选 |
| 焦点 | 蓝色边框 | 键盘可见的边框 |
| 禁用 | 灰色 | 光标变化、不可点击 |

灰度打印/色盲模拟下仍应能区分。

---

## 6. 如何计算对比度

WCAG 公式：

```text
L = 0.2126 * R + 0.7152 * G + 0.0722 * B
其中 R/G/B 先做 sRGB 线性化：
  c <= 0.04045 ? c / 12.92 : ((c + 0.055) / 1.055) ^ 2.4

contrast = (Lmax + 0.05) / (Lmin + 0.05)
```

### 6.1 自动检查

```powershell
pwsh ./tools/ContrastCheck.ps1
```

脚本会：

- 读取 `Theme/Tokens.xaml` 的 `Color.*`；
- 检查正文、白字、语义色、禁用、焦点等固定组合；
- 输出比值与通过/失败；
- 任一失败时退出码为 1。

### 6.2 新增颜色时

1. 在 `Tokens.xaml` 增加 `Color.*` 和 `Brush.*`。
2. 在 `tools/ContrastCheck.ps1` 的 `$Pairs` 中增加组合。
3. 在 `tests/ThemeContractTests.cs` 中增加同样的组合。
4. 在本文档的表格中登记实测值。
5. 运行脚本和测试。

---

## 7. 高 DPI 与灰度

- 100% / 125% / 150% / 200% 都要检查。
- 灰度截图检查状态是否仍可区分。
- 不要用纯红/纯绿作为唯一语义色。
- 不要用 1px 浅灰边框作为唯一选中指示；配合背景色或加粗。

---

## 8. 常见可访问性错误

| 错误 | 后果 | 修复 |
|---|---|---|
| 白字压浅蓝底 | 对比度 < 2:1 | 改深字或加深背景 |
| 禁用用 `Opacity=0.45` | 文字不可读 | 显式禁用色 |
| 浅灰边框作为焦点 | 看不到焦点 | `Brush.Focus` 1px |
| 只用颜色表示成功/失败 | 色盲不可辨 | 加文字/图标 |
| 字号 < 12px | 高 DPI 发虚 | 最小 12px，正文 13.5px |
| 局部 `Foreground` 覆盖状态 | 悬停/禁用失效 | 用内容宿主传递前景 |
| ToolTip 黑底黑字 | 完全不可读 | 同时设置背景和前景 |
