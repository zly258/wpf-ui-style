using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfStylePreview
{
    /// <summary>
    /// 交互状态校验窗口：进入编辑态、展开面板、打开弹层后离屏渲染，
    /// 用来抓“静态看不出来、只有交互态才暴露”的缺陷。
    /// </summary>
    public partial class ProbeWindow : Window
    {
        public ProbeWindow()
        {
            InitializeComponent();
        }

        internal void OnRenderHook()
        {
            Grid.ItemsSource = new List<Row>
            {
                new Row { Code = "A-001", Name = "总装图", TextCount = 42, Status = "已完成" },
                new Row { Code = "A-002", Name = "零件图", TextCount = 18, Status = "翻译中" },
                new Row { Code = "A-003", Name = "明细表", TextCount = 63, Status = "待处理" },
                new Row { Code = "A-004", Name = "焊接图", TextCount = 27, Status = "待处理" },
                new Row { Code = "A-005", Name = "液压原理图", TextCount = 35, Status = "失败" },
            };

            // 让 DataGrid 进入编辑态：选中第二行第一列并开始编辑
            Grid.SelectedIndex = 1;
            Grid.CurrentColumn = Grid.Columns[1];
            Grid.CurrentCell = new DataGridCellInfo(1, Grid.Columns[1]);
            Grid.Focus();
            Grid.BeginEdit();


        }

        private static Popup FindPopup(DependencyObject root)
        {
            int n = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < n; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (child is Popup p) return p;
                var nested = FindPopup(child);
                if (nested != null) return nested;
            }
            return null;
        }

        /// <summary>布局完成后自检：日历弹层必须有内容、编辑器高度必须放得下。</summary>
        internal void AfterLayout()
        {
            var popup = FindPopup(PickerNormal);
            System.Console.WriteLine("DATEPOPUP child=" + (popup?.Child?.GetType().FullName ?? "<null>"));

            // 模拟在日历上选一天，确认能回写到 DatePicker
            if (popup?.Child is Calendar cal)
            {
                cal.SelectedDate = new System.DateTime(2026, 12, 1);
                System.Console.WriteLine("DATEPICKER selected=" + PickerNormal.SelectedDate?.ToString("yyyy-MM-dd"));
                cal.Measure(new Size(600, 600));
                System.Console.WriteLine($"CAL template={(cal.Template != null ? cal.Template.GetType().Name : "<null>")} measured={cal.DesiredSize.Width:0.0}x{cal.DesiredSize.Height:0.0} minW={cal.MinWidth} width={cal.Width}");
            }

            // 模拟点开下拉：弹层里必须有候选项
            Combo.IsDropDownOpen = true;
            Combo.UpdateLayout();
            var comboPopup = FindPopup(Combo);
            int itemCount = 0;
            if (comboPopup?.Child is ComboBoxItem ci) itemCount = 1;
            else if (comboPopup?.Child is Panel panel) itemCount = panel.Children.Count;
            System.Console.WriteLine("COMBOPOPUP child=" + (comboPopup?.Child?.GetType().Name ?? "<null>") + " items=" + itemCount);

            // 可编辑 ComboBox 必须正确装配 PART_EditableTextBox 并且与 Text 属性双向联动
            ComboEditable.UpdateLayout();
            var editableTb = ComboEditable.Template.FindName("PART_EditableTextBox", ComboEditable) as TextBox;
            System.Console.WriteLine("COMBOEDITABLE tb=" + (editableTb != null ? editableTb.GetType().Name : "<null>") + " text=" + editableTb?.Text);
            if (editableTb != null)
            {
                editableTb.Text = "输入新内容测试";
                System.Console.WriteLine("COMBOEDITABLE sync back combo.Text=" + ComboEditable.Text);
            }

            var readOnlyTb = ComboEditableReadOnly.Template.FindName("PART_EditableTextBox", ComboEditableReadOnly) as TextBox;
            System.Console.WriteLine("COMBOEDITABLE_READONLY isReadOnly=" + readOnlyTb?.IsReadOnly);

            // 单元格编辑器高度 vs 行高
            foreach (var row in Grid.ItemContainerGenerator.ContainerFromIndex(1) is null
                        ? new DataGridRow[0]
                        : new[] { (DataGridRow)Grid.ItemContainerGenerator.ContainerFromIndex(1) })
            {
                System.Console.WriteLine($"ROW h={row.ActualHeight:0.0}");
            }
        }

        public sealed class Row
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public int TextCount { get; set; }
            public string Status { get; set; }
        }
    }
}
