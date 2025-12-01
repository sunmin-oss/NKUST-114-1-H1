-- ========================================
-- 空氣品質資料 JSON 一次性匯入 MySQL
-- 作者: GitHub Copilot
-- 日期: 2025-12-01
-- ========================================

-- ========================================
-- 步驟 1: 建立資料庫
-- ========================================
CREATE DATABASE IF NOT EXISTS air_quality_db 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE air_quality_db;

-- ========================================
-- 步驟 2: 建立資料表
-- ========================================
DROP TABLE IF EXISTS air_quality;

CREATE TABLE air_quality (
    -- 主鍵
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT '自動編號',
    
    -- 測站資訊
    site_id VARCHAR(20) NOT NULL COMMENT '測站代碼',
    site_name VARCHAR(100) NOT NULL COMMENT '測站名稱',
    
    -- 測項資訊
    item_id VARCHAR(20) NOT NULL COMMENT '測項代碼',
    item_name VARCHAR(100) NOT NULL COMMENT '測項名稱',
    item_eng_name VARCHAR(100) COMMENT '測項英文名稱',
    item_unit VARCHAR(50) COMMENT '測項單位',
    
    -- 監測資料
    monitor_month VARCHAR(10) NOT NULL COMMENT '監測月份 (YYYY-MM)',
    concentration DECIMAL(10, 4) COMMENT '監測平均值',
    
    -- 時間戳記
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '資料建立時間',
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '資料更新時間',
    
    -- 索引優化查詢效能
    INDEX idx_site (site_id, site_name),
    INDEX idx_item (item_id, item_name),
    INDEX idx_month (monitor_month),
    INDEX idx_site_item_month (site_id, item_id, monitor_month)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='空氣品質監測資料';

-- ========================================
-- 步驟 3: 建立暫存表用於 JSON 匯入
-- ========================================
DROP TABLE IF EXISTS air_quality_temp;

CREATE TABLE air_quality_temp (
    json_data JSON NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ========================================
-- 步驟 4: 匯入 JSON 檔案到暫存表
-- ========================================
-- 注意: 請根據你的實際檔案路徑修改下方路徑
-- 範例路徑: D:/大學/大三上/軟體工程/作業1/ConsoleApp/App_Data/aqx_p_08_data.json

LOAD DATA LOCAL INFILE 'D:/大學/大三上/軟體工程/作業1/ConsoleApp/App_Data/aqx_p_08_data.json'
INTO TABLE air_quality_temp
LINES TERMINATED BY '\n'
(json_data);

-- ========================================
-- 步驟 5: 從暫存表解析 JSON 並插入正式表
-- ========================================
INSERT INTO air_quality (
    site_id, 
    site_name, 
    item_id, 
    item_name, 
    item_eng_name, 
    item_unit, 
    monitor_month, 
    concentration
)
SELECT 
    JSON_UNQUOTE(JSON_EXTRACT(record, '$.siteid')) AS site_id,
    JSON_UNQUOTE(JSON_EXTRACT(record, '$.sitename')) AS site_name,
    JSON_UNQUOTE(JSON_EXTRACT(record, '$.itemid')) AS item_id,
    JSON_UNQUOTE(JSON_EXTRACT(record, '$.itemname')) AS item_name,
    JSON_UNQUOTE(JSON_EXTRACT(record, '$.itemengname')) AS item_eng_name,
    JSON_UNQUOTE(JSON_EXTRACT(record, '$.itemunit')) AS item_unit,
    JSON_UNQUOTE(JSON_EXTRACT(record, '$.monitormonth')) AS monitor_month,
    CAST(JSON_UNQUOTE(JSON_EXTRACT(record, '$.concentration')) AS DECIMAL(10,4)) AS concentration
FROM air_quality_temp,
     JSON_TABLE(
         json_data,
         '$.records[*]' COLUMNS (
             record JSON PATH '$'
         )
     ) AS jt;

-- ========================================
-- 步驟 6: 清理暫存表
-- ========================================
DROP TABLE IF EXISTS air_quality_temp;

-- ========================================
-- 步驟 7: 驗證匯入結果
-- ========================================
-- 檢查總筆數
SELECT '總筆數' AS 項目, COUNT(*) AS 數量 FROM air_quality;

-- 檢查測站數量
SELECT '測站數量' AS 項目, COUNT(DISTINCT site_name) AS 數量 FROM air_quality;

-- 檢查測項數量
SELECT '測項數量' AS 項目, COUNT(DISTINCT item_name) AS 數量 FROM air_quality;

-- 顯示前 10 筆資料
SELECT 
    site_name AS 測站,
    item_name AS 測項,
    item_unit AS 單位,
    monitor_month AS 月份,
    concentration AS 數值
FROM air_quality 
ORDER BY id 
LIMIT 10;

-- 顯示各測站的資料筆數
SELECT 
    site_name AS 測站名稱,
    COUNT(*) AS 資料筆數
FROM air_quality
GROUP BY site_name
ORDER BY 資料筆數 DESC
LIMIT 10;

-- ========================================
-- 常用查詢範例
-- ========================================

-- 查詢特定測站的所有資料
-- SELECT * FROM air_quality WHERE site_name = '員林' ORDER BY monitor_month DESC, item_name;

-- 查詢特定測項的所有測站資料
-- SELECT * FROM air_quality WHERE item_name = 'PM2.5' ORDER BY concentration DESC;

-- 查詢特定月份的所有資料
-- SELECT * FROM air_quality WHERE monitor_month = '2024-01' ORDER BY site_name, item_name;

-- 查詢濃度最高的前 10 筆記錄
-- SELECT site_name, item_name, monitor_month, concentration 
-- FROM air_quality 
-- WHERE concentration IS NOT NULL 
-- ORDER BY concentration DESC 
-- LIMIT 10;

-- ========================================
-- 完成!
-- ========================================
