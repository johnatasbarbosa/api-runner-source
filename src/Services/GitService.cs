using APIRunner.Business;
using APIRunner.Enums;
using APIRunner.Models;
using System.Diagnostics;

namespace APIRunner.Services
{
  public class GitService
  {
    // Delegate para enviar mensagens para o WebView
    private readonly Action<object> _postJsonToWeb;

    public GitService(Action<object> postJsonToWeb)
    {
      _postJsonToWeb = postJsonToWeb ?? throw new ArgumentNullException(nameof(postJsonToWeb));
    }

    public async Task Pull(int index, Config config)
    {
      if (index < 0 || index >= config.Apps.Count)
      {
        return;
      }

      var app = config.Apps[index];
      if (!app.FileExists)
      {
        Debug.WriteLine($"App {index} ({app.Name}): Diretório não existe.");
        return;
      }

      app.IsGitUpdating = true;
      _postJsonToWeb(new { type = (int)WebMessageType.GitUpdateStateChanged, index, isUpdating = true });

      try
      {
        await Task.Run(() =>
        {
          string pullOutput = Git.Pull(app.Path);

          LoadGitCommit(index, config);
        });
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"Erro durante Git pull para App {index} ({app.Name}): {ex.Message}");
        app.IsGitUpdating = false;
        _postJsonToWeb(new
        {
          type = (int)WebMessageType.GitPullError,
          index,
          message = ex.Message,
          isUpdating = false,
          branch = app.CurrentBranch,
          commits = app.Commits?.Count ?? 0
        });
      }
      finally
      {
        app.IsGitUpdating = false;
        _postJsonToWeb(new
        {
          type = (int)WebMessageType.GitUpdateStateChanged,
          index,
          isUpdating = false,
          branch = app.CurrentBranch,
          commits = app.Commits?.Count ?? 0
        });
      }
    }

    public void LoadGitCommit(int index, Config config)
    {
      if (index < 0 || index >= config.Apps.Count)
      {
        return;
      }

      var app = config.Apps[index];

      if (!app.FileExists)
      {
        app.CurrentBranch = null;
        app.Commits = new List<string>();
        app.IsGitUpdating = false;
        _postJsonToWeb(new
        {
          type = (int)WebMessageType.GitUpdateStateChanged,
          index,
          isUpdating = false,
          branch = (string?)null,
          commits = 0
        });
        Debug.WriteLine($"App {index} ({app.Name}): Path não encontrado, pulando Git.");
        return;
      }

      try
      {
        app.IsGitUpdating = true;
        _postJsonToWeb(new { type = (int)WebMessageType.GitUpdateStateChanged, index, isUpdating = true });

        app.Commits = Git.GetCommitsNotPulled(app.Path);
        app.CurrentBranch = Git.GetCurrentGitBranch(app.Path);
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"Erro ao carregar commit Git para App {index} ({app.Name}): {ex.Message}");
        app.CurrentBranch = "--";
        app.Commits = new List<string>();
      }
      finally
      {
        app.IsGitUpdating = false;
        _postJsonToWeb(new
        {
          type = (int)WebMessageType.GitUpdateStateChanged,
          index,
          isUpdating = false,
          branch = app.CurrentBranch,
          commits = app.Commits?.Count ?? 0
        });
      }
    }

    public void CheckNewVersion()
    {
      bool enabled = false;

      try
      {
        if (Directory.Exists(Path.Combine("", ".git")))
        {
          var commits = Git.GetCommitsNotPulled("");
          enabled = commits.Any();
        }

        _postJsonToWeb(new
        {
          type = (int)WebMessageType.VersionStatus,
          updateAvailable = enabled
        });
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"Erro ao verificar nova versão: {ex.Message}");
        _postJsonToWeb(new
        {
          type = (int)WebMessageType.GitConnectionError,
          message = ex.Message
        });
        _postJsonToWeb(new
        {
          type = (int)WebMessageType.VersionStatus,
          updateAvailable = false,
          hadError = true
        });
      }
    }

    public async Task LoadGitInfos(Config config)
    {
      try
      {
        var tasks = new List<Task>();
        for (int i = 0; i < config.Apps.Count; i++)
        {
          int index = i;
          tasks.Add(Task.Run(async () =>
          {
            try
            {
              LoadGitCommit(index, config);
              await Task.Delay(100);
            }
            catch (Exception ex)
            {
              Debug.WriteLine($"Erro ao carregar informações Git para app {index}: {ex.Message}");
              _postJsonToWeb(new
              {
                type = (int)WebMessageType.GitConnectionError,
                message = ex.Message,
                appIndex = index
              });
            }
          }));
        }
        await Task.WhenAll(tasks);
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"Erro geral ao carregar informações Git: {ex.Message}");
        _postJsonToWeb(new
        {
          type = (int)WebMessageType.GitConnectionError,
          message = ex.Message
        });
      }
    }
  }
}