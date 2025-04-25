using System.Diagnostics; // For Trace, Debug, TextWriterTraceListener

namespace APIRunner
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ConfigureFileLogging();

            Trace.WriteLine("Logging configured. Initializing application...");

            try
            {
                ApplicationConfiguration.Initialize();

                Trace.WriteLine("Running main form (Form1)...");
                Application.Run(new Form1());

                Trace.WriteLine("Application exited normally.");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"FATAL UNHANDLED EXCEPTION: {ex}");
                MessageBox.Show($"Ocorreu um erro fatal e nao tratado:\n\n{ex.Message}\n\nConsulte o arquivo de log para detalhes.", "Erro Fatal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Trace.Flush();
            }
        }

        /// <summary>
        /// Configura o sistema System.Diagnostics.Trace para escrever logs em um arquivo de texto.
        /// </summary>
        private static void ConfigureFileLogging()
        {
            try
            {
                string logDirectory = AppContext.BaseDirectory;
                string logFileName = "APIRunner_DebugLog.txt";
                string logFilePath = Path.Combine(logDirectory, logFileName);

                var logFileStream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);

                var fileListener = new TextWriterTraceListener(logFileStream, "fileLogListener");

                Trace.Listeners.Add(fileListener);
                Trace.AutoFlush = true;

                Trace.WriteLine("==================================================");
                Trace.WriteLine($"APIRunner Log Session Started: {DateTime.Now}");
                Trace.WriteLine($"Log File: {logFilePath}");
                Trace.WriteLine($"OS Version: {Environment.OSVersion}");
                Trace.WriteLine($"User: {Environment.UserName}");
                Trace.WriteLine($".NET Runtime: {Environment.Version}");
                Trace.WriteLine("==================================================");

                Debug.WriteLine("Debug logging initialized and directed to file.");
                Trace.WriteLine("Trace logging initialized and directed to file.");

            }
            catch (IOException ioEx)
            {
                MessageBox.Show($"Erro de I/O ao configurar o log em '{Path.Combine(AppContext.BaseDirectory, "APIRunner_DebugLog.txt")}':\n{ioEx.Message}\n\nO log em arquivo pode nao funcionar.",
                                "Erro de Log", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro inesperado ao configurar o log em arquivo:\n{ex.Message}\n\nO log em arquivo pode nao funcionar.",
                                "Erro de Log", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}