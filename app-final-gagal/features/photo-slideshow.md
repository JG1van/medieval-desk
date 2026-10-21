# Gambar Hotspot, Lightbox, dan Slideshow

## Gambar hotspot

Setiap hotspot dapat memiliki daftar gambar. Gambar disalin ke:

```text
%AppData%\MedievalDesk\images\<id-hotspot>\
```

Penyalinan:

- Nama file asli dipertahankan setelah dibersihkan.
- Jika nama sama dan isi sama (SHA-256), file tidak diduplikasi.
- Jika nama sama tetapi isi berbeda, diberi akhiran `_1`, `_2`, dan seterusnya.
- Format: `.png`, `.jpg`, `.jpeg`, `.gif`, `.bmp`.
- File sumber tidak diubah atau dihapus.

Menghapus gambar dari daftar juga menghapus salinannya di folder data.

## Kapan lightbox terbuka

Klik hotspot membuka lightbox jika:

- Hotspot punya gambar yang bisa dibaca, dan
- Tidak punya target, atau
- `kind` = `fixed` dengan `fixedType` `photo`, `note`, atau kosong.

Hotspot `configurable` yang punya target membuka popup daftar, bukan lightbox. Gunakan tombol **Lihat gambar** di popup untuk membuka galeri.

Jika tidak ada gambar valid, aplikasi menampilkan status "Hotspot ini belum punya gambar."

## Lightbox

Kontrol:

- **Sebelumnya** / **Berikutnya** atau panah kiri/kanan.
- Penghitung `posisi / jumlah` dan nama file.
- **Slideshow** / **Stop**.
- **Tutup** atau `Esc`.

Lightbox menutup otomatis saat jendela kehilangan fokus.

## Slideshow

- Minimal 2 gambar.
- Interval diambil dari `slideshowMs` hotspot.
- Default 5000 ms; nilai dijepit ke 1000-60000 ms.
- Timer berhenti saat lightbox ditutup, `Esc`, pindah aplikasi, atau tombol **Stop**.
- Slide berputar melingkar.

## Batasan

- Tidak ada zoom/pan.
- Tidak ada pengurutan manual; urutan mengikuti daftar gambar.
- GIF ditampilkan sesuai dukungan WPF, tanpa kontrol animasi khusus.
- Gambar yang dihapus dari disk akan dilewati saat resolve; entri tetap ada di data sampai dihapus lewat dialog.
