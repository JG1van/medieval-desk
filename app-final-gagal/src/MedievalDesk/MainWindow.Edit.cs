using Geometry = MedievalDesk.Core.Geometry;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MedievalDesk.Core;

namespace MedievalDesk;

public partial class MainWindow
{
    private void SetEditMode(bool enabled)
    {
        _editMode = enabled;
        _userHidden = false;
        _selecting = false;
        SelectionBox.Visibility = Visibility.Collapsed;
        RootGrid.Cursor = Cursors.Arrow;
        ControlBar.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
        EditFrame.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
        ClosePopup();
        if (enabled && !IsVisible) Show();
        ApplyEditVisuals();
        ShowStatus(enabled
            ? "Mode edit. Klik hotspot untuk atur, seret area untuk hotspot baru, Ctrl+Alt+E keluar."
            : "Mode normal.");
        Logger.Info($"Mode edit: {enabled}");
    }

    private void ApplyEditVisuals()
    {
        var accent = (Brush)FindResource("Accent");
        foreach (var view in _views.Values)
        {
            if (_editMode)
            {
                view.Border.BorderBrush = accent;
                view.Border.BorderThickness = new Thickness(1);
                view.Border.Background = new SolidColorBrush(Color.FromArgb(18, 64, 138, 113));
                view.Border.Cursor = Cursors.Hand;
                SetHotspotImage(view, hover: false);
            }
            else
            {
                view.Border.BorderBrush = Brushes.Transparent;
                view.Border.BorderThickness = new Thickness(1);
                view.Border.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
                view.Border.Cursor = Cursors.Hand;
            }
        }
    }

    private void OnEditToggleClick(object sender, RoutedEventArgs e) => SetEditMode(!_editMode);

    private void OnAddHotspotClick(object sender, RoutedEventArgs e)
    {
        if (!_editMode) SetEditMode(true);
        BeginAreaSelect();
    }

    private void BeginAreaSelect()
    {
        _selecting = true;
        _suppressClick = true;
        ClosePopup();
        SelectionBox.Visibility = Visibility.Collapsed;
        RootGrid.Cursor = Cursors.Cross;
        RootGrid.CaptureMouse();
        ShowStatus("Seret kotak pada area desktop untuk membuat hotspot baru. Esc batal.");
    }

    private void OnRootPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!_selecting) return;
        _selectStart = e.GetPosition(RootGrid);
        SelectionBox.Margin = new Thickness(_selectStart.X, _selectStart.Y, 0, 0);
        SelectionBox.Width = 0;
        SelectionBox.Height = 0;
        SelectionBox.Visibility = Visibility.Visible;
        e.Handled = true;
    }

    private void OnRootPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_selecting || e.LeftButton != MouseButtonState.Pressed) return;
        Point now = e.GetPosition(RootGrid);
        double x = Math.Min(now.X, _selectStart.X);
        double y = Math.Min(now.Y, _selectStart.Y);
        SelectionBox.Margin = new Thickness(x, y, 0, 0);
        SelectionBox.Width = Math.Abs(now.X - _selectStart.X);
        SelectionBox.Height = Math.Abs(now.Y - _selectStart.Y);
        e.Handled = true;
    }

    private void OnRootPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_selecting) return;
        _selecting = false;
        RootGrid.ReleaseMouseCapture();
        RootGrid.Cursor = Cursors.Arrow;
        e.Handled = true;

        Point end = e.GetPosition(RootGrid);
        double x = Math.Min(end.X, _selectStart.X);
        double y = Math.Min(end.Y, _selectStart.Y);
        double w = Math.Abs(end.X - _selectStart.X);
        double h = Math.Abs(end.Y - _selectStart.Y);
        SelectionBox.Visibility = Visibility.Collapsed;

        if (w < 12 || h < 12)
        {
            ShowStatus("Area terlalu kecil, dibatalkan.", true);
            _suppressClick = false;
            return;
        }

        var box = Geometry.PixelsToBox(new Rect(x, y, w, h), _drawn);
        if (box.W < 0.5 || box.H < 0.5)
        {
            ShowStatus("Area di luar gambar, dibatalkan.", true);
            _suppressClick = false;
            return;
        }

        string id = NextHotspotId();
        var hotspot = new Hotspot
        {
            Id = id,
            Label = "Hotspot Baru",
            Kind = "configurable",
            Box = box,
            DisplayMode = "popup-list"
        };
        OpenConfigWindow(hotspot, isNew: true);
        _suppressClick = false;
    }

    private string NextHotspotId()
    {
        int n = 1;
        while (_data.Hotspots.Any(h => string.Equals(h.Id, $"gambar_{n}", StringComparison.OrdinalIgnoreCase))) n++;
        return $"gambar_{n}";
    }

    private void OpenConfigWindow(Hotspot hotspot, bool isNew = false)
    {
        var dialog = new ConfigWindow(_store, _data, Clone(hotspot), isNew) { Owner = this };
        bool? result = dialog.ShowDialog();
        if (result != true) return;

        if (dialog.DeleteRequested)
        {
            if (MessageBox.Show($"Hapus hotspot \"{hotspot.Label}\"?", "Hapus Hotspot",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            _data.Hotspots.RemoveAll(h => string.Equals(h.Id, hotspot.Id, StringComparison.OrdinalIgnoreCase));
            _store.Save(_data);
            RefreshHotspots();
            ShowStatus($"Hotspot {hotspot.Id} dihapus.");
            return;
        }

        if (dialog.Result == null) return;
        var edited = dialog.Result;

        if (_data.Hotspots.Any(h => string.Equals(h.Id, edited.Id, StringComparison.OrdinalIgnoreCase)) && isNew)
        {
            MessageBox.Show($"ID \"{edited.Id}\" sudah dipakai hotspot lain.", "Medieval Desk",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int index = _data.Hotspots.FindIndex(h => string.Equals(h.Id, hotspot.Id, StringComparison.OrdinalIgnoreCase));
        if (isNew || index < 0) _data.Hotspots.Add(edited);
        else _data.Hotspots[index] = edited;

        try
        {
            _store.Save(_data);
            RefreshHotspots();
            ShowStatus(isNew ? $"Hotspot {edited.Id} ditambahkan." : $"Hotspot {edited.Id} disimpan.");
        }
        catch (Exception ex)
        {
            Logger.Error("Gagal simpan hotspot", ex);
            MessageBox.Show(ex.Message, "Medieval Desk", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static Hotspot Clone(Hotspot hotspot)
    {
        string json = JsonSerializer.Serialize(hotspot);
        return JsonSerializer.Deserialize<Hotspot>(json) ?? new Hotspot();
    }
}
