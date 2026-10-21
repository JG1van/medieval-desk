using Geometry = MedievalDesk.Core.Geometry;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MedievalDesk.Core;

public sealed class DeskData
{
    [JsonPropertyName("version")] public int Version { get; set; } = 1;
    [JsonPropertyName("reference")] public Reference Reference { get; set; } = new();
    [JsonPropertyName("settings")] public DeskSettings Settings { get; set; } = new();
    [JsonPropertyName("hotspots")] public List<Hotspot> Hotspots { get; set; } = new();
    [JsonPropertyName("widgets")] public List<JsonElement> Widgets { get; set; } = new();
    [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

public sealed class Reference
{
    [JsonPropertyName("width")] public double Width { get; set; } = 1376;
    [JsonPropertyName("height")] public double Height { get; set; } = 768;
}

public sealed class DeskSettings
{
    [JsonPropertyName("minimizeAfterLaunch")] public bool MinimizeAfterLaunch { get; set; } = true;
    [JsonPropertyName("hotkey")] public string Hotkey { get; set; } = "Ctrl+Alt+D";
    [JsonPropertyName("location")] public Location Location { get; set; } = new();
    [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

public sealed class Location
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("lat")] public double? Lat { get; set; }
    [JsonPropertyName("lon")] public double? Lon { get; set; }
}

public sealed class Hotspot
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("label")] public string Label { get; set; } = "";
    [JsonPropertyName("kind")] public string Kind { get; set; } = "configurable";
    [JsonPropertyName("fixedType")] public string? FixedType { get; set; }
    [JsonPropertyName("box")] public HotspotBox Box { get; set; } = new();
    [JsonPropertyName("groups")] public List<HotspotGroup> Groups { get; set; } = new();
    [JsonPropertyName("displayMode")] public string DisplayMode { get; set; } = "direct";
    [JsonPropertyName("images")] public List<string> Images { get; set; } = new();
    [JsonPropertyName("slideshowMs")] public int? SlideshowMs { get; set; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

public sealed class HotspotBox
{
    [JsonPropertyName("x")] public double X { get; set; }
    [JsonPropertyName("y")] public double Y { get; set; }
    [JsonPropertyName("w")] public double W { get; set; }
    [JsonPropertyName("h")] public double H { get; set; }
}

public sealed class HotspotGroup
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("items")] public List<HotspotItem> Items { get; set; } = new();
}

public sealed class HotspotItem
{
    [JsonPropertyName("label")] public string Label { get; set; } = "";
    [JsonPropertyName("target")] public string Target { get; set; } = "";
}

public static class DataValidation
{
    public static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg", ".gif", ".bmp"];

