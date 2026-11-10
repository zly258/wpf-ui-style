using System.Windows;
using System.Windows.Controls;

namespace WpfStylePreview.Views
{
    /// <summary>
    /// 安全表单骨架示例（规则 R14）：标签列 Auto + MinWidth、输入列 * + MinWidth、
    /// 独立运行时 SizeToContent + 一层 ScrollViewer、状态文案与按钮栏分行。
    /// 用于人眼确认“标签不裁、值不藏、按钮不被盖”。
    /// </summary>
    public partial class SafeFormView : UserControl
    {
        public SafeFormView()
        {
            InitializeComponent();
        }

        internal void OnRenderHook() { }
    }
}
