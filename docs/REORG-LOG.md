# REORG-LOG.md

## Tabel folder & status

| Folder | Tindakan | Status |
|--------|--------|--------|
| docs/ | Dibuat (reencana) | ✅ Selesai |
| Native-C#/ | Dibuat (spike Gerbang B) | ✅ Selesai |
| Final/ | Dibuat (placeholder) | ✅ Selesai |

## File yang disalin

### Native-C#/MedievalDesk.Spike/Assets
- wallpaper.png → Prototipe2/assets/wallpaper.png
- fonts/cardo-latin-400-normal.woff2
- fonts/cardo-latin-700-normal.woff2
- fonts/grenze-gotisch-latin-400-normal.woff2
- fonts/grenze-gotisch-latin-700-normal.woff2

**Total: 5 file (1 wallpaper + 4 font woff2)**

## Ringkasan git status (tanpa commit)

```
?? Final/
?? Native-C#/
?? docs/
```

## Hasil dotnet --version + dotnet build

### dotnet --version
Tidak dapat dijalankan: "The application '--version' does not exist."

### dotnet build (1x)
Pesan error: "The command could not be loaded, possibly because: ... No .NET SDKs were found."

## Daftar cek manual (dari Native-C#/README.md)

- [ ] Jendela berada di belakang semua jendela.
- [ ] Setelah Win+D, wallpaper spike tetap terlihat.
- [ ] Klik hotspot uji berfungsi.
- [ ] Aman di DPI 100%.
- [ ] Aman di DPI 150%.
- [ ] Setelah program ditutup, wallpaper Windows asli kembali.

## Status langkah

- ✅ Folder dibuat
- ✅ docs/RENCANA.md ditulis (docs/)
- ✅ assets disalin
- ✅ Native-C#/ (csproj, manifest, sln, Win32.cs, App.xaml/cs, MainWindow.xaml/cs, .gitignore) ditulis
- ✅ SPEC-NATIVE.md (Native-C#/) ditulis
- ✅ README.md (Native-C#/) ditulis
- ✅ Final/README.md (Final/) ditulis
- ✅ dotnet build gagal (tidak ada SDK) — dicatat, dilanjutkan.
- ✅ REORG-LOG.md ditulis (file ini)

## Perhatian

.NET SDK tidak terpasang; spike tidak dapat dibuild. Rencana berlanjut ke fase selanjutnya (umpan balik pengguna, install SDK, jalankan build).