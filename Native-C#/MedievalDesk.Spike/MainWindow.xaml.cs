using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using MedievalDesk.Spike.Native;

namespace MedievalDesk.Spike;

public partial class MainWindow : Window
{
    private const int HotkeyQuit = 1;
    private const int HotkeyOverlay = 2;
    private const int HotkeyEdit = 3;
    private const double RefW = 1376;
    private const double RefH = 768;

    private IntPtr _hwnd;
    private int _pw;
    private int _ph;
    private double _targetW;
    private double _targetH;
    private bool _resizing;

    private Grid? _grid;
    private Canvas? _canvas;
    private Border? _popup;
    private string? _popupLabel;
    private Hotspot? _firstHotspot;
    private Border? _firstBorder;

    private List<Hotspot>? _data;
    private bool _editMode;
    private Border? _configForm;
    private Hotspot? _editingHotspot;
    private ComboBox? _cmbJenis;
    private Button? _btnTambahItem;
    private StackPanel? _itemsPanel;
    private StackPanel? _urlBlock;
    private StackPanel? _tambahRow;
    private ScrollViewer? _scrollItems;
    private readonly List<EditItem> _editItems = new();

    private readonly List<string> _dump = new();
    private readonly List<string> _tests = new();
    private string _status = "GAGAL";

