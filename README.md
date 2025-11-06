## 專案說明

本專案包含兩個 .NET 9.0 C# 應用程式，用於讀取並分析空氣品質月平均資料（aqx_p_08）：

### 1. ConsoleApp - 主控台應用程式

讀取並解析空氣品質資料，以 System.Text.Json 反序列化並於主控台列出前 10 筆紀錄。

### 2. AirQualityWinForms - Windows Forms 圖形介面應用程式

提供視覺化互動介面，功能包括：

- **資料表格顯示**：使用 DataGridView 呈現完整資料
- **多重篩選**：依測站、測項、監測月份進行篩選
- **關鍵字搜尋**：在所有欄位中搜尋特定關鍵字
- **即時統計**：顯示當前篩選後的資料筆數
- **一鍵重置**：快速清除所有篩選條件

程式重點：

- 資料模型 `AirInfo` 對應 JSON 欄位（測站代碼、測站名稱、測項代碼、測項名稱、測項英文名稱、測項單位、監測月份、監測平均值）。
- 完整錯誤處理：檔案不存在、目錄不存在、權限不足、安全性限制、JSON 格式錯誤、I/O 錯誤、記憶體不足與其他未預期錯誤。
- 專案設定已將 `ConsoleApp/App_Data` 內容自動複製到輸出目錄（Build/Run 時生效）。

## 資料來源

- 來源：政府開放資料（環境部空氣品質資料集 aqx_p_08）。
- 專案已內含範例檔案：`ConsoleApp/App_Data/aqx_p_08_data.json`，可直接執行。

## 開發環境需求

- .NET 9.0 SDK（或相容的 .NET SDK）
- Windows PowerShell（本文範例指令以 PowerShell 為主）

## 建置與執行

在專案根目錄執行以下指令：

### ConsoleApp (主控台版本)

```powershell
# 建置（Release 組態）
dotnet build "ConsoleApp/ConsoleApp.csproj" -c Release

# 直接執行（以專案執行）
dotnet run --project "ConsoleApp" -c Release

# 或執行已編譯的可執行檔
& "ConsoleApp/bin/Release/net9.0/ConsoleApp.exe"
```

執行後會在主控台輸出前 10 筆資料；若某筆 `concentration` 為空，會顯示「(無資料)」。

### AirQualityWinForms (圖形介面版本)

```powershell
# 建置（Release 組態）
dotnet build "AirQualityWinForms/AirQualityWinForms.csproj" -c Release

# 直接執行（以專案執行）
dotnet run --project "AirQualityWinForms" -c Release

# 或執行已編譯的可執行檔
& "AirQualityWinForms/bin/Release/net9.0-windows/AirQualityWinForms.exe"
```

執行後會開啟 Windows 視窗：

1. 點擊「載入資料」按鈕讀取 JSON 檔案
2. 使用下拉選單篩選測站、測項或月份
3. 在搜尋框輸入關鍵字並點擊「搜尋」
4. 點擊「重置」清除所有篩選條件

## 專案結構

```text
NKUST-114-1-H1/
├─ ConsoleApp/
│  ├─ ConsoleApp.csproj
│  ├─ Program.cs
│  ├─ AirInfo.cs
│  └─ App_Data/
│     └─ aqx_p_08_data.json
├─ AirQualityWinForms/
│  ├─ AirQualityWinForms.csproj
│  ├─ Program.cs
│  ├─ MainForm.cs
│  └─ MainForm.Designer.cs
└─ README.md
```

## 錯誤處理說明

程式對常見情境提供明確訊息：

- 檔案或資料夾不存在（FileNotFound、DirectoryNotFound）
- 權限不足或安全性限制（UnauthorizedAccess、SecurityException）
- JSON 格式不正確（JsonException）
- 檔案被占用、I/O 錯誤（IOException）
- 記憶體不足（OutOfMemoryException）
- 其他未預期錯誤（Exception）

## 授權

此專案程式碼授權以本儲存庫 LICENSE（若無，預設為僅供課程作業示範使用）。資料集之著作權與使用規範請依政府開放資料平台之授權條款為準。
