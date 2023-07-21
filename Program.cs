namespace csharp;
using System.Diagnostics;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        Trace.Listeners.Add(new TextWriterTraceListener("logfile.txt"));
        Trace.AutoFlush = true;
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }    
}