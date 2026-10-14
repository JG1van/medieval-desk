# Medieval Desk

Medieval Desk adalah aplikasi desktop Windows berupa wallpaper interaktif bergaya medieval RPG yang mengubah desktop menjadi ruang kerja bergaya perpustakaan / dungeon. Aplikasi ini berfungsi sebagai launcher visual: objek-objek pada wallpaper dapat dikonfigurasikan sebagai "hotspot" untuk membuka file, folder, shortcut Windows (.lnk), atau URL.

> Catatan: README sebelumnya merujuk `docs/screenshot.png`. File tersebut tidak ada pada repository; sebagai gantinya gunakan wallpaper SVG/PNG di `src/assets/wallpaper.svg` atau `src/assets/wallpaper.png` sebagai ilustrasi.

Ringkasan Fitur

- Hotspot universal: hampir semua objek pada wallpaper dapat menjadi interaktif.
- Edit Mode: mengatur posisi/ukuran/nama/tujuan hotspot secara langsung lewat antarmuka.
- Peluncur aplikasi: hotspot bisa membuka file, folder, shortcut (.lnk), atau URL.
- Kontrol jendela bertema medieval dan global hotkey `Ctrl + Alt + D` untuk sembunyikan/tampilkan aplikasi.
- Penyimpanan konfigurasi lokal (localStorage) dan dukungan ekspor/impor konfigurasi.

Isi Repository (pada saat update ini)

- src/: kode frontend (HTML, CSS, JS)
  - src/index.html
  - src/assets/wallpaper.svg, src/assets/wallpaper.png
  - src/js/ (app.js, editor.js, hotspots.js, storage.js, dll.)
- src-tauri/: backend Rust dan konfigurasi Tauri
- assets/: aset tambahan (wallpaper, font, dll.)
- package.json, package-lock.json
- setup-and-check.ps1, build.cmd, preview-source.webp

Persyaratan Sistem

- Windows 10 atau Windows 11
- Node.js (versi LTS direkomendasikan)
- Rust toolchain (rustup)
- Microsoft C++ Build Tools (dibutuhkan untuk beberapa dependensi native)
- WebView2 Runtime (untuk komponen WebView pada Windows)

Instalasi dan Menjalankan (pengembang)

1. Clone:

   git clone https://github.com/JG1van/medieval-desk.git
   cd medieval-desk

2. Instal dependency:

   npm install

3. Jalankan mode pengembangan:

   npm run tauri dev

Perintah `npm run tauri dev` menjalankan aplikasi dalam mode pengembangan sesuai konfigurasi di package.json.

Build Produksi

Untuk membangun aplikasi desktop produksi:

npm run tauri build

Hasil build biasanya berada di `src-tauri/target/release/` dan bundel (installer) di `src-tauri/target/release/bundle/` bila bundling diaktifkan.

Konfigurasi Hotspot (detail teknis)

Medieval Desk menyimpan konfigurasi hotspot di localStorage browser-wrapped aplikasi. Kunci yang dipakai:

- `medieval-desk-hotspots-v2` — daftar hotspot (format JSON array)
- `medieval-desk-edit-mode` — boolean ("true"/"false") untuk menyimpan status Edit Mode
- `medieval-desk-layout-version` — versi layout/pengaturan
- `medieval-desk-popup-<key>` — data popup/temporary per key

Format objek hotspot (contoh minimal):

{
  "id": "rack",
  "name": "Rak Buku",
  "x": 3.2,     // posisi X dalam persen relatif ke lebar wallpaper
  "y": 18.5,    // posisi Y dalam persen relatif ke tinggi wallpaper
  "w": 21.0,    // lebar area klik dalam persen
  "h": 66.5,    // tinggi area klik dalam persen
  "type": "list", // tipe: 'list' | 'photo' | 'calendar' | 'note' | ...
  "items": [ { "name": "Catatan", "url": "C:\\path\\to\\file.txt" } ], // untuk type 'list'
  "target": "C:\\path\\to\\image.jpg", // untuk type 'photo'
  "displayMode": "popup-list" // atau 'direct', 'popup-grid', dll.
}

