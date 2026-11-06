using System.Collections.ObjectModel;
using System.Windows;

namespace WpfStylePreview
{
    /// <summary>
    /// 案例 2：数据表格。
    /// 演示紧凑表格骨架——26px 行高、28px 表头，工具栏与分页条各占一行。
    /// </summary>
    public partial class CaseTableWindow : Window
    {
        public ObservableCollection<CaseRow> Rows { get; } = new ObservableCollection<CaseRow>
        {
            new CaseRow { Code = "A-001", Name = "总装图.dwg", TextCount = 42, Status = "已完成", Note = "术语库命中 12 条" },
            new CaseRow { Code = "A-002", Name = "零件图.dwg", TextCount = 18, Status = "翻译中", Note = "第 12 / 18 条" },
            new CaseRow { Code = "A-003", Name = "明细表.dwg", TextCount = 63, Status = "待处理", Note = "—" },
            new CaseRow { Code = "A-004", Name = "焊接图.dwg", TextCount = 27, Status = "待处理", Note = "—" },
            new CaseRow { Code = "A-005", Name = "液压原理图.dwg", TextCount = 35, Status = "失败", Note = "术语冲突：3 处" },
            new CaseRow { Code = "A-006", Name = "装配示意图.dwg", TextCount = 12, Status = "待处理", Note = "—" },
            new CaseRow { Code = "A-007", Name = "齿轮箱剖视图.dwg", TextCount = 88, Status = "待处理", Note = "—" },
            new CaseRow { Code = "A-008", Name = "紧固件明细.dwg", TextCount = 24, Status = "待处理", Note = "—" },
        };

        public CaseTableWindow()
        {
            InitializeComponent();
            OnRenderHook();
        }

        internal void OnRenderHook()
        {
            if (GridDrawings != null && GridDrawings.ItemsSource == null)
                GridDrawings.ItemsSource = Rows;
        }
    }

    public class CaseRow
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int TextCount { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
    }
}
