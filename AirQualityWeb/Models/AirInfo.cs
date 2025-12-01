using System.ComponentModel.DataAnnotations;

namespace AirQualityWeb.Models;

/// <summary>
/// 空氣品質監測資料模型
/// </summary>
public class AirInfo
{
    public int Id { get; set; }
    
    [Display(Name = "測站代碼")]
    public string SiteId { get; set; } = string.Empty;
    
    [Display(Name = "測站名稱")]
    public string SiteName { get; set; } = string.Empty;
    
    [Display(Name = "測項代碼")]
    public string ItemId { get; set; } = string.Empty;
    
    [Display(Name = "測項名稱")]
    public string ItemName { get; set; } = string.Empty;
    
    [Display(Name = "測項英文")]
    public string ItemEngName { get; set; } = string.Empty;
    
    [Display(Name = "單位")]
    public string ItemUnit { get; set; } = string.Empty;
    
    [Display(Name = "監測月份")]
    public string MonitorMonth { get; set; } = string.Empty;
    
    [Display(Name = "監測值")]
    public decimal? Concentration { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 測站座標模型
/// </summary>
public class StationCoordinate
{
    public string SiteName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

/// <summary>
/// 首頁 ViewModel
/// </summary>
public class HomeViewModel
{
    public List<AirInfo> Records { get; set; } = new();
    public List<string> Sites { get; set; } = new();
    public List<string> Items { get; set; } = new();
    public List<string> Months { get; set; } = new();
    
    // 篩選條件
    public string? SelectedSite { get; set; }
    public string? SelectedItem { get; set; }
    public string? SelectedMonth { get; set; }
    public string? SearchKeyword { get; set; }
    
    // 分頁
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 50;
    public int TotalRecords { get; set; }
}
