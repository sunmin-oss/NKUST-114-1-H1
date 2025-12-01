using AirQualityWeb.Models;
using Microsoft.Data.Sqlite;

namespace AirQualityWeb.Services;

/// <summary>
/// 空氣品質資料服務 (SQLite 版本)
/// </summary>
public class AirQualityService
{
    private readonly string _connectionString;
    private readonly ILogger<AirQualityService> _logger;

    public AirQualityService(IConfiguration configuration, ILogger<AirQualityService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("找不到資料庫連線字串");
        _logger = logger;
        
        // 確保資料庫和資料表存在
        InitializeDatabase();
    }

    /// <summary>
    /// 初始化資料庫 (建立資料表)
    /// </summary>
    private void InitializeDatabase()
    {
        try
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var createTableSql = @"
                CREATE TABLE IF NOT EXISTS air_quality (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    site_id TEXT NOT NULL,
                    site_name TEXT NOT NULL,
                    item_id TEXT NOT NULL,
                    item_name TEXT NOT NULL,
                    item_eng_name TEXT,
                    item_unit TEXT,
                    monitor_month TEXT NOT NULL,
                    concentration REAL,
                    created_at TEXT DEFAULT CURRENT_TIMESTAMP
                );

                CREATE INDEX IF NOT EXISTS idx_site ON air_quality(site_name);
                CREATE INDEX IF NOT EXISTS idx_item ON air_quality(item_name);
                CREATE INDEX IF NOT EXISTS idx_month ON air_quality(monitor_month);
            ";

            using var cmd = new SqliteCommand(createTableSql, conn);
            cmd.ExecuteNonQuery();

            _logger.LogInformation("資料庫初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "資料庫初始化失敗");
        }
    }

    /// <summary>
    /// 從 JSON 匯入資料到 SQLite
    /// </summary>
    public async Task<int> ImportFromJsonAsync(string jsonPath)
    {
        var json = await File.ReadAllTextAsync(jsonPath);
        var response = System.Text.Json.JsonSerializer.Deserialize<AirInfoResponse>(json);
        
        if (response?.Records == null || response.Records.Count == 0)
            return 0;

        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var transaction = conn.BeginTransaction();
        var count = 0;

        try
        {
            var insertSql = @"
                INSERT INTO air_quality 
                (site_id, site_name, item_id, item_name, item_eng_name, item_unit, monitor_month, concentration)
                VALUES (@sid, @sname, @iid, @iname, @ieng, @iunit, @month, @conc)";

            foreach (var record in response.Records)
            {
                using var cmd = new SqliteCommand(insertSql, conn, transaction);
                cmd.Parameters.AddWithValue("@sid", record.SiteId ?? "");
                cmd.Parameters.AddWithValue("@sname", record.SiteName ?? "");
                cmd.Parameters.AddWithValue("@iid", record.ItemId ?? "");
                cmd.Parameters.AddWithValue("@iname", record.ItemName ?? "");
                cmd.Parameters.AddWithValue("@ieng", record.ItemEngName ?? "");
                cmd.Parameters.AddWithValue("@iunit", record.ItemUnit ?? "");
                cmd.Parameters.AddWithValue("@month", record.MonitorMonth ?? "");
                
                if (decimal.TryParse(record.Concentration, out var conc))
                    cmd.Parameters.AddWithValue("@conc", conc);
                else
                    cmd.Parameters.AddWithValue("@conc", DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
                count++;
            }

            await transaction.CommitAsync();
            _logger.LogInformation("成功匯入 {Count} 筆資料", count);
            return count;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "資料匯入失敗");
            throw;
        }
    }

