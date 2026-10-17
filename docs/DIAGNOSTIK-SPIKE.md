# Diagnostik Spike Gerbang B

## A. WorkerW Selection (Temuan Utama)

Sebelum diperbaiki: kondisi `if (shellView != IntPtr.Zero)` **salah**. Di mesin ini, `SHELLDLL_DefView` berada di bawah `Progman`, bukan di bawah WorkerW top-level. Akibatnya, tidak ada WorkerW yang ditemukan → `WorkerW dengan SHELLDLL_DefView tidak ditemukan - GAGAL`.

Setelah diperbaiki: kondisi `if (shellView == IntPtr.Zero)` **benar**. Memilih WorkerW tanpa `SHELLDLL_DefView` (0x7047A) adalah target yang benar untuk wallpaper layer. Divergensi terverifikasi log run `12:34:26`: `WorkerW dipilih: 0x7047A`, lalu `SetParent OK, parent sebelumnya 0x1000C`.

## B. Parenting + Sizing

- `SetParent(_hwnd, workerW)` → sukses. `GetParent` mengembalikan `0x7047A` → `samaWorkerW=True`.
- `SetWindowPos` dengan `0,0 1920x1008` → ukuran fisik benar (screen 1920×1080, DPI 150%, taskbar 72px fisik → workarea 1920×1008).
- Masih ada offset: `GetWindowRect` merekam `Left=11 Top=45` bukan `0,0`. Ini artefak WPF setelah `SetParent` + DPI scaling. `WPF IsVisible=True` meski `IsWindowVisible=False` di Win32 — ikut sifat parenting anak jendela yang tersembunyi di level Win32 tapi tetap terrender di level WPF.

## C. Kesimpulan

**B: Spike diterima lanjut.** Root cause inversi WorkerW sudah diperbaiki dan bekerja. 2 isu layout minor (offset posisi 11,45, status visible) bukan *blocker* karena wallpaper menamputi area kerja fisik dengan benar. Siap ke tahap berikut (modul DataStore/Visual/Launcher).