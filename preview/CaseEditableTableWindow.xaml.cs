using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfStylePreview
{
    /// <summary>
    /// 案例：复杂可编辑数据表格（Editable & Complex DataGrid）。
    /// 演示：
    /// 1. 表头三态全选与行勾选双向联动；
    /// 2. 可编辑文本单元格（编码、名称、数量、单价）；
    /// 3. 下拉单元格（MaterialType 下拉框实时绑定）；
    /// 4. 带复选框单元格（关键部件快速标记）；
    /// 5. 状态徽章与数值右对齐；
    /// 6. RowDetails 嵌套展开详情行；
    /// 7. 顶部多维筛选过滤与底部分页/金额统计联动。
    /// </summary>
    public partial class CaseEditableTableWindow : Window
    {
        private readonly List<string> _availableMaterials = new List<string>
        {
            "Q235 碳素钢",
            "45# 优质钢",
            "304 不锈钢",
            "6061-T6 铝合金",
            "POM 赛钢",
            "QT400 球铁"
        };

        public ObservableCollection<MaterialItem> AllItems { get; } = new ObservableCollection<MaterialItem>();
        public ObservableCollection<MaterialItem> DisplayItems { get; } = new ObservableCollection<MaterialItem>();

        private bool _isUpdatingSelectAll = false;
        private bool _isInitialized = false;

        public CaseEditableTableWindow()
        {
            InitializeComponent();
            InitSampleData();
            SetupColumnComboBox();
            _isInitialized = true;
            RefreshDisplayList();
        }

        private void InitSampleData()
        {
            var data = new List<MaterialItem>
            {
                new MaterialItem { Index = 1, MaterialCode = "M-1001", MaterialName = "主轴驱动齿轮", MaterialType = "45# 优质钢", Quantity = 12, UnitPrice = 185.00, IsCritical = true, Status = "已审核", ProcessNote = "调质处理 HRC 28-32, 滚齿加工", Tolerance = "IT7 / Ra 0.8", LastUpdated = "2026-11-05 14:20" },
                new MaterialItem { Index = 2, MaterialCode = "M-1002", MaterialName = "法兰联轴器法兰套", MaterialType = "Q235 碳素钢", Quantity = 40, UnitPrice = 45.50, IsCritical = false, Status = "已审核", ProcessNote = "数控车削外圆与端面, 表面发黑", Tolerance = "IT8 / Ra 1.6", LastUpdated = "2026-11-05 15:10" },
                new MaterialItem { Index = 3, MaterialCode = "M-1003", MaterialName = "高压密封导向环", MaterialType = "POM 赛钢", Quantity = 100, UnitPrice = 18.00, IsCritical = true, Status = "审核中", ProcessNote = "精密注塑后精车密封槽, 严格去毛刺", Tolerance = "IT6 / Ra 0.4", LastUpdated = "2026-11-06 09:30" },
                new MaterialItem { Index = 4, MaterialCode = "M-1004", MaterialName = "散热安装支架", MaterialType = "6061-T6 铝合金", Quantity = 25, UnitPrice = 68.00, IsCritical = false, Status = "待处理", ProcessNote = "激光切割下料, 数控折弯, 本色阳极氧化", Tolerance = "IT9 / 自由公差", LastUpdated = "2026-11-06 10:15" },
                new MaterialItem { Index = 5, MaterialCode = "M-1005", MaterialName = "耐腐蚀阀体外壳", MaterialType = "304 不锈钢", Quantity = 8, UnitPrice = 420.00, IsCritical = true, Status = "已驳回", ProcessNote = "精密铸造后五轴机加工, 固溶钝化处理", Tolerance = "IT7 / Ra 0.8", LastUpdated = "2026-11-06 11:00" },
                new MaterialItem { Index = 6, MaterialCode = "M-1006", MaterialName = "液压泵缸体支座", MaterialType = "QT400 球铁", Quantity = 15, UnitPrice = 135.00, IsCritical = false, Status = "已审核", ProcessNote = "立式加工中心铣面镗孔, 涂防锈底漆", Tolerance = "IT7 / Ra 1.6", LastUpdated = "2026-11-06 13:45" },
                new MaterialItem { Index = 7, MaterialCode = "M-1007", MaterialName = "传动行星销轴", MaterialType = "45# 优质钢", Quantity = 60, UnitPrice = 28.00, IsCritical = true, Status = "审核中", ProcessNote = "高频淬火表面 HRC 50-55, 外圆无心磨", Tolerance = "IT6 / Ra 0.2", LastUpdated = "2026-11-06 16:20" },
                new MaterialItem { Index = 8, MaterialCode = "M-1008", MaterialName = "管路固定卡箍", MaterialType = "304 不锈钢", Quantity = 120, UnitPrice = 6.50, IsCritical = false, Status = "待处理", ProcessNote = "连续模冲压成形, 振动研磨光洁", Tolerance = "IT10", LastUpdated = "2026-11-07 08:50" }
            };

            AllItems.Clear();
            foreach (var item in data)
            {
                item.PropertyChanged += Item_PropertyChanged;
                AllItems.Add(item);
            }
        }

        private void SetupColumnComboBox()
        {
            // 为 DataGridComboBoxColumn 配置可选材质列表
            foreach (var col in GridMaterials.Columns)
            {
                if (col is DataGridComboBoxColumn comboCol)
                {
                    comboCol.ItemsSource = _availableMaterials;
                }
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MaterialItem.IsSelected))
            {
                UpdateSelectAllState();
                UpdateSelectionSummary();
            }
            else if (e.PropertyName == nameof(MaterialItem.Quantity) || e.PropertyName == nameof(MaterialItem.UnitPrice))
            {
                UpdateTotalSummary();
            }
            else if (e.PropertyName == nameof(MaterialItem.Status))
            {
                // 状态变动时触发样式通知已在属性内处理
            }
        }

        private void RefreshDisplayList()
        {
            if (!_isInitialized || GridMaterials == null || AllItems == null) return;

            string keyword = TxtSearch?.Text?.Trim().ToLowerInvariant() ?? string.Empty;
            string materialFilter = (CmbMaterialFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "全部材质";
            string statusFilter = (CmbStatusFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "全部状态";

            DisplayItems.Clear();
            int idx = 1;
            foreach (var item in AllItems)
            {
                bool matchKeyword = string.IsNullOrEmpty(keyword)
                                    || (item.MaterialCode != null && item.MaterialCode.ToLowerInvariant().Contains(keyword))
                                    || (item.MaterialName != null && item.MaterialName.ToLowerInvariant().Contains(keyword));

                bool matchMaterial = materialFilter == "全部材质" || (item.MaterialType != null && item.MaterialType.Contains(materialFilter.Split(' ')[0]));
                bool matchStatus = statusFilter == "全部状态" || item.Status == statusFilter;

                if (matchKeyword && matchMaterial && matchStatus)
                {
                    item.Index = idx++;
                    DisplayItems.Add(item);
                }
            }

            GridMaterials.ItemsSource = DisplayItems;
            UpdateSelectAllState();
            UpdateSelectionSummary();
            UpdateTotalSummary();
        }

        private void UpdateSelectAllState()
        {
            if (_isUpdatingSelectAll || ChkSelectAll == null) return;
            _isUpdatingSelectAll = true;

            int selectedCount = DisplayItems.Count(i => i.IsSelected);
            int total = DisplayItems.Count;

            if (total == 0 || selectedCount == 0)
            {
                ChkSelectAll.IsChecked = false;
            }
            else if (selectedCount == total)
            {
                ChkSelectAll.IsChecked = true;
            }
            else
            {
                ChkSelectAll.IsChecked = null; // 部分选中
            }

            _isUpdatingSelectAll = false;
        }

        private void UpdateSelectionSummary()
        {
            int selectedCount = DisplayItems.Count(i => i.IsSelected);
            if (TxtSelectedSummary != null)
            {
                TxtSelectedSummary.Text = selectedCount > 0 ? $"已选中 {selectedCount} 项物料" : "未选择项";
            }
            if (BtnBatchApprove != null) BtnBatchApprove.IsEnabled = selectedCount > 0;
            if (BtnBatchDelete != null) BtnBatchDelete.IsEnabled = selectedCount > 0;
        }

        private void UpdateTotalSummary()
        {
            if (TxtTotalSummary == null) return;
            int totalKinds = DisplayItems.Count;
            int totalPieces = DisplayItems.Sum(i => i.Quantity);
            double totalAmount = DisplayItems.Sum(i => i.Quantity * i.UnitPrice);

            TxtTotalSummary.Text = $"当前展示 {totalKinds} 种物料 · 总件数 {totalPieces} 件 · 预估总额 ￥{totalAmount:N2}";
        }

        // ============================================================
        // 事件响应
        // ============================================================

        private void ChkSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingSelectAll) return;
            bool targetState = ChkSelectAll.IsChecked == true;

            _isUpdatingSelectAll = true;
            foreach (var item in DisplayItems)
            {
                item.IsSelected = targetState;
            }
            _isUpdatingSelectAll = false;

            UpdateSelectAllState();
            UpdateSelectionSummary();
        }

        private void RowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectAllState();
            UpdateSelectionSummary();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshDisplayList();
        }

        private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshDisplayList();
        }

        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            if (TxtSearch != null) TxtSearch.Text = string.Empty;
            if (CmbMaterialFilter != null) CmbMaterialFilter.SelectedIndex = 0;
            if (CmbStatusFilter != null) CmbStatusFilter.SelectedIndex = 0;
            RefreshDisplayList();
        }

        private void BtnAddRow_Click(object sender, RoutedEventArgs e)
        {
            int nextId = AllItems.Count + 1;
            var newItem = new MaterialItem
            {
                Index = nextId,
                MaterialCode = $"M-10{nextId:D2}",
                MaterialName = "新加结构组件",
                MaterialType = "Q235 碳素钢",
                Quantity = 10,
                UnitPrice = 50.00,
                IsCritical = false,
                Status = "待处理",
                ProcessNote = "待工艺编制与工时核定",
                Tolerance = "IT8",
                LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            };
            newItem.PropertyChanged += Item_PropertyChanged;
            AllItems.Add(newItem);
            RefreshDisplayList();

            // 滚动并聚焦新增行
            GridMaterials.SelectedItem = newItem;
            GridMaterials.ScrollIntoView(newItem);
        }

        private void BtnBatchApprove_Click(object sender, RoutedEventArgs e)
        {
            var selected = DisplayItems.Where(i => i.IsSelected).ToList();
            foreach (var item in selected)
            {
                item.Status = "已审核";
                item.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            }
            MessageBox.Show($"已成功批量审核 {selected.Count} 项物料！", "批量操作", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnBatchDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = DisplayItems.Where(i => i.IsSelected).ToList();
            if (selected.Count == 0) return;

            var result = MessageBox.Show($"确认删除所选的 {selected.Count} 项物料吗？", "删除确认", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                foreach (var item in selected)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                    AllItems.Remove(item);
                }
                RefreshDisplayList();
            }
        }

        private void BtnRowToggleDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is MaterialItem item)
            {
                var row = (DataGridRow)GridMaterials.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                {
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                }
            }
        }

        private void BtnToggleAllDetails_Click(object sender, RoutedEventArgs e)
        {
            bool anyCollapsed = false;
            foreach (var item in DisplayItems)
            {
                var row = (DataGridRow)GridMaterials.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null && row.DetailsVisibility != Visibility.Visible)
                {
                    anyCollapsed = true;
                    break;
                }
            }

            var targetVis = anyCollapsed ? Visibility.Visible : Visibility.Collapsed;
            foreach (var item in DisplayItems)
            {
                var row = (DataGridRow)GridMaterials.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                {
                    row.DetailsVisibility = targetVis;
                }
            }
        }

        private void BtnRowDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is MaterialItem item)
            {
                item.PropertyChanged -= Item_PropertyChanged;
                AllItems.Remove(item);
                RefreshDisplayList();
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"已成功导出当前 {DisplayItems.Count} 条物料明细数据至 CSV 文件。", "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("物料变更与工艺参数已全部保存入库！", "保存成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    /// <summary>
    /// 物料明细实体，实现 INotifyPropertyChanged。
    /// </summary>
    public class MaterialItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private int _index;
        private string _materialCode;
        private string _materialName;
        private string _materialType;
        private int _quantity;
        private double _unitPrice;
        private bool _isCritical;
        private string _status;
        private string _processNote;
        private string _tolerance;
        private string _lastUpdated;

        public bool IsSelected
        {
            get => _isSelected;
            set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(); } }
        }

        public int Index
        {
            get => _index;
            set { if (_index != value) { _index = value; OnPropertyChanged(); } }
        }

        public string MaterialCode
        {
            get => _materialCode;
            set { if (_materialCode != value) { _materialCode = value; OnPropertyChanged(); } }
        }

        public string MaterialName
        {
            get => _materialName;
            set { if (_materialName != value) { _materialName = value; OnPropertyChanged(); } }
        }

        public string MaterialType
        {
            get => _materialType;
            set { if (_materialType != value) { _materialType = value; OnPropertyChanged(); } }
        }

        public int Quantity
        {
            get => _quantity;
            set { if (_quantity != value) { _quantity = value; OnPropertyChanged(); } }
        }

        public double UnitPrice
        {
            get => _unitPrice;
            set { if (_unitPrice != value) { _unitPrice = value; OnPropertyChanged(); } }
        }

        public bool IsCritical
        {
            get => _isCritical;
            set { if (_isCritical != value) { _isCritical = value; OnPropertyChanged(); } }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusBackground));
                    OnPropertyChanged(nameof(StatusForeground));
                }
            }
        }

        public string ProcessNote
        {
            get => _processNote;
            set { if (_processNote != value) { _processNote = value; OnPropertyChanged(); } }
        }

        public string Tolerance
        {
            get => _tolerance;
            set { if (_tolerance != value) { _tolerance = value; OnPropertyChanged(); } }
        }

        public string LastUpdated
        {
            get => _lastUpdated;
            set { if (_lastUpdated != value) { _lastUpdated = value; OnPropertyChanged(); } }
        }

        // 状态对应的徽章色彩，全部取自系统 Token
        public Brush StatusBackground
        {
            get
            {
                switch (Status)
                {
                    case "已审核":
                        return Application.Current.FindResource("Brush.Success.Subtle") as Brush;
                    case "审核中":
                        return Application.Current.FindResource("Brush.Accent.Subtle") as Brush;
                    case "已驳回":
                        return Application.Current.FindResource("Brush.Danger.Subtle") as Brush;
                    default:
                        return Application.Current.FindResource("Brush.SurfaceAlt") as Brush;
                }
            }
        }

        public Brush StatusForeground
        {
            get
            {
                switch (Status)
                {
                    case "已审核":
                        return Application.Current.FindResource("Brush.Success") as Brush;
                    case "审核中":
                        return Application.Current.FindResource("Brush.Accent") as Brush;
                    case "已驳回":
                        return Application.Current.FindResource("Brush.Danger") as Brush;
                    default:
                        return Application.Current.FindResource("Brush.Text.Secondary") as Brush;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
