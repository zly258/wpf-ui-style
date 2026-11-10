using System.Windows;
using System.Windows.Controls;

namespace WpfStylePreview.Views
{
    /// <summary>
    /// 双栏对照 + 日志。
    /// 演示主内容占 * 行、日志固定高度、底部操作栏不被顶出的紧凑骨架。
    /// </summary>
    public partial class CompareView : UserControl
    {
        public CompareView()
        {
            InitializeComponent();
        }

        internal void OnRenderHook() { }
    }
}
