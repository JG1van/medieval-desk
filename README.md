# Medieval Desk

Aplikasi desktop Windows berupa **wallpaper interaktif bertema medieval RPG**. Latarnya adalah ruangan dungeon library, dan setiap benda di dalamnya (rak buku, loker, peti, perisai, gagak, bola kristal, dan lainnya) berfungsi sebagai **hotspot** yang bisa diatur sendiri untuk membuka aplikasi, folder, atau situs web.

Tema warna dan tipografi diselaraskan dengan website portfolio pribadi ([jg1van.github.io](https://jg1van.github.io)).

## Fitur Utama

**Hotspot Universal**
- Hampir semua benda di ruangan dapat diatur fungsinya sesuai keinginan
- Nama dan tujuan hotspot (file, folder, shortcut `.lnk`, atau URL) dapat diubah kapan saja lewat mode edit
- Area klik transparan berbasis persentase, sehingga tetap akurat di berbagai resolusi layar
- Efek glow dan label nama saat kursor diarahkan ke benda

**Mode Edit**
- Geser dan ubah ukuran hotspot langsung dengan mouse
- Ikon gear pada tiap hotspot untuk mengatur nama dan tujuan
- Posisi dan isi hotspot tersimpan otomatis secara lokal

**Peluncur Aplikasi**
- Membuka file, folder, shortcut, atau URL dari hotspot
- Medieval Desk otomatis di-minimize setelah aplikasi tujuan dibuka, sehingga aplikasi yang dibuka tampil di depan

**Kontrol Window**
- Tombol minimize, maximize/restore, dan close bergaya tema
- Mode maximized: mengisi layar dengan taskbar Windows tetap terlihat
- Gambar latar tampil penuh (mode cover) tanpa bar hitam pada berbagai ukuran window
- Hotkey global `Ctrl+Alt+D` untuk menampilkan atau menyembunyikan aplikasi

## Zona

Setiap benda di ruangan adalah satu zona. Fungsi dan isi zona dapat diubah kapan saja saat **mode edit**, kecuali tiga zona tetap (Kalender, Note, dan Bingkai).

| Benda | Fungsi |
|---|---|
| Rak Buku | Dapat diubah di mode edit |
| Loker | Dapat diubah di mode edit |
| Peti | Dapat diubah di mode edit |
| Gulungan | Dapat diubah di mode edit |
| Burung Gagak | Dapat diubah di mode edit |
| Bola Kristal | Dapat diubah di mode edit |
| Perisai | Dapat diubah di mode edit |
| Kalender | Tetap: kalender |
| Note | Tetap: catatan |
| Bingkai | Tetap: foto |

## Tech Stack

- **Framework desktop:** Tauri v2 (Rust)
- **Frontend:** HTML, CSS, JavaScript (vanilla)
- **Penyimpanan:** JSON lokal
- **Aset:** ilustrasi ruangan medieval RPG (dibuat dengan AI image generation)
- **Tipografi:** Grenze Gotisch dan Cardo
- **Pengembangan:** vibe coding dengan OpenCode

## Screenshot



![Medieval Desk](docs/screenshot.png)



## Instalasi

**Prasyarat:** Windows 10/11, [Node.js](https://nodejs.org), [Rust](https://rustup.rs), Microsoft C++ Build Tools, dan WebView2 Runtime.

1. Clone repository:

```
git clone https://github.com/JG1van/medieval-desk.git
```

2. Masuk ke direktori project:

```
cd medieval-desk
```

3. Install dependency:

```
npm install
```

4. Jalankan mode pengembangan:

```
npm run tauri dev
```

5. Build menjadi aplikasi `.exe`:

```
npm run tauri build
```

Hasil build berada di `src-tauri/target/release/medieval-desk.exe`. Installer (jika dibuat) ada di `src-tauri/target/release/bundle/`.

## Cara Penggunaan

1. Arahkan kursor ke benda di ruangan, lalu klik untuk membuka tujuan yang sudah diatur.
2. Klik tombol pensil di pojok kanan atas untuk masuk ke mode edit.
3. Di mode edit, geser atau ubah ukuran hotspot, lalu klik ikon gear untuk mengubah nama dan tujuannya (file, folder, shortcut, atau URL).
4. Keluar dari mode edit dengan menekan tombol pensil lagi.
5. Tekan `Ctrl+Alt+D` untuk menampilkan atau menyembunyikan Medieval Desk.
6. Gunakan tombol di pojok kanan atas untuk minimize, maximize/restore, atau menutup aplikasi.

## Struktur Proyek

```
medieval-desk/
├── assets/        # wallpaper dan font
├── docs/          # screenshot
├── src/           # antarmuka (HTML, CSS, JavaScript)
└── src-tauri/     # kode Rust dan konfigurasi Tauri
```

## Keterbatasan

- Aplikasi berjalan sebagai jendela biasa, sehingga jendela lain dapat menutupinya. Gunakan `Ctrl+Alt+D` atau taskbar untuk kembali ke Medieval Desk.
- Perilaku pada setup multi-monitor dan skala DPI yang berbeda masih perlu diuji.

## Status Project

Project pribadi yang masih dalam pengembangan.

## Inspirasi

Konsep zona desktop dipelajari dari proyek [BentoDesk](https://github.com/ZRainbow1275/bentodesk) dan [DeskRealm](https://github.com/ekimaku/DeskRealm). Tidak ada kode dari proyek tersebut yang disalin.

Medieval Desk dirancang dan diarahkan sendiri, dengan pengembangan kode memakai pendekatan *vibe coding* bersama [OpenCode](https://opencode.ai). Konsep, tata letak, tema visual, dan keputusan fitur berasal dari saya, sedangkan penulisan kodenya dibantu agen AI.
