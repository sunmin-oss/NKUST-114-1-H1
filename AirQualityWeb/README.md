# 空氣品質監測 Web 系統

ASP.NET Core MVC 9.0 空氣品質監測網站 - 全台空氣品質資料瀏覽與地圖視覺化

---

## 📋 專案說明

這是一個基於 **ASP.NET Core MVC** 開發的空氣品質監測網站,整合 MySQL 資料庫,提供全台空氣品質資料的瀏覽、篩選、搜尋與地圖視覺化功能。

### ✨ 主要功能

- **資料瀏覽**: 資料表格顯示,支援分頁 (每頁 50 筆)
- **多重篩選**: 測站、測項、監測月份篩選
- **關鍵字搜尋**: 支援測站名稱、測項名稱搜尋
- **地圖視覺化**: Leaflet.js 互動式台灣地圖,顯示測站 PM2.5 分布
- **響應式設計**: Bootstrap 5 響應式介面,支援各種裝置

### 🎨 技術棧

- **後端**: ASP.NET Core MVC 9.0
- **資料庫**: MySQL 8.0+ (`air_quality_db`)
- **前端框架**: Bootstrap 5
- **地圖**: Leaflet.js + OpenStreetMap
- **圖示**: Bootstrap Icons
- **ORM**: ADO.NET (MySql.Data)

---

## 🚀 快速開始

### 前置需求

1. **.NET 9.0 SDK** 已安裝
   ```powershell
   dotnet --version  # 確認版本 >= 9.0
   ```

2. **MySQL 8.0+** 已安裝並執行
   ```powershell
   mysql --version
   ```

3. **資料庫已匯入** (使用上層目錄的 `import_to_mysql.sql`)
   ```powershell
   cd ..
   mysql -u root -p --local-infile=1 < import_to_mysql.sql
   ```

### 步驟 1: 設定資料庫連線

編輯 `appsettings.json`,修改 MySQL 密碼:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=air_quality_db;Uid=root;Pwd=your_password;CharSet=utf8mb4;"
  }
}
```

> **請將 `your_password` 改成你的 MySQL root 密碼!**

### 步驟 2: 還原套件與建置

```powershell
# 還原 NuGet 套件
dotnet restore

# 建置專案
dotnet build
```

### 步驟 3: 執行網站

```powershell
# 執行開發伺服器
dotnet run
```

專案啟動後,開啟瀏覽器訪問:
- **HTTPS**: https://localhost:5001
- **HTTP**: http://localhost:5000

---

## 📂 專案結構

```
AirQualityWeb/
├── Controllers/
│   └── HomeController.cs         # 主控制器 (Index, Map)
├── Models/
│   └── AirInfo.cs                 # 資料模型 (AirInfo, StationCoordinate, HomeViewModel)
├── Services/
│   └── AirQualityService.cs       # 資料服務 (資料庫查詢邏輯)
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml          # 首頁 - 資料瀏覽
│   │   └── Map.cshtml            # 地圖視覺化
│   └── Shared/
│       └── _Layout.cshtml         # 共用版面配置
├── App_Data/
│   └── stations_tw_coords.csv     # 測站座標資料 (58 個測站)
├── wwwroot/                       # 靜態檔案 (CSS, JS, 圖片)
├── appsettings.json               # 組態設定 (含資料庫連線字串)
└── Program.cs                     # 應用程式進入點

