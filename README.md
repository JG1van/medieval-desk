# Medieval Desk

## Komposisi bahasa di repo (ringkas)
- JavaScript: 37.8%
- CSS: 20.6%
- Rust: 14.1%
- C#: 13.9%  ← ada implementasi native (WPF spike) di `Native-C#/MedievalDesk.Spike`
- TypeScript: 9.9%
- HTML: 1.9%
- Other: 1.8%

Catatan: bagian C# adalah jalur native (WPF spike). Pengguna yang ingin menjalankan versi native bisa membangun dari source atau menjalankan hasil publish (.exe).

## Menjalankan Native-C# (MedievalDesk.Spike)

Prasyarat:
- Windows
- .NET 8 SDK terpasang (https://dotnet.microsoft.com/download/dotnet/8.0)

Ada dua cara:

A) Untuk development (jalankan langsung)
1. Buka terminal di folder proyek:
   ```
   cd Native-C#\MedievalDesk.Spike
   ```
2. Jalankan:
   ```
   dotnet run --configuration Debug
   ```
   Aplikasi akan berjalan dari build Debug (bagus untuk pengembangan dan debugging).

B) Untuk pengguna / rilis (buat build release dan jalankan exe)
1. Publish release (hasil berupa exe siap dipakai):
   ```
   cd Native-C#\MedievalDesk.Spike
   dotnet publish -c Release -r win-x64 -o ./publish
   ```
   (Opsional: single-file self-contained tanpa perlu .NET di target)
   ```
   dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -o ./publish
   ```
2. Jalankan executable yang dihasilkan:
   ```
   ./publish/MedievalDesk.Spike.exe
   ```
3. Jika Anda menyiapkan paket Final, salin exe tersebut ke `Final/installers/MedievalDesk.Spike.exe` untuk distribusi atau attach ke GitHub Release.

Catatan singkat:
- Build artifacts (bin/, obj/, publish/) tidak disimpan di repo; mereka ada lokal setelah Anda menjalankan build/publish.
- Untuk distribusi publik lebih baik unggah exe ke GitHub Release atau buat installer daripada commit binary ke source tree.

Medieval Desk adalah eksperimen aplikasi desktop Windows bergaya medieval RPG yang mengubah desktop menjadi ruangan interaktif bertema perpustakaan dungeon. Objek di dalam ruangan berfungsi sebagai hotspot yang dapat membuka file, folder, shortcut (.lnk), atau URL, dengan mode edit untuk mengubah konfigurasi hotspot dan menyimpannya ke JSON.

Proyek ini ditulis dalam beberapa jalur pengembangan: prototype web (`Prototipe2`), prototype native Tauri (`Prototipe` dan `medieval-desk-native`), serta spike native C# WPF (`Native-C#`). Tujuan utamanya adalah mengeksplor pengalaman desktop yang estetik, personal, dan berguna.

---

## Isi README ini
1. Gambaran singkat & tujuan  
2. Struktur repo (apa ada dan fungsinya) — lengkap, bukan ringkasan saja  
3. Kontrak data hotspot (medieval-desk.json) — skema & contoh  
4. Cara menjalankan tiap jalur (runbook singkat)  
5. Hotkeys penting  
6. Troubleshooting & recovery (kasus nyata)  
7. Kriteria Gerbang B (SPEC-NATIVE.md) & roadmap  
8. File penting lain (logs, skrip) yang tidak selalu dijelaskan di README lama  
9. Contributing singkat (cara kerja, PR, agen AI)  
10. Catatan lisensi & langkah berikutnya

Baca lengkap agar pengembang / pengguna baru tidak kebingungan.

---

## 1) Tujuan singkat
Membuat desktop yang terasa seperti “ruangan” medieval yang fungsional — hotspot visual sebagai launcher personal. Fokus: estetika + utilitas (launcher, widget, widget permanen seperti kalender/note/photo).

---

## 2) Struktur repo — apa ada & peran praktisnya
Berikut semua item top-level yang ada sekarang dan fungsi masing-masing (apa yang sudah ada di README original + tambahan informasi teknis):

- `README.md` — file ini (gabungan panduan pengguna + developer).
- `Prototipe/` — prototype Tauri awal (client + `src-tauri/`), berisi skrip build (`build.cmd`), helper (`setup-and-check.ps1`), preview artwork (`preview-source.webp`).
  - Peruntukan: jalur native (butuh Rust, WebView2, MSVC).
- `Prototipe2/` — prototype web statis (HTML/CSS/JS). Cepat untuk uji UI, edit mode, popup, dan struktur JSON.
  - Isi penting: `index.html`, `js/` (platform.js, data.js, stage.js, popup.js, app.js), `css/`, `assets/`, `data/medieval-desk.json`, `README.md` (lebih detil penggunaan).
- `medieval-desk-native/` — implementasi Tauri lebih terstruktur (folder `client/` React+TS + `src-tauri/` Rust).
  - Isi penting: `package.json` (dev/build), `src-tauri/Cargo.toml`, `tauri.conf.json`, logs & diagnostik build.
