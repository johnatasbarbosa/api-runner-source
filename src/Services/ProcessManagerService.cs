using APIRunner.Models;
using APIRunner.Business;
using System.Diagnostics;
using APIRunner.Enums;

namespace APIRunner.Services
{
    public class ProcessManagerService
    {
        private readonly Action<object> _postJsonToWeb;
        private readonly Config _config;

        public ProcessManagerService(Action<object> postJsonToWeb, Config config)
        {
            _postJsonToWeb = postJsonToWeb ?? throw new ArgumentNullException(nameof(postJsonToWeb));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task OpenVisualStudio(int index, Config config)
        {
            if (index < 0 || index >= config.Apps.Count) return;
            var app = config.Apps[index];

            if (app.VsProcess != null && !app.VsProcess.HasExited)
            {
                try
                {
                    IntPtr handle = app.VsProcess.MainWindowHandle;
                    if (NativeMethods.IsIconic(handle))
                    {
                        NativeMethods.ShowWindow(handle, NativeMethods.SW_RESTORE);
                    }
                    NativeMethods.SetForegroundWindow(handle);
                    _postJsonToWeb(new
                    {
                        type = (int)WebMessageType.VsOpenActionCompleted,
                        index = index,
                        alreadyRunning = true
                    });
                    return;
                }
                catch
                {
                    app.VsProcess = null;
                }
            }

            _postJsonToWeb(new { type = (int)WebMessageType.SystemLoading, index = index, isLoading = true });

            var path = app.Path;

            try
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var solutionFile = Directory.GetFiles(path).FirstOrDefault(f => f.EndsWith(".sln"));
                        if (solutionFile != null)
                        {
                            app.VsProcess = CMD.OpenVisualStudio(solutionFile);
                            if (app.VsProcess != null)
                            {
                                app.VsProcess.EnableRaisingEvents = true;
                                app.VsProcess.Exited += (sender, args) =>
                        {
                            app.VsProcess = null;
                            _postJsonToWeb(new { type = (int)WebMessageType.VsProcessExited, index = index });
                        };
                                return;
                            }
                        }
                        var parentDirectory = Directory.GetParent(path);
                        if (parentDirectory == null)
                        {
                            throw new InvalidOperationException("Parent directory not found.");
                        }
                        path = parentDirectory.FullName;
                    }
                });
            }
            finally
            {
                _postJsonToWeb(new { type = (int)WebMessageType.VsOpenActionCompleted, index = index, hasVsProcess = app.VsProcess != null });
            }
        }

        public void ButtonAction(int rowIndex, EnvironmentEnum environment)
        {
            var app = _config.Apps[rowIndex];
            var props = GetEnvProps(app, environment);

            if (props == null) return;

            if (app.Environment != null && app.Environment != environment)
            {
                if (app.Process != null)
                {
                    app.Process.Exited -= Process_Exited;
                    CMD.StopProcess(app.Process);
                }
                app.Environment = null;
                app.Process = null;
            }

            if (app.Process != null && !app.Process.HasExited)
            {
                app.Process.Exited -= Process_Exited;
                CMD.StopProcess(app.Process);
            }

            if (app.Environment == environment)
            {
                app.Environment = null;
                app.Process = null;
                UseUserSecrets(app.Path, true);
            }
            else
            {
                app.Environment = environment;
                UseUserSecrets(app.Path, false);
                AllowSeed(app.Path, environment == EnvironmentEnum.Local);
                JsonFile.UpdateJson(app.Path + "//appsettings.Development.json", props);
                app.Process = CMD.RunDotnet(app.Process, app.Path);
                if (app.Process != null)
                {
                    app.Process.EnableRaisingEvents = true;
                    app.Process.Exited += Process_Exited;
                }
            }

            _postJsonToWeb(new
            {
                type = (int)WebMessageType.EnvToggled,
                index = rowIndex,
                env = environment.ToString(),
                isRunning = app.Environment == environment
            });
        }

        private Dictionary<string, string> GetEnvProps(App app, EnvironmentEnum env)
        {
            return env switch
            {
                EnvironmentEnum.Local => app.Local,
                EnvironmentEnum.Stage => app.Stage,
                EnvironmentEnum.Homolog => app.Homolog,
                EnvironmentEnum.Prod => app.Prod,
                _ => new Dictionary<string, string>(),
            };
        }

        private void AllowSeed(string path, bool allowSeed)
        {
            path = path + "//Startup.cs";
            var seedCommand = "services.AddHostedService<SeedConfig>();";

            if (!File.Exists(path)) return;

            var lines = File.ReadAllLines(path).ToList();
            var lineIndex = lines.FindIndex(l => l.Contains(seedCommand));
            if (lineIndex < 0) return;

            var line = lines[lineIndex];
            var isCommented = line.TrimStart().StartsWith("//");

            if (allowSeed && isCommented)
                lines[lineIndex] = line.Replace("//", "");
            else if (!allowSeed && !isCommented)
                lines[lineIndex] = "//" + line;

            File.WriteAllLines(path, lines);
        }

        private void UseUserSecrets(string path, bool useUserSecrets)
        {
            path = path + "//Properties//AssemblyInfo.cs";
            var seedCommand = "UserSecretsId";

            if (!File.Exists(path)) return;

            var lines = File.ReadAllLines(path).ToList();
            var lineIndex = lines.FindIndex(l => l.Contains(seedCommand));
            if (lineIndex < 0) return;

            var line = lines[lineIndex];
            var isCommented = line.TrimStart().StartsWith("//");

            if (isCommented && useUserSecrets)
            {
                lines[lineIndex] = line.Replace("//", "");
                File.WriteAllLines(path, lines);
            }
            else if (!isCommented && !useUserSecrets)
            {
                lines[lineIndex] = "//" + line;
                File.WriteAllLines(path, lines);
            }
        }

        private void Process_Exited(object? sender, EventArgs e)
        {
            if (sender is not Process process || process == null) return;

            var app = _config.Apps.FirstOrDefault(a => a.Process == process);
            if (app != null)
            {
                Console.WriteLine($"Processo externo para {app.Name} ({app.Environment}) encerrado.");
                var exitedEnv = app.Environment;
                app.Environment = null;
                app.Process = null;
                process.Exited -= Process_Exited;
                if (exitedEnv.HasValue)
                {
                    _postJsonToWeb(new
                    {
                        type = (int)WebMessageType.EnvToggled,
                        index = _config.Apps.IndexOf(app),
                        env = exitedEnv.Value.ToString(),
                        isRunning = false
                    });
                }
            }
        }

        public void StopAllProcesses()
        {
            foreach (var app in _config.Apps)
            {
                if (app.Process != null && !app.Process.HasExited)
                {
                    app.Process.Exited -= Process_Exited;
                    CMD.StopProcess(app.Process);
                    app.Process = null;
                }
                if (app.VsProcess != null && !app.VsProcess.HasExited)
                {
                    try
                    {
                        app.VsProcess.Exited -= null;
                        CMD.StopProcess(app.VsProcess);
                        app.VsProcess = null;
                    }
                    catch { }
                }
            }
        }
    }
}