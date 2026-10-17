# PERBAIKAN-OVERLAY
Waktu: 2026-10-17 17.37.23
PID: 12384

## 1. Penyebab
Dari log (dump sebelum ukuran eksplisit vs sesudah):
- [sebelum-ukuran] hwnd=0x80A20 class=HwndWrapper[MedievalDesk.Spike;;ade546dc-c83e-4bc2-b258-184cddd1cf43] title='Medieval Desk Spike' visible=True rect=228,228,1668,955
- [sebelum-ukuran] hwnd=0x50A1E class=HwndWrapper[MedievalDesk.Spike;;7429f2d2-8366-4ea6-85ee-264d31bb2157] title='Hidden Window' visible=False rect=190,190,1630,917
- [sebelum-ukuran] hwnd=0x40A3A class=HwndWrapper[MedievalDesk.Spike;;1ef595e6-326a-4864-ac9d-45cd8dd2a045] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x50A08 class=HwndWrapper[MedievalDesk.Spike;;3bdadb99-15c6-415c-83bc-f5a28f408ace] title='' visible=False rect=0,0,198,56
- [sebelum-ukuran] hwnd=0x70A0C class=HwndWrapper[MedievalDesk.Spike;;a27da343-4847-49af-a927-c3757851532b] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0xC08E0 class=HwndWrapper[MedievalDesk.Spike;;8f991f91-6926-417b-b108-2200788cf4b2] title='' visible=False rect=0,0,198,56
- [sebelum-ukuran] hwnd=0xA0A4E class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [sebelum-ukuran] hwnd=0x40A4A class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x50920 class=IME title='Default IME' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x609FC class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
- [sebelum-ukuran] WPF Actual=960x484,7 Width=960 Height=484,7 Left=152 Top=152 State=Normal IsVisible=True AllowsTransparency=True Background=#00FFFFFF SizeToContent=Manual
- [setelah-ukuran] hwnd=0x80A20 class=HwndWrapper[MedievalDesk.Spike;;ade546dc-c83e-4bc2-b258-184cddd1cf43] title='Medieval Desk Spike' visible=True rect=0,0,1920,1080
- [setelah-ukuran] hwnd=0x50A1E class=HwndWrapper[MedievalDesk.Spike;;7429f2d2-8366-4ea6-85ee-264d31bb2157] title='Hidden Window' visible=False rect=190,190,1630,917
- [setelah-ukuran] hwnd=0x40A3A class=HwndWrapper[MedievalDesk.Spike;;1ef595e6-326a-4864-ac9d-45cd8dd2a045] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x50A08 class=HwndWrapper[MedievalDesk.Spike;;3bdadb99-15c6-415c-83bc-f5a28f408ace] title='' visible=False rect=0,0,198,56
- [setelah-ukuran] hwnd=0x70A0C class=HwndWrapper[MedievalDesk.Spike;;a27da343-4847-49af-a927-c3757851532b] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0xC08E0 class=HwndWrapper[MedievalDesk.Spike;;8f991f91-6926-417b-b108-2200788cf4b2] title='' visible=False rect=0,0,198,56
- [setelah-ukuran] hwnd=0xA0A4E class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [setelah-ukuran] hwnd=0x40A4A class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x50920 class=IME title='Default IME' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x609FC class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
- [setelah-ukuran] WPF Actual=1280x720 Width=1280 Height=720 Left=0 Top=0 State=Normal IsVisible=True AllowsTransparency=True Background=#00FFFFFF SizeToContent=Manual
- [3detik] hwnd=0x130778 class=HwndWrapper[MedievalDesk.Spike;;75e44735-c926-410c-bcf2-177e12856f74] title='' visible=True rect=381,465,475,504
- [3detik] hwnd=0x80A20 class=HwndWrapper[MedievalDesk.Spike;;ade546dc-c83e-4bc2-b258-184cddd1cf43] title='Medieval Desk Spike' visible=True rect=0,0,1920,1080
- [3detik] hwnd=0x50A1E class=HwndWrapper[MedievalDesk.Spike;;7429f2d2-8366-4ea6-85ee-264d31bb2157] title='Hidden Window' visible=False rect=190,190,1630,917
- [3detik] hwnd=0x40A3A class=HwndWrapper[MedievalDesk.Spike;;1ef595e6-326a-4864-ac9d-45cd8dd2a045] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [3detik] hwnd=0x50A08 class=HwndWrapper[MedievalDesk.Spike;;3bdadb99-15c6-415c-83bc-f5a28f408ace] title='' visible=False rect=0,0,198,56
- [3detik] hwnd=0x70A0C class=HwndWrapper[MedievalDesk.Spike;;a27da343-4847-49af-a927-c3757851532b] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [3detik] hwnd=0xC08E0 class=HwndWrapper[MedievalDesk.Spike;;8f991f91-6926-417b-b108-2200788cf4b2] title='' visible=False rect=0,0,198,56
- [3detik] hwnd=0x40A4A class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [3detik] hwnd=0x50920 class=IME title='Default IME' visible=False rect=0,0,0,0
- [3detik] hwnd=0xA0A4E class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [3detik] hwnd=0x609FC class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
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
