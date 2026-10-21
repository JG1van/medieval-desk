using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.Win32;
using MedievalDesk.Core;

namespace MedievalDesk;

public sealed class ConfigWindow : Window
{
    private readonly DataStore _store;
    private readonly DeskData _data;
    private readonly Hotspot _original;
    private readonly bool _isNew;
    private readonly List<string> _images = new();

    private readonly TextBox _id = new();
    private readonly TextBox _label = new();
    private readonly ComboBox _kind = new();
    private readonly TextBox _fixedType = new();
    private readonly ComboBox _display = new();
    private readonly TextBox _x = new();
    private readonly TextBox _y = new();
    private readonly TextBox _w = new();
    private readonly TextBox _h = new();
    private readonly TextBox _groups = new()
    {
        Style = DialogUi.InputStyle,
        AcceptsReturn = true,
        Height = 110,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        FontFamily = new FontFamily("Consolas")
    };
    private readonly TextBox _slideshow = new();
    private readonly ListBox _imageList = new() { Height = 110, Margin = new Thickness(0, 0, 0, 6) };

    public Hotspot? Result { get; private set; }
    public bool DeleteRequested { get; private set; }

    public ConfigWindow(DataStore store, DeskData data, Hotspot hotspot, bool isNew)
    {
        _store = store;
        _data = data;
        _original = hotspot;
        _isNew = isNew;

        DialogUi.StyleWindow(this, isNew ? "Hotspot Baru" : $"Atur: {hotspot.Label}", 500, 760);
        MinWidth = 440;
        MinHeight = 560;

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = new TextBlock
        {
            Text = isNew ? "Tentukan detail hotspot baru." : "Ubah detail hotspot lalu Simpan.",
            Margin = new Thickness(14, 14, 14, 0),
            FontWeight = FontWeights.SemiBold
        };
        header.SetResourceReference(TextBlock.ForegroundProperty, "Ink");
        grid.Children.Add(header);

        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Padding = new Thickness(14),
            Margin = new Thickness(0, 0, 0, 0)
        };
        var form = new StackPanel();
        scroll.Content = form;
        Grid.SetRow(scroll, 1);
        grid.Children.Add(scroll);

        _id.Text = hotspot.Id;
        _id.IsReadOnly = !isNew;
        _label.Text = hotspot.Label;
        _kind.Style = DialogUi.ComboStyle;
        _display.Style = DialogUi.ComboStyle;
        _kind.ItemsSource = new[] { "configurable", "fixed" };
        _kind.SelectedItem = hotspot.Kind == "fixed" ? "fixed" : "configurable";
        _fixedType.Text = hotspot.FixedType ?? "";
        _display.ItemsSource = new[] { "direct", "popup-list", "popup-grid" };
        _display.SelectedItem = hotspot.DisplayMode is "popup-list" or "popup-grid" or "direct" ? hotspot.DisplayMode : "popup-list";
        _x.Text = hotspot.Box.X.ToString("0.##");
        _y.Text = hotspot.Box.Y.ToString("0.##");
        _w.Text = hotspot.Box.W.ToString("0.##");
        _h.Text = hotspot.Box.H.ToString("0.##");
        _slideshow.Text = hotspot.SlideshowMs?.ToString() ?? "";
        _groups.Text = GroupsToText(hotspot.Groups);
        _images.AddRange(hotspot.Images);

        AddField(form, "ID", _id);
        if (!isNew) AddNote(form, "ID tidak bisa diubah agar target lama tetap tersimpan.");
        AddField(form, "Label", _label);
        AddField(form, "Jenis", _kind);
        AddField(form, "Fixed type (opsional)", _fixedType);
        AddField(form, "Mode tampilan", _display);
        AddNote(form, "Posisi dalam persen (0-100) terhadap gambar, bukan piksel layar.");
        var boxGrid = new UniformGrid { Columns = 4 };
        boxGrid.Children.Add(BoxCell("X", _x));
        boxGrid.Children.Add(BoxCell("Y", _y));
        boxGrid.Children.Add(BoxCell("W", _w));
        boxGrid.Children.Add(BoxCell("H", _h));
        form.Children.Add(boxGrid);

