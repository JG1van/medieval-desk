# FINAL-PLAN.md — Instruksi Implementasi OpenCode CLI

> **Dokumen historis.** Rencana ini sudah dieksekusi (lihat `IMPLEMENTATION-LOG.md` dan `RELEASE_NOTES.md`).
> Path di dalam dokumen memakai penamaan lama. Pemetaan ke struktur sekarang:
>
> | Path lama | Path sekarang |
> |---|---|
> | `Final/` | `app-final-gagal/` |
> | `Native-C#/` | `app-desktop/` |
> | `medieval-desk-native/` | `prototipe-web/` |
> | `Prototipe/`, `Prototipe2/` | dihapus |

Tujuan dokumen ini adalah memberi agen coding (OpenCode) rencana kerja yang bisa dieksekusi, kriteria verifikasi, dan batasan rilis yang jelas.

- Repository: JG1van/medieval-desk
- Target paket: folder Final/
- Aset utama: Final/medieval-desk-assets/
- Jangan menganggap rencana ini sebagai bukti bahwa kode, build, fitur, atau artefak sudah selesai.

## 0. Prompt untuk OpenCode

Jalankan OpenCode dari root repository, lalu berikan prompt berikut:

```text
Baca dan ikuti Final/FINAL-PLAN.md sampai selesai. Baca semua AGENTS.md yang berlaku serta README, kontrak data, dan spesifikasi arsitektur yang relevan. Periksa git status dan diff sebelum mengedit. Audit struktur proyek dan perintah build yang benar, lalu kerjakan fase dalam rencana secara berurutan. Jangan menimpa pekerjaan pengguna, menghapus prototipe lama, atau melakukan operasi git destruktif. Jangan berhenti untuk keputusan teknis kecil; pilih opsi paling sederhana yang kompatibel dan catat keputusan. Jalankan build dan tes yang tersedia, perbaiki regresi, lalu perbarui dokumentasi dan checklist berdasarkan bukti nyata. Jika pengujian memerlukan Windows/GUI yang tidak tersedia, tandai sebagai blocked/manual-test-required dan jangan mengklaim lulus. Jangan membuat placeholder yang diklaim sebagai fitur jadi, checksum palsu, atau executable palsu. Jangan membuat commit atau GitHub Release tanpa permintaan terpisah. Akhiri dengan laporan perubahan, perintah dan hasil tes, blocker, serta langkah berikutnya.
```

### Aturan kerja

1. Mulai dengan git status --short, struktur folder, seluruh AGENTS.md yang berlaku, README, Native-C#/SPEC-NATIVE.md, Native-C#/README.md, Prototipe2/PLAN.md, kontrak data yang dirujuk, serta kode dan tes terkait.
2. Verifikasi nama file, framework, hotkeys, koordinat, target framework, dan perintah build dari repository; jangan mengarang.
3. Jangan menghapus atau merusak Prototipe/, Prototipe2/, Native-C#/, atau medieval-desk-native/.
4. Periksa perubahan lokal sebelum mengedit. Jangan menjalankan git reset --hard, git clean -fd, force-push, atau operasi destruktif serupa.
5. Catat baseline, keputusan, hasil tes, dan blocker di Final/IMPLEMENTATION-LOG.md.
6. Jangan meminta konfirmasi untuk keputusan teknis rutin. Jika terhalang izin, perangkat, kredensial, atau keputusan produk yang tidak dapat disimpulkan, dokumentasikan blocker dan lanjutkan bagian lain yang tidak bergantung padanya.
7. Jangan mengklaim fitur, file, build, executable, checksum, atau release tersedia sebelum memeriksanya.
8. Jangan membuat commit atau GitHub Release kecuali diminta terpisah.

## 1. Tujuan dan arsitektur

Medieval Desk adalah desktop berbasis wallpaper dengan hotspot interaktif. Target yang direncanakan adalah C#/.NET + WPF/Win32, tetapi audit repository dan status Gerbang B wajib dilakukan sebelum memperluas aplikasi native.

