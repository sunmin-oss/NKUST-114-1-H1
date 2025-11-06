using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using ConsoleApp;

namespace AirQualityWinForms
{
    public partial class MapForm : Form
    {
        private readonly List<AirInfo> _data;
        private readonly Dictionary<string, (double lat, double lon)> _coords;

        public MapForm(List<AirInfo> data, Dictionary<string, (double lat, double lon)> coords)
        {
            _data = data;
            _coords = coords;
            InitializeComponent();
        }

        private async void MapForm_Load(object? sender, EventArgs e)
        {
            await EnsureWebView2Ready();
            var html = BuildHtml();
            webView.NavigateToString(html);
        }

        private async Task EnsureWebView2Ready()
        {
            try
            {
                if (webView.CoreWebView2 == null)
                {
                    await webView.EnsureCoreWebView2Async();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化 WebView2 失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string BuildHtml()
        {
            // 聚合資料：以測站為單位，取目前資料的 concentration（如有多筆取第一筆或可再強化）
            var points = new List<object>();
            foreach (var g in _data.GroupBy(d => d.SiteName))
            {
                var name = g.Key;
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (!_coords.TryGetValue(name, out var coord)) continue;

                var first = g.First();
                double? value = null;
                if (double.TryParse(first.Concentration, out var v)) value = v;

                points.Add(new
                {
                    name,
                    lat = coord.lat,
                    lon = coord.lon,
                    item = first.ItemName,
                    month = first.MonitorMonth,
                    unit = first.ItemUnit,
                    value
                });
            }

            var json = JsonSerializer.Serialize(points, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var sb = new StringBuilder();
            sb.Append(@"<!doctype html>");
            sb.Append(@"<html><head><meta charset='utf-8'>");
            sb.Append(@"<meta name='viewport' content='width=device-width, initial-scale=1'>");
            sb.Append(@"<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />");
            sb.Append(@"<style>html,body,#map{height:100%;margin:0;padding:0;} .legend{background:#fff;padding:8px;border-radius:4px;line-height:1.4;}");
            sb.Append(@".marker-label{font-size:12px;font-weight:600;background:#fff;padding:2px 4px;border-radius:3px;border:1px solid #999;}");
            sb.Append(@"</style></head><body><div id='map'></div>");
            sb.Append(@"<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>");
            sb.Append(@"<script>");
            sb.Append(@"const map=L.map('map',{center:[23.7,120.9],zoom:7});");
            sb.Append(@"L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',{maxZoom:19,attribution:'&copy; OpenStreetMap contributors'}).addTo(map);");
            sb.Append($"const points={json};\n");
            sb.Append(@"function color(v){ if(v==null) return '#777'; if(v<12) return '#2ecc71'; if(v<20) return '#f1c40f'; if(v<30) return '#e67e22'; return '#e74c3c'; }");
            sb.Append(@"points.forEach(p=>{ const marker=L.circleMarker([p.lat,p.lon],{radius:8,fillColor:color(p.value),color:'#333',weight:1,fillOpacity:0.9}).addTo(map);");
            sb.Append(@"const val=(p.value==null)?'(無資料)':p.value; marker.bindPopup(`<b>${p.name}</b><br/>${p.item}：${val} ${p.unit||''}<br/>月份：${p.month}`); });");
            sb.Append(@"const legend=L.control({position:'bottomright'}); legend.onAdd=function(){const d=L.DomUtil.create('div','legend'); d.innerHTML='<div><b>PM/濃度顏色</b></div><div><span style=\'background:#2ecc71;display:inline-block;width:12px;height:12px;margin-right:6px\'></span><12</div><div><span style=\'background:#f1c40f;display:inline-block;width:12px;height:12px;margin-right:6px\'></span>12-20</div><div><span style=\'background:#e67e22;display:inline-block;width:12px;height:12px;margin-right:6px\'></span>20-30</div><div><span style=\'background:#e74c3c;display:inline-block;width:12px;height:12px;margin-right:6px\'></span>>=30</div>'; return d;}; legend.addTo(map);");
            sb.Append(@"</script></body></html>");
            return sb.ToString();
        }
    }
}
