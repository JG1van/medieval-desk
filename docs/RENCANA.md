# RENCANA Medieval Desk

## Struktur folder

| Folder | Isi | Status |
| --- | --- | --- |
| Prototipe/ | Tauri lama | Tidak diubah |
| Prototipe2/ | Web prototipe (HTML, CSS, JS), hasil ganti nama dari ProdukJadi | Tahap 1, sudah ada |
| Native-C#/ | C# (.NET 8) + WPF, spike Gerbang B | Dibuat sekarang |
| Final/ | Hasil final | Placeholder, dibuat nanti |
| medieval-desk-native/ | Tauri + React + Rust | Hanya referensi, tidak diubah |

## Keputusan bahasa

- Prototipe web (HTML, CSS, JavaScript): untuk menguji tampilan, alur, dan data. Tidak dipakai untuk launcher desktop.
- Launcher desktop: C# (.NET 8) + WPF, akses Win32 lewat P/Invoke, efek visual dengan SkiaSharp.
- Cadangan jika spike C# gagal: Rust + crate windows + Direct2D.

## Kontrak data (dipakai semua bahasa)

- Prototipe2/data/medieval-desk.json (skema v1)
- Prototipe2/config/visual.json
- Prototipe2/docs/CONTRACT.md

Native-C# dan Final hanya membaca kontrak ini.

## Gerbang B (wajib lulus sebelum versi final)

Jendela tetap di belakang semua jendela; tetap ada setelah Win+D; klik hotspot tembus ke ruangan; aman di DPI 100% dan 150%; tidak bermasalah di multi-monitor. Gagal: pindah ke cadangan Rust.

## Urutan

1. Spike Gerbang B di Native-C# (tahap ini).
2. Jika lulus: modul DataStore, Visual, Launcher, Hotkey, Tray, Stats, Weather di Native-C#.
3. Jika lulus dan disetujui pengguna: hasil final di Final/.

## Status

- [ ] Native-C# dibuat
- [ ] Spike dibuild
- [ ] Gerbang B diuji pengguna
- [ ] Final dibuat (nanti)
