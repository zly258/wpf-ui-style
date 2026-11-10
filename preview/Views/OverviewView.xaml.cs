using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WpfStylePreview.Shell;

namespace WpfStylePreview.Views
{
    /// <summary>
    /// 首页：规范定位、核心约束与全部案例入口。
    /// 通过 CaseRequested 事件请求主壳切换案例，自身不持有导航逻辑。
    /// </summary>
    public partial class OverviewView : UserControl
    {
        public OverviewView()
        {
            DataContext = this;
            InitializeComponent();
        }

        public event Action<string> CaseRequested;

        public IReadOnlyList<CaseDescriptor> Cases => CaseCatalog.Cases;

        public void OnRenderHook() { }

        private void CaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string key)
            {
                CaseRequested?.Invoke(key);
            }
        }
    }
}
