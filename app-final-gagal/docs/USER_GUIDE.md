# Panduan Pengguna - Medieval Desk

## 1. Instalasi

1. Ekstrak `MedievalDesk-1.0.0-win-x64.zip` ke folder tetap, misalnya `C:\Apps\MedievalDesk`.
2. Pastikan `MedievalDesk.exe` dan folder `Assets` berada dalam folder yang sama.
3. Jalankan `MedievalDesk.exe`.

Aplikasi bersifat portable: tidak ada installer dan tidak menulis ke folder Program Files. Saat berjalan, aplikasi memasang `Assets\wallpaper_polos.png` sebagai wallpaper desktop; wallpaper asli dipulihkan saat keluar normal.

## 2. Hotkey

| Hotkey | Fungsi |
|---|---|
| `Ctrl+Alt+D` | Tampilkan / sembunyikan layer |
| `Ctrl+Alt+E` | Masuk / keluar mode edit |
| `Ctrl+Alt+Q` | Keluar aplikasi |
| `Esc` | Tutup popup/lightbox, batalkan pemilihan area, atau sembunyikan layer |

Jika hotkey gagal, tutup aplikasi lain yang memakai kombinasi sama, lalu jalankan ulang. Pesan peringatan muncul saat start.

## 3. Mode normal

Layer mengikuti desktop:

- Tampil saat desktop aktif (`Win+D`).
- Tersembunyi saat aplikasi lain aktif.
- Area kosong meneruskan klik ke desktop; ikon desktop tetap bisa diklik.
- Hotspot menampilkan gambar hover dan membuka target saat diklik.

Jenis aksi hotspot:

- **Target langsung**: membuka satu file/folder/URL.
- **Popup daftar**: menampilkan beberapa target, opsional per grup.
- **Galeri**: membuka lightbox jika hotspot punya gambar dan tidak punya target, atau jika `fixedType` bernilai `photo`/`note`.

## 4. Mode edit

Tekan `Ctrl+Alt+E` atau klik tombol **Keluar Edit** untuk kembali.

Saat mode edit:

- Semua hotspot diberi bingkai.
- Klik hotspot untuk membuka dialog pengaturan.
- Tombol **Simpan**, **Ekspor**, **Impor**, **Profil**, **Tema**, **Tambah Hotspot** tersedia di bar bawah.
- Klik di luar hotspot tidak menembus desktop; ini normal karena mode edit membutuhkan area seleksi.

### Membuat hotspot baru

1. Klik **Tambah Hotspot**.
2. Seret kotak pada area gambar. Area minimal 12x12 piksel.
3. Isi dialog, lalu **Simpan**. ID awal berbentuk `gambar_1`, `gambar_2`, dan seterusnya.
4. Tekan `Esc` untuk membatalkan seleksi.

Posisi disimpan dalam persen terhadap gambar, bukan piksel layar.

## 5. Dialog hotspot

| Field | Keterangan |
|---|---|
| ID | Unik, maksimal 64 karakter. Tidak bisa diubah saat mengedit hotspot lama. |
| Label | Nama yang tampil di tooltip dan popup. |
| Jenis | `configurable` atau `fixed`. |
| Fixed type | Opsional: `photo`, `note`, `calendar`, atau nilai lain sesuai data. |
| Mode tampilan | `direct`, `popup-list`, `popup-grid`. |
| X, Y, W, H | Posisi dan ukuran dalam persen (0-100). |
| Target / grup | Satu baris `Label | Target`; header `[Nama Grup]` untuk grup baru. |
| Gambar | Daftar gambar hotspot. |
| Interval slideshow | Milidetik, 1000-60000. Kosong = 5000 ms. |

Contoh target:

```text
[Dokumen]
Tugas Akhir | C:\Users\User\Documents\tugas-akhir
Folder Desain | C:\Users\User\Pictures\Desain

[Web]
GitHub | https://github.com
```

Target dapat berupa file, folder, atau URL. Baris tanpa `|` dianggap target dengan label kosong.

## 6. Gambar dan lightbox

