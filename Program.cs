namespace csharp;
using System.Diagnostics;

static class Program
{
    [STAThread]
    static void Main()
    {
        string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".savetool", "logfile.txt");
        Trace.Listeners.Add(new TextWriterTraceListener(logFilePath));
        Trace.AutoFlush = true;
        ApplicationConfiguration.Initialize();
        Trace.WriteLine($"[{DateTime.Now}]: Ouverture de l'application.");
        Application.Run(new Form1());
    }    
}