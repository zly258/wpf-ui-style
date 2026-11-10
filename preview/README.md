# 可运行预览工程（案例中心）

把仓库里的 `Theme/` 接到一个真实 WPF 工程里，用一个**侧边导航主壳 + 可切换案例页**演示规范效果。
主题文件通过 `Link` 直接引用仓库根目录的 `Theme/`，不复制副本，改主题即时生效。

## 目录

```text
preview/
├─ Shell/
│  ├─ MainWindow.xaml(.cs)   ← 案例中心：左侧分类导航 + 右侧案例宿主 + 密度切换
│  ├─ CaseCatalog.cs         ← 唯一案例清单（导航、首页卡片、截图入口共用）
│  └─ CaseHostWindow.cs      ← 把单个案例页包成独立窗口，供截图/测试/“独立窗口”按钮
├─ Views/
│  ├─ OverviewView.xaml      ← 首页：规范摘要 + 全部案例卡片
│  ├─ ControlsGalleryView    ← 基础控件全览
│  ├─ FormLayoutView         ← 表单对齐（宽面板两列 / 窄面板单列）
│  ├─ SafeFormView           ← 安全表单骨架
│  ├─ SettingsView           ← 密集设置表单
│  ├─ EditableTableView      ← 可编辑数据表格
│  ├─ CompareView            ← 双栏对照 + 日志
│  ├─ WorkbenchView          ← 工程工作台三栏
│  ├─ WizardView             ← 分步向导
│  └─ DashboardView          ← 监控看板
├─ Diagnostics/
│  └─ ProbeWindow.xaml       ← 交互态探针（不在导航中）
├─ App.xaml(.cs)             ← 入口、离屏渲染、命令行
├─ DensitySwitcher.cs        ← 运行时密度切换
└─ UiTestEngine.cs           ← 交互点击与自检引擎
```

案例页全部是 `UserControl`：既能被主壳右侧直接承载，也能被 `CaseHostWindow`
包装成独立窗口。新增案例只需在 `CaseCatalog.cs` 增加一条元数据。

## 案例清单

| 页面 | `-window` | 内容 |
|---|---|---|
| `OverviewView` | `home` / `overview` | 首页：规范定位、三条核心约束、全部案例入口 |
| `ControlsGalleryView` | `controls` | 按钮/输入/下拉/复选/标签页/密度等基础控件矩阵 |
| `FormLayoutView` | `form-layout` | 表单对齐：宽面板两列、窄面板单列，标签与控件左对齐 |
| `SafeFormView` | `safeform` | 标签不裁、值不藏、状态与按钮栏分行 |
| `SettingsView` | `settings` | 三列并排的密集设置表单 |
| `EditableTableView` | `editable-table` | 全选联动、单元格编辑、批量操作、分页 |
| `CompareView` | `compare` | 主内容弹性、日志固定高度、底部操作栏不位移 |
| `WorkbenchView` | `workbench` | 特征树 / 视口 / 属性检查器三栏 |
| `WizardView` | `wizard` | 步骤状态机与标准化流程控制 |
| `DashboardView` | `dashboard` | KPI 卡片、任务队列、告警表格 |
| `ProbeWindow` | `probe` | 交互态探针（编辑态、弹层、折叠长内容），仅诊断用 |

已删除的旧入口：`showcase` 仍映射到首页；`layouts` / `anti` / `case-table`
已合并到正式案例（`editable-table`），不再单独维护。

## 运行

交互窗口（默认打开案例中心首页）：

```bash
dotnet run --project preview

# 打开指定案例页（主壳内）
dotnet run --project preview -- -window home -page workbench

# 只打开单个案例的独立窗口（无侧边导航）
dotnet run --project preview -- -window editable-table
```

离屏渲染成 PNG（不弹窗口、不进消息循环）：

```bash
# 主壳首页（含侧边导航）
dotnet run --project preview -- -window home -shot out/home.png

# 主壳 + 指定页面
dotnet run --project preview -- -window home -page dashboard -shot out/home-dashboard.png

# 单个案例独立窗口
dotnet run --project preview -- -window editable-table -shot out/table.png -scale 1.5

# 指定密度（Compact / Medium / Relaxed）
dotnet run --project preview -- -window home -density Compact -shot out/home-compact.png

# 全部案例各出一张（含主壳首页）
dotnet run --project preview -- -shot out/all.png -all -scale 1.0

# 展开菜单弹层（验证 MenuItem 模板）
dotnet run --project preview -- -shot out/menu.png -menu
```

| 参数 | 说明 |
|---|---|
| `-window <name>` | `home` / `overview` / `controls` / `safeform` / `settings` / `editable-table` / `compare` / `workbench` / `wizard` / `dashboard` / `probe` |
| `-page <key>` | 主壳启动时显示的页面（默认 `overview`） |
| `-density <档位>` | `Compact` / `Medium` / `Relaxed`，运行时热切换（默认中等） |
| `-shot <path>` | 输出 PNG；`-all` 时按 `<目录>/<窗口名>.png` 输出全部案例页 |
| `-scale <n>` | 渲染倍率，`1.5` 等价于 150% DPI 观感 |
| `-all` | 全部案例各出一张（含主壳首页） |
| `-menu` | 展开控件全览的菜单并把弹层合成到图上 |
| `-autoclose <秒>` | 实机冒烟：渲染完成后自动关闭并退出 |

离屏渲染会先把内容撑到自然高度再截图，所以整页内容都在一张图里，而不是只截一屏。

## 实现要点

- **导航与页面解耦**：`CaseCatalog` 是唯一清单；主壳只负责选择/缓存页面，
  首页卡片通过 `CaseRequested` 事件请求切换，不直接持有导航逻辑。
- **密度全局一处**：主壳右上角切换密度，所有页面通过 `{DynamicResource Layout.*}` 即时响应；
  单个页面不再各自放密度下拉。
- **页面可独立运行**：`CaseHostWindow` 复用同一 `UserControl`，截图与 UiTest 都走这条路径。
- **向导取消/完成**：嵌在主壳时重置到第 1 步，不会关闭整个应用。
- **清理无用案例**：旧的 Showcase、Layouts、AntiPattern、CaseTable 已移除，
  避免同一骨架维护多份副本。

## 校验

```bash
python tools/LayoutCheck.py --root preview   # 0 错误
python tools/ContrastCheck.py                # 全部通过
python tools/UiTest.py --list
python tools/UiTest.py --all --test-clicks
```
