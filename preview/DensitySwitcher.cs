using System;
using System.Windows;

namespace WpfStylePreview
{
    /// <summary>
    /// 运行时密度切换。主题控件全部使用 {DynamicResource Layout.*}，
    /// 因此在应用级合并字典末尾替换 Density.*.xaml 即可整窗换挡。
    /// </summary>
    internal static class DensitySwitcher
    {
        public static void Apply(string profile)
        {
            if (string.IsNullOrWhiteSpace(profile)) return;

            if (string.Equals(profile, "Compact", StringComparison.OrdinalIgnoreCase)) profile = "Compact";
            else if (string.Equals(profile, "Relaxed", StringComparison.OrdinalIgnoreCase)) profile = "Relaxed";
            else profile = "Medium";

            Application app = Application.Current;
            if (app == null) return;

            var dictionary = new ResourceDictionary
            {
                Source = new Uri(
                    "pack://application:,,,/WpfStylePreview;component/Theme/Density." + profile + ".xaml",
                    UriKind.Absolute)
            };

            var merged = app.Resources.MergedDictionaries;
            for (int i = merged.Count - 1; i >= 0; i--)
            {
                Uri source = merged[i].Source;
                if (source != null &&
                    source.OriginalString.IndexOf("/Density.", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    merged.RemoveAt(i);
                }
            }

            merged.Add(dictionary);
        }
    }
}
