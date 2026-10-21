# SPEC-NATIVE.md

## Tahap ini hanya spike Gerbang B

Aplikasi penuh belum dibuat. Fokus hanya pada:

- Jendela tetap di belakang semua jendela (via WorkerW parenting).
- Hotspot di tengah (200×120) — klik=MessageBox.
- Tombol Esc menutup.
- Log ke spike-log.txt.

## Kontrak data (dibaca dari folder Assets/)

Kontrak data berikut dibaca dari folder `Assets/` sesuai `docs/RENCANA.md`:
- `Assets/sample-data.json` (skema v1, seed hotspot awal)
- `Assets/wallpaper.png` (wallpaper ruangan)

Modul app-desktop (DataStore, Visual, dll) hanya membaca kontrak ini.

## Kriteria Gerbang B (wajib lulus sebelum versi final)

1. Jendela tetap di belakang semua jendela; tetap ada setelah Win+D; klik hotspot tembus ke ruangan; aman di DPI 100% dan 150%; tidak bermasalah di multi-monitor.
2. Jika gagal kriteria #1: pindah ke cadangan Rust (tidak dibuat di tahap ini).

## Modul setelah Gerbang B (belum dikerjakan)

- DataStore — membaca/tulis kontrak JSON ke disk.
- Visual — SkiaSharp rendering di dalam jendela.
- Launcher — pakai Win32 P/Invoke untuk kontrol desktop.
- Hotkey — registrasi hotkey global.
- Tray — ikon sistem tray.
- Stats — catat metrik interaksi.
- Weather — tampilkan cuaca (opsional).

Status: **Belum dikerjakan — spike Gerbang B selesai dulu**.