# PERBAIKAN-OVERLAY
Waktu: 2026-10-17 20.12.09
PID: 9780

## 1. Penyebab
Dari log (dump sebelum ukuran eksplisit vs sesudah):
- [sebelum-ukuran] hwnd=0x3F071A class=HwndWrapper[MedievalDesk.Spike;;542c0d87-339f-4f42-b370-d5cce39078a9] title='Medieval Desk Spike' visible=True rect=114,114,1554,841
- [sebelum-ukuran] hwnd=0x1407EE class=HwndWrapper[MedievalDesk.Spike;;a249d95d-0f6f-4a08-b6fc-b3a9e0e761ae] title='Hidden Window' visible=False rect=76,76,1516,803
- [sebelum-ukuran] hwnd=0x380896 class=HwndWrapper[MedievalDesk.Spike;;1b47f8f1-5cdb-4d18-bf16-de13d0fe9114] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x2807B6 class=HwndWrapper[MedievalDesk.Spike;;38673b3b-6df3-4763-9c11-d3bce21e45a4] title='' visible=False rect=0,0,198,56
- [sebelum-ukuran] hwnd=0x340A46 class=HwndWrapper[MedievalDesk.Spike;;5933b68c-384a-4f6c-992a-8910cf0a4df8] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x14098E class=HwndWrapper[MedievalDesk.Spike;;c2a230f8-0f32-42f1-8fcd-7d1a83d88a36] title='' visible=False rect=0,0,198,56
- [sebelum-ukuran] hwnd=0x110A5A class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [sebelum-ukuran] hwnd=0x1C09E4 class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x1E0666 class=IME title='Default IME' visible=False rect=0,0,0,0
- [sebelum-ukuran] hwnd=0x200A58 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
- [sebelum-ukuran] WPF Actual=960x484,7 Width=960 Height=484,7 Left=76 Top=76 State=Normal IsVisible=True AllowsTransparency=True Background=#00FFFFFF SizeToContent=Manual
- [setelah-ukuran] hwnd=0x3F071A class=HwndWrapper[MedievalDesk.Spike;;542c0d87-339f-4f42-b370-d5cce39078a9] title='Medieval Desk Spike' visible=True rect=0,0,1920,1080
- [setelah-ukuran] hwnd=0x1407EE class=HwndWrapper[MedievalDesk.Spike;;a249d95d-0f6f-4a08-b6fc-b3a9e0e761ae] title='Hidden Window' visible=False rect=76,76,1516,803
- [setelah-ukuran] hwnd=0x380896 class=HwndWrapper[MedievalDesk.Spike;;1b47f8f1-5cdb-4d18-bf16-de13d0fe9114] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x2807B6 class=HwndWrapper[MedievalDesk.Spike;;38673b3b-6df3-4763-9c11-d3bce21e45a4] title='' visible=False rect=0,0,198,56
- [setelah-ukuran] hwnd=0x340A46 class=HwndWrapper[MedievalDesk.Spike;;5933b68c-384a-4f6c-992a-8910cf0a4df8] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x14098E class=HwndWrapper[MedievalDesk.Spike;;c2a230f8-0f32-42f1-8fcd-7d1a83d88a36] title='' visible=False rect=0,0,198,56
- [setelah-ukuran] hwnd=0x110A5A class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [setelah-ukuran] hwnd=0x1C09E4 class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x1E0666 class=IME title='Default IME' visible=False rect=0,0,0,0
- [setelah-ukuran] hwnd=0x200A58 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
- [setelah-ukuran] WPF Actual=1280x720 Width=1280 Height=720 Left=0 Top=0 State=Normal IsVisible=True AllowsTransparency=True Background=#00FFFFFF SizeToContent=Manual
- [3detik] hwnd=0x3F071A class=HwndWrapper[MedievalDesk.Spike;;542c0d87-339f-4f42-b370-d5cce39078a9] title='Medieval Desk Spike' visible=True rect=0,0,1920,1080
- [3detik] hwnd=0x1407EE class=HwndWrapper[MedievalDesk.Spike;;a249d95d-0f6f-4a08-b6fc-b3a9e0e761ae] title='Hidden Window' visible=False rect=76,76,1516,803
- [3detik] hwnd=0x380896 class=HwndWrapper[MedievalDesk.Spike;;1b47f8f1-5cdb-4d18-bf16-de13d0fe9114] title='MediaContextNotificationWindow' visible=False rect=0,0,0,0
- [3detik] hwnd=0x2807B6 class=HwndWrapper[MedievalDesk.Spike;;38673b3b-6df3-4763-9c11-d3bce21e45a4] title='' visible=False rect=0,0,198,56
- [3detik] hwnd=0x340A46 class=HwndWrapper[MedievalDesk.Spike;;5933b68c-384a-4f6c-992a-8910cf0a4df8] title='SystemResourceNotifyWindow' visible=False rect=0,0,0,0
- [3detik] hwnd=0x14098E class=HwndWrapper[MedievalDesk.Spike;;c2a230f8-0f32-42f1-8fcd-7d1a83d88a36] title='' visible=False rect=0,0,198,56
- [3detik] hwnd=0x110A5A class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=200,200,400,400
- [3detik] hwnd=0x1C09E4 class=MSCTFIME UI title='MSCTFIME UI' visible=False rect=0,0,0,0
- [3detik] hwnd=0x1E0666 class=IME title='Default IME' visible=False rect=0,0,0,0
- [3detik] hwnd=0x200A58 class=CiceroUIWndFrame title='CiceroUIWndFrame' visible=False rect=0,0,0,0
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