    public static List<string> Validate(DeskData? data)
    {
        var errors = new List<string>();
        if (data == null)
        {
            errors.Add("Data kosong.");
            return errors;
        }

        if (data.Version != 1)
            errors.Add($"Versi schema tidak didukung: {data.Version}. Hanya versi 1.");

        if (data.Reference == null || !IsPositive(data.Reference.Width) || !IsPositive(data.Reference.Height))
            errors.Add("reference.width/height harus angka positif.");

        if (data.Hotspots == null)
        {
            errors.Add("hotspots harus berupa array.");
            return errors;
        }

        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < data.Hotspots.Count; i++)
        {
            var h = data.Hotspots[i];
            string where = $"hotspot[{i}]";
            if (h == null)
            {
                errors.Add($"{where} null.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(h.Id))
                errors.Add($"{where}.id wajib diisi.");
            else if (h.Id.Length > 64 || h.Id.Any(char.IsControl))
                errors.Add($"{where}.id tidak valid (maks 64 karakter, tanpa kontrol).");
            else if (!ids.Add(h.Id))
                errors.Add($"ID duplikat: {h.Id}");

            if (h.Box == null)
            {
                errors.Add($"{where}.box wajib ada.");
            }
            else
            {
                if (!IsFinite(h.Box.X) || !IsFinite(h.Box.Y) || !IsPositive(h.Box.W) || !IsPositive(h.Box.H))
                    errors.Add($"{where}.box harus angka dan w/h > 0.");
                else if (h.Box.X < 0 || h.Box.Y < 0 || h.Box.X + h.Box.W > 100.01 || h.Box.Y + h.Box.H > 100.01)
                    errors.Add($"{where}.box keluar batas 0-100%.");
            }

            if (h.Groups == null)
            {
                errors.Add($"{where}.groups harus berupa array.");
            }
            else
            {
                for (int g = 0; g < h.Groups.Count; g++)
                {
                    var grp = h.Groups[g];
                    if (grp == null)
                    {
                        errors.Add($"{where}.groups[{g}] null.");
                        continue;
                    }
                    if (grp.Items == null)
                    {
                        errors.Add($"{where}.groups[{g}].items harus array.");
                        continue;
                    }
                    for (int it = 0; it < grp.Items.Count; it++)
                    {
                        if (grp.Items[it] == null)
                            errors.Add($"{where}.groups[{g}].items[{it}] null.");
                    }
                }
            }

            if (h.Images != null)
            {
                foreach (string img in h.Images)
                {
                    if (!IsSafeRelativeImage(img))
                        errors.Add($"{where}.images path tidak aman: {img}");
                }
            }
        }

        return errors;
    }

    public static bool IsSafeRelativeImage(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        if (Path.IsPathRooted(path)) return false;
        string normalized = path.Replace('\\', '/');
        if (normalized.Split('/').Any(p => p == "..")) return false;
        string ext = Path.GetExtension(normalized);
        return ImageExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsPositive(double value) => IsFinite(value) && value > 0;
    public static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

    public static int ClampInterval(int? ms, int fallback = 5000)
    {
        int value = ms is > 0 ? ms.Value : fallback;
        return Math.Clamp(value, 1000, 60000);
    }
}

public static class Geometry
{
    public static System.Windows.Rect CoverRect(double viewportWidth, double viewportHeight, double imageWidth, double imageHeight)
    {
        if (viewportWidth <= 0 || viewportHeight <= 0 || imageWidth <= 0 || imageHeight <= 0)
            return new System.Windows.Rect(0, 0, Math.Max(0, viewportWidth), Math.Max(0, viewportHeight));

        double scale = Math.Max(viewportWidth / imageWidth, viewportHeight / imageHeight);
        double width = imageWidth * scale;
        double height = imageHeight * scale;
        return new System.Windows.Rect((viewportWidth - width) / 2, (viewportHeight - height) / 2, width, height);
    }

    public static System.Windows.Rect BoxToPixels(HotspotBox box, System.Windows.Rect drawn)
        => new(
            drawn.X + box.X / 100.0 * drawn.Width,
            drawn.Y + box.Y / 100.0 * drawn.Height,
            box.W / 100.0 * drawn.Width,
            box.H / 100.0 * drawn.Height);

    public static HotspotBox PixelsToBox(System.Windows.Rect area, System.Windows.Rect drawn)
    {
        if (drawn.Width <= 0 || drawn.Height <= 0) return new HotspotBox();
        double x = (area.X - drawn.X) / drawn.Width * 100.0;
        double y = (area.Y - drawn.Y) / drawn.Height * 100.0;
        double w = area.Width / drawn.Width * 100.0;
        double h = area.Height / drawn.Height * 100.0;
        x = Math.Clamp(x, 0, 100);
        y = Math.Clamp(y, 0, 100);
        w = Math.Clamp(w, 1, 100 - x);
        h = Math.Clamp(h, 1, 100 - y);
        return new HotspotBox { X = Math.Round(x, 2), Y = Math.Round(y, 2), W = Math.Round(w, 2), H = Math.Round(h, 2) };
    }
}
