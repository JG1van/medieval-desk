# MedievalDesk app-desktop (WPF .NET 8)

Versi utama yang sedang dikembangkan: overlay desktop bergaya ruangan perpustakaan dungeon.
Wallpaper digantikan gambar ruangan, objek di dalamnya jadi hotspot untuk membuka file, folder, shortcut, atau URL.

Keunggulan dibanding `app-final-gagal/`: data, aset, dan logging sepenuhnya mandiri (portable),
tanpa installer, dan seed data awal tersedia sehingga aplikasi langsung punya 10 hotspot.

## Isi folder

```text
app-desktop/
  MedievalDesk.sln
  README.md
  SPEC-NATIVE.md
  MedievalDesk.Spike/
    MedievalDesk.Spike.csproj
    App.xaml, App.xaml.cs
    MainWindow.xaml, MainWindow.xaml.cs
    Native/Win32.cs
    Assets/
      wallpaper.png          # wallpaper ruangan (wajib)
      sample-data.json       # seed 10 hotspot (wajib)
      fonts/*.woff2          # font web, opsional
```

Folder `Assets/` wajib ikut di samping `MedievalDesk.Spike.exe`. Tanpa `wallpaper.png` atau
`sample-data.json`, aplikasi tetap jalan tapi tanpa wallpaper ruangan dan tanpa hotspot awal.

## Build

```powershell
dotnet build "MedievalDesk.Spike\MedievalDesk.Spike.csproj" -c Release
```

Publish self-contained (tidak butuh .NET di mesin tujuan):

```powershell
dotnet publish "MedievalDesk.Spike\MedievalDesk.Spike.csproj" -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true -o .\publish
```

Hasil publish: `MedievalDesk.Spike.exe` (single file) + folder `Assets\`.

## Menjalankan

1. Jalankan `MedievalDesk.Spike.exe` dari foldernya.
2. `Win+D` untuk melihat desktop.
3. `Ctrl+Alt+D` tampil/sembunyikan layer, `Ctrl+Alt+E` mode edit, `Ctrl+Alt+Q` keluar dan mengembalikan wallpaper.

Data aktif: `%APPDATA%\MedievalDesk\data\medieval-desk.json`.
Log: `spike-log.txt` di folder aplikasi.

Target hotspot bisa memakai variabel lingkungan (`%USERPROFILE%\Documents`) dan otomatis diekspansi.

## Verifikasi manual (setelah build & jalankan .exe)

| # | Cek | Expected |
|---|-----|----------|
| 1 | Jendela berada di belakang semua jendela | ✅ |
| 2 | Setelah Win+D, wallpaper spike tetap terlihat | ✅ |
| 3 | Klik hotspot (kotak 200×120 di tengah) menampilkan MessageBox "Klik tembus OK" | ✅ |
| 4 | Aman di DPI 100% | ✅ |
| 5 | Aman di DPI 150% | ✅ |
| 6 | Setelah program ditutup, wallpaper Windows asli kembali | ✅ |

## Batasan

- Font "Grenze Gotisch" tidak terpasang di Windows; WPF memakai font cadangan (Georgia).
- `Ctrl+Alt+Q` adalah cara keluar yang aman. Menutup paksa dapat meninggalkan wallpaper terpasang.
- Struktur overlay dan multi-monitor dicatat di `docs/PERBAIKAN-OVERLAY.md` dan `docs/PROBE-MERAH.md`.
