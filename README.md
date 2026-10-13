# Medieval Desk

Aplikasi desktop modern dibangun dengan **Tauri v2** + **Vanilla JavaScript** + **Rust** untuk manajemen proyek dan catatan.

---

## ✨ Fitur Utama

- ⚡ **Startup cepat** — Binary Rust native, bundle ~5MB
- 🗂️ **Manajemen proyek** — Buat, edit, hapus proyek dengan metadata lengkap
- 📝 **Catatan Markdown** — Editor live-preview dengan syntax highlighting
- 🔍 **Pencarian cepat** — Filter proyek/catatan real-time
- 🌙 **Dark mode** — Tema gelap/terang otomatis mengikuti sistem
- 📦 **Portable** — Jalankan langsung tanpa install (Windows MSI tersedia)

---

## 🛠️ Tech Stack

| Layer | Teknologi |
|-------|-----------|
| Frontend | Vanilla JS (ES Modules), CSS Variables, IndexedDB |
| Backend | Rust 2021, Tauri v2 API, serde JSON |
| Build | `npm run build` → `cargo tauri build` |
| Bundle | NSIS Installer (`.msi`), Portable (`.exe`) |

---

## 🚀 Quick Start

### Prasyarat
- **Node.js 18+** & npm
- **Rust toolchain** (`rustup default stable`)
- **Windows 10/11** (target build saat ini)

### Install Dependensi
```bash
npm install
```

### Development Mode
```bash
npm run tauri dev
```
> Hot-reload frontend + Rust rebuild otomatis.

### Production Build
```bash
npm run build
```
Output:
- `src-tauri/target/release/medieval-desk.exe` — portable binary
- `src-tauri/target/release/bundle/msi/medieval-desk_*.msi` — Windows installer

---

## 📁 Struktur Proyek

```
medieval-desk/
├── index.html              # Entry point
├── src/
│   ├── main.js             # App bootstrap
│   ├── css/
│   │   └── style.css       # Design system (CSS vars)
│   ├── js/
│   │   ├── app.js          # State & routing
│   │   ├── db.js           # IndexedDB wrapper
│   │   ├── projects.js     # Project CRUD
│   │   ├── notes.js        # Note CRUD + markdown
│   │   └── ui.js           # DOM helpers
│   └── assets/             # Icons, fonts
├── src-tauri/
│   ├── Cargo.toml          # Rust deps
│   ├── tauri.conf.json     # Tauri config (bundle, window, security)
│   ├── src/
│   │   └── main.rs         # Tauri entry + commands
│   ├── icons/              # App icons (PNG + ICO)
│   └── build.rs            # Embedded assets
├── build.cmd               # Windows build wrapper
├── make_icon.ps1           # Generate .ico from PNG
├── package.json
└── .gitignore
```

---

## ⚙️ Konfigurasi

Edit `src-tauri/tauri.conf.json`:
- **Window**: size, title, resizable, decorations
- **Bundle**: identifier, publisher, copyright, icon paths
- **Security**: CSP, allowlist, capability permissions

---

## 🧪 Testing Checklist Manual

| Skenario | Status |
|----------|--------|
| `npm run tauri dev` — app terbuka, tidak error console | ⬜ |
| Buat proyek baru → simpan → reload → data persist | ⬜ |
| Buat catatan markdown → live preview render benar | ⬜ |
| Dark/light mode toggle → CSS vars update instan | ⬜ |
| Search filter proyek/catatan real-time | ⬜ |
| `npm run build` → MSI terbuat di `target/release/bundle/msi/` | ⬜ |
| Install MSI → shortcut Start Menu → jalankan app | ⬜ |
| Portable `.exe` jalan di PC lain tanpa Rust/Node | ⬜ |

---

## 📝 Catatan Migrasi

Proyek dipindahkan dari `C:\Users\ASUS\medieval-desk-svg` ke `C:\LARAGON-2025-1\www\@GITHUB\medieval-desk` dengan langkah:

1. **Robocopy** (exclude `node_modules`, `target`, `dist`, `*.log`)
2. **Fix path absolut** di `make_icon.ps1` & `build.cmd` → relative
3. **npm install** ✓
4. **npm run build** ✓ (compile OK, bundle butuh `.ico` → generate via `make_icon.ps1`)
5. **cargo check** ✓
6. **.gitignore** dibuat (exclude build artifacts, local config)
7. **README.md** dibuat (file ini)

> ⚠️ Folder sumber **TIDAK dihapus** — masih utuh di lokasi asal.

---

## 📄 Lisensi

MIT License — bebas gunakan, modifikasi, distribusi.

---

## 🤝 Kontribusi

PR & issue welcome. Untuk fitur besar, buka issue dulu untuk diskusi desain.