- Periksa Native-C#/SPEC-NATIVE.md dan Native-C#/README.md. Spike dasar bukan bukti bahwa aplikasi final sudah dibuat.
- Periksa kontrak dan implementasi di Prototipe2/ serta proyek lain. Gunakan sebagai referensi; jangan otomatis mengganti target ke web/Tauri karena lebih mudah.
- Verifikasi Gerbang B (z-order/WorkerW, Win+D, click-through, DPI 100%/150%, multi-monitor, restore wallpaper). Jika tidak dapat diuji di lingkungan sekarang, tandai belum diverifikasi.
- Jika Gerbang B gagal, jangan mengklaim lulus. Implementasikan pekerjaan independen yang aman dan dokumentasikan tes Windows yang masih dibutuhkan.
- Jangan membuat skema data baru jika kontrak lama masih berlaku tanpa mencatat alasan kompatibilitas.

## 2. Aset visual — aturan wajib

Struktur aset yang sudah ada:

```text
Final/medieval-desk-assets/
  wallpaper_polos.png
  wallpaper.png
  diam/
    bookshelf_diam.png
    calendar_diam.png
    crystal_diam.png
    frame_diam.png
    gagak_diam.png
    locker_diam.png
    note_diam.png
    perisai_diam.png
    peti_diam.png
    scroll_diam.png
  hover/
    bookshelf_hover.png
    calendar_hover.png
    crystal_hover.png
    frame_hover.png
    gagak_hover.png
    locker_hover.png
    note_hover.png
    perisai_hover.png
    peti_hover.png
    scroll_hover.png
```

### Komposisi yang diminta pengguna

- Gunakan **wallpaper_polos.png sebagai latar utama**. Sepuluh ikon dari folder diam/ dan hover/ ditempatkan di atas latar ini sebagai hotspot.
- Pertahankan wallpaper.png sebagai aset alternatif/referensi. Jangan menjadikannya latar utama menggantikan wallpaper_polos.png tanpa alasan teknis yang dicatat.
- Normal: diam/<nama>_diam.png. Hover: hover/<nama>_hover.png. Ketika pointer keluar, kembali ke gambar normal.
- Pasangkan berdasarkan nama dasar: bookshelf, calendar, crystal, frame, gagak, locker, note, perisai, peti, scroll.
- Pertahankan aset sumber, nama, transparansi PNG, dan rasio aspek. Jangan menggambar ulang atau melakukan kompresi lossy. Jika perlu aset di folder publish, salin apa adanya.
- Gunakan path relatif stabil; jangan gunakan path absolut komputer pengembang. Pastikan aset tetap ditemukan saat aplikasi dijalankan dari folder lain atau hasil publish.
- Gunakan koordinat dari kontrak/config lama bila tersedia. Jangan mengarang koordinat final. Jika tidak ditemukan, buat konfigurasi awal yang mudah dikalibrasi dan dokumentasikan bahwa posisinya masih sementara.
- Hover hanya mengubah tampilan, bukan menjalankan aksi. Aksi hotspot dipicu oleh klik/input yang dirancang.
- Aset hilang harus menghasilkan log/pesan diagnostik, bukan crash diam-diam.
- Verifikasi secara visual posisi, ukuran, transparansi, z-order, dan DPI sebelum menyebut tampilan final.

## 3. Fase implementasi

### Fase 0 — Audit dan baseline

- [ ] Catat git status --short dan pertahankan perubahan lokal.
- [ ] Petakan proyek aktif, entry point, framework, dependensi, target framework, konfigurasi, kontrak JSON, hotkeys, tes, dan perintah build.
- [ ] Baca status Gerbang B dan kriteria DPI/multi-monitor.
- [ ] Verifikasi keberadaan seluruh 22 aset yang tercantum di bagian 2.
- [ ] Catat baseline dan keputusan di Final/IMPLEMENTATION-LOG.md.
- [ ] Jalankan build/test baseline yang tersedia dan catat hasil aktual.

