namespace APIRunner.Enums
{
  public enum WebMessageType
  {
    DataLoaded = 0,
    ReloadStarted = 1,
    EnvToggled = 2,
    IpStatus = 3,
    VersionStatus = 4,
    GitUpdateStateChanged = 5,
    GitPullError = 6,
    SystemLoading = 7,
    InitialCompactInterface = 8,
    DirectoryActionCompleted = 9,
    VsOpenActionCompleted = 10,
    VsProcessExited = 11,
    GitConnectionError = 12
  }
}