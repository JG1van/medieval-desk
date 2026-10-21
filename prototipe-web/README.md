# prototipe-web

Prototipe Tauri 2 + React + TypeScript + Rust. Berisi layer wallpaper/hotspot versi web
yang dipakai untuk menguji tampilan, alur, dan bentuk data sebelum versi WPF.

## Isi

```text
prototipe-web/
├─ package.json          # skrip root + @tauri-apps/cli
├─ src-tauri/            # crate Rust: perintah data, hotspot, sistem, cuaca
│  ├─ data/medieval-desk.json
│  └─ tauri.conf.json
└─ client/               # frontend React (Vite)
   └─ src/
```

## Menjalankan

```powershell
npm ci
npm --prefix client ci
npm run dev          # hanya frontend Vite
npx tauri dev        # aplikasi desktop
npx tauri build      # binary rilis
```

## Kontrak data

JSON di `src-tauri/data/medieval-desk.json` memakai skema yang sama dengan versi WPF
(`version`, `reference`, `settings`, `hotspots[]`, `widgets[]`). Lihat `README.md` root repo.

Nama crate Rust masih `medieval-desk-native` (nama internal), sedangkan nama package npm sudah
`prototipe-web`; nama produk di `src-tauri/tauri.conf.json` adalah `Medieval Desk`.
Binary hasil build: `src-tauri/target/release/medieval-desk-native.exe`.

## Status

Eksperimen. Build terakhir terverifikasi (Rust release + frontend production build).
Tidak ada rilis pengguna; unduh dari workflow **Release** bila perlu.