Selesai bila: ada catatan baseline dengan perintah, hasil, dan blocker yang nyata.

### Fase 1 — Shell aplikasi dan tampilan desktop

- [ ] Gunakan arsitektur yang telah diverifikasi dan sesuai dengan status Gerbang B.
- [ ] Render wallpaper_polos.png sebagai latar utama.
- [ ] Render 10 hotspot dengan pasangan normal/hover yang tepat.
- [ ] Gunakan koordinat relatif/persen yang kompatibel dengan kontrak dan verifikasi konversi DPI/multi-monitor.
- [ ] Terapkan perilaku desktop (WorkerW, z-order, click-through) hanya sesuai desain yang sudah diverifikasi.
- [ ] Tambahkan logging, penanganan aset hilang, dan penutupan yang aman.
- [ ] Verifikasi mouse, fokus, Win+D, Esc, hotkeys, dan z-order sesuai kebutuhan proyek.
- [ ] Jika aplikasi mengubah wallpaper Windows, simpan state asli lebih dulu dan sediakan recovery.

### Fase 2 — Data, konfigurasi, dan hotkeys

- [ ] Gunakan kontrak data yang sudah ada, terutama Prototipe2/data/, Prototipe2/config/, dan dokumen kontraknya jika masih berlaku.
- [ ] Validasi JSON dan tampilkan error yang jelas.
- [ ] Simpan konfigurasi pengguna pada lokasi user data/AppData yang sesuai, bukan working directory.
- [ ] Verifikasi hotkeys dari implementasi/spesifikasi aktual; jangan mengarang kombinasi.
- [ ] Lepaskan hotkeys, timer, event handler, dan resource ketika aplikasi ditutup.
- [ ] Sediakan recovery untuk konfigurasi rusak dan kegagalan pemulihan wallpaper.

### Fase 3 — Lima fitur tambahan

Implementasikan fitur secara nyata di aplikasi, bukan hanya membuat dokumen spesifikasi. Buat satu dokumen per fitur di Final/features/.

#### 3A. Export / Import JSON

Tugas:
- UI export/import sesuai pola UI aplikasi.
- Ekspor konfigurasi aktif ke JSON valid dengan versi schema.
- Validasi ukuran, struktur, versi, dan field wajib sebelum import.
- Tampilkan preview perbedaan sebelum menerapkan. Cancel harus meninggalkan konfigurasi aktif tanpa perubahan.
- Merge hotspot berdasarkan id jika kompatibel dengan kontrak; jangan menggandakan ID.
- Backup konfigurasi sebelum perubahan dan peringatkan schema yang tidak kompatibel.

Acceptance:
- [ ] JSON hasil ekspor dapat dibaca ulang.
- [ ] Import valid dapat dipreview dan diterapkan.
- [ ] Cancel tidak mengubah konfigurasi aktif.
- [ ] Merge berdasarkan ID tidak membuat duplikat.
- [ ] JSON invalid/schema tak didukung ditolak dengan pesan jelas dan tanpa merusak data.

#### 3B. Profiles dan Themes

Tugas:
- Simpan preset layout JSON di lokasi profil yang sesuai; contoh boleh di repository, data pengguna di AppData.
- Implementasikan theme memakai mekanisme yang cocok dengan framework aktual (misalnya ResourceDictionary untuk WPF).
- UI memilih profil/theme dan menyimpan konfigurasi saat ini sebagai profil.
- Validasi nama/path/isi; hindari path traversal dan penimpaan diam-diam.

Acceptance:
- [ ] Profil dapat dipilih dan layout berubah.
- [ ] Theme benar-benar mengubah tampilan yang disepakati.
- [ ] Profil yang disimpan bisa dimuat ulang.
- [ ] Profil invalid tidak merusak konfigurasi aktif.

