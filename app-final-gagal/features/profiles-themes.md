# Profil dan Tema

## Profil layout

Profil adalah salinan penuh layout dalam file JSON terpisah. Berguna untuk beberapa susunan meja, misalnya "Kerja", "Belajar", atau "Presentasi".

Lokasi:

```text
%AppData%\MedievalDesk\profiles\<nama>.json
```

Operasi:

| Tombol | Perilaku |
|---|---|
| Muat | Menyimpan layout saat ini ke data utama, memuat profil, lalu menyimpan hasilnya sebagai data utama. |
| Simpan | Menulis layout saat ini ke profil. Jika nama sudah ada, konfirmasi timpa. |
| Hapus | Menghapus file profil setelah konfirmasi. |

Nama profil:

- 1-40 karakter.
- Huruf, angka, spasi, `_`, `-`.
- Bukan `.` atau `..`.
- Nama dipakai sebagai nama file; karakter di luar daftar ditolak, bukan dibersihkan diam-diam.

Muat profil tidak menghapus gambar lama di folder `images`. Gambar yang hanya dipakai profil lain tetap tersimpan.

## Tema

Dua tema tersedia:

- **Parchment**: latar terang, aksen cokelat.
- **Night**: latar gelap, aksen keemasan.

Tombol **Tema** berganti bergantian. Pilihan disimpan di:

```text
%AppData%\MedievalDesk\preferences.json
```

Isi:

```json
{
  "theme": "Night"
}
```

Tema hanya mengubah tampilan panel edit, dialog, popup, dan lightbox. Tema tidak mengubah aset hotspot atau wallpaper.

Jika file preferensi rusak atau berisi nama tema tak dikenal, aplikasi kembali ke `Parchment` tanpa gagal start.

## Batasan

- Profil menyimpan path gambar relatif; memindahkan profil ke mesin lain tidak otomatis membawa gambar.
- Tidak ada profil bawaan; data awal berasal dari `Assets\sample-layout.json`.
