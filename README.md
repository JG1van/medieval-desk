# Medieval Desk

Medieval Desk adalah proyek eksperimen aplikasi desktop Windows bertema medieval RPG. Ruangannya berupa perpustakaan dungeon, dan setiap benda di dalamnya (rak buku, loker, peti, perisai, burung gagak, bola kristal, kalender, note, dan bingkai) dirancang sebagai tempat untuk membuka file, folder, aplikasi, atau URL.

**Status: eksperimen aktif.** Wallpaper ruangan dipasang secara otomatis saat spike WPF dijalankan. Lapisan ruangan dapat ditampilkan di atas desktop, benda dapat diklik untuk membuka isinya, dan setiap benda dapat diisi melalui mode edit yang menyimpan datanya ke JSON.

---

## Tentang Proyek Ini

**Apa:** aplikasi desktop Windows berbentuk ruangan medieval.

**Fungsi yang sudah berjalan:**

- Lapisan ruangan tampil di atas desktop (jendela transparan, selalu di atas).
- Klik benda untuk membuka isinya (langsung atau lewat popup).
- Mode edit untuk mengisi item pada setiap benda dan menyimpannya ke JSON.
- Pintasan papan ketik untuk menampilkan, menyembunyikan, mengedit, dan keluar.

**Fungsi yang direncanakan:**

- Kalender, note, dan bingkai foto memiliki fungsi tetap (belum diaktifkan).
- Pengujian di beberapa monitor dan skala DPI yang berbeda.

**Yang sudah dibuat:**

- Gambar ruangan sebagai wallpaper (dibuat dengan AI image generation).
- Prototipe web untuk menguji tampilan dan data.
- Aplikasi Tauri yang bisa dibuka sebagai jendela biasa.
- Spike WPF yang menampilkan ruangan sebagai jendela transparan selalu di atas, lengkap dengan popup, mode edit, dan penyimpanan data.

**Yang belum berfungsi:**

- Hasil akhir (`Final/`) belum dibuat.
- Belum diuji di beberapa monitor dan skala DPI yang berbeda.

---

## Bahasa dan Teknologi

| Bagian | Bahasa / Teknologi |
|---|---|
| Prototipe web | HTML, CSS, JavaScript |
| Aplikasi Tauri | Rust (backend), React + TypeScript (tampilan) |
| Spike WPF | C# (.NET 8), WPF, P/Invoke Win32 |
| Data dan konfigurasi | JSON |
| Tipografi | Grenze Gotisch, Cardo |

---

## Isi Folder

| Folder | Isi | Bisa dibuka? |
|---|---|---|
| `Prototipe2/` | Prototipe web dengan wallpaper, data, dan konfigurasi | Ya, lewat browser (lihat di bawah) |
| `Prototipe/` | Aplikasi Tauri versi awal, berupa HTML, CSS, JS, dan Rust | Hanya build |
| `medieval-desk-native/` | Aplikasi Tauri dengan React dan Rust | Ya, `.exe` setelah build |
| `Native-C#/` | Solusi `MedievalDesk.sln` berisi spike WPF `MedievalDesk.Spike/`, ditambah `SPEC-NATIVE.md` dan `README.md` | Ya, lewat `dotnet run` |
| `Final/` | Placeholder untuk hasil akhir | Belum ada isi |
| `docs/` | Rencana, log, dan laporan pengujian | Berupa dokumen teks |
| `data/` | Data lama | Kosong |

---

## Cara Menjalankan

### 1. Pasang wallpaper ruangan

Spike WPF memasang wallpaper ruangan secara otomatis saat dijalankan, dan mengembalikan wallpaper asli saat keluar. Langkah manual berikut hanya diperlukan bila ingin memakai wallpaper ruangan tanpa menjalankan spike.

1. Klik kanan desktop, lalu pilih **Personalize > Background**.
2. Pilih **Picture**, klik **Browse**, lalu pilih `Prototipe2\assets\wallpaper.png`.
3. Pilih **Fill**.

### 2. Prototipe web (`Prototipe2/`)

Dengan Laragon aktif, buka:

```
http://localhost/@GITHUB/medieval-desk/Prototipe2/
```

Tanpa Laragon, jalankan dari folder `Prototipe2`:

```
python -m http.server 8000
```

Lalu buka `http://localhost:8000`. Jangan buka `index.html` langsung dari file, karena data JSON tidak akan terbaca.

### 3. Aplikasi Tauri (`medieval-desk-native`)

Jalankan langsung:

```
cd "medieval-desk\medieval-desk-native\src-tauri\target\release"
.\medieval-desk-native.exe
```

Jika `.exe` belum ada, build dulu:

```
cd medieval-desk\medieval-desk-native
npm install
npm run tauri build
```

### 4. Spike WPF (`Native-C#/`)

Membutuhkan .NET 8 SDK.

```
cd "C:\LARAGON-2025-1\www\@GITHUB\medieval-desk"
dotnet run --project "Native-C#\MedievalDesk.Spike" -c Release
```

