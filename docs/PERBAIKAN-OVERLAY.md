# PERBAIKAN-OVERLAY
Waktu: 2026-10-17 18.13.46
PID: 16280

## 1. Penyebab
Dari log (dump sebelum ukuran eksplisit vs sesudah):
- [sebelum-ukuran] hwnd=0x1404E2 class=HwndWrapper[MedievalDesk.Spike;;62392ec5-3bc3-4561-ac6c-3b1dd3ea4f73] title='Medieval Desk Spike' visible=True rect=152,152,1592,879
- [sebelum-ukuran] hwnd=0x3501FA class=HwndWrapper[MedievalDesk.Spike;;49975144-bc98-43e5-b5c5-2be56267f20d] title='Hidden Window' visible=False rect=114,114,1554,841
- [sebelum-ukuran] hwnd=0x140838 class=HwndWrapper[MedievalDesk.Spike;;fdd738c9-c254-4843-90cf-279a386097da] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x380796 class=HwndWrapper[MedievalDesk.Spike;;35d3b61b-6a3b-4ef5-b19f-c178fb9fd75b] title='' visible=False rect=0,0,198,56
- [sebelum-ukuran] hwnd=0x4085E class=HwndWrapper[MedievalDesk.Spike;;d017b092-c4fe-4b06-985a-4a14d725d928] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x22092C class=HwndWrapper[MedievalDesk.Spike;;add69746-cb21-4bac-a0ca-cb6f6138e965] title='' visible=False rect=0,0,198,56
- [sebelum-ukuran] hwnd=0x2509E8 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [sebelum-ukuran] hwnd=0x1E0920 class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0xE07EE class=IME title='Default IME' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0xD098E class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
- [sebelum-ukuran] WPF Actual=960x484,7 Width=960 Height=484,7 Left=101,3 Top=101,3 State=Normal IsVisible=True AllowsTransparency=True Background=#00FFFFFF SizeToContent=Manual
- [setelah-ukuran] hwnd=0x1404E2 class=HwndWrapper[MedievalDesk.Spike;;62392ec5-3bc3-4561-ac6c-3b1dd3ea4f73] title='Medieval Desk Spike' visible=True rect=0,0,1920,1080
- [setelah-ukuran] hwnd=0x3501FA class=HwndWrapper[MedievalDesk.Spike;;49975144-bc98-43e5-b5c5-2be56267f20d] title='Hidden Window' visible=False rect=114,114,1554,841
- [setelah-ukuran] hwnd=0x140838 class=HwndWrapper[MedievalDesk.Spike;;fdd738c9-c254-4843-90cf-279a386097da] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x380796 class=HwndWrapper[MedievalDesk.Spike;;35d3b61b-6a3b-4ef5-b19f-c178fb9fd75b] title='' visible=False rect=0,0,198,56
- [setelah-ukuran] hwnd=0x4085E class=HwndWrapper[MedievalDesk.Spike;;d017b092-c4fe-4b06-985a-4a14d725d928] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x22092C class=HwndWrapper[MedievalDesk.Spike;;add69746-cb21-4bac-a0ca-cb6f6138e965] title='' visible=False rect=0,0,198,56
- [setelah-ukuran] hwnd=0x2509E8 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [setelah-ukuran] hwnd=0x1E0920 class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0xE07EE class=IME title='Default IME' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0xD098E class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
- [setelah-ukuran] WPF Actual=1280x720 Width=1280 Height=720 Left=0 Top=0 State=Normal IsVisible=True AllowsTransparency=True Background=#00FFFFFF SizeToContent=Manual
- [3detik] hwnd=0x1404E2 class=HwndWrapper[MedievalDesk.Spike;;62392ec5-3bc3-4561-ac6c-3b1dd3ea4f73] title='Medieval Desk Spike' visible=True rect=0,0,1920,1080
- [3detik] hwnd=0x3501FA class=HwndWrapper[MedievalDesk.Spike;;49975144-bc98-43e5-b5c5-2be56267f20d] title='Hidden Window' visible=False rect=114,114,1554,841
- [3detik] hwnd=0x140838 class=HwndWrapper[MedievalDesk.Spike;;fdd738c9-c254-4843-90cf-279a386097da] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [3detik] hwnd=0x380796 class=HwndWrapper[MedievalDesk.Spike;;35d3b61b-6a3b-4ef5-b19f-c178fb9fd75b] title='' visible=False rect=0,0,198,56
- [3detik] hwnd=0x4085E class=HwndWrapper[MedievalDesk.Spike;;d017b092-c4fe-4b06-985a-4a14d725d928] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [3detik] hwnd=0x22092C class=HwndWrapper[MedievalDesk.Spike;;add69746-cb21-4bac-a0ca-cb6f6138e965] title='' visible=False rect=0,0,198,56
- [3detik] hwnd=0x1E0920 class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [3detik] hwnd=0xE07EE class=IME title='Default IME' visible=False rect=0,0,0,0
- [3detik] hwnd=0x2509E8 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [3detik] hwnd=0xD098E class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
- [3detik] WPF Actual=1280x720 Width=1280 Height=720 Left=0 Top=0 State=Normal IsVisible=True AllowsTransparency=True Background=#00FFFFFF SizeToContent=Manual

## 2. Perubahan
- Native\Win32.cs: tambah EnumWindows, GetWindowThreadProcessId, IsWindowVisible, GetWindowRect, GetClassName, GetWindowText, WindowFromPoint, struct RECT/POINT (untuk dump + bukti).
- MainWindow.xaml.cs: OnLoaded set Left/Top/Width/Height eksplisit sebelum Show; ResetSize + SizeChanged; Content=Grid(Transparent) berisi Canvas eksplisit; DumpWindows; RunTests.
- App.xaml.cs: wallpaper asli dibaca dari %AppData%\MedievalDesk\original-wallpaper.txt (sekali) atau docs\SPIKE-A.md.

## 3. Hasil tes
- T1: LULUS (IsVisible=True rect=0,0,1920,1080 target=0,0,1920,1080)
- T2: LULUS (WindowFromPoint 4 titik)
- T3: LULUS (popupVisible=True title='Rak Buku' label='Rak Buku')
- T4: LULUS (popup setelah Esc null=True)
- T5: LULUS (HideVisible=False ShowVisible=True)

STATUS: LULUS

## 4. Hotspot
- Jumlah hotspot: 10
- Dilewati (box null): (tidak ada)

## 5. Checklist manual
1. Ctrl+Alt+D menampilkan lapisan.
2. Klik rak buku memunculkan popup dengan judul dan grup.
3. Klik item membuka target.
4. Esc menutup popup, lalu lapisan.
5. Ctrl+Alt+Q keluar dan wallpaper asli kembali.
