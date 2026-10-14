Medieval Desk

Medieval Desk adalah aplikasi desktop Windows berupa wallpaper interaktif bertema medieval RPG yang mengubah desktop menjadi ruang kerja bergaya perpustakaan dungeon. Berbagai objek di dalam ruangan, seperti rak buku, loker, peti, perisai, burung gagak, dan bola kristal, dapat berfungsi sebagai hotspot yang dikustomisasi untuk membuka aplikasi, file, folder, shortcut, atau situs web.

Aplikasi ini menggabungkan estetika fantasy abad pertengahan dengan fungsi desktop launcher yang praktis. Tema warna dan tipografinya juga diselaraskan dengan website portfolio pribadi "JG1van" (https://jg1van.github.io).

<img width="1920" height="1080" alt="Medieval Desk Wallpaper" src="src/assets/wallpaper.svg" />
Fitur Utama

1. Hotspot Universal

Hampir setiap objek di dalam ruangan dapat digunakan sebagai hotspot interaktif.

- Mengatur nama dan fungsi objek sesuai kebutuhan.
- Membuka file, folder, shortcut Windows (".lnk"), atau URL.
- Menggunakan area klik transparan berbasis persentase agar posisi hotspot lebih fleksibel terhadap perubahan resolusi.
- Menampilkan efek glow dan label nama ketika kursor diarahkan ke objek.
- Mengubah konfigurasi hotspot melalui Edit Mode.

2. Edit Mode

Sesuaikan posisi, ukuran, dan fungsi hotspot langsung melalui antarmuka aplikasi.

- Memindahkan hotspot menggunakan mouse.
- Mengubah ukuran area klik.
- Membuka pengaturan melalui ikon gear pada hotspot.
- Mengubah nama dan tujuan hotspot.
- Menyimpan perubahan konfigurasi secara otomatis di penyimpanan lokal.

Edit Mode dapat diakses melalui tombol pensil di pojok kanan atas aplikasi.

3. Peluncur Aplikasi

Hotspot berfungsi sebagai launcher visual untuk mengakses berbagai sumber daya di komputer.

Target yang didukung meliputi:

- File lokal.
- Folder.
- Shortcut Windows (".lnk").
- Situs web atau URL.

Setelah target dibuka, Medieval Desk akan diminimalkan secara otomatis sesuai mekanisme aplikasi agar jendela tujuan dapat tampil di depan.

4. Kontrol Jendela

Medieval Desk menyediakan kontrol jendela dengan tampilan yang disesuaikan dengan tema medieval.

- Minimize: meminimalkan jendela aplikasi.
- Maximize/Restore: memperbesar jendela atau mengembalikannya ke ukuran sebelumnya.
- Close: menutup aplikasi.
- Global hotkey: menggunakan "Ctrl + Alt + D" untuk menampilkan atau menyembunyikan aplikasi.

Dalam mode maximized, aplikasi mengisi area layar dengan taskbar Windows tetap terlihat. Wallpaper menggunakan mode cover agar memenuhi area jendela tanpa bar hitam pada berbagai ukuran tampilan.

Zona Interaktif

Setiap objek yang dapat diklik mewakili satu zona dengan fungsi tertentu. Sebagian besar zona dapat dikustomisasi melalui Edit Mode, sedangkan Kalender, Note, dan Bingkai memiliki fungsi tetap.

Objek| Fungsi
Rak Buku| Hotspot yang dapat dikustomisasi.
Loker| Hotspot yang dapat dikustomisasi.
Peti| Hotspot yang dapat dikustomisasi.
Gulungan| Hotspot yang dapat dikustomisasi.
Burung Gagak| Hotspot yang dapat dikustomisasi.
Bola Kristal| Hotspot yang dapat dikustomisasi.
Perisai| Hotspot yang dapat dikustomisasi.
Kalender| Zona tetap untuk fungsi kalender.
Note| Zona tetap untuk fungsi catatan.
Bingkai| Zona tetap untuk menampilkan foto.

Teknologi yang Digunakan

Teknologi| Kegunaan
"Tauri v2" (https://v2.tauri.app/)| Framework aplikasi desktop.
Rust| Backend dan integrasi dengan sistem operasi Windows.
HTML| Struktur antarmuka.
CSS| Tampilan, tata letak, dan efek visual.
JavaScript (Vanilla)| Logika dan interaksi frontend.
JSON| Penyimpanan konfigurasi lokal.
SVG / PNG| Aset visual dan wallpaper.
Grenze Gotisch| Tipografi bergaya gothic.
Cardo| Tipografi pendukung.
"OpenCode" (https://opencode.ai)| Agen AI yang membantu proses pengembangan melalui pendekatan vibe coding.


Sebelum menjalankan proyek dari source code, pastikan perangkat menggunakan Windows 10 atau Windows 11 dan telah memiliki:

- "Node.js" (https://nodejs.org/).
- "Rust" (https://rustup.rs/).
- Microsoft C++ Build Tools.
- WebView2 Runtime.

Untuk persyaratan tambahan dan konfigurasi lingkungan pengembangan, lihat "dokumentasi resmi Tauri v2" (https://v2.tauri.app/start/prerequisites/).

Instalasi dan Menjalankan Aplikasi

1. Clone repository

git clone https://github.com/JG1van/medieval-desk.git

2. Masuk ke direktori proyek

cd medieval-desk

3. Instal dependency

npm install

4. Jalankan mode pengembangan

npm run tauri dev

Perintah ini menjalankan aplikasi dalam mode pengembangan sesuai script yang dikonfigurasi dalam "package.json".

Build Aplikasi

Untuk membangun aplikasi desktop versi produksi, jalankan:

npm run tauri build

Jika script tersebut telah dikonfigurasi dengan benar, Tauri akan membangun aplikasi sesuai pengaturan proyek.

Hasil build umumnya tersedia di:

src-tauri/target/release/

Jika bundling diaktifkan, installer atau paket distribusi dapat ditemukan di:

src-tauri/target/release/bundle/

Lokasi dan format hasil akhir mengikuti konfigurasi Tauri dan target distribusi yang digunakan.

Cara Penggunaan

1. Jalankan aplikasi. Wallpaper ruangan medieval akan muncul sebagai antarmuka desktop interaktif.
2. Gunakan hotspot. Arahkan kursor ke objek untuk melihat efek visual dan label, lalu klik untuk menjalankan fungsi yang telah dikonfigurasi.
3. Masuk ke Edit Mode. Klik tombol pensil di pojok kanan atas.
4. Atur hotspot. Geser atau ubah ukuran area klik, kemudian klik ikon gear untuk mengubah nama dan tujuan hotspot.
5. Selesaikan pengeditan. Klik kembali tombol pensil untuk keluar dari Edit Mode.
6. Tampilkan atau sembunyikan aplikasi. Tekan "Ctrl + Alt + D".
7. Kelola jendela. Gunakan tombol minimize, maximize/restore, atau close yang tersedia di pojok kanan atas.

Perubahan konfigurasi hotspot disimpan secara lokal sesuai mekanisme penyimpanan yang digunakan aplikasi.

Struktur Proyek

Gambaran umum struktur direktori:

medieval-desk/
├── assets/         # Wallpaper, font, dan aset visual
├── docs/           # Dokumentasi dan screenshot
├── src/            # Antarmuka HTML, CSS, dan JavaScript
├── src-tauri/      # Backend Rust dan konfigurasi Tauri
├── package.json    # Script dan dependency proyek
├── package-lock.json
├── .gitignore
└── README.md

Struktur aktual dapat mencakup file dan direktori tambahan sesuai implementasi proyek.

Keterbatasan

- Jendela aplikasi: Medieval Desk berjalan sebagai jendela desktop biasa, sehingga dapat tertutupi oleh jendela lain. Gunakan "Ctrl + Alt + D" atau taskbar untuk kembali ke aplikasi.
- Multi-monitor: perilaku posisi hotspot pada konfigurasi beberapa monitor masih perlu diuji lebih lanjut.
- Skala DPI: perbedaan resolusi dan skala tampilan Windows dapat memengaruhi akurasi posisi hotspot.
- Validasi target: path file, folder, shortcut, dan URL perlu ditangani dengan baik apabila target tidak tersedia atau gagal dibuka.

Rencana Pengembangan

Beberapa pengembangan yang dapat dipertimbangkan untuk meningkatkan fungsionalitas aplikasi:

- Backup, ekspor, dan impor konfigurasi hotspot.
- Preset layout desktop dan tema visual.
- Dukungan multi-monitor yang lebih baik.
- Penyesuaian posisi hotspot berdasarkan resolusi dan DPI.
- Validasi target serta penanganan error yang lebih informatif.
- Pengaturan efek visual dan shortcut keyboard.
- Dokumentasi atau tutorial penggunaan di dalam aplikasi.

Daftar tersebut merupakan ide pengembangan, bukan pernyataan bahwa fitur-fitur tersebut sudah tersedia.

Status Proyek

Proyek pribadi — masih dalam pengembangan.

Medieval Desk dikembangkan secara bertahap dengan fokus pada pengalaman desktop yang personal, estetis, dan fungsional melalui perpaduan wallpaper fantasy, zona interaktif, serta launcher yang dapat dikustomisasi.

Inspirasi dan Proses Pengembangan

Konsep zona desktop interaktif dipelajari dari proyek berikut:

- "BentoDesk" (https://github.com/ZRainbow1275/bentodesk)
- "DeskRealm" (https://github.com/ekimaku/DeskRealm)

Kedua proyek tersebut menjadi referensi konseptual. Tidak ada kode dari proyek tersebut yang disalin.

Medieval Desk dirancang dan diarahkan secara mandiri, dengan proses penulisan kode yang dibantu agen AI melalui "OpenCode" (https://opencode.ai) menggunakan pendekatan vibe coding. Konsep, tata letak, tema visual, dan keputusan fitur ditentukan selama proses pengembangan, sementara agen AI membantu implementasi kode.

---

Dikembangkan sebagai proyek personal untuk menghadirkan pengalaman desktop Windows yang lebih unik melalui perpaduan estetika medieval RPG dan fungsionalitas sehari-hari.
