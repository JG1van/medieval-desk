# Implementation Log - Medieval Desk 1.0.0

> Catatan historis proses pembuatan rilis 1.0.0. Struktur folder saat ini sudah direorganized
> (lihat `docs/REORG-LOG.md`): `Final/` → `app-final-gagal/`, `Native-C#/` → `app-desktop/`,
> `medieval-desk-native/` → `prototipe-web/`, dan `Prototipe/`/`Prototipe2/` dihapus.

## Ruang lingkup

Implementasi `FINAL-PLAN.md` untuk aplikasi WPF native `MedievalDesk`, termasuk build, publish, verifikasi otomatis, dokumentasi, dan paket rilis.

- Repo: `JG1van/medieval-desk`
- Branch: `main`
- Baseline: `f19895d`
- Tanggal: 21 Oktober 2026

Pada saat penulisan, folder lama `Prototipe/`, `Prototipe2/`, `Native-C#/`, dan `medieval-desk-native/` belum diubah. Reorg dan sanitasi dilakukan setelahnya (lihat `docs/REORG-LOG.md`).

## Lingkungan

```text
.NET SDK          8.0.426
WindowsDesktop    8.0.32
Runtime target    win-x64, self-contained
OS                Windows 10.0.26200
```

Tidak ada NuGet baru. Tidak memakai WinForms.

## Struktur implementasi

```text
src/MedievalDesk/
  MedievalDesk.csproj
  app.manifest
  App.xaml, App.xaml.cs
  MainWindow.xaml, MainWindow.xaml.cs
  MainWindow.Edit.cs
  MainWindow.Features.cs
  ConfigWindow.cs
  Dialogs.cs
  SelfTest.cs
  Themes/Parchment.xaml, Themes/Night.xaml
  Core/Models.cs, Logger.cs, DataStore.cs, Stores.cs,
       ThemeManager.cs, Native.cs
```

## Keputusan teknis

### Layer desktop-only

Window transparan, tanpa border, tanpa taskbar, `Topmost=false`, ukuran mengikuti monitor utama. Polling foreground 300 ms menyembunyikan layer saat aplikasi lain aktif; proses sendiri diabaikan. Mode edit memblokir auto-hide.

Alasan: kebutuhan overlay desktop, bukan always-on-top. Wallpaper dan ikon desktop tetap terlihat.

### Click-through

`WM_NCHITTEST` mengembalikan `HTTRANSPARENT` bila `RootGrid.InputHitTest` bernilai null; selain itu `HTCLIENT`. Memakai `RootGrid`, bukan window, karena window transparan tetap menerima hit-test.

Diverifikasi dengan `SendMessage(hwnd, WM_NCHITTEST, ...)`:

- Titik kosong (5,5): `-1` (`HTTRANSPARENT`)
- 10 pusat hotspot: `1` (`HTCLIENT`)

### Geometri persen

Hotspot disimpan 0-100% terhadap gambar referensi, lalu dipetakan ke piksel layar dengan `CoverRect` agar tidak distorsi. Posisi tetap proporsional saat resolusi berubah.

### Data

