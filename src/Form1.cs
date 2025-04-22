using System.Text.Json;
using System.Text.Json.Nodes;
using APIRunner.Models;
using APIRunner.Business;
using APIRunner.Services;
using APIRunner.Enums;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Web.WebView2.Core;
namespace APIRunner
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public partial class Form1 : Form
    {
        #region Propriedades e Campos
        public Config Config { get; set; }
        private System.Windows.Forms.Timer? resizeTimer;
        private Size targetSize;
        private int resizeStep = 10;
        private GitService? _gitService;
        private ConfigManagerService? _configManagerService;
        private ProcessManagerService? _processManagerService;
        private WebViewService? _webViewService;
        #endregion
        #region Construtor e Inicialização
        public Form1()
        {
            InitializeComponent();

            _configManagerService = new ConfigManagerService("config.json");

            if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                this.FormClosing += Form1_FormClosing;
            }

            if (File.Exists("config-update.json"))
            {
                File.Delete("config.json");
                File.Move("config-update.json", "config.json");
            }

            Config = _configManagerService.LoadConfig();

            UpdateFormSize(Config.CompactInterface, animate: false);
        }
        #endregion

        #region Manipulação de Interface
        public void UpdateFormSize(bool isCompactInterface, bool animate = false)
        {
            Size newSize = isCompactInterface ? new Size(905, 461) : new Size(905, 549);

            if (animate)
            {
                StartSmoothResize(newSize);
            }
            else
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
                {
                    this.ClientSize = newSize;
                }
            }
        }

        private void StartSmoothResize(Size newSize)
        {
            targetSize = newSize;

            if (resizeTimer == null)
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
                {
                    resizeTimer = new System.Windows.Forms.Timer();
                }
                else
                {
                    throw new PlatformNotSupportedException("System.Windows.Forms.Timer is only supported on Windows 6.1 and later.");
                }
                resizeTimer.Interval = 10;
                resizeTimer.Tick += ResizeTimer_Tick;
            }

            resizeTimer.Start();
        }

        private void ResizeTimer_Tick(object? sender, EventArgs e)
        {
            int widthDiff = 0;
            int heightDiff = 0;

            if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                widthDiff = targetSize.Width - this.ClientSize.Width;
                heightDiff = targetSize.Height - this.ClientSize.Height;
            }

            if (Math.Abs(widthDiff) <= resizeStep && Math.Abs(heightDiff) <= resizeStep)
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
                {
                    this.ClientSize = targetSize;
                }
                if (resizeTimer != null && OperatingSystem.IsWindowsVersionAtLeast(6, 1))
                {
                    resizeTimer.Stop();
                }
                return;
            }

            int newWidth = OperatingSystem.IsWindowsVersionAtLeast(6, 1)
                ? this.ClientSize.Width + Math.Sign(widthDiff) * Math.Min(resizeStep, Math.Abs(widthDiff))
                : OperatingSystem.IsWindowsVersionAtLeast(6, 1) ? this.ClientSize.Width : 0;
            int newHeight = OperatingSystem.IsWindowsVersionAtLeast(6, 1)
                ? this.ClientSize.Height + Math.Sign(heightDiff) * Math.Min(resizeStep, Math.Abs(heightDiff))
                : 0;

            if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                this.ClientSize = new Size(newWidth, newHeight);
            }
        }
        #endregion

        #region Eventos do Formulário
        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                var options = new CoreWebView2EnvironmentOptions();
                options.AdditionalBrowserArguments = "--enable-features=ExperimentalJavaScript";

                string? userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "APIRunner"); // Ou outro nome adequado
                var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);

                await webView22.EnsureCoreWebView2Async(environment);

                if (webView22.CoreWebView2 == null)
                {
                    Trace.WriteLine("!!! FATAL ERROR: webView22.CoreWebView2 is NULL after EnsureCoreWebView2Async!");
                    if (OperatingSystem.IsWindowsVersionAtLeast(6, 1)) Application.Exit();
                    return;
                }

                _webViewService = new WebViewService(webView22.CoreWebView2);
                _gitService = new GitService(_webViewService.PostJsonToWeb);
                _processManagerService = new ProcessManagerService(_webViewService.PostJsonToWeb, Config);
                webView22.CoreWebView2.WebMessageReceived += WebView22_WebMessageReceived;

                // Abrir DevTools apenas em modo Debug
