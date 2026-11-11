using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace WpfStyles.Tests
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
        private static string ExtrasPath => Path.Combine(StylesRoot, "Theme", "Extras.xaml");

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

            Assert.Equal("12", sizes["FontSize.Body"]);
            Assert.Equal("11.5", sizes["FontSize.Caption"]);
            Assert.Equal("11.5", sizes["FontSize.Code"]);
            Assert.Equal("13", sizes["FontSize.Title"]);
            Assert.Equal("16", sizes["FontSize.Display"]);
            Assert.Equal("10", sizes["FontSize.Micro"]);
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
        public void IconGeometry_UsesUniform24ViewportAnchor()
        {
            XDocument icons = Load(IconsPath);

            var geometries = icons.Root.Elements()
                .Where(e => e.Name.LocalName == "StreamGeometry"
                    && ((string)e.Attribute(X + "Key") ?? "").StartsWith("Icon.Geo."))
                .ToList();

            Assert.NotEmpty(geometries);

            foreach (XElement geo in geometries)
            {
                string key = (string)geo.Attribute(X + "Key");
                string data = geo.Value.Trim();

                // 零面积锚点保证 Stretch="Uniform" 始终按完整 24x24 视口缩放
                Assert.EndsWith("M0,0 L24,24", data);
            }
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
        public void ComboBox_Template_ContainsPartEditableTextBoxAndTriggers()
        {
            XElement style = FindKeyedStyle(ControlsPath, "ComboBox.Base");
            Assert.NotNull(style);

            // 1. 模板必须包含名为 PART_EditableTextBox 的 TextBox，保证 IsEditable="True" 时文本可输入
            XElement editableTextBox = style.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "TextBox"
                    && (string)e.Attribute(X + "Name") == "PART_EditableTextBox");
            Assert.NotNull(editableTextBox);
            Assert.Equal("{StaticResource ComboBox.EditableTextBox}", (string)editableTextBox.Attribute("Style"));

            // 2. 模板必须包含 PART_Popup
            XElement popup = style.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Popup"
                    && (string)e.Attribute(X + "Name") == "PART_Popup");
            Assert.NotNull(popup);

            // 3. 必须包含 IsEditable="True" 触发器：显示输入框、隐藏静态内容、禁用外部 Tab 停靠
            XElement isEditableTrigger = style.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Trigger"
                    && (string)e.Attribute("Property") == "IsEditable"
                    && (string)e.Attribute("Value") == "True");
            Assert.NotNull(isEditableTrigger);

            var setters = isEditableTrigger.Elements().Where(e => e.Name.LocalName == "Setter").ToList();
            Assert.Contains(setters, s => (string)s.Attribute("TargetName") == "PART_EditableTextBox"
                && (string)s.Attribute("Property") == "Visibility"
                && (string)s.Attribute("Value") == "Visible");
            Assert.Contains(setters, s => (string)s.Attribute("TargetName") == "ContentSite"
                && (string)s.Attribute("Property") == "Visibility"
                && (string)s.Attribute("Value") == "Collapsed");
            Assert.Contains(setters, s => (string)s.Attribute("Property") == "IsTabStop"
                && (string)s.Attribute("Value") == "False");

            // 4. ComboBox.EditableTextBox 专用样式必须存在，背景透明且包含 PART_ContentHost
            XElement editableStyle = FindKeyedStyle(ControlsPath, "ComboBox.EditableTextBox");
            Assert.NotNull(editableStyle);
            Assert.Equal("True", (string)editableStyle.Elements().FirstOrDefault(e => (string)e.Attribute("Property") == "OverridesDefaultStyle")?.Attribute("Value"));
            Assert.Equal("Transparent", (string)editableStyle.Elements().FirstOrDefault(e => (string)e.Attribute("Property") == "Background")?.Attribute("Value"));

            XElement contentHost = editableStyle.Descendants()
                .FirstOrDefault(e => (string)e.Attribute(X + "Name") == "PART_ContentHost");
            Assert.NotNull(contentHost);
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
        public void FormLayoutTokens_ExistAndAreUsable()
        {
            XDocument tokens = Load(TokensPath);
            var doubles = tokens.Root.Elements()
                .Where(e => e.Name.LocalName == "Double")
                .ToDictionary(e => (string)e.Attribute(X + "Key") ?? "", e => double.Parse(e.Value));

            Assert.True(doubles.ContainsKey("Layout.FormLabelMinWidth"));
            Assert.True(doubles.ContainsKey("Layout.InputMinWidth"));
            Assert.True(doubles.ContainsKey("Layout.WindowMinWidth"));

            // 标签列最小宽度要放得下常见中文标签，输入列要放得下一段可读文本
            Assert.True(doubles["Layout.FormLabelMinWidth"] >= 80);
            Assert.True(doubles["Layout.InputMinWidth"] >= 140);
            Assert.True(doubles["Layout.WindowMinWidth"] >= 480);

            // 列间距是 GridLength（ColumnDefinition.Width 不接受 Double）
            XElement gap = tokens.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "GridLength"
                    && (string)e.Attribute(X + "Key") == "Layout.ColumnGap");
            Assert.NotNull(gap);
            Assert.True(double.Parse(gap.Value) >= 8);
        }

        [Fact]
        public void FormRow_And_FormTextBox_UseAutoAndStarColumns()
        {
            XElement formRow = FindKeyedStyle(ControlsPath, "FormRow");
            Assert.Equal("{x:Type Grid}", (string)formRow.Attribute("TargetType"));
            Assert.Equal("{DynamicResource Layout.FormRowMargin}",
                SetterValue(formRow, "Margin", "FormRow"));

            XElement formTextBox = FindKeyedStyle(ControlsPath, "TextBox.Form");
            Assert.Equal("{StaticResource TextBox.Base}",
                (string)formTextBox.Attribute("BasedOn"));
            Assert.Equal("{StaticResource Layout.InputMinWidth}",
                SetterValue(formTextBox, "MinWidth", "TextBox.Form"));

            // 表单输入框不得写死宽度（会截断内容）
            Assert.DoesNotContain(formTextBox.Elements()
                    .Where(e => e.Name.LocalName == "Setter"),
                s => (string)s.Attribute("Property") == "Width");
        }

        [Fact]
        public void TextStyles_DoNotTrimTextByDefault()
        {
            foreach (string styleKey in new[] { "Text.Label", "Text.Caption" })
            {
                XElement style = FindKeyedStyle(TokensPath, styleKey);
                string trimming = SetterValue(style, "TextTrimming", styleKey);
                Assert.Equal("None", trimming);
            }
        }

        [Fact]
        public void ThemeFiles_HaveNoNegativeMargins()
        {
            foreach (string path in new[] { TokensPath, ControlsPath, DataControlsPath, IconsPath })
            {
                XDocument doc = Load(path);
                foreach (XElement node in doc.Descendants())
                {
                    string margin = (string)node.Attribute("Margin");
                    if (string.IsNullOrWhiteSpace(margin)) continue;

                    foreach (string part in margin.Split(','))
                    {
                        Assert.False(part.Trim().StartsWith("-"),
                            path + ": negative Margin '" + margin + "' causes overlapping controls");
                    }
                }
            }
        }

        [Fact]
        public void ThemeFiles_TrimmingAlwaysHasToolTip()
        {
            foreach (string path in new[] { TokensPath, ControlsPath, DataControlsPath, IconsPath })
            {
                XDocument doc = Load(path);
                foreach (XElement node in doc.Descendants())
                {
                    string trimming = (string)node.Attribute("TextTrimming");
                    string overflow = (string)node.Attribute("TextOverflow");

                    bool ellipsis =
                        (trimming != null && trimming.Contains("Ellipsis")) ||
                        (overflow != null && overflow.Contains("Ellipsis"));
                    if (!ellipsis) continue;

                    Assert.True(node.Attribute("ToolTip") != null,
                        path + ": ellipsis trimming without a ToolTip hides the full text");
                }
            }
        }

        [Fact]
        public void DataGridCell_CellAndHeaderShareTheSameLeftInset()
        {
            XDocument data = Load(DataControlsPath);

            // 单元格必须显式给模板：WPF 默认的 DataGridCell 模板不消费 Padding，
            // 内容会贴着列边界渲染，与表头文字左右错位。
            XElement cellTemplate = data.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "ControlTemplate"
                    && (string)e.Attribute(X + "Key") == "DataGridCell.Template");
            Assert.NotNull(cellTemplate);

            XElement presenter = cellTemplate.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "ContentPresenter");
            Assert.NotNull(presenter);
            Assert.Contains("TemplateBinding Padding", (string)presenter.Attribute("Margin") ?? "");

            string cellPadding = ImplicitStyleSetterValue(DataControlsPath, "DataGridCell", "Padding");
            string headerPadding = ImplicitStyleSetterValue(DataControlsPath, "DataGridColumnHeader", "Padding");

            double cellLeft = double.Parse(cellPadding.Split(',')[0]);
            double headerLeft = double.Parse(headerPadding.Split(',')[0]);

            Assert.Equal(headerLeft, cellLeft);
            Assert.True(cellLeft >= 8);
        }

        [Fact]
        public void Expander_LongContentScrollsInsteadOfGrowingTheWindow()
        {
            XDocument controls = Load(ControlsPath);
            XElement expander = controls.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Style"
                    && (string)e.Attribute("TargetType") == "{x:Type Expander}");

            Assert.NotNull(expander);

            XElement scroll = expander.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "ScrollViewer");
            Assert.NotNull(scroll);

            Assert.Equal("Auto", (string)scroll.Attribute("VerticalScrollBarVisibility"));
            Assert.Contains("Layout.ExpanderMaxHeight",
                (string)scroll.Attribute("MaxHeight") ?? "");
        }

        [Fact]
        public void DataGrid_RowHeightFitsTheCellEditor()
        {
            string rowHeight = ImplicitStyleSetterValue(DataControlsPath, "DataGrid", "RowHeight");
            Assert.Contains("Layout.RowHeight", rowHeight);

            // 行高必须容得下 18px 正文行高 + 编辑器内边距，三档都不小于 24px
            foreach (string profile in DensityProfiles)
            {
                double value = double.Parse(DensityProfile(profile)["Layout.RowHeight"].Value);
                Assert.True(value >= 24, "Layout.RowHeight is too small in " + profile);
            }
        }

        [Fact]
        public void Controls_DoNotImposeExcessiveMinHeight()
        {
            // DataGridRow、TextBox、PasswordBox 不得锁死高 MinHeight，确保弹性网格和紧凑布局能自适应收缩
            string dataGridRowMinH = ImplicitStyleSetterValue(DataControlsPath, "DataGridRow", "MinHeight");
            Assert.Equal("0", dataGridRowMinH);

            string textBoxMinH = KeyedStyleSetterValue(ControlsPath, "TextBox.Base", "MinHeight");
            Assert.Equal("0", textBoxMinH);

            string pwdBoxMinH = KeyedStyleSetterValue(ControlsPath, "PasswordBox.Base", "MinHeight");
            Assert.Equal("0", pwdBoxMinH);
        }

        [Fact]
        public void ScrollBar_TrackFollowsBarOrientation()
        {
            XDocument controls = Load(ControlsPath);
            XElement scrollBar = controls.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Style"
                    && (string)e.Attribute("TargetType") == "{x:Type ScrollBar}");

            Assert.NotNull(scrollBar);

            XElement track = scrollBar.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Track");
            Assert.NotNull(track);

            // Track 必须跟随 ScrollBar 的方向，否则竖向滚动条的滑块会横着排、几乎看不见
            Assert.Contains("TemplateBinding Orientation",
                (string)track.Attribute("Orientation") ?? "");
        }

        [Fact]
        public void DatePicker_UsesOwnTemplateWithCalendarInPopup()
        {
            XDocument extras = Load(ExtrasPath);
            XElement picker = extras.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Style"
                    && (string)e.Attribute(X + "Key") == "DatePicker.Base");

            Assert.NotNull(picker);
            Assert.Contains("DatePicker.Template",
                SetterValue(picker, "Template", "DatePicker.Base"));

            XElement template = extras.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "ControlTemplate"
                    && (string)e.Attribute(X + "Key") == "DatePicker.Template");
            Assert.NotNull(template);

            // 弹层里必须有日历，否则点开是空白
            XElement popup = template.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Popup");
            Assert.NotNull(popup);
            Assert.NotNull(popup.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Calendar"));

            // 文本框必须 Stretch：默认按内容收缩会把最后一个字裁掉
            XElement textBoxStyle = extras.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Style"
                    && (string)e.Attribute(X + "Key") == "DatePickerTextBox.Base");
            Assert.NotNull(textBoxStyle);
            Assert.Equal("Stretch",
                SetterValue(textBoxStyle, "HorizontalAlignment", "DatePickerTextBox.Base"));

            // 日历按钮不能被隐式 ToggleButton 的 MinWidth 撑宽
            XElement toggle = template.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "ToggleButton");
            Assert.NotNull(toggle);
            Assert.Equal("0", (string)toggle.Attribute("MinWidth"));

            // 按钮尺寸必须跟随密度：写死 26 会在紧凑档（控件高 26）里被上下裁掉
            Assert.Equal("Auto", (string)toggle.Attribute("Height"));
            Assert.Equal("Stretch", (string)toggle.Attribute("VerticalAlignment"));
        }

        [Fact]
        public void Calendar_HasAMinWidthSoThePopupCannotCollapse()
        {
            XDocument extras = Load(ExtrasPath);
            XElement calendar = extras.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Style"
                    && (string)e.Attribute("TargetType") == "{x:Type Calendar}");

            Assert.NotNull(calendar);

            // DatePicker 会按文本框宽度推算日历宽度，布局早期可能算成 0 ——
            // 面板会塌成竖条，星期标题被挤成一排细线。
            string minWidth = SetterValue(calendar, "MinWidth", "Calendar");
            double value = double.Parse(minWidth.Replace("{StaticResource ", "").Replace("}", ""));
            Assert.True(value >= 220, "Calendar.MinWidth is too small: " + minWidth);

            // 模板里的 Calendar 也要有 MinWidth：弹层里的实例可能不继承隐式样式
            XElement template = extras.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "ControlTemplate"
                    && (string)e.Attribute(X + "Key") == "DatePicker.Template");
            Assert.NotNull(template);

            XElement calendarInPopup = template.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Calendar");
            Assert.NotNull(calendarInPopup);

            string popupMinWidth = (string)calendarInPopup.Attribute("MinWidth") ?? "";
            Assert.True(double.Parse(popupMinWidth) >= 220,
                "DatePicker.Template 的 Calendar 缺少 MinWidth，弹层可能塌成竖条");
        }

        private static readonly string[] DensityProfiles = { "Compact", "Medium", "Relaxed" };

        private static string ThemeFile(string name) => Path.Combine(StylesRoot, "Theme", name);

        private static Dictionary<string, XElement> KeyedResources(string path)
        {
            return Load(path).Root.Elements()
                .Where(e => !string.IsNullOrWhiteSpace((string)e.Attribute(X + "Key")))
                .ToDictionary(e => (string)e.Attribute(X + "Key"), e => e);
        }

        private static Dictionary<string, XElement> DensityProfile(string name)
        {
            return KeyedResources(ThemeFile("Density." + name + ".xaml"));
        }

        [Fact]
        public void DensityProfiles_DefineTheSameLayoutKeys()
        {
            Dictionary<string, XElement> tokens = KeyedResources(TokensPath);
            Dictionary<string, XElement> compact = DensityProfile("Compact");
            Dictionary<string, XElement> medium = DensityProfile("Medium");
            Dictionary<string, XElement> relaxed = DensityProfile("Relaxed");

            string[] compactKeys = compact.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();
            Assert.Equal(compactKeys, medium.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray());
            Assert.Equal(compactKeys, relaxed.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray());

            foreach (string key in compactKeys)
            {
                // Tokens 必须保留同名兜底值，单独合并 Controls 的项目才不会缺资源。
                Assert.True(tokens.ContainsKey(key), "Tokens.xaml is missing density key " + key);
                Assert.Equal(tokens[key].Name.LocalName, compact[key].Name.LocalName);
            }

            Assert.Equal("compact", compact["Density.Profile"].Value.Trim());
            Assert.Equal("medium", medium["Density.Profile"].Value.Trim());
            Assert.Equal("relaxed", relaxed["Density.Profile"].Value.Trim());
        }

        [Fact]
        public void ThemeEntries_MergeTheMatchingDensityProfile()
        {
            foreach (var pair in new[]
            {
                new KeyValuePair<string, string>("Theme.xaml", "Density.Medium.xaml"),
                new KeyValuePair<string, string>("Theme.Compact.xaml", "Density.Compact.xaml"),
                new KeyValuePair<string, string>("Theme.Relaxed.xaml", "Density.Relaxed.xaml")
            })
            {
                var sources = Load(ThemeFile(pair.Key)).Root
                    .Descendants()
                    .Where(e => e.Name.LocalName == "ResourceDictionary")
                    .Select(e => (string)e.Attribute("Source"))
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                Assert.Contains(pair.Value, sources);
                Assert.Contains("Tokens.xaml", sources);
                Assert.Contains("Controls.xaml", sources);
                // 密度必须最后合并，否则会被控件字典内嵌套的 Tokens 中等兜底盖住
                Assert.Equal(pair.Value, sources.Last());
            }
        }

        [Fact]
        public void Controls_ReadDensityTokensThroughDynamicResource()
        {
            HashSet<string> densityKeys = new HashSet<string>(
                DensityProfile("Medium").Keys, StringComparer.OrdinalIgnoreCase);

            var dynamicRe = new Regex(@"\{DynamicResource\s+(Layout\.[A-Za-z0-9.]+)\}");
            var staticRe = new Regex(@"\{StaticResource\s+(Layout\.[A-Za-z0-9.]+)\}");

            foreach (string path in new[] { ControlsPath, DataControlsPath, ExtrasPath, IconsPath })
            {
                string text = File.ReadAllText(path, Encoding.UTF8);

                foreach (Match match in staticRe.Matches(text))
                {
                    string key = match.Groups[1].Value;
                    Assert.False(densityKeys.Contains(key),
                        path + " must read density token " + key + " with DynamicResource");
                }

                foreach (Match match in dynamicRe.Matches(text))
                {
                    string key = match.Groups[1].Value;
                    Assert.True(densityKeys.Contains(key),
                        path + " references unknown density token " + key);
                }
            }
        }

        [Fact]
        public void DensityProfiles_AreStrictlyOrderedCompactMediumRelaxed()
        {
            var compact = DensityProfile("Compact");
            var medium = DensityProfile("Medium");
            var relaxed = DensityProfile("Relaxed");

            foreach (string key in new[]
            {
                "Layout.ControlHeight",
                "Layout.ControlHeight.Small",
                "Layout.ControlHeight.Large",
                "Layout.RowHeight",
                "Layout.HeaderHeight",
                "Layout.ButtonMinWidth",
                "Layout.FormLabelMaxWidth",
                "Layout.ProgressBarHeight"
            })
            {
                double c = double.Parse(compact[key].Value);
                double m = double.Parse(medium[key].Value);
                double r = double.Parse(relaxed[key].Value);
                Assert.True(c < m && m < r, key + " must grow: " + c + " < " + m + " < " + r);
            }

            foreach (string key in new[]
            {
                "Layout.TabPadding",
                "Layout.InputPadding",
                "Layout.FormRowMargin",
                "Layout.ListItemPadding"
            })
            {
                double[] c = compact[key].Value.Split(',').Select(double.Parse).ToArray();
                double[] m = medium[key].Value.Split(',').Select(double.Parse).ToArray();
                double[] r = relaxed[key].Value.Split(',').Select(double.Parse).ToArray();
                Assert.True(c[0] <= m[0] && m[0] <= r[0], key + " left/start inset must grow");
            }

            // 按钮上下内边距必须为 0：高度由 Height 控制，避免“按钮显得过高”。
            foreach (var profile in new[] { compact, medium, relaxed })
            {
                double[] padding = profile["Layout.ButtonPadding"].Value.Split(',').Select(double.Parse).ToArray();
                Assert.Equal(2, padding.Length);
                Assert.Equal(0, padding[1]);
            }
        }

        [Fact]
        public void InputPadding_StaysTightAndHasNoVerticalInset()
        {
            // 输入框左右内边距过大时，文本会离边框太远、可用宽度变少。
            foreach (string profile in DensityProfiles)
            {
                var tokens = DensityProfile(profile);

                double[] single = tokens["Layout.InputPadding"].Value.Split(',').Select(double.Parse).ToArray();
                Assert.Equal(2, single.Length);
                Assert.True(single[0] <= 8, "Layout.InputPadding horizontal is too large in " + profile);
                Assert.Equal(0, single[1]);

                double[] multiline = tokens["Layout.InputPadding.Multiline"].Value.Split(',').Select(double.Parse).ToArray();
                Assert.True(multiline[0] <= 8, "multiline horizontal padding is too large in " + profile);

                double[] combo = tokens["Layout.ComboBoxPadding"].Value.Split(',').Select(double.Parse).ToArray();
                Assert.True(combo[0] <= 8, "ComboBox left padding is too large in " + profile);
            }
        }

        [Fact]
        public void TextBox_ContentHostFillsAvailableWidth()
        {
            XElement baseStyle = FindKeyedStyle(ControlsPath, "TextBox.Base");
            Assert.Equal("Stretch",
                SetterValue(baseStyle, "HorizontalContentAlignment", "TextBox.Base"));

            XDocument controls = Load(ControlsPath);
            XElement host = controls.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "ScrollViewer"
                    && (string)e.Attribute(X + "Name") == "PART_ContentHost");
            Assert.NotNull(host);
            Assert.Equal("Stretch", (string)host.Attribute("HorizontalAlignment"));
            Assert.DoesNotContain("HorizontalContentAlignment",
                (string)host.Attribute("HorizontalAlignment") ?? "");
        }

        [Fact]
        public void TabItem_UsesNativeBoxedTabs()
        {
            XDocument controls = Load(ControlsPath);
            XElement tabItem = controls.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Style"
                    && (string)e.Attribute("TargetType") == "{x:Type TabItem}"
                    && e.Attribute(X + "Key") == null);
            Assert.NotNull(tabItem);

            Assert.Equal("{DynamicResource Layout.TabPadding}",
                SetterValue(tabItem, "Padding", "TabItem"));

            // 方角原生页签：没有圆角、没有底部强调条
            Assert.Null(tabItem.Descendants()
                .FirstOrDefault(e => (string)e.Attribute(X + "Name") == "Underline"));

            XElement bd = tabItem.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Border"
                    && (string)e.Attribute(X + "Name") == "Bd");
            Assert.NotNull(bd);
            Assert.Null(bd.Attribute("CornerRadius"));
            // 未选中也有完整 1px 边框，页签边界可见
            Assert.Equal("1,1,1,1", (string)bd.Attribute("BorderThickness"));

            XElement selected = tabItem.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Trigger"
                    && (string)e.Attribute("Property") == "IsSelected"
                    && (string)e.Attribute("Value") == "True");
            Assert.NotNull(selected);

            var setters = selected.Elements()
                .Where(e => e.Name.LocalName == "Setter")
                .ToList();

            Assert.Contains(setters, x => (string)x.Attribute("TargetName") == "Bd"
                && (string)x.Attribute("Property") == "Background"
                && (string)x.Attribute("Value") == "{StaticResource Brush.Surface}");
            // 选中页签去掉下边框，与内容区连成一体
            Assert.Contains(setters, x => (string)x.Attribute("TargetName") == "Bd"
                && (string)x.Attribute("Property") == "BorderThickness"
                && (string)x.Attribute("Value") == "1,1,1,0");
            Assert.Contains(setters, x => (string)x.Attribute("Property") == "FontWeight"
                && (string)x.Attribute("Value") == "SemiBold");

            // 内容区是完整方框（左边/右边/下边），上边线由标签栏基线提供
            XElement content = controls.Descendants()
                .Where(e => e.Name.LocalName == "Border"
                    && (string)e.Attribute("BorderThickness") == "1,0,1,1")
                .FirstOrDefault();
            Assert.NotNull(content);
            Assert.Equal("{StaticResource Brush.Surface}", (string)content.Attribute("Background"));
        }

        [Fact]
        public void TabItem_FrameUsesSofterBorderTabToken()
        {
            XDocument controls = Load(ControlsPath);
            XElement tabItem = controls.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Style"
                    && (string)e.Attribute("TargetType") == "{x:Type TabItem}"
                    && e.Attribute(X + "Key") == null);
            Assert.NotNull(tabItem);

            // 页签方框走比 Border.Control 浅一档的 Border.Tab
            Assert.Equal("{StaticResource Brush.Border.Tab}",
                SetterValue(tabItem, "BorderBrush", "TabItem"));

            XElement selected = FindTrigger(tabItem, "IsSelected", "True");
            Assert.NotNull(selected);
            Assert.Contains(selected.Elements(), x => (string)x.Attribute("TargetName") == "Bd"
                && (string)x.Attribute("Property") == "BorderBrush"
                && (string)x.Attribute("Value") == "{StaticResource Brush.Border.Tab}");

            // 内容区方框和标签栏基线必须与页签同色，否则方框会断开
            var frame = controls.Descendants()
                .Where(e => e.Name.LocalName == "Border"
                    && (string)e.Attribute("BorderBrush") == "{StaticResource Brush.Border.Tab}")
                .ToList();
            Assert.True(frame.Count >= 2, "TabControl 的标签栏基线与内容区方框也该用 Border.Tab");

            // 按钮/输入框仍保持 3:1 的 Border.Control，不能被浅一档令牌顶替
            Assert.Equal("{StaticResource Brush.Border.Control}",
                KeyedStyleSetterValue(ControlsPath, "Button.Base", "BorderBrush"));
        }

        [Fact]
        public void ToolTip_WrapsLongTextWithinMaxWidth()
        {
            XElement toolTip = Load(ControlsPath).Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Style"
                    && (string)e.Attribute("TargetType") == "{x:Type ToolTip}"
                    && e.Attribute(X + "Key") == null);
            Assert.NotNull(toolTip);

            Assert.Equal("460", SetterValue(toolTip, "MaxWidth", "ToolTip"));

            // 字符串内容由 ContentPresenter 生成 TextBlock，必须显式给 Wrap，
            // 否则长提示会一路撑到屏幕外（历史事故：换行根本没生效）。
            XElement presenter = toolTip.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "ContentPresenter");
            Assert.NotNull(presenter);

            XElement wrapping = presenter.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Setter"
                    && (string)e.Attribute("Property") == "TextWrapping");
            Assert.True(wrapping != null, "ToolTip 内容缺少 TextWrapping=\"Wrap\"");
            Assert.Equal("Wrap", (string)wrapping.Attribute("Value"));
        }

        [Fact]
        public void SecondaryButton_HasVisibleBorderAndCompactHeight()
        {
            XElement baseStyle = FindKeyedStyle(ControlsPath, "Button.Base");
            Assert.Equal("{StaticResource Brush.Border.Control}",
                SetterValue(baseStyle, "BorderBrush", "Button.Base"));
            Assert.Equal("{DynamicResource Layout.ControlHeight}",
                SetterValue(baseStyle, "Height", "Button.Base"));

            XElement secondary = FindKeyedStyle(ControlsPath, "Button.Secondary");
            XElement hover = FindTrigger(secondary, "IsMouseOver", "True");
            Assert.NotNull(hover);
            Assert.Equal("{StaticResource Brush.Accent}",
                SetterValue(hover, "BorderBrush", "Button.Secondary hover"));
        }

        [Fact]
        public void GroupBox_HeaderAndBodyShareTheSameLeftInset()
        {
            XElement groupBox = Load(ControlsPath).Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Style"
                    && (string)e.Attribute("TargetType") == "{x:Type GroupBox}"
                    && e.Attribute(X + "Key") == null);
            Assert.NotNull(groupBox);

            XElement dock = groupBox.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "DockPanel");
            Assert.NotNull(dock);
            Assert.Equal("{TemplateBinding Padding}", (string)dock.Attribute("Margin"));

            var presenters = dock.Elements()
                .Where(e => e.Name.LocalName == "ContentPresenter")
                .ToList();
            Assert.Equal(2, presenters.Count);
            Assert.Equal("Top", (string)presenters[0].Attribute("DockPanel.Dock"));
        }

        [Fact]
        public void FormAlignmentHelpers_ExistAndKeepContentLeft()
        {
            XElement combo = FindKeyedStyle(ControlsPath, "ComboBox.Base");
            Assert.Equal("Left", SetterValue(combo, "HorizontalContentAlignment", "ComboBox.Base"));

            XElement label = FindKeyedStyle(TokensPath, "Text.FieldLabel");
            Assert.Equal("Left", SetterValue(label, "HorizontalAlignment", "Text.FieldLabel"));
            Assert.Equal("{StaticResource Text.Label}", (string)label.Attribute("BasedOn"));

            XElement stack = FindKeyedStyle(ControlsPath, "FormStack");
            Assert.Equal("{x:Type StackPanel}", (string)stack.Attribute("TargetType"));
            Assert.Equal("{DynamicResource Layout.FormRowMargin}",
                SetterValue(stack, "Margin", "FormStack"));
        }

        [Fact]
        public void DangerButton_DisabledStateIsNeutral()
        {
            // 危险按钮禁用时必须中性化：不允许保留红底/红字/红边框，
            // 否则“批量删除”在没有任何选中项时看起来仍然可用。
            XElement danger = FindKeyedStyle(ControlsPath, "Button.Danger");
            XElement disabled = FindTrigger(danger, "IsEnabled", "False");
            Assert.NotNull(disabled);

            Assert.Equal("{StaticResource Brush.Control.Disabled}",
                SetterValue(disabled, "Background", "Button.Danger disabled"));
            Assert.Equal("{StaticResource Brush.Text.Disabled}",
                SetterValue(disabled, "Foreground", "Button.Danger disabled"));
            Assert.Equal("{StaticResource Brush.Border}",
                SetterValue(disabled, "BorderBrush", "Button.Danger disabled"));
        }

        [Fact]
        public void ToolStrip_StyleExistsForBulkActionBars()
        {
            XElement strip = FindKeyedStyle(ExtrasPath, "ToolStrip");
            Assert.Equal("{x:Type Border}", (string)strip.Attribute("TargetType"));
            Assert.Equal("{StaticResource Brush.Surface}",
                SetterValue(strip, "Background", "ToolStrip"));
            Assert.Equal("{StaticResource Brush.Border}",
                SetterValue(strip, "BorderBrush", "ToolStrip"));
            Assert.Equal("{DynamicResource Layout.ToolBarPadding}",
                SetterValue(strip, "Padding", "ToolStrip"));
        }

        [Fact]
        public void CheckBox_CheckIsAFilledCenteredGlyph()
        {
            XDocument controls = Load(ControlsPath);
            XElement check = controls.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Path"
                    && (string)e.Attribute(X + "Name") == "Check");
            Assert.NotNull(check);

            // 对勾必须是填充路径：细描边在 16px 下会发虚、显细
            Assert.Null(check.Attribute("Stroke"));
            Assert.NotNull(check.Attribute("Fill"));

            // 几何在 16×16 视口内左右上下居中，且足够大（>=6px 跨距）
            MatchCollection numbers = Regex.Matches((string)check.Attribute("Data"), @"-?\d+(?:\.\d+)?");
            var xs = new List<double>();
            var ys = new List<double>();
            for (int i = 0; i + 1 < numbers.Count; i += 2)
            {
                xs.Add(double.Parse(numbers[i].Value, CultureInfo.InvariantCulture));
                ys.Add(double.Parse(numbers[i + 1].Value, CultureInfo.InvariantCulture));
            }

            Assert.True(xs.Count >= 3, "check geometry is too simple");

            // 几何必须从 (0,0) 起笔：Path 的 DesiredSize 取的是“到原点的外包”，
            // 若从 (2.6,3.8) 起笔，Center 对齐后墨迹会整体右下偏移（约 1.3px / 1.9px），
            // 视觉上就是不居中。起笔归零后 DesiredSize == 外包，对齐才真正居中。
            Assert.True(Math.Abs(xs.Min()) <= 0.01, "check geometry must start at x=0, otherwise it renders off-center");
            Assert.True(Math.Abs(ys.Min()) <= 0.01, "check geometry must start at y=0, otherwise it renders off-center");

            double glyphWidth = xs.Max() - xs.Min();
            double glyphHeight = ys.Max() - ys.Min();
            Assert.True(glyphWidth >= 6.0, "check is too small horizontally");
            Assert.True(glyphHeight >= 5.0, "check is too small vertically");

            // 16px 方框去掉 1px 边框后内容区为 14×14，字形必须放得下才不会被裁切
            Assert.True(glyphWidth <= 14.0 && glyphHeight <= 14.0, "check overflows the 16px box content area");

            // 外包取偶数：内容区 14×14，SnapsToDevicePixels 会把 (14-w)/2 取整，
            // 奇数宽高取整后墨迹会偏半个像素，偶数才能稳稳落在正中
            Assert.True(Math.Abs(glyphWidth % 2) <= 0.01, "check width must be even to stay pixel-centered");
            Assert.True(Math.Abs(glyphHeight % 2) <= 0.01, "check height must be even to stay pixel-centered");
        }

        [Fact]
        public void CellControlHeight_IsRowHeightMinusGridLine()
        {
            // 数据行底部有 1px 网格线。单元格内控件若按行高撑满，hover/展开态的描边
            // 会正好压在分隔线上，看起来像溢到下一行——所以单元格控件高度必须留 2px。
            var densityFiles = new[]
            {
                TokensPath,
                Path.Combine(StylesRoot, "Theme", "Density.Compact.xaml"),
                Path.Combine(StylesRoot, "Theme", "Density.Medium.xaml"),
                Path.Combine(StylesRoot, "Theme", "Density.Relaxed.xaml")
            };

            foreach (string file in densityFiles)
            {
                XDocument doc = Load(file);
                var doubles = doc.Root.Elements()
                    .Where(e => e.Name.LocalName == "Double")
                    .ToDictionary(e => (string)e.Attribute(X + "Key") ?? "", e => e.Value);

                Assert.True(doubles.ContainsKey("Layout.CellControlHeight"),
                    $"{Path.GetFileName(file)} is missing Layout.CellControlHeight");
                double row = double.Parse(doubles["Layout.RowHeight"], CultureInfo.InvariantCulture);
                double cell = double.Parse(doubles["Layout.CellControlHeight"], CultureInfo.InvariantCulture);
                Assert.Equal(2.0, row - cell, 2);
            }
        }

        [Fact]
        public void CellComboBox_UsesInsetHeightNotRowHeight()
        {
            XDocument controls = Load(DataControlsPath);
            XElement style = controls.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Style"
                    && (string)e.Attribute(X + "Key") == "DataGrid.Cell.ComboBox");
            Assert.NotNull(style);

            Assert.Equal("{DynamicResource Layout.CellControlHeight}",
                SetterValue(style, "MinHeight", "DataGrid.Cell.ComboBox"));
        }

        [Fact]
        public void ComboBox_HidesEmptyDropDownShell()
        {
            // ItemsSource 为空（绑定失败/异步加载中）时，Popup 的 Border 只剩
            // Padding + 边框，会在光标下方留一条 6px 的空胶囊。
            XDocument controls = Load(ControlsPath);
            XElement template = controls.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "ControlTemplate"
                    && (string)e.Attribute("TargetType") == "{x:Type ComboBox}");
            Assert.NotNull(template);

            bool guarded = template.Descendants()
                .Where(e => e.Name.LocalName == "Trigger")
                .Any(t => (string)t.Attribute("Property") == "HasItems"
                    && t.Elements().Any(s => (string)s.Attribute("TargetName") == "DropDownBorder"
                        && (string)s.Attribute("Property") == "Visibility"
                        && (string)s.Attribute("Value") == "Collapsed"));
            Assert.True(guarded, "empty ComboBox must collapse its DropDownBorder");
        }

        [Fact]
        public void TextBox_ContentHostDoesNotDoubleApplyPadding()
        {
            // TextBox 会把 Padding 交给 PART_ContentHost；模板若再给它一个
            // Margin="{TemplateBinding Padding}"，左内边距就变成两倍（6px→12px），
            // 同一表单里输入框的文字比下拉框多缩进一大截。
            XDocument controls = Load(ControlsPath);
            XElement template = controls.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "ControlTemplate"
                    && (string)e.Attribute(X + "Key") == "TextBox.Template");
            Assert.NotNull(template);

            XElement host = template.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "ScrollViewer"
                    && (string)e.Attribute(X + "Name") == "PART_ContentHost");
            Assert.NotNull(host);
            Assert.Null(host.Attribute("Margin"));
        }

        [Fact]
        public void CheckBox_WithoutContentHasNoLabelMargin()
        {
            // 隐式 CheckBox/RadioButton 带的是“文字与方框的间距”。表头全选框、
            // 单元格里的独立复选框没有 Content，这个间距会把它们推离中心。
            foreach (var (path, target) in new[]
            {
                (ControlsPath, "{x:Type CheckBox}"),
                (ControlsPath, "{x:Type RadioButton}")
            })
            {
                XDocument doc = Load(path);
                XElement style = doc.Root.Elements()
                    .FirstOrDefault(e => e.Name.LocalName == "Style"
                        && (string)e.Attribute("TargetType") == target
                        && e.Attribute(X + "Key") == null);
                Assert.NotNull(style);

                bool guarded = style.Elements()
                    .Where(e => e.Name.LocalName == "Style.Triggers")
                    .SelectMany(e => e.Elements())
                    .Any(t => (string)t.Attribute("Property") == "HasContent"
                        && t.Elements().Any(s => (string)s.Attribute("Property") == "Margin"
                            && (string)s.Attribute("Value") == "0"));
                Assert.True(guarded, target + " must drop its label margin when it has no content");
            }
        }

        [Fact]
        public void KeyedControlStyles_DoNotSilentlyFallBackToSystemDefaults()
        {
            // WPF 里带 x:Key 的 Style 不会继承隐式样式。一个 keyed 样式若既没 BasedOn、
            // 也没自己的 Template，用它的控件就静默退回系统默认外观——表格里的复选框
            // 就是这样变成方角深灰框的，和旁边用隐式样式的表头复选框长得不一样。
            var files = new[] { TokensPath, ControlsPath, DataControlsPath, IconsPath, ExtrasPath };
            var docs = files.ToDictionary(f => f, Load);

            var templatedTargets = new HashSet<string>();
            foreach (var doc in docs.Values)
            {
                foreach (var style in doc.Root.Elements().Where(e => e.Name.LocalName == "Style"))
                {
                    if (style.Attribute(X + "Key") != null) continue;
                    if (!SetsTemplate(style)) continue;
                    templatedTargets.Add((string)style.Attribute("TargetType") ?? "");
                }
            }

            var offenders = new List<string>();
            foreach (var kv in docs)
            {
                foreach (var style in kv.Value.Root.Elements().Where(e => e.Name.LocalName == "Style"))
                {
                    string key = (string)style.Attribute(X + "Key");
                    string target = (string)style.Attribute("TargetType") ?? "";
                    if (string.IsNullOrEmpty(key) || key.StartsWith("{x:Static")) continue;
                    if (!templatedTargets.Contains(target)) continue;
                    if (style.Attribute("BasedOn") != null || SetsTemplate(style)) continue;
                    offenders.Add($"{Path.GetFileName(kv.Key)}:{key} ({target})");
                }
            }

            Assert.True(offenders.Count == 0,
                "keyed styles inherit nothing from the implicit style and render as system defaults: "
                + string.Join(", ", offenders));
        }

        private static bool SetsTemplate(XElement style)
        {
            return style.Elements().Any(s =>
                s.Name.LocalName == "Setter" && (string)s.Attribute("Property") == "Template");
        }

        [Fact]
        public void ItemContainers_DefineTheirOwnTemplate()
        {
            // WPF 默认的 TreeViewItem / ListBoxItem / ListViewItem 模板用
            // SystemColors.HighlightBrushKey 画选中（系统蓝 #0078D7 + 白字），
            // 样式里只写 Background/Foreground 触发器根本不起作用。
            // 这几类容器必须自带模板，否则选中态一定跑成系统默认外观。
            var docs = new[] { DataControlsPath, ExtrasPath, ControlsPath }.Select(Load);

            var templates = new HashSet<string>();
            foreach (var doc in docs)
            {
                foreach (var t in doc.Root.Elements().Where(e => e.Name.LocalName == "ControlTemplate"))
                {
                    string target = (string)t.Attribute("TargetType") ?? "";
                    if (target.Contains("ListBoxItem") || target.Contains("TreeViewItem"))
                        templates.Add(target);
                }
            }

            var offenders = new List<string>();
            foreach (var doc in docs)
            {
                foreach (var style in doc.Root.Elements().Where(e => e.Name.LocalName == "Style"))
                {
                    string target = (string)style.Attribute("TargetType") ?? "";
                    if (!target.EndsWith("TreeViewItem}") && !target.EndsWith("ListBoxItem}")
                        && !target.EndsWith("ListViewItem}"))
                        continue;

                    bool ownTemplate = SetsTemplate(style);
                    bool basedOn = style.Attribute("BasedOn") != null;
                    if (!ownTemplate && !basedOn) offenders.Add(target);
                }
            }

            Assert.True(offenders.Count == 0,
                "item containers fall back to the system template, whose selection uses "
                + "SystemColors.HighlightBrushKey (system blue + white text): "
                + string.Join(", ", offenders));
            Assert.NotEmpty(templates);
        }

        [Fact]
        public void CheckBox_SupportsIndeterminateAndDisabledStates()
        {
            XDocument controls = Load(ControlsPath);
            XElement style = controls.Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Style"
                    && (string)e.Attribute("TargetType") == "{x:Type CheckBox}"
                    && e.Attribute(X + "Key") == null);
            Assert.NotNull(style);

            XElement dash = style.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Rectangle"
                    && (string)e.Attribute(X + "Name") == "Dash");
            Assert.NotNull(dash);

            // 未确定态（全选框部分选中）不能退化成空框
            XElement indeterminate = style.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Trigger"
                    && (string)e.Attribute("Property") == "IsChecked"
                    && (string)e.Attribute("Value") == "{x:Null}");
            Assert.NotNull(indeterminate);
            Assert.Contains(indeterminate.Elements(),
                e => (string)e.Attribute("TargetName") == "Dash"
                    && (string)e.Attribute("Property") == "Visibility"
                    && (string)e.Attribute("Value") == "Visible");

            // 禁用态：勾与横杠都转灰，不留白 glyph
            XElement disabled = style.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Trigger"
                    && (string)e.Attribute("Property") == "IsEnabled"
                    && (string)e.Attribute("Value") == "False");
            Assert.NotNull(disabled);
            Assert.Contains(disabled.Elements(),
                e => (string)e.Attribute("TargetName") == "Check"
                    && (string)e.Attribute("Property") == "Fill");
            Assert.Contains(disabled.Elements(),
                e => (string)e.Attribute("TargetName") == "Dash"
                    && (string)e.Attribute("Property") == "Fill");
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
                Pair("Color.Focus", "Color.WindowBackground", 3.0),
                // 按钮/输入框描边属于非文字 UI 组件，必须与相邻表面有 3:1
                Pair("Color.BorderControl", "Color.Surface", 3.0),
                Pair("Color.BorderControl", "Color.WindowBackground", 3.0),
                Pair("Color.BorderControl", "Color.ControlHover", 3.0)
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