- Root: `%AppData%\MedievalDesk`
- Data: `data\medieval-desk.json`, backup `data\medieval-desk.bak.json`
- Profil: `profiles\`
- Gambar: `images\<id>\`
- Preferensi: `preferences.json`
- Log: `logs\medieval-desk.log`, rotasi 1 MB
- Batas baca: 4 MB
- Seed: `Assets\sample-layout.json`, hanya jika data utama tidak ada

Penyimpanan memakai file sementara + `File.Move`, backup sebelum menimpa. Schema v1 divalidasi; field tak dikenal dipertahankan.

### Aset

Pemetaan utama berdasarkan ID hotspot, fallback `fixedType` (`calendar`, `photo`, `note`). Aset diam/hover berada di `Assets\diam` dan `Assets\hover`.

### Tanpa perubahan wallpaper

Aplikasi hanya menampilkan gambar latar transparan. Tidak ada pemanggilan API wallpaper. Utilitas pemulihan disediakan terpisah.

### Mode tes

`--self-test` menjalankan 13 pemeriksaan pada direktori sementara, menulis `self-test.txt`, dan mengembalikan exit code 0/1. Tidak menyentuh data pengguna.

## Insiden dan perbaikan

### 1. Korupsi `Core/Native.cs`

File sempat rusak saat penulisan. Konstanta dan signature dipulihkan, lalu build diverifikasi.

### 2. Warning nullable

Empat warning build:

- Tiga field `DispatcherTimer` di `MainWindow.xaml.cs`
- `_name` di `Dialogs.cs`

Diperbaiki dengan `= null!` dan `_name!.Text`. Hasil: 0 warning, 0 error.

### 3. Aset hilang dari publish single-file

Gejala: exe publish 192 MB, folder `Assets` hanya berisi sebagian file; aset besar tidak tersedia saat runtime.

Akar masalah: `PublishSingleFile=true` membundel setiap item `ResolvedFileToPublish` yang tidak memiliki `ExcludeFromSingleFile=true`, termasuk `Content`. Aset kecil tetap tersalin, aset besar masuk ke exe.

Perbaikan: semua item `Content` di `MedievalDesk.csproj` diberi:

```xml
CopyToOutputDirectory="PreserveNewest"
CopyToPublishDirectory="PreserveNewest"
ExcludeFromSingleFile="true"
```

Hasil publish ulang:

```text
MedievalDesk.exe  161.807.845 byte
Assets            24 file
```

### 4. `Esc` tidak membatalkan pemilihan area

Status menyatakan "Esc batal", tetapi handler hanya menangani lightbox, popup, dan hide. Ditambahkan cabang `_selecting` untuk melepas mouse capture, menyembunyikan kotak seleksi, dan mereset state. Build ulang dan publish ulang dilakukan setelah perbaikan.

### 5. Konflik hotkey saat pengujian

`MedievalDesk.Spike` (PID 13848) masih berjalan dan memegang `Ctrl+Alt+D/E/Q`, sehingga build baru menampilkan `RegisterHotKey gagal`. Ini bukan defect build; pengujian hotkey menunggu Spike ditutup.

## Bukti verifikasi

### Build

```powershell
dotnet build .\src\MedievalDesk\MedievalDesk.csproj -c Release
```

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

### Publish

```powershell
.\tools\build-and-copy.ps1
```

```text
Publish selesai: ...\Final\installers\win-x64
MedievalDesk.exe: 161.807.845 byte
Assets: 24 file
```

### Self-test folder publish bersih

Salinan `installers\win-x64` ke direktori temp baru, lalu:

```text
exit=0
LULUS: 13; GAGAL: 0
```

Pemeriksaan: seed/validasi, ID duplikat, box, versi schema, traversal, backup, merge import, cover rect, clamp slideshow, nama profil, salin/dedupe/hapus gambar, simpan/muat profil, preferensi tema.

### Self-test dari zip rilis

Zip diekstrak ke direktori temp baru:

```text
exit=0
LULUS: 13; GAGAL: 0
assets=24
files=40
```

### Click-through

`WM_NCHITTEST` otomatis: area kosong `HTTRANSPARENT`, 10 hotspot `HTCLIENT`.

### Hotkey, data live, dan shutdown

Setelah `MedievalDesk.Spike` tidak lagi berjalan, build publish dijalankan dengan data pengguna asli:

```text
Hotkey terdaftar: Ctrl+Alt+D/E/Q
Layer siap. Hotspot: 10
Aplikasi ditutup.
```

Tidak ada warning `RegisterHotKey gagal`. Penutupan lewat `WM_CLOSE` menghasilkan exit bersih dan log penutupan. Penekanan kombinasi hotkey secara fisik belum diuji otomatis; registrasi dan handler sudah aktif.

### Utilitas wallpaper

```powershell
.\tools\restore-wallpaper.ps1 -ImagePath .\medieval-desk-assets\wallpaper_polos.png -WhatIf
```

Berhasil, wallpaper tidak diubah.

## Paket rilis

```text
installers/win-x64/MedievalDesk.exe
installers/win-x64/MedievalDesk.pdb
installers/win-x64/Assets/                  (24 file)
installers/MedievalDesk-1.0.0-win-x64.zip   (97.504.975 byte)
```

Isi zip: exe, pdb, `Assets`, `docs`, `features`, `tools`, `samples`, README, RELEASE_NOTES.

SHA-256:

```text
9cb80f4afcc47c7de23c1d2e2573228ae94fe969014431394fb271d16d30f678  installers/win-x64/MedievalDesk.exe
75451d2d8202956eab27974b74bfbbe8d95765e4c07b2429803975f9435dc368  installers/MedievalDesk-1.0.0-win-x64.zip
```

Diverifikasi ulang dengan `Get-FileHash`; keduanya cocok dengan `checksums.txt`.

## Belum diverifikasi

Uji visual GUI harus dilakukan pengguna pada mesin target:

- Hover dan klik 10 hotspot.
- Tampilan setelah `Win+D` dan saat aplikasi lain aktif.
- Click-through di atas ikon desktop.
- DPI 100% dan 150%.
- Multi-monitor (hanya monitor utama yang dicakup).
- Seret area hotspot baru dan `Esc` saat seleksi.
- Dialog konfigurasi, profil, impor, tema.
- Lightbox, panah, slideshow, `Esc`.
- Penekanan fisik ketiga hotkey (registrasi sudah terverifikasi).

## Batasan yang diterima

- Hanya monitor utama.
- Hotkey global dapat bentrok.
- Gambar tidak ikut dalam ekspor JSON.
- Tidak ada undo untuk drag-drop.
- Belum ada lisensi proyek.

## Status gerbang

- Build/test otomatis: lulus.
- Packaging: selesai.
- Gate B: `partial` sampai uji visual GUI dilaporkan pengguna.
- Commit dan Release GitHub belum dilakukan; menunggu permintaan terpisah.

## Perbaikan pasca-rilis: skala ikon saat hover

### Gejala

Ukuran artwork berubah saat pointer masuk/keluar hotspot (paling jelas pada `kalender`, `perisai`, `note`, `burung_gagak`, `bola_kristal`).

### Akar masalah

PNG diam dan hover punya padding transparan serta ukuran canvas berbeda. `Image` memakai `Stretch="Uniform"` di dalam `Border`, sehingga yang di-fit adalah canvas penuh, bukan artwork. Skala artwork jadi berbeda antar state.

### Perbaikan

- Baru: `src/MedievalDesk/Core/AssetNormalizer.cs`
  - `Load(path)` memuat `BitmapImage` lalu memotong alpha-bbox.
  - `CropTransparent` menghitung `AlphaBounds` (Bgra32 `CopyPixels`, threshold alpha 8).
  - `UniformFit` menghitung dimensi fit.
- `MainWindow.xaml.cs`: `LoadBitmap` memakai `AssetNormalizer.Load(path)`; `AssetBase` menjadi `internal`.
- `SelfTest.cs`: dua pemeriksaan regresi baru.

Aset sumber tidak diubah.

### Bukti verifikasi

```text
dotnet build .\src\MedievalDesk\MedievalDesk.csproj -c Release
Build succeeded. 0 Warning(s) 0 Error(s)

