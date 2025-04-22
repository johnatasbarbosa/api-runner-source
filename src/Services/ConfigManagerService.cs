using System.Diagnostics;
using System.Text.Json;
using APIRunner.Models;

namespace APIRunner.Services
{
  public class ConfigManagerService
  {
    private readonly string _configFilePath;

    public ConfigManagerService(string configFilePath)
    {
      _configFilePath = configFilePath;
    }

    public Config LoadConfig()
    {
      try
      {
        if (!File.Exists(_configFilePath))
        {
          return CreateAndSaveDefaultConfig();
        }

        var configJson = File.ReadAllText(_configFilePath);
        if (string.IsNullOrWhiteSpace(configJson))
        {
          return CreateAndSaveDefaultConfig();
        }

        var deserializedConfig = JsonSerializer.Deserialize<Config>(configJson);
        if (deserializedConfig != null)
        {
          return deserializedConfig;
        }
        else
        {
          throw new InvalidOperationException("Falha ao deserializar o arquivo de configuração.");
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"Erro ao carregar configuração: {ex.Message}");
        return new Config();
      }
    }

    private Config CreateAndSaveDefaultConfig()
    {
      var defaultConfig = new Config
      {
        Email = string.Empty,
        Repository = "https://github.com/johnatasbarbosa/api-runner.git",
        Apps = new List<App>
            {
                new App
                {
                    Name = "App",
                    Path = string.Empty,
                    Local = new Dictionary<string, string>
                    {
                        { "ConnectionStrings.DefaultConnection", "" },
                        { "CryptBaseKey.EncryptionIV", "" },
                        { "CryptBaseKey.EncryptionKey", "" }
                    },
                    Stage = new Dictionary<string, string>
                    {
                        { "ConnectionStrings.DefaultConnection", "" },
                        { "CryptBaseKey.EncryptionIV", "" },
                        { "CryptBaseKey.EncryptionKey", "" }
                    }
                }
            }
      };

      SaveConfig(defaultConfig);
      return defaultConfig;
    }

    public void SaveConfig(Config config)
    {
      try
      {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string updatedJson = JsonSerializer.Serialize(config, options);
        File.WriteAllText(_configFilePath, updatedJson);
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"Erro ao salvar configuração: {ex.Message}");
      }
    }

    public void UpdateEmail(string email)
    {
      var config = LoadConfig();
      config.Email = email;
      SaveConfig(config);
    }

    public void UpdateCompactInterface(bool isCompactEnabled)
    {
      var config = LoadConfig();
      config.CompactInterface = isCompactEnabled;
      SaveConfig(config);
    }
  }
}