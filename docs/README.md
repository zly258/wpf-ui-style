# 文档索引

| 文档 | 内容 | 何时读 |
|---|---|---|
| [01-design-tokens.md](01-design-tokens.md) | 颜色、字体、尺寸、圆角令牌 | 选色、加 token、查对比度数值 |
| [02-control-states.md](02-control-states.md) | 控件状态矩阵（Normal/Hover/Pressed/Selected/Disabled） | 改模板、排查白字浅底 |
| [03-layout.md](03-layout.md) | 布局、防截断、密度与控件可识别性 | 排表单、修遮挡 |
| [04-common-pitfalls.md](04-common-pitfalls.md) | 常见错误与修复对照 | 遇到具体症状 |
| [05-accessibility-contrast.md](05-accessibility-contrast.md) | WCAG 对比度实测与规则 | 新增颜色、验收 |
| [06-density.md](06-density.md) | 紧凑 / 中等 / 宽松三档密度与切换 | 选密度、实现热切换 |
| [07-testing.md](07-testing.md) | 自动化脚本、契约测试、人工矩阵 | 提交前验证 |
| [09-icon-system.md](09-icon-system.md) | 24×24 矢量图标契约 | 加图标、改图标按钮 |

配套资源：

- `references/cheatsheet.md`：一页速查。
- `references/xaml-snippets.md`：可复制片段（合并主题、密度、表单、表格、Tab、安全骨架）。
- `samples/forms|tables|layouts|dialogs|icons/`：按用途分类的可复制窗口骨架。
- `preview/`：可运行案例中心（侧边导航 + 10 个案例页，支持 `-page` / `-density` 与离屏截图）。

常用验证：

```bash
python tools/LayoutCheck.py --root . --theme-dir Theme
python tools/ContrastCheck.py
dotnet test tests/WpfStyleContractTests.csproj
dotnet build preview/WpfStylePreview.csproj
```
