# PLAN: Medieval Desk (versi final)

Dokumen ini adalah sumber kebenaran untuk agen coding (OpenCode).

---

## 0. Lokasi kerja dan instruksi eksekusi (WAJIB DIBACA PERTAMA)

- **Folder kerja:** `C:\LARAGON-2025-1\www\@GITHUB\medieval-desk\ProdukJadi`
- Semua file dibuat dan diubah HANYA di folder ini. Jangan membuat atau mengubah apa pun di luarnya.
- Jalankan TAHAP 1 secara otomatis dari awal sampai laporan akhir. Jangan berhenti untuk meminta konfirmasi, persetujuan, atau pengecekan dari pengguna di antara fase.
- Verifikasi dilakukan agen sendiri (bagian 9). Tes manual (bagian 10) hanya untuk pengguna setelah laporan. Tes manual BUKAN gerbang.
- TAHAP 2 hanya membuat `native/SPEC.md`. Jangan menulis kode C# dan jangan memasang .NET.
- Keputusan kecil yang tidak tertulis: pilih yang paling sederhana, catat di `docs/PROTOTYPE-NOTES.md`, lalu lanjut.
- Blocker (tidak bisa diselesaikan sendiri): catat di `docs/PROTOTYPE-NOTES.md` dengan judul `BLOCKER`, lalu lanjutkan bagian lain yang tidak bergantung padanya.
- Centang bagian 12 setiap kali satu butir selesai.
- Laporan hanya sekali, di akhir, sesuai bagian 11.

---

## 1. Keputusan final (jangan diubah)

- Proyek dibangun dalam dua tahap.
- **TAHAP 1 = PROTOTIPE WEB.** Bahasa: HTML, CSS, dan JavaScript biasa. Tanpa framework, build step, npm dependency, CDN, atau ES module (jangan pakai `import`/`export`). Kode dibungkus dalam objek global, misalnya `window.Platform`. Dibangun di **root folder kerja** (`ProdukJadi`). Tujuannya menguji tampilan, alur, dan data.
- **TAHAP 2 = VERSI ASLI.** Bahasa: C# (.NET 8) + WPF, akses Win32 lewat P/Invoke, efek visual dengan SkiaSharp. Hasilnya satu file `.exe`. Dibangun di folder `native/` nanti. Tahap 2 BELUM dikerjakan sekarang.
- Versi Tauri lama diarsipkan di `legacy/tauri/`. Jangan dihapus.
- Kedua tahap memakai kontrak yang sama: `data/medieval-desk.json`, `config/visual.json`, dan `docs/CONTRACT.md`.

---

## 2. TAHAP 1, FASE A: Restrukturisasi

