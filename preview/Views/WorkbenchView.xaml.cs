using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace WpfStylePreview.Views
{
    /// <summary>
    /// 案例：工程工作台三栏布局（Workbench）。
    /// 演示：
    /// 1. 左侧装配结构与特征导航树；
    /// 2. 中间主 3D/图纸视口与特征参数表格；
    /// 3. 右侧分组折叠式属性检查器（Property Inspector）；
    /// 4. 底部系统状态与坐标信息条。
    /// </summary>
    public partial class WorkbenchView : UserControl
    {
        public ObservableCollection<FeatureParamItem> FeatureParams { get; } = new ObservableCollection<FeatureParamItem>
        {
            new FeatureParamItem { Name = "分度圆直径 d", DesignValue = "47.50", Unit = "mm", UpperTol = "+0.00", LowerTol = "-0.02", Note = "关键啮合尺寸" },
            new FeatureParamItem { Name = "齿顶圆直径 da", DesignValue = "52.50", Unit = "mm", UpperTol = "+0.05", LowerTol = "-0.05", Note = "车削外圆控制" },
            new FeatureParamItem { Name = "齿根圆直径 df", DesignValue = "41.25", Unit = "mm", UpperTol = "+0.00", LowerTol = "-0.10", Note = "滚齿深度控制" },
            new FeatureParamItem { Name = "齿宽 b", DesignValue = "28.00", Unit = "mm", UpperTol = "+0.10", LowerTol = "-0.10", Note = "端面对称加工" },
            new FeatureParamItem { Name = "内孔直径 d_inner", DesignValue = "20.00", Unit = "mm", UpperTol = "+0.02", LowerTol = "+0.00", Note = "轴承位过渡配合 H7" },
            new FeatureParamItem { Name = "键槽宽度 w", DesignValue = "6.00", Unit = "mm", UpperTol = "+0.03", LowerTol = "+0.00", Note = "JS9 配合键联接" },
        };

        public WorkbenchView()
        {
            InitializeComponent();
            GridFeatureParams.ItemsSource = FeatureParams;
        }

        private void TreeAssembly_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem selectedItem && TxtSelectedNodeTitle != null)
            {
                TxtSelectedNodeTitle.Text = $"当前选中项：{selectedItem.Header}";
            }
        }

        private void BtnRefreshTree_Click(object sender, RoutedEventArgs e)
        {
            AppDialog.Show("特征树已重新从 CAD 宿主模型拓扑中解析同步。", "特征刷新", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnRecalculate_Click(object sender, RoutedEventArgs e)
        {
            AppDialog.Show("质量矩阵与惯性矩重新积分完成：0.842 kg。", "物理计算", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            AppDialog.Show("特征参数与属性已同步保存至宿主模型数据库！", "应用成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class FeatureParamItem
    {
        public string Name { get; set; }
        public string DesignValue { get; set; }
        public string Unit { get; set; }
        public string UpperTol { get; set; }
        public string LowerTol { get; set; }
        public string Note { get; set; }
    }
}