#### 3C. Photo Slideshow / Photo Frame

Tugas:
- Widget gambar per-hotspot jika sesuai dengan model data.
- Pemilihan file/folder melalui dialog native dan preview/thumbnail bila memungkinkan.
- Interval slideshow dapat dikonfigurasi dengan batas yang masuk akal.
- Timer sesuai framework UI (misalnya DispatcherTimer untuk WPF) berhenti ketika widget ditutup/dilepas.
- Tangani gambar besar/rusak, folder kosong, file hilang, dan penggunaan memori.
- Jangan mengubah aset sumber; jelaskan jika foto pengguna disalin ke AppData.

Acceptance:
- [ ] Foto dapat dipilih dan ditampilkan.
- [ ] Slideshow berganti sesuai interval.
- [ ] Timer berhenti ketika widget ditutup.
- [ ] File hilang/rusak ditangani dengan baik.
- [ ] Perubahan sumber terlihat setelah refresh/reload.

#### 3D. Drag-and-drop ke hotspot (Mode Edit)

Tugas:
- Mode Edit terpisah dari mode normal.
- Dukung drag/drop satu atau banyak file yang didukung.
- Tampilkan preview dan izinkan label diubah sebelum menyimpan.
- Dokumentasikan apakah file direferensikan lewat path atau disalin ke AppData; jangan menyalin diam-diam.
- Resolusi shortcut .lnk opsional. Jika dibuat, gunakan API Windows yang sesuai.
- Tangani file tidak didukung, path invalid, dan nama duplikat.

Acceptance:
- [ ] Satu dan banyak file dapat di-drop.
- [ ] Label dapat diedit sebelum simpan.
- [ ] Cancel tidak menyimpan perubahan.
- [ ] Item muncul setelah reload.
- [ ] Path/file hilang ditangani dengan pesan yang jelas.

#### 3E. Hotspot Image Mode

Tugas:
- Mode Edit punya overlay seleksi area pada latar.
- Simpan posisi/ukuran sebagai persentase terhadap area acuan.
- Dialog memilih beberapa gambar; salin ke lokasi data aplikasi bila memang diperlukan.
- Klik hotspot membuka lightbox; slideshow bila dibuat harus memiliki kontrol dan interval.
- Tangani orientasi gambar, file invalid, Escape, tombol tutup, dan navigasi.
- Jangan sampai lightbox membuat klik menembus ke desktop secara tidak sengaja.

Acceptance:
- [ ] Seleksi area dapat dibuat dan diedit.
- [ ] Koordinat tetap proporsional ketika ukuran layar berubah.
- [ ] Binding gambar tersimpan setelah restart.
- [ ] Lightbox dapat dibuka, dinavigasi, dan ditutup.
- [ ] Timer slideshow berhenti ketika lightbox ditutup.

## 4. Struktur paket yang ditargetkan

Gunakan struktur ini sebagai target; jangan membuat file kosong hanya untuk memenuhi daftar.

```text
Final/
  FINAL-PLAN.md
  README.md
  RELEASE_NOTES.md
  IMPLEMENTATION-LOG.md
  LICENSE                       # hanya jika lisensi sudah dipilih/diverifikasi
  installers/                   # output publish nyata, bukan placeholder
  features/
    export-import.md
    profiles-themes.md
    photo-slideshow.md
    drag-drop.md
    hotspot-image-mode.md
  samples/
    sample-layout.json
    images/                     # hanya contoh yang tersedia dan izinnya jelas
  tools/
    restore-wallpaper.ps1
    build-and-copy.ps1          # opsional
  docs/
    USER_GUIDE.md
    TROUBLESHOOTING.md
  demo/                         # hanya demo asli
  checksums.txt                 # dihitung dari artefak aktual
  medieval-desk-assets/
```