### A0. Catat koordinat lama dulu
Sebelum ada file yang dipindah, baca koordinat hotspot dari kode atau config lama di `..\Prototipe\` (misalnya `..\Prototipe\config.json`, `..\Prototipe\src\js\hotspots.js`, atau `..\Prototipe\src\js\app.js`). Salin label dan `box` (x, y, w, h dalam persen) ke `docs/PROTOTYPE-NOTES.md` bagian "Koordinat asal". Jangan mengarang koordinat.

### A1. Arsipkan Tauri (COPY dari `..\Prototipe\`, JANGAN PINDAHKAN)
- Buat folder `legacy/tauri/` di dalam folder kerja.
- **SALIN** (copy, bukan move) dari `..\Prototipe\` ke `legacy/tauri/`: `src/`, `src-tauri/`, `package.json`, `package-lock.json` (jika ada), `build.cmd`, `setup-and-check.ps1`, `diff_app.txt`, `preview-source.webp`, dan `config.json` (jika ada). `config.json` lama berisi path pribadi, jadi jangan dipakai lagi.
- **Folder `..\Prototipe\` TIDAK AKAN DIHAPUS atau diubah.** Semua operasi hanya di folder kerja `ProdukJadi`.
- Item yang tidak ditemukan dilewati dan dicatat.
- **Copy juga dari `..\Prototipe\` ke root folder kerja:** `README.md`, `.gitignore`, dan `assets/` (folder fonts dan wallpaper). Ini diperlukan karena file-file tersebut ada di `Prototipe\`, bukan di root.

### A2. Buat AGENTS.md
Di root folder kerja, isinya tiga baris:
```
Sebelum bekerja, baca docs/PLAN.md dan ikuti seluruh isinya.
Folder kerja: C:\LARAGON-2025-1\www\@GITHUB\medieval-desk\ProdukJadi. Semua perubahan hanya di folder ini.
Tahap 1 (prototipe web) dikerjakan. Tahap 2 hanya spesifikasi.
```

### A3. Bangun prototipe baru di root folder kerja
- Boleh menyalin logika UI dari `..\Prototipe\legacy\tauri\src\` yang masih relevan.
- Tidak boleh ada pemanggilan `window.__TAURI__` di luar `legacy/`.
- Koordinat hotspot diambil dari catatan A0. Jika ada hotspot yang tidak ditemukan (misalnya Perisai, Burung Gagak, Peti), isi `box` dengan `null` dan tulis "perlu kalibrasi" di `docs/PROTOTYPE-NOTES.md`.

### A4. Struktur prototipe (semua path di dalam `ProdukJadi/`)

```
index.html
css/style.css
js/platform.js            (kontrak platform, bagian 3)
js/data.js                (muat, simpan, validasi skema)
js/stage.js               (layout cover dan posisi hotspot dalam persen)
js/popup.js               (pop up grup dan item)
js/app.js                 (inisialisasi, mode edit, pintasan keyboard)
data/medieval-desk.json   (data default skema v1, bagian 4)
config/visual.json        (nilai visual, bagian 5)
assets/wallpaper.png
assets/fonts/             (Grenze Gotisch dan Cardo, file lokal)
docs/PLAN.md              (dokumen ini)
docs/CONTRACT.md
docs/PROTOTYPE-NOTES.md
native/SPEC.md            (bagian 7)
legacy/tauri/             (arsip)
AGENTS.md
```

---

## 3. Kontrak platform (`js/platform.js`)

Semua akses ke browser atau sistem harus lewat file ini.

- **`loadData()` → Promise<object>**
  Baca dari `localStorage` dengan kunci `medieval-desk:data`. Jika kosong, ambil `data/medieval-desk.json` lewat `fetch`. Jika keduanya gagal, pakai data kosong dan tampilkan pesan.
- **`saveData(data)` → Promise<void>**
  Validasi skema v1, lalu simpan ke `localStorage`. Jika validasi gagal, jangan simpan dan tampilkan pesan yang jelas.
- **`exportData()` → void**
  Unduh data sebagai `medieval-desk-backup.json`.
- **`importData(file)` → Promise<object>**
  Baca file JSON, validasi, lalu simpan.
- **`openItem(item)` → void**
  - Jika `item.type === "url"`: `window.open(item.target, "_blank", "noopener")`.
  - Jika tipe lain (`app`, `file`, `folder`, `shortcut`): tampilkan kotak info berisi label, target, dan tombol Salin. Tulis pesan: "Prototipe web tidak bisa membuka file lokal. Versi asli akan membukanya."
- **`windowMinimize()`, `windowMaximize()`, `windowClose()` → void**
  Tampilkan toast "Hanya tersedia di versi asli".
- **`getSystemStats()` → null**
  UI menampilkan "Tidak tersedia di prototipe".
- **`getWeather(lat, lon)` → Promise<object | null>**
  Ambil dari `https://api.open-meteo.com/v1/forecast?latitude=LAT&longitude=LON&current=temperature_2m,weather_code`. Cache 15 menit di `localStorage`. Jika gagal, kembalikan `null`.
- **`now()` → Date**
  Kembalikan `new Date()`. Dibuat sebagai fungsi agar mudah diuji.

Catat setiap fungsi di `docs/CONTRACT.md` beserta input, output, dan perilakunya. Dokumen ini menjadi acuan untuk versi asli.

---

## 4. Skema data v1 (`data/medieval-desk.json`)

```json
{
  "version": 1,
  "reference": { "width": 1376, "height": 768 },
  "settings": {
    "minimizeAfterLaunch": true,
    "hotkey": "Ctrl+Alt+D",
    "location": { "name": "", "lat": null, "lon": null }
  },
  "hotspots": [],
  "widgets": []
}
```

Setiap hotspot:

```json
{
  "id": "rak_buku",
  "label": "Rak Buku",
  "kind": "configurable",
  "fixedType": null,
  "box": { "x": 2.9, "y": 19.5, "w": 21, "h": 67.5 },
  "groups": []
}
```

- `kind`: `"configurable"` atau `"fixed"`.
- `fixedType` (hanya jika `fixed`): `"calendar"`, `"note"`, atau `"photo"`.
- `box`: persen dari gambar.
- `groups` (hanya jika `configurable`).

Setiap grup: `{ "id": "g1", "name": "Rak 1", "items": [] }`

Setiap item: `{ "id": "i1", "type": "url", "label": "...", "target": "...", "icon": null }`

