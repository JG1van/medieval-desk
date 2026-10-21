using Geometry = MedievalDesk.Core.Geometry;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MedievalDesk.Core;

public static class ProfileStore
{
    public static bool IsValidName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        string trimmed = name.Trim();
        if (trimmed.Length is < 1 or > 40) return false;
        if (trimmed is "." or "..") return false;
        return trimmed.All(c => char.IsLetterOrDigit(c) || c is ' ' or '_' or '-');
    }

    public static string SanitizeFileName(string name)
    {
        var sb = new StringBuilder();
        foreach (char c in Path.GetFileName(name))
            sb.Append(char.IsLetterOrDigit(c) || c is '.' or '_' or '-' or ' ' ? c : '_');
        string result = sb.ToString().Trim();
        return string.IsNullOrWhiteSpace(result) ? "file" : result;
    }

    public static string HashFile(string path)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(sha.ComputeHash(stream)).ToLowerInvariant();
    }
}

public static class ImageStore
{
    public static string CopyInto(DataStore store, string hotspotId, string sourcePath)
    {
        string ext = Path.GetExtension(sourcePath).ToLowerInvariant();
        if (!DataValidation.ImageExtensions.Contains(ext))
            throw new DataException($"Format gambar tidak didukung: {ext}");

        string safeId = ProfileStore.SanitizeFileName(hotspotId);
        string dir = Path.Combine(store.ImagesDir, safeId);
        Directory.CreateDirectory(dir);

        string baseName = ProfileStore.SanitizeFileName(Path.GetFileNameWithoutExtension(sourcePath));
        string fileName = baseName + ext;
        string target = Path.Combine(dir, fileName);
        int counter = 1;
        while (File.Exists(target))
        {
            if (ProfileStore.HashFile(target) == ProfileStore.HashFile(sourcePath))
                return ToRelative(store, target);
            fileName = $"{baseName}_{counter++}{ext}";
            target = Path.Combine(dir, fileName);
        }

        File.Copy(sourcePath, target);
        return ToRelative(store, target);
    }

    public static string? Resolve(DataStore store, string? relative)
    {
        if (!DataValidation.IsSafeRelativeImage(relative)) return null;
        string full = Path.GetFullPath(Path.Combine(store.Root, relative!));
        string root = Path.GetFullPath(store.ImagesDir) + Path.DirectorySeparatorChar;
        return full.StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(full) ? full : null;
    }

    public static void Delete(DataStore store, string? relative)
    {
        if (!DataValidation.IsSafeRelativeImage(relative)) return;
        string full = Path.GetFullPath(Path.Combine(store.Root, relative!));
        string root = Path.GetFullPath(store.ImagesDir) + Path.DirectorySeparatorChar;
        if (full.StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(full))
            File.Delete(full);
    }

    public static IEnumerable<string> EnumerateFolder(string folder)
    {
        if (!Directory.Exists(folder)) yield break;
        foreach (string file in Directory.EnumerateFiles(folder))
            if (DataValidation.ImageExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                yield return file;
    }

    private static string ToRelative(DataStore store, string fullPath)
        => Path.GetRelativePath(store.Root, fullPath).Replace('\\', '/');
}
