using Geometry = MedievalDesk.Core.Geometry;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text;
using System.Text.Json;
using MedievalDesk.Core;

namespace MedievalDesk;

public static class SelfTest
{
    public static int Run()
    {
        var lines = new List<string>();
        int passed = 0, failed = 0;

        void Check(string name, Action test)
        {
            try
            {
                test();
                passed++;
                lines.Add($"LULUS  {name}");
            }
            catch (Exception ex)
            {
                failed++;
                lines.Add($"GAGAL  {name}: {ex.Message}");
            }
        }

        string temp = Path.Combine(Path.GetTempPath(), "medieval-desk-selftest-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(temp);
            string seed = Path.Combine(AppContext.BaseDirectory, "Assets", "sample-layout.json");
            var store = new DataStore(temp, seed);

            Check("seed dan validasi 10 hotspot", () =>
            {
                var (data, warning) = store.LoadOrSeed();
                Assert(data.Hotspots.Count == 10, $"jumlah hotspot {data.Hotspots.Count}");
                Assert(warning == null, "warning tak terduga");
                Assert(data.Hotspots.Select(h => h.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() == 10, "id duplikat");
            });

            Check("tolak id duplikat", () =>
            {
                var data = Minimal();
                data.Hotspots.Add(new Hotspot { Id = "a", Box = new HotspotBox { X = 1, Y = 1, W = 5, H = 5 } });
                Assert(DataValidation.Validate(data).Any(e => e.Contains("duplikat")), "duplikat lolos");
            });

            Check("tolak box keluar batas", () =>
            {
                var data = Minimal();
                data.Hotspots[0].Box = new HotspotBox { X = 95, Y = 1, W = 20, H = 5 };
                Assert(DataValidation.Validate(data).Any(e => e.Contains("batas")), "box lolos");
            });

            Check("tolak versi schema lain", () =>
            {
                var data = Minimal();
                data.Version = 2;
                Assert(DataValidation.Validate(data).Any(e => e.Contains("Versi")), "versi 2 lolos");
            });

            Check("tolak path gambar traversal", () =>
            {
                var data = Minimal();
                data.Hotspots[0].Images.Add("images/../../evil.png");
                Assert(DataValidation.Validate(data).Any(e => e.Contains("tidak aman")), "traversal lolos");
                Assert(!DataValidation.IsSafeRelativeImage("C:\\evil.png"), "path absolut lolos");
            });

            Check("backup dibuat saat simpan kedua", () =>
            {
                var data = store.LoadFile(store.DataFile);
                data.Hotspots[0].Label = "Uji Backup";
                store.Save(data);
                Assert(File.Exists(store.BackupFile), "backup tidak ada");
                var backup = store.LoadFile(store.BackupFile);
                Assert(backup.Hotspots.Count == 10, "backup tidak valid");
            });

            Check("preview dan merge import by id", () =>
            {
                var current = store.LoadFile(store.DataFile);
                var imported = Minimal();
                string existingId = current.Hotspots[0].Id;
                imported.Hotspots[0].Id = existingId;
                imported.Hotspots[0].Label = "Judul Baru";
                imported.Hotspots.Add(new Hotspot { Id = "baru", Label = "Baru", Box = new HotspotBox { X = 40, Y = 40, W = 10, H = 10 } });
                string importPath = Path.Combine(temp, "import.json");
                store.Export(importPath, imported);
                var preview = store.PreviewImport(importPath, current);
                Assert(preview.Added.Count == 1, $"added {preview.Added.Count}");
                Assert(preview.Updated.Count == 1, $"updated {preview.Updated.Count}");
                store.ApplyImport(current, preview);
                var merged = store.LoadFile(store.DataFile);
                Assert(merged.Hotspots.Count == 11, $"hasil merge {merged.Hotspots.Count}");
                Assert(merged.Hotspots.Count(h => h.Id == existingId) == 1, "id dobel setelah merge");
                Assert(merged.Hotspots.First(h => h.Id == existingId).Label == "Judul Baru", "label tidak terupdate");
            });

            Check("cover rect tidak distorsi", () =>
            {
                var rect = Geometry.CoverRect(1920, 1080, 1365, 768);
                Assert(Math.Abs(rect.Width / rect.Height - 1365.0 / 768.0) < 0.0001, "rasio berubah");
                Assert(rect.Width >= 1920 && rect.Height >= 1080, "cover tidak menutup viewport");
                var box = new HotspotBox { X = 10, Y = 20, W = 30, H = 40 };
                var px = Geometry.BoxToPixels(box, rect);
                var back = Geometry.PixelsToBox(px, rect);
                Assert(Math.Abs(back.X - 10) < 0.01 && Math.Abs(back.H - 40) < 0.01, "konversi persen tidak balik");
            });

            Check("skala tampilan normal/hover konsisten", () =>
            {
                var (data, _) = store.LoadOrSeed();
                int pairs = 0;
                foreach (var hotspot in data.Hotspots)
                {
                    string? baseName = MainWindow.AssetBase(hotspot);
                    if (baseName == null) continue;

                    string diamPath = Path.Combine(AppContext.BaseDirectory, "Assets", "diam", baseName + "_diam.png");
                    string hoverPath = Path.Combine(AppContext.BaseDirectory, "Assets", "hover", baseName + "_hover.png");
                    if (!File.Exists(diamPath) || !File.Exists(hoverPath)) continue;

                    BitmapSource diam = AssetNormalizer.Load(diamPath);
                    BitmapSource hover = AssetNormalizer.Load(hoverPath);
                    Size a = AssetNormalizer.UniformFit(diam.PixelWidth, diam.PixelHeight, hotspot.Box.W, hotspot.Box.H);
                    Size b = AssetNormalizer.UniformFit(hover.PixelWidth, hover.PixelHeight, hotspot.Box.W, hotspot.Box.H);

                    double deltaW = Math.Abs(b.Width / a.Width - 1);
                    double deltaH = Math.Abs(b.Height / a.Height - 1);
                    Assert(Math.Min(deltaW, deltaH) <= 0.03,
                        $"{hotspot.Id}: lebar {deltaW:P0}, tinggi {deltaH:P0} berbeda");
                    pairs++;
                }
                Assert(pairs == 10, $"pasangan diuji {pairs}");
            });

            Check("ganti sumber gambar tidak mengubah ukuran kontrol", () =>
            {
                string diamPath = Path.Combine(AppContext.BaseDirectory, "Assets", "diam", "calendar_diam.png");
                string hoverPath = Path.Combine(AppContext.BaseDirectory, "Assets", "hover", "calendar_hover.png");
                var image = new System.Windows.Controls.Image { Stretch = Stretch.Uniform };
                var border = new System.Windows.Controls.Border { Width = 120, Height = 60, Child = image };

                void Layout() { border.Measure(new Size(120, 60)); border.Arrange(new Rect(0, 0, 120, 60)); }

                image.Source = AssetNormalizer.Load(diamPath);
                Layout();
                Size normal = border.RenderSize;
                image.Source = AssetNormalizer.Load(hoverPath);
                Layout();
                Size hover = border.RenderSize;

                Assert(normal == hover, $"kontrol {normal} menjadi {hover}");
            });

            Check("interval slideshow dibatasi", () =>
            {
                Assert(DataValidation.ClampInterval(null) == 5000, "default salah");
                Assert(DataValidation.ClampInterval(10) == 1000, "minimum salah");
                Assert(DataValidation.ClampInterval(999999) == 60000, "maksimum salah");
            });

            Check("nama profil aman", () =>
            {
                Assert(ProfileStore.IsValidName("Kerja Malam_1"), "nama valid ditolak");
                Assert(!ProfileStore.IsValidName("..\\evil"), "traversal diterima");
                Assert(!ProfileStore.IsValidName("a/b"), "slash diterima");
                Assert(!ProfileStore.IsValidName(""), "kosong diterima");
            });

            Check("salin gambar, dedupe, dan hapus", () =>
            {
                string image = Path.Combine(AppContext.BaseDirectory, "Assets", "wallpaper_polos.png");
                Assert(File.Exists(image), "aset wallpaper_polos tidak ada");
                string rel1 = ImageStore.CopyInto(store, "uji", image);
                string rel2 = ImageStore.CopyInto(store, "uji", image);
                Assert(rel1 == rel2, "duplikat tidak dedupe");
                string? full = ImageStore.Resolve(store, rel1);
                Assert(full != null && File.Exists(full), "path relatif tidak resolve");
                ImageStore.Delete(store, rel1);
                Assert(!File.Exists(full), "file tidak terhapus");
            });

            Check("profil simpan dan muat", () =>
            {
                var data = store.LoadFile(store.DataFile);
                Directory.CreateDirectory(store.ProfilesDir);
                string path = Path.Combine(store.ProfilesDir, "Uji Profil.json");
                store.Export(path, data);
                var loaded = store.LoadProfile("Uji Profil");
                Assert(loaded.Hotspots.Count == data.Hotspots.Count, "profil tidak sama");
            });

            Check("preferensi tema tersimpan", () =>
            {
                Directory.CreateDirectory(store.Root);
                File.WriteAllText(store.PreferencesFile, "{\"theme\":\"Night\"}");
                Assert(ThemeManager.LoadPreference(store) == "Night", "preferensi tidak terbaca");
                File.WriteAllText(store.PreferencesFile, "rusak");
                Assert(ThemeManager.LoadPreference(store) == "Parchment", "fallback gagal");
            });

            Check("backup wallpaper asli tidak tertimpa", () =>
            {
                const string room = @"C:\MedievalDesk\Assets\wallpaper_polos.png";
                const string original = @"C:\Users\User\wallpaper.jpg";
                Assert(!Wallpaper.ShouldReplaceBackup(original, original, room), "backup asli tertimpa");
                Assert(!Wallpaper.ShouldReplaceBackup(room, original, room), "backup crash tertimpa");
                Assert(!Wallpaper.ShouldReplaceBackup("", original, room), "backup kosong menimpa");
                Assert(Wallpaper.ShouldReplaceBackup(@"C:\baru.jpg", original, room), "wallpaper baru tidak dicatat");
            });
        }
        finally
        {
            try { Directory.Delete(temp, true); } catch { }
        }

        var report = new StringBuilder();
        report.AppendLine($"SELF-TEST {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"LULUS: {passed}; GAGAL: {failed}");
        foreach (string line in lines) report.AppendLine(line);
        string text = report.ToString();

        try
        {
            Native.AttachConsole(Native.ATTACH_PARENT_PROCESS);
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine();
            Console.Write(text);
        }
        catch { }

        try
        {
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "self-test.txt"), text, Encoding.UTF8);
        }
        catch { }

        Logger.Info($"Self-test selesai: lulus={passed} gagal={failed}");
        return failed == 0 ? 0 : 1;
    }

    private static DeskData Minimal() => new()
    {
        Version = 1,
        Reference = new Reference { Width = 1376, Height = 768 },
        Hotspots =
        [
            new Hotspot { Id = "a", Label = "A", Kind = "configurable", Box = new HotspotBox { X = 1, Y = 1, W = 5, H = 5 }, Groups = [], Images = [] }
        ]
    };

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
