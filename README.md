# Medieval Desk

Medieval Desk adalah aplikasi desktop Windows bertema ruangan perpustakaan dungeon: wallpaper berubah menjadi ruangan, dan objek di dalamnya menjadi hotspot untuk membuka file, folder, shortcut, atau URL. Konfigurasi hotspot disimpan sebagai JSON sehingga bisa diedit dan dipindah antar mesin.

Repo ini berisi tiga jalur pengembangan, masing-masing berdiri sendiri:

| Folder | Isi | Status |
|---|---|---|
| `app-desktop/` | Aplikasi WPF (.NET 8) — versi terbaik dan paling lengkap | Aktif dikembangkan |
| `app-final-gagal/` | Rilis WPF v1.0.0 (portable) | Arsip, tidak dilanjutkan |
| `prototipe-web/` | Prototipe Tauri 2 + React + TypeScript + Rust | Eksperimen |

Catatan: `app-final-gagal/` berstatus arsip — hasil tampilannya berbeda dengan `app-desktop/` dan gambar hotspot (diam maupun saat hover) masih kurang bagus dilihat. Detail kekurangannya ada di bagian "Kegagalan yang Diketahui" pada `app-final-gagal/README.md`.

Dokumentasi dan catatan eksperimen tiap jalur ada di `docs/`, `app-desktop/README.md`, dan `app-final-gagal/docs/`.

```text
medieval-desk/
├─ app-desktop/        # WPF .NET 8 — versi terbaik, aktif dikembangkan
├─ app-final-gagal/    # rilis WPF v1.0.0 (arsip, ada self-test)
├─ prototipe-web/      # Tauri 2 + React + TypeScript + Rust (eksperimen)
├─ docs/               # catatan spike, diagnostik, perbaikan
├─ .github/workflows/  # build otomatis (Release)
└─ README.md
```

---

## Unduh (pengguna akhir)

Binary tidak disimpan di dalam repo (limit GitHub 100 MB per file). Build otomatis dibuat oleh GitHub Actions:

- Tab **Actions** → pilih workflow **Release** → **Run workflow** → unduh artifact `app-final-gagal-win-x64.zip` / `app-desktop-win-x64.zip`.
- Push tag `v1.0.0` otomatis membuat GitHub Release berisi kedua zip.

Aplikasi bersifat portable: tidak ada installer, tidak menulis ke `Program Files`, dan tidak butuh .NET terpasang bila di-build self-contained.

---

## Menjalankan tiap jalur

### `app-desktop/` (WPF, .NET 8) — recommended

Prasyarat: Windows 10/11 + .NET 8 SDK (hanya untuk build; pengguna cukup unduh hasil publish).

Development:

```powershell
dotnet run --project "app-desktop\MedievalDesk.Spike" -c Release
```

Build siap-distribusi (self-contained, tanpa syarat .NET di mesin tujuan):

```powershell
dotnet publish "app-desktop\MedievalDesk.Spike\MedievalDesk.Spike.csproj" `
  -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true `
  -o .\publish
```

Hasil publish: `MedievalDesk.Spike.exe` + folder `Assets\` (wallpaper dan data contoh). Jalankan `MedievalDesk.Spike.exe` dari folder tersebut.

### `app-final-gagal/` (WPF v1.0.0, arsip)

```powershell
dotnet publish "app-final-gagal\src\MedievalDesk\MedievalDesk.csproj" `
  -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true `
  -o .\publish
