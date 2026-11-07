using System.Collections.ObjectModel;
using System.Windows;

namespace WpfStylePreview
{
    public partial class ShowcaseWindow : Window
    {
        public ObservableCollection<DrawingRow> Drawings { get; } = new ObservableCollection<DrawingRow>
        {
            new DrawingRow { Code = "A-001", Name = "总装图", TextCount = 42, Status = "已完成" },
            new DrawingRow { Code = "A-002", Name = "零件图", TextCount = 18, Status = "翻译中" },
            new DrawingRow { Code = "A-003", Name = "明细表", TextCount = 63, Status = "待处理" },
            new DrawingRow { Code = "A-004", Name = "焊接图", TextCount = 27, Status = "待处理" },
            new DrawingRow { Code = "A-005", Name = "液压原理图", TextCount = 35, Status = "失败" },
        };

        public ShowcaseWindow()
        {
            InitializeComponent();
            GridDrawings.ItemsSource = Drawings;
        }

        private void BtnOpenEditableTable_Click(object sender, RoutedEventArgs e)
        {
            new CaseEditableTableWindow().Show();
        }

        private void BtnOpenWorkbench_Click(object sender, RoutedEventArgs e)
        {
            new CaseWorkbenchWindow().Show();
        }

        private void BtnOpenWizard_Click(object sender, RoutedEventArgs e)
        {
            new CaseWizardWindow().Show();
        }

        private void BtnOpenDashboard_Click(object sender, RoutedEventArgs e)
        {
            new CaseDashboardWindow().Show();
        }

        private void BtnOpenTable_Click(object sender, RoutedEventArgs e)
        {
            new CaseTableWindow().Show();
        }

        private void BtnOpenCompare_Click(object sender, RoutedEventArgs e)
        {
            new CaseCompareWindow().Show();
        }

        private void BtnOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            new CaseSettingsWindow().Show();
        }

        private void BtnOpenLayouts_Click(object sender, RoutedEventArgs e)
        {
            new LayoutsWindow().Show();
        }

        private void BtnOpenControls_Click(object sender, RoutedEventArgs e)
        {
            new ControlsGalleryWindow().Show();
        }
    }

    public class DrawingRow
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int TextCount { get; set; }
        public string Status { get; set; }
    }
}
