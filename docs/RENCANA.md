# RENCANA Medieval Desk

> Catatan historis. Struktur folder sudah direorganized (lihat `README.md` utama). Nama lama dicatat agar mudah penelusuran.

## Struktur folder (kondisi sekarang)

| Folder | Isi | Status |
| --- | --- | --- |
| app-desktop/ | C# (.NET 8) + WPF, aplikasi utama | Aktif dikembangkan |
| app-final-gagal/ | Rilis WPF v1.0.0 (portable) | Arsip |
| prototipe-web/ | Tauri 2 + React + TypeScript + Rust | Eksperimen |
| docs/ | Catatan spike, diagnostik, perbaikan | Referensi |

Nama lama: `Prototipe/` (Tauri lama), `Prototipe2/` (web statis), `Native-C#/` → `app-desktop/`, `Final/` → `app-final-gagal/`, `medieval-desk-native/` → `prototipe-web/`.

## Keputusan bahasa

- Prototipe web (HTML, CSS, JavaScript): untuk menguji tampilan, alur, dan data. Tidak dipakai untuk launcher desktop.
- Launcher desktop: C# (.NET 8) + WPF, akses Win32 lewat P/Invoke, efek visual dengan WinForms host.
- Cadangan jika WPF gagal: Rust + crate windows + Direct2D (masih dipakai di `prototipe-web`).

## Kontrak data (dipakai semua bahasa)

- `app-desktop/MedievalDesk.Spike/Assets/sample-data.json` (skema v1)
- `app-final-gagal/samples/sample-layout.json`
- `prototipe-web/src-tauri/data/medieval-desk.json`

Semua jalur hanya membaca kontrak ini.

## Gerbang B (wajib lulus sebelum versi final)

Jendela tetap di belakang semua jendela; tetap ada setelah Win+D; klik hotspot tembus ke ruangan; aman di DPI 100% dan 150%; tidak bermasalah di multi-monitor. Gagal: pindah ke cadangan Rust.

## Urutan

1. Spike Gerbang B di WPF — selesai.
2. Modul DataStore, Visual, Launcher, Hotkey, Tray, Stats, Weather di WPF — sebagian berjalan di `app-desktop/`.
3. Rilis v1.0.0 — ada di `app-final-gagal/`.

## Status

- [x] WPF dibuat
- [x] Spike dibuild
- [x] Gerbang B diuji
- [x] Rilis v1.0.0 dibuat
- [ ] Iterasi lanjutan di `app-desktop/`
