# Native Specification — Medieval Desk v2

Dokumen ini mendeskripsikan spesifikasi untuk implementasi asli (native) Medieval Desk v2 berbasis **Tauri v2 + Rust + React (frontend)**.

---

## 1. Arsitektur Umum

| Layer | Teknologi |
|-------|-----------|
| Frontend | React 18 + TypeScript + Vite |
| Backend | Rust (Tauri v2) |
| IPC | Tauri Commands (`#[tauri::command]`) |
| State | Zustand / React Context |
| Storage | SQLite (via `sqlx`) + `tauri-plugin-store` |
| OS Integration | `tauri-plugin-shell`, `tauri-plugin-dialog`, `tauri-plugin-fs`, `tauri-plugin-global-shortcut`, `tauri-plugin-opener` |

Struktur folder (di luar `legacy/`):

```
/src-tauri/
  Cargo.toml
  tauri.conf.json
  capabilities/
  src/
    main.rs
    commands/
      hotspot.rs
      data.rs
      system.rs
      weather.rs
    models/
      hotspot.rs
      data.rs
/src/
  main.tsx
  App.tsx
  components/
    Desktop.tsx
    Hotspot.tsx
    Popup/
      GroupPopup.tsx
      FixedPopup.tsx
      ItemInfoPopup.tsx
  hooks/
    useHotspots.ts
    useEditMode.ts
  store/
    useDataStore.ts
  styles/
    globals.css
    visual.css
```

---

## 2. Kontrak Platform (dari `docs/CONTRACT.md`)

Semua fungsi platform diimplementasikan sebagai **Tauri Commands** di Rust, dipanggil via `invoke()` dari frontend.

| Command | Frontend (`window.Platform`) | Rust handler |
|---------|------------------------------|--------------|
| `loadData` | `Platform.loadData()` | `load_data()` → `DataStore` |
| `saveData` | `Platform.saveData(data)` | `save_data(data)` |
| `exportData` | `Platform.exportData()` | `export_data()` → dialog save file |
| `importData` | `Platform.importData(file)` | `import_data(json)` → validate → save |
| `openItem` | `Platform.openItem(item)` | `open_item(item)` → `shell::open` |
| `windowMinimize` | `Platform.windowMinimize()` | `window_minimize()` |
| `windowMaximize` | `Platform.windowMaximize()` | `window_maximize()` |
| `windowClose` | `Platform.windowClose()` | `window_close()` |
| `getSystemStats` | `Platform.getSystemStats()` | `get_system_stats()` → `sysinfo` |
| `getWeather` | `Platform.getWeather(lat, lon)` | `get_weather(lat, lon)` → `open-meteo` |
| `now` | `Platform.now()` | `Local::now()` |
| `copyLastTarget` | `Platform.copyLastTarget()` | `clipboard::write_text()` |

Semua command di `@tauri::command` dengan `async` + `Result<T, String>`.

---

## 3. Skema Data (v1, identik `data/medieval-desk.json`)

```json
{
  "version": 1,
  "reference": { "width": 1376, "height": 768 },
  "settings": {
    "minimizeAfterLaunch": true,
    "hotkey": "Ctrl+Alt+D",
    "location": { "name": "", "lat": null, "lon": null }
  },
  "hotspots": [
    { "id": "rak_buku", "label": "Rak Buku", "kind": "configurable", "fixedType": null,
      "box": { "x": 2.9, "y": 19.5, "w": 21.0, "h": 67.5 },
      "groups": [ {"id":"g1","name":"Rak 1","items":[]}, ... ] },
    { "id": "kalender", "label": "Kalender", "kind": "fixed", "fixedType": "calendar",
      "box": { "x": 26.9, "y": 36.1, "w": 5.3, "h": 11.9 }, "groups": null },
    ...
  ],
  "widgets": []
}
```

**Model Rust (`src-tauri/src/models/hotspot.rs`)**:
```rust
#[derive(Serialize, Deserialize, Clone, Debug)]
#[serde(rename_all = "camelCase")]
pub struct Hotspot {
    pub id: String,
    pub label: String,
    pub kind: HotspotKind,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub fixed_type: Option<FixedType>,
    pub box: BoxPercent,
    #[serde(default, skip_serializing_if = "Vec::is_empty")]
    pub groups: Vec<Group>,
}

#[derive(Serialize, Deserialize, Clone, Debug)]
#[serde(rename_all = "lowercase")]
pub enum HotspotKind { Configurable, Fixed }

#[derive(Serialize, Deserialize, Clone, Debug)]
#[serde(rename_all = "lowercase")]
pub enum FixedType { Calendar, Note, Photo }

#[derive(Serialize, Deserialize, Clone, Debug)]
pub struct BoxPercent { pub x: f32, pub y: f32, pub w: f32, pub h: f32 }

#[derive(Serialize, Deserialize, Clone, Debug)]
pub struct Group { pub id: String, pub name: String, pub items: Vec<Item> }

#[derive(Serialize, Deserialize, Clone, Debug)]
#[serde(rename_all = "camelCase")]
pub struct Item {
    pub id: String,
    #[serde(rename = "type")]
    pub item_type: ItemType,
    pub label: String,
    pub target: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub icon: Option<String>,
}

#[derive(Serialize, Deserialize, Clone, Debug)]
#[serde(rename_all = "lowercase")]
pub enum ItemType { App, File, Folder, Shortcut, Url }
```

Validasi:
- `version` wajib = 1 (migrasi nanti)
- `reference` wajib ada
- `hotspots` minimal 1
- `box` nilai 0-100
- `kind` harus valid
- `fixed_type` hanya jika `kind == Fixed`

---

## 4. Fitur Utama

