using System;
using System.Collections.Generic;
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
        public void TabItem_SelectedStateHasSurfaceBorderAndAccentBar()
        {
            XElement tabItem = Load(ControlsPath).Root.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Style"
                    && (string)e.Attribute("TargetType") == "{x:Type TabItem}"
                    && e.Attribute(X + "Key") == null);
            Assert.NotNull(tabItem);

            Assert.Equal("{DynamicResource Layout.TabPadding}",
                SetterValue(tabItem, "Padding", "TabItem"));
            Assert.Equal("{DynamicResource Layout.TabMargin}",
                SetterValue(tabItem, "Margin", "TabItem"));

            XElement underline = tabItem.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Border"
                    && (string)e.Attribute(X + "Name") == "Underline");
            Assert.NotNull(underline);
            Assert.Equal("2", (string)underline.Attribute("Height"));

            XElement selected = tabItem.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Trigger"
                    && (string)e.Attribute("Property") == "IsSelected"
                    && (string)e.Attribute("Value") == "True");
            Assert.NotNull(selected);

            var setters = selected.Elements()
                .Where(e => e.Name.LocalName == "Setter")
                .ToList();

            Assert.Contains(setters, s => (string)s.Attribute("TargetName") == "Bd"
                && (string)s.Attribute("Property") == "Background"
                && (string)s.Attribute("Value") == "{StaticResource Brush.Surface}");
            Assert.Contains(setters, s => (string)s.Attribute("TargetName") == "Bd"
                && (string)s.Attribute("Property") == "BorderBrush"
                && (string)s.Attribute("Value") == "{StaticResource Brush.Border}");
            Assert.Contains(setters, s => (string)s.Attribute("TargetName") == "Underline"
                && (string)s.Attribute("Property") == "Background"
                && (string)s.Attribute("Value") == "{StaticResource Brush.Accent}");
            Assert.Contains(setters, s => (string)s.Attribute("Property") == "Foreground"
                && (string)s.Attribute("Value") == "{StaticResource Brush.Accent}");
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
