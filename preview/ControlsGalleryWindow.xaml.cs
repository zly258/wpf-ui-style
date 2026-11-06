using System.Windows;

namespace WpfStylePreview
{
    public partial class ControlsGalleryWindow : Window
    {
        public ControlsGalleryWindow()
        {
            InitializeComponent();
        }

        // 离屏渲染时由 App 调用，用于填充数据绑定内容。
        internal void OnRenderHook() { }
    }
}
