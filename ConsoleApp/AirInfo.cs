using System.Text.Json.Serialization;

namespace ConsoleApp;

/// <summary>
/// 空氣品質監測資料模型
/// </summary>
public class AirInfo
{
	[JsonPropertyName("siteid")] public string SiteId { get; set; } = string.Empty;           // 測站代碼
	[JsonPropertyName("sitename")] public string SiteName { get; set; } = string.Empty;        // 測站名稱
	[JsonPropertyName("itemid")] public string ItemId { get; set; } = string.Empty;            // 測項代碼
	[JsonPropertyName("itemname")] public string ItemName { get; set; } = string.Empty;        // 測項名稱
	[JsonPropertyName("itemengname")] public string ItemEngName { get; set; } = string.Empty;  // 測項英文名稱
	[JsonPropertyName("itemunit")] public string ItemUnit { get; set; } = string.Empty;        // 測項單位
	[JsonPropertyName("monitormonth")] public string MonitorMonth { get; set; } = string.Empty;// 監測月份
	[JsonPropertyName("concentration")] public string Concentration { get; set; } = string.Empty; // 監測平均值
}

/// <summary>
/// 對應來源 JSON 外層結構（僅需 records）
/// </summary>
internal sealed class AirInfoResponse
{
	[JsonPropertyName("records")] public List<AirInfo> Records { get; set; } = new();
}

