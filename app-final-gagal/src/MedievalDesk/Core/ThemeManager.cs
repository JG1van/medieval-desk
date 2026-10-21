using Geometry = MedievalDesk.Core.Geometry;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace MedievalDesk.Core;

public static class ThemeManager
{
    public static readonly string[] Themes = ["Parchment", "Night"];

    public static string LoadPreference(DataStore store)
    {
        try
        {
            if (!File.Exists(store.PreferencesFile)) return "Parchment";
            using var doc = JsonDocument.Parse(File.ReadAllText(store.PreferencesFile));
            string? theme = doc.RootElement.TryGetProperty("theme", out var t) ? t.GetString() : null;
            return Themes.Contains(theme) ? theme! : "Parchment";
        }
        catch
        {
            return "Parchment";
        }
    }

    public static void Apply(DataStore store, string theme)
    {
        if (!Themes.Contains(theme)) theme = "Parchment";

        var app = Application.Current;
        if (app != null)
        {
            var dictionary = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Themes/{theme}.xaml", UriKind.Absolute)
            };
            var merged = app.Resources.MergedDictionaries;
            var existing = merged.FirstOrDefault(d =>
                d.Source != null && (d.Source.OriginalString.Contains("/Parchment.xaml") || d.Source.OriginalString.Contains("/Night.xaml")));
            if (existing != null) merged.Remove(existing);
            merged.Insert(0, dictionary);
        }

        try
        {
            Directory.CreateDirectory(store.Root);
            string temp = store.PreferencesFile + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(new { theme }, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temp, store.PreferencesFile, true);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Gagal menyimpan preferensi tema: {ex.Message}");
        }
    }
}
