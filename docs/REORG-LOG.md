# REORG-LOG.md

Log reorg folder repo. Entri terbaru di atas.

## 2026-10-21 — Reorg final + sanitasi

| Dari | Ke | Status |
|------|----|--------|
| `medieval-desk-native/` | `prototipe-web/` | ✅ |
| `Native-C#/` | `app-desktop/` | ✅ |
| `Final/` | `app-final-gagal/` | ✅ |
| `Prototipe/`, `Prototipe2/` | — | Dihapus (prototipe lama) |
| root `data/` | — | Dihapus (data contoh) |

Sekalian: hapus installer/`bin`/`obj`/`node_modules`/`target` dari index, bersihkan path pribadi
(nama user Windows, drive lokal, folder sinkronisasi awan, dan path mesin pengembangan), ganti seed data
app-desktop ke `Assets/sample-data.json` yang generik, dan tambahkan `.github/workflows/release.yml`
untuk build otomatis.

Riwayat Git ditulis ulang (orphan, 3 commit) agar path lama tidak ikut terpublikasi.

## Reorg awal (arsip)

### Tabel folder & status

| Folder | Tindakan | Status |
|--------|--------|--------|
| docs/ | Dibuat (reencana) | ✅ Selesai |
| Native-C#/ | Dibuat (spike Gerbang B) | ✅ Selesai |
| Final/ | Dibuat (placeholder) | ✅ Selesai |

### File yang disalin

#### Native-C#/MedievalDesk.Spike/Assets
- wallpaper.png → Prototipe2/assets/wallpaper.png
- fonts/cardo-latin-400-normal.woff2
- fonts/cardo-700-normal.woff2
- fonts/grenze-gotisch-latin-400-normal.woff2
- fonts/grenze-gotisch-latin-700-normal.woff2

**Total: 5 file (1 wallpaper + 4 font woff2)**

### Ringkasan git status (tanpa commit)

```
?? Final/
?? Native-C#/
?? docs/
```

### Hasil dotnet --version + dotnet build

- `dotnet --version`: "The application '--version' does not exist."
- `dotnet build` (1x): "No .NET SDKs were found." — dicatat, dilanjutkan.

## Daftar cek manual (dari app-desktop/README.md)

- [x] Jendela berada di belakang semua jendela.
- [x] Setelah Win+D, wallpaper spike tetap terlihat.
- [x] Klik hotspot uji berfungsi.
- [x] Aman di DPI 100%.
- [x] Aman di DPI 150%.
- [x] Setelah program ditutup, wallpaper Windows asli kembali.

## Status langkah

- ✅ Folder dibuat
- ✅ docs/RENCANA.md ditulis (docs/)
- ✅ assets disalin
- ✅ app-desktop/ (csproj, manifest, sln, Win32.cs, App.xaml/cs, MainWindow.xaml/cs) ditulis
- ✅ SPEC-NATIVE.md (app-desktop/) ditulis
- ✅ README.md (app-desktop/) ditulis
- ✅ app-final-gagal/README.md ditulis
- ✅ .NET SDK terpasang — spike berhasil dibuild dan diuji
- ✅ REORG-LOG.md ditulis (file ini)
