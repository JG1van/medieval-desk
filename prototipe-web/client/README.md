# Medieval Desk — Client (React + Vite)

Frontend prototipe Medieval Desk: layer wallpaper + editor hotspot.
Dijalankan lewat Tauri (`npx tauri dev` dari folder `prototipe-web`), bukan standalone.

## Perintah

```powershell
npm ci
npm run dev      # Vite dev server
npm run build    # output statis ke client/dist
npm run lint     # oxlint
```

## Struktur

```text
client/
├─ index.html
├─ src/
│  ├─ App.tsx, main.tsx
│  ├─ components/AssignModal.tsx
│  ├─ hooks/useDataStore.ts, useHotspots.ts, useEditMode.ts
│  ├─ store/useDataStore.ts
│  ├─ styles/visual.css
│  └─ assets/    # wallpaper, hero, font woff2
└─ vite.config.ts
```

Data diambil dan disimpan lewat perintah Rust di `src-tauri/src/commands/`,
bukan `localStorage`, sehingga format JSON sama dengan versi WPF.
