# Ekspor dan Impor

## Tujuan

Memindahkan atau menggabungkan layout antar mesin tanpa menyentuh file data internal.

## Ekspor

Tombol **Ekspor** di mode edit membuka `SaveFileDialog`.

- Nama default: `medieval-desk-YYYYMMDD.json`.
- Isi: seluruh `DeskData` (versi, referensi, pengaturan, hotspot, target, gambar, slideshow).
- Path gambar ditulis relatif terhadap `%AppData%\MedievalDesk`.
- File ditulis lewat file sementara lalu dipindahkan, sehingga file lama tidak rusak jika proses gagal.

Ekspor tidak menyalin file gambar. Jika layout dipindah ke mesin lain, gambar harus disalin terpisah ke `images\<id>\` atau ditambahkan ulang.

## Impor

Tombol **Impor**:

1. Memilih file JSON.
2. Memvalidasi ukuran (maksimal 4 MB), JSON, schema versi 1, ID unik, box, dan path gambar.
3. Menampilkan pratinjau: jumlah hotspot baru, jumlah yang diperbarui, dan daftar ID.
4. Menyimpan layout saat ini ke data utama dan `.bak.json`.
5. Menggabungkan data.

Aturan merge:

- ID sama: data lama diganti data impor.
- ID baru: ditambahkan.
- ID lama yang tidak ada di file: dipertahankan.
- `reference` dan `settings` dari file impor tidak menimpa pengaturan aplikasi saat ini.

Pembatalan pada pratinjau tidak mengubah data.

## Format minimal

```json
{
  "version": 1,
  "reference": { "w": 1376, "h": 768 },
  "settings": { "minimizeAfterLaunch": true, "hotkey": "Ctrl+Alt+D" },
  "hotspots": [
    {
      "id": "contoh",
      "label": "Contoh",
      "kind": "configurable",
      "box": { "x": 10, "y": 10, "w": 20, "h": 20 },
      "groups": [
        { "name": "Tujuan", "items": [{ "label": "Folder", "target": "%USERPROFILE%\\Documents" }] }
      ]
    }
  ]
}
```

Field opsional: `fixedType`, `displayMode`, `images`, `slideshowMs`.

## Validasi

- `version` harus `1`.
- `id` wajib, maksimal 64 karakter, tidak boleh duplikat (case-insensitive).
- `box` dalam 0-100%, `w`/`h` > 0.
- Gambar hanya `.png`, `.jpg`, `.jpeg`, `.gif`, `.bmp`.
- Path gambar harus relatif, tidak absolut, tidak mengandung `..`.
- `slideshowMs` dijepit ke 1000-60000.

## Batasan

- Gambar tidak ikut dalam JSON.
- Impor tidak melakukan rollback otomatis jika penyimpanan setelah merge gagal; backup `.bak.json` tersedia.
- Field tak dikenal dipertahankan lewat `[JsonExtensionData]`, tetapi tidak divalidasi.
