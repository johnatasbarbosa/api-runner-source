using Newtonsoft.Json.Linq;

namespace APIRunner.Business;

public static class JsonFile
{
    public static void UpdateJson(string filePath, Dictionary<string, string> properties)
    {
        string json = File.ReadAllText(filePath);
        JObject jsonObject = null;
        JArray jsonArray = null;
        if (json[0] == '{')
            jsonObject = JObject.Parse(json);
        else
            jsonArray = JArray.Parse(json);

        foreach (var item in properties)
        {
            JToken encryptionKeyToken;
            if (jsonObject != null)
                encryptionKeyToken = jsonObject.SelectToken("$." + item.Key);
            else
                encryptionKeyToken = jsonArray.SelectToken("$." + item.Key);

            if (encryptionKeyToken != null)
            {
                encryptionKeyToken.Replace(Convert(item.Value));
            }
        }

        string modifiedJson;
        if (jsonObject != null)
            modifiedJson = jsonObject.ToString();
        else
            modifiedJson = jsonArray.ToString();

        File.WriteAllText(filePath, modifiedJson);
    }

    private static JToken Convert(string item)
    {
        if (item == "true")
            return true;
        if (item == "false")
            return false;

        return item.ToString();
    }
}
