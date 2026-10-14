# Medieval Desk

Medieval Desk adalah aplikasi desktop Windows berupa wallpaper interaktif bertema medieval RPG yang mengubah desktop menjadi ruang kerja bergaya perpustakaan dungeon.

Berbagai objek di dalam ruangan—seperti rak buku, loker, peti, perisai, burung gagak, dan bola kristal—dapat berfungsi sebagai hotspot yang dikustomisasi untuk membuka aplikasi, file, folder, shortcut, atau situs web.

---

## Struktur Proyek

```
medieval-desk/
├── data/                     # Folder data (staging, saat ini kosong)
├── medieval-desk-native/     # Aplikasi Tauri + React (source & build)
│   ├── client/               # Kode React/Vite UI
│   ├── src-tauri/            # Backend Rust + konfigurasi Tauri
│   ├── package.json
│   └── package-lock.json
├── ProdukJadi/               # Prototipe web + dokumentasi lengkap
│   ├── assets/               # Wallpaper, font, visual
│   ├── config/               # visual.json warna/effek
│   ├── data/                 # medieval-desk.json skema v1
│   ├── docs/                 # PLAN.md, CONTRACT.md, PROTOTYPE-NOTES.md
│   ├── js/                   # platform.js, data.js, stage.js, popup.js, app.js
│   ├── legacy/               # Arsip Tauri lama
│   ├── native/               # SPEC.md versi C# .NET 8 WPF (belum dikerjakan)
│   ├── index.html
│   ├── README.md, AGENTS.md, PLAN.md
│   └── .gitignore
└── Prototipe/                # Prototipe awal (vanilla JS/HTML)
    ├── assets/
    ├── src/                  # HTML/CSS/JS logika
    ├── src-tauri/
    ├── .gitignore
    ├── build.cmd
    ├── package.json
    └── preview-source.webp
```

---

## Fitur Utama

1. **Hotspot Universal** — hampir setiap objek di dalam ruangan dapat digunakan sebagai hotspot interaktif. Pengaturan nama, fungsi, area klik berbasis persentase, efek glow, label tooltip, dan edit mode.

2. **Edit Mode** — masuk dengan tombol **E** (atau klik tombol pensil di versi lama). Geser/ubah ukuran area klik, ubah nama/tujuan hotspot, simpan konfigurasi ke localStorage/SQLite.

3. **Peluncur Aplikasi** — hotspot sebagai launcher visual untuk file, folder, shortcut Windows (.lnk), atau URL. Setelah target dibuka, aplikasi diminimalkan otomatis (`settings.minimizeAfterLaunch`).

4. **Kontrol Jendela** — tombol minimize/maximize/restore/close yang disesuaikan tema medieval, plus global hotkey **Ctrl + Alt + D** untuk menampilkan/sembunyikan aplikasi.

---

## Teknologi

| Teknologi | Kegunaan |
|-----------|----------|
| Tauri v2 | Framework aplikasi desktop |
| Rust | Backend & integrasi sistem operasi |
| React + Vite | Frontend UI (medieval-desk-native) |
| HTML/CSS/JS (Vanilla) | Prototipe web (ProdukJadi) |
| JSON | Penyimpanan konfigurasi lokal |
| SVG/PNG | Aset visual & wallpaper |
| Grenze Gotisch / Cardo | Tipografi bergaya gothic |
| OpenCode | Agen AI membantu pengembangan |

---

## Persyaratan