- Jangan membuat sample1.jpg/sample2.jpg bila tidak tersedia. Jangan mengarang lisensi atau membuat LICENSE dengan asumsi.
- Binary hasil build boleh menjadi artefak lokal/CI/release. Jangan menambahkan executable kosong atau mengklaim binary belum diuji sebagai rilis.
- Jika membuat native-spike/, pastikan isi tidak menyimpang dari sumber Native-C#/ dan hindari salinan yang mudah kedaluwarsa jika tautan relatif cukup.

## 5. Build dan packaging

Audit file project/solution, target framework, SDK, dan perintah yang benar sebelum build. Contoh untuk proyek WPF .NET yang benar-benar terverifikasi:

```powershell
dotnet --info
dotnet restore <project-atau-solution-yang-terverifikasi>
dotnet build <project-atau-solution-yang-terverifikasi> -c Release
dotnet test <solution-yang-terverifikasi> -c Release
dotnet publish <project-yang-terverifikasi> -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o <folder-output>
```

- Ganti placeholder path sebelum menjalankan; jangan menjalankan contoh literal.
- Jalankan dotnet test jika ada test project. Jika tidak ada, catat dan jalankan pemeriksaan relevan.
- WPF memerlukan target Windows. Jika OpenCode berjalan di non-Windows, lakukan tes yang kompatibel dan tandai tes GUI/DPI/multi-monitor sebagai belum diverifikasi.
- Pastikan aset PNG tetap terbaca setelah publish dan aplikasi berjalan dari folder bersih.
- Salin hanya output publish yang benar-benar dihasilkan. Sertakan file runtime yang dibutuhkan.
- Skrip build-and-copy.ps1 bila dibuat harus memeriksa exit code, keberadaan output, dan tidak menimpa output valid secara diam-diam.
- Bangun Tauri hanya jika audit membuktikan itu target yang memang masih dibutuhkan.

ZIP portable (jalankan dari root setelah memeriksa isi paket):

```powershell
Compress-Archive -Path .\Final\* -DestinationPath .\medieval-desk-final.zip
```

Verifikasi isi ZIP dan tes dari folder ekstraksi bersih. Jangan sertakan secrets, cache, temporary files, atau log pribadi.

## 6. Wallpaper dan recovery

Jika aplikasi mengubah wallpaper Windows:
- Simpan wallpaper asli dan informasi pemulihan sebelum mengubahnya.
- Simpan state di lokasi data aplikasi, bukan current working directory.
- Pulihkan pada penutupan normal dan sediakan recovery manual jika aplikasi gagal.
- Final/tools/restore-wallpaper.ps1 harus memvalidasi backup, memberi pesan jika backup tidak ada, dan tidak menghapus file pengguna.
- Jangan gunakan taskkill sebagai pengujian penutupan normal.
- Hindari meminta Administrator bila tidak diperlukan. Uji dengan aman dan catat jika recovery memerlukan tes manual.
- Tes restore harus membuktikan wallpaper asli kembali, bukan hanya proses berhenti.

Acceptance:
- [ ] State asli disimpan sebelum perubahan.
- [ ] Penutupan normal memulihkan wallpaper.
- [ ] Recovery manual menangani backup yang ada maupun tidak ada.
- [ ] Kegagalan pemulihan tercatat jelas.

## 7. Checklist pengujian

Centang hanya dengan bukti. Jika tidak dapat diuji, gunakan status blocked/manual-test-required.

### Build dan paket
- [ ] Restore/build Release berhasil.
- [ ] Tes otomatis yang tersedia lulus.
- [ ] Aplikasi berjalan dari folder publish/ekstraksi bersih.
- [ ] Aset dimuat tanpa path absolut.
- [ ] README mencantumkan cara menjalankan dan hotkeys yang benar-benar diterapkan.
- [ ] Tidak ada secret/path pribadi.