- `Native-C#/` — .NET 8 WPF spike (MedievalDesk.sln, MedievalDesk.Spike/).
  - Peruntukan: validasi Gerbang B (window parenting, wallpaper restore, hotkey).
  - File: `SPEC-NATIVE.md`, `README.md` (checklist manual).
- `docs/` — dokumen rencana & troubleshooting: `RENCANA.md`, `DIAGNOSTIK-SPIKE.md`, `PERBAIKAN-OVERLAY.md`, `PERBAIKAN-SPIKE.md`, dll. Berisi catatan eksperimen, log, dan perbaikan.
- `Final/` — placeholder hasil final (kosong / belum dibuat).
- `data/` — arsip data lama (kosong).
- file lainnya: `package-lock.json`, `node_modules/`, dan artefak build/log.

Catatan penting:
- Gunakan `Prototipe2` untuk iterasi UI cepat (cukup server HTTP sederhana).  
- Gunakan `Prototipe` / `medieval-desk-native` untuk development Tauri (butuh native toolchain).  
- `Native-C#` adalah spike; kriteria kelulusan ada di `Native-C#/SPEC-NATIVE.md`.

---

## 3) Kontrak data hotspot (source of truth)
File: `Prototipe2/data/medieval-desk.json`

Skema ringkas:
- root:
  - `version`: nomor skema
  - `reference`: { width, height } — resolusi acuan untuk box%
  - `settings`: pengaturan umum (minimizeAfterLaunch, hotkey, location)
  - `hotspots`: array hotspot
  - `widgets`: array widget tetap

Contoh satu hotspot:
```json
{
  "id": "rak_buku",
  "label": "Rak Buku",
  "kind": "configurable",
  "box": { "x": 2.9, "y": 19.5, "w": 21.0, "h": 67.5 },
  "groups": [
    {
      "name": "Rak Buku",
      "items": [
        { "label": "Dokumen", "target": "D:\\@\\TOOL\\tools(operasi opencode).lnk" }
      ]
    }
  ],
  "displayMode": "direct"
}
```

Arti field:
- `box.x/y/w/h` adalah persen relatif ke `reference` (0..100). UI mengskalakan ke resolusi aktual.
- `displayMode`:
  - `direct` — klik buka target langsung
  - `popup-grid` — buka grid (3x3) tombol item
  - `popup-list` — buka daftar bertingkat per grup
- `items[].target` bisa berupa absolute path, `.lnk`, folder, atau URL — perlu disesuaikan pada mesin lain.

Praktis:
- Jika memindahkan JSON ke mesin lain: ganti semua `target` yang menunjuk ke path lokal.
- Jika `displayMode="direct"` tapi ada banyak item: implementasi memilih item pertama atau ubah menjadi popup-*.

---

## 4) Runbook singkat — cara menjalankan (dari clone bersih)

Prasyarat utama:
- Node.js (disarankan v18+ / v20+)
- npm
- Rust toolchain (rustup) + Tauri prerequisites (lihat docs Tauri)
- WebView2 Runtime (Windows)
- Microsoft C++ Build Tools / MSVC (Windows)
- .NET 8 SDK (untuk Native-C#)

A) Prototipe2 (web static — paling cepat)
```bash
cd Prototipe2
python -m http.server 8000
# buka http://localhost:8000
```
Catatan: jangan buka index.html langsung via file:// (fetch JSON akan gagal).

B) Prototipe (Tauri early)
```bash
cd Prototipe
npm install
npm run dev    # tauri dev (tergantung konfigurasi)
npm run build  # tauri build
```

C) medieval-desk-native (React + src-tauri Rust)
```bash
# dev
npm --prefix ./medieval-desk-native run dev

# build (client then tauri)
cd medieval-desk-native
npm install
npm run build
npm run tauri build
```

D) Native-C# WPF spike
Prasyarat: .NET 8 SDK
```bash
dotnet run --project "Native-C#\MedievalDesk.Spike" -c Release
```
Keluar bersih: tekan `Ctrl+Alt+Q` (agar wallpaper asli dikembalikan).

---

## 5) Hotkeys utama (default)
- Ctrl+Alt+D — tampilkan / sembunyikan lapisan ruangan  
- Ctrl+Alt+E — masuk / keluar mode edit (global)  
- Ctrl+E — alternatif mode edit saat jendela fokus  
- Ctrl+Alt+Q — keluar aplikasi & restore wallpaper asli (PENTING: gunakan ini, jangan kill process)  
- Esc — menutup form / popup atau sembunyikan lapisan (berlapis)

---

## 6) Troubleshooting & recovery (kasus nyata)
- Wallpaper tetap terpasang setelah crash / `taskkill`:
  - Gunakan Settings > Personalization > Background untuk kembalikan wallpaper manual.
  - Cek `%AppData%\MedievalDesk\original-wallpaper.txt` (jika spike menyimpan nilai asli).
