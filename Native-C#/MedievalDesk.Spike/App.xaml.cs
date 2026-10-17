using System;
using System.IO;
using System.Text;
using System.Windows;
using MedievalDesk.Spike.Native;

namespace MedievalDesk.Spike;

public partial class App : Application
{
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "spike-log.txt");

    public static string? OriginalWallpaper;
    private static bool _wallpaperRestored;

    public static void Log(string message) =>
        File.AppendAllText(LogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");

    public static void InitOriginalWallpaper()
    {
        try
        {
            string appDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MedievalDesk");
            Directory.CreateDirectory(appDir);
            string file = Path.Combine(appDir, "original-wallpaper.txt");

            if (File.Exists(file))
            {
                OriginalWallpaper = File.ReadAllText(file).Trim();
                Log($"WALLPAPER ASLI (file): {OriginalWallpaper}");
                return;
            }

            string? fromDocs = TryReadFromSpikeA();
            if (fromDocs != null)
            {
                OriginalWallpaper = fromDocs;
                File.WriteAllText(file, fromDocs);
                Log($"WALLPAPER ASLI (SPIKE-A.md): {fromDocs}");
                return;
            }

            var sb = new StringBuilder(260);
            Win32.SystemParametersInfo(Win32.SPI_GETDESKWALLPAPER, 260, sb, 0);
            OriginalWallpaper = sb.ToString();
            File.WriteAllText(file, OriginalWallpaper);
            Log($"WALLPAPER ASLI (saat ini, disimpan sekali): {OriginalWallpaper}");
        }
        catch (Exception ex)
        {
            Log($"EXCEPTION InitOriginalWallpaper: {ex.Message}");
        }
    }

    private static string? TryReadFromSpikeA()
    {
        DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
        {
            string candidate = Path.Combine(dir.FullName, "docs", "SPIKE-A.md");
            if (!File.Exists(candidate)) continue;
            foreach (var raw in File.ReadAllLines(candidate))
            {
                string line = raw.TrimStart('-', ' ', '\t');
                int idx = line.IndexOf("Wallpaper asli:", StringComparison.OrdinalIgnoreCase);
                if (idx < 0) continue;
                string value = line[(idx + "Wallpaper asli:".Length)..].Trim();
                if (value.Length > 0) return value;
            }
        }
        return null;
    }

    public static void RestoreWallpaper()
    {
        if (_wallpaperRestored) return;
        if (string.IsNullOrEmpty(OriginalWallpaper)) OriginalWallpaper = ReadFileValue();
        if (string.IsNullOrEmpty(OriginalWallpaper))
        {
            Log("RESTORE: tidak ada wallpaper asli tersimpan");
            return;
        }
        var sb = new StringBuilder(OriginalWallpaper);
        int r = Win32.SystemParametersInfo(Win32.SPI_SETDESKWALLPAPER, 0, sb,
            Win32.SPIF_UPDATEINIFILE | Win32.SPIF_SENDCHANGE);
        _wallpaperRestored = true;
        Log($"RESTORE wallpaper: {(r != 0 ? "OK" : "GAGAL")} -> {OriginalWallpaper}");
    }

    private static string? ReadFileValue()
    {
        try
        {
            string file = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MedievalDesk", "original-wallpaper.txt");
            return File.Exists(file) ? File.ReadAllText(file).Trim() : null;
        }
        catch
        {
            return null;
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        InitOriginalWallpaper();

        DispatcherUnhandledException += (_, args) =>
        {
            Log($"UI EXCEPTION: {args.Exception}");
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Log($"DOMAIN EXCEPTION: {args.ExceptionObject}");

        Exit += (_, args) =>
        {
            RestoreWallpaper();
            Log($"App.Exit, kode {args.ApplicationExitCode}");
        };
    }
}
