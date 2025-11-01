using System.Security;
using System.Text;
using System.Text.Json;

namespace ConsoleApp;

internal class Program
{
	private static int Main(string[] args)
	{
		Console.OutputEncoding = Encoding.UTF8;
		Console.InputEncoding = Encoding.UTF8;

		// 建立輸入檔路徑（已在 .csproj 設定將 App_Data 複製到輸出資料夾）
		var dataFilePath = Path.Combine(AppContext.BaseDirectory, "App_Data", "aqx_p_08_data.json");

		try
		{
			// 檢查檔案是否存在
			if (!File.Exists(dataFilePath))
			{
				Console.Error.WriteLine($"找不到資料檔案：{dataFilePath}");
				return 1;
			}

			// 讀取檔案內容
			string json;
			using (var stream = File.Open(dataFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
			{
				json = reader.ReadToEnd();
			}

			// 反序列化（只關心 records 陣列）
			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				ReadCommentHandling = JsonCommentHandling.Skip,
				AllowTrailingCommas = true
			};

			var response = JsonSerializer.Deserialize<AirInfoResponse>(json, options);
			var list = response?.Records ?? new List<AirInfo>();

			if (list.Count == 0)
			{
				Console.WriteLine("資料為空（records 無資料）。");
				return 0;
			}

			// 顯示前 10 筆
			Console.WriteLine("前 10 筆資料：\n");
			foreach (var (item, idx) in list.Take(10).Select((x, i) => (x, i + 1)))
			{
				var conc = string.IsNullOrWhiteSpace(item.Concentration) ? "(無資料)" : item.Concentration;
				Console.WriteLine($"#{idx}");
				Console.WriteLine($"  測站代碼: {item.SiteId}");
				Console.WriteLine($"  測站名稱: {item.SiteName}");
				Console.WriteLine($"  測項代碼: {item.ItemId}");
				Console.WriteLine($"  測項名稱: {item.ItemName}");
				Console.WriteLine($"  測項英文名稱: {item.ItemEngName}");
				Console.WriteLine($"  測項單位: {item.ItemUnit}");
				Console.WriteLine($"  監測月份: {item.MonitorMonth}");
				Console.WriteLine($"  監測平均值: {conc}\n");
			}

			return 0;
		}
		catch (FileNotFoundException ex)
		{
			PrintError("檔案不存在", ex);
			return 1;
		}
		catch (DirectoryNotFoundException ex)
		{
			PrintError("資料夾不存在", ex);
			return 1;
		}
		catch (UnauthorizedAccessException ex)
		{
			PrintError("讀取權限不足", ex);
			return 1;
		}
		catch (SecurityException ex)
		{
			PrintError("安全性限制，無法存取檔案", ex);
			return 1;
		}
		catch (JsonException ex)
		{
			PrintError("JSON 解析失敗（格式可能不正確）", ex);
			return 1;
		}
		catch (OutOfMemoryException ex)
		{
			PrintError("系統記憶體不足，無法載入資料", ex);
			return 1;
		}
		catch (IOException ex)
		{
			PrintError("I/O 錯誤（檔案可能被佔用或損毀）", ex);
			return 1;
		}
		catch (Exception ex)
		{
			PrintError("發生未預期的錯誤", ex);
			return 1;
		}
	}

	private static void PrintError(string title, Exception ex)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.Error.WriteLine($"[錯誤] {title}: {ex.Message}");
		Console.ResetColor();
	}
}