```

Folder `Assets\` wajib ikut di samping executable (wallpaper + data contoh). Jalankan `MedievalDesk.exe`.

### `prototipe-web/` (Tauri 2 + React)

Prasyarat: Node.js 20+, Rust stable, Visual Studio Build Tools (MSVC), WebView2 Runtime.

```powershell
cd prototipe-web
npm ci
npm --prefix client ci
npm run dev          # client Vite
npx tauri dev        # aplikasi desktop
npx tauri build      # bundel rilis
```

Data Rust memakai `src-tauri/data/medieval-desk.json`; frontend Vite ada di `client/`.

---

## Data hotspot (kontrak JSON)

Sumber data contoh:

- `app-desktop/MedievalDesk.Spike/Assets/sample-data.json`
- `app-final-gagal/samples/sample-layout.json`
- `prototipe-web/src-tauri/data/medieval-desk.json`

Struktur utama:

```json
{
  "version": 1,
  "reference": { "width": 1376, "height": 768 },
  "settings": { "minimizeAfterLaunch": true, "hotkey": "Ctrl+Alt+D" },
  "hotspots": [
    {
      "id": "rak_buku",
      "label": "Rak Buku",
      "kind": "configurable",
      "box": { "x": 2.9, "y": 19.5, "w": 21.0, "h": 67.5 },
      "displayMode": "popup-list",
      "groups": [
        { "id": "g1", "name": "Rak 1", "items": [ { "label": "Notepad", "target": "notepad.exe" } ] }
      ]
    }
  ],
  "widgets": []
}
```

- `box` memakai persen (0-100) terhadap `reference`, lalu diskalakan ke resolusi layar.
- `displayMode`: `direct` (buka langsung), `popup-list`, `popup-grid`.
- `items[].target`: file, folder, `.lnk`, atau URL. Variabel lingkungan seperti `%USERPROFILE%` otomatis diekspansi. **Sesuaikan dengan mesin Anda** — contoh di repo memakai target generik.

Data aktif tersimpan di `%APPDATA%\MedievalDesk\data\medieval-desk.json`. Edit di aplikasi (mode edit) atau salin file itu secara manual.

---

## Hotkey

| Pintasan | Fungsi |
|---|---|
| `Ctrl+Alt+D` | Tampilkan / sembunyikan lapisan ruangan |
| `Ctrl+Alt+E` | Masuk / keluar mode edit |
| `Ctrl+Alt+Q` | Keluar aplikasi dan kembalikan wallpaper asli |

Gunakan `Ctrl+Alt+Q` untuk menutup. Menutup paksa (Task Manager) dapat meninggalkan wallpaper terpasang.

---

## Troubleshooting

- **Wallpaper tidak kembali setelah crash**: buka Settings → Personalization → Background, atau jalankan `app-final-gagal/tools/restore-wallpaper.ps1 -ImagePath <path-asli>`.
- **Log**: `%APPDATA%\MedievalDesk\logs\` (app-final-gagal) dan `spike-log.txt` di folder aplikasi (app-desktop).
- **Data awal tidak ditemukan**: pastikan folder `Assets\` ada di samping executable (`wallpaper.png` + `sample-data.json` untuk app-desktop).
- **Aplikasi langsung minimize saat start**: `settings.minimizeAfterLaunch` di JSON, atau tekan `Ctrl+Alt+D`.
- **Posisi hotspot meleset**: sesuaikan nilai persen `box` pada JSON; catatan multi-monitor/DPI ada di `docs/PERBAIKAN-OVERLAY.md`.
- **Font "Grenze Gotisch"** tidak terpasang di Windows: WPF otomatis memakai font cadangan (Georgia).
- **Hotkey tidak merespons**: aplikasi lain mungkin memakai `Ctrl+Alt+D/E/Q`; tutup dulu.

---

## Build otomatis (GitHub Actions)

Workflow `.github/workflows/release.yml`:

| Trigger | Hasil |
|---|---|
| Manual (Actions → Release → Run workflow) | Artifact zip `app-final-gagal-win-x64.zip` dan `app-desktop-win-x64.zip` (self-contained, sudah diuji `--self-test`) |
| Push tag `v1.0.0` | GitHub Release berisi kedua zip + release notes otomatis |
| — | Job Tauri (`prototipe-web`) opsional; bila gagal, rilis WPF tetap terbit |

Semua build memakai `windows-latest`, .NET SDK 8, Node 20, dan Rust stable.
