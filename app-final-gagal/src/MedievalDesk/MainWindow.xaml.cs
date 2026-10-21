using Geometry = MedievalDesk.Core.Geometry;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MedievalDesk.Core;

namespace MedievalDesk;

public partial class MainWindow : Window
{
    private const int HotkeyToggle = 1;
    private const int HotkeyEdit = 2;
    private const int HotkeyQuit = 3;

    private readonly DataStore _store = new();
    private readonly Dictionary<string, HotspotView> _views = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, BitmapSource?> _bitmapCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly DispatcherTimer _foregroundTimer = null!;
    private readonly DispatcherTimer _statusTimer = null!;
    private readonly DispatcherTimer _slideshowTimer = null!;

    private DeskData _data = new();
    private IntPtr _hwnd;
    private HwndSource? _source;
    private bool _hotkeysOk;
    private bool _editMode;
    private bool _userHidden;
    private bool _selecting;
    private bool _suppressClick;
    private Point _selectStart;
    private Rect _drawn;
    private string? _selectedId;
    private Hotspot? _lightboxHost;
    private readonly List<string> _lightboxPaths = new();
    private int _lightboxIndex;
    private bool _autoAdvance;

    public MainWindow()
    {
        InitializeComponent();

        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;
        Left = 0;
        Top = 0;

        ThemeManager.Apply(_store, ThemeManager.LoadPreference(_store));

        try
        {
            var (data, warning) = _store.LoadOrSeed();
            _data = data;
            if (warning != null)
                Loaded += (_, _) => ShowStatus(warning, true);
        }
        catch (Exception ex)
        {
            Logger.Error("Gagal memuat data", ex);
            MessageBox.Show(ex.Message, "Medieval Desk", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current?.Shutdown(1);
            return;
        }

        _foregroundTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _foregroundTimer.Tick += OnForegroundTick;
        _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _statusTimer.Tick += (_, _) => { StatusBanner.Visibility = Visibility.Collapsed; _statusTimer.Stop(); };
        _slideshowTimer = new DispatcherTimer();
        _slideshowTimer.Tick += (_, _) => LightboxStep(1);

        Loaded += OnLoaded;
        SizeChanged += (_, _) => PositionHotspots();
        Deactivated += (_, _) =>
        {
            ClosePopup();
            if (Lightbox.Visibility == Visibility.Visible) CloseLightbox();
        };
        RootGrid.PreviewMouseLeftButtonDown += OnRootPreviewMouseDown;
        RootGrid.PreviewMouseMove += OnRootPreviewMouseMove;
        RootGrid.PreviewMouseLeftButtonUp += OnRootPreviewMouseUp;
        PreviewMouseDown += (_, e) =>
        {
            if (Popup.Visibility == Visibility.Visible && !IsWithin(e.OriginalSource as DependencyObject, Popup))
                ClosePopup();
        };
        SourceInitialized += OnSourceInitialized;
        Closed += OnClosed;

        Logger.Info("MainWindow dibuat.");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        BackgroundImage.Source = LoadBitmap(AssetPath("wallpaper_polos.png"));
        ThemeButton.Content = "Tema: " + ThemeManager.LoadPreference(_store);
        RefreshHotspots();
        _foregroundTimer.Start();
        ShowStatus(_hotkeysOk ? "Ctrl+Alt+D tampil/sembunyi · Ctrl+Alt+E edit · Esc sembunyikan" : "Hotkey gagal didaftarkan. Tutup aplikasi lain yang memakai Ctrl+Alt+D/E/Q.", !_hotkeysOk);
        Logger.Info($"Layer siap. Hotspot: {_data.Hotspots.Count}");
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        _hwnd = new WindowInteropHelper(this).Handle;
        _source = HwndSource.FromHwnd(_hwnd);
        _source?.AddHook(WndProc);

        _hotkeysOk = Native.RegisterHotKey(_hwnd, HotkeyToggle, Native.MOD_CONTROL | Native.MOD_ALT, 0x44)
                     && Native.RegisterHotKey(_hwnd, HotkeyEdit, Native.MOD_CONTROL | Native.MOD_ALT, 0x45)
                     && Native.RegisterHotKey(_hwnd, HotkeyQuit, Native.MOD_CONTROL | Native.MOD_ALT, 0x51);
        if (!_hotkeysOk) Logger.Warn("RegisterHotKey gagal.");
        else Logger.Info("Hotkey terdaftar: Ctrl+Alt+D/E/Q");
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == Native.WM_NCHITTEST && !_editMode)
        {
            int lp = lParam.ToInt32();
            var screen = new Point(unchecked((short)(lp & 0xFFFF)), unchecked((short)((lp >> 16) & 0xFFFF)));
            var local = PointFromScreen(screen);
            if (RootGrid.InputHitTest(local) is null)
            {
                handled = true;
                return new IntPtr(Native.HTTRANSPARENT);
            }
            return new IntPtr(Native.HTCLIENT);
        }

        if (msg == Native.WM_HOTKEY)
        {
            switch (wParam.ToInt32())
            {
                case HotkeyToggle: ToggleVisible(); handled = true; break;
                case HotkeyEdit: SetEditMode(!_editMode); handled = true; break;
                case HotkeyQuit: Close(); handled = true; break;
            }
        }
        return IntPtr.Zero;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _foregroundTimer.Stop();
        _statusTimer.Stop();
        _slideshowTimer.Stop();
        if (_hwnd != IntPtr.Zero)
        {
            Native.UnregisterHotKey(_hwnd, HotkeyToggle);
            Native.UnregisterHotKey(_hwnd, HotkeyEdit);
            Native.UnregisterHotKey(_hwnd, HotkeyQuit);
        }
        _source?.RemoveHook(WndProc);
        Logger.Info("Aplikasi ditutup.");
    }