- Windows 10 / 11
- **Node.js** (https://nodejs.org/)
- **Rust** (https://rustup.rs/)
- Microsoft C++ Build Tools
- WebView2 Runtime

---

## Instalasi & Menjalankan (medieval-desk-native)

```bash
# 1. Clone repository
git clone https://github.com/JG1van/medieval-desk.git

# 2. Masuk ke direktori proyek
cd medieval-desk

# 3. Instal dependency
npm install

# 4. Jalankan mode pengembangan
npm run tauri dev

# 5. Build aplikasi (produksi)
npm run tauri build
```

Hasil build:
- `src-tauri/target/release/medieval-desk-native.exe` (≈ 32.7 MB)
- Installer MSI: `src-tauri/target/release/bundle/msi/Medieval Desk_0.1.0_x64_en-US.msi` (≈ 20.3 MiB)
- Installer NSIS: `src-tauri/target/release/bundle/nsis/Medieval Desk_0.1.0_x64-setup.exe` (≈ 18.4 MiB)

---

## Cara Penggunaan

1. Jalankan aplikasi. Wallpaper ruangan medieval muncul sebagai antarmuka desktop interaktif.
2. Arahkan kursor ke objek untuk melihat efek visual dan label, lalu klik untuk menjalankan fungsi.
3. Tekan **E** untuk masuk **Edit Mode**. Geser/ubah ukuran area klik, klik ikon gear pada hotspot untuk mengubah nama/tujuan.
4. Tekan **Esc** untuk menutup popup / keluar Edit Mode.
5. Tekan **Ctrl + Alt + D** untuk menampilkan/sembunyikan aplikasi (global hotkey).
6. Gunakan tombol minimize/maximize/restore/close di pojok kanan atas.

---

## Zona Interaktif (Hotspot)

| Objek | Fungsi |
|-------|--------|
| Rak Buku | Hotspot configurable (4 grup: Rak 1–4) |
| Loker | Hotspot configurable (2 grup: Pintu Kiri/Kanan) |
| Peti | Hotspot configurable (1 grup: Downloads) |
| Gulungan | Hotspot configurable (1 grup: Umum) |
| Burung Gagak | Hotspot configurable (1 grup: Umum) |
| Bola Kristal | Hotspot configurable (1 grup: Umum) |
| Perisai | Hotspot configurable (1 grup: Umum) |
| Kalender | Fixed → buka Outlook Calendar (`outlookcal:`) |
| Note | Fixed → buka Notepad |
| Bingkai | Fixed → foto (mode `direct` dipaksa oleh prototipe) |

Catatan: Kalender, Note, dan Bingkai memiliki fungsi tetap (tidak bisa dikustomisasi targetnya di versi saat ini).

---

## Perubahan Terakhir (Sesi Ini)

- Tombol **✎** di pojok kanan atas **dihapus**; edit mode hanya via tombol **E** (tooltip: `E: edit · Esc: tutup`).
- Toast transient untuk **Note/Kalender** menampilkan pesan "Notepad dibuka"/"Kalender dibuka" (1400 ms), sesuai prototipe `showTransientPopup`.
- **Bingkai (photo)** tetap `displayMode: "direct"` — prototipe memaksa photo ke direct via `LOCKED_DISPLAY_MODE_TYPES=['note','calendar','photo']→forced direct`.
- **Build sukses**: `cargo check` ok, `npm run build` ok, `npx tauri build` sukses — EXE 32.7 MB, MSI 20.3 MiB, NSIS 18.4 MiB.

---

## Keterbatasan

- Jendela dapat tertutup oleh jendela lain; gunakan **Ctrl + Alt + D** atau taskbar untuk kembali.
- Posisi hotspot pada multi-monitor & skala DPI masih perlu diuji lebih lanjut.
- Path file/folder/shortcut/URL perlu ditangani dengan baik jika target tidak tersedia.

---

## Rencana Pengembangan

- Backup/ekspor/impor konfigurasi hotspot.
- Preset layout desktop & tema visual.
- Dukungan multi-monitor yang lebih baik.
- Penyesuaian posisi hotspot berdasarkan resolusi & DPI.
- Validasi target & penanganan error yang lebih informatif.
- Pengaturan efek visual & shortcut keyboard.
- Dokumentasi/tutorial penggunaan di dalam aplikasi.

---

## Status Proyek

- **Proyek pribadi** — masih dalam pengembangan.
- Dikembangkan secara bertahap dengan fokus pada pengalaman desktop yang personal, estetis, dan fungsional melalui perpaduan wallpaper fantasy, zona interaktif, serta launcher yang dapat dikustomisasi.

---

## Inspirasi & Proses Pengembangan

- **BentoDesk** — https://github.com/ZRainbow1275/bentodesk
- **DeskRealm** — https://github.com/ekimaku/DeskRealm

Kedua proyek tersebut menjadi referensi konseptual. Tidak ada kode dari proyek tersebut yang disalin.

Dikembangkan secara mandiri, dengan proses penulisan kode yang dibantu agen AI melalui **OpenCode** menggunakan pendekatan vibe coding. Konsep, tata letak, tema visual, dan keputusan fitur ditentukan selama proses pengembangan, sementara agen AI membantu implementasi kode.

---

Dikembangkan sebagai proyek personal untuk menghadirkan pengalaman desktop Windows yang lebih unik melalui perpaduan estetika medieval RPG dan fungsionalitas sehari-hari.