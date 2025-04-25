using System.Text.Json.Serialization;
using System.IO;
using System.Diagnostics;
using APIRunner.Enums;

public class App
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;

    // Transient properties (não salvas no config.json)
    [JsonIgnore]
    public Process? Process { get; set; }
    [JsonIgnore]
    public EnvironmentEnum? Environment { get; set; }
    [JsonIgnore]
    public Process? VsProcess { get; set; }
    [JsonIgnore]
    public bool FileExists => !string.IsNullOrEmpty(Path) && Directory.Exists(Path);

    [JsonIgnore]
    public string? CurrentBranch { get; set; }
    [JsonIgnore]
    public List<string> Commits { get; set; }

    [JsonIgnore]
    public bool IsGitUpdating { get; set; } = false;

    // Configurações de ambiente
    public Dictionary<string, string> Local { get; set; }
    public Dictionary<string, string> Stage { get; set; }
    public Dictionary<string, string> Homolog { get; set; }
    public Dictionary<string, string> Prod { get; set; }

    public App()
    {
        Local = new Dictionary<string, string>();
        Stage = new Dictionary<string, string>();
        Homolog = new Dictionary<string, string>();
        Prod = new Dictionary<string, string>();
        Commits = new List<string>();
    }
}