        AddField(form, "Target / grup", _groups);
        AddNote(form, "Satu baris: Label | Target. Awali baris dengan [Nama Grup] untuk grup baru. Target bisa file, folder, atau URL.");

        AddField(form, "Gambar (opsional)", _imageList);
        var imageButtons = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        imageButtons.Children.Add(DialogUi.Button("Tambah File", (_, _) => AddImageFiles()));
        imageButtons.Children.Add(DialogUi.Button("Tambah Folder", (_, _) => AddImageFolder()));
        imageButtons.Children.Add(DialogUi.Button("Lihat", (_, _) => PreviewImage()));
        imageButtons.Children.Add(DialogUi.Button("Hapus", (_, _) => RemoveImage()));
        form.Children.Add(imageButtons);
        RefreshImageList();

        AddField(form, "Interval slideshow (ms)", _slideshow);
        AddNote(form, "Kosongkan untuk default 5000 ms. Batas 1000-60000 ms.");

        var footer = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(14)
        };
        if (!isNew)
        {
            var delete = DialogUi.Button("Hapus Hotspot", (_, _) => ConfirmDelete());
            delete.SetResourceReference(Control.ForegroundProperty, "Danger");
            footer.Children.Add(delete);
        }
        footer.Children.Add(DialogUi.Button("Batal", (_, _) => DialogResult = false));
        footer.Children.Add(DialogUi.Button("Simpan", (_, _) => Save()));
        Grid.SetRow(footer, 2);
        grid.Children.Add(footer);

        Content = grid;
        KeyDown += (_, e) => { if (e.Key == System.Windows.Input.Key.Escape) DialogResult = false; };
    }

    private static StackPanel BoxCell(string label, TextBox box)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 6, 0) };
        var text = new TextBlock { Text = label, Margin = new Thickness(0, 0, 0, 2) };
        text.SetResourceReference(TextBlock.ForegroundProperty, "MutedInk");
        panel.Children.Add(text);
        panel.Children.Add(box);
        return panel;
    }

    private static void AddField(Panel panel, string label, UIElement control)
    {
        var text = new TextBlock { Text = label, Margin = new Thickness(0, 8, 0, 3) };
        text.SetResourceReference(TextBlock.ForegroundProperty, "Ink");
        text.FontWeight = FontWeights.SemiBold;
        panel.Children.Add(text);
        panel.Children.Add(control);
    }

    private static void AddNote(Panel panel, string note)
    {
        var text = new TextBlock
        {
            Text = note,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 11,
            Margin = new Thickness(0, 3, 0, 2)
        };
        text.SetResourceReference(TextBlock.ForegroundProperty, "MutedInk");
        panel.Children.Add(text);
    }

    private void AddImageFiles()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Tambah gambar",
            Multiselect = true,
            Filter = "Gambar (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp"
        };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            foreach (string file in dialog.FileNames)
            {
                string relative = ImageStore.CopyInto(_store, _original.Id, file);
                if (!_images.Contains(relative)) _images.Add(relative);
            }
            RefreshImageList();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Gambar", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddImageFolder()
    {
        var dialog = new OpenFolderDialog { Title = "Pilih folder gambar" };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            int added = 0;
            foreach (string file in ImageStore.EnumerateFolder(dialog.FolderName))
            {
                string relative = ImageStore.CopyInto(_store, _original.Id, file);
                if (!_images.Contains(relative))
                {
                    _images.Add(relative);
                    added++;
                }
            }
            RefreshImageList();
            if (added == 0) MessageBox.Show("Tidak ada gambar baru di folder itu.", "Gambar", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Gambar", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RemoveImage()
    {
        if (_imageList.SelectedItem is not string relative) return;
        _images.Remove(relative);
        ImageStore.Delete(_store, relative);
        RefreshImageList();
    }

    private void PreviewImage()
    {
        if (_imageList.SelectedItem is not string relative) return;
        string? full = ImageStore.Resolve(_store, relative);
        if (full == null)
        {
            MessageBox.Show("File gambar tidak ditemukan.", "Gambar", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        new ImagePreviewWindow(full) { Owner = this }.ShowDialog();
    }

    private void RefreshImageList()
    {
        _imageList.Items.Clear();
        foreach (string image in _images) _imageList.Items.Add(image);
    }

    private void ConfirmDelete()
    {
        if (MessageBox.Show($"Hapus hotspot \"{_original.Label}\"?", "Hapus Hotspot",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        DeleteRequested = true;
        DialogResult = true;
    }

    private void Save()
    {
        string id = _id.Text.Trim();
        string label = _label.Text.Trim();
        if (string.IsNullOrWhiteSpace(id))
        {
            MessageBox.Show("ID wajib diisi.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(label))
        {
            MessageBox.Show("Label wajib diisi.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (_isNew && _data.Hotspots.Any(h => string.Equals(h.Id, id, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show($"ID \"{id}\" sudah dipakai.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!TryNumber(_x.Text, out double x) || !TryNumber(_y.Text, out double y) ||
            !TryNumber(_w.Text, out double w) || !TryNumber(_h.Text, out double h))
        {
            MessageBox.Show("X, Y, W, H harus angka.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        x = Math.Clamp(x, 0, 100);
        y = Math.Clamp(y, 0, 100);
        w = Math.Clamp(w, 0.5, 100 - x);
        h = Math.Clamp(h, 0.5, 100 - y);

        int? slideshow = null;
        if (!string.IsNullOrWhiteSpace(_slideshow.Text))
        {
            if (!int.TryParse(_slideshow.Text.Trim(), out int ms))
            {
                MessageBox.Show("Interval slideshow harus angka (ms).", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            slideshow = DataValidation.ClampInterval(ms);
        }

        var result = new Hotspot
        {
            Id = id,
            Label = label,
            Kind = _kind.SelectedItem as string ?? "configurable",
            FixedType = string.IsNullOrWhiteSpace(_fixedType.Text) ? null : _fixedType.Text.Trim(),
            DisplayMode = _display.SelectedItem as string ?? "popup-list",
            Box = new HotspotBox
            {
                X = Math.Round(x, 2),
                Y = Math.Round(y, 2),
                W = Math.Round(w, 2),
                H = Math.Round(h, 2)
            },
            Groups = ParseGroups(_groups.Text),
            Images = _images.ToList(),
            SlideshowMs = slideshow,
            Extra = _original.Extra
        };

        var probe = new DeskData
        {
            Version = 1,
            Reference = _data.Reference,
            Hotspots = [result]
        };
        var errors = DataValidation.Validate(probe);
        if (errors.Count > 0)
        {
            MessageBox.Show("Data belum valid:\n- " + string.Join("\n- ", errors), "Validasi",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Result = result;
        DialogResult = true;
    }

    private static bool TryNumber(string text, out double value)
        => double.TryParse(text.Trim(), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.CurrentCulture, out value)
           || double.TryParse(text.Trim(), System.Globalization.NumberStyles.Float,
               System.Globalization.CultureInfo.InvariantCulture, out value);

    private static string GroupsToText(List<HotspotGroup> groups)
    {
        var sb = new StringBuilder();
        bool multiple = groups.Count > 1;
        foreach (var group in groups)
        {
            if (!string.IsNullOrWhiteSpace(group.Name) && (multiple || group.Name != "Tujuan"))
                sb.AppendLine($"[{group.Name}]");
            foreach (var item in group.Items)
                sb.AppendLine($"{item.Label} | {item.Target}");
        }
        return sb.ToString().TrimEnd();
    }

    private static List<HotspotGroup> ParseGroups(string text)
    {
        var groups = new List<HotspotGroup>();
        HotspotGroup? current = null;

        foreach (string raw in text.Split('\n'))
        {
            string line = raw.Trim();
            if (line.Length == 0) continue;

            if (line.StartsWith('[') && line.EndsWith(']') && line.Length > 2)
            {
                current = new HotspotGroup { Name = line[1..^1].Trim() };
                groups.Add(current);
                continue;
            }

            current ??= new HotspotGroup { Name = "Tujuan" };
            if (!groups.Contains(current)) groups.Add(current);

            int bar = line.IndexOf('|');
            if (bar >= 0)
            {
                string label = line[..bar].Trim();
                string target = line[(bar + 1)..].Trim();
                if (target.Length > 0 || label.Length > 0)
                    current.Items.Add(new HotspotItem { Label = label, Target = target });
            }
            else
            {
                current.Items.Add(new HotspotItem { Label = "", Target = line });
            }
        }

        return groups;
    }
}
