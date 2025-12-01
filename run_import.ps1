# run_import.ps1 - 自動化 MySQL 匯入腳本
# 用途: 一鍵執行 JSON 資料匯入 MySQL

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  空氣品質資料匯入 MySQL 自動化腳本  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ========================================
# 設定變數
# ========================================
$mysqlUser = "root"
$scriptPath = "d:\大學\大三上\軟體工程\作業1\import_to_mysql.sql"
$jsonPath = "d:\大學\大三上\軟體工程\作業1\ConsoleApp\App_Data\aqx_p_08_data.json"

# ========================================
# 前置檢查
# ========================================
Write-Host "[檢查] 驗證環境..." -ForegroundColor Yellow

# 檢查 MySQL 是否安裝
try {
    $mysqlVersion = mysql --version 2>&1
    Write-Host "✓ MySQL 已安裝: $mysqlVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ 找不到 MySQL,請先安裝 MySQL 8.0+" -ForegroundColor Red
    exit 1
}

# 檢查 SQL 腳本是否存在
if (-not (Test-Path $scriptPath)) {
    Write-Host "✗ 找不到 SQL 腳本: $scriptPath" -ForegroundColor Red
    exit 1
}
Write-Host "✓ SQL 腳本存在" -ForegroundColor Green

# 檢查 JSON 檔案是否存在
if (-not (Test-Path $jsonPath)) {
    Write-Host "✗ 找不到 JSON 檔案: $jsonPath" -ForegroundColor Red
    exit 1
}
Write-Host "✓ JSON 檔案存在" -ForegroundColor Green

Write-Host ""

# ========================================
# 取得 MySQL 密碼
# ========================================
$mysqlPassword = Read-Host "請輸入 MySQL root 密碼" -AsSecureString
$mysqlPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($mysqlPassword)
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  開始匯入程序  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ========================================
# Step 1: 測試 MySQL 連線
# ========================================
Write-Host "[1/4] 測試 MySQL 連線..." -ForegroundColor Yellow
try {
    $testCmd = "SELECT 'Connection OK' AS status;"
    $result = $testCmd | mysql -u $mysqlUser -p$mysqlPasswordPlain 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "連線失敗"
    }
    Write-Host "✓ MySQL 連線成功" -ForegroundColor Green
} catch {
    Write-Host "✗ MySQL 連線失敗,請檢查密碼是否正確" -ForegroundColor Red
    exit 1
}

Write-Host ""

# ========================================
# Step 2: 啟用 LOCAL INFILE
# ========================================
Write-Host "[2/4] 啟用 LOCAL INFILE..." -ForegroundColor Yellow
try {
    $enableCmd = "SET GLOBAL local_infile = 1;"
    $enableCmd | mysql -u $mysqlUser -p$mysqlPasswordPlain 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠ 無法啟用 LOCAL INFILE (可能已啟用或權限不足)" -ForegroundColor Yellow
    } else {
        Write-Host "✓ LOCAL INFILE 已啟用" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠ LOCAL INFILE 設定警告 (繼續執行)" -ForegroundColor Yellow
}

Write-Host ""

# ========================================
# Step 3: 執行 SQL 腳本
# ========================================
Write-Host "[3/4] 執行 SQL 匯入腳本 (請稍候)..." -ForegroundColor Yellow
$startTime = Get-Date

try {
    # 使用 --local-infile=1 參數執行
    $output = mysql -u $mysqlUser -p$mysqlPasswordPlain --local-infile=1 < $scriptPath 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ SQL 執行失敗" -ForegroundColor Red
        Write-Host "錯誤訊息:" -ForegroundColor Red
        Write-Host $output -ForegroundColor Red
        exit 1
    }
    
    $endTime = Get-Date
    $duration = ($endTime - $startTime).TotalSeconds
    
    Write-Host "✓ SQL 腳本執行完成 (耗時: $([math]::Round($duration, 2)) 秒)" -ForegroundColor Green
} catch {
    Write-Host "✗ SQL 執行失敗: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# ========================================
# Step 4: 驗證匯入結果
# ========================================
Write-Host "[4/4] 驗證匯入結果..." -ForegroundColor Yellow

try {
    # 查詢總筆數
    $countCmd = @"
USE air_quality_db;
SELECT COUNT(*) AS total FROM air_quality;
"@
    $totalRecords = $countCmd | mysql -u $mysqlUser -p$mysqlPasswordPlain -N 2>&1
    
    # 查詢測站數量
    $sitesCmd = @"
USE air_quality_db;
SELECT COUNT(DISTINCT site_name) AS sites FROM air_quality;
"@
    $totalSites = $sitesCmd | mysql -u $mysqlUser -p$mysqlPasswordPlain -N 2>&1
    
    # 查詢測項數量
    $itemsCmd = @"
USE air_quality_db;
SELECT COUNT(DISTINCT item_name) AS items FROM air_quality;
"@
    $totalItems = $itemsCmd | mysql -u $mysqlUser -p$mysqlPasswordPlain -N 2>&1
    
    Write-Host "✓ 匯入驗證成功" -ForegroundColor Green
    Write-Host ""
    Write-Host "  總筆數: $totalRecords" -ForegroundColor White
    Write-Host "  測站數: $totalSites" -ForegroundColor White
    Write-Host "  測項數: $totalItems" -ForegroundColor White
    
} catch {
    Write-Host "⚠ 驗證時發生錯誤,但資料可能已成功匯入" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ✓ 匯入完成!  " -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "資料庫名稱: air_quality_db" -ForegroundColor White
Write-Host "資料表名稱: air_quality" -ForegroundColor White
Write-Host ""
Write-Host "查詢資料範例:" -ForegroundColor Yellow
Write-Host "  mysql -u root -p air_quality_db" -ForegroundColor Cyan
Write-Host "  SELECT * FROM air_quality LIMIT 10;" -ForegroundColor Cyan
Write-Host ""

# 詢問是否要開啟 MySQL 命令列
$openMysql = Read-Host "是否要開啟 MySQL 命令列查看資料? (Y/N)"
if ($openMysql -eq "Y" -or $openMysql -eq "y") {
    Write-Host ""
    Write-Host "正在開啟 MySQL..." -ForegroundColor Cyan
    Start-Process mysql -ArgumentList "-u", $mysqlUser, "-p$mysqlPasswordPlain", "air_quality_db" -NoNewWindow -Wait
}

Write-Host ""
Write-Host "執行完畢!" -ForegroundColor Green