    /// <summary>
    /// 取得資料 (支援分頁與篩選)
    /// </summary>
    public async Task<(List<AirInfo> records, int totalCount)> GetRecordsAsync(
        string? site = null, 
        string? item = null, 
        string? month = null, 
        string? search = null,
        int page = 1, 
        int pageSize = 50)
    {
        var records = new List<AirInfo>();
        var totalCount = 0;

        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        // 建立 WHERE 條件
        var conditions = new List<string>();
        var parameters = new List<SqliteParameter>();

        if (!string.IsNullOrWhiteSpace(site))
        {
            conditions.Add("site_name = @site");
            parameters.Add(new SqliteParameter("@site", site));
        }
        if (!string.IsNullOrWhiteSpace(item))
        {
            conditions.Add("item_name = @item");
            parameters.Add(new SqliteParameter("@item", item));
        }
        if (!string.IsNullOrWhiteSpace(month))
        {
            conditions.Add("monitor_month = @month");
            parameters.Add(new SqliteParameter("@month", month));
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add("(site_name LIKE @search OR item_name LIKE @search OR item_eng_name LIKE @search)");
            parameters.Add(new SqliteParameter("@search", $"%{search}%"));
        }

        var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

        // 查詢總筆數
        var countSql = $"SELECT COUNT(*) FROM air_quality {whereClause}";
        using (var countCmd = new SqliteCommand(countSql, conn))
        {
            foreach (var param in parameters)
                countCmd.Parameters.Add(new SqliteParameter(param.ParameterName, param.Value));
            
            var result = await countCmd.ExecuteScalarAsync();
            totalCount = result != null ? Convert.ToInt32(result) : 0;
        }

        // 查詢分頁資料
        var offset = (page - 1) * pageSize;
        var dataSql = $@"
            SELECT id, site_id, site_name, item_id, item_name, item_eng_name, 
                   item_unit, monitor_month, concentration, created_at
            FROM air_quality 
            {whereClause}
            ORDER BY id DESC
            LIMIT @limit OFFSET @offset";

        using (var dataCmd = new SqliteCommand(dataSql, conn))
        {
            foreach (var param in parameters)
                dataCmd.Parameters.Add(new SqliteParameter(param.ParameterName, param.Value));
            
            dataCmd.Parameters.AddWithValue("@limit", pageSize);
            dataCmd.Parameters.AddWithValue("@offset", offset);

            using var reader = await dataCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                records.Add(new AirInfo
                {
                    Id = reader.GetInt32(0),
                    SiteId = reader.GetString(1),
                    SiteName = reader.GetString(2),
                    ItemId = reader.GetString(3),
                    ItemName = reader.GetString(4),
                    ItemEngName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    ItemUnit = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    MonitorMonth = reader.GetString(7),
                    Concentration = reader.IsDBNull(8) ? null : Convert.ToDecimal(reader.GetDouble(8)),
                    CreatedAt = reader.IsDBNull(9) ? DateTime.Now : DateTime.Parse(reader.GetString(9))
                });
            }
        }

