using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AirQualityWeb.Models;
using AirQualityWeb.Services;

namespace AirQualityWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AirQualityService _airQualityService;

    public HomeController(ILogger<HomeController> logger, AirQualityService airQualityService)
    {
        _logger = logger;
        _airQualityService = airQualityService;
    }

    /// <summary>
    /// 首頁 - 資料瀏覽
    /// </summary>
    public async Task<IActionResult> Index(string? site, string? item, string? month, string? search, int page = 1)
    {
        try
        {
            // 檢查是否需要匯入資料
            var (totalRecords, _, _) = await _airQualityService.GetStatisticsAsync();
            if (totalRecords == 0)
            {
                TempData["Info"] = "資料庫為空,請點擊「匯入資料」按鈕載入 JSON 資料";
            }

            var pageSize = 50;
            
            // 取得篩選資料
            var (records, totalCount) = await _airQualityService.GetRecordsAsync(site, item, month, search, page, pageSize);
            var sites = await _airQualityService.GetSitesAsync();
            var items = await _airQualityService.GetItemsAsync();
            var months = await _airQualityService.GetMonthsAsync();

            var viewModel = new HomeViewModel
            {
                Records = records,
                Sites = sites,
                Items = items,
                Months = months,
                SelectedSite = site,
                SelectedItem = item,
                SelectedMonth = month,
                SearchKeyword = search,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入資料時發生錯誤");
            TempData["Error"] = "載入資料失敗: " + ex.Message;
            return View(new HomeViewModel());
        }
    }

    /// <summary>
    /// 從 JSON 匯入資料
    /// </summary>
    public async Task<IActionResult> ImportData()
    {
        try
        {
            // 取得專案根目錄(AirQualityWeb 的上一層)
            var projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;
            if (projectRoot == null)
            {
                TempData["Error"] = "無法找到專案根目錄";
                return RedirectToAction("Index");
            }

            var jsonPath = Path.Combine(projectRoot, "ConsoleApp", "App_Data", "aqx_p_08_data.json");

            if (!System.IO.File.Exists(jsonPath))
            {
                TempData["Error"] = $"找不到 JSON 檔案: {jsonPath}";
                return RedirectToAction("Index");
            }

            var count = await _airQualityService.ImportFromJsonAsync(jsonPath);
            TempData["Success"] = $"成功匯入 {count} 筆資料!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯入資料失敗");
            TempData["Error"] = "匯入資料失敗: " + ex.Message;
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    /// 地圖視覺化頁面
    /// </summary>
    public async Task<IActionResult> Map()
    {
        try
        {
            var mapData = await _airQualityService.GetMapDataAsync();
            return View(mapData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入地圖資料時發生錯誤");
            TempData["Error"] = "載入地圖失敗: " + ex.Message;
            return RedirectToAction("Index");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
