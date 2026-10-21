using Geometry = MedievalDesk.Core.Geometry;
using System.IO;
using System.Text;
using System.Text.Json;

namespace MedievalDesk.Core;

public sealed class DataException : Exception
{
    public DataException(string message) : base(message) { }
}

public sealed record ImportPreview(
    DeskData Imported,
    List<string> Added,
    List<string> Updated,
    List<string> Warnings)
{
    public bool HasChanges => Added.Count > 0 || Updated.Count > 0;
}

public sealed class DataStore
{
    private const long MaxBytes = 4 * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true
    };

    public string Root { get; }
    public string DataDir { get; }
    public string DataFile { get; }
    public string BackupFile { get; }
    public string SeedFile { get; }
    public string ProfilesDir { get; }
    public string ImagesDir { get; }
    public string PreferencesFile { get; }
    public string LogsDir { get; }

    public DataStore(string? root = null, string? seed = null)
    {
        Root = root ?? Logger.Root;
        DataDir = Path.Combine(Root, "data");
        DataFile = Path.Combine(DataDir, "medieval-desk.json");
        BackupFile = Path.Combine(DataDir, "medieval-desk.bak.json");
        SeedFile = seed ?? Path.Combine(AppContext.BaseDirectory, "Assets", "sample-layout.json");
        ProfilesDir = Path.Combine(Root, "profiles");
        ImagesDir = Path.Combine(Root, "images");
        PreferencesFile = Path.Combine(Root, "preferences.json");
        LogsDir = Path.Combine(Root, "logs");
    }

    public (DeskData Data, string? Warning) LoadOrSeed()
    {
        Directory.CreateDirectory(DataDir);

        if (File.Exists(DataFile))
        {
            try
            {
                return (LoadFile(DataFile), null);
            }
            catch (DataException ex)
            {
                Logger.Warn($"Data utama rusak: {ex.Message}");
                if (File.Exists(BackupFile))
                {
                    try
                    {
                        var backup = LoadFile(BackupFile);
                        Logger.Warn("Data cadangan dipakai.");
                        return (backup, $"Data utama rusak, cadangan dipakai.\n{ex.Message}");
                    }
                    catch (DataException backupEx)
                    {
                        throw new DataException(
                            $"Data hotspot rusak dan cadangan juga tidak valid.\n\nUtama: {DataFile}\n{ex.Message}\n\nCadangan: {BackupFile}\n{backupEx.Message}");
                    }
                }
                throw new DataException(
                    $"Data hotspot rusak dan tidak ada cadangan.\n\nFile: {DataFile}\n{ex.Message}\n\nPerbaiki file lalu jalankan ulang.");
            }
        }

        if (!File.Exists(SeedFile))
            throw new DataException($"Data awal tidak ditemukan: {SeedFile}");

        File.Copy(SeedFile, DataFile, false);
        Logger.Info($"Seed disalin: {SeedFile} -> {DataFile}");
        return (LoadFile(DataFile), null);
    }

    public DeskData LoadFile(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists) throw new DataException($"File tidak ditemukan: {path}");
        if (info.Length > MaxBytes) throw new DataException($"File terlalu besar (maks 4 MB): {path}");

        DeskData? data;
        try
        {
            data = JsonSerializer.Deserialize<DeskData>(File.ReadAllText(path, Encoding.UTF8), JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new DataException($"JSON tidak valid: {ex.Message}");
        }

        var errors = DataValidation.Validate(data);
        if (errors.Count > 0)
            throw new DataException("Validasi gagal:\n- " + string.Join("\n- ", errors.Take(12)) +
                                    (errors.Count > 12 ? $"\n- ... {errors.Count - 12} error lain" : ""));

        Normalize(data!);
        return data!;
    }

    public void Save(DeskData data) => SaveTo(DataFile, data, atomicWithBackup: true);

    public void Export(string path, DeskData data) => SaveTo(path, data, atomicWithBackup: false);

    public void SaveTo(string path, DeskData data, bool atomicWithBackup)
    {
        var errors = DataValidation.Validate(data);
        if (errors.Count > 0)
            throw new DataException("Tidak disimpan, data tidak valid:\n- " + string.Join("\n- ", errors.Take(12)));

        Normalize(data);
        string json = JsonSerializer.Serialize(data, JsonOptions);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);

        if (!atomicWithBackup)
        {
            File.WriteAllText(path, json, new UTF8Encoding(false));
            return;
        }

        string temp = path + ".tmp";
        File.WriteAllText(temp, json, new UTF8Encoding(false));
        Directory.CreateDirectory(Path.GetDirectoryName(BackupFile)!);
        if (File.Exists(path))
            File.Replace(temp, path, BackupFile, true);
        else
            File.Move(temp, path);
    }

    public ImportPreview PreviewImport(string path, DeskData current)
    {
        var imported = LoadFile(path);
        var currentById = current.Hotspots.ToDictionary(h => h.Id, StringComparer.OrdinalIgnoreCase);
        var added = new List<string>();
        var updated = new List<string>();
        var warnings = new List<string>();

        foreach (var h in imported.Hotspots)
        {
            if (currentById.TryGetValue(h.Id, out var existing))
            {
                if (!string.Equals(JsonSerializer.Serialize(existing), JsonSerializer.Serialize(h), StringComparison.Ordinal))
                    updated.Add($"{h.Id} ({h.Label})");
            }
            else
            {
                added.Add($"{h.Id} ({h.Label})");
            }
        }

        foreach (var old in current.Hotspots)
            if (!imported.Hotspots.Any(h => string.Equals(h.Id, old.Id, StringComparison.OrdinalIgnoreCase)))
                warnings.Add($"Hotspot lama dipertahankan (tidak ada di file impor): {old.Id}");

        return new ImportPreview(imported, added, updated, warnings);
    }

    public void ApplyImport(DeskData current, ImportPreview preview)
    {
        var imported = preview.Imported;
        current.Reference = imported.Reference;
        current.Settings = imported.Settings;
        current.Widgets = imported.Widgets;
        current.Extra = imported.Extra;

        var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < current.Hotspots.Count; i++) index[current.Hotspots[i].Id] = i;

        foreach (var h in imported.Hotspots)
        {
            if (index.TryGetValue(h.Id, out int at))
                current.Hotspots[at] = h;
            else
            {
                index[h.Id] = current.Hotspots.Count;
                current.Hotspots.Add(h);
            }
        }

        Save(current);
    }

    public IReadOnlyList<string> ListProfiles()
    {
        if (!Directory.Exists(ProfilesDir)) return Array.Empty<string>();
        return Directory.GetFiles(ProfilesDir, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => n != null && ProfileStore.IsValidName(n))
            .Select(n => n!)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public DeskData LoadProfile(string name)
    {
        if (!ProfileStore.IsValidName(name)) throw new DataException("Nama profil tidak valid.");
        string path = Path.Combine(ProfilesDir, name + ".json");
        if (!path.StartsWith(ProfilesDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new DataException("Path profil tidak aman.");
        return LoadFile(path);
    }

    private static void Normalize(DeskData data)
    {
        data.Reference ??= new Reference();
        data.Settings ??= new DeskSettings();
        data.Settings.Location ??= new Location();
        data.Hotspots ??= new List<Hotspot>();
        data.Widgets ??= new List<JsonElement>();
        foreach (var h in data.Hotspots)
        {
            h.Groups ??= new List<HotspotGroup>();
            h.Images ??= new List<string>();
            if (string.IsNullOrWhiteSpace(h.DisplayMode))
                h.DisplayMode = h.Kind == "configurable" ? "popup-list" : "direct";
            foreach (var g in h.Groups)
                g.Items ??= new List<HotspotItem>();
        }
    }
}