        return (records, totalCount);
    }

    /// <summary>
    /// 取得所有測站清單
    /// </summary>
    public async Task<List<string>> GetSitesAsync()
    {
        var sites = new List<string>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        var sql = "SELECT DISTINCT site_name FROM air_quality ORDER BY site_name";
        using var cmd = new SqliteCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            sites.Add(reader.GetString(0));
        }

        return sites;
    }

    /// <summary>
    /// 取得所有測項清單
    /// </summary>
    public async Task<List<string>> GetItemsAsync()
    {
        var items = new List<string>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        var sql = "SELECT DISTINCT item_name FROM air_quality ORDER BY item_name";
        using var cmd = new SqliteCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(reader.GetString(0));
        }

        return items;
    }

    /// <summary>
    /// 取得所有監測月份清單
    /// </summary>
    public async Task<List<string>> GetMonthsAsync()
    {
        var months = new List<string>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        var sql = "SELECT DISTINCT monitor_month FROM air_quality ORDER BY monitor_month DESC";
        using var cmd = new SqliteCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            months.Add(reader.GetString(0));
        }

        return months;
    }

    /// <summary>
    /// 取得地圖資料 (包含測站座標與最新監測值)
    /// </summary>
    public async Task<List<(StationCoordinate coord, AirInfo? latestRecord)>> GetMapDataAsync()
    {
        var result = new List<(StationCoordinate, AirInfo?)>();
        
        // 讀取測站座標 CSV
        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "stations_tw_coords.csv");
        var coordinates = new Dictionary<string, StationCoordinate>();
        
        if (File.Exists(csvPath))
        {
            var lines = await File.ReadAllLinesAsync(csvPath);
            foreach (var line in lines.Skip(1)) // 跳過標題
            {
                var parts = line.Split(',');
                if (parts.Length == 3 && 
                    double.TryParse(parts[1], out var lat) && 
                    double.TryParse(parts[2], out var lon))
                {
                    coordinates[parts[0]] = new StationCoordinate
                    {
                        SiteName = parts[0],
                        Latitude = lat,
                        Longitude = lon
                    };
                }
            }
        }

        // 取得每個測站的最新 PM2.5 資料
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        foreach (var coord in coordinates.Values)
        {
            var sql = @"
                SELECT id, site_id, site_name, item_id, item_name, item_eng_name, 
                       item_unit, monitor_month, concentration, created_at
                FROM air_quality 
                WHERE site_name = @site AND item_name = 'PM2.5'
                ORDER BY monitor_month DESC, id DESC
                LIMIT 1";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@site", coord.SiteName);

            using var reader = await cmd.ExecuteReaderAsync();
            AirInfo? record = null;
            if (await reader.ReadAsync())
            {
                record = new AirInfo
                {
                    Id = reader.GetInt32(0),
                    SiteId = reader.GetString(1),
                    SiteName = reader.GetString(2),
                    ItemId = reader.GetString(3),
                    ItemName = reader.GetString(4),
                    ItemEngName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    ItemUnit = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    MonitorMonth = reader.GetString(7),
                    Concentration = reader.IsDBNull(8) ? null : Convert.ToDecimal(reader.GetDouble(8)),
                    CreatedAt = reader.IsDBNull(9) ? DateTime.Now : DateTime.Parse(reader.GetString(9))
                };
            }

            result.Add((coord, record));
        }

        return result;
    }

    /// <summary>
    /// 取得資料庫統計資訊
    /// </summary>
    public async Task<(int totalRecords, int sites, int items)> GetStatisticsAsync()
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        var totalSql = "SELECT COUNT(*) FROM air_quality";
        var sitesSql = "SELECT COUNT(DISTINCT site_name) FROM air_quality";
        var itemsSql = "SELECT COUNT(DISTINCT item_name) FROM air_quality";

        var total = 0;
        var sites = 0;
        var items = 0;

        using (var cmd = new SqliteCommand(totalSql, conn))
        {
            var result = await cmd.ExecuteScalarAsync();
            total = result != null ? Convert.ToInt32(result) : 0;
        }

        using (var cmd = new SqliteCommand(sitesSql, conn))
        {
            var result = await cmd.ExecuteScalarAsync();
            sites = result != null ? Convert.ToInt32(result) : 0;
        }

        using (var cmd = new SqliteCommand(itemsSql, conn))
        {
            var result = await cmd.ExecuteScalarAsync();
            items = result != null ? Convert.ToInt32(result) : 0;
        }

        return (total, sites, items);
    }
}

/// <summary>
/// JSON 反序列化用的包裝類別
/// </summary>
public class AirInfoResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("records")]
    public List<AirInfoRecord> Records { get; set; } = new();
}

public class AirInfoRecord
{
    [System.Text.Json.Serialization.JsonPropertyName("siteid")]
    public string SiteId { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("sitename")]
    public string SiteName { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("itemid")]
    public string ItemId { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("itemname")]
    public string ItemName { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("itemengname")]
    public string ItemEngName { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("itemunit")]
    public string ItemUnit { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("monitormonth")]
    public string MonitorMonth { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("concentration")]
    public string Concentration { get; set; } = string.Empty;
}
