using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace Steelmeet2;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        // logga alla okända krascher till fil så att "den crashade" alltid har spår
        Dispatcher.UIThread.UnhandledException += (_, e) => TryLog(e.Exception);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            string? meetId = null;
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "--meet") meetId = args[i + 1];
            desktop.MainWindow = new MainWindow(meetId);
            if (args.Contains("--max"))
                desktop.MainWindow.WindowState = WindowState.Maximized;
        }
        base.OnFrameworkInitializationCompleted();
    }

    static void TryLog(Exception ex)
    {
        try
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Steelmeet2");
            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, "crashlog.txt"),
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{ex}\n\n");
        }
        catch { }
    }
}
