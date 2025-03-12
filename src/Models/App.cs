using System.Diagnostics;
using System.Text.Json.Serialization;

namespace APIRunner.Models;

public class App
{
    public int Id { get; set; }
    public string Name { get; set; }
    public EnvironmentEnum? Environment { get; set; }
    public string EnvironmentText
    {
        get
        {
            if (Environment == EnvironmentEnum.Local) return "Local";
            if (Environment == EnvironmentEnum.Stage) return "Stage";
            if (Environment == EnvironmentEnum.Homolog) return "Homolog";
            if (Environment == EnvironmentEnum.Prod) return "Prod";
            var a = "".Split('\n');
            return "Pronto";
        }
    }
    public string Path { get; set; }
    public bool FileExists
    {
        get
        {
            var file = Path + "//appsettings.Development.json";
            return !string.IsNullOrEmpty(file) && File.Exists(file);
        }
    }
    public Dictionary<string, string> Local { get; set; }
    public Dictionary<string, string> Stage { get; set; }
    public Dictionary<string, string> Homolog { get; set; }
    public Dictionary<string, string> Prod { get; set; }

    public string CurrentBranch { get; set; }
    public List<string> Commits { get; set; }

    [JsonIgnore]
    public Process Process { get; set; }
}

