using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfStylePreview.Shell
{
    /// <summary>
    /// 把单个案例页包成独立窗口，供 -window 截图、UiTest 交互测试和“独立窗口”按钮使用。
    /// 页面本身是 UserControl，仍可被主壳复用，窗口只负责尺寸/标题/背景。
    /// </summary>
    public sealed class CaseHostWindow : Window
    {
        private readonly UserControl _view;

        public CaseHostWindow(CaseDescriptor descriptor)
        {
            _view = descriptor.Create();

            Title = "WPF UI Skills · " + descriptor.Title;
            Width = descriptor.Width;
            Height = descriptor.Height;
            MinWidth = 720;
            MinHeight = 480;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = (Brush)Application.Current.FindResource("Brush.WindowBackground");
            Content = _view;
        }

        /// <summary>离屏渲染/UiTest 会通过反射调用；转交给页面自身。</summary>
        internal void OnRenderHook()
        {
            Invoke(_view, "OnRenderHook");
        }

        internal void AfterLayout()
        {
            Invoke(_view, "AfterLayout");
        }

        private static void Invoke(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            method?.Invoke(target, null);
        }
    }
}
