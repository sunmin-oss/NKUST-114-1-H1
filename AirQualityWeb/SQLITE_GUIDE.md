# 🚀 SQLite 版本快速啟動指南

## ✨ 優點

使用 **SQLite** 版本的優勢:
- ✅ **無需安裝資料庫** - 不用安裝 MySQL
- ✅ **無需設定密碼** - 開箱即用
- ✅ **自動建立資料庫** - 首次執行自動建立
- ✅ **資料檔案化** - 資料庫就是一個檔案 (`App_Data/air_quality.db`)
- ✅ **輕量快速** - 適合中小型資料集
- ✅ **跨平台** - Windows, Linux, macOS 都能用

---

## 🎯 快速開始 (3 步驟)

### Step 1: 確認專案完整

確保以下檔案存在:
```
AirQualityWeb/
├── App_Data/
│   └── stations_tw_coords.csv    ✅ 測站座標
└── ../ConsoleApp/App_Data/
    └── aqx_p_08_data.json         ✅ 空氣品質資料
```

### Step 2: 執行網站

```powershell
cd "d:\大學\大三上\軟體工程\作業1\AirQualityWeb"
dotnet run
```

### Step 3: 開啟瀏覽器並匯入資料

1. 訪問: **http://localhost:5000** 或 **https://localhost:5001**
2. 首頁會顯示「資料庫為空,請點擊匯入資料」
3. 點擊 **「匯入資料」** 按鈕
4. 等待數秒,資料匯入完成!
5. 開始使用篩選、搜尋、地圖等功能

---

## 📊 使用說明

### 🔄 匯入資料

首次使用時,資料庫是空的,需要從 JSON 匯入資料:

1. 點擊首頁的 **「匯入資料」** 按鈕
2. 系統會從 `../ConsoleApp/App_Data/aqx_p_08_data.json` 讀取資料
3. 自動匯入到 SQLite 資料庫 (`App_Data/air_quality.db`)
4. 匯入完成後會顯示成功訊息與筆數

> **注意**: 匯入不會刪除既有資料,重複匯入會產生重複記錄

### 📂 資料庫檔案位置

資料庫檔案位於:
```
AirQualityWeb/App_Data/air_quality.db
```