`type`: `"app"`, `"file"`, `"folder"`, `"shortcut"`, atau `"url"`.

**Isi default:**
- `rak_buku`: configurable, 4 grup (Rak 1 sampai Rak 4).
- `loker`: configurable, 2 grup (Pintu Kiri, Pintu Kanan).
- `peti`: configurable, 1 grup (Downloads).
- `gulungan`, `burung_gagak`, `bola_kristal`, `perisai`: configurable, masing-masing 1 grup "Umum".
- `kalender`: fixed, `fixedType` = `"calendar"`.
- `note`: fixed, `fixedType` = `"note"`.
- `bingkai`: fixed, `fixedType` = `"photo"`.
- Semua `target` default kosong (`""`). Jangan mengisi path atau URL pribadi.
- `box` diambil dari catatan A0.

---

## 5. Nilai visual (`config/visual.json`)

Semua nilai visual diambil dari file ini. Jangan menulis warna langsung di banyak tempat.

```json
{
  "colors": {
    "dark": "#091413",
    "greenDark": "#285A48",
    "greenMid": "#408A71",
    "greenLight": "#B0E4CC",
    "paper1": "#e3d0b0",
    "paper2": "#fdf5e6",
    "paper3": "#fffaf0",
    "paperBorder": "#c9b28a",
    "paperText": "#3a2f1f",
    "wood": "#7a5c3a",
    "torchGold": "#ffcc6a",
    "torchEmber": "#e39b39"
  },
  "fonts": { "title": "Grenze Gotisch", "body": "Cardo" },
  "hover": { "glow": "#B0E4CC", "scale": 1.04 },
  "popup": { "openMs": 250, "closeMs": 200 },
  "ambient": { "dustCount": 24, "torchFlicker": true },
  "respectReducedMotion": true
}
```

---

## 6. TAHAP 1, FASE B: Pengelompokan (prioritas 1)

Hanya ini. Pintasan dan widget BELUM dikerjakan.

1. Klik hotspot `configurable` membuka pop up grup. Grup tampil sebagai tab atau seksi yang bisa dipilih. Item di tiap grup tampil dalam grid dengan ikon sederhana sesuai tipe.
2. Di dalam pop up, pengguna bisa:
   - menambah grup, mengganti nama grup, dan menghapus grup (dengan konfirmasi)
   - menambah item: pilih tipe, isi label dan target secara manual
   - mengedit dan menghapus item
   - memindahkan item ke grup lain lewat dropdown
3. Klik item memanggil `platform.openItem(item)`. Jika `settings.minimizeAfterLaunch` bernilai `true`, panggil `platform.windowMinimize()` setelahnya.
4. Hover pada item menampilkan label lengkap sebagai tooltip.
5. Hotspot `fixed` (kalender, note, bingkai) membuka pop up sederhana bertuliskan "Dikerjakan di tahap berikutnya".
6. Mode edit yang sudah ada tetap berfungsi: geser dan ubah ukuran `box`, lalu simpan ke `hotspots[].box` lewat `saveData`.
7. Semua perubahan disimpan lewat `platform.saveData`.

---

## 7. TAHAP 2: Spesifikasi versi asli (HANYA `native/SPEC.md`)

Isi `native/SPEC.md`:

- **Bahasa dan alat:** C# 12, .NET 8, WPF.
- **Build:** `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true`
- **Modul:**
  - `DataStore`: membaca dan menulis `medieval-desk.json` skema v1 di `%AppData%\MedievalDesk\`
  - `Visual`: membaca `config/visual.json`, efek dengan SkiaSharp
  - `Launcher`: membuka app, file, folder, dan shortcut lewat `Process.Start` atau `ShellExecute`. URL dibuka di browser default
  - `Hotkey`: `RegisterHotKey` (Ctrl+Alt+D)
  - `Tray`: `NotifyIcon` (Buka, Keluar)
  - `Stats`: `PerformanceCounter` (CPU, RAM)
  - `Weather`: `HttpClient` ke Open-Meteo
  - `DesktopLayer`: WorkerW, dibuktikan lewat spike (Gerbang B)
- **Gerbang B (spike, harus lulus sebelum versi asli dibuat):** jendela tetap di belakang semua jendela, tetap ada setelah Win+D, klik pada hotspot tembus ke ruangan, aman di DPI 100% sampai 150%, dan tidak bermasalah di multi-monitor. Jika gagal, catat alasannya dan usulkan cadangan Rust (crate `windows`).
- **Aturan keamanan:** tidak mengganti `explorer.exe`, tidak membuat service, tidak butuh hak admin, tidak menulis di luar `%AppData%\MedievalDesk\` dan folder kerja.

---

## 8. Aturan umum

- Jangan menghapus file. Jika tidak dipakai, pindahkan ke `legacy/tauri/` di dalam folder kerja.
- Jangan menulis path pribadi (misalnya `C:\Users\...`) di kode, data default, atau dokumentasi. Path folder kerja hanya boleh muncul di `AGENTS.md`.
- Jangan mengubah `README.md` dan jangan menambahkan lisensi.
- Jangan mengubah gambar wallpaper.
- Jangan memakai framework, CDN, atau npm dependency untuk prototipe.
- Menjalankan prototipe: buka `http://localhost/@GITHUB/medieval-desk/ProdukJadi/` jika Laragon aktif (folder ini ada di `www`), atau jalankan `python -m http.server 8000` dari folder kerja `ProdukJadi`. Jangan membuka lewat `file://`, karena `fetch` JSON akan gagal.

