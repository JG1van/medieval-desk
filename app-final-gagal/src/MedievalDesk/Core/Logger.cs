using Geometry = MedievalDesk.Core.Geometry;
using System.IO;
using System.Text;

namespace MedievalDesk.Core;

public static class Logger
{
    private static readonly object Gate = new();
    public static string Root { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MedievalDesk");
    public static string LogFile { get; } = Path.Combine(Root, "logs", "medieval-desk.log");

    public static void Init()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogFile)!);
            var info = new FileInfo(LogFile);
            if (info.Exists && info.Length > 1_000_000)
            {
                File.Copy(LogFile, LogFile + ".old", true);
                File.WriteAllText(LogFile, "");
            }
            Info($"=== START {DateTime.Now:yyyy-MM-dd HH:mm:ss} v1.0.0 ===");
        }
        catch
        {
            // Logging must never block startup.
        }
    }

    public static void Info(string message) => Write("INFO", message);
    public static void Warn(string message) => Write("WARN", message);
    public static void Error(string message) => Write("ERROR", message);
    public static void Error(string message, Exception ex) => Write("ERROR", $"{message}: {ex}");

    public static void Write(string level, string message)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogFile)!);
                File.AppendAllText(LogFile, $"{DateTime.Now:HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}", Encoding.UTF8);
            }
        }
        catch
        {
            // ignore logging failures
        }
    }
}