Saat dijalankan, program memasang wallpaper ruangan dan menampilkan lapisan ruangan. Tekan **Ctrl+Alt+Q** untuk keluar.

---

## Pintasan Papan Ketik (Hotkey)

Seluruh interaksi dengan lapisan ruangan dikendalikan lewat pintasan papan ketik. Pintasan bekerja secara global, artinya tetap aktif walaupun jendela spike tidak sedang terfokus.

### Ringkasan

| Hotkey | Fungsi singkat |
|---|---|
| **Ctrl+Alt+D** | Tampilkan atau sembunyikan lapisan ruangan |
| **Ctrl+Alt+E** | Masuk atau keluar dari mode edit |
| **Ctrl+E** | Sama dengan Ctrl+Alt+E, khusus saat jendela spike sedang terfokus |
| **Ctrl+Alt+Q** | Keluar dari aplikasi dan kembalikan wallpaper asli |
| **Esc** | Menutup form, lalu popup, lalu lapisan (berlapis) |

### Ctrl+Alt+D — Tampilkan atau sembunyikan lapisan

Pintasan ini digunakan untuk menampilkan dan menyembunyikan lapisan ruangan tanpa menutup aplikasi.

- **Untuk apa:** menyembunyikan ruangan sesaat ketika ingin melihat ikon desktop, membuka jendela lain, atau bekerja tanpa gangguan lapisan.
- **Kapan dipakai:** saat lapisan ruangan menutupi sesuatu yang ingin dilihat, atau saat ingin menampilkan kembali setelah disembunyikan.
- **Cara pakai:** tekan **Ctrl+Alt+D** sekali untuk menyembunyikan, tekan sekali lagi untuk menampilkan kembali.
- **Yang terjadi:** jendela transparan berisi ruangan disembunyikan atau ditampilkan kembali pada posisi semula.
- **Catatan:** wallpaper ruangan tetap terpasang; yang disembunyikan hanya lapisan interaktif di atasnya. Status tampil dapat dipastikan dari log `spike-log.txt`.

### Ctrl+Alt+E — Mode edit

Mode edit digunakan untuk mengisi atau mengubah isi setiap benda di ruangan.

- **Untuk apa:** mengatur nama, jenis tampilan, dan daftar item (label beserta target) pada setiap benda.
- **Cara pakai:** tekan **Ctrl+Alt+E** untuk masuk ke mode edit. Setelah itu, klik salah satu benda untuk membuka form isian. Tekan **Ctrl+Alt+E** sekali lagi untuk keluar dari mode edit.
- **Yang terjadi:**
  - Di dalam mode edit, klik pada benda akan membuka **form konfigurasi**, bukan popup atau membuka target.
  - Di luar mode edit, klik pada benda akan menjalankan fungsinya (membuka popup atau membuka target langsung).
- **Isi form konfigurasi:**
  - **@NAMA:** nama benda yang akan ditampilkan pada popup.
  - **@JENIS:** cara benda merespons klik. Pilihannya **LANGSUNG**, **POP UP 3X3**, atau **POP UP LIST**.
  - **@URL:** target atau lokasi yang dibuka (URL, file, folder, atau aplikasi).
  - **|TAMBAH|:** tombol untuk menambah item ke dalam daftar benda. Tombol ini hanya muncul ketika **@JENIS** bernilai **POP UP 3X3** atau **POP UP LIST**, dan disembunyikan ketika **@JENIS** bernilai **LANGSUNG**.
  - **SIMPAN:** menyimpan perubahan ke JSON, lalu menutup form.
  - **BATAL:** menutup form tanpa menyimpan.
- **Efek tiap pilihan JENIS:**
  - **LANGSUNG:** klik pada benda akan langsung membuka target tanpa menampilkan popup. Cocok untuk benda yang hanya memiliki satu tujuan, misalnya membuka satu folder.
  - **POP UP 3X3:** klik pada benda akan menampilkan panel berbentuk kotak 3x3 berisi beberapa item. Setiap item memiliki target sendiri dan dibuka saat diklik.
  - **POP UP LIST:** klik pada benda akan menampilkan daftar item. Setiap item memiliki target sendiri dan dibuka saat diklik.
- **Contoh alur lengkap:**
  1. Tekan **Ctrl+Alt+E** untuk masuk mode edit.
  2. Klik benda **Rak Buku**.
  3. Isi **@NAMA** dengan `Rak Buku`.
  4. Pilih **@JENIS** = **POP UP LIST**.
  5. Isi **@URL**, lalu tekan **|TAMBAH|** untuk menambah item. Ulangi untuk item berikutnya.
  6. Tekan **SIMPAN**. Data tersimpan ke `Prototipe2\data\medieval-desk.json`.
  7. Tekan **Ctrl+Alt+E** untuk keluar mode edit.
  8. Klik **Rak Buku** pada mode normal. Popup berisi daftar item akan muncul.

### Ctrl+E — Alternatif mode edit

Pintasan ini melakukan hal yang sama seperti **Ctrl+Alt+E**, tetapi hanya aktif saat jendela spike sedang terfokus.

