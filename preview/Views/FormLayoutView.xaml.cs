using System.Windows.Controls;

namespace WpfStylePreview.Views
{
    /// <summary>
    /// 表单对齐示例：宽面板两列、窄面板单列，标签与控件共享左基线。
    /// </summary>
    public partial class FormLayoutView : UserControl
    {
        public FormLayoutView()
        {
            InitializeComponent();
        }

        internal void OnRenderHook() { }
    }
}
