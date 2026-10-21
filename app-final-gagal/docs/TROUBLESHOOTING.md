# Troubleshooting - Medieval Desk

## Aplikasi tidak terlihat

1. Tekan `Win+D` untuk membuka desktop.
2. Tekan `Ctrl+Alt+D`.
3. Periksa ikon di area notifikasi/taskbar; aplikasi memang tidak menampilkan taskbar secara normal.
4. Lihat `%AppData%\MedievalDesk\logs\medieval-desk.log`.

Jika log berisi `Layer tampil.` tetapi layar tidak berubah, kemungkinan wallpaper/aplikasi lain selalu di atas layer. Medieval Desk sengaja tidak memakai `Topmost`.

## Hotkey tidak berfungsi

Pesan `Hotkey gagal didaftarkan` berarti kombinasi sudah dipakai aplikasi lain.

- Tutup aplikasi lain yang memakai `Ctrl+Alt+D`, `Ctrl+Alt+E`, atau `Ctrl+Alt+Q`.
- Jalankan ulang Medieval Desk.
- Jika perlu, keluar lewat Task Manager dengan mengakhiri `MedievalDesk.exe`.

## Klik menembus ke desktop / ikon desktop ikut terklik

Perilaku ini disengaja pada area kosong. Di mode normal, klik pada area tanpa hotspot diteruskan ke desktop. Di mode edit, klik tidak menembus karena area seleksi aktif.

Jika klik menembus tepat di atas hotspot:

1. Pastikan aplikasi versi 1.0.0 (build dengan perbaikan `WM_NCHITTEST`).
2. Tutup dan jalankan ulang.
3. Periksa log; hotspot yang gagal dimuat dapat membuat area tidak terdeteksi.

## Gambar hotspot tidak muncul

- Pastikan seluruh isi zip diekstrak, termasuk folder `Assets` di sebelah exe.
- Jumlah aset yang benar: 24 file.
- Jangan memindahkan exe sendirian.
- Periksa log untuk pesan `Gagal muat gambar`.
- Nama hotspot harus dikenali: `rak_buku`, `kalender`, `gulungan`/`gula`, `bingkai`, `note`, `bola_kristal`/`kristal`, `loker`, `perisai`, `gagak`, `peti`. Hotspot baru tanpa aset tetap bisa dipakai, hanya tanpa gambar.

## Gambar dari folder/drag-drop tidak muncul

- Format yang didukung: `.png`, `.jpg`, `.jpeg`, `.gif`, `.bmp`.
- Drag-drop hanya aktif di mode edit.
- File harus bisa dibaca dan tidak rusak.
- Gambar disalin ke `%AppData%\MedievalDesk\images\`; jika folder itu tidak bisa ditulis, proses gagal.

## Lightbox tidak terbuka

- Hotspot harus punya minimal satu gambar yang masih ada.
- Jika hotspot punya target dan `fixedType` bukan `photo`/`note`, klik akan membuka popup target. Gunakan **Lihat gambar** di popup.
- Jika file gambar dihapus manual dari folder data, entri akan diabaikan.

## Slideshow tidak berjalan

- Minimal 2 gambar.
- Interval dibatasi 1000-60000 ms.
- Timer berhenti saat lightbox ditutup atau jendela kehilangan fokus.

## Data rusak / aplikasi gagal start

Pesan menyebut file utama dan cadangan. Langkah aman:

1. Tutup aplikasi.
2. Salin `%AppData%\MedievalDesk\data\medieval-desk.json` ke tempat aman.
3. Ganti file utama dengan `medieval-desk.bak.json` jika ada.
4. Jalankan ulang.

Jika keduanya rusak, pindahkan seluruh folder `data` ke tempat aman. Aplikasi akan menyalin ulang data awal dari `Assets\sample-layout.json` (hanya jika data utama tidak ada).

Jangan menghapus folder `images` jika masih memakai gambar hotspot.

## Impor ditolak

- File harus JSON valid, maksimal 4 MB, `version` = 1.
- ID hotspot harus unik.
- `box` harus 0-100% dan `w`/`h` > 0.
- Path gambar harus relatif, tanpa `..`, dan berekstensi gambar yang didukung.
- ID duplikat di dalam file impor akan ditolak.

## Profil tidak bisa dimuat

- Nama profil hanya huruf, angka, spasi, `_`, `-`; maksimal 40 karakter.
- File profil harus valid seperti data utama.
- Profil yang rusak tidak otomatis dihapus; perbaiki atau hapus manual dari `%AppData%\MedievalDesk\profiles`.

## Wallpaper ruangan tertinggal

Medieval Desk memasang `Assets\wallpaper_polos.png` saat berjalan dan memulihkan wallpaper asli saat keluar normal. Jika aplikasi berhenti mendadak:

1. Jalankan lagi, lalu tekan `Ctrl+Alt+Q`.
2. Atau pulihkan manual:

```powershell
$backup = Get-Content "$env:APPDATA\MedievalDesk\original-wallpaper.txt" -Raw
.\tools\restore-wallpaper.ps1 -ImagePath $backup.Trim()
```

Jika `original-wallpaper.txt` tidak ada, pilih wallpaper lewat Settings Windows.

## Log

```text
%AppData%\MedievalDesk\logs\medieval-desk.log
```

Log lama dipindahkan ke `medieval-desk.log.old` jika melebihi 1 MB. Sertakan log saat melaporkan bug.