這是一個 SQLite 資料庫檔案,可以用以下工具開啟:
- [DB Browser for SQLite](https://sqlitebrowser.org/) (免費GUI工具)
- SQLite 命令列工具
- Visual Studio Code + SQLite 擴充套件

### 🗑️ 重置資料庫

如果想要清空資料庫重新開始:

```powershell
# 方法 1: 刪除資料庫檔案
Remove-Item "App_Data/air_quality.db" -Force

# 方法 2: 用 SQLite 命令清空資料表
sqlite3 App_Data/air_quality.db "DELETE FROM air_quality;"
```

---

## 🎨 功能介紹

### 1️⃣ 資料瀏覽頁面

**URL**: `http://localhost:5000`

**功能**:
- 分頁顯示 (每頁 50 筆)
- 測站篩選 (下拉選單)
- 測項篩選 (下拉選單)
- 監測月份篩選 (下拉選單)
- 關鍵字搜尋
- 監測值顏色標示 (綠/黃/橘/紅)

**操作**:
1. 選擇篩選條件
2. 點擊「篩選」按鈕
3. 使用分頁導航瀏覽資料
4. 點擊「清除篩選」重置

### 2️⃣ 地圖視覺化

**URL**: `http://localhost:5000/Home/Map`

**功能**:
- 互動式台灣地圖 (Leaflet.js)
- 58 個空氣品質監測站
- PM2.5 濃度顏色編碼
- 點擊標記顯示詳細資訊

**操作**:
1. 點擊首頁的「地圖視覺化」按鈕
2. 地圖自動載入所有測站
3. 滑鼠拖曳移動地圖
4. 滾輪縮放地圖
5. 點擊標記查看詳細資訊

### 3️⃣ 資料匯入

**URL**: `http://localhost:5000/Home/ImportData`

**功能**:
- 從 JSON 檔案匯入資料到 SQLite
- 自動解析 JSON 結構
- 批次插入提升效能
- 交易保證資料一致性

---

## 🔧 技術細節

### 資料庫結構

```sql
CREATE TABLE air_quality (
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

CREATE INDEX idx_site ON air_quality(site_name);
CREATE INDEX idx_item ON air_quality(item_name);
CREATE INDEX idx_month ON air_quality(monitor_month);
```

### 連線字串

`appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=App_Data/air_quality.db"
  }
}
```

### NuGet 套件

```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.0" />
<PackageReference Include="System.Data.SQLite" Version="1.0.119" />
```

---

## 🐛 常見問題

### 問題 1: 找不到 JSON 檔案

**錯誤訊息**: `找不到 JSON 檔案: ...`

**解決方法**:
```powershell
# 確認 JSON 檔案存在
Test-Path "..\ConsoleApp\App_Data\aqx_p_08_data.json"

# 如果不存在,請確認專案結構
```

### 問題 2: 資料庫檔案被鎖定

**錯誤訊息**: `database is locked`

**解決方法**:
```powershell
# 停止網站 (在執行 dotnet run 的終端機按 Ctrl+C)
# 刪除資料庫檔案
Remove-Item "App_Data/air_quality.db" -Force
# 重新啟動網站
dotnet run
```

### 問題 3: 資料重複匯入

**症狀**: 匯入兩次,資料變成兩倍

**解決方法**:
```powershell
# 清空資料表
sqlite3 App_Data/air_quality.db "DELETE FROM air_quality;"
# 重新匯入
```

### 問題 4: 地圖無法顯示

**原因**: 需要網路連線 (OpenStreetMap)

**解決方法**:
- 確保電腦連上網路
- 檢查瀏覽器主控台 (F12) 是否有錯誤

---

## 📈 效能提示

### 匯入速度

- **1000 筆**: 約 1-2 秒
- **10000 筆**: 約 10-20 秒
- **100000 筆**: 約 1-2 分鐘

### 查詢速度

- **無篩選**: 幾乎即時
- **單一篩選**: < 100ms
- **多重篩選**: < 200ms
- **關鍵字搜尋**: < 500ms

### 優化建議

1. **定期重建索引**:
   ```sql
   REINDEX;
   ```

2. **使用 VACUUM 壓縮資料庫**:
   ```sql
   VACUUM;
   ```

3. **分析查詢效能**:
   ```sql
   EXPLAIN QUERY PLAN SELECT * FROM air_quality WHERE site_name = '員林';
   ```

---

## 🎓 SQLite 工具推薦

### 1. DB Browser for SQLite (GUI)
- **下載**: https://sqlitebrowser.org/
- **功能**: 瀏覽、編輯、查詢、匯出
- **平台**: Windows, macOS, Linux

### 2. SQLite 命令列
```powershell
# 安裝 (Chocolatey)
choco install sqlite

# 使用
sqlite3 App_Data/air_quality.db

# 常用命令
.tables              # 列出所有資料表
.schema air_quality  # 顯示資料表結構
SELECT COUNT(*) FROM air_quality;  # 查詢總筆數
.quit                # 離開
```

### 3. VS Code 擴充套件
- **SQLite Viewer** (qwtel.sqlite-viewer)
- **SQLite** (alexcvzz.vscode-sqlite)

---

## ✅ 驗證清單

啟動成功後請檢查:

- [ ] 網站能正常載入 (http://localhost:5000)
- [ ] 首頁顯示「資料庫為空」提示 (首次執行)
- [ ] 點擊「匯入資料」成功匯入
- [ ] 資料表格正常顯示
- [ ] 篩選功能正常
- [ ] 搜尋功能正常
- [ ] 分頁導航正常
- [ ] 地圖頁面能顯示台灣地圖
- [ ] 地圖上有測站標記
- [ ] 點擊標記能顯示測站資訊
- [ ] `App_Data/air_quality.db` 檔案已建立

---

## 📝 開發提示

### 檢視資料庫內容

```powershell
# 用 SQLite 命令列
sqlite3 App_Data/air_quality.db "SELECT * FROM air_quality LIMIT 10;"

# 查看統計
sqlite3 App_Data/air_quality.db "SELECT COUNT(*) AS 總筆數 FROM air_quality;"
sqlite3 App_Data/air_quality.db "SELECT COUNT(DISTINCT site_name) AS 測站數 FROM air_quality;"
```

### 備份資料庫

```powershell
# 複製檔案即可
Copy-Item "App_Data/air_quality.db" -Destination "Backup/air_quality_$(Get-Date -Format 'yyyyMMdd').db"
```

### 還原資料庫

```powershell
# 覆蓋現有檔案
Copy-Item "Backup/air_quality_20251201.db" -Destination "App_Data/air_quality.db" -Force
```

---

**開發日期**: 2025-12-01  
**資料庫**: SQLite 3  
**框架**: ASP.NET Core MVC 9.0  
**優勢**: 零設定、開箱即用! 🎉
