using System.Diagnostics;
using System.Text.Json;
using APIRunner.Models;
using Microsoft.Web.WebView2.Core;

namespace APIRunner.Services
{
  public class WebViewService
  {
    private readonly CoreWebView2 _webView;

    public WebViewService(CoreWebView2 webView)
    {
      _webView = webView ?? throw new ArgumentNullException(nameof(webView));
    }

    public void PostJsonToWeb(object payload)
    {
      try
      {
        var json = JsonSerializer.Serialize(payload);
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1) && Application.OpenForms.Count > 0 && Application.OpenForms[0]?.InvokeRequired == true)
        {
          Application.OpenForms[0]?.Invoke(new Action(() =>
          {
            _webView.PostWebMessageAsJson(json);
          }));
        }
        else
        {
          _webView.PostWebMessageAsJson(json);
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"Erro ao enviar mensagem para o WebView: {ex.Message}");
        throw;
      }
    }

    public void SendConfigDataToWeb(Config config, bool isInitialLoad = false)
    {
      var payload = ConfigFormatterService.FormatConfigForWeb(config, isInitialLoad);
      PostJsonToWeb(payload);
    }
  }
}