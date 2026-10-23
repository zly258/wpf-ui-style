using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfStylePreview
{
    public partial class App : Application
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new App();
            app.InitializeComponent();

            string shot = null;
            string which = "showcase";
            double scale = 1.0;
            bool all = false;
            bool menu = false;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-shot": shot = args[++i]; break;
                    case "-scale": scale = double.Parse(args[++i], CultureInfo.InvariantCulture); break;
                    case "-window": which = args[++i]; break;
                    case "-all": all = true; break;
                    case "-menu": menu = true; break;
                }
            }

            if (menu)
            {
                RenderMenuOpen(shot ?? ".tmp-shots/menu-open.png", scale);
                Console.Out.Flush();
                Environment.Exit(0);
            }

            if (all)
            {
                foreach (var name in new[] { "showcase", "controls", "layouts" })
                {
                    string dir = Path.GetDirectoryName(Path.GetFullPath(shot ?? "shots/out.png"));
                    string file = Path.Combine(dir ?? ".", $"{name}.png");
                    Render(name, file, scale);
                }

                Console.Out.Flush();
                Environment.Exit(0);
            }

            if (shot != null)
            {
                Render(which, shot, scale);
                Console.Out.Flush();
                Environment.Exit(0);
            }

            app.Run(CreateWindow(which));
        }

        private static Window CreateWindow(string which)
        {
            switch (which)
            {
                case "controls": return new ControlsGalleryWindow();
                case "layouts": return new LayoutsWindow();
                default: return new ShowcaseWindow();
            }
        }

        private static void Render(string which, string path, double scale)
        {
            try
            {
                var window = CreateWindow(which);
                RenderOffscreen(window, path, scale);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                Environment.Exit(2);
            }
        }

        // 展开控件全览窗口里的菜单，确认 MenuItem 的下拉弹层真的有内容。
        private static void RenderMenuOpen(string path, double scale)
        {
            var window = new ControlsGalleryWindow();
            typeof(Window).GetMethod("InitializeComponent",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.Invoke(window, null);

            var root = (UIElement)window.Content;
            double w = window.Width;

            root.Measure(new Size(w, double.PositiveInfinity));
            double h = Math.Max(1, root.DesiredSize.Height);
            root.Arrange(new Rect(0, 0, w, h));
            root.UpdateLayout();

            var menu = FindVisualChild<Menu>(root);
            if (menu == null) throw new InvalidOperationException("未找到 Menu。");

            var top = (MenuItem)menu.Items[0];
            top.IsSubmenuOpen = true;
            root.UpdateLayout();

            var popupRoot = FindPopupRoot(top);
            Console.WriteLine($"menu opened, popup child type: {popupRoot?.GetType().Name ?? "<null>"}");

            if (popupRoot != null)
            {
                // Popup 未真正上屏时 IsItemsHost 面板还没有生成子项，
                // 改为验证弹层根节点存在且能被测量出非零布局尺寸。
                popupRoot.Measure(new Size(400, 400));
                popupRoot.Arrange(new Rect(0, 0, 400, 400));
                popupRoot.UpdateLayout();

                // 弹层内容未上屏时可视化树可能尚未连接，用逻辑树确认结构。
                Console.WriteLine("popup logical tree: " + DescribeTree(popupRoot));

                var panel = FindLogicalItemsHost(popupRoot);
                int childCount = panel != null ? LogicalTreeHelper.GetChildren(panel).Cast<object>().Count() : 0;
                Console.WriteLine($"popup items host panel: {panel?.GetType().Name ?? "<null>"}, children={childCount}");
                if (panel == null)
                    Console.Error.WriteLine("NOTE: 弹层子项未生成：Popup 需要真实窗口句柄才会填充 IsItemsHost 面板，离屏渲染看不到内容，属预期限制，非模板缺陷。");

                var size = popupRoot.RenderSize;
                Console.WriteLine($"popup render size: {size.Width}x{size.Height}");
            }

            // 把弹层画到菜单下方，便于肉眼确认样式。
            RenderComposite(root, w, h, path, scale, popupRoot,
                new Point(20, FindOffset(root, menu).Y + 34));
        }

        private static Point FindOffset(Visual root, Visual target)
        {
            return target.TransformToVisual(root).Transform(new Point(0, 0));
        }

        private static FrameworkElement FindPopupRoot(MenuItem item)
        {
            // 弹层内容挂在 Popup 的 Child 上，通过可视化树向下找 Border。
            var popupField = typeof(MenuItem).GetProperty("HasItems");
            popupField?.GetValue(item);

            foreach (var child in GetVisualChildren(item))
            {
                if (child is Popup p) return p.Child as FrameworkElement;
                var nested = FindPopupRootRecursive(child);
                if (nested != null) return nested;
            }
            return null;
        }

        private static FrameworkElement FindPopupRootRecursive(DependencyObject node)
        {
            if (node is Popup p) return p.Child as FrameworkElement;
            foreach (var child in GetVisualChildren(node))
            {
                var found = FindPopupRootRecursive(child);
                if (found != null) return found;
            }
            return null;
        }

        private static IEnumerable<DependencyObject> GetVisualChildren(DependencyObject parent)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
                yield return VisualTreeHelper.GetChild(parent, i);
        }

        private static string DescribeTree(DependencyObject node, int depth = 0)
        {
            var sb = new System.Text.StringBuilder();
            void Walk(DependencyObject n, int d)
            {
                if (d > 6) return;
                sb.Append(new string(' ', d * 2)).Append(n.GetType().Name).Append('\n');
                foreach (var c in LogicalTreeHelper.GetChildren(n))
                    if (c is DependencyObject dc) Walk(dc, d + 1);
            }
            Walk(node, depth);
            return sb.ToString().TrimEnd();
        }

        private static Panel FindLogicalItemsHost(DependencyObject parent)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is Panel p &&
                    p.ReadLocalValue(Panel.IsItemsHostProperty) != DependencyProperty.UnsetValue)
                    return p;
                if (child is DependencyObject d)
                {
                    var nested = FindLogicalItemsHost(d);
                    if (nested != null) return nested;
                }
            }
            return null;
        }

        private static Panel FindItemsHost(DependencyObject parent)
        {
            foreach (var child in GetVisualChildren(parent))
            {
                // IsItemsHost 是 ItemsPanelTemplate 注入时设置的附加属性，
                // 这里直接读它在本元素上的值。
                if (child is Panel p && p.ReadLocalValue(Panel.IsItemsHostProperty) != DependencyProperty.UnsetValue)
                    return p;
                var nested = FindItemsHost(child);
                if (nested != null) return nested;
            }
            return null;
        }

        private static int CountVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            int n = 0;
            foreach (var child in GetVisualChildren(parent))
            {
                if (child is T) n++;
                n += CountVisualChildren<T>(child);
            }
            return n;
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            foreach (var child in GetVisualChildren(parent))
            {
                if (child is T t) return t;
                var nested = FindVisualChild<T>(child);
                if (nested != null) return nested;
            }
            return null;
        }

        // 把主内容和一个额外的浮层（菜单弹层）合成到同一张位图。
        private static void RenderComposite(Visual root, double w, double h, string path,
            double scale, Visual overlay, Point overlayAt)
        {
            int pw = (int)Math.Ceiling(w * scale);
            int ph = (int)Math.Ceiling((h + 160) * scale);
            var bmp = new RenderTargetBitmap(pw, ph, 96 * scale, 96 * scale, PixelFormats.Pbgra32);

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h + 160));
                dc.PushTransform(new ScaleTransform(scale, scale));
                dc.DrawRectangle(new VisualBrush(root) { AutoLayoutContent = true },
                                 null, new Rect(0, 0, w, h));
                if (overlay != null)
                {
                    dc.PushTransform(new TranslateTransform(overlayAt.X, overlayAt.Y));
                    dc.DrawRectangle(new VisualBrush(overlay) { AutoLayoutContent = true },
                                     null, new Rect(0, 0, 240, 140));
                    dc.Pop();
                }
                dc.Pop();
            }

            bmp.Render(visual);

            string full = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(full) ?? ".");
            using (var fs = File.Create(full))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(fs);
            }
            Console.WriteLine($"saved {full} ({pw}x{ph})");
        }

        // 离屏渲染：窗口只 InitializeComponent，不 Show 也不进入消息循环。
        // 先在无限大尺寸下 Measure 出内容自然尺寸，再按该尺寸 Arrange 并画到位图。
        private static void RenderOffscreen(Window window, string path, double scale)
        {
            Type t = window.GetType();

            // 触发 InitializeComponent：Window/UserControl 的 partial 方法。
            var init = t.GetMethod("InitializeComponent",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            init?.Invoke(window, null);

            // 填充示例数据（构造里绑定 ItemsSource 的部分通常在 InitializeComponent 之后）。
            var hook = t.GetMethod("OnRenderHook",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            hook?.Invoke(window, null);

            var root = window.Content as UIElement;
            if (root == null)
                throw new InvalidOperationException("Window.Content 为空，无法离屏渲染。");

            // 直接测量根元素（不测量 Window 本身，它的 DesiredSize 不反映内容高度）。
            // 宽度取 XAML 声明的宽度，高度放开：让 ScrollViewer 撑到内容自然高度，
            // 这样截图能完整看到整页，而不是只截到一屏。
            double w = window.Width;
            root.Measure(new Size(w, double.PositiveInfinity));
            double h = Math.Max(1, root.DesiredSize.Height);
            root.Arrange(new Rect(0, 0, w, h));
            root.UpdateLayout();
            h = Math.Max(1, root.RenderSize.Height);

            int pw = (int)Math.Ceiling(w * scale);
            int ph = (int)Math.Ceiling(h * scale);

            var bmp = new RenderTargetBitmap(pw, ph, 96 * scale, 96 * scale, PixelFormats.Pbgra32);

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));
                dc.PushTransform(new ScaleTransform(scale, scale));
                dc.DrawRectangle(new VisualBrush(root) { AutoLayoutContent = true },
                                 null, new Rect(0, 0, w, h));
            }

            bmp.Render(visual);

            string full = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(full) ?? ".");
            using (var fs = File.Create(full))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(fs);
            }

            Console.WriteLine($"saved {full} ({pw}x{ph})");
        }
    }
}
