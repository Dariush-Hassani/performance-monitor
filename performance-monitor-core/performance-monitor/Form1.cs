using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace performance_monitor
{
    public partial class Form1 : Form
    {
        private WebView2 webView;
        private HardwareMonitor _hardwareMonitor;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            _hardwareMonitor = new HardwareMonitor();
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
            string id = null;

            try
            {
                string jsonMessage = e.WebMessageAsJson;

                if (string.IsNullOrEmpty(jsonMessage) || jsonMessage == "null") return;

                JObject requestMessage = JObject.Parse(jsonMessage);

                id = requestMessage["id"]?.ToString();
                string endpoint = requestMessage["endpoint"]?.ToString();

                object data = null;
                string error = null;

                switch (endpoint)
                {
                    case "getCpuStats":
                        data = _hardwareMonitor.GetCpuStats();
                        break;
                    case "getRamStats":
                        data = _hardwareMonitor.GetRamStats();
                        break;
                    case "getDiskStats":
                        data = _hardwareMonitor.GetDiskStats();
                        break;
                    default:
                        error = $"Endpoint '{endpoint}' not found in C#.";
                        break;
                }

                var responseMessage = new
                {
                    id = id,
                    data = data,
                    error = error
                };

                string jsonResponse = JsonConvert.SerializeObject(responseMessage);

                if (webView?.CoreWebView2 != null)
                {
                    webView.CoreWebView2.PostWebMessageAsJson(jsonResponse);
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    id = id,
                    data = (object)null,
                    error = ex.Message
                };

                if (webView?.CoreWebView2 != null)
                {
                    webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(errorResponse));
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _hardwareMonitor?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
