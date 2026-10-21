# Drag & Drop Gambar

## Ringkasan

Menambahkan gambar ke hotspot dengan menyeret file dari Explorer ke hotspot saat mode edit.

## Alur

1. Tekan `Ctrl+Alt+E` untuk masuk mode edit.
2. Seret satu atau beberapa file gambar dari Explorer.
3. Arahkan ke hotspot; muncul hint "Lepaskan gambar ke: <label>".
4. Lepaskan.

## Perilaku

- Hanya aktif di mode edit. Di mode normal, drop ditolak.
- Hanya `DataFormats.FileDrop`.
- Ekstensi yang diterima: `.png`, `.jpg`, `.jpeg`, `.gif`, `.bmp` (case-insensitive).
- Setiap file disalin ke `%AppData%\MedievalDesk\images\<id>\`.
- Duplikat isi tidak ditambahkan dua kali.
- Gambar yang sama pada hotspot lain dihapus dari hotspot tersebut agar kepemilikan gambar jelas.
- Perubahan langsung disimpan dan tampilan di-refresh.
- File yang gagal disalin dicatat di log; file lain tetap diproses.

## Umpan balik

- Hint drop muncul saat drag masuk dan hilang saat keluar.
- Status menunjukkan jumlah gambar yang ditambahkan atau nama file yang gagal.

## Batasan

- Tidak ada undo.
- Tidak menerima URL atau data clipboard, hanya file.
- Tidak membaca subfolder.
- Jika penyimpanan data gagal setelah semua salinan berhasil, gambar yatim dapat tertinggal di folder `images`.
- Menyeret ke area kosong tidak membuat hotspot baru.