#if DEBUG
                webView22.CoreWebView2.OpenDevToolsWindow();
#endif

                bool isDevelopment = false;
                if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
                {
                    string webContentPath = Path.Combine(Application.StartupPath, "WebContent", "dist");
                    isDevelopment = Directory.Exists(webContentPath);
                }

                if (isDevelopment)
                {
                    // --- MODO DESENVOLVIMENTO ---
                    string webContentPath = Path.Combine(Application.StartupPath, "WebContent", "dist");
                    webView22.CoreWebView2.SetVirtualHostNameToFolderMapping(
                        "apirunner.local", webContentPath, CoreWebView2HostResourceAccessKind.Allow
                    );
                    webView22.Source = new Uri("http://apirunner.local/index.html");
                }
                else
                {
                    // --- MODO PRODUÇÃO (Recursos Embutidos) ---
                    webView22.CoreWebView2.AddWebResourceRequestedFilter("http://apirunner.local/*", CoreWebView2WebResourceContext.All);
                    webView22.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
                    webView22.CoreWebView2.Navigate("http://apirunner.local/index.html");
                }

                _webViewService.PostJsonToWeb(new { type = (int)WebMessageType.InitialCompactInterface, compactInterface = Config.CompactInterface });

                await Task.Run(async () =>
                {
                    if (_gitService != null)
                    {
                        await _gitService.LoadGitInfos(Config);
                        _gitService.CheckNewVersion();
                    }
                });

                _webViewService.SendConfigDataToWeb(Config, true);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"!!! EXCEPTION in Form1_Load: {ex.ToString()}");
                if (OperatingSystem.IsWindowsVersionAtLeast(6, 1)) Application.Exit();
            }
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            foreach (var app in Config.Apps)
            {
                if (app.Process != null && !app.Process.HasExited)
                {
                    _processManagerService?.StopAllProcesses();
                }
                if (app.Process != null)
                {
                    CMD.StopProcess(app.Process);
                }

                if (app.VsProcess != null && !app.VsProcess.HasExited)
                {
                    try
                    {
                        app.VsProcess.Exited -= null;
                    }
                    catch { }
                }
            }
        }
        #endregion

        #region Manipulação de Mensagens do WebView

        private void CoreWebView2_WebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            string uri = e.Request.Uri;

            if (!uri.StartsWith("http://apirunner.local/"))
            {
                return;
            }

            try
            {
                string relativePath = uri.Substring("http://apirunner.local/".Length);
                if (string.IsNullOrEmpty(relativePath) || relativePath == "/")
                {
                    relativePath = "index.html";
                }

                int queryIndex = relativePath.IndexOf('?');
                if (queryIndex >= 0) relativePath = relativePath.Substring(0, queryIndex);

                string resourcePath = relativePath.Replace('/', '.');
                string resourceName = $"Executor_de_Projetos.WebContent.dist.{resourcePath}";

                var assembly = Assembly.GetExecutingAssembly();

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var ms = new MemoryStream();
                        stream.CopyTo(ms);
                        ms.Position = 0;

                        string mimeType = GetMimeType(relativePath);

                        if (webView22?.CoreWebView2?.Environment != null)
                        {
                            string contentTypeHeader = $"Content-Type: {mimeType}";
                            if (mimeType.StartsWith("text/") || mimeType == "application/javascript")
                            {
                                contentTypeHeader += "; charset=utf-8";
                            }

                            var response = webView22.CoreWebView2.Environment.CreateWebResourceResponse(
                                ms,
                                200,
                                "OK",
                                $"{contentTypeHeader}\nAccess-Control-Allow-Origin: *"
                            );
                            e.Response = response;
                        }
                    }
                    else
                    {
                        Trace.WriteLine($"!!! CoreWebView2_WebResourceRequested - Resource NOT FOUND: {resourceName}");

                        if (webView22?.CoreWebView2?.Environment != null)
                        {
                            e.Response = webView22.CoreWebView2.Environment.CreateWebResourceResponse(
                                null,
                                404,
                                "Not Found",
                                "Content-Type: text/plain"
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"!!! EXCEPTION in CoreWebView2_WebResourceRequested for URI {uri}: {ex.ToString()}");
                if (webView22?.CoreWebView2?.Environment != null)
                {
                    try
                    {
                        e.Response = webView22.CoreWebView2.Environment.CreateWebResourceResponse(null, 500, "Internal Server Error", "Content-Type: text/plain");
                    }
                    catch { }
                }
            }
        }

        private string GetMimeType(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            return ext switch
            {
                ".html" => "text/html",
                ".js" => "text/javascript",
                ".css" => "text/css",
                ".json" => "application/json",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".woff" => "font/woff",
                ".woff2" => "font/woff2",
                ".ttf" => "font/ttf",
                _ => "application/octet-stream",
            };
        }

        private async void WebView22_WebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = JsonSerializer.Deserialize<WebMessage>(e.WebMessageAsJson, options);

                if (message == null || !ValidateMessage(message, out WebMessageAction action))
                    return;


                switch (action)
                {
                    case WebMessageAction.LoadData:
                        await HandleLoadData();
                        break;
                    case WebMessageAction.UpdateEmail:
                        await HandleUpdateEmail(message.Email);
                        break;
                    case WebMessageAction.UpdateCompactInterface:
                        await HandleUpdateCompactInterface(message.Enabled);
                        break;
                    case WebMessageAction.UpdateConfig:
                        await HandleUpdateConfig(message.App);
                        break;
                    case WebMessageAction.CreateEnvironment:
                        await HandleCreateEnvironment(message.App);
                        break;
                    case WebMessageAction.PullGit:
                        await HandlePullGit(message.Index);
                        break;
                    case WebMessageAction.OpenVS:
                        await HandleOpenVS(message.Index);
                        break;
                    case WebMessageAction.ToggleEnv:
                        await HandleToggleEnv(message.Index, message.Env);
                        break;
                    case WebMessageAction.SelectDirectory:
                        await HandleSelectDirectory(message.Index);
                        break;
                    case WebMessageAction.ReleaseIP:
                        await HandleReleaseIP(true);
                        break;
                    case WebMessageAction.UpdateVersion:
                        await HandleUpdateVersion();
                        break;
                    case WebMessageAction.Reload:
                        await HandleReload();
                        break;
                    case WebMessageAction.CloseApplication:
                        await HandleCloseApplication();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao receber mensagem no WebView: {ex.Message}");
            }
        }

        private bool ValidateMessage(WebMessage? message, out WebMessageAction action)
        {
            action = default;
            if (message == null) return false;
            if (!Enum.IsDefined(typeof(WebMessageAction), message.Action)) return false;
            action = message.Action;
            return true;
        }

        private async Task HandleLoadData()
        {
            _webViewService?.SendConfigDataToWeb(Config);
            await Task.CompletedTask;
        }

        private async Task HandleUpdateEmail(string? email)
        {
            if (string.IsNullOrEmpty(email)) return;

            _configManagerService?.UpdateEmail(email);
            Config.Email = email;

            await Task.CompletedTask;
        }

        private async Task HandleUpdateCompactInterface(bool isCompactEnabled)
        {
            _configManagerService?.UpdateCompactInterface(isCompactEnabled);
            Config.CompactInterface = isCompactEnabled;
            UpdateFormSize(isCompactEnabled, animate: true);

            await Task.CompletedTask;
        }

        private async Task HandleUpdateConfig(JsonNode? appNode)
        {
            if (_configManagerService == null || _gitService == null || _webViewService == null)
                throw new InvalidOperationException();

            var updater = new UpdateService(_configManagerService, _gitService, _webViewService, Config);
            await updater.UpdateAppAsync(appNode, HandleReleaseIP);
        }

        private Task HandleCreateEnvironment(JsonNode? appNode)
        {
            if (_configManagerService == null || _gitService == null || _webViewService == null)
                throw new InvalidOperationException();

            var updater = new UpdateService(_configManagerService, _gitService, _webViewService, Config);
            updater.CreateEnvironment(appNode);
            return Task.CompletedTask;
        }

        private async Task HandlePullGit(int index)
        {
            if (_gitService != null)
                await _gitService.Pull(index, Config);
        }

        private async Task HandleOpenVS(int index)
        {
            if (_processManagerService != null)
                await _processManagerService.OpenVisualStudio(index, Config);
        }

        private async Task HandleToggleEnv(int index, string? envValue)
        {
            if (string.IsNullOrEmpty(envValue) || !Enum.TryParse<EnvironmentEnum>(envValue, true, out var env))
            {
                Debug.WriteLine($"Erro: Ambiente inválido: {envValue}");
                return;
            }

            _processManagerService?.ButtonAction(index, env);
            await Task.CompletedTask;
        }

        private Task HandleSelectDirectory(int index)
        {
            if (index < 0 || index >= Config.Apps.Count)
                return Task.CompletedTask;

            if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                _webViewService?.PostJsonToWeb(new
                {
                    type = (int)WebMessageType.DirectoryActionCompleted,
                    index,
                    canceled = true,
                    error = "Seleção de diretório não é suportada nesta plataforma."
                });
                return Task.CompletedTask;
            }

            using var folderDialog = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(Config.Apps[index].Path) && Directory.Exists(Config.Apps[index].Path))
            {
                folderDialog.SelectedPath = Config.Apps[index].Path;
            }

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                Config.Apps[index].Path = folderDialog.SelectedPath;
                _configManagerService?.SaveConfig(Config);
                _gitService?.LoadGitCommit(index, Config);
                _webViewService?.PostJsonToWeb(new { type = (int)WebMessageType.DirectoryActionCompleted, index });
                _webViewService?.SendConfigDataToWeb(Config);
            }
            else
            {
                _webViewService?.PostJsonToWeb(new { type = (int)WebMessageType.DirectoryActionCompleted, index, canceled = true });
            }

            return Task.CompletedTask;
        }
        #endregion

        #region Gerenciamento de IP
        private async Task HandleReleaseIP(bool tryRelease = false)
        {
            var connectionString = Config.Apps
                .SelectMany(a => a.Stage)
                .FirstOrDefault(s => s.Key.Contains("ConnectionString"));

            IpStatus status = IpStatus.IpBlocked;
            bool enabled = true;

            if (string.IsNullOrEmpty(connectionString.Key))
            {
                SendIpStatusToWeb(status, enabled);
                return;
            }

            SendIpStatusToWeb(IpStatus.CheckingIp, false);

            if (Database.VerifyConnection(connectionString.Value))
            {
                status = IpStatus.IpReleased;
                enabled = false;
            }
            else if (tryRelease)
            {
                SendIpStatusToWeb(IpStatus.ReleasingIp, false);

                if (!string.IsNullOrEmpty(Config.Email))
                {
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                        var updateIpTask = OAuth2.UpdateIp(Config.Email);

                        var completedTask = await Task.WhenAny(updateIpTask, Task.Delay(10000, cts.Token));

                        bool success = false;
                        if (completedTask == updateIpTask)
                        {
                            success = await updateIpTask;
                        }

                        status = success ? IpStatus.IpReleased : IpStatus.IpBlocked;
                        enabled = !success;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Erro ao liberar IP: {ex.Message}");
                        status = IpStatus.IpBlocked;
                        enabled = true;
                    }
                }
            }

            SendIpStatusToWeb(status, enabled);
        }

        private void SendIpStatusToWeb(IpStatus status, bool enabled)
        {
            var response = new
            {
                type = (int)WebMessageType.IpStatus,
                status = (int)status,
                enabled
            };

            _webViewService?.PostJsonToWeb(response);
        }
        #endregion

        #region Atualização e Recarga
        private async Task HandleUpdateVersion()
        {
            var path = Directory.GetCurrentDirectory();
            File.Delete("update_version_run.bat");
            File.Move("update_version.bat", "update_version_run.bat");
            File.Move("config.json", "config-update.json");
            await Task.Delay(500);
            CMD.RunBatFile(path, "update_version_run.bat");
            if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                Close();
            }
        }

        private async Task HandleReload()
        {
            _webViewService?.PostJsonToWeb(new { type = (int)WebMessageType.ReloadStarted });


            await Task.Run(async () =>
            {
                if (_gitService != null)
                {
                    await HandleReleaseIP(true);
                    await _gitService.LoadGitInfos(Config);
                    _gitService.CheckNewVersion();
                }
            });
            _webViewService?.SendConfigDataToWeb(Config);
        }

        private async Task HandleCloseApplication()
        {
            _processManagerService?.StopAllProcesses();

            await Task.Delay(200);

            if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                Application.Exit();
            }

            await Task.CompletedTask;
        }
        #endregion
    }
}