- Posisi dan ukuran memakai persentase sehingga layout relatif terhadap resolusi layar.
- Field `items` hanya diperlukan untuk `type: 'list'`.
- Field `target` dipakai untuk `type: 'photo'` atau target tunggal.

API Export / Import

Kode menyediakan fungsi ekspor/impor konfigurasi yang dapat dipakai dari UI (jika tersedia) atau console:

- `exportConfig()` → mengembalikan JSON string berisi { hotspots, editMode, version }
- `importConfig(json)` → mengimpor JSON yang berisi struktur di atas dan menyimpannya ke localStorage

Implementasi penyimpanan ada di `src/js/storage.js` — baca file tersebut untuk detail migrasi versi, default hotspot, dan proses merge dengan konfigurasi pengguna.

Perilaku & Keterbatasan

- Aplikasi berjalan sebagai jendela biasa: dapat tertutup oleh jendela lain. Gunakan `Ctrl + Alt + D` atau klik di taskbar untuk menampilkannya kembali.
- Multi-monitor dan skala DPI: hotspot memakai persentase, tetapi perbedaan DPI pada konfigurasi multi-monitor dapat memengaruhi akurasi. Perlu pengujian lebih lanjut.
- Validasi target: aplikasi menyimpan path/URL yang diberikan, tetapi jika tujuan tidak ditemukan saat klik, perilaku tergantung OS—sebaiknya periksa keberadaan file/shortcut/URL.

Keamanan

- Hotspot dapat membuka file atau URL luar; berhati-hati dengan target dari sumber tak tepercaya.

Troubleshooting (Masalah umum)

- WebView2 tidak terinstall: instal WebView2 Runtime dari Microsoft.
- Build gagal karena toolchain Rust/C++: pasang Microsoft C++ Build Tools dan rustup.
- Hotkey `Ctrl + Alt + D` bentrok: matikan atau ganti hotkey lain di OS atau di konfigurasi aplikasi (jika disediakan).

Dokumentasi & Pengembangan

- Penjelasan kode utama terkait hotspot dan penyimpanan ada di:
  - `src/js/hotspots.js` — logika interaksi hotspot
  - `src/js/editor.js` — logika Edit Mode
  - `src/js/storage.js` — migrasi, loading, saving, export/import
- Font yang dipakai: Grenze Gotisch, Cardo (periksa lisensi font sebelum distribusi)

Lisensi

- Repository saat ini tidak memiliki file LICENSE. Disarankan menambahkan lisensi (mis. MIT atau Apache-2.0) untuk menjelaskan hak penggunaan dan kontribusi.

Kontribusi

- Untuk kontribusi: buka issue untuk diskusi perubahan besar, buat PR untuk perbaikan/fitur. Tambahkan test dan dokumentasi bila perlu.

Kontak

- Issues di GitHub adalah cara terbaik untuk melaporkan bug atau meminta fitur: https://github.com/JG1van/medieval-desk/issues

Perubahan yang saya lakukan sekarang

- Memperbarui README.md agar lebih lengkap dan menjelaskan struktur, konfigurasi (format JSON), export/import, serta keterbatasan.

Catatan lanjutan

- Anda ingin README benar-benar komplit. Jika setuju, saya bisa juga:
  - Menambahkan contoh file konfigurasi `example-config.json` ke repo.
  - Menambahkan `CONTRIBUTING.md` dan `CHANGELOG.md`.
  - Menambahkan file `LICENSE` (harap konfirmasi lisensi yang diinginkan).

Jika Anda setuju, beri tahu opsi mana yang mau ditambahkan selanjutnya (contoh: "Tambahkan LICENSE MIT dan example-config.json"), dan saya akan buat commit/PR berikutnya.
