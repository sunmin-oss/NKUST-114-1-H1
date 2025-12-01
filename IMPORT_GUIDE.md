# 空氣品質資料匯入 MySQL 資料庫指南

## 📋 前置作業

### 1. 安裝 MySQL
確保已安裝 MySQL 8.0 或更新版本:
```powershell
# 檢查 MySQL 版本
mysql --version
```

### 2. 啟用 LOCAL INFILE
MySQL 預設可能禁用 `LOAD DATA LOCAL INFILE`,需要啟用:

**方法 A: 臨時啟用 (當次連線有效)**
```sql
SET GLOBAL local_infile = 1;
```

**方法 B: 永久啟用**
編輯 MySQL 設定檔 (通常是 `my.ini` 或 `my.cnf`):
```ini
[mysqld]
local_infile=1

[mysql]
local_infile=1
```
修改後重啟 MySQL 服務。

---

## 🚀 執行步驟

### 方法 1: 使用 MySQL 命令列工具 (推薦)

#### Step 1: 啟動 MySQL 並啟用 LOCAL INFILE
```powershell
# 以啟用 local-infile 模式連線
mysql -u root -p --local-infile=1

# 輸入密碼後,執行以下指令啟用
SET GLOBAL local_infile = 1;
exit;
```

#### Step 2: 執行 SQL 腳本
```powershell
# 切換到專案目錄
cd "d:\大學\大三上\軟體工程\作業1"

# 執行 SQL 腳本
mysql -u root -p --local-infile=1 < import_to_mysql.sql
```

#### Step 3: 驗證匯入結果
```powershell
# 連線到資料庫
mysql -u root -p air_quality_db

# 執行驗證查詢
SELECT COUNT(*) FROM air_quality;
SELECT DISTINCT site_name FROM air_quality LIMIT 10;
```

---

### 方法 2: 使用 MySQL Workbench GUI

#### Step 1: 開啟 MySQL Workbench
1. 連線到 MySQL Server
2. 點選 "File" → "Open SQL Script"
3. 選擇 `import_to_mysql.sql`

#### Step 2: 修改檔案路徑 (如需要)
在腳本中找到以下行,確認路徑正確:
```sql
LOAD DATA LOCAL INFILE 'D:/大學/大三上/軟體工程/作業1/ConsoleApp/App_Data/aqx_p_08_data.json'
```

#### Step 3: 執行腳本
1. 點選工具列的 "Execute" (閃電圖示) 或按 `Ctrl+Shift+Enter`
2. 等待執行完成 (約數秒至數分鐘,視資料量而定)
3. 檢視輸出視窗確認匯入結果

---

### 方法 3: 使用 PowerShell 腳本 (自動化)

建立 PowerShell 腳本自動執行:

```powershell
# save as run_import.ps1

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "空氣品質資料匯入 MySQL" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 設定變數
$mysqlUser = "root"
$mysqlPassword = Read-Host "請輸入 MySQL 密碼" -AsSecureString
$mysqlPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($mysqlPassword)
)
$scriptPath = "d:\大學\大三上\軟體工程\作業1\import_to_mysql.sql"

# Step 1: 啟用 LOCAL INFILE
Write-Host "`n[1/3] 啟用 LOCAL INFILE..." -ForegroundColor Yellow
$enableCmd = "SET GLOBAL local_infile = 1;"
$enableCmd | mysql -u $mysqlUser -p$mysqlPasswordPlain 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ LOCAL INFILE 已啟用" -ForegroundColor Green
} else {
    Write-Host "✗ 啟用失敗,請檢查權限" -ForegroundColor Red
    exit 1
}

# Step 2: 執行 SQL 腳本
Write-Host "`n[2/3] 執行 SQL 匯入腳本..." -ForegroundColor Yellow
mysql -u $mysqlUser -p$mysqlPasswordPlain --local-infile=1 < $scriptPath 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ SQL 腳本執行完成" -ForegroundColor Green
} else {
    Write-Host "✗ SQL 執行失敗" -ForegroundColor Red
    exit 1
}