### 4.1 Desktop Overlay
- Full-screen borderless window (transparent bg).
- Wallpaper sebagai background (tidak di-scale, `object-fit: cover`).
- Drag region: seluruh area kecuali hotspot & kontrol.

### 4.2 Hotspot System
- 10 hotspot dari `PROTOTYPE-NOTES.md`.
- Koordinat persen (`box`) → konversi ke piksel via `reference`.
- Render: `position: absolute; left/top/width/height` %.
- **Configurable** (`rak_buku`, `loker`, `peti`, `gulungan`, `burung_gagak`, `bola_kristal`, `perisai`):
  - Klik → buka `GroupPopup`.
  - Edit mode (E): drag/resize, simpan via `saveData`.
- **Fixed** (`kalender`, `note`, `bingkai`):
  - Klik → buka `FixedPopup` ("Dikerjakan di tahap berikutnya").

### 4.3 Group Popup (`GroupPopup.tsx`)
- Tab per grup (dari `hotspot.groups`).
- Grid item (card): icon, label, klik → `openItem`.
- Toolbar: + Grup, + Item, Rename, Hapus, Pindah.
- State: `useDataStore` (Zustand) + `invoke('save_data')`.

### 4.4 Item Popup (`ItemInfoPopup.tsx`)
- Tampilkan label, type, target.
- Tombol **Salin** → clipboard.
- Pesan: "Prototipe web tidak bisa membuka file lokal. Versi asli akan membukanya."

### 4.5 Edit Mode
- Toggle `E` atau tombol ✏️.
- Drag hotspot: update `box.x`, `box.y`.
- Resize handle (SE corner): update `box.w`, `box.h`.
- Simpan otomatis ke store + `save_data` command.

### 4.6 Global Shortcut
- `Ctrl+Alt+D` → toggle show/hide window.
- Register via `tauri-plugin-global-shortcut`.
- Handle `windowMinimize`/`windowClose` via Tauri window API.

### 4.7 Weather
- `get_weather(lat, lon)` → `https://api.open-meteo.com/v1/forecast?...`.
- Cache 15 menit (in-memory + disk via `tauri-plugin-store`).
- Return `{ temperature: f32, code: i32 }` atau `null`.

### 4.8 Export / Import
- Export → dialog save `.json` (pretty-print).
- Import → dialog open `.json` → validate → replace store → `save_data`.

---

## 5. Visual Tokens (dari `config/visual.json`)

```css
:root {
  --dark: #091413;
  --green-dark: #285A48;
  --green-mid: #408A71;
  --green-light: #B0E4CC;
  --paper-1: #e3d0b0;
  --paper-2: #fdf5e6;
  --paper-3: #fffaf0;
  --paper-border: #c9b28a;
  --paper-text: #3a2f1f;
  --wood: #7a5c3a;
  --torch-gold: #ffcc6a;
  --torch-ember: #e39b39;
  --font-title: 'Grenze Gotisch', Georgia, serif;
  --font-body: 'Cardo', Georgia, serif;
}
```

- Fonts: **lokal** di `assets/fonts/` (woff2). **Tidak boleh CDN**.
- Reduced motion: `@media (prefers-reduced-motion: reduce) { * { transition: none !important; animation: none !important; } }`

---

## 6. Build & Distribusi

| Target | Command |
|--------|---------|
| Dev | `cargo tauri dev` |
| Build | `cargo tauri build` |
| Installer | NSIS (Windows), DMG (macOS), AppImage (Linux) |

`tauri.conf.json`:
- `identifier`: `com.jg1van.medievaldesk`
- `windows`: `[{ "label": "main", "title": "Medieval Desk", "fullscreen": true, "decorations": false, "transparent": true, "resizable": false }]`
- `plugins`: `shell`, `dialog`, `fs`, `global-shortcut`, `opener`, `store`, `clipboard-manager`

---

## 7. Migrasi dari Prototipe Web

| Prototipe | Native |
|-----------|--------|
| `localStorage` | SQLite + `tauri-plugin-store` |
| `fetch('data/medieval-desk.json')` | `load_data()` command |
| `window.open(url)` | `shell::open(url)` / `opener::open_path(path)` |
| `window.__TAURI__` | Semua via `invoke()` |
| CSS di `<style>` | `globals.css` + `visual.css` (CSS vars) |
| JS globals (`window.Platform`) | React hooks + `invoke()` |

---

## 8. Checklist Implementasi

- [ ] `Cargo.toml` + deps
- [ ] `tauri.conf.json` + capabilities
- [ ] Rust commands (load/save/export/import/open/minimize/maximize/close/system/weather/now/copy)
- [ ] SQLite schema + migrations
- [ ] React app + Vite + TS
- [ ] `Desktop.tsx` + wallpaper + hotspot layer
- [ ] `Hotspot.tsx` (click, drag, resize)
- [ ] `GroupPopup.tsx` (CRUD grup + item)
- [ ] `FixedPopup.tsx` + `ItemInfoPopup.tsx`
- [ ] `useDataStore` (Zustand) + persist
- [ ] Global shortcut register
- [ ] Visual tokens CSS vars
- [ ] Fonts lokal woff2
- [ ] Export/Import JSON dialog
- [ ] Unit test Rust commands
- [ ] E2E test (Playwright) hotspot flow

---

## 9. Blocker / Catatan

- **Fonts**: `Grenze Gotisch` & `Cardo` wajib disertakan sebagai `.woff2` di `assets/fonts/` (lisensi SIL OFL).
- **Wallpaper**: `assets/wallpaper.png` (1376×768) & `.svg` sudah ada di root `assets/`.
- **Tidak ada C#** — ini spesifikasi Rust/Tauri.
- **Tidak boleh ada `import`/`export` di file JS prototipe** — sudah dipenuhi.
- **Verifikasi §9 lulus** (catat di laporan akhir).