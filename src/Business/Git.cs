using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace APIRunner.Business
{
    public static class Git
    {
        public static string GetCurrentGitBranch(string path)
        {
            return ExecuteGitCommand("rev-parse --abbrev-ref HEAD", path)?.Trim() ?? string.Empty;
        }

        public static List<string> GetCommitsNotPulled(string path)
        {
            ExecuteGitCommand("fetch", path);

            var commits = ExecuteGitCommand("log ..@{u} --oneline", path);
            return commits.Split('\n').Where(c => c.Length > 0).ToList();
        }


        public static string Pull(string path)
        {
            return ExecuteGitCommand("pull", path);
        }

        public static string Clone(string path, string repository)
        {
            return ExecuteGitCommand("clone " + repository, path);
        }

        static string ExecuteGitCommand(string arguments, string path)
        {
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = path,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = processStartInfo;
                process.Start();

                var outputTask = Task.Run(() =>
                {
                    while (!process.StandardOutput.EndOfStream)
                    {
                        string? line = process.StandardOutput.ReadLine();
                        if (line != null)
                        {
                            output.AppendLine(line);
                        }
                    }
                });

                var errorTask = Task.Run(() =>
                {
                    while (!process.StandardError.EndOfStream)
                    {
                        string line = process.StandardError.ReadLine() ?? string.Empty;
                        error.AppendLine(line);
                    }
                });

                Task.WaitAll(outputTask, errorTask);
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Git command failed with exit code {process.ExitCode}. Error: {error.ToString().Trim()}");
                }
            }

            return output.ToString().Replace("\r", "");
        }
    }
}
