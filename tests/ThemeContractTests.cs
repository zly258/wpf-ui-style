using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace OpenCAD.WpfStyles.Tests
{
    /// <summary>
    /// 主题契约测试模板。
    ///
    /// 复制到项目测试目录后，确保 StylesRoot 能找到 wpf-styles：
    ///   - 设置环境变量 WPF_STYLES_ROOT；
    ///   - 或把 wpf-styles 放在测试项目上级目录。
    ///
    /// 这些测试防的是实际发生过的事故：
    ///   - 隐式 TextBlock 把 Foreground 写死，主按钮变成蓝底深灰字；
    ///   - 禁用态用 Opacity，文字看不清；
    ///   - 主按钮不再是白字；
    ///   - 对比度组合低于 WCAG AA。
    /// </summary>
    public sealed class ThemeContractTests
    {
        private static readonly XNamespace X =
            "http://schemas.microsoft.com/winfx/2006/xaml";

        private static string StylesRoot
        {
            get
            {
                string env = Environment.GetEnvironmentVariable("WPF_STYLES_ROOT");
                if (!string.IsNullOrWhiteSpace(env)) return env;

                var dir = new DirectoryInfo(AppContext.BaseDirectory);
                while (dir != null)
                {
                    string candidate = Path.Combine(dir.FullName, "wpf-styles");
                    if (Directory.Exists(candidate)) return candidate;

                    string sibling = Path.GetFullPath(
                        Path.Combine(dir.FullName, "..", "wpf-styles"));
                    if (Directory.Exists(sibling)) return sibling;

                    dir = dir.Parent;
                }

                return Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory, "..", "..", "..", "..", "..", "wpf-styles"));
            }
        }

        private static string TokensPath => Path.Combine(StylesRoot, "Theme", "Tokens.xaml");
        private static string ControlsPath => Path.Combine(StylesRoot, "Theme", "Controls.xaml");
        private static string DataControlsPath => Path.Combine(StylesRoot, "Theme", "DataControls.xaml");
        private static string IconsPath => Path.Combine(StylesRoot, "Theme", "Icons.xaml");

        [Fact]
        public void Typography_IsUnifiedMicrosoftYaHeiWithGlobalScale()
        {
            XDocument tokens = Load(TokensPath);

            string font = tokens.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "FontFamily"
                    && (string)e.Attribute(X + "Key") == "Font.Ui")?.Value ?? "";

            Assert.Contains("Microsoft YaHei", font);

            var sizes = tokens.Root.Elements()
                .Where(e => e.Name.LocalName == "Double")
                .ToDictionary(e => (string)e.Attribute(X + "Key") ?? "", e => e.Value);

            Assert.Equal("14.5", sizes["FontSize.Body"]);
            Assert.Equal("14", sizes["FontSize.Caption"]);
            Assert.Equal("13.5", sizes["FontSize.Code"]);
            Assert.Equal("15.5", sizes["FontSize.Title"]);
        }

        [Fact]
        public void CheckBox_And_RadioButton_HitTargetsAreAtLeast16()
        {
            XDocument tokens = Load(TokensPath);
            var doubles = tokens.Root.Elements()
                .Where(e => e.Name.LocalName == "Double")
                .ToDictionary(e => (string)e.Attribute(X + "Key") ?? "", e => double.Parse(e.Value));

            Assert.True(doubles["Layout.CheckBox.Size"] >= 16);
            Assert.True(doubles["Layout.RadioButton.Size"] >= 16);

            XDocument controls = Load(ControlsPath);
            XElement checkBoxTemplate = controls.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Border"
                    && (string)e.Attribute(X + "Name") == "Box");
            Assert.NotNull(checkBoxTemplate);
            Assert.Contains("Layout.CheckBox.Size", (string)checkBoxTemplate.Attribute("Width") ?? "");

            XElement radioTemplate = controls.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Border"
                    && (string)e.Attribute(X + "Name") == "Ring");
            Assert.NotNull(radioTemplate);
            Assert.Contains("Layout.RadioButton.Size", (string)radioTemplate.Attribute("Width") ?? "");
        }

        [Fact]
        public void IconBase_BindsToForegroundAndUniformStretch()
        {
            XElement iconBase = FindKeyedStyle(IconsPath, "Icon.Base");
            Assert.NotNull(iconBase);

            var setters = iconBase.Elements().Where(e => e.Name.LocalName == "Setter").ToList();
            Assert.Contains(setters, s => (string)s.Attribute("Property") == "Stretch" && (string)s.Attribute("Value") == "Uniform");
            
            XElement fillSetter = setters.FirstOrDefault(s => (string)s.Attribute("Property") == "Fill");
            Assert.NotNull(fillSetter);
            string fillVal = (string)fillSetter.Attribute("Value");
            Assert.Contains("(TextElement.Foreground)", fillVal);
        }

        [Fact]
        public void IconButton_UsesExplicitStatesAndNoOpacity()
        {
            XElement buttonIcon = FindKeyedStyle(IconsPath, "Button.Icon");
            Assert.NotNull(buttonIcon);

            XDocument doc = Load(IconsPath);
            XElement template = doc.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "ControlTemplate" && (string)e.Attribute(X + "Key") == "IconButton.Template");
            Assert.NotNull(template);

            XElement disabledTrigger = template.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Trigger" && (string)e.Attribute("Property") == "IsEnabled" && (string)e.Attribute("Value") == "False");
            Assert.NotNull(disabledTrigger);

            var setters = disabledTrigger.Elements().Where(e => e.Name.LocalName == "Setter").ToList();
            Assert.Contains(setters, s => (string)s.Attribute("Property") == "Foreground" && (string)s.Attribute("Value") == "{StaticResource Brush.Text.Disabled}");
            Assert.DoesNotContain(setters, s => (string)s.Attribute("Property") == "Opacity");
        }


        [Fact]
        public void GlobalTextBlock_ForegroundFollowsContentHost()
        {
            string value = ImplicitStyleSetterValue(ControlsPath, "TextBlock", "Foreground");

            Assert.Contains("(TextElement.Foreground)", value);
            Assert.Contains("FallbackValue=", value);
        }

        [Fact]
        public void GlobalAccessText_ForegroundFollowsContentHost()
        {
            string value = ImplicitStyleSetterValue(ControlsPath, "AccessText", "Foreground");

            Assert.Contains("(TextElement.Foreground)", value);
            Assert.Contains("FallbackValue=", value);
        }

        [Fact]
        public void PrimaryButton_IsWhiteOnAccent()
        {
            Assert.Equal("{StaticResource Brush.OnAccent}",
                KeyedStyleSetterValue(ControlsPath, "Button.Primary", "Foreground"));
            Assert.Equal("{StaticResource Brush.Accent}",
                KeyedStyleSetterValue(ControlsPath, "Button.Primary", "Background"));
        }

        [Fact]
        public void PrimaryButtonDisabled_UsesExplicitForegroundAndNoOpacity()
        {
            XElement style = FindKeyedStyle(ControlsPath, "Button.Primary");
            XElement disabled = FindTrigger(style, "IsEnabled", "False");
            Assert.NotNull(disabled);

            var setters = disabled.Elements()
                .Where(e => e.Name.LocalName == "Setter")
                .ToList();

            Assert.Contains(setters, s =>
                (string)s.Attribute("Property") == "Foreground"
                && (string)s.Attribute("Value") == "{StaticResource Brush.Text.Disabled}");

            Assert.DoesNotContain(setters, s =>
                (string)s.Attribute("Property") == "Opacity");
        }

        [Fact]
        public void NoDisabledTriggerUsesOpacity()
        {
            foreach (string path in new[] { ControlsPath, DataControlsPath })
            {
                XDocument doc = Load(path);
                foreach (XElement trigger in doc.Descendants()
                    .Where(e => e.Name.LocalName == "Trigger"
                        && (string)e.Attribute("Property") == "IsEnabled"
                        && (string)e.Attribute("Value") == "False"))
                {
                    Assert.DoesNotContain(trigger.Elements()
                            .Where(e => e.Name.LocalName == "Setter"),
                        s => (string)s.Attribute("Property") == "Opacity");
                }
            }
        }

        [Fact]
        public void ContrastPairs_MeetMinimum()
        {
            Dictionary<string, string> colors = LoadColors();
            var pairs = new[]
            {
                Pair("Color.TextPrimary", "Color.WindowBackground", 4.5),
                Pair("Color.TextPrimary", "Color.Surface", 4.5),
                Pair("Color.TextPrimary", "Color.SurfaceAlt", 4.5),
                Pair("Color.TextPrimary", "Color.SurfaceSunken", 4.5),
                Pair("Color.TextPrimary", "Color.AccentSubtle", 4.5),
                Pair("Color.TextPrimary", "Color.AccentSubtleHover", 4.5),
                Pair("Color.TextPrimary", "Color.ControlHover", 4.5),
                Pair("Color.TextPrimary", "Color.ControlPressed", 4.5),
                Pair("Color.TextPrimary", "Color.Selection", 4.5),
                Pair("Color.TextPrimary", "Color.SelectionHover", 4.5),
                Pair("Color.TextSecondary", "Color.WindowBackground", 4.5),
                Pair("Color.TextSecondary", "Color.Surface", 4.5),
                Pair("Color.TextSecondary", "Color.SurfaceAlt", 4.5),
                Pair("Color.TextSecondary", "Color.SurfaceSunken", 4.5),
                Pair("Color.TextDisabled", "Color.ControlDisabled", 3.0),
                Pair("Color.OnAccent", "Color.Accent", 4.5),
                Pair("Color.OnAccent", "Color.AccentHover", 4.5),
                Pair("Color.OnAccent", "Color.AccentPressed", 4.5),
                Pair("Color.OnAccent", "Color.Danger", 4.5),
                Pair("Color.OnAccent", "Color.DangerHover", 4.5),
                Pair("Color.OnAccent", "Color.DangerPressed", 4.5),
                Pair("Color.TextPrimary", "Color.DangerSubtle", 4.5),
                Pair("Color.TextPrimary", "Color.SuccessSubtle", 4.5),
                Pair("Color.TextPrimary", "Color.WarningSubtle", 4.5),
                Pair("Color.Danger", "Color.DangerSubtle", 4.5),
                Pair("Color.Success", "Color.SuccessSubtle", 4.5),
                Pair("Color.Warning", "Color.WarningSubtle", 4.5),
                Pair("Color.TooltipForeground", "Color.TooltipBackground", 4.5),
                Pair("Color.Focus", "Color.Surface", 3.0),
                Pair("Color.Focus", "Color.WindowBackground", 3.0)
            };

            foreach (var pair in pairs)
            {
                Assert.True(colors.ContainsKey(pair.Foreground),
                    "Missing token: " + pair.Foreground);
                Assert.True(colors.ContainsKey(pair.Background),
                    "Missing token: " + pair.Background);

                double ratio = Contrast(colors[pair.Foreground], colors[pair.Background]);
                Assert.True(ratio >= pair.Minimum,
                    string.Format("{0} / {1} = {2:0.00}:1, minimum {3:0.0}:1",
                        pair.Foreground, pair.Background, ratio, pair.Minimum));
            }
        }

        private sealed class ContrastPair
        {
            public string Foreground;
            public string Background;
            public double Minimum;
        }

        private static ContrastPair Pair(string foreground, string background, double minimum)
        {
            return new ContrastPair
            {
                Foreground = foreground,
                Background = background,
                Minimum = minimum
            };
        }

        private static Dictionary<string, string> LoadColors()
        {
            var colors = new Dictionary<string, string>(StringComparer.Ordinal);
            XDocument doc = Load(TokensPath);
            foreach (XElement node in doc.Root.Elements()
                .Where(e => e.Name.LocalName == "Color"))
            {
                string key = (string)node.Attribute(X + "Key");
                if (!string.IsNullOrWhiteSpace(key))
                    colors[key] = node.Value.Trim();
            }
            return colors;
        }

        private static double Contrast(string foreground, string background)
        {
            double lf = Luminance(foreground);
            double lb = Luminance(background);
            double hi = Math.Max(lf, lb);
            double lo = Math.Min(lf, lb);
            return (hi + 0.05) / (lo + 0.05);
        }

        private static double Luminance(string hex)
        {
            string value = hex.Trim().TrimStart('#');
            if (value.Length == 8) value = value.Substring(2);
            if (value.Length != 6) throw new FormatException("Invalid color: " + hex);

            double r = Convert.ToInt32(value.Substring(0, 2), 16) / 255.0;
            double g = Convert.ToInt32(value.Substring(2, 2), 16) / 255.0;
            double b = Convert.ToInt32(value.Substring(4, 2), 16) / 255.0;

            return 0.2126 * Linear(r) + 0.7152 * Linear(g) + 0.0722 * Linear(b);
        }

        private static double Linear(double c)
        {
            return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        private static string ImplicitStyleSetterValue(string path, string targetType, string property)
        {
            XElement style = Load(path).Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Style"
                    && (string)e.Attribute("TargetType") == "{x:Type " + targetType + "}"
                    && e.Attribute(X + "Key") == null);

            Assert.True(style != null,
                path + ": missing implicit style for " + targetType);

            return SetterValue(style, property, targetType + " implicit style");
        }

        private static string KeyedStyleSetterValue(string path, string styleKey, string property)
        {
            return SetterValue(FindKeyedStyle(path, styleKey), property, styleKey);
        }

        private static XElement FindKeyedStyle(string path, string styleKey)
        {
            XElement style = Load(path).Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Style"
                    && (string)e.Attribute(X + "Key") == styleKey);

            Assert.True(style != null, path + ": missing style " + styleKey);
            return style;
        }

        private static XElement FindTrigger(XElement style, string property, string value)
        {
            return style.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Trigger"
                    && (string)e.Attribute("Property") == property
                    && (string)e.Attribute("Value") == value);
        }

        private static string SetterValue(XElement style, string property, string label)
        {
            XElement setter = style.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Setter"
                    && (string)e.Attribute("Property") == property);

            Assert.True(setter != null, label + ": missing Setter for " + property);
            return (string)setter.Attribute("Value") ?? "";
        }

        private static XDocument Load(string path)
        {
            return XDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        }
    }
}
