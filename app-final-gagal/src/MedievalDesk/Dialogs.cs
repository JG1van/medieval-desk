using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MedievalDesk.Core;

namespace MedievalDesk;

internal static class DialogUi
{
    public static Brush Brush(string key) => (Brush)Application.Current.Resources[key];
    public static Style ButtonStyle => (Style)Application.Current.Resources["ToolButton"];
    public static Style InputStyle => (Style)Application.Current.Resources["Input"];
    public static Style ComboStyle => (Style)Application.Current.Resources["InputCombo"];

    public static Button Button(string text, RoutedEventHandler click)
    {
        var button = new Button { Content = text, Style = ButtonStyle };
        button.Click += click;
        return button;
    }

    public static void StyleWindow(Window window, string title, double width, double height)
    {
        window.Title = title;
        window.Width = width;
        window.Height = height;
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        window.ResizeMode = ResizeMode.CanResizeWithGrip;
        window.SetResourceReference(Control.BackgroundProperty, "PanelBg");
        window.SetResourceReference(Control.ForegroundProperty, "Ink");
    }
}

public sealed class ProfilesWindow : Window
{
    private readonly ListBox _list;
    private readonly TextBox _name;

    public string? Action { get; private set; }
    public string? ProfileName { get; private set; }

    public ProfilesWindow(IReadOnlyList<string> profiles)
    {
        DialogUi.StyleWindow(this, "Profil Layout", 420, 460);
        MinWidth = 380;
        MinHeight = 400;

        var grid = new Grid { Margin = new Thickness(14) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var info = new TextBlock
        {
            Text = "Pilih profil lalu Muat. Simpan memakai nama di kotak bawah.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        };
        info.SetResourceReference(TextBlock.ForegroundProperty, "MutedInk");
        grid.Children.Add(info);

        _list = new ListBox { Margin = new Thickness(0, 0, 0, 10) };
        _list.SetResourceReference(Control.BackgroundProperty, "PanelBg");
        _list.SetResourceReference(Control.ForegroundProperty, "Ink");
        _list.SetResourceReference(Control.BorderBrushProperty, "PanelBorder");
        foreach (string profile in profiles) _list.Items.Add(profile);
        _list.SelectionChanged += (_, _) =>
        {
            if (_list.SelectedItem is string selected) _name!.Text = selected;
        };
        _list.MouseDoubleClick += (_, _) => Do("load");
        Grid.SetRow(_list, 1);
        grid.Children.Add(_list);

        var nameRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        var nameLabel = new TextBlock { Text = "Nama profil:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
        _name = new TextBox { Width = 230, Style = DialogUi.InputStyle };
        nameRow.Children.Add(nameLabel);
        nameRow.Children.Add(_name);
        Grid.SetRow(nameRow, 2);
        grid.Children.Add(nameRow);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(DialogUi.Button("Muat", (_, _) => Do("load")));
        buttons.Children.Add(DialogUi.Button("Simpan", (_, _) => Do("save")));
        buttons.Children.Add(DialogUi.Button("Hapus", (_, _) => Do("delete")));
        buttons.Children.Add(DialogUi.Button("Batal", (_, _) => { DialogResult = false; }));
        Grid.SetRow(buttons, 3);
        grid.Children.Add(buttons);

        Content = grid;
        Loaded += (_, _) => _name.Focus();
    }

    private void Do(string action)
    {
        string name = _name.Text.Trim();
        if (string.IsNullOrEmpty(name) && _list.SelectedItem is string selected) name = selected;

        if (action is "save" or "delete" or "load")
        {
            if (!ProfileStore.IsValidName(name))
            {
                MessageBox.Show("Nama profil hanya huruf, angka, spasi, _ atau - (maks 40).", "Profil",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        Action = action;
        ProfileName = name;
        DialogResult = true;
    }
}

public sealed class ImportPreviewWindow : Window
{
    public ImportPreviewWindow(ImportPreview preview, string path)
    {
        DialogUi.StyleWindow(this, "Pratinjau Impor", 520, 520);
        MinWidth = 460;
        MinHeight = 420;

        var grid = new Grid { Margin = new Thickness(14) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = new TextBlock
        {
            Text = $"File: {path}\n{preview.Added.Count} hotspot baru, {preview.Updated.Count} diperbarui. Layout saat ini dicadangkan otomatis.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 8)
        };
        header.SetResourceReference(TextBlock.ForegroundProperty, "Ink");
        grid.Children.Add(header);

        var summary = new TextBlock
        {
            Text = preview.HasChanges ? "Perubahan akan diterapkan setelah menekan Impor." : "Tidak ada perubahan hotspot.",
            Margin = new Thickness(0, 0, 0, 8),
            FontWeight = FontWeights.SemiBold
        };
        summary.SetResourceReference(TextBlock.ForegroundProperty, preview.HasChanges ? "Accent" : "MutedInk");
        Grid.SetRow(summary, 1);
        grid.Children.Add(summary);

        var list = new ListBox { Margin = new Thickness(0, 0, 0, 10), MaxHeight = 320 };
        list.SetResourceReference(Control.BackgroundProperty, "PanelBg");
        list.SetResourceReference(Control.ForegroundProperty, "Ink");
        list.SetResourceReference(Control.BorderBrushProperty, "PanelBorder");
        foreach (string item in preview.Added) list.Items.Add("+ " + item);
        foreach (string item in preview.Updated) list.Items.Add("~ " + item);
        foreach (string item in preview.Warnings) list.Items.Add("! " + item);
        Grid.SetRow(list, 2);
        grid.Children.Add(list);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var import = DialogUi.Button("Impor", (_, _) => DialogResult = true);
        import.IsEnabled = preview.HasChanges;
        buttons.Children.Add(import);
        buttons.Children.Add(DialogUi.Button("Batal", (_, _) => DialogResult = false));
        Grid.SetRow(buttons, 3);
        grid.Children.Add(buttons);

        Content = grid;
    }
}

public sealed class ImagePreviewWindow : Window
{
    public ImagePreviewWindow(string path)
    {
        DialogUi.StyleWindow(this, Path.GetFileName(path), 760, 560);
        MinWidth = 420;
        MinHeight = 320;

        var grid = new Grid { Margin = new Thickness(10) };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var image = new Image { Stretch = Stretch.Uniform };
        RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            image.Source = bitmap;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal memuat gambar:\n{ex.Message}", "Gambar", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        grid.Children.Add(image);

        var close = DialogUi.Button("Tutup", (_, _) => Close());
        close.HorizontalAlignment = HorizontalAlignment.Right;
        Grid.SetRow(close, 1);
        grid.Children.Add(close);

        Content = grid;
        KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
    }
}
