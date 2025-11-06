using System.Text.Json;
using System.Text.Json.Serialization;
using ConsoleApp;

namespace AirQualityWinForms
{
    public partial class MainForm : Form
    {
        private List<AirInfo> _allData = new();
        private List<AirInfo> _filteredData = new();
        private Dictionary<string, (double lat, double lon)> _siteCoords = new(StringComparer.OrdinalIgnoreCase);

        public MainForm()
        {
            InitializeComponent();
            InitializeFilters();
        }

        private void InitializeFilters()
        {
            // 初始化篩選器下拉選單
            cmbSite.Items.Add("(全部測站)");
            cmbSite.SelectedIndex = 0;

            cmbItem.Items.Add("(全部測項)");
            cmbItem.SelectedIndex = 0;

            cmbMonth.Items.Add("(全部月份)");
            cmbMonth.SelectedIndex = 0;
        }

        private void BtnLoad_Click(object? sender, EventArgs e)
        {
            try
            {
                var dataFilePath = Path.Combine(AppContext.BaseDirectory, "App_Data", "aqx_p_08_data.json");

                if (!File.Exists(dataFilePath))
                {
                    MessageBox.Show($"找不到資料檔案:\n{dataFilePath}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 讀取並反序列化 JSON
                var json = File.ReadAllText(dataFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                var response = JsonSerializer.Deserialize<AirInfoResponse>(json, options);
                _allData = response?.Records ?? new List<AirInfo>();

                if (_allData.Count == 0)
                {
                    MessageBox.Show("資料為空或格式不正確", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 填充篩選器選項
                PopulateFilters();

                // 顯示全部資料
                _filteredData = _allData;
                UpdateDataGrid();

                MessageBox.Show($"成功載入 {_allData.Count} 筆資料!", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"載入資料時發生錯誤:\n{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateFilters()
        {
            // 測站篩選
            var sites = _allData.Select(x => x.SiteName).Distinct().OrderBy(x => x).ToList();
            cmbSite.Items.Clear();
            cmbSite.Items.Add("(全部測站)");
            foreach (var site in sites)
            {
                if (!string.IsNullOrWhiteSpace(site))
                    cmbSite.Items.Add(site);
            }
            cmbSite.SelectedIndex = 0;

            // 測項篩選
            var items = _allData.Select(x => x.ItemName).Distinct().OrderBy(x => x).ToList();
            cmbItem.Items.Clear();
            cmbItem.Items.Add("(全部測項)");
            foreach (var item in items)
            {
                if (!string.IsNullOrWhiteSpace(item))
                    cmbItem.Items.Add(item);
            }
            cmbItem.SelectedIndex = 0;

            // 月份篩選
            var months = _allData.Select(x => x.MonitorMonth).Distinct().OrderByDescending(x => x).ToList();
            cmbMonth.Items.Clear();
            cmbMonth.Items.Add("(全部月份)");
            foreach (var month in months)
            {
                if (!string.IsNullOrWhiteSpace(month))
                    cmbMonth.Items.Add(month);
            }
            cmbMonth.SelectedIndex = 0;
        }

        private void Filter_Changed(object? sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allData.Count == 0) return;

            _filteredData = _allData;

            // 測站篩選
            if (cmbSite.SelectedIndex > 0)
            {
                var selectedSite = cmbSite.SelectedItem?.ToString();
                _filteredData = _filteredData.Where(x => x.SiteName == selectedSite).ToList();
            }

            // 測項篩選
            if (cmbItem.SelectedIndex > 0)
            {
                var selectedItem = cmbItem.SelectedItem?.ToString();
                _filteredData = _filteredData.Where(x => x.ItemName == selectedItem).ToList();
            }

            // 月份篩選
            if (cmbMonth.SelectedIndex > 0)
            {
                var selectedMonth = cmbMonth.SelectedItem?.ToString();
                _filteredData = _filteredData.Where(x => x.MonitorMonth == selectedMonth).ToList();
            }

            UpdateDataGrid();
        }

        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            var keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                MessageBox.Show("請輸入搜尋關鍵字", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_allData.Count == 0)
            {
                MessageBox.Show("請先載入資料", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 在所有欄位中搜尋關鍵字
            _filteredData = _allData.Where(x =>
                x.SiteName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.ItemName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.ItemEngName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.Concentration.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.MonitorMonth.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            UpdateDataGrid();

            if (_filteredData.Count == 0)
            {
                MessageBox.Show("未找到符合的資料", "搜尋結果", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            // 重置所有篩選器
            cmbSite.SelectedIndex = 0;
            cmbItem.SelectedIndex = 0;
            cmbMonth.SelectedIndex = 0;
            txtSearch.Clear();

            // 顯示全部資料
            _filteredData = _allData;
            UpdateDataGrid();
        }

        private void BtnShowMap_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_allData.Count == 0)
                {
                    MessageBox.Show("請先載入資料", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 載入或選擇站點座標 CSV：格式 sitename,lat,lon（含標頭）
                if (_siteCoords.Count == 0)
                {
                    using var ofd = new OpenFileDialog
                    {
                        Title = "選擇站點座標 CSV (欄位: sitename,lat,lon)",
                        Filter = "CSV 檔案 (*.csv)|*.csv|所有檔案 (*.*)|*.*",
                        InitialDirectory = Path.Combine(AppContext.BaseDirectory, "App_Data")
                    };
                    if (ofd.ShowDialog(this) != DialogResult.OK) return;
                    LoadSiteCoordsFromCsv(ofd.FileName);
                }

                // 以目前篩選結果顯示地圖
                var current = _filteredData.Count > 0 ? _filteredData : _allData;
                using var map = new MapForm(current, _siteCoords);
                map.StartPosition = FormStartPosition.CenterParent;
                map.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"顯示地圖發生錯誤:\n{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSiteCoordsFromCsv(string path)
        {
            var lines = File.ReadAllLines(path);
            _siteCoords.Clear();
            foreach (var line in lines.Skip(1)) // skip header
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length < 3) continue;
                var name = parts[0].Trim();
                if (double.TryParse(parts[1].Trim(), out var lat) && double.TryParse(parts[2].Trim(), out var lon))
                {
                    if (!string.IsNullOrWhiteSpace(name))
                        _siteCoords[name] = (lat, lon);
                }
            }
            if (_siteCoords.Count == 0)
            {
                throw new InvalidOperationException("座標檔案內容無效或為空，請確認格式為 'sitename,lat,lon'");
            }
        }

        private void UpdateDataGrid()
        {
            // 建立資料表格的顯示資料
            var displayData = _filteredData.Select(x => new
            {
                測站代碼 = x.SiteId,
                測站名稱 = x.SiteName,
                測項代碼 = x.ItemId,
                測項名稱 = x.ItemName,
                測項英文 = x.ItemEngName,
                單位 = x.ItemUnit,
                監測月份 = x.MonitorMonth,
                監測平均值 = string.IsNullOrWhiteSpace(x.Concentration) ? "(無資料)" : x.Concentration
            }).ToList();

            dataGridView1.DataSource = displayData;
            lblRecordCount.Text = $"共 {_filteredData.Count} 筆資料";

            // 自動調整欄位寬度
            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
        }
    }

    /// <summary>
    /// 對應來源 JSON 外層結構(僅需 records)
    /// </summary>
    internal sealed class AirInfoResponse
    {
        [JsonPropertyName("records")] public List<AirInfo> Records { get; set; } = new();
    }
}