self-test.txt
LULUS: 15; GAGAL: 0
```

Pemeriksaan baru: "skala tampilan normal/hover konsisten" (selisih lebar/tinggi minimal <= 3% untuk 10 pasangan) dan "ganti sumber gambar tidak mengubah ukuran kontrol".

### Verifikasi visual

Screenshot 1920x1080 (DPI 150%), 10 pasangan normal/hover, diff terhadap `wallpaper_polos.png`:

| Hotspot | Normal | Hover | Delta |
|---|---|---|---|
| rak_buku | 395x706 | 398x728 | +0.8% / +3.1% |
| kalender | 97x117 | 99x128 | +2.1% / +9.4% |
| gulungan | 121x74 | 124x79 | +2.5% / +6.8% |
| bingkai | 364x464 | 369x475 | +1.4% / +2.4% |
| note | 110x109 | 113x115 | +2.7% / +5.5% |
| bola_kristal | 77x98 | 82x102 | +6.5% / +4.1% |
| loker | 320x599 | 377x605 | +17.8% / +1.0% |
| perisai | 184x169 | 190x174 | +3.3% / +3.0% |
| burung_gagak | 142x347 | 143x350 | +0.7% / +0.9% |
| peti | 188x172 | 193x179 | +2.7% / +4.1% |

Catatan: border emas hover ~2 px ikut terukur (+2-3%). `loker` +17.8% berasal dari pintu yang terbuka pada artwork hover, bukan skala; inspeksi visual menunjukkan tepi luar dan tinggi sejajar.

Semua 10 pasangan diperiksa secara visual: dimensi luar dan posisi stabil, perubahan hanya pada state artwork dan border hover.

## 2026-10-21 - Wallpaper ruangan tetap saat layer disembunyikan

### Masalah

Saat desktop tidak aktif, window overlay disembunyikan. Karena wallpaper ruangan hanya digambar di dalam window, latar ikut hilang dan wallpaper Windows terlihat.

### Perbaikan

- `Core/Wallpaper.cs`: baca wallpaper aktif, cadangkan path ke `%AppData%\MedievalDesk\original-wallpaper.txt`, pasang `Assets\wallpaper_polos.png`, dan pulihkan saat exit.
- Backup tidak ditimpa jika wallpaper aktif masih wallpaper ruangan (kasus restart setelah crash).
- `App.xaml.cs`: apply setelah cabang `--self-test`; restore lewat event `Exit`.
- Deteksi desktop disamakan dengan spike: `Progman`, `WorkerW`, `SHELLDLL_DefView`.
- Tidak memakai `SetParent`/WorkerW; perilaku mengikuti Native: overlay transparan + wallpaper desktop.

### Bukti verifikasi

```text
dotnet build .\src\MedievalDesk\MedievalDesk.csproj -c Release
Build succeeded. 0 Warning(s) 0 Error(s)

self-test.txt
LULUS: 16; GAGAL: 0
LULUS  backup wallpaper asli tidak tertimpa
```

Uji lifecycle nyata:

```text
BEFORE c:\windows\web\wallpaper\themeb\img24.jpg
DURING ...\Assets\wallpaper_polos.png
AFTER  c:\windows\web\wallpaper\themeb\img24.jpg   (setelah Ctrl+Alt+Q)
```
