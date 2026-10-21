using Geometry = MedievalDesk.Core.Geometry;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using MedievalDesk.Core;

namespace MedievalDesk;

public partial class MainWindow
{
    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _store.Save(_data);
            ShowStatus($"Tersimpan: {_store.DataFile}");
        }
        catch (Exception ex)
        {
            Logger.Error("Gagal simpan", ex);
            MessageBox.Show(ex.Message, "Simpan", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnExportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Ekspor layout",
            Filter = "JSON (*.json)|*.json",
            DefaultExt = ".json",
            AddExtension = true,
            FileName = $"medieval-desk-{DateTime.Now:yyyyMMdd}.json"
        };
        if (dialog.ShowDialog(this) != true) return;

        try
        {
            _store.Export(dialog.FileName, _data);
            ShowStatus($"Diekspor ke {dialog.FileName}");
        }
        catch (Exception ex)
        {
            Logger.Error("Gagal ekspor", ex);
            MessageBox.Show(ex.Message, "Ekspor", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnImportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Impor layout",
            Filter = "JSON (*.json)|*.json|Semua file (*.*)|*.*"
        };
        if (dialog.ShowDialog(this) != true) return;

        try
        {
            var preview = _store.PreviewImport(dialog.FileName, _data);
            var window = new ImportPreviewWindow(preview, dialog.FileName) { Owner = this };
            if (window.ShowDialog() != true) return;

            _store.Save(_data);
            _store.ApplyImport(_data, preview);
            RefreshHotspots();
            ShowStatus($"Impor selesai. Ditambah: {preview.Added.Count}, diperbarui: {preview.Updated.Count}.");
        }
        catch (Exception ex)
        {
            Logger.Error("Gagal impor", ex);
            MessageBox.Show(ex.Message, "Impor", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnProfilesClick(object sender, RoutedEventArgs e)
    {
        var window = new ProfilesWindow(_store.ListProfiles()) { Owner = this };
        if (window.ShowDialog() != true || string.IsNullOrWhiteSpace(window.Action)) return;

        string? name = window.ProfileName;
        try
        {
            switch (window.Action)
            {
                case "load" when name != null:
                    if (MessageBox.Show($"Muat profil \"{name}\"? Layout saat ini akan dicadangkan dulu.",
                            "Muat Profil", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                    _store.Save(_data);
                    _data = _store.LoadProfile(name);
                    _store.Save(_data);
                    RefreshHotspots();
                    ShowStatus($"Profil {name} dimuat.");
                    break;

                case "save" when name != null:
                {
                    if (!ProfileStore.IsValidName(name))
                    {
                        MessageBox.Show("Nama profil hanya huruf, angka, spasi, _ atau - (maks 40).", "Profil",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    Directory.CreateDirectory(_store.ProfilesDir);
                    string path = Path.Combine(_store.ProfilesDir, name + ".json");
                    if (File.Exists(path) && MessageBox.Show($"Timpa profil \"{name}\"?", "Simpan Profil",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                    _store.Export(path, _data);
                    ShowStatus($"Profil {name} disimpan.");
                    break;
                }

                case "delete" when name != null:
                {
                    if (MessageBox.Show($"Hapus profil \"{name}\"?", "Hapus Profil",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
                    string path = Path.Combine(_store.ProfilesDir, name + ".json");
                    if (File.Exists(path)) File.Delete(path);
                    ShowStatus($"Profil {name} dihapus.");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Operasi profil gagal", ex);
            MessageBox.Show(ex.Message, "Profil", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnThemeClick(object sender, RoutedEventArgs e)
    {
        string current = ThemeManager.LoadPreference(_store);
        string next = current == "Parchment" ? "Night" : "Parchment";
        ThemeManager.Apply(_store, next);
        ApplyEditVisuals();
        foreach (var view in _views.Values) SetHotspotImage(view, hover: false);
        ShowStatus($"Tema: {next}");
    }

    private void OnHotspotDragOver(object sender, DragEventArgs e)
    {
        if (!_editMode || !e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        e.Effects = DragDropEffects.Copy;
        DropHint.Visibility = Visibility.Visible;
        DropHintText.Text = sender is Border { Tag: Hotspot h }
            ? $"Lepaskan gambar ke: {h.Label}"
            : "Lepaskan gambar di hotspot";
        e.Handled = true;
    }

    private void OnHotspotDrop(object sender, DragEventArgs e)
    {
        DropHint.Visibility = Visibility.Collapsed;
        e.Handled = true;
        if (!_editMode) return;
        if (sender is not Border { Tag: Hotspot hotspot }) return;
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files) return;

        int added = 0;
        foreach (string file in files)
        {
            if (!DataValidation.ImageExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                continue;
            try
            {
                string relative = ImageStore.CopyInto(_store, hotspot.Id, file);
                foreach (var other in _data.Hotspots.Where(o => o.Id != hotspot.Id))
                    other.Images.RemoveAll(i => string.Equals(i, relative, StringComparison.OrdinalIgnoreCase));
                if (!hotspot.Images.Contains(relative))
                {
                    hotspot.Images.Add(relative);
                    added++;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Gagal impor drag-drop {file}", ex);
                ShowStatus($"Gagal: {Path.GetFileName(file)}", true);
            }
        }

        if (added == 0)
        {
            ShowStatus("Tidak ada gambar baru yang ditambahkan.", true);
            return;
        }

        try
        {
            _store.Save(_data);
            RefreshHotspots();
            ShowStatus($"{added} gambar ditambahkan ke {hotspot.Label}.");
        }
        catch (Exception ex)
        {
            Logger.Error("Gagal simpan hasil drag-drop", ex);
            MessageBox.Show(ex.Message, "Drag & Drop", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnHotspotDragLeave(object sender, DragEventArgs e) => DropHint.Visibility = Visibility.Collapsed;

    private void OpenLightbox(Hotspot hotspot)
    {
        var paths = ResolveImages(hotspot);
        if (paths.Count == 0)
        {
            ShowStatus("Hotspot ini belum punya gambar.", true);
            return;
        }

        _lightboxHost = hotspot;
        _lightboxPaths.Clear();
        _lightboxPaths.AddRange(paths);
        _lightboxIndex = 0;
        _autoAdvance = false;
        _slideshowTimer.Stop();
        SlideButton.Content = "Slideshow";
        LightboxTitle.Text = hotspot.Label;
        Lightbox.Visibility = Visibility.Visible;
        ShowLightboxImage();
        Logger.Info($"Lightbox {hotspot.Id}: {paths.Count} gambar.");
    }

    private void ShowLightboxImage()
    {
        if (_lightboxPaths.Count == 0) return;
        _lightboxIndex = (_lightboxIndex % _lightboxPaths.Count + _lightboxPaths.Count) % _lightboxPaths.Count;
        LightboxImage.Source = LoadBitmap(_lightboxPaths[_lightboxIndex]);
        LightboxCounter.Text = $"{_lightboxIndex + 1} / {_lightboxPaths.Count} · {Path.GetFileName(_lightboxPaths[_lightboxIndex])}";
    }

    private void LightboxStep(int delta)
    {
        if (Lightbox.Visibility != Visibility.Visible) return;
        _lightboxIndex += delta;
        ShowLightboxImage();
    }

    private void CloseLightbox()
    {
        _slideshowTimer.Stop();
        _autoAdvance = false;
        SlideButton.Content = "Slideshow";
        Lightbox.Visibility = Visibility.Collapsed;
        LightboxImage.Source = null;
        _lightboxHost = null;
        _lightboxPaths.Clear();
        _lightboxIndex = 0;
    }

    private void OnLightboxPrev(object sender, RoutedEventArgs e) => LightboxStep(-1);
    private void OnLightboxNext(object sender, RoutedEventArgs e) => LightboxStep(1);
    private void OnLightboxClose(object sender, RoutedEventArgs e) => CloseLightbox();

    private void OnLightboxSlideshow(object sender, RoutedEventArgs e)
    {
        if (_lightboxPaths.Count < 2)
        {
            ShowStatus("Slideshow butuh minimal 2 gambar.", true);
            return;
        }

        if (_autoAdvance)
        {
            _autoAdvance = false;
            _slideshowTimer.Stop();
            SlideButton.Content = "Slideshow";
        }
        else
        {
            _autoAdvance = true;
            int ms = DataValidation.ClampInterval(_lightboxHost?.SlideshowMs);
            _slideshowTimer.Interval = TimeSpan.FromMilliseconds(ms);
            _slideshowTimer.Start();
            SlideButton.Content = "Stop";
        }
    }
}
