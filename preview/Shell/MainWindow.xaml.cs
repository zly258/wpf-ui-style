using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WpfStylePreview.Views;

namespace WpfStylePreview.Shell
{
    /// <summary>
    /// 案例中心主壳：左侧分类导航，右侧承载任意案例页（UserControl）。
    /// 页面按需创建并缓存，切换导航不会重复构建可视树。
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, UserControl> _pages = new Dictionary<string, UserControl>();
        private CaseDescriptor _current;
        private bool _densityReady;

        public MainWindow(string initialKey = "overview")
        {
            InitializeComponent();

            var view = new ListCollectionView(CaseCatalog.All.ToList());
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(CaseDescriptor.Category)));
            NavList.ItemsSource = view;

            string profile = Application.Current?.TryFindResource("Density.Profile") as string;
            DensityCombo.SelectedIndex =
                string.Equals(profile, "compact", StringComparison.OrdinalIgnoreCase) ? 0 :
                string.Equals(profile, "relaxed", StringComparison.OrdinalIgnoreCase) ? 2 : 1;

            _densityReady = true;

            NavList.SelectedItem = CaseCatalog.Find(initialKey) ?? CaseCatalog.All[0];
        }

        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavList.SelectedItem is CaseDescriptor descriptor)
            {
                ShowCase(descriptor);
            }
        }

        private void ShowCase(CaseDescriptor descriptor)
        {
            _current = descriptor;
            PageTitle.Text = descriptor.Title;
            PageSubtitle.Text = descriptor.Subtitle;
            StandaloneButton.IsEnabled = !descriptor.IsHome;

            if (!_pages.TryGetValue(descriptor.Key, out UserControl page))
            {
                page = descriptor.Create();
                if (page is OverviewView overview)
                {
                    overview.CaseRequested += OpenCase;
                }
                _pages[descriptor.Key] = page;
            }

            PageHost.Content = page;
        }

        private void OpenCase(string key)
        {
            CaseDescriptor target = CaseCatalog.Find(key);
            if (target != null)
            {
                NavList.SelectedItem = target;
            }
        }

        private void DensityCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_densityReady) return;

            string profile;
            switch (DensityCombo.SelectedIndex)
            {
                case 0: profile = "Compact"; break;
                case 2: profile = "Relaxed"; break;
                default: profile = "Medium"; break;
            }

            DensitySwitcher.Apply(profile);
        }

        private void StandaloneButton_Click(object sender, RoutedEventArgs e)
        {
            if (_current == null || _current.IsHome) return;
            new CaseHostWindow(_current).Show();
        }
    }
}
