using System.Text.Json;
using System.Text.Json.Nodes;
using APIRunner.Enums;
using APIRunner.Models;
using APIRunner.Services;

namespace APIRunner.Business
{
  public class UpdateService
  {
    private readonly ConfigManagerService _configManagerService;
    private readonly GitService _gitService;
    private readonly WebViewService _webViewService;
    private readonly Config _config;

    public UpdateService(ConfigManagerService configManagerService, GitService gitService, WebViewService webViewService, Config config)
    {
      _configManagerService = configManagerService ?? throw new ArgumentNullException(nameof(configManagerService));
      _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));
      _webViewService = webViewService ?? throw new ArgumentNullException(nameof(webViewService));
      _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<bool> UpdateAppAsync(JsonNode? appNode, Func<bool, Task> releaseIPAsync)
    {
      if (appNode == null) return false;

      var oldName = appNode["oldName"]?.GetValue<string>();
      var app = _config.Apps.FirstOrDefault(a => a.Name == oldName);
      if (app == null || oldName == null) return false;

      int appIndex = _config.Apps.IndexOf(app);
      var originalPath = app.Path;
      var newName = appNode["name"]?.GetValue<string>() ?? app.Name;
      var newPath = appNode["path"]?.GetValue<string>() ?? app.Path;
      var environmentString = appNode["environment"]?.GetValue<string>();
      var paramsData = appNode["params"]?.Deserialize<Dictionary<string, string>>() ?? new Dictionary<string, string>();

      if (!Enum.TryParse<EnvironmentEnum>(environmentString, true, out var environment)) return false;

      bool needsIpCheck = environment != EnvironmentEnum.Local;

      app.Name = newName;
      app.Path = newPath;

      switch (environment)
      {
        case EnvironmentEnum.Local: app.Local = paramsData; break;
        case EnvironmentEnum.Stage: app.Stage = paramsData; break;
        case EnvironmentEnum.Homolog: app.Homolog = paramsData; break;
        case EnvironmentEnum.Prod: app.Prod = paramsData; break;
      }

      _configManagerService.SaveConfig(_config);

      if (appIndex != -1 && originalPath != null && app.Path != originalPath)
      {
        _gitService.LoadGitCommit(appIndex, _config);
      }
      if (needsIpCheck)
      {
        await releaseIPAsync(true);
      }
      _gitService.CheckNewVersion();
      _webViewService.SendConfigDataToWeb(_config);

      return true;
    }

    public bool CreateEnvironment(JsonNode? appNode)
    {
      if (appNode == null) return false;

      var appName = appNode["name"]?.GetValue<string>();
      var environmentString = appNode["environment"]?.GetValue<string>();
      if (string.IsNullOrEmpty(appName) || string.IsNullOrEmpty(environmentString)) return false;

      if (!Enum.TryParse<EnvironmentEnum>(environmentString, true, out var environment)) return false;

      var app = _config.Apps.FirstOrDefault(a => a.Name == appName);
      if (app == null) return false;

      var stageConfig = app.Stage ?? new Dictionary<string, string>();
      var newEnvironment = new Dictionary<string, string>();
      foreach (var param in stageConfig.Keys)
      {
        newEnvironment[param] = "";
      }

      bool environmentUpdated = false;
      switch (environment)
      {
        case EnvironmentEnum.Local: app.Local = newEnvironment; environmentUpdated = true; break;
        case EnvironmentEnum.Stage: app.Stage = newEnvironment; environmentUpdated = true; break;
        case EnvironmentEnum.Homolog: app.Homolog = newEnvironment; environmentUpdated = true; break;
        case EnvironmentEnum.Prod: app.Prod = newEnvironment; environmentUpdated = true; break;
      }

      if (environmentUpdated)
      {
        _configManagerService.SaveConfig(_config);
        _webViewService.SendConfigDataToWeb(_config);
      }

      return environmentUpdated;
    }
  }
}