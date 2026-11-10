using System.Windows.Controls;

namespace WpfStylePreview.Views
{
    public partial class ControlsGalleryView : UserControl
    {
        public ControlsGalleryView()
        {
            InitializeComponent();
        }

        // 离屏渲染时由 App 调用，用于填充数据绑定内容。
        internal void OnRenderHook() { }
    }
}
