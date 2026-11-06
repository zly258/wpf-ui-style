using System.Windows;

namespace WpfStylePreview
{
    /// <summary>
    /// 反模式对照窗口：复刻旧版《图签信息设置》的写法——
    /// 标签列固定 104px、窗口固定高度、状态文案与按钮同一行。
    /// 用来和 SafeFormWindow 对照，看“标签被裁 / 按钮被盖”的差别。
    /// </summary>
    public partial class AntiPatternWindow : Window
    {
        public AntiPatternWindow()
        {
            InitializeComponent();
        }

        internal void OnRenderHook() { }
    }
}
