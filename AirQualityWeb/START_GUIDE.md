# 🚀 快速啟動指南

## 📋 啟動前檢查清單

在啟動網站前,請確認以下項目:

- [ ] MySQL 8.0+ 已安裝並執行
- [ ] 資料庫 `air_quality_db` 已建立且匯入資料
- [ ] 已修改 `appsettings.json` 中的 MySQL 密碼
- [ ] .NET 9.0 SDK 已安裝

---

## ⚡ 方法 1: 命令列啟動 (推薦)

### Step 1: 修改資料庫密碼

編輯 `appsettings.json`,將 `your_password` 改成你的 MySQL root 密碼:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=air_quality_db;Uid=root;Pwd=你的密碼;CharSet=utf8mb4;"
  }
}
```

### Step 2: 測試資料庫連線

```powershell
# 測試 MySQL 連線
mysql -u root -p air_quality_db -e "SELECT COUNT(*) FROM air_quality;"
```

應該顯示資料總筆數 (如果是 0,請先執行上層目錄的 `import_to_mysql.sql`)

### Step 3: 啟動網站

```powershell
# 方式 A: 直接執行
dotnet run

# 方式 B: 指定 Port
dotnet run --urls "https://localhost:7001;http://localhost:7000"
```

### Step 4: 開啟瀏覽器

啟動成功後,開啟瀏覽器訪問:

```
https://localhost:5001
```

或

```
http://localhost:5000
```

---

## 🖱️ 方法 2: Visual Studio 啟動

### Step 1: 用 Visual Studio 開啟專案

```powershell
# 開啟專案檔
start AirQualityWeb.csproj
```

或直接在檔案總管中雙擊 `AirQualityWeb.csproj`

### Step 2: 修改資料庫密碼

在 Visual Studio 中開啟 `appsettings.json`,修改連線字串中的密碼。

### Step 3: 執行專案

按 `F5` 或點選工具列的 ▶️ (Start) 按鈕。

---

## 🎨 方法 3: Visual Studio Code 啟動

### Step 1: 開啟專案資料夾

```powershell
code .
```

### Step 2: 安裝擴充套件

確保已安裝以下 VS Code 擴充套件:
- C# Dev Kit (Microsoft)
- C# (Microsoft)

### Step 3: 修改資料庫密碼

開啟 `appsettings.json`,修改連線字串。

### Step 4: 執行專案

- 方式 A: 按 `F5` (偵錯模式)
- 方式 B: 按 `Ctrl+F5` (非偵錯模式)
- 方式 C: 在終端機執行 `dotnet run`

---

## 🔍 啟動後的畫面

成功啟動後,終端機會顯示:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

## 🌐 訪問網站

開啟瀏覽器,訪問以下任一網址:

### 首頁 (資料瀏覽)
```
https://localhost:5001
```

### 地圖視覺化
```
https://localhost:5001/Home/Map
```

---

## 🎯 使用說明

### 1. 資料瀏覽頁面

- **篩選資料**: 選擇測站、測項、月份後點擊「篩選」
- **關鍵字搜尋**: 在搜尋框輸入測站或測項名稱
- **清除篩選**: 點擊「清除篩選」回到初始狀態
- **翻頁**: 使用底部分頁導航

### 2. 地圖視覺化頁面

- **查看測站**: 點擊地圖上的圓點標記
- **放大/縮小**: 使用滑鼠滾輪或地圖左上角的 +/- 按鈕
- **拖曳地圖**: 滑鼠左鍵拖曳

---

## ⚠️ 常見錯誤排解

### 錯誤 1: 無法連線到 MySQL

**症狀**: 網頁顯示資料庫連線錯誤

**解決方法**:
1. 確認 MySQL 服務已啟動:
   ```powershell
   # 檢查 MySQL 是否執行
   Get-Service MySQL* | Where-Object {$_.Status -eq 'Running'}
   ```

2. 測試連線:
   ```powershell
   mysql -u root -p -e "SELECT 1;"
   ```

3. 檢查 `appsettings.json` 密碼是否正確

### 錯誤 2: 資料表不存在

**症狀**: 錯誤訊息 `Table 'air_quality_db.air_quality' doesn't exist`

**解決方法**:
```powershell
# 回到上層目錄執行匯入腳本
cd ..
mysql -u root -p --local-infile=1 < import_to_mysql.sql
cd AirQualityWeb
```

### 錯誤 3: Port 被佔用

**症狀**: 錯誤訊息 `EADDRINUSE: address already in use`

**解決方法**:
```powershell
# 使用不同的 Port
dotnet run --urls "https://localhost:7001;http://localhost:7000"
```

### 錯誤 4: 地圖無法載入

**症狀**: 地圖頁面空白

**解決方法**:
1. 確認網路連線正常 (OpenStreetMap 需要網路)
2. 按 F12 開啟開發者工具,檢查主控台錯誤
3. 確認 `App_Data/stations_tw_coords.csv` 檔案存在

---

## 🛑 停止網站

在執行 `dotnet run` 的終端機按 `Ctrl+C`

```
Application is shutting down...
```

---

## 📱 測試連結

啟動成功後,可以測試以下連結:

### 首頁
```
http://localhost:5000
```

### 篩選範例
```
http://localhost:5000/Home/Index?site=員林&item=PM2.5
```

### 搜尋範例
```
http://localhost:5000/Home/Index?search=PM2.5
```

### 地圖視覺化
```
http://localhost:5000/Home/Map
```

---

## ✅ 驗證清單

啟動後請驗證:

- [ ] 首頁能正常載入資料表格
- [ ] 篩選功能正常運作
- [ ] 搜尋功能正常運作
- [ ] 分頁導航正常
- [ ] 地圖頁面能顯示台灣地圖
- [ ] 地圖上有測站標記
- [ ] 點擊標記能顯示測站資訊

---

## 🎓 下一步

1. **探索功能**: 試試看不同的篩選組合
2. **查看資料**: 檢視不同測站的監測資料
3. **地圖互動**: 在地圖上探索各測站的空氣品質
4. **自訂修改**: 參考 `README.md` 進行功能擴充

---

**有問題嗎?** 檢查 `README.md` 中的「🐛 常見問題」章節!
