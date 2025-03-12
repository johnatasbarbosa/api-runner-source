using System.Diagnostics;
using System.Text;

namespace APIRunner.Business
{
    public static class Git
    {
        public static string GetCurrentGitBranch(string path)
        {
            return ExecuteGitCommand("rev-parse --abbrev-ref HEAD", path)?.Trim();
        }

        public static List<string> GetCommitsNotPulled(string path)
        {
            // First, fetch updates from the remote
            ExecuteGitCommand("fetch", path);

            // Then, get the list of commits that are in the upstream tracking branch but not in the current branch
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

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = path,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = processStartInfo;
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    output.AppendLine(line);
                }

                process.WaitForExit();
            }

            return output.ToString().Replace("\r", "");
        }
    }
}
