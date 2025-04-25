using System.Diagnostics;
using System.IO;

namespace APIRunner.Business;

public static class CMD
{
    public static Process RunDotnet(Process? process, string path)
    {
        if (process != null && !process.HasExited)
        {
            StopProcess(process);
        }

        process = StartProcess(new Process(), path, "dotnet", "watch run");
        return process;
    }

    public static void RunUpdateDatabase(string path)
    {
        new Process
        {
            StartInfo = new ProcessStartInfo(path)
            {
                FileName = "dotnet",
                Arguments = "ef database update",
                WorkingDirectory = path,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        }.Start();
    }

    public static Process OpenVisualStudio(string solutionFile)
    {
        // Cria um novo ProcessStartInfo
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = solutionFile,
            UseShellExecute = true
        };

        // Inicia o processo e retorna a referência
        var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start the process.");
        }
        return process;
    }

    public static void RunBatFile(string path, string file)
    {
        new Process
        {
            StartInfo = new ProcessStartInfo(path)
            {
                FileName = file,
                WorkingDirectory = path,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        }.Start();
    }

    static Process StartProcess(Process process, string path, string fileName, string arguments = "")
    {
        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = path,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Minimized
        };

        processStartInfo.FileName = fileName;
        processStartInfo.Arguments = arguments;

        process = new Process { StartInfo = processStartInfo };
        process.OutputDataReceived += (sender, args) => { };
        process.ErrorDataReceived += (sender, args) => { };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }

    public static void StopProcess(Process process)
    {
        if (process != null && !process.HasExited)
        {
            process.Kill();
        }
    }
}
