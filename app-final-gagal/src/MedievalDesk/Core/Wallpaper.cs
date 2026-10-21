using System.IO;
using System.Text;

namespace MedievalDesk.Core;

public static class Wallpaper
{
    private static bool _applied;
    private static bool _restored;

    public static string BackupFile => Path.Combine(Logger.Root, "original-wallpaper.txt");

    public static string Current()
    {
        try
        {
            var value = new StringBuilder(1024);
            return Native.SystemParametersInfo(Native.SPI_GETDESKWALLPAPER, (uint)value.Capacity, value, 0)
                ? value.ToString()
                : "";
        }
        catch (Exception ex)
        {
            Logger.Warn($"Gagal membaca wallpaper: {ex.Message}");
            return "";
        }
    }

    public static bool ShouldReplaceBackup(string current, string saved, string room) =>
        !string.IsNullOrWhiteSpace(current)
        && !SamePath(current, saved)
        && !SamePath(current, room);

    public static void Apply(string roomPath)
    {
        if (_applied) return;
        try
        {
            string room = Path.GetFullPath(roomPath);
            if (!File.Exists(room))
            {
                Logger.Warn($"Wallpaper tidak dipasang, aset tidak ada: {room}");
                return;
            }

            string current = Current();
            string saved = ReadBackup();
            if (ShouldReplaceBackup(current, saved, room))
            {
                WriteBackup(current);
                saved = current;
            }

            if (!SetWallpaper(room))
            {
                Logger.Warn($"Gagal memasang wallpaper: {room}");
                return;
            }

            _applied = true;
            Logger.Info($"Wallpaper dipasang: {room}; asli: {(saved.Length > 0 ? saved : "(tidak tersedia)")}");
        }
        catch (Exception ex)
        {
            Logger.Warn($"Gagal memasang wallpaper: {ex.Message}");
        }
    }

    public static void Restore()
    {
        if (!_applied || _restored) return;
        try
        {
            string saved = ReadBackup();
            if (string.IsNullOrWhiteSpace(saved))
            {
                Logger.Warn("Wallpaper asli tidak tersimpan; restore dilewati.");
                _restored = true;
                return;
            }
            if (!File.Exists(saved))
            {
                Logger.Warn($"File wallpaper asli tidak ditemukan: {saved}");
                _restored = true;
                return;
            }

            _restored = SetWallpaper(saved);
            if (_restored) Logger.Info($"Wallpaper dipulihkan: {saved}");
            else Logger.Warn($"Gagal memulihkan wallpaper: {saved}");
        }
        catch (Exception ex)
        {
            Logger.Warn($"Gagal memulihkan wallpaper: {ex.Message}");
        }
    }

    private static bool SetWallpaper(string path)
    {
        try
        {
            return Native.SystemParametersInfo(
                Native.SPI_SETDESKWALLPAPER, 0, path,
                Native.SPIF_UPDATEINIFILE | Native.SPIF_SENDCHANGE);
        }
        catch (Exception ex)
        {
            Logger.Warn($"SystemParametersInfo gagal: {ex.Message}");
            return false;
        }
    }

    private static string ReadBackup()
    {
        try
        {
            return File.Exists(BackupFile) ? File.ReadAllText(BackupFile).Trim() : "";
        }
        catch (Exception ex)
        {
            Logger.Warn($"Gagal membaca backup wallpaper: {ex.Message}");
            return "";
        }
    }

    private static void WriteBackup(string path)
    {
        Directory.CreateDirectory(Logger.Root);
        string temp = BackupFile + ".tmp";
        File.WriteAllText(temp, path);
        File.Move(temp, BackupFile, true);
    }

    private static bool SamePath(string a, string b)
    {
        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;
        try
        {
            return string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
