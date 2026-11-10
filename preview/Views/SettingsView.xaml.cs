using System.Windows;
using System.Windows.Controls;

namespace WpfStylePreview.Views
{
    /// <summary>
    /// 密集设置表单。
    /// 演示紧凑骨架——三列并排、行距与控件高度取自当前密度，
    /// 一屏看到更多字段；内容区只有一层 ScrollViewer。
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        internal void OnRenderHook() { }
    }
}
