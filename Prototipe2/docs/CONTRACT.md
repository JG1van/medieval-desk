## Kontrak Platform

Semua akses ke browser atau sistem harus lewat `platform.js`. Berikut dokumentasi setiap fungsi bersama input, output, dan perilakunya.

### loadData()
**Input:** -
**Output:** Promise<object>
**Perilaku:**
- Baca dari `localStorage` dengan kunci `medieval-desk:data`.
- Jika kosong, ambil `data/medieval-desk.json` lewat fetch.
- Jika keduanya gagal, pakai data kosong:
```
{
  "version": 1,
  "reference": { "width": 1376, "height": 768 },
  "settings": { "minimizeAfterLaunch": true, "hotkey": "Ctrl+Alt+D", "location": { "name": "", "lat": null, "lon": null } },
  "hotspots": [],
  "widgets": []
}
```
- Tampilkan toast jika gagal.

### saveData(data)
**Input:** object
**Output:** Promise<void>
**Perilaku:**
- Validasi skema v1 (lihat `data.js`).
- Jika validasi gagal, jangan simpan dan tampilkan pesan yang jelas.
- Jika berhasil, simpan ke `localStorage`.

### exportData()
**Input:** -
**Output:** void
**Perilaku:**
- Ekspor data yang tersimpan di `localStorage` (`medieval-desk:data`) sebagai file JSON.
- Jika tidak ada data, tampilkan toast "Tidak ada data untuk diekspor."

### importData(file)
**Input:** File object
**Output:** Promise<object>
**Perilaku:**
- Baca file JSON.
- Validasi skema v1.
- Jika valid, simpan via `saveData` dan kembalikan data terimport.
- Jika gagal, tampilkan toast dan tolak promise.

### openItem(item)
**Input:** object dengan properti: id, label, type, target
**Output:** void
**Perilaku:**
- Jika `item.type === "url"`: `window.open(item.target, "_blank", "noopener")`
- Jika tipe lain (`app`, `file`, `folder`, `shortcut`): tampilkan kotak info yang berisi label, target, dan tombol Salin. Tulis pesan: "Prototipe web tidak bisa membuka file lokal. Versi asli akan membukanya."

### windowMinimize(), windowMaximize(), windowClose()
**Input:** -
**Output:** void
**Perilaku:**
- Tampilkan toast "Hanya tersedia di versi asli".

### getSystemStats()
**Input:** -
**Output:** null
**Perilaku:**
- Tampilkan "Tidak tersedia di prototipe".

### getWeather(lat, lon)
**Input:** number latitude, number longitude
**Output:** Promise<object | null>
**Perilaku:**
- Ambil dari `https://api.open-meteo.com/v1/forecast?latitude=LAT&longitude=LON&current=temperature_2m,weather_code`.
- Cache 15 menit di `localStorage`.
- Jika gagal, kembalikan null.

### now()
**Input:** -
**Output:** Date
**Perilaku:**
- Kembalikan `new Date()`.

### Toast
**Input:** string message
**Output:** void
**Perilaku:**
- Tampilkan toast dengan pesan yang diberikan.

### Popup
**Input:**  - `openGroupPopup(hotspotId)` - buka pop up grup untuk hotspot ID
  - `closeGroupPopup()` - tutup pop up grup
  - `openFixedPopup(label)` - tampilkan pop up tetap bertuliskan label
  - `openItemInfo(item)` - tampilkan detail item
  - `closeItemInfo()` - tutup detail item
**Perilaku:**
- Semua pop up memiliki animasi dan overlay yang tepat.
- Tombol Salin di detail item menyalin target.
- Tidak ada state yang bocor antar sesi.

### Koordinat hot spot (persen dari wallpaper)
- rak_buku: x 2.9, y 19.5, w 21.0, h 67.5
- kalender: x 26.9, y 36.1, w 5.3, h 11.9
- gulungan: x 26.4, y 52.0, w 6.6, h 7.4
- bingkai: x 40.5, y 23.9, w 19.2, h 44.0
- note: x 67.4, y 36.9, w 6.0, h 10.6
- bola_kristal: x 68.8, y 47.9, w 4.4, h 9.5
- loker: x 78.0, y 29.3, w 19.8, h 56.0
- perisai: x 45.2, y 4.4, w 9.9, h 16.1
- burung_gagak: x 32.1, y 52.4, w 7.4, h 32.4
- peti: x 65.4, y 66.9, w 10.1, h 16.7

Kontrak ini menjadi acuan untuk versi asli.