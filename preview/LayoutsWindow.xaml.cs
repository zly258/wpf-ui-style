using System.Collections.ObjectModel;
using System.Windows;

namespace WpfStylePreview
{
    public partial class LayoutsWindow : Window
    {
        public ObservableCollection<DrawingRow> Drawings { get; } = new ObservableCollection<DrawingRow>
        {
            new DrawingRow { Code = "A-001", Name = "总装图", TextCount = 42, Status = "已完成" },
            new DrawingRow { Code = "A-002", Name = "零件图", TextCount = 18, Status = "翻译中" },
            new DrawingRow { Code = "A-003", Name = "明细表", TextCount = 63, Status = "待处理" },
            new DrawingRow { Code = "A-004", Name = "焊接图", TextCount = 27, Status = "待处理" },
            new DrawingRow { Code = "A-005", Name = "液压原理图", TextCount = 35, Status = "失败" },
        };

        public LayoutsWindow()
        {
            InitializeComponent();
            OnRenderHook();
        }

        internal void OnRenderHook()
        {
            if (GridLayouts != null && GridLayouts.ItemsSource == null)
                GridLayouts.ItemsSource = Drawings;
        }
    }
}