- Jangan pakai Task Manager / taskkill untuk menutup aplikasi; dapat menyebabkan wallpaper tetap terpasang.
- Logs untuk debugging:
  - `medieval-desk-native/src-tauri/check.log`
  - `medieval-desk-native/src-tauri/check-verbose.log`
  - `spike-log.txt` (jika ada)
- Multi-monitor / DPI offsets:
  - Kesalahan posisi biasanya karena skala/DPI; gunakan `Prototipe2` untuk menyesuaikan `box` persen dan uji pada resolusi berbeda.
  - Lihat `docs/PERBAIKAN-OVERLAY.md` dan `docs/DIAGNOSTIK-SPIKE.md` untuk langkah perbaikan yang pernah dicatat.
- Build errors Tauri / Cargo:
  - Pastikan Rust toolchain & MSVC ada; baca `src-tauri/Cargo.toml` dan `tauri.conf.json` sebelum build.

---

## 7) Gerbang B (SPEC-NATIVE.md) & roadmap singkat
Kriteria Gerbang B (wajib lulus sebelum final native):
- Jendela tetap berada di belakang ikon desktop (WorkerW parenting atau metodologi yang stabil).
- Jendela tetap ada setelah Win+D (Show Desktop).
- Hotspot clickable (tembus) dan buka target sesuai konfigurasi.
- Aman pada DPI 100% dan 150%; multi-monitor tidak menimbulkan kesalahan posisi.
- Saat keluar, wallpaper asli dikembalikan.

Roadmap (prioritas singkat):
- Validasi & widget tetap: Kalender, Note, Bingkai Foto
- Export/import konfigurasi hotspot
- Preset layout & tema
- Robust path handling & validation
- Packaging & installer Windows

---

## 8) File & artefak penting (sering tidak dijelaskan di README lama)
- Skrip helper: `Prototipe/setup-and-check.ps1`, `Prototipe/build.cmd`
- Preview artwork: `Prototipe/preview-source.webp`
- Logs / diagnostik: `medieval-desk-native/src-tauri/check.log`, `check-verbose.log`, `full-output.txt`
- SPEC & docs: `Native-C#/SPEC-NATIVE.md`, `docs/RENCANA.md`, `docs/DIAGNOSTIK-SPIKE.md`, `docs/PERBAIKAN-OVERLAY.md`
- Tauri config: `medieval-desk-native/src-tauri/tauri.conf.json`, `Cargo.toml`
- WPF solution: `Native-C#/MedievalDesk.sln` dan project `MedievalDesk.Spike/`

---

## 9) Contributing singkat (developer & PR guide)
- Branching: buat branch dari `main` (mis. `feat/hotspot-grid`, `fix/wpf-dpi`).
- Commit message: prefix (`feat:`, `fix:`, `docs:`) + ringkasan.
- PR: jelaskan jalur yang diubah (Prototipe2 | Prototipe | medieval-desk-native | Native-C#) + langkah test lokal.
- Jika mengubah struktur JSON:
  - Bump `version` di JSON.
  - Tambah dokumentasi perubahan di README & `docs/CONTRACT.md`.
- Menggunakan agen AI (OpenCode):
  - Sertakan file referensi saat meminta perubahan (e.g. `SPEC-NATIVE.md`, `Prototipe2/data/medieval-desk.json`, `docs/RENCANA.md`).
  - Simpan keluaran agen sebagai draft & review manual sebelum merge.

Checklist minimal PR:
- Deskripsi perubahan & alasan
- Langkah reproduksi / test
- File yang diubah & dampak
- Jika mengubah JSON: contoh JSON baru & migrasi

---

## 10) Lisensi & langkah berikutnya
- Saat ini tidak ada file `LICENSE` di repo. Jika ingin kontribusi publik formal, tambahkan lisensi (MIT / Apache-2.0 / lainnya).
- Rekomendasi berikutnya:
  - Tambah `CONTRIBUTING.md` (proses PR, code style).
  - Tambah CI (GitHub Actions) untuk build client & lint.
  - Pertimbangkan menambahkan `docs/CONTRACT.md` formal untuk schema JSON.

---

## Ringkasan singkat (1 paragraf)
Medieval Desk adalah eksperimen desktop interaktif bertema medieval dengan tiga jalur: prototype web untuk iterasi cepat (`Prototipe2`), jalur Tauri untuk aplikasi native (`Prototipe`, `medieval-desk-native`), dan spike WPF native (`Native-C#`) untuk memvalidasi perilaku Windows khusus. Konfigurasi hotspot disimpan di `Prototipe2/data/medieval-desk.json` — periksa & sesuaikan `target` jika memindahkan ke mesin lain. Gunakan hotkey `Ctrl+Alt+Q` untuk keluar aman agar wallpaper asli direstore.

---

Terima kasih — README diperbarui. Jika Anda ingin, saya bisa juga membuat `CONTRIBUTING.md` dan melakukan commit bersamaan.