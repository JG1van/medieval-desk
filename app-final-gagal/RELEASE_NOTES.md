# Release Notes - Medieval Desk 1.0.0

Tanggal build: 21 Oktober 2026
Platform: Windows 10/11 x64
Artefak: `MedievalDesk-1.0.0-win-x64.zip` (dibangun lewat workflow **Release**; tidak disimpan di repo)

## Ringkasan

Rilis pertama aplikasi overlay Medieval Desk berbasis C# WPF (.NET 8). Aplikasi menampilkan meja abad pertengahan di atas wallpaper dengan hotspot interaktif, mode edit, galeri gambar, profil, dan tema.

## Fitur

- Layer desktop-only, transparan, click-through pada area kosong.
- Wallpaper ruangan dipasang saat berjalan; wallpaper asli dicadangkan dan dipulihkan saat keluar normal.
- Hotkey global: `Ctrl+Alt+D` (tampil/sembunyi), `Ctrl+Alt+E` (edit), `Ctrl+Alt+Q` (keluar).
- 10 hotspot awal dengan aset diam dan hover.
- Mode edit: pilih hotspot, seret area untuk hotspot baru, atur posisi dalam persen.
- Target file, folder, URL; beberapa target per grup.
- Gambar hotspot: tambah file/folder, pratinjau, hapus, lightbox, slideshow 1-60 detik.
- Drag & drop gambar ke hotspot saat mode edit.
- Profil layout: simpan, muat, hapus.
- Tema Parchment dan Night, preferensi tersimpan.
- Ekspor/impor JSON dengan pratinjau dan merge berdasarkan ID.
- Penyimpanan atomik + backup `.bak.json`; validasi schema dan path.

## Perbaikan penting selama pengembangan

- Click-through: `WM_NCHITTEST` memakai `RootGrid.InputHitTest`, bukan window, karena background transparan membuat seluruh window menerima hit-test.
- Publish single-file: aset besar sempat ikut dibundel ke exe sehingga tidak tersedia sebagai file. Semua `Content` sekarang diberi `ExcludeFromSingleFile=true`; publish ulang menghasilkan 24 file `Assets` dan exe lebih kecil.
- Wallpaper: versi awal hanya menyembunyikan window saat desktop tidak aktif, sehingga latar ikut hilang. Sekarang wallpaper ruangan dipasang melalui `SystemParametersInfo`, dengan backup path asli dan restore saat exit.

## Verifikasi

- `dotnet build -c Release`: berhasil, 0 warning, 0 error.
- `MedievalDesk.exe --self-test`: exit `0`, 16 lulus, 0 gagal.
- Pemeriksaan click-through otomatis: titik kosong mengembalikan `HTTRANSPARENT (-1)`, 10 pusat hotspot mengembalikan `HTCLIENT (1)`.
- Uji lifecycle wallpaper: wallpaper berubah ke `wallpaper_polos.png` saat start dan kembali ke wallpaper asli setelah `Ctrl+Alt+Q`.
- Jumlah aset pada folder publish: 24 file.

## Belum diverifikasi / batasan

- Uji visual GUI oleh pengguna belum dilakukan: hover, tampilan saat `Win+D`, DPI 100%/150%, multi-monitor, dialog, seret area, lightbox, dan profil.
- Layer hanya mencakup monitor utama.
- Hotkey dapat gagal jika aplikasi lain memakai kombinasi yang sama; pesan peringatan ditampilkan.
- Jika proses berhenti mendadak, wallpaper ruangan bisa tertinggal; jalankan aplikasi lagi lalu keluar normal, atau gunakan `tools/restore-wallpaper.ps1` dengan path dari `%AppData%\MedievalDesk\original-wallpaper.txt`.
- Belum ada lisensi proyek.

## Cara pakai cepat

1. Ekstrak zip.
2. Jalankan `MedievalDesk.exe` dari folder hasil ekstrak.
3. `Win+D`, lalu gunakan hotspot.
4. `Ctrl+Alt+E` untuk mengedit.

Detail: `docs/USER_GUIDE.md`.