- **Untuk apa:** memberi cara cepat masuk atau keluar mode edit ketika sedang berinteraksi langsung dengan jendela spike, misalnya saat baru selesai mengisi form.
- **Catatan:** **Ctrl+Alt+E** berlaku global, sedangkan **Ctrl+E** dibatasi pada jendela yang sedang fokus agar tidak mengganggu aplikasi lain.

### Ctrl+Alt+Q — Keluar

Pintasan ini menutup aplikasi sekaligus mengembalikan wallpaper asli.

- **Untuk apa:** keluar dari spike dengan rapi.
- **Cara pakai:** tekan **Ctrl+Alt+Q**.
- **Yang terjadi:** lapisan ruangan ditutup, lalu wallpaper asli dari sebelum spike dijalankan dikembalikan. Nilai wallpaper asli disimpan pada `%AppData%\MedievalDesk\original-wallpaper.txt`.
- **Peringatan:** jangan menutup aplikasi lewat Task Manager atau `taskkill`. Cara tersebut dapat meninggalkan wallpaper ruangan tetap terpasang. Gunakan **Ctrl+Alt+Q**, atau kembalikan wallpaper secara manual bila hal itu sudah terjadi.

### Esc — Menutup berlapis

Tombol **Esc** menutup lapisan paling atas terlebih dahulu, secara berurutan:

1. Jika form konfigurasi sedang terbuka, **Esc** menutup form tersebut.
2. Jika popup sedang terbuka, **Esc** menutup popup tersebut.
3. Jika tidak ada form atau popup, **Esc** menyembunyikan lapisan ruangan.

Tekan **Esc** berulang kali untuk mundur satu lapis setiap kali penekanan.

---

## Alur Pakai Pertama Kali

1. Jalankan spike WPF (lihat bagian **Cara Menjalankan**). Lapisan ruangan akan tampil dan wallpaper ruangan dipasang.
2. Tekan **Ctrl+Alt+E** untuk masuk ke mode edit.
3. Klik salah satu benda, misalnya **Rak Buku**.
4. Isi **@NAMA**, pilih **@JENIS**, lalu isi **@URL**. Tekan **|TAMBAH|** untuk menambah item bila jenis yang dipilih adalah popup.
5. Tekan **SIMPAN**. Ulangi langkah 3-5 untuk benda lain yang ingin diisi.
6. Tekan **Ctrl+Alt+E** untuk keluar dari mode edit.
7. Klik benda pada mode normal untuk mencoba membukanya.
8. Tekan **Ctrl+Alt+D** untuk menyembunyikan atau menampilkan lapisan sesuai kebutuhan.
9. Tekan **Ctrl+Alt+Q** untuk keluar dan kembalikan wallpaper asli.

---

## Contoh Data Hotspot (JSON)

Data setiap benda disimpan pada `Prototipe2\data\medieval-desk.json`. Berikut potongan yang menunjukkan bentuk satu benda yang dapat dikonfigurasi.

```json
{
  "id": "rak_buku",
  "label": "Rak Buku",
  "kind": "configurable",
  "displayMode": "popup-list",
  "box": { "x": 100, "y": 200, "w": 180, "h": 220 },
  "groups": [
    {
      "name": "Rak Buku",
      "items": [
        { "label": "Dokumen", "target": "C:\\Users\\ASUS\\Documents" },
        { "label": "Catatan", "target": "https://example.com" }
      ]
    }
  ]
}
```

Keterangan singkat:

- **displayMode:** sesuai pilihan **@JENIS** pada form. `direct` untuk **LANGSUNG**, `popup-grid` untuk **POP UP 3X3**, dan `popup-list` untuk **POP UP LIST**.
- **box:** posisi dan ukuran benda pada gambar referensi.
- **groups[].items[]:** daftar item beserta target yang dibuka saat item diklik.

---

## Yang Perlu Diketahui

- Lapisan ruangan memakai jendela transparan yang selalu berada di atas (topmost). Pendekatan menempel ke lapisan desktop (WorkerW) tidak dipakai karena tidak stabil dan dapat rusak setelah pembaruan Windows.
- Karena bersifat topmost, lapisan dapat menutupi ikon desktop. Gunakan **Ctrl+Alt+D** untuk menyembunyikan sementara.
- Selalu keluar dengan **Ctrl+Alt+Q** agar wallpaper asli dikembalikan.
- Belum diuji di beberapa monitor dan skala DPI yang berbeda.

## Referensi

- [Rainmeter](https://www.rainmeter.net/) untuk widget di lapisan desktop.
- Wallpaper Engine (berbayar) untuk wallpaper interaktif.
- [BentoDesk](https://github.com/ZRainbow1275/bentodesk) dan [DeskRealm](https://github.com/ekimaku/DeskRealm) sebagai inspirasi konsep. Tidak ada kode yang disalin.

## Pengembangan

Proyek ini dikembangkan sendiri dengan bantuan agen AI melalui [OpenCode](https://opencode.ai), dengan pendekatan vibe coding. Konsep, tata letak, dan keputusan fitur ditentukan oleh pengembang.