    private void OnForegroundTick(object? sender, EventArgs e)
    {
        if (_editMode || _userHidden || !string.IsNullOrEmpty(_selectedId)) return;

        IntPtr fg = Native.GetForegroundWindow();
        if (fg == IntPtr.Zero) return;
        if (fg == _hwnd) return;

        var sb = new System.Text.StringBuilder(256);
        Native.GetClassName(fg, sb, sb.Capacity);
        string cls = sb.ToString();

        Native.GetWindowThreadProcessId(fg, out uint pid);
        if (pid == (uint)Environment.ProcessId) return;

        bool desktop = cls is "Progman" or "WorkerW" or "SHELLDLL_DefView";
        if (desktop) ShowLayer();
        else HideLayer();
    }

    private void ShowLayer()
    {
        if (_userHidden) return;
        if (!IsVisible)
        {
            Show();
            Logger.Info("Layer tampil.");
        }
    }

    private void HideLayer()
    {
        if (IsVisible) Hide();
    }

    private void ToggleVisible()
    {
        if (IsVisible && !_userHidden)
        {
            _userHidden = true;
            ClosePopup();
            Hide();
        }
        else
        {
            _userHidden = false;
            Show();
        }
    }

    private void RefreshHotspots()
    {
        HotspotLayer.Children.Clear();
        _views.Clear();
        Popup.Visibility = Visibility.Collapsed;

        foreach (var hotspot in _data.Hotspots)
        {
            var image = new Image
            {
                Stretch = Stretch.Uniform,
                IsHitTestVisible = false,
                SnapsToDevicePixels = true
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Cursor = Cursors.Hand,
                Tag = hotspot,
                ToolTip = $"{hotspot.Label}\n{hotspot.Id}",
                AllowDrop = true,
                Child = image
            };
            border.MouseEnter += OnHotspotEnter;
            border.MouseLeave += OnHotspotLeave;
            border.MouseLeftButtonUp += OnHotspotClick;
            border.DragOver += OnHotspotDragOver;
            border.DragLeave += OnHotspotDragLeave;
            border.Drop += OnHotspotDrop;

            HotspotLayer.Children.Add(border);
            var view = new HotspotView(hotspot, border, image);
            _views[hotspot.Id] = view;
            SetHotspotImage(view, hover: false);
        }

        PositionHotspots();
        ApplyEditVisuals();
    }

    private void PositionHotspots()
    {
        if (BackgroundImage.Source is not BitmapSource bitmap) return;

        _drawn = Geometry.CoverRect(RootGrid.ActualWidth, RootGrid.ActualHeight, bitmap.PixelWidth, bitmap.PixelHeight);
        foreach (var view in _views.Values)
        {
            var rect = Geometry.BoxToPixels(view.Hotspot.Box, _drawn);
            Canvas.SetLeft(view.Border, rect.X);
            Canvas.SetTop(view.Border, rect.Y);
            view.Border.Width = rect.Width;
            view.Border.Height = rect.Height;
        }
    }

    private void OnHotspotEnter(object sender, MouseEventArgs e)
    {
        if (_editMode || _selecting) return;
        if (sender is Border { Tag: Hotspot hotspot } && _views.TryGetValue(hotspot.Id, out var view))
        {
            SetHotspotImage(view, hover: true);
            view.Border.BorderBrush = (Brush)FindResource("HoverBorder");
            view.Border.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
        }
    }

