# PERBAIKAN-OVERLAY
Waktu: 2026-10-17 19.14.40
PID: 13452

## 1. Penyebab
Dari log (dump sebelum ukuran eksplisit vs sesudah):
- [sebelum-ukuran] hwnd=0x70870 class=HwndWrapper[MedievalDesk.Spike;;4028fc67-8865-4147-8137-c1b615699cb0] title='Medieval Desk Spike' visible=True rect=152,152,1592,879
- [sebelum-ukuran] hwnd=0x7085E class=HwndWrapper[MedievalDesk.Spike;;5e630f73-cf87-46b5-a5d1-58c31ff3af66] title='Hidden Window' visible=False rect=114,114,1554,841
- [sebelum-ukuran] hwnd=0x2408BA class=HwndWrapper[MedievalDesk.Spike;;0ae9d872-d278-4c3c-9c06-1c7bd11c55d9] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x1107EE class=HwndWrapper[MedievalDesk.Spike;;be9528bf-57bf-4c94-a7f2-0ee21e120f81] title='' visible=False rect=0,0,198,56
- [sebelum-ukuran] hwnd=0x170838 class=HwndWrapper[MedievalDesk.Spike;;2256b352-fdcc-4a22-bff9-256b12abf911] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x3C0796 class=HwndWrapper[MedievalDesk.Spike;;348935c0-e361-4af5-9221-d4ba57496345] title='' visible=False rect=0,0,198,56
- [sebelum-ukuran] hwnd=0x370A02 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [sebelum-ukuran] hwnd=0x10098E class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0xA0824 class=IME title='Default IME' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x2809E8 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
- [sebelum-ukuran] WPF Actual=960x484,7 Width=960 Height=484,7 Left=101,3 Top=101,3 State=Normal IsVisible=True AllowsTransparency=True Background=#00FFFFFF SizeToContent=Manual
- [setelah-ukuran] hwnd=0x70870 class=HwndWrapper[MedievalDesk.Spike;;4028fc67-8865-4147-8137-c1b615699cb0] title='Medieval Desk Spike' visible=True rect=0,0,1920,1080
- [setelah-ukuran] hwnd=0x7085E class=HwndWrapper[MedievalDesk.Spike;;5e630f73-cf87-46b5-a5d1-58c31ff3af66] title='Hidden Window' visible=False rect=114,114,1554,841
- [setelah-ukuran] hwnd=0x2408BA class=HwndWrapper[MedievalDesk.Spike;;0ae9d872-d278-4c3c-9c06-1c7bd11c55d9] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x1107EE class=HwndWrapper[MedievalDesk.Spike;;be9528bf-57bf-4c94-a7f2-0ee21e120f81] title='' visible=False rect=0,0,198,56
- [setelah-ukuran] hwnd=0x170838 class=HwndWrapper[MedievalDesk.Spike;;2256b352-fdcc-4a22-bff9-256b12abf911] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x3C0796 class=HwndWrapper[MedievalDesk.Spike;;348935c0-e361-4af5-9221-d4ba57496345] title='' visible=False rect=0,0,198,56
- [setelah-ukuran] hwnd=0x370A02 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [setelah-ukuran] hwnd=0x10098E class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0xA0824 class=IME title='Default IME' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x2809E8 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
- [setelah-ukuran] WPF Actual=1280x720 Width=1280 Height=720 Left=0 Top=0 State=Normal IsVisible=True AllowsTransparency=True Background=#00FFFFFF SizeToContent=Manual
- [3detik] hwnd=0x230382 class=HwndWrapper[MedievalDesk.Spike;;7fd1e833-08d0-4789-84fe-1822de71bbb9] title='' visible=True rect=233,872,327,911
- [3detik] hwnd=0x70870 class=HwndWrapper[MedievalDesk.Spike;;4028fc67-8865-4147-8137-c1b615699cb0] title='Medieval Desk Spike' visible=True rect=0,0,1920,1080
- [3detik] hwnd=0x7085E class=HwndWrapper[MedievalDesk.Spike;;5e630f73-cf87-46b5-a5d1-58c31ff3af66] title='Hidden Window' visible=False rect=114,114,1554,841
- [3detik] hwnd=0x2408BA class=HwndWrapper[MedievalDesk.Spike;;0ae9d872-d278-4c3c-9c06-1c7bd11c55d9] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [3detik] hwnd=0x1107EE class=HwndWrapper[MedievalDesk.Spike;;be9528bf-57bf-4c94-a7f2-0ee21e120f81] title='' visible=False rect=0,0,198,56
- [3detik] hwnd=0x170838 class=HwndWrapper[MedievalDesk.Spike;;2256b352-fdcc-4a22-bff9-256b12abf911] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [3detik] hwnd=0x3C0796 class=HwndWrapper[MedievalDesk.Spike;;348935c0-e361-4af5-9221-d4ba57496345] title='' visible=False rect=0,0,198,56
- [3detik] hwnd=0x10098E class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [3detik] hwnd=0xA0824 class=IME title='Default IME' visible=False rect=0,0,0,0
- [3detik] hwnd=0x370A02 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [3detik] hwnd=0x2809E8 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
- [3detik] WPF Actual=1280x720 Width=1280 Height=720 Left=0 Top=0 State=Normal IsVisible=True AllowsTransparency=True Background=#00FFFFFF SizeToContent=Manual

## 2. Perubahan
- Native\Win32.cs: tambah EnumWindows, GetWindowThreadProcessId, IsWindowVisible, GetWindowRect, GetClassName, GetWindowText, WindowFromPoint, struct RECT/POINT (untuk dump + bukti).
- MainWindow.xaml.cs: OnLoaded set Left/Top/Width/Height eksplisit sebelum Show; ResetSize + SizeChanged; Content=Grid(Transparent) berisi Canvas eksplisit; DumpWindows; RunTests.
- App.xaml.cs: wallpaper asli dibaca dari %AppData%\MedievalDesk\original-wallpaper.txt (sekali) atau docs\SPIKE-A.md.

## 3. Hasil tes
- T1: LULUS (IsVisible=True rect=0,0,1920,1080 target=0,0,1920,1080)
- T2: LULUS (WindowFromPoint 4 titik)
- T3: LULUS (popupVisible=True title='Gulungan' label='Gulungan')
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
