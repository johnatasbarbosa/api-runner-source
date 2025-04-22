using System.Text.Json;
using System.Net.Http.Headers;
using System.Text;

namespace APIRunner.Business;

public static class OAuth2
{
    public static async Task<bool> UpdateIp(string email)
    {
        var token = await GetToken();
        var ip = await GetIp();

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var jsonContent = JsonSerializer.Serialize(new UpdateIPDto(ip, email));
        var contentString = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var result = await client.PostAsync("https://x3mjn2u9j5.execute-api.us-east-1.amazonaws.com/integration-hub-stage/v1/sg", contentString);

        return result.StatusCode == System.Net.HttpStatusCode.OK;
    }

    private static async Task<string> GetToken()
    {
        var parameters = new Dictionary<string, string>
        {
           { "address", "https://integration-hub.auth.us-east-1.amazoncognito.com/oauth2/token" },
           { "client_id", "6vpgqv3e9tub2qllentlnju2rv" },
           { "client_secret", "1ttt1je9bqvebkilqkfcncu44ojig5rfpv7l9sdc8v57gebuhppc" },
           { "grant_type", "client_credentials" },
           { "scope", "about/about addresses/addresses banks/banks companies/companies emails/emails health/health logs/logs people/people files/files" }
        };

        var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://integration-hub.auth.us-east-1.amazoncognito.com/oauth2/token");
        request.Headers.Add("Accept", "application/json");
        request.Content = new FormUrlEncodedContent(parameters);
        var tokenResponse = await client.SendAsync(request);
        var tokenStr = await tokenResponse.Content.ReadAsStringAsync();
        var tokenResult = JsonSerializer.Deserialize<TokenDto>(tokenStr);

        if (tokenResult == null || string.IsNullOrEmpty(tokenResult.access_token))
        {
            throw new InvalidOperationException("Failed to retrieve a valid access token.");
        }
        return tokenResult.access_token;
    }

    private static async Task<string> GetIp()
    {
        var client = new HttpClient();
        var result = await client.GetAsync("https://api.ipify.org");
        return await result.Content.ReadAsStringAsync();
    }
}

public class TokenDto
{
    public string? access_token { get; set; }
    public int expires_in { get; set; }
    public string? token_type { get; set; }
    public string? error { get; set; }
}

public class UpdateIPDto
{
    public UpdateIPDto(string ip, string email)
    {
        this.ip = ip;
        this.email = email;
    }
    public string ip { get; set; }
    public string email { get; set; }
}
