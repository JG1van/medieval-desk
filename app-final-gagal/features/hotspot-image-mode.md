# Mode Hotspot dan Pemetaan Gambar

## Jenis hotspot

### `configurable`

Hotspot yang diedit pengguna.

- Diklik saat mode normal: membuka popup daftar target.
- Cocok untuk rak, catatan, atau loker dengan beberapa tujuan.

### `fixed`

Hotspot dengan perilaku khusus.

- Jika punya gambar dan `fixedType` `photo`/`note`/kosong: klik membuka lightbox.
- Nilai `fixedType` lain: mengikuti target/popup seperti biasa.
- Biasanya dipakai untuk bingkai foto atau catatan tempel.

## Mode tampilan

| Nilai | Perilaku |
|---|---|
| `direct` | Jika `kind` bukan `configurable` dan ada target, target pertama langsung dibuka. |
| `popup-list` | Popup daftar vertikal. |
| `popup-grid` | Saat ini diperlakukan sama seperti `popup-list`; nilai disimpan untuk kompatibilitas data. |

Untuk `configurable`, popup selalu dipakai agar semua target terlihat.

## Pemetaan aset

Aset diam dan hover dicari berdasarkan **ID hotspot**, huruf kecil:

| ID | Basis aset |
|---|---|
| `rak_buku` | `bookshelf` |
| `kalender` | `calendar` |
| `gula`, `gulungan` | `scroll` |
| `bingkai` | `frame` |
| `note`, `catatan` | `note` |
| `bola_kristal`, `kristal` | `crystal` |
| `lemari`, `loker` | `locker` |
| `perisai` | `perisai` |
| `gagak`, `burung_gagak` | `gagak` |
| `peti` | `peti` |

Fallback berdasarkan `fixedType`:

| `fixedType` | Basis aset |
|---|---|
| `calendar` | `calendar` |
| `photo` | `frame` |
| `note` | `note` |

File yang dicari:

```text
Assets\diam\<basis>_diam.png
Assets\hover\<basis>_hover.png
```

ID baru (`gambar_1`, dan seterusnya) tidak punya aset kecuali `fixedType` cocok. Hotspot tetap berfungsi, hanya tampil sebagai kotak transparan dengan bingkai saat mode edit.

## Menambah aset baru

1. Tambahkan PNG ke `medieval-desk-assets\diam` dan `hover`.
2. Daftarkan sebagai `Content` di `MedievalDesk.csproj` dengan `CopyToOutputDirectory`, `CopyToPublishDirectory`, dan `ExcludeFromSingleFile=true`.
3. Tambahkan cabang di `AssetBase`.
4. Publish ulang dan pastikan jumlah file `Assets` bertambah.

## Batasan

- Nama file harus tepat; tidak ada pencarian fuzzy.
- Aset mengikuti ID, bukan label.
- Tidak ada editor aset di dalam aplikasi.
