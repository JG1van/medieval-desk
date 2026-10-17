# MedievalDesk Native-C# Spike — Manual Checklist

## Verifikasi manual (jalankan setelah build & jalankan .exe)

| # | Cek | Expected |
|---|-----|----------|
| 1 | Jendela berada di belakang semua jendela | ✅ |
| 2 | Setelah Win+D, wallpaper spike tetap terlihat | ✅ |
| 3 | Klik hotspot (kotak 200×120 di tengah) menampilkan MessageBox "Klik tembus OK" | ✅ |
| 4 | Aman di DPI 100% | ✅ |
| 5 | Aman di DPI 150% | ✅ |
| 6 | Setelah program ditutup, wallpaper Windows asli kembali | ✅ |

---

Jika semua cek lulus → lanjut ke modul DataStore/Visual/...
Jika ada yang gagal → cek gerbang Rust cadangan (tidak dibuat di tahap ini).