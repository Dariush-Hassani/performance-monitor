using Microsoft.Web.WebView2.WinForms;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace performance_monitor
{
    public partial class Form1 : Form
    {
        private WebView2 webView;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await InitializeWebView();
        }

        private async Task InitializeWebView()
        {
            webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            webView.Dock = DockStyle.Fill;
            this.Controls.Add(webView);

            await webView.EnsureCoreWebView2Async(null);

            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            webView.CoreWebView2.Navigate("http://localhost:5173");
            webView.CoreWebView2.OpenDevToolsWindow();
        }


        private void CoreWebView2_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // ✨ تغییر مهم اینجاست ✨
                // به جای TryGetWebMessageAsString از WebMessageAsJson استفاده شد
                string jsonMessage = e.WebMessageAsJson;

                // اگر جاوااسکریپت رشته متنی ساده (String) بفرستد، کوتیشن های اضافه را حذف می کند
                if (jsonMessage.StartsWith("\"") && jsonMessage.EndsWith("\""))
                {
                    jsonMessage = jsonMessage.Trim('"');
                    // برای حالتی که با JSON.stringify سمت جاوااسکریپت فرستاده شده باشد
                    jsonMessage = jsonMessage.Replace("\\\"", "\"");
                }

                var request = JsonSerializer.Deserialize<WebMessageDto>(jsonMessage);

                if (request == null || string.IsNullOrEmpty(request.id)) return;

                object responseData = null;

                switch (request.endpoint)
                {
                    case "getSystemInfo":
                        responseData = new { os = "Windows 11", ram = "16GB", cpu = "Core i7" };
                        break;
                    case "saveData":
                        responseData = new { success = true, received = request.payload };
                        break;
                    default:
                        responseData = new { error = "Endpoint not found" };
                        break;
                }

                var responseMessage = new
                {
                    id = request.id,
                    data = responseData
                };

                string jsonResponse = JsonSerializer.Serialize(responseMessage);
                webView.CoreWebView2.PostWebMessageAsJson(jsonResponse);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing message: " + ex.Message);
            }
        }
    }
}

public class WebMessageDto
{
    public string id { get; set; }
    public string endpoint { get; set; }
    public JsonElement payload { get; set; } 
}
