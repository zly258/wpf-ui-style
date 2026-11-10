using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfStylePreview.Views
{
    /// <summary>
    /// 案例：向导分步对话框布局（Wizard）。
    /// 演示：
    /// 1. 顶部步骤条（Step Indicator）状态联动与高亮；
    /// 2. 中间步骤容器按状态切换，无 TabControl 带来的高度跳变问题；
    /// 3. 底部上一步、下一步/完成、取消的标准化流程控制；
    /// 4. 步骤说明文字独立排布，防遮挡与截断。
    /// </summary>
    public partial class WizardView : UserControl
    {
        private int _currentStep = 1;

        public WizardView()
        {
            InitializeComponent();
            UpdateStepView();
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 1)
            {
                _currentStep--;
                UpdateStepView();
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep < 3)
            {
                _currentStep++;
                UpdateStepView();
            }
            else
            {
                // 完成步骤
                PanelProgress.Visibility = Visibility.Visible;
                BtnPrev.IsEnabled = false;
                BtnNext.IsEnabled = false;
                TxtStepGuide.Text = "批处理任务正在执行中，请稍候...";
                AppDialog.Show("批处理向导执行完成！已成功转换 12 份工程图纸。", "向导完成", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetWizard();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // 案例页嵌在主壳里，取消返回第 1 步而不是关闭整个应用。
            ResetWizard();
        }

        private void ResetWizard()
        {
            _currentStep = 1;
            PanelProgress.Visibility = Visibility.Collapsed;
            UpdateStepView();
        }

        private void UpdateStepView()
        {
            var accentBrush = Application.Current.FindResource("Brush.Accent") as Brush;
            var onAccentBrush = Application.Current.FindResource("Brush.OnAccent") as Brush;
            var surfaceAltBrush = Application.Current.FindResource("Brush.SurfaceAlt") as Brush;
            var textSecBrush = Application.Current.FindResource("Brush.Text.Secondary") as Brush;
            var textPrimaryBrush = Application.Current.FindResource("Brush.Text.Primary") as Brush;
            var successSubtleBrush = Application.Current.FindResource("Brush.Success.Subtle") as Brush;
            var successBrush = Application.Current.FindResource("Brush.Success") as Brush;

            // 切换面板显示
            PanelStep1.Visibility = _currentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
            PanelStep2.Visibility = _currentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
            PanelStep3.Visibility = _currentStep == 3 ? Visibility.Visible : Visibility.Collapsed;

            // 按钮状态与文案
            BtnPrev.IsEnabled = _currentStep > 1;
            BtnNext.Content = _currentStep == 3 ? "开始批处理" : "下一步";

            // 步骤 1 指示器状态
            if (_currentStep == 1)
            {
                Step1Badge.Background = accentBrush;
                Step1Number.Foreground = onAccentBrush;
                Step1Title.Foreground = textPrimaryBrush;
                TxtStepGuide.Text = "第 1 步：请勾选或添加待处理的图纸文件，然后点击“下一步”。";
            }
            else
            {
                Step1Badge.Background = successSubtleBrush;
                Step1Number.Foreground = successBrush;
                Step1Number.Text = "✓";
                Step1Title.Foreground = textSecBrush;
            }

            // 步骤 2 指示器状态
            if (_currentStep == 2)
            {
                Step2Badge.Background = accentBrush;
                Step2Number.Foreground = onAccentBrush;
                Step2Number.Text = "2";
                Step2Title.Foreground = textPrimaryBrush;
                TxtStepGuide.Text = "第 2 步：配置图纸属性字段映射与目标单位制规则。";
            }
            else if (_currentStep > 2)
            {
                Step2Badge.Background = successSubtleBrush;
                Step2Number.Foreground = successBrush;
                Step2Number.Text = "✓";
                Step2Title.Foreground = textSecBrush;
            }
            else
            {
                Step2Badge.Background = surfaceAltBrush;
                Step2Number.Foreground = textSecBrush;
                Step2Number.Text = "2";
                Step2Title.Foreground = textSecBrush;
            }

            // 步骤 3 指示器状态
            if (_currentStep == 3)
            {
                Step3Badge.Background = accentBrush;
                Step3Number.Foreground = onAccentBrush;
                Step3Number.Text = "3";
                Step3Title.Foreground = textPrimaryBrush;
                TxtStepGuide.Text = "第 3 步：确认任务摘要无误后，点击“开始批处理”启动队列。";
            }
            else
            {
                Step3Badge.Background = surfaceAltBrush;
                Step3Number.Foreground = textSecBrush;
                Step3Number.Text = "3";
                Step3Title.Foreground = textSecBrush;
            }
        }
    }
}