### Tampilan dan interaksi
- [ ] wallpaper_polos.png tampil sebagai latar utama.
- [ ] Semua 10 ikon normal tampil.
- [ ] Semua 10 pasangan hover bekerja dan kembali normal saat pointer keluar.
- [ ] Rasio aspek dan transparansi benar.
- [ ] Klik hotspot menjalankan aksi yang terdokumentasi.
- [ ] Win+D, z-order, click-through, Esc, dan hotkeys diuji sesuai target.
- [ ] DPI 100% dan 150% diuji di Windows.
- [ ] Multi-monitor diuji atau diberi status belum diverifikasi.

### Fitur dan recovery
- [ ] Export/import, preview, cancel, validasi, dan merge diuji.
- [ ] Profile/theme bisa disimpan dan dimuat.
- [ ] Slideshow berjalan dan timer berhenti ketika ditutup.
- [ ] Drag/drop satu dan banyak file bekerja.
- [ ] Hotspot Image Mode, koordinat relatif, dan lightbox bekerja.
- [ ] JSON/gambar invalid ditangani dengan aman.
- [ ] Wallpaper asli dipulihkan saat aplikasi keluar.
- [ ] Recovery script diuji atau diberi status manual-test-required.

### Rilis
- [ ] RELEASE_NOTES.md hanya menyebut fitur yang benar-benar tersedia.
- [ ] SHA-256 dihitung dari artefak aktual.
- [ ] ZIP dapat diekstrak dan dijalankan dari folder bersih.
- [ ] Tidak ada checklist dicentang tanpa bukti.

## 8. Dokumentasi wajib

- Final/README.md: status produk, prasyarat, cara menjalankan, hotkeys yang terverifikasi, konfigurasi, dan cara keluar.
- Final/docs/USER_GUIDE.md: hotspot, Mode Edit, profil/theme, slideshow, import/export.
- Final/docs/TROUBLESHOOTING.md: aset hilang, JSON invalid, startup, wallpaper restore, DPI/multi-monitor, log.
- Final/features/*.md: perilaku dan acceptance test untuk tiap fitur.
- Final/RELEASE_NOTES.md: perubahan aktual dan known issues, bukan rencana.
- Final/IMPLEMENTATION-LOG.md: baseline, keputusan, perintah, hasil tes, blocker.

## 9. CI opsional

Setelah build lokal jelas:
- Gunakan Windows-latest untuk restore/build/test .NET.
- Unggah publish artifact hanya jika job sukses.
- Pisahkan build Tauri hanya jika memang masih diperlukan.
- Release hanya dari tag yang dibuat eksplisit, bukan setiap push ke main.
- Hitung SHA-256 dari artefak yang benar-benar diunggah.
- Jangan menyimpan secrets di repository/workflow.

## 10. Definisi selesai

Paket belum siap rilis hanya karena source code atau dokumentasi ada. Siap rilis berarti:
1. Implementasi target berhasil dibangun dengan perintah terdokumentasi.
2. Aplikasi berjalan dari paket bersih dan memuat semua aset dengan benar.
3. Lima fitur terintegrasi dan acceptance test yang dapat dijalankan lulus.
4. Tes Windows, DPI, multi-monitor, hotkey, dan recovery tercatat; yang belum diuji dinyatakan jelas.
5. Dokumentasi sesuai perilaku aplikasi aktual.
6. Artefak dan checksum benar-benar dibuat dan diverifikasi.
7. Tidak ada blocker kritis yang disembunyikan.

Jika tes penting hanya bisa dilakukan manual pada Windows, laporkan status **belum sepenuhnya terverifikasi**.

## 11. Format laporan akhir OpenCode

1. Ringkasan perubahan.
2. File yang dibuat/diubah.
3. Perintah build/test dan hasil aktual.
4. Status acceptance: selesai/gagal/blocked/manual-test-required.
5. Lokasi executable/ZIP/checksum hanya jika benar-benar dihasilkan.
6. Blocker dan hal yang belum dapat diverifikasi.
7. Langkah berikutnya.

Jangan menyebut aplikasi siap rilis jika kriteria bagian 10 belum terpenuhi.
