using APIRunner.Models;
using APIRunner.Enums;

namespace APIRunner.Services
{
  public class ConfigFormatterService
  {
    public static object FormatConfigForWeb(Config config, bool isInitialLoad = false)
    {
      var apps = config.Apps.Select(app =>
      {
        bool CheckEnvironmentConfigured(Dictionary<string, string> envParams)
        {
          if (envParams == null) return false;
          if (envParams.TryGetValue("ConnectionStrings.DefaultConnection", out var defaultConn) && !string.IsNullOrWhiteSpace(defaultConn))
          {
            return true;
          }
          return envParams.Any(kvp =>
                          !kvp.Key.Equals("ConnectionStrings.DefaultConnection", StringComparison.OrdinalIgnoreCase) &&
                          kvp.Key.IndexOf("ConnectionString", StringComparison.OrdinalIgnoreCase) >= 0 &&
                          !string.IsNullOrWhiteSpace(kvp.Value)
                      );
        }

        return new
        {
          name = app.Name,
          path = app.Path,
          environment = app.Environment?.ToString(),
          currentBranch = app.CurrentBranch,
          commits = app.Commits?.Count ?? 0,
          fileExists = app.FileExists,
          local = app.Local,
          stage = app.Stage,
          homolog = app.Homolog,
          prod = app.Prod,
          isLocalConfigured = CheckEnvironmentConfigured(app.Local),
          isStageConfigured = CheckEnvironmentConfigured(app.Stage),
          isHomologConfigured = CheckEnvironmentConfigured(app.Homolog),
          isProdConfigured = CheckEnvironmentConfigured(app.Prod),
          isGitUpdating = app.IsGitUpdating
        };
      });

      return new
      {
        type = (int)WebMessageType.DataLoaded,
        version = "v.2.0.0",
        apps,
        email = config.Email,
        compactInterface = config.CompactInterface,
        isInitialLoad
      };
    }
  }
}