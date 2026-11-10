using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using WpfStylePreview.Views;

namespace WpfStylePreview.Shell
{
    /// <summary>
    /// 一个可浏览的案例：左侧导航与首页卡片共用同一份元数据。
    /// </summary>
    public sealed class CaseDescriptor
    {
        public string Key { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public double Width { get; set; } = 1180;
        public double Height { get; set; } = 760;
        public Func<UserControl> Create { get; set; }

        public bool IsHome => string.Equals(Key, "overview", StringComparison.Ordinal);
    }

    public static class CaseCatalog
    {
        /// <summary>
        /// 唯一的案例清单。新增案例只改这里，侧边导航、首页卡片、
        /// -window 截图/测试别名都从这里取，不再分散维护。
        /// </summary>
        public static IReadOnlyList<CaseDescriptor> All { get; } = new List<CaseDescriptor>
        {
            new CaseDescriptor
            {
                Key = "overview", Category = "开始",
                Title = "案例总览", Subtitle = "规范、密度与全部业务案例入口",
                Width = 1280, Height = 860,
                Create = () => new OverviewView()
            },
            new CaseDescriptor
            {
                Key = "controls", Category = "基础",
                Title = "基础控件", Subtitle = "按钮、输入、下拉、复选、标签页与密度切换",
                Width = 1180, Height = 940,
                Create = () => new ControlsGalleryView()
            },
            new CaseDescriptor
            {
                Key = "safeform", Category = "基础",
                Title = "安全表单", Subtitle = "标签不裁、值不藏、按钮不被内容顶出",
                Width = 820, Height = 560,
                Create = () => new SafeFormView()
            },
            new CaseDescriptor
            {
                Key = "settings", Category = "基础",
                Title = "密集设置表单", Subtitle = "三列并排骨架，一屏看到更多字段",
                Width = 1040, Height = 720,
                Create = () => new SettingsView()
            },
            new CaseDescriptor
            {
                Key = "editable-table", Category = "数据与布局",
                Title = "可编辑数据表格", Subtitle = "全选联动、单元格编辑、批量操作与分页",
                Width = 1240, Height = 760,
                Create = () => new EditableTableView()
            },
            new CaseDescriptor
            {
                Key = "compare", Category = "数据与布局",
                Title = "双栏对照与日志", Subtitle = "主内容弹性、日志固定高度、操作栏不位移",
                Width = 1180, Height = 720,
                Create = () => new CompareView()
            },
            new CaseDescriptor
            {
                Key = "workbench", Category = "数据与布局",
                Title = "工程工作台", Subtitle = "三栏布局：特征树 / 视口 / 属性检查器",
                Width = 1320, Height = 820,
                Create = () => new WorkbenchView()
            },
            new CaseDescriptor
            {
                Key = "wizard", Category = "流程与监控",
                Title = "分步向导", Subtitle = "步骤状态机与标准化流程控制栏",
                Width = 980, Height = 680,
                Create = () => new WizardView()
            },
            new CaseDescriptor
            {
                Key = "dashboard", Category = "流程与监控",
                Title = "监控看板", Subtitle = "KPI 卡片、任务队列与告警表格",
                Width = 1260, Height = 820,
                Create = () => new DashboardView()
            }
        };

        /// <summary>首页卡片：排除“总览”自身。</summary>
        public static IReadOnlyList<CaseDescriptor> Cases { get; } =
            All.Where(c => !c.IsHome).ToList();

        public static CaseDescriptor Find(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            return All.FirstOrDefault(c => string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase));
        }
    }
}