```

---

## 🎯 功能說明

### 1️⃣ 資料瀏覽頁面 (`/Home/Index`)

**功能**:
- 顯示空氣品質監測資料表格
- 支援測站、測項、月份篩選
- 關鍵字搜尋 (測站名稱、測項名稱)
- 分頁顯示 (每頁 50 筆)
- 監測值顏色標示:
  - 🟢 綠色: < 12 (良好)
  - 🟡 黃色: 12-20 (普通)
  - 🟠 橘色: 20-30 (對敏感族群不健康)
  - 🔴 紅色: ≥ 30 (不健康)

**URL 參數**:
```
/Home/Index?site=員林&item=PM2.5&month=2024-01&search=&page=1
```

### 2️⃣ 地圖視覺化 (`/Home/Map`)

**功能**:
- 互動式台灣地圖 (Leaflet.js)
- 顯示 58 個空氣品質監測站位置
- 每個測站顯示最新 PM2.5 資料
- 點擊標記顯示詳細資訊
- 顏色編碼表示空氣品質等級

**圖例**:
- 🟢 良好 (< 12)
- 🟡 普通 (12-20)
- 🟠 對敏感族群不健康 (20-30)
- 🔴 不健康 (≥ 30)
- ⚫ 無資料

---

## 🔧 開發指南

### 修改資料庫連線

編輯 `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server;Database=air_quality_db;Uid=your_user;Pwd=your_password;CharSet=utf8mb4;"
  }
}
```

### 新增控制器動作

在 `Controllers/HomeController.cs` 新增方法:
```csharp
public async Task<IActionResult> Statistics()
{
    // 統計邏輯
    return View();
}
```

### 修改分頁大小

在 `Controllers/HomeController.cs` 的 `Index` 方法中:
```csharp
var pageSize = 100;  // 預設 50,可改為其他數值
```

### 新增測站座標

編輯 `App_Data/stations_tw_coords.csv`:
```csv
sitename,lat,lon
新測站,24.1234,120.5678
```

---

## 🐛 常見問題

### 問題 1: 無法連線到資料庫

**錯誤訊息**: `Unable to connect to any of the specified MySQL hosts.`

**解決方法**:
1. 確認 MySQL 服務已啟動
2. 檢查 `appsettings.json` 中的連線字串
3. 確認 MySQL 使用者帳號密碼正確
4. 確認防火牆沒有阻擋 3306 port

### 問題 2: 地圖無法顯示

**原因**: 缺少網路連線 (OpenStreetMap 需要網路)

**解決方法**:
- 確保電腦連上網路
- 檢查瀏覽器主控台是否有 JavaScript 錯誤

### 問題 3: 資料表是空的

**原因**: 資料庫尚未匯入資料

**解決方法**:
```powershell
cd ..
mysql -u root -p --local-infile=1 < import_to_mysql.sql
```

### 問題 4: Port 衝突

**錯誤訊息**: `EADDRINUSE: address already in use`

**解決方法**:
```powershell
# 方法 1: 修改 Port
dotnet run --urls "https://localhost:7001;http://localhost:7000"

# 方法 2: 關閉佔用 Port 的程式
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

---

## 📊 資料庫 Schema

### `air_quality` 資料表

| 欄位              | 型別           | 說明           |
|-------------------|----------------|----------------|
| `id`              | INT            | 主鍵 (自動編號) |
| `site_id`         | VARCHAR(20)    | 測站代碼       |
| `site_name`       | VARCHAR(100)   | 測站名稱       |
| `item_id`         | VARCHAR(20)    | 測項代碼       |
| `item_name`       | VARCHAR(100)   | 測項名稱       |
| `item_eng_name`   | VARCHAR(100)   | 測項英文名稱   |
| `item_unit`       | VARCHAR(50)    | 測項單位       |
| `monitor_month`   | VARCHAR(10)    | 監測月份       |
| `concentration`   | DECIMAL(10,4)  | 監測平均值     |
| `created_at`      | TIMESTAMP      | 資料建立時間   |
| `updated_at`      | TIMESTAMP      | 資料更新時間   |

**索引**:
- PRIMARY KEY (`id`)
- INDEX `idx_site` (`site_id`, `site_name`)
- INDEX `idx_item` (`item_id`, `item_name`)
- INDEX `idx_month` (`monitor_month`)
- INDEX `idx_site_item_month` (`site_id`, `item_id`, `monitor_month`)

---

## 🚀 部署

### 發佈為獨立執行檔

```powershell
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained true

# Linux x64
dotnet publish -c Release -r linux-x64 --self-contained true

# macOS ARM64
dotnet publish -c Release -r osx-arm64 --self-contained true
```

發佈檔案位於 `bin/Release/net9.0/{runtime}/publish/`

### IIS 部署

1. 安裝 [ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/9.0)
2. 發佈專案:
   ```powershell
   dotnet publish -c Release -o ./publish
   ```
3. 在 IIS 建立應用程式集區 (.NET CLR 版本: 無受控碼)
4. 建立網站,指向 `publish` 資料夾
5. 設定 `web.config` (自動產生)

### Docker 部署

建立 `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "AirQualityWeb.dll"]
```

建置與執行:
```powershell
docker build -t air-quality-web .
docker run -d -p 8080:80 --name aqw air-quality-web
```

---

## 📝 授權

此專案為教育用途,資料來源為環保署空氣品質監測網。

---

## 🙋 技術支援

遇到問題? 檢查以下項目:

- [ ] .NET 9.0 SDK 已安裝
- [ ] MySQL 8.0+ 已安裝並執行
- [ ] 資料庫已匯入 (`air_quality_db`)
- [ ] `appsettings.json` 連線字串正確
- [ ] NuGet 套件已還原 (`dotnet restore`)
- [ ] 專案已成功建置 (`dotnet build`)

---

**開發日期**: 2025-12-01  
**框架版本**: ASP.NET Core MVC 9.0  
**資料庫**: MySQL 8.0+
