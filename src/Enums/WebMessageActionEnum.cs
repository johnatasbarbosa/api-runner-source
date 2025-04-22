using System.Text.Json.Serialization;

namespace APIRunner.Enums
{
  public enum WebMessageAction
  {
    LoadData = 0,
    UpdateEmail = 1,
    UpdateCompactInterface = 2,
    UpdateConfig = 3,
    CreateEnvironment = 4,
    PullGit = 5,
    OpenVS = 6,
    ToggleEnv = 7,
    SelectDirectory = 8,
    ReleaseIP = 9,
    UpdateVersion = 10,
    Reload = 11,
    CloseApplication = 12
  }
}