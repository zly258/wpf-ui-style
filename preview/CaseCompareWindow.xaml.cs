using System.Windows;

namespace WpfStylePreview
{
    /// <summary>
    /// 案例 3：双栏对照 + 日志。
    /// 演示主内容占 * 行、日志固定高度、底部操作栏不被顶出的紧凑骨架。
    /// </summary>
    public partial class CaseCompareWindow : Window
    {
        public CaseCompareWindow()
        {
            InitializeComponent();
        }

        internal void OnRenderHook() { }
    }
}