    private void OnHotspotLeave(object sender, MouseEventArgs e)
    {
        if (sender is Border { Tag: Hotspot hotspot } && _views.TryGetValue(hotspot.Id, out var view))
        {
            SetHotspotImage(view, hover: false);
            view.Border.BorderBrush = Brushes.Transparent;
            view.Border.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
        }
    }

    private void OnHotspotClick(object sender, MouseButtonEventArgs e)
    {
        if (_suppressClick)
        {
            _suppressClick = false;
            return;
        }
        if (sender is not Border { Tag: Hotspot hotspot }) return;

        if (_editMode)
        {
            _selectedId = hotspot.Id;
            OpenConfigWindow(hotspot);
            _selectedId = null;
            return;
        }

        ActivateHotspot(hotspot);
    }

    private void ActivateHotspot(Hotspot hotspot)
    {
        bool hasImages = ResolveImages(hotspot).Count > 0;
        bool noTargets = !hotspot.Groups.Any(g => g.Items.Count > 0);

        if (hasImages && (noTargets || hotspot.Kind == "fixed") && hotspot.FixedType is "photo" or "note" or null)
        {
            OpenLightbox(hotspot);
            return;
        }

        if (hotspot.Kind == "configurable" || hotspot.DisplayMode != "direct" || noTargets)
        {
            OpenPopup(hotspot);
            return;
        }

        var target = hotspot.Groups.SelectMany(g => g.Items).FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.Target));
        if (target != null) LaunchTarget(target.Target);
        else OpenPopup(hotspot);
    }

    private void ShowPopup(Hotspot hotspot, double x, double y)
    {
        PopupTitle.Text = hotspot.Label;
        PopupItems.Children.Clear();
        PopupHint.Text = $"{hotspot.Kind} · {hotspot.Id}";

        foreach (var group in hotspot.Groups)
        {
            if (hotspot.Groups.Count > 1 && !string.IsNullOrWhiteSpace(group.Name))
            {
                PopupItems.Children.Add(new TextBlock
                {
                    Text = group.Name,
                    Foreground = (Brush)FindResource("MutedInk"),
                    FontSize = 11,
                    Margin = new Thickness(2, 6, 2, 2)
                });
            }
            foreach (var item in group.Items)
            {
                var button = new Button
                {
                    Content = string.IsNullOrWhiteSpace(item.Label) ? item.Target : item.Label,
                    Style = (Style)FindResource("PopupItem"),
                    ToolTip = item.Target
                };
                button.Click += (_, _) =>
                {
                    ClosePopup();
                    LaunchTarget(item.Target);
                };
                PopupItems.Children.Add(button);
            }
        }

        if (hotspot.Groups.All(g => g.Items.Count == 0))
        {
            PopupItems.Children.Add(new TextBlock
            {
                Text = "Belum ada target. Buka Atur Hotspot untuk menambah.",
                Foreground = (Brush)FindResource("MutedInk"),
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 300,
                Margin = new Thickness(2, 4, 2, 4)
            });
        }

        int imageCount = ResolveImages(hotspot).Count;
        if (imageCount > 0)
        {
            var gallery = new Button
            {
                Content = $"Lihat gambar ({imageCount})",
                Style = (Style)FindResource("PopupItem")
            };
            gallery.Click += (_, _) => { ClosePopup(); OpenLightbox(hotspot); };
            PopupItems.Children.Add(gallery);
        }

        var config = new Button { Content = "Atur Hotspot", Style = (Style)FindResource("PopupItem") };
        config.Click += (_, _) => { ClosePopup(); OpenConfigWindow(hotspot); };
        PopupItems.Children.Add(config);

        Popup.Visibility = Visibility.Visible;
        Popup.UpdateLayout();
        double maxX = Math.Max(8, RootGrid.ActualWidth - Popup.ActualWidth - 12);
        double maxY = Math.Max(8, RootGrid.ActualHeight - Popup.ActualHeight - 12);
        Popup.Margin = new Thickness(Math.Clamp(x, 8, maxX), Math.Clamp(y, 8, maxY), 0, 0);
    }

    private void OpenPopup(Hotspot hotspot)
    {
        if (_views.TryGetValue(hotspot.Id, out var view))
        {
            double x = Canvas.GetLeft(view.Border);
            double y = Canvas.GetTop(view.Border);
            ShowPopup(hotspot, x + view.Border.ActualWidth + 8, y);
        }
        else ShowPopup(hotspot, 40, 40);
    }

    private void ClosePopup() => Popup.Visibility = Visibility.Collapsed;

    private void LaunchTarget(string? target)
    {
        if (string.IsNullOrWhiteSpace(target)) return;
        try
        {
            string resolved = Environment.ExpandEnvironmentVariables(target.Trim());
            Process.Start(new ProcessStartInfo { FileName = resolved, UseShellExecute = true });
            Logger.Info($"Buka target: {resolved}");
            HideLayer();
        }
        catch (Exception ex)
        {
            Logger.Error($"Gagal buka target {target}", ex);
            MessageBox.Show($"Tidak bisa membuka:\n{target}\n\n{ex.Message}", "Medieval Desk",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void SetHotspotImage(HotspotView view, bool hover)
    {
        string? baseName = AssetBase(view.Hotspot);
        if (baseName == null)
        {
            view.Image.Source = null;
            return;
        }
        string file = AssetPath(Path.Combine(hover ? "hover" : "diam", baseName + (hover ? "_hover.png" : "_diam.png")));
        view.Image.Source = File.Exists(file) ? LoadBitmap(file) : null;

        double scale = hover ? HoverScale(baseName) : 1.0;
        view.Image.RenderTransformOrigin = new Point(0.5, 1);
        view.Image.RenderTransform = scale == 1.0
            ? Transform.Identity
            : new ScaleTransform(scale, scale);
    }

    // Artwork hover sebagian digambar lebih kecil dari versi diam. Skala dikoreksi
    // agar ukuran objek (bukan canvas) tetap sama; anchor bawah-tengah.
    private static double HoverScale(string baseName) => baseName switch
    {
        "gagak" => 1.11,
        "scroll" => 1.20,
        _ => 1.0
    };

    internal static string? AssetBase(Hotspot hotspot) => hotspot.Id.ToLowerInvariant() switch
    {
        "rak_buku" => "bookshelf",
        "kalender" => "calendar",
        "gula" or "gulungan" => "scroll",
        "bingkai" => "frame",
        "note" or "catatan" => "note",
        "bola_kristal" or "kristal" => "crystal",
        "lemari" or "loker" => "locker",
        "perisai" => "perisai",
        "gagak" or "burung_gagak" => "gagak",
        "peti" => "peti",
        _ => hotspot.FixedType switch
        {
            "calendar" => "calendar",
            "photo" => "frame",
            "note" => "note",
            _ => null
        }
    };

    private static string AssetPath(string relative) => Path.Combine(AppContext.BaseDirectory, "Assets", relative);

    private BitmapSource? LoadBitmap(string path)
    {
        if (_bitmapCache.TryGetValue(path, out var cached)) return cached;
        BitmapSource? bitmap = null;
        try
        {
            bitmap = AssetNormalizer.Load(path);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Gagal muat gambar {path}: {ex.Message}");
        }
        _bitmapCache[path] = bitmap;
        return bitmap;
    }

    private List<string> ResolveImages(Hotspot hotspot)
    {
        var list = new List<string>();
        foreach (string relative in hotspot.Images)
        {
            string? full = ImageStore.Resolve(_store, relative);
            if (full != null) list.Add(full);
        }
        return list;
    }

    private void ShowStatus(string message, bool error = false)
    {
        StatusText.Text = message;
        StatusBanner.BorderBrush = (Brush)FindResource(error ? "Danger" : "PanelBorder");
        StatusBanner.Visibility = Visibility.Visible;
        _statusTimer.Stop();
        _statusTimer.Start();
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        if (Lightbox.Visibility == Visibility.Visible)
        {
            if (e.Key == Key.Escape) { CloseLightbox(); e.Handled = true; }
            else if (e.Key == Key.Left) { LightboxStep(-1); e.Handled = true; }
            else if (e.Key == Key.Right) { LightboxStep(1); e.Handled = true; }
            return;
        }

        if (e.Key == Key.Escape && Popup.Visibility == Visibility.Visible)
        {
            ClosePopup();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape && _selecting)
        {
            _selecting = false;
            RootGrid.ReleaseMouseCapture();
            RootGrid.Cursor = Cursors.Arrow;
            SelectionBox.Visibility = Visibility.Collapsed;
            _suppressClick = false;
            ShowStatus("Pemilihan area dibatalkan.", true);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape && !_editMode)
        {
            _userHidden = true;
            HideLayer();
            e.Handled = true;
        }
    }

    private static bool IsWithin(DependencyObject? source, DependencyObject parent)
    {
        while (source != null)
        {
            if (ReferenceEquals(source, parent)) return true;
            source = VisualTreeHelper.GetParent(source);
        }
        return false;
    }

    private sealed record HotspotView(Hotspot Hotspot, Border Border, Image Image);
}
