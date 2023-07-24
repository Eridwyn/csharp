namespace csharp;
using System.Diagnostics;

static class Program
{
    [STAThread]
    static void Main()
    {
        string logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".savetool");
        if (!Directory.Exists(logFolder))
        {
            Directory.CreateDirectory(logFolder);
        }

        string logFilePath = Path.Combine(logFolder, "logfile.txt");
        if (File.Exists(logFilePath))
        {
            FileInfo fi = new FileInfo(logFilePath);
            if (fi.CreationTime < DateTime.Now.AddMonths(-1))
            {
                fi.Delete();
            }
        }
        Trace.Listeners.Add(new TextWriterTraceListener(logFilePath));
        Trace.AutoFlush = true;
        ApplicationConfiguration.Initialize();
        Trace.WriteLine($"[{DateTime.Now}]: Ouverture de l'application.");
        Application.Run(new Form1());
    }    
}