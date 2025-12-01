## 專案說明

本專案包含三個 .NET 9.0 C# 應用程式，用於讀取、分析並視覺化空氣品質月平均資料（aqx_p_08）:

### 1. ConsoleApp - 主控台應用程式

讀取並解析空氣品質資料，以 System.Text.Json 反序列化並於主控台列出前 10 筆紀錄。

### 2. AirQualityWinForms - Windows Forms 圖形介面應用程式

提供視覺化互動介面，功能包括：

- **資料表格顯示**：使用 DataGridView 呈現完整資料
- **多重篩選**：依測站、測項、監測月份進行篩選
- **關鍵字搜尋**：在所有欄位中搜尋特定關鍵字
- **地圖視覺化**：使用 WebView2 + Leaflet.js 顯示台灣空氣品質地圖
- **即時統計**：顯示當前篩選後的資料筆數
- **一鍵重置**：快速清除所有篩選條件

### 3. AirQualityWeb - ASP.NET Core MVC Web 應用程式 ⭐ 新增

完整的 Web 應用程式，整合 MySQL 資料庫，提供：

- **資料瀏覽**：分頁顯示、多重篩選、關鍵字搜尋
- **地圖視覺化**：Leaflet.js 互動式台灣地圖，顯示 58 個測站 PM2.5 分布
- **資料庫整合**：MySQL 8.0+ 資料持久化
- **響應式設計**：Bootstrap 5 支援各種裝置
- **即時查詢**：ADO.NET 高效能資料庫查詢

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
4. 點擊「顯示地圖」開啟互動式台灣地圖視覺化
5. 點擊「重置」清除所有篩選條件

### AirQualityWeb (Web 應用程式) ⭐ 推薦

#### 前置作業: 匯入資料庫

**第一次使用前必須先匯入資料到 MySQL**:

```powershell
# 方法 1: 使用自動化腳本 (推薦)
.\run_import.ps1

# 方法 2: 使用 MySQL 命令列
mysql -u root -p --local-infile=1 < import_to_mysql.sql
```

> 詳細說明請參閱 `IMPORT_GUIDE.md`

#### 執行 Web 應用程式

```powershell
# 1. 修改 AirQualityWeb/appsettings.json 中的 MySQL 密碼
# 將 "Pwd=your_password" 改成你的 MySQL root 密碼

# 2. 切換到 Web 專案目錄
cd AirQualityWeb

# 3. 還原套件
dotnet restore

# 4. 建置專案
dotnet build

# 5. 執行網站
dotnet run
```

執行成功後,開啟瀏覽器訪問:
- **首頁 (資料瀏覽)**: https://localhost:5001
- **地圖視覺化**: https://localhost:5001/Home/Map

> 詳細啟動說明請參閱 `AirQualityWeb/START_GUIDE.md`

## 專案結構

```text
NKUST-114-1-H1/
├─ ConsoleApp/                      # 主控台應用程式
│  ├─ ConsoleApp.csproj
│  ├─ Program.cs
│  ├─ AirInfo.cs
│  └─ App_Data/
│     ├─ aqx_p_08_data.json        # 空氣品質資料
│     └─ stations_tw_coords.csv    # 測站座標 (58 個測站)
├─ AirQualityWinForms/             # Windows Forms 圖形介面
│  ├─ AirQualityWinForms.csproj
│  ├─ Program.cs
│  ├─ MainForm.cs
│  ├─ MainForm.Designer.cs
│  └─ MapForm.cs                    # 地圖視覺化表單
├─ AirQualityWeb/                  # ASP.NET Core MVC Web 應用程式 ⭐
│  ├─ Controllers/
│  │  └─ HomeController.cs         # 主控制器
│  ├─ Models/
│  │  └─ AirInfo.cs                # 資料模型
│  ├─ Services/
│  │  └─ AirQualityService.cs      # 資料庫服務
│  ├─ Views/
│  │  └─ Home/
│  │     ├─ Index.cshtml           # 首頁 (資料瀏覽)
│  │     └─ Map.cshtml             # 地圖視覺化
│  ├─ App_Data/
│  │  └─ stations_tw_coords.csv
│  ├─ appsettings.json             # 組態設定 (含資料庫連線)
│  ├─ README.md                    # Web 專案說明
│  └─ START_GUIDE.md               # 快速啟動指南
├─ import_to_mysql.sql             # MySQL 資料庫匯入腳本
├─ IMPORT_GUIDE.md                 # 資料庫匯入詳細說明
├─ run_import.ps1                  # 自動化匯入 PowerShell 腳本
└─ README.md                       # 專案總說明 (本檔案)
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
