using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace WpfStylePreview
{
    /// <summary>
    /// 对话框与消息弹窗管理。
    /// 在自动化测试（UiTest）、离屏渲染或无头模式下，全面静默 MessageBox，
    /// 自动返回确认结果，彻底避免弹出模态窗阻塞测试执行。
    /// </summary>
    public static class AppDialog
    {
        /// <summary>
        /// 全局开关：当为 true 时，静默所有 MessageBox 弹窗。
        /// </summary>
        public static bool Suppressed { get; set; } = false;

        /// <summary>
        /// 弹窗替代方法：Suppressed 时静默返回默认结果并输出日志，避免阻塞测试。
        /// </summary>
        public static MessageBoxResult Show(
            string messageBoxText,
            string caption = "",
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None)
        {
            if (Suppressed)
            {
                Console.WriteLine($"[UiTest Suppressed MessageBox] {caption}: {messageBoxText}");
                return (button == MessageBoxButton.YesNo || button == MessageBoxButton.YesNoCancel)
                    ? MessageBoxResult.Yes
                    : MessageBoxResult.OK;
            }

            return MessageBox.Show(messageBoxText, caption, button, icon);
        }
    }

    /// <summary>
    /// Win32 CBT Hook 自动关闭器：兜底防御即使第三方或未知代码直接调用原生 MessageBox.Show，
    /// 也能在窗口激活的 0ms 内自动投递确认关闭消息，绝不阻塞测试线程。
    /// </summary>
    public sealed class MessageBoxAutoCloser : IDisposable
    {
        private IntPtr _hook = IntPtr.Zero;
        private readonly HookProc _hookProc;

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_CBT = 5;
        private const int HCBT_ACTIVATE = 5;
        private const uint WM_COMMAND = 0x0111;
        private const int IDOK = 1;
        private const int IDYES = 6;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        public MessageBoxAutoCloser()
        {
            _hookProc = HookCallback;
            _hook = SetWindowsHookEx(WH_CBT, _hookProc, IntPtr.Zero, GetCurrentThreadId());
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == HCBT_ACTIVATE)
            {
                var sb = new StringBuilder(256);
                GetClassName(wParam, sb, 256);
                // #32770 为 Win32 对话框 / MessageBox 标准系统类名
                if (sb.ToString() == "#32770")
                {
                    PostMessage(wParam, WM_COMMAND, (IntPtr)IDOK, IntPtr.Zero);
                    PostMessage(wParam, WM_COMMAND, (IntPtr)IDYES, IntPtr.Zero);
                }
            }
            return CallNextHookEx(_hook, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            if (_hook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hook);
                _hook = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }

        ~MessageBoxAutoCloser()
        {
            Dispose();
        }
    }
}