---

## 9. Verifikasi otomatis (dijalankan agen sendiri)

1. `node --check` untuk setiap file di `js/`. Hasilnya harus tanpa error.
2. Validasi `data/medieval-desk.json` dan `config/visual.json` dengan `JSON.parse` lewat node.
3. Cari `__TAURI__` di luar `legacy/` dan `.git/`. Hasilnya harus nol. **Scope: hanya folder kerja `ProdukJadi/`.**
4. Cari `http` di seluruh prototipe. Hanya `api.open-meteo.com` yang boleh muncul. Tidak boleh ada CDN. **Scope: hanya folder kerja `ProdukJadi/`.**
5. Cari `C:\Users` di seluruh prototipe dan docs. Hasilnya harus nol. **Scope: hanya folder kerja `ProdukJadi/`.**
6. Pastikan tidak ada file yang terhapus dibanding kondisi awal di `ProdukJadi/`.
7. Catat semua hasil verifikasi di laporan akhir.

---

## 10. Tes manual (untuk pengguna, dilakukan setelah laporan)

1. Ruangan tampil penuh, semua hotspot di posisi yang benar.
2. Klik rak buku: grup Rak 1 sampai Rak 4 tampil.
3. Tambah grup baru, ganti namanya, lalu hapus.
4. Tambah item URL, klik, dan halaman baru terbuka.
5. Tambah item aplikasi, klik, dan kotak info dengan tombol Salin muncul.
6. Pindahkan item ke grup lain.
7. Muat ulang halaman: semua perubahan tetap ada.
8. Export lalu import JSON: datanya sama.
9. Mode edit: geser dan ubah ukuran satu hotspot, muat ulang, posisinya tetap.
10. Kalender, note, dan bingkai membuka pop up "Dikerjakan di tahap berikutnya".

---

## 11. Laporan akhir (sekali, di akhir)

- Daftar file yang dibuat, dipindahkan, dan dihapus (seharusnya tidak ada yang dihapus).
- Hasil setiap butir verifikasi bagian 9.
- Daftar tes manual bagian 10 sebagai checklist kosong untuk pengguna.
- Isi `docs/PROTOTYPE-NOTES.md`, termasuk BLOCKER jika ada.
- Status bagian 12.

---

## 12. Status (agen memperbarui sendiri)

- [ ] A0 Koordinat asal dicatat di PROTOTYPE-NOTES.md
- [ ] A1 Tauri diarsipkan ke `legacy/tauri/`
- [ ] A2 `AGENTS.md` dibuat di root
- [ ] A3 Prototipe baru di root tanpa `__TAURI__`
- [ ] A4 Struktur folder sesuai bagian 2
- [ ] A5 `js/platform.js` memenuhi bagian 3
- [ ] A6 `data/medieval-desk.json` skema v1 (bagian 4)
- [ ] A7 `config/visual.json` (bagian 5)
- [ ] A8 `docs/CONTRACT.md` ditulis
- [ ] B1 Pop up grup dan hotspot configurable
- [ ] B2 Tambah, ubah nama, hapus grup
- [ ] B3 Tambah, edit, hapus item
- [ ] B4 Pindah item antar grup
- [ ] B5 `openItem` dan `minimizeAfterLaunch`
- [ ] B6 Tooltip dan pop up hotspot fixed
- [ ] B7 Mode edit tersimpan lewat `saveData`
- [ ] V Verifikasi bagian 9 selesai (atau dicatat sebagai BLOCKER)
- [ ] T2 `native/SPEC.md` dibuat, tanpa kode C#
- [ ] L Laporan akhir bagian 11 ditulis
