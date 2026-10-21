# PERBAIKAN-OVERLAY
Waktu: 2026-10-21 15.24.52
PID: 8860

## 1. Penyebab
Dari log (dump sebelum ukuran eksplisit vs sesudah):
- [sebelum-ukuran] hwnd=0x803D0 class=HwndWrapper[MedievalDesk.Spike;;4795ae1a-5920-47d0-87ca-0425eae79199] title='Medieval Desk Spike' visible=True rect=190,190,1630,917
- [sebelum-ukuran] hwnd=0x3A0616 class=HwndWrapper[MedievalDesk.Spike;;ff9beaf5-550b-4e44-9a9f-5f4c1dd5150c] title='Hidden Window' visible=False rect=152,152,1592,879
- [sebelum-ukuran] hwnd=0x34094C class=HwndWrapper[MedievalDesk.Spike;;4cb3d5fe-081c-43b6-a43d-5044a6fdb350] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x3108F2 class=HwndWrapper[MedievalDesk.Spike;;4a6905b1-be16-419a-b801-bb32ef6d6093] title='' visible=False rect=0,0,198,56
- [sebelum-ukuran] hwnd=0x3107FA class=HwndWrapper[MedievalDesk.Spike;;af28bbf9-c92b-4fa7-8fd6-a7a287db8ee3] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x3B079A class=HwndWrapper[MedievalDesk.Spike;;ed49c5a4-7adf-4fc2-ab56-9de72f9762e7] title='' visible=False rect=0,0,198,56
- [sebelum-ukuran] hwnd=0xE0790 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [sebelum-ukuran] hwnd=0x1E020E class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x4A0846 class=IME title='Default IME' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x170AE2 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
- [sebelum-ukuran] WPF Actual=960x484,7 Width=960 Height=484,7 Left=126,7 Top=126,7 State=Normal IsVisible=True AllowsTransparency=True Background=#00FFFFFF SizeToContent=Manual
- [setelah-ukuran] hwnd=0x803D0 class=HwndWrapper[MedievalDesk.Spike;;4795ae1a-5920-47d0-87ca-0425eae79199] title='Medieval Desk Spike' visible=True rect=0,0,1920,1080
- [setelah-ukuran] hwnd=0x3A0616 class=HwndWrapper[MedievalDesk.Spike;;ff9beaf5-550b-4e44-9a9f-5f4c1dd5150c] title='Hidden Window' visible=False rect=152,152,1592,879
- [setelah-ukuran] hwnd=0x34094C class=HwndWrapper[MedievalDesk.Spike;;4cb3d5fe-081c-43b6-a43d-5044a6fdb350] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x3108F2 class=HwndWrapper[MedievalDesk.Spike;;4a6905b1-be16-419a-b801-bb32ef6d6093] title='' visible=False rect=0,0,198,56
- [setelah-ukuran] hwnd=0x3107FA class=HwndWrapper[MedievalDesk.Spike;;af28bbf9-c92b-4fa7-8fd6-a7a287db8ee3] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x3B079A class=HwndWrapper[MedievalDesk.Spike;;ed49c5a4-7adf-4fc2-ab56-9de72f9762e7] title='' visible=False rect=0,0,198,56
- [setelah-ukuran] hwnd=0xE0790 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [setelah-ukuran] hwnd=0x1E020E class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x4A0846 class=IME title='Default IME' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x170AE2 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
- [setelah-ukuran] WPF Actual=1280x720 Width=1280 Height=720 Left=0 Top=0 State=Normal IsVisible=True AllowsTransparency=True Background=#00FFFFFF SizeToContent=Manual
- [3detik] hwnd=0x3D0888 class=HwndWrapper[MedievalDesk.Spike;;fdfddfe1-81a7-4642-9732-a43217b0a50c] title='' visible=True rect=1269,551,1330,590
- [3detik] hwnd=0x803D0 class=HwndWrapper[MedievalDesk.Spike;;4795ae1a-5920-47d0-87ca-0425eae79199] title='Medieval Desk Spike' visible=True rect=0,0,1920,1080
- [3detik] hwnd=0x3A0616 class=HwndWrapper[MedievalDesk.Spike;;ff9beaf5-550b-4e44-9a9f-5f4c1dd5150c] title='Hidden Window' visible=False rect=152,152,1592,879
- [3detik] hwnd=0x34094C class=HwndWrapper[MedievalDesk.Spike;;4cb3d5fe-081c-43b6-a43d-5044a6fdb350] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [3detik] hwnd=0x3108F2 class=HwndWrapper[MedievalDesk.Spike;;4a6905b1-be16-419a-b801-bb32ef6d6093] title='' visible=False rect=0,0,198,56
- [3detik] hwnd=0x3107FA class=HwndWrapper[MedievalDesk.Spike;;af28bbf9-c92b-4fa7-8fd6-a7a287db8ee3] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [3detik] hwnd=0x3B079A class=HwndWrapper[MedievalDesk.Spike;;ed49c5a4-7adf-4fc2-ab56-9de72f9762e7] title='' visible=False rect=0,0,198,56
- [3detik] hwnd=0x1E020E class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [3detik] hwnd=0x4A0846 class=IME title='Default IME' visible=False rect=0,0,0,0
- [3detik] hwnd=0xE0790 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [3detik] hwnd=0x170AE2 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
- [3detik] WPF Actual=1280x720 Width=1280 Height=720 Left=0 Top=0 State=Normal IsVisible=True AllowsTransparency=True Background=#00FFFFFF SizeToContent=Manual

## 2. Perubahan
- Native\Win32.cs: tambah EnumWindows, GetWindowThreadProcessId, IsWindowVisible, GetWindowRect, GetClassName, GetWindowText, WindowFromPoint, struct RECT/POINT (untuk dump + bukti).
- MainWindow.xaml.cs: OnLoaded set Left/Top/Width/Height eksplisit sebelum Show; ResetSize + SizeChanged; Content=Grid(Transparent) berisi Canvas eksplisit; DumpWindows; RunTests.
- App.xaml.cs: wallpaper asli dibaca dari %AppData%\MedievalDesk\original-wallpaper.txt (sekali) atau docs\SPIKE-A.md.

## 3. Hasil tes
- T1: LULUS (IsVisible=True rect=0,0,1920,1080 target=0,0,1920,1080)
- T2: LULUS (area kosong tembus + titik hotspot kena)
- T3: LULUS (popupVisible=True title='Task' label='Task')
- T4: LULUS (popup setelah Esc null=True)
- T5: LULUS (HideVisible=False ShowVisible=True)
- T6: LULUS (mati(hit=False,opacity=0)=True hidup(hit=True,opacity=1)=True)

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
