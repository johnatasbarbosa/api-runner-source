using System.Diagnostics;
using System.IO;

namespace APIRunner.Business;

public static class CMD
{
    public static Process RunDotnet(Process process, string path)
    {
        StopProcess(process);
        process = StartProcess(process, path, "dotnet", "watch run");

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

    public static void OpenVisualStudio(string filePath)
    {
        new Process
        {
            StartInfo = new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            }
        }.Start();
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
            CreateNoWindow = false
        };

        process = new Process { StartInfo = processStartInfo };
        process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        process.ErrorDataReceived += (sender, args) => Console.WriteLine("ERROR: " + args.Data);

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
            //process.WaitForExit();
            Console.WriteLine("Process stopped.");
        }
    }
}