- **Tambah File**: pilih satu atau beberapa gambar (`.png`, `.jpg`, `.jpeg`, `.gif`, `.bmp`).
- **Tambah Folder**: semua gambar dalam folder (tidak rekursif) disalin.
- **Lihat**: pratinjau gambar terpilih.
- **Hapus**: hapus gambar dari hotspot dan dari folder data aplikasi.

Gambar disalin ke `%AppData%\MedievalDesk\images\<id-hotspot>\`. File sumber tidak diubah.

Lightbox:

- Klik hotspot bergambar.
- **Sebelumnya** / **Berikutnya** atau tombol panah kiri/kanan.
- **Slideshow** membutuhkan minimal 2 gambar; tombol berubah menjadi **Stop**.
- **Tutup** atau `Esc` menutup lightbox dan menghentikan timer.

## 7. Drag & drop

Saat mode edit, seret file gambar dari Explorer ke hotspot. Gambar disalin ke folder data dan langsung disimpan. Format di luar daftar putih diabaikan.

## 8. Profil

Klik **Profil** untuk:

- **Muat**: memuat layout profil. Layout saat ini dicadangkan lebih dulu ke data utama.
- **Simpan**: menyimpan layout saat ini sebagai profil baru atau menimpa profil yang ada.
- **Hapus**: menghapus file profil.

Nama profil: huruf, angka, spasi, `_`, `-`; panjang 1-40 karakter. Profil disimpan di `%AppData%\MedievalDesk\profiles\`.

## 9. Tema

Klik **Tema** untuk berganti antara `Parchment` dan `Night`. Pilihan disimpan di `%AppData%\MedievalDesk\preferences.json` dan dipakai saat start berikutnya.

## 10. Ekspor dan impor

- **Ekspor**: menyimpan seluruh layout ke file JSON.
- **Impor**: memilih file JSON, menampilkan pratinjau berisi hotspot yang akan ditambah/diperbarui, lalu menerapkan setelah konfirmasi.

Aturan impor:

- Hotspot dengan ID sama akan diperbarui.
- ID baru akan ditambahkan.
- Hotspot lama yang tidak ada di file impor tetap dipertahankan.
- File maksimal 4 MB, schema harus versi 1.
- Path gambar harus relatif dan aman; path absolut atau mengandung `..` ditolak.

## 11. Lokasi data

```text
%AppData%\MedievalDesk\
  data\medieval-desk.json       # data utama
  data\medieval-desk.bak.json   # backup otomatis saat simpan
  profiles\                     # profil layout
  images\                       # gambar yang disalin
  preferences.json              # tema
  original-wallpaper.txt        # path wallpaper asli
  logs\medieval-desk.log        # log aplikasi
```

Data awal disalin dari `Assets\sample-layout.json` hanya jika data utama belum ada. Data pengguna yang sudah ada tidak ditimpa saat aplikasi diperbarui.

## 12. Wallpaper

Saat mulai, Medieval Desk menyimpan path wallpaper aktif ke `%AppData%\MedievalDesk\original-wallpaper.txt`, lalu memasang `Assets\wallpaper_polos.png`. Saat keluar normal (`Ctrl+Alt+Q`), wallpaper asli dipulihkan.

Jika aplikasi berhenti mendadak dan wallpaper ruangan tertinggal:

1. Jalankan aplikasi lagi, lalu keluar dengan `Ctrl+Alt+Q`; backup yang ada dipakai untuk memulihkan.
2. Atau pulihkan manual:

```powershell
$backup = Get-Content "$env:APPDATA\MedievalDesk\original-wallpaper.txt" -Raw
.\tools\restore-wallpaper.ps1 -ImagePath $backup.Trim()
```

Skrip memakai `SystemParametersInfo` Windows. Backup hanya berisi path, bukan salinan file gambar.

## 13. Uninstall

1. Tekan `Ctrl+Alt+Q`.
2. Hapus folder aplikasi.
3. Opsional: hapus `%AppData%\MedievalDesk` untuk menghapus data, profil, gambar, dan log.

## 14. Batasan

- Hanya monitor utama.
- Windows 10/11 x64.
- Hotkey global dapat bentrok dengan aplikasi lain.
- Uji visual pada DPI berbeda dan multi-monitor belum dilakukan pada rilis ini.
