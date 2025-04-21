// Adicione este using no topo do arquivo
using System.Text.Json.Serialization;
using System.IO;
using System.Diagnostics; // Certifique-se que este using está presente para Directory.Exists

// Dentro da classe App:
public class App
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public EnvironmentEnum? Environment { get; set; }

    // Transient properties (não salvas no config.json)
    [JsonIgnore]
    public Process? Process { get; set; }

    [JsonIgnore]
    public Process? VsProcess { get; set; }

    // Modifique FileExists para usar Directory.Exists
    [JsonIgnore]
    public bool FileExists => !string.IsNullOrEmpty(Path) && Directory.Exists(Path);

    [JsonIgnore]
    public string? CurrentBranch { get; set; }
    [JsonIgnore]
    public List<string> Commits { get; set; }

    // <<< NOVA PROPRIEDADE >>>
    [JsonIgnore] // Não serializar este estado de runtime para config.json
    public bool IsGitUpdating { get; set; } = false; // Padrão para false

    // Configurações de ambiente
    public Dictionary<string, string> Local { get; set; }
    public Dictionary<string, string> Stage { get; set; }
    public Dictionary<string, string> Homolog { get; set; }
    public Dictionary<string, string> Prod { get; set; }

    // Construtor para inicializar dicionários e lista se necessário
    public App()
    {
        Local = new Dictionary<string, string>();
        Stage = new Dictionary<string, string>();
        Homolog = new Dictionary<string, string>();
        Prod = new Dictionary<string, string>();
        Commits = new List<string>(); // Inicializa lista
    }
}

// Definição do Enum (mantenha como está)
public enum EnvironmentEnum
{
    Local,
    Stage,
    Homolog,
    Prod

}