# Medieval Desk

Overlay desktop bergaya meja abad pertengahan untuk Windows. Gambar latar dan hotspot transparan menempel di atas wallpaper; klik ikon untuk membuka file, folder, URL, atau galeri gambar.

Aplikasi berjalan sebagai layer desktop-only: tampil saat desktop aktif, tersembunyi saat aplikasi lain digunakan, dan area kosong meneruskan klik ke desktop.

## Fitur

- 10 hotspot awal: rak buku, kalender, gulungan, bingkai, catatan, bola kristal, loker, perisai, burung gagak, peti.
- Mode edit: pindah/ubah hotspot, buat hotspot baru dengan seret area.
- Target file, folder, URL, atau daftar target per grup.
- Gambar per hotspot, lightbox, dan slideshow.
- Drag & drop gambar ke hotspot saat mode edit.
- Profil layout, tema Parchment/Night, ekspor/impor JSON.
- Posisi hotspot dalam persen, tetap proporsional saat resolusi berubah.
- Data lokal di `%AppData%\MedievalDesk`; wallpaper ruangan dipasang saat berjalan dan wallpaper asli dipulihkan saat keluar normal.

## Persyaratan

- Windows 10/11 64-bit.
- Build mandiri (self-contained): .NET Runtime tidak perlu dipasang.
- Folder `Assets` harus berada di sebelah `MedievalDesk.exe`.

## Menjalankan

1. Ekstrak `MedievalDesk-1.0.0-win-x64.zip` ke folder tetap, misalnya `C:\Apps\MedievalDesk`.
2. Jalankan `MedievalDesk.exe`.
3. Tekan `Win+D` untuk melihat desktop.
4. `Ctrl+Alt+D` menampilkan/menyembunyikan layer.
5. `Ctrl+Alt+E` masuk/keluar mode edit.
6. `Ctrl+Alt+Q` keluar aplikasi.

Jangan memindahkan `MedievalDesk.exe` tanpa folder `Assets`. Untuk memindahkan aplikasi, pindahkan seluruh folder hasil ekstrak.

Panduan lengkap: [docs/USER_GUIDE.md](docs/USER_GUIDE.md). Masalah umum: [docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md).

## Build dari sumber

```powershell
dotnet build .\src\MedievalDesk\MedievalDesk.csproj -c Release
dotnet publish .\src\MedievalDesk\MedievalDesk.csproj -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o .\installers\win-x64
```

Atau gunakan `tools/build-and-copy.ps1`.

## Tes mandiri

```powershell
.\MedievalDesk.exe --self-test
```

Hasil ditulis ke `self-test.txt` di folder aplikasi; exit code `0` berarti lulus. Tes mencakup validasi data, impor, geometri, profil, tema, penyimpanan gambar, dan logika backup wallpaper.

## Struktur

```text
app-final-gagal/
  README.md
  RELEASE_NOTES.md
  IMPLEMENTATION-LOG.md
  features/                    # dokumentasi fitur
  samples/sample-layout.json   # data awal
  tools/                       # skrip bantu
  docs/                        # panduan pengguna
  src/MedievalDesk/            # sumber C# WPF
  medieval-desk-assets/        # aset gambar sumber
```

Folder `installers/` tidak disimpan di repo (hasil build besar). Build sendiri memakai perintah di atas, atau ambil dari tab **Releases**.

## Status verifikasi

- Build Release: 0 error, 0 warning.
- Self-test: 16 lulus, 0 gagal.
- Click-through divertifikasi otomatis lewat `WM_NCHITTEST`.
- Lifecycle wallpaper divertifikasi: ruangan saat start, wallpaper asli kembali setelah `Ctrl+Alt+Q`.
- Uji visual GUI (hover, DPI, multi-monitor, dialog) masih perlu dilakukan pengguna di mesin target. Lihat `RELEASE_NOTES.md`.

## Kegagalan yang Diketahui

Folder ini **arsip, bukan versi yang dipakai**. Hasilnya berbeda dengan `app-desktop/`:

- **Tampilan tidak sama dengan `app-desktop/`.** Releases ini memakai aset gambar lama di `medieval-desk-assets/`, sedangkan `app-desktop/` memakai `Assets/wallpaper.png` + `Assets/sample-data.json` yang sudah diganti dan posisi hotspot-nya ditata ulang. Hasil visual di sini tidak sepadan dengan versi `app-desktop/`.
- **Gambar hotspot kurang bagus dilihat**, baik gambar diam (sebelum hover) maupun gambar saat hover. Aset di `medieval-desk-assets/diam/` dan `medieval-desk-assets/hover/` belum di-redesign, jadi hotspot terlihat asal-asalan dan tidak serasi dengan tema ruangan.
- **Belum ada perbaikan aset visual di folder ini.** Redesain gambar diam/hover tidak dikerjakan di sini; kalau perlu perbaikan tampilan, kerjakan di `app-desktop/`.

Untuk pemakaian sehari-hari pakai `app-desktop/`. Rilis ini disimpan sebagai arsip v1.0.0.