    public MainWindow()
    {
        InitializeComponent();

        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ShowInTaskbar = false;
        Topmost = true;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.Manual;

        Loaded += OnLoaded;
        ContentRendered += (_, _) =>
        {
            var content = Content as FrameworkElement;
            App.Log($"ContentRendered: Content Actual={content?.ActualWidth:0.#}x{content?.ActualHeight:0.#}; Window Actual={ActualWidth:0.#}x{ActualHeight:0.#}");
        };
        SizeChanged += OnSizeChanged;
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                HandleEscape();
            }
            else if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                ToggleEditMode();
            }
        };
        Closed += (_, _) =>
        {
            if (_hwnd != IntPtr.Zero)
            {
                Win32.UnregisterHotKey(_hwnd, HotkeyQuit);
                Win32.UnregisterHotKey(_hwnd, HotkeyOverlay);
                Win32.UnregisterHotKey(_hwnd, HotkeyEdit);
            }
            App.Log("Window Closed");
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _pw = Win32.GetSystemMetrics(Win32.SM_CXSCREEN);
        _ph = Win32.GetSystemMetrics(Win32.SM_CYSCREEN);
        double m11 = 1, m22 = 1;
        var src = PresentationSource.FromVisual(this);
        if (src?.CompositionTarget != null)
        {
            m11 = src.CompositionTarget.TransformToDevice.M11;
            m22 = src.CompositionTarget.TransformToDevice.M22;
        }
        _targetW = _pw / m11;
        _targetH = _ph / m22;
        App.Log($"Layar fisik: {_pw}x{_ph}; target DIP: {_targetW:0.##}x{_targetH:0.##}; skala M11={m11:0.####} M22={m22:0.####}");

        // ==== LANGKAH 1: bukti penyebab (tampilkan dulu tanpa ukuran eksplisit) ====
        Show();
        DumpWindows("sebelum-ukuran");

        // ==== LANGKAH 2: ukuran eksplisit ====
        Left = 0;
        Top = 0;
        Width = _targetW;
        Height = _targetH;
        ResetSize("setelah-ukuran");
        DumpWindows("setelah-ukuran");

        _hwnd = new WindowInteropHelper(this).Handle;
        App.Log($"Handle jendela: 0x{_hwnd.ToInt64():X}");
        HwndSource.FromHwnd(_hwnd)?.AddHook(WndProc);

        bool hkQuit = Win32.RegisterHotKey(_hwnd, HotkeyQuit, Win32.MOD_CONTROL | Win32.MOD_ALT, 0x51);
        bool hkOverlay = Win32.RegisterHotKey(_hwnd, HotkeyOverlay, Win32.MOD_CONTROL | Win32.MOD_ALT, 0x44);
        bool hkEdit = Win32.RegisterHotKey(_hwnd, HotkeyEdit, Win32.MOD_CONTROL | Win32.MOD_ALT, 0x45);
        App.Log($"RegisterHotKey Ctrl+Alt+Q: {hkQuit}");
        App.Log($"RegisterHotKey Ctrl+Alt+D: {hkOverlay}");
        App.Log($"RegisterHotKey Ctrl+Alt+E: {hkEdit}");

        var (data, skipped) = LoadData();
        if (data == null)
        {
            App.Log("DATA TIDAK DITEMUKAN");
            _tests.Add("T1: GAGAL (data tidak ditemukan)");
            WriteReport(hkQuit, hkOverlay, 0, skipped);
            return;
        }
        App.Log($"DATA: {data.Count} hotspot terbaca; dilewati: {skipped.Count}");
        BuildOverlay(data);

        var room = Path.Combine(DataDir!, "Prototipe2", "assets", "wallpaper.png");
        bool wallOk = ApplyWallpaper(room);
        App.Log($"WALLPAPER PASANG: {room}");

        var t3s = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        t3s.Tick += (_, _) => { t3s.Stop(); DumpWindows("3detik"); };
        t3s.Start();

        var tTests = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(3500) };
        tTests.Tick += (_, _) =>
        {
            tTests.Stop();
            RunTests(hkQuit, hkOverlay, data.Count, wallOk, skipped);
        };
        tTests.Start();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_resizing) return;
        if (Math.Abs(Width - _targetW) > 0.5 || Math.Abs(Height - _targetH) > 0.5)
        {
            App.Log($"SizeChanged ke {Width:0.#}x{Height:0.#}; set ulang ke {_targetW:0.#}x{_targetH:0.#}");
            _resizing = true;
            Width = _targetW;
            Height = _targetH;
            _resizing = false;
        }
        if (_canvas != null)
        {
            _canvas.Width = _targetW;
            _canvas.Height = _targetH;
        }
    }

    private void ResetSize(string tag)
    {
        _resizing = true;
        Width = _targetW;
        Height = _targetH;
        _resizing = false;
        App.Log($"[{tag}] Width={Width:0.#} Height={Height:0.#} Left={Left:0.#} Top={Top:0.#}");
    }

    private void DumpWindows(string tag)
    {
        App.Log($"=== DUMP {tag} ===");
        uint myPid = (uint)Process.GetCurrentProcess().Id;
        Win32.EnumWindows((h, _) =>
        {
            Win32.GetWindowThreadProcessId(h, out uint pid);
            if (pid != myPid) return true;
            var cls = new StringBuilder(256);
            Win32.GetClassName(h, cls, cls.Capacity);
            var title = new StringBuilder(256);
            Win32.GetWindowText(h, title, title.Capacity);
            Win32.GetWindowRect(h, out var r);
            bool vis = Win32.IsWindowVisible(h);
            string line = $"[{tag}] hwnd=0x{h.ToInt64():X} class={cls} title='{title}' visible={vis} rect={r.Left},{r.Top},{r.Right},{r.Bottom}";
            App.Log(line);
            _dump.Add(line);
            return true;
        }, IntPtr.Zero);

        string wpf = $"[{tag}] WPF Actual={ActualWidth:0.#}x{ActualHeight:0.#} Width={Width:0.#} Height={Height:0.#} Left={Left:0.#} Top={Top:0.#} State={WindowState} IsVisible={IsVisible} AllowsTransparency={AllowsTransparency} Background={Background} SizeToContent={SizeToContent}";
        App.Log(wpf);
        _dump.Add(wpf);
    }

    private bool CheckFourPoints(string tag)
    {
        var pts = new[] { (10, 10), (_pw / 2, _ph / 2), (_pw - 10, _ph - 10), (_pw / 2, _ph - 20) };
        bool all = true;
        foreach (var (x, y) in pts)
        {
            var w = Win32.WindowFromPoint(new Win32.POINT { X = x, Y = y });
            var cls = new StringBuilder(256);
            Win32.GetClassName(w, cls, cls.Capacity);
            bool ok = w == _hwnd;
            if (!ok) all = false;
            App.Log($"[{tag}] WindowFromPoint ({x},{y}) -> hwnd=0x{w.ToInt64():X} class={cls} match={ok}");
        }
        return all;
    }

    private void RunTests(bool hkQuit, bool hkOverlay, int count, bool wallOk, List<string> skipped)
    {
        // T1
        Win32.GetWindowRect(_hwnd, out var r);
        bool full = r.Left == 0 && r.Top == 0 && r.Right == _pw && r.Bottom == _ph;
        bool t1 = IsVisible && full;
        LogTest("T1", t1, $"IsVisible={IsVisible} rect={r.Left},{r.Top},{r.Right},{r.Bottom} target=0,0,{_pw},{_ph}");

        // T2
        bool t2 = CheckFourPoints("T2");
        LogTest("T2", t2, "WindowFromPoint 4 titik");

        // T3
        bool t3 = false;
        string n3 = "tidak ada hotspot";
        if (_firstHotspot != null && _firstBorder != null)
        {
            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonUpEvent
            };
            _firstBorder.RaiseEvent(args);
            bool vis = _popup != null && _popup.IsVisible;
            bool titleOk = _popupLabel == _firstHotspot.Label;
            t3 = vis && titleOk;
            n3 = $"popupVisible={vis} title='{_popupLabel}' label='{_firstHotspot.Label}'";
        }
        LogTest("T3", t3, n3);

        // T4
        bool t4;
        string n4;
        if (_popup != null)
        {
            HandleEscape();
            t4 = _popup == null;
            n4 = $"popup setelah Esc null={_popup == null}";
        }
        else
        {
            t4 = false;
            n4 = "popup tidak ada sebelum T4";
        }
        LogTest("T4", t4, n4);

        // T5
        Hide();
        bool afterHide = !IsVisible;
        Show();
        bool afterShow = IsVisible;
        bool t5 = afterHide && afterShow;
        LogTest("T5", t5, $"HideVisible={!afterHide} ShowVisible={afterShow}");

        _status = (t1 && t2 && t3 && t4 && t5) ? "LULUS" : "GAGAL";
        App.Log($"STATUS: {_status}");

        Hide();
        App.Log("Tes selesai, overlay disembunyikan");

        WriteReport(hkQuit, hkOverlay, count, skipped);
    }

    private void LogTest(string id, bool pass, string note)
    {
        string line = $"{id}: {(pass ? "LULUS" : "GAGAL")} ({note})";
        _tests.Add(line);
        App.Log(line);
    }

    private static string? DataDir;

    private static (List<Hotspot>? Data, List<string> Skipped) LoadData()
    {
        var skipped = new List<string>();
        DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
        {
            string candidate = Path.Combine(dir.FullName, "Prototipe2", "data", "medieval-desk.json");
            if (!File.Exists(candidate)) continue;
            DataDir = dir.FullName;
            return (ParseHotspots(candidate, skipped), skipped);
        }
        return (null, skipped);
    }

    private static List<Hotspot> ParseHotspots(string path, List<string> skipped)
    {
        var list = new List<Hotspot>();
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        if (!doc.RootElement.TryGetProperty("hotspots", out var hs) || hs.ValueKind != JsonValueKind.Array)
            return list;

        foreach (var h in hs.EnumerateArray())
        {
            string id = h.TryGetProperty("id", out var e1) ? e1.GetString() ?? "" : "";
            string label = h.TryGetProperty("label", out var e2) ? e2.GetString() ?? "" : "";
            string kind = h.TryGetProperty("kind", out var e3) ? e3.GetString() ?? "" : "";
            string displayMode = h.TryGetProperty("displayMode", out var e4) && e4.ValueKind == JsonValueKind.String
                ? e4.GetString() ?? "" : "";

            if (!h.TryGetProperty("box", out var box) || box.ValueKind != JsonValueKind.Object)
            {
                skipped.Add(string.IsNullOrWhiteSpace(label) ? id : label);
                continue;
            }

            var spot = new Hotspot
            {
                Id = id,
                Label = label,
                Kind = kind,
                DisplayMode = displayMode,
                X = box.GetProperty("x").GetDouble(),
                Y = box.GetProperty("y").GetDouble(),
                Bw = box.GetProperty("w").GetDouble(),
                Bh = box.GetProperty("h").GetDouble()
            };

            if (h.TryGetProperty("groups", out var g) && g.ValueKind == JsonValueKind.Array)
            {
                spot.Groups = new List<Group>();
                foreach (var gi in g.EnumerateArray())
                {
                    var grp = new Group
                    {
                        Name = gi.TryGetProperty("name", out var nm) && nm.ValueKind == JsonValueKind.String
                            ? nm.GetString() ?? ""
                            : ""
                    };
                    if (gi.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var it in items.EnumerateArray())
                        {
                            grp.Items.Add(new Item
                            {
                                Label = it.TryGetProperty("label", out var lb) && lb.ValueKind == JsonValueKind.String ? lb.GetString() ?? "" : "",
                                Target = it.TryGetProperty("target", out var tg) && tg.ValueKind == JsonValueKind.String ? tg.GetString() ?? "" : ""
                            });
                        }
                    }
                    spot.Groups.Add(grp);
                }
            }
            list.Add(spot);
        }
        return list;
    }

    private void BuildOverlay(List<Hotspot> data)
    {
        _data = data;
        double W = ActualWidth > 1 ? ActualWidth : _targetW;
        double H = ActualHeight > 1 ? ActualHeight : _targetH;

        // alpha=1 (bukan 0) agar permukaan layered window tetap kena hit-test WindowFromPoint
        var hitBrush = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
        hitBrush.Freeze();

        _canvas = new Canvas { Background = hitBrush, Width = W, Height = H };
        _canvas.MouseLeftButtonDown += (_, _) =>
        {
            if (_popup == null) return;
            _canvas.Children.Remove(_popup);
            _popup = null;
            App.Log("Klik latar: popup ditutup");
        };

        double s = Math.Max(W / RefW, H / RefH);
        double ox = (W - RefW * s) / 2;
        double oy = (H - RefH * s) / 2;
        var highlight = new SolidColorBrush(Color.FromRgb(0xB0, 0xE4, 0xCC));

        foreach (var hs in data)
        {
            double left = ox + hs.X / 100.0 * RefW * s;
            double top = oy + hs.Y / 100.0 * RefH * s;
            double width = hs.Bw / 100.0 * RefW * s;
            double height = hs.Bh / 100.0 * RefH * s;

            var border = new Border
            {
                Width = width,
                Height = height,
                Background = Brushes.Transparent,
                Cursor = Cursors.Hand,
                ToolTip = hs.Label
            };
            var spot = hs;
            border.MouseEnter += (_, _) =>
            {
                border.BorderBrush = highlight;
                border.BorderThickness = new Thickness(2);
            };
            border.MouseLeave += (_, _) => border.BorderThickness = new Thickness(0);
            border.MouseLeftButtonUp += (_, e) =>
            {
                e.Handled = true;
                if (_editMode) { ShowConfigForm(spot); return; }
                if (EffectiveMode(spot) == "direct") OpenTarget(FirstTarget(spot));
                else ShowPopup(spot);
            };
            Canvas.SetLeft(border, left);
            Canvas.SetTop(border, top);
            _canvas.Children.Add(border);

            if (_firstHotspot == null && EffectiveMode(hs) != "direct")
            {
                _firstHotspot = hs;
                _firstBorder = border;
            }
        }

        _grid = new Grid { Background = hitBrush };
        _grid.SetBinding(WidthProperty, new Binding("ActualWidth") { Source = this });
        _grid.SetBinding(HeightProperty, new Binding("ActualHeight") { Source = this });
        _grid.Children.Add(_canvas);
        Content = _grid;
        App.Log($"Overlay dibuat: {data.Count} area klik, kanvas {W:0.#}x{H:0.#} DIP (s={s:0.####}, ox={ox:0.#}, oy={oy:0.#})");
    }

    private void ShowPopup(Hotspot hs)
    {
        if (_canvas == null) return;
        if (_popup != null)
        {
            _canvas.Children.Remove(_popup);
            _popup = null;
        }

        double W = ActualWidth > 1 ? ActualWidth : _targetW;
        double H = ActualHeight > 1 ? ActualHeight : _targetH;
        var titleBrush = new SolidColorBrush(Color.FromRgb(0x3A, 0x2F, 0x1F));
        bool grid = EffectiveMode(hs) == "popup-grid";

        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = hs.Label,
            FontFamily = new FontFamily("Grenze Gotisch, Georgia"),
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Foreground = titleBrush
        });

        if (hs.Groups == null || hs.Groups.Count == 0)
        {
            stack.Children.Add(new TextBlock { Text = "(tidak ada item)", Foreground = titleBrush, Margin = new Thickness(0, 6, 0, 0) });
        }
        else if (grid)
        {
            var ug = new UniformGrid { Columns = 3, Margin = new Thickness(0, 10, 0, 0) };
            foreach (var g in hs.Groups)
                foreach (var it in g.Items)
                {
                    var enter = PopupButton(it.Label, it.Target);
                    enter.Margin = new Thickness(3);
                    enter.Padding = new Thickness(8, 12, 8, 12);
                    ug.Children.Add(enter);
                }
            stack.Children.Add(ug);
        }
        else
        {
            foreach (var g in hs.Groups)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = g.Name,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = titleBrush,
                    Margin = new Thickness(0, 10, 0, 2)
                });
                foreach (var it in g.Items)
                    stack.Children.Add(PopupButton(it.Label, it.Target));
            }
        }

        double width = 360;
        var border = new Border
        {
            Width = width,
            Background = new SolidColorBrush(Color.FromRgb(0xFD, 0xF5, 0xE6)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xC9, 0xB2, 0x8A)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Child = stack
        };
        Canvas.SetLeft(border, (W - width) / 2);
        Canvas.SetTop(border, (H - 260) / 2);
        _popup = border;
        _popupLabel = hs.Label;
        _canvas.Children.Add(border);
        App.Log($"POPUP ({(grid ? "grid" : "list")}): {hs.Label}");
    }

    private Button PopupButton(string label, string target)
    {
        var btn = new Button
        {
            Content = string.IsNullOrWhiteSpace(label) ? "(tanpa label)" : label,
            Style = (Style)FindResource("PopupItem")
        };
        btn.Click += (_, _) => OpenTarget(target);
        return btn;
    }

    private static string EffectiveMode(Hotspot hs) =>
        !string.IsNullOrEmpty(hs.DisplayMode) ? hs.DisplayMode
        : (hs.Kind == "configurable" ? "popup-list" : "direct");

    private static string? FirstTarget(Hotspot hs) =>
        hs.Groups is { Count: > 0 } && hs.Groups[0].Items.Count > 0
            ? hs.Groups[0].Items[0].Target : null;

    private static void OpenTarget(string? target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            App.Log("ITEM: target kosong, tidak membuka apa pun");
            return;
        }
        try
        {
            Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
            App.Log($"ITEM: buka {target}");
        }
        catch (Exception ex)
        {
            App.Log($"ITEM GAGAL buka {target}: {ex.Message}");
        }
    }

    private void ToggleEditMode()
    {
        _editMode = !_editMode;
        App.Log($"EDIT MODE: {_editMode}");
    }

    private void CloseConfigForm()
    {
        if (_canvas != null && _configForm != null)
            _canvas.Children.Remove(_configForm);
        _configForm = null;
        _editingHotspot = null;
        _cmbJenis = null;
        _btnTambahItem = null;
        _itemsPanel = null;
        _urlBlock = null;
        _tambahRow = null;
        _scrollItems = null;
        _editItems.Clear();
    }

    private void RefreshItemsList()
    {
        string jenis = _cmbJenis?.SelectedItem as string ?? "LANGSUNG";
        bool isPopup = jenis == "POP UP 3X3" || jenis == "POP UP LIST";

        if (_urlBlock != null) _urlBlock.Visibility = isPopup ? Visibility.Collapsed : Visibility.Visible;
        if (_tambahRow != null) _tambahRow.Visibility = isPopup ? Visibility.Visible : Visibility.Collapsed;
        if (_scrollItems != null) _scrollItems.Visibility = isPopup ? Visibility.Visible : Visibility.Collapsed;

        if (_itemsPanel == null) return;
        _itemsPanel.Children.Clear();

        foreach (var item in _editItems)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            var tbLabel = new TextBox { Width = 100, Padding = new Thickness(4), Text = item.Label };
            tbLabel.TextChanged += (_, _) => item.Label = tbLabel.Text;
            var tbTarget = new TextBox { Width = 170, Margin = new Thickness(4, 0, 0, 0), Padding = new Thickness(4), Text = item.Target };
            tbTarget.TextChanged += (_, _) => item.Target = tbTarget.Text;
            var btnDel = new Button { Content = "X", Width = 26, Margin = new Thickness(4, 0, 0, 0), Padding = new Thickness(0), Cursor = Cursors.Hand };
            btnDel.Click += (_, _) => { _editItems.Remove(item); RefreshItemsList(); };
            row.Children.Add(tbLabel);
            row.Children.Add(tbTarget);
            row.Children.Add(btnDel);
            _itemsPanel.Children.Add(row);
        }
    }

    private void ShowConfigForm(Hotspot hs)
    {
        if (_canvas == null) return;
        CloseConfigForm();

        _editingHotspot = hs;
        _editItems.Clear();

        string defaultUrl = "";
        if (hs.Groups != null && hs.Groups.Count > 0)
        {
            foreach (var g in hs.Groups)
                foreach (var it in g.Items)
                    _editItems.Add(new EditItem { Label = it.Label, Target = it.Target });
            if (hs.Groups[0].Items.Count > 0)
                defaultUrl = hs.Groups[0].Items[0].Target;
        }

        double W = ActualWidth > 1 ? ActualWidth : _targetW;
        double H = ActualHeight > 1 ? ActualHeight : _targetH;
        var ink = new SolidColorBrush(Color.FromRgb(0x3A, 0x2F, 0x1F));

        var stack = new StackPanel { Margin = new Thickness(10) };

        stack.Children.Add(new TextBlock { Text = "@NAMA", FontWeight = FontWeights.SemiBold, Foreground = ink, Margin = new Thickness(0, 0, 0, 2) });
        var txtNama = new TextBox { Text = hs.Label, Padding = new Thickness(6, 4, 6, 4), Margin = new Thickness(0, 0, 0, 8) };
        stack.Children.Add(txtNama);

        stack.Children.Add(new TextBlock { Text = "@JENIS (LANGSUNG, POP UP 3X3, POP UP LIST)", FontWeight = FontWeights.SemiBold, Foreground = ink, Margin = new Thickness(0, 0, 0, 2) });
        _cmbJenis = new ComboBox { Margin = new Thickness(0, 0, 0, 8) };
        _cmbJenis.Items.Add("LANGSUNG");
        _cmbJenis.Items.Add("POP UP 3X3");
        _cmbJenis.Items.Add("POP UP LIST");
        string mode = hs.DisplayMode;
        if (string.IsNullOrEmpty(mode)) mode = hs.Kind == "configurable" ? "popup-list" : "direct";
        _cmbJenis.SelectedIndex = mode == "popup-grid" ? 1 : mode == "popup-list" ? 2 : 0;
        _cmbJenis.SelectionChanged += (_, _) => RefreshItemsList();
        stack.Children.Add(_cmbJenis);

        _urlBlock = new StackPanel();
        _urlBlock.Children.Add(new TextBlock { Text = "@URL", FontWeight = FontWeights.SemiBold, Foreground = ink, Margin = new Thickness(0, 0, 0, 2) });
        var txtUrl = new TextBox { Text = defaultUrl, Padding = new Thickness(6, 4, 6, 4), Margin = new Thickness(0, 0, 0, 8) };
        _urlBlock.Children.Add(txtUrl);
        stack.Children.Add(_urlBlock);

        _tambahRow = new StackPanel();
        _tambahRow.Children.Add(new TextBlock { Text = "@URL ITEM (isi URL tiap baris)", FontWeight = FontWeights.SemiBold, Foreground = ink, Margin = new Thickness(0, 0, 0, 2) });
        _btnTambahItem = new Button { Content = "| TAMBAH |", Padding = new Thickness(10, 4, 10, 4), Cursor = Cursors.Hand, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 8) };
        _btnTambahItem.Click += (_, _) => { _editItems.Add(new EditItem()); RefreshItemsList(); };
        _tambahRow.Children.Add(_btnTambahItem);
        stack.Children.Add(_tambahRow);

        _itemsPanel = new StackPanel();
        _scrollItems = new ScrollViewer
        {
            MaxHeight = 220,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _itemsPanel
        };
        stack.Children.Add(_scrollItems);

        var rowBtn = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 8, 0, 0) };
        var btnBatal = new Button { Content = "BATAL", Padding = new Thickness(12, 4, 12, 4), Margin = new Thickness(0, 0, 8, 0), Cursor = Cursors.Hand };
        btnBatal.Click += (_, _) => CloseConfigForm();
        var btnSimpan = new Button { Content = "SIMPAN", Padding = new Thickness(12, 4, 12, 4), Cursor = Cursors.Hand, Background = new SolidColorBrush(Color.FromRgb(0x40, 0x8A, 0x71)), Foreground = Brushes.White };
        btnSimpan.Click += (_, _) => SaveConfig(hs, txtNama.Text, txtUrl.Text);
        rowBtn.Children.Add(btnBatal);
        rowBtn.Children.Add(btnSimpan);
        stack.Children.Add(rowBtn);

        const double formW = 360;
        var form = new Border
        {
            Width = formW,
            MaxHeight = H - 80,
            Background = new SolidColorBrush(Color.FromRgb(0xFD, 0xF5, 0xE6)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xC9, 0xB2, 0x8A)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Child = stack
        };
        Canvas.SetLeft(form, (W - formW) / 2);
        Canvas.SetTop(form, 40);
        _configForm = form;
        _canvas.Children.Add(form);
        RefreshItemsList();
        App.Log($"FORM KONFIGURASI: {hs.Label}");
    }

    private void SaveConfig(Hotspot hs, string nama, string urlDefault)
    {
        string j = _cmbJenis?.SelectedItem?.ToString() ?? "LANGSUNG";
        string mode = j == "POP UP 3X3" ? "popup-grid" : j == "POP UP LIST" ? "popup-list" : "direct";

        hs.Label = nama.Trim();
        hs.DisplayMode = mode;
        hs.Kind = mode == "direct" ? hs.Kind : "configurable";

        if (hs.Groups == null) hs.Groups = new List<Group>();
        if (hs.Groups.Count == 0) hs.Groups.Add(new Group { Name = hs.Label });

        var keep = hs.Groups[0];
        hs.Groups.Clear();
        keep.Name = hs.Label;
        keep.Items.Clear();

        if (j == "LANGSUNG")
        {
            if (!string.IsNullOrWhiteSpace(urlDefault))
                keep.Items.Add(new Item { Label = hs.Label, Target = urlDefault.Trim() });
        }
        else
        {
            foreach (var ei in _editItems)
            {
                if (string.IsNullOrWhiteSpace(ei.Target)) continue;
                keep.Items.Add(new Item
                {
                    Label = string.IsNullOrWhiteSpace(ei.Label) ? Path.GetFileName(ei.Target) : ei.Label.Trim(),
                    Target = ei.Target.Trim()
                });
            }
        }
        hs.Groups.Add(keep);

        SaveData();
        CloseConfigForm();
        App.Log("CONFIG DISIMPAN");
    }

    private void SaveData()
    {
        if (_data == null) return;
        try
        {
            string path = GetDataPath();
            JsonNode root = File.Exists(path)
                ? JsonNode.Parse(File.ReadAllText(path)) ?? new JsonObject()
                : new JsonObject();

            if (root is not JsonObject obj) { App.Log("SIMPAN: root bukan objek"); return; }
            if (obj["hotspots"] is not JsonArray arr) { arr = new JsonArray(); obj["hotspots"] = arr; }

            foreach (var hs in _data)
            {
                JsonObject? node = null;
                foreach (var el in arr)
                {
                    if (el is JsonObject o && o["id"]?.GetValue<string>() == hs.Id) { node = o; break; }
                }
                if (node == null)
                {
                    node = new JsonObject { ["id"] = hs.Id };
                    arr.Add(node);
                }
                node["label"] = hs.Label;
                node["kind"] = hs.Kind;
                node["displayMode"] = hs.DisplayMode;
                if (hs.Groups == null)
                {
                    node["groups"] = null;
                }
                else
                {
                    var gArr = new JsonArray();
                    foreach (var g in hs.Groups)
                    {
                        var gObj = new JsonObject { ["name"] = g.Name };
                        var iArr = new JsonArray();
                        foreach (var it in g.Items)
                            iArr.Add(new JsonObject { ["label"] = it.Label, ["target"] = it.Target });
                        gObj["items"] = iArr;
                        gArr.Add(gObj);
                    }
                    node["groups"] = gArr;
                }
            }

            File.WriteAllText(path, obj.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            App.Log($"DATA TERSIMPAN: {path}");
        }
        catch (Exception ex)
        {
            App.Log($"GAGAL SIMPAN: {ex.Message}");
        }
    }

    private static string GetDataPath()
    {
        string basePath = DataDir ?? AppContext.BaseDirectory;
        return Path.Combine(basePath, "Prototipe2", "data", "medieval-desk.json");
    }

    private void HandleEscape()
    {
        if (_configForm != null)
        {
            CloseConfigForm();
            App.Log("Esc: form konfigurasi ditutup");
            return;
        }
        if (_popup != null)
        {
            _canvas?.Children.Remove(_popup);
            _popup = null;
            App.Log("Esc: popup ditutup");
        }
        else
        {
            Hide();
            App.Log("Esc: lapisan disembunyikan");
        }
    }

    private static string ReadWallpaper()
    {
        var sb = new StringBuilder(260);
        Win32.SystemParametersInfo(Win32.SPI_GETDESKWALLPAPER, 260, sb, 0);
        return sb.ToString();
    }

    private static bool ApplyWallpaper(string room)
    {
        if (!File.Exists(room)) return false;
        var sb = new StringBuilder(room);
        Win32.SystemParametersInfo(Win32.SPI_SETDESKWALLPAPER, 0, sb,
            Win32.SPIF_UPDATEINIFILE | Win32.SPIF_SENDCHANGE);
        string read = ReadWallpaper();
        return string.Equals(read, room, StringComparison.OrdinalIgnoreCase)
               || string.Equals(read, Path.GetFullPath(room), StringComparison.OrdinalIgnoreCase);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != Win32.WM_HOTKEY) return IntPtr.Zero;

        int id = wParam.ToInt32();
        if (id == HotkeyQuit)
        {
            App.Log("KELUAR via Ctrl+Alt+Q");
            handled = true;
            App.RestoreWallpaper();
            Application.Current.Shutdown();
        }
        else if (id == HotkeyOverlay)
        {
            handled = true;
            if (IsVisible)
            {
                Hide();
                App.Log("OVERLAY: sembunyi");
            }
            else
            {
                Show();
                Activate();
                App.Log("OVERLAY: tampil");
            }
        }
        else if (id == HotkeyEdit)
        {
            handled = true;
            if (!IsVisible) Show();
            Activate();
            ToggleEditMode();
        }
        return IntPtr.Zero;
    }

    private void WriteReport(bool hkQuit, bool hkOverlay, int count, List<string> skipped)
    {
        try
        {
            DirectoryInfo? root = new DirectoryInfo(AppContext.BaseDirectory);
            string? docs = null;
            for (int i = 0; i < 8 && root != null; i++, root = root.Parent)
            {
                if (File.Exists(Path.Combine(root.FullName, "Native-C#", "MedievalDesk.sln")))
                {
                    docs = Path.Combine(root.FullName, "docs");
                    break;
                }
            }
            if (docs == null)
            {
                App.Log("docs dir tidak ditemukan, lewati tulis laporan");
                return;
            }
            Directory.CreateDirectory(docs);

            int pid = Process.GetCurrentProcess().Id;
            var sb = new StringBuilder();
            sb.AppendLine("# PERBAIKAN-OVERLAY");
            sb.AppendLine($"Waktu: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"PID: {pid}");
            sb.AppendLine();
            sb.AppendLine("## 1. Penyebab");
            sb.AppendLine("Dari log (dump sebelum ukuran eksplisit vs sesudah):");
            foreach (var line in _dump) sb.AppendLine("- " + line);
            sb.AppendLine();
            sb.AppendLine("## 2. Perubahan");
            sb.AppendLine("- Native\\Win32.cs: tambah EnumWindows, GetWindowThreadProcessId, IsWindowVisible, GetWindowRect, GetClassName, GetWindowText, WindowFromPoint, struct RECT/POINT (untuk dump + bukti).");
            sb.AppendLine("- MainWindow.xaml.cs: OnLoaded set Left/Top/Width/Height eksplisit sebelum Show; ResetSize + SizeChanged; Content=Grid(Transparent) berisi Canvas eksplisit; DumpWindows; RunTests.");
            sb.AppendLine("- App.xaml.cs: wallpaper asli dibaca dari %AppData%\\MedievalDesk\\original-wallpaper.txt (sekali) atau docs\\SPIKE-A.md.");
            sb.AppendLine();
            sb.AppendLine("## 3. Hasil tes");
            foreach (var line in _tests) sb.AppendLine("- " + line);
            sb.AppendLine();
            sb.AppendLine($"STATUS: {_status}");
            sb.AppendLine();
            sb.AppendLine("## 4. Hotspot");
            sb.AppendLine($"- Jumlah hotspot: {count}");
            sb.AppendLine($"- Dilewati (box null): {(skipped.Count == 0 ? "(tidak ada)" : string.Join(", ", skipped))}");
            sb.AppendLine();
            sb.AppendLine("## 5. Checklist manual");
            sb.AppendLine("1. Ctrl+Alt+D menampilkan lapisan.");
            sb.AppendLine("2. Klik rak buku memunculkan popup dengan judul dan grup.");
            sb.AppendLine("3. Klik item membuka target.");
            sb.AppendLine("4. Esc menutup popup, lalu lapisan.");
            sb.AppendLine("5. Ctrl+Alt+Q keluar dan wallpaper asli kembali.");

            File.WriteAllText(Path.Combine(docs, "PERBAIKAN-OVERLAY.md"), sb.ToString());
            App.Log($"Laporan ditulis: {Path.Combine(docs, "PERBAIKAN-OVERLAY.md")}");
        }
        catch (Exception ex)
        {
            App.Log($"EXCEPTION WriteReport: {ex.Message}");
        }
    }

    private sealed class Hotspot
    {
        public string Id = "";
        public string Label = "";
        public string Kind = "";
        public string DisplayMode = "";
        public double X, Y, Bw, Bh;
        public List<Group>? Groups;
    }

    private sealed class Group
    {
        public string Name = "";
        public List<Item> Items = new();
    }

    private sealed class Item
    {
        public string Label = "";
        public string Target = "";
    }

    private sealed class EditItem
    {
        public string Label { get; set; } = "";
        public string Target { get; set; } = "";
    }
}
