## 專案說明

本專案為 .NET 9.0 的 C# 主控台應用程式，讀取並解析空氣品質月平均資料（aqx_p_08），以 System.Text.Json 反序列化並於主控台列出前 10 筆紀錄。

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

```powershell
# 建置（Release 組態）
dotnet build "ConsoleApp/ConsoleApp.csproj" -c Release

# 直接執行（以專案執行）
dotnet run --project "ConsoleApp" -c Release

# 或執行已編譯的可執行檔
& "ConsoleApp/bin/Release/net9.0/ConsoleApp.exe"
```

執行後會在主控台輸出前 10 筆資料；若某筆 `concentration` 為空，會顯示「(無資料)」。

## 專案結構

```text
NKUST-114-1-H1/
├─ ConsoleApp/
│  ├─ ConsoleApp.csproj
│  ├─ Program.cs
│  ├─ AirInfo.cs
│  └─ App_Data/
│     └─ aqx_p_08_data.json
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
