using System.Collections.ObjectModel;
using System.Windows;

namespace WpfStylePreview
{
    /// <summary>
    /// 案例：生产与任务监控看板（Dashboard）。
    /// 演示：
    /// 1. 顶部 KPI 指标卡片网格与状态色彩；
    /// 2. 左侧批次进度与任务流水线队列；
    /// 3. 右侧系统异常与质检告警表格；
    /// 4. 底部运维状态与日志操作。
    /// </summary>
    public partial class CaseDashboardWindow : Window
    {
        public ObservableCollection<AlertItem> Alerts { get; } = new ObservableCollection<AlertItem>
        {
            new AlertItem { Level = "警告", Timestamp = "14:28:10", Source = "PRT-1005_销轴.dwg", Message = "孔轴配合公差间隙偏小：理论间隙 0.005mm 接近干涉阈值" },
            new AlertItem { Level = "错误", Timestamp = "14:25:02", Source = "M-1005_阀体.dwg", Message = "未找到关联材质硬度标准文档：ASTM A351 CF8M" },
            new AlertItem { Level = "信息", Timestamp = "14:22:45", Source = "BATCH-20261107-04", Message = "节点 #3 已自动扩容以应对密集图形多边形计算" },
            new AlertItem { Level = "警告", Timestamp = "14:18:19", Source = "DOC-0482_总装.iam", Message = "标题栏中“重量”属性与三维实体积分差 2.4%，已自动更新" },
            new AlertItem { Level = "信息", Timestamp = "14:10:00", Source = "调度核心引擎", Message = "每日自动化数据冷备份完成，校验哈希匹配" },
        };

        public CaseDashboardWindow()
        {
            InitializeComponent();
            GridAlerts.ItemsSource = Alerts;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("监控指标与告警流已从分布式节点同步刷新完成。", "看板刷新", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("今日监控运行报告与合格率报表已导出至 Desktop/Metrics_Report.xlsx。", "报表导出", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnClearFinished_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("已清理完成态的历史任务条目。", "清理成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExportLog_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("已打包最近 24 小时系统运行日志至 logs/system_20261107.zip。", "日志导出", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class AlertItem
    {
        public string Level { get; set; }
        public string Timestamp { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
    }
}
