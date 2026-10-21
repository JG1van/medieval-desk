using System.IO;
using System.Windows;
using System.Windows.Threading;
using MedievalDesk.Core;

namespace MedievalDesk;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Logger.Init();

        DispatcherUnhandledException += OnUnhandled;
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Logger.Error("Unhandled domain exception", args.ExceptionObject as Exception ?? new Exception("unknown"));

        if (e.Args.Any(a => string.Equals(a, "--self-test", StringComparison.OrdinalIgnoreCase)))
        {
            int code = SelfTest.Run();
            Shutdown(code);
            return;
        }

        Wallpaper.Apply(Path.Combine(AppContext.BaseDirectory, "Assets", "wallpaper_polos.png"));
        Exit += (_, _) => Wallpaper.Restore();

        try
        {
            var window = new MainWindow();
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            Logger.Error("Gagal start", ex);
            MessageBox.Show($"Aplikasi gagal start:\n{ex.Message}\n\nLog: {Logger.LogFile}", "Medieval Desk",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void OnUnhandled(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Logger.Error("Unhandled UI exception", e.Exception);
        MessageBox.Show($"Terjadi error:\n{e.Exception.Message}\n\nLog: {Logger.LogFile}", "Medieval Desk",
            MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
