using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace ASLInputTool;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global exception handling to catch and log any crash causes
        AppDomain.CurrentDomain.UnhandledException += (s, ev) => LogCrash(ev.ExceptionObject as Exception);
        DispatcherUnhandledException += (s, ev) => { LogCrash(ev.Exception); ev.Handled = false; };
    }

    private void LogCrash(Exception? ex)
    {
        if (ex == null) return;
        try
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
            var sb = new StringBuilder();
            sb.AppendLine($"--- Crash Log {DateTime.Now} ---");
            sb.AppendLine($"Message: {ex.Message}");
            sb.AppendLine($"Type: {ex.GetType().Name}");
            sb.AppendLine("Stack Trace:");
            sb.AppendLine(ex.StackTrace);
            
            if (ex.InnerException != null)
            {
                sb.AppendLine("Inner Exception:");
                sb.AppendLine(ex.InnerException.Message);
                sb.AppendLine(ex.InnerException.StackTrace);
            }
            
            File.AppendAllText(logPath, sb.ToString());
            
            MessageBox.Show($"A critical error occurred. Details have been written to crash_log.txt.\n\nError: {ex.Message}", 
                "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch { /* Last resort silence */ }
    }
}
