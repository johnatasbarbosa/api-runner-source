using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace APIRunner.Business;

public static class JsonFile
{
    public static void UpdateJson(string filePath, Dictionary<string, string> properties)
    {
        if (!File.Exists(filePath))
        {
            Debug.WriteLine($"Erro: Arquivo não encontrado em {filePath}");
            return;
        }

        try
        {
            // Lê o JSON original
            string json = File.ReadAllText(filePath);
            JObject? jsonObject = null;
            JArray? jsonArray = null;

            // Determina se o JSON é um objeto ou um array
            if (json.TrimStart().StartsWith("{"))
                jsonObject = JObject.Parse(json);
            else
                jsonArray = JArray.Parse(json);

            foreach (var item in properties)
            {
                try
                {
                    JToken? tokenToUpdate;

                    // Localiza o token a ser atualizado
                    if (jsonObject != null)
                        tokenToUpdate = jsonObject?.SelectToken("$." + item.Key);
                    else
                        tokenToUpdate = jsonArray?.SelectToken("$." + item.Key);

                    if (tokenToUpdate != null)
                    {
                        // Substitui o valor do token
                        tokenToUpdate.Replace(ConvertValue(item.Value));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erro ao processar a chave '{item.Key}' para o arquivo {filePath}: {ex.Message}");
                }
            }

            // Salva o JSON modificado
            string modifiedJson = jsonObject?.ToString() ?? jsonArray?.ToString() ?? string.Empty;
            File.WriteAllText(filePath, modifiedJson);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao processar o arquivo {filePath}: {ex.Message}");
        }
    }

    private static JToken ConvertValue(string value)
    {
        if (bool.TryParse(value, out bool boolValue))
            return boolValue;

        if (int.TryParse(value, out int intValue))
            return intValue;

        if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double doubleValue))
            return doubleValue;

        return value; // Retorna como string por padrão
    }
}