# Step 3: 驗證結果
Write-Host "`n[3/3] 驗證匯入結果..." -ForegroundColor Yellow
$verifyCmd = @"
USE air_quality_db;
SELECT CONCAT('總筆數: ', COUNT(*)) FROM air_quality;
"@
$verifyCmd | mysql -u $mysqlUser -p$mysqlPasswordPlain 2>&1

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "✓ 匯入完成!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "資料庫: air_quality_db" -ForegroundColor White
Write-Host "資料表: air_quality" -ForegroundColor White
```

執行:
```powershell
powershell -ExecutionPolicy Bypass -File run_import.ps1
```

---

## 🔍 常見問題排解

### 問題 1: `ERROR 1148: The used command is not allowed with this MySQL version`
**原因**: LOCAL INFILE 未啟用  
**解決**:
```sql
SET GLOBAL local_infile = 1;
```
並在連線時加上 `--local-infile=1` 參數。

### 問題 2: `ERROR 2: File not found`
**原因**: JSON 檔案路徑錯誤  
**解決**:
1. 確認檔案存在: `Test-Path "d:\大學\大三上\軟體工程\作業1\ConsoleApp\App_Data\aqx_p_08_data.json"`
2. 修改 SQL 腳本中的路徑,使用**絕對路徑**
3. Windows 路徑使用 `/` 而非 `\`

### 問題 3: `ERROR 1290: The MySQL server is running with the --secure-file-priv option`
**原因**: MySQL 限制檔案存取路徑  
**解決**:
```sql
-- 查詢允許的路徑
SHOW VARIABLES LIKE 'secure_file_priv';

-- 方法 1: 將 JSON 檔案複製到允許的路徑
-- 方法 2: 修改 my.ini 設定 secure_file_priv=""
```

### 問題 4: JSON 解析錯誤
**原因**: JSON 格式問題  
**解決**:
1. 確認 JSON 檔案編碼為 UTF-8
2. 檢查 JSON 格式是否正確: `Get-Content aqx_p_08_data.json | ConvertFrom-Json`

---

## 📊 匯入後的資料表結構

```
air_quality 資料表欄位:
├── id (主鍵)
├── site_id (測站代碼)
├── site_name (測站名稱)
├── item_id (測項代碼)
├── item_name (測項名稱)
├── item_eng_name (測項英文名稱)
├── item_unit (測項單位)
├── monitor_month (監測月份)
├── concentration (監測平均值)
├── created_at (建立時間)
└── updated_at (更新時間)

索引:
├── PRIMARY KEY (id)
├── INDEX idx_site (site_id, site_name)
├── INDEX idx_item (item_id, item_name)
├── INDEX idx_month (monitor_month)
└── INDEX idx_site_item_month (site_id, item_id, monitor_month)
```

---

## 🎯 查詢範例

### 查詢員林測站的所有資料
```sql
SELECT * FROM air_quality 
WHERE site_name = '員林' 
ORDER BY monitor_month DESC, item_name;
```

### 查詢 PM2.5 濃度最高的前 10 筆
```sql
SELECT site_name, monitor_month, concentration 
FROM air_quality 
WHERE item_name = 'PM2.5' AND concentration IS NOT NULL
ORDER BY concentration DESC 
LIMIT 10;
```

### 查詢 2024 年的所有資料
```sql
SELECT * FROM air_quality 
WHERE monitor_month LIKE '2024%'
ORDER BY monitor_month DESC;
```

### 統計各測站的資料筆數
```sql
SELECT site_name, COUNT(*) as total
FROM air_quality
GROUP BY site_name
ORDER BY total DESC;
```

---

## 📝 注意事項

1. **備份**: 匯入前建議先備份現有資料庫
2. **權限**: 確保 MySQL 使用者有 `FILE` 權限
3. **編碼**: JSON 檔案必須是 UTF-8 編碼
4. **路徑**: Windows 路徑在 SQL 中使用 `/` 分隔
5. **效能**: 10,000 筆資料約需 10-30 秒 (視硬體而定)

---

## ✅ 驗證清單

- [ ] MySQL 8.0+ 已安裝
- [ ] `local_infile` 已啟用
- [ ] JSON 檔案路徑正確
- [ ] MySQL 使用者有 FILE 權限
- [ ] 執行 SQL 腳本成功
- [ ] 資料表 `air_quality` 已建立
- [ ] 總筆數符合預期
- [ ] 可正常查詢資料

---

需要協助? 執行以下命令檢查狀態:
```powershell
mysql -u root -p -e "USE air_quality_db; SELECT COUNT(*) FROM air_quality;"
```
