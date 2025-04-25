namespace APIRunner.Models;

public class Config
{
    public Config()
    {
        Apps = new List<App>();
        Email = string.Empty;
        Repository = string.Empty;
    }
    public string Email { get; set; }
    public bool CompactInterface { get; set; } = true;
    public string Repository { get; set; }
    public List<App> Apps { get; set; }
}
