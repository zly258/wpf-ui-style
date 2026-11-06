using System.Windows;

namespace WpfStylePreview
{
    /// <summary>
    /// 案例 1：密集设置表单。
    /// 演示紧凑骨架——三列并排、行距 3px、控件高 32px，
    /// 一屏看到更多字段；内容区只有一层 ScrollViewer。
    /// </summary>
    public partial class CaseSettingsWindow : Window
    {
        public CaseSettingsWindow()
        {
            InitializeComponent();
        }

        internal void OnRenderHook() { }
    }
}
