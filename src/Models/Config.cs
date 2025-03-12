namespace APIRunner.Models;

public class Config
{
    public Config()
    {
        Apps = new List<App>();
    }
    public string Email { get; set; }
    public string Repository { get; set; }
    public List<App> Apps { get; set; }
}
