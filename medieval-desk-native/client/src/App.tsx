import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { invoke } from '@tauri-apps/api/core'
import { useDataStore } from './store/useDataStore'
import { useEditMode } from './hooks/useEditMode'
import { useHotspots } from './hooks/useHotspots'
import { AssignModal } from './components/AssignModal'
import wallpaperUrl from './assets/wallpaper.png'

export default function App() {
  const { data, load } = useDataStore()
  const { isEditMode, toggle: toggleEdit } = useEditMode()
  const {
    selectedHotspot, selectedGroup, selectedItem,
    isPopupOpen, isItemInfoOpen,
    openHotspot, openGroup, openItem, closePopups, getHotspotStyle,
    addGroup, renameGroup, deleteGroup,
    addItem, editItem, deleteItem, moveItem, openItemTarget,
    assignHotspot, closeAssignModal,
  } = useHotspots()

  const [groupName, setGroupName] = useState('')
  const [editGroupId, setEditGroupId] = useState<string | null>(null)
  const [editGroupName, setEditGroupName] = useState('')
  const [itemLabel, setItemLabel] = useState('')
  const [itemTarget, setItemTarget] = useState('')
  const [itemType, setItemType] = useState<'app' | 'file' | 'folder' | 'shortcut' | 'url'>('url')
  const [editItemId, setEditItemId] = useState<string | null>(null)
  const [toastMsg, setToastMsg] = useState<string | null>(null)

  useEffect(() => { load() }, [load])

  useEffect(() => {
    document.body.classList.toggle('edit-mode', isEditMode)
  }, [isEditMode])

  useEffect(() => {
    const onToast = (e: Event) => setToastMsg((e as CustomEvent<string>).detail)
    window.addEventListener('md-toast', onToast)
    return () => window.removeEventListener('md-toast', onToast)
  }, [])

  useEffect(() => {
    if (!toastMsg) return
    const t = setTimeout(() => setToastMsg(null), 1400)
    return () => clearTimeout(t)
  }, [toastMsg])

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if ((e.key === 'e' || e.key === 'E') && !(e.target instanceof HTMLInputElement)) toggleEdit()
      if (e.key === 'Escape') { closePopups(); closeAssignModal() }
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [toggleEdit, closePopups, closeAssignModal])

  const win = useCallback((cmd: string) => invoke(cmd).catch(() => {}), [])

  const launch = useCallback((target: string, msg?: string) => {
    if (!target) return
    invoke('launch_file', { path: target }).catch(() => {})
    if (msg) setToastMsg(msg)
  }, [])

  if (!data) return <div id="app"><p>Memuat Medieval Desk...</p></div>

  const groups = selectedHotspot?.groups ?? []
  const activeGroup = selectedGroup ?? groups[0] ?? null
  const mode = selectedHotspot?.displayMode || (selectedHotspot?.kind === 'configurable' ? 'popup-list' : 'direct')
  const isPhotoPopup = selectedHotspot?.kind === 'fixed' && selectedHotspot.fixedType === 'photo' && mode !== 'direct'

  const submitGroup = (e: FormEvent) => {
    e.preventDefault()
    if (!selectedHotspot || !groupName.trim()) return
    addGroup(selectedHotspot.id, groupName.trim())
    setGroupName('')
  }

  const submitItem = (e: FormEvent) => {
    e.preventDefault()
    if (!selectedHotspot || !activeGroup || !itemLabel.trim() || !itemTarget.trim()) return
    if (editItemId) {
      editItem(selectedHotspot.id, activeGroup.id, editItemId, { label: itemLabel.trim(), target: itemTarget.trim(), type: itemType })
      setEditItemId(null)
    } else {
      addItem(selectedHotspot.id, activeGroup.id, { label: itemLabel.trim(), target: itemTarget.trim(), type: itemType })
    }
    setItemLabel('')
    setItemTarget('')
  }

  const startEditItem = (id: string, label: string, target: string, type: typeof itemType) => {
    setEditItemId(id)
    setItemLabel(label)
    setItemTarget(target)
    setItemType(type)
  }

  return (
    <div id="app">
      <div id="wallpaper-container">
        <img id="wallpaper" src={wallpaperUrl} alt="" draggable={false} />
      </div>

      <div id="hotspots-layer">
        {data.hotspots.map((hs) => (
          <div
            key={hs.id}
            className="hotspot-area"
            style={getHotspotStyle(hs)}
            role="button"
            tabIndex={0}
            aria-label={hs.label}
            onClick={() => openHotspot(hs)}
            onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') openHotspot(hs) }}
          >
            <span className="hotspot-label">{hs.label}</span>
          </div>
        ))}
      </div>

      <div className="drag-region" data-tauri-drag-region />

      <div className="window-controls">
        <button className="win-btn" title="Minimalkan" onClick={() => win('window_minimize')}>_</button>
        <button className="win-btn" title="Maksimalkan" onClick={() => win('window_maximize')}>▢</button>
        <button className="win-btn win-close" title="Tutup" onClick={() => win('window_close')}>×</button>
      </div>

      <div id="hotkey-hint">E: edit · Esc: tutup</div>

      <div className={`popup-overlay${isPopupOpen ? ' active' : ''}`} hidden={!isPopupOpen}
        onMouseDown={(e) => { if (e.target === e.currentTarget) closePopups() }}>
        <div className={`popup popup-wide${mode === 'popup-list' ? ' popup-list' : ''}`} role="dialog" aria-modal="true" aria-labelledby="group-popup-title">
          <button className="close-btn" aria-label="Tutup" onClick={closePopups}>×</button>
          <h2 id="group-popup-title">{selectedHotspot?.label ?? ''}</h2>
          <div className="content">
            {groups.length > 1 && (
              <div className="group-tabs" role="tablist">
                {groups.map((g) => (
                  <button
                    key={g.id}
                    role="tab"
                    aria-selected={activeGroup?.id === g.id}
                    className={`group-tab${activeGroup?.id === g.id ? ' active' : ''}`}
                    onClick={() => openGroup(g)}
                  >{g.name}</button>
                ))}
              </div>
            )}

            {activeGroup && activeGroup.items.length === 0 && (
              <p className="empty-hint">Belum ada daftar. Tekan <b>E</b> lalu klik area ini untuk menambahkan.</p>
            )}

            {activeGroup && (
              <ul className="app-grid" id="app-grid">
                {activeGroup.items.map((it) => (
                  <li key={it.id} className="app-card">
                    <button
                      className="app-card-main"
                      onClick={() => (isEditMode ? openItem(it) : launch(it.target))}
                      onDoubleClick={() => openItemTarget(it.target)}
                      title={isEditMode ? `${it.label} — klik untuk info` : `${it.label} — klik untuk buka`}
                    >
                      <span className="app-card-name">{it.label}</span>
                      <span className={`kind-badge ${it.type === 'url' ? 'kind-web' : 'kind-file'}`}>{it.type}</span>
                    </button>
                    {isEditMode && (
                      <span className="app-card-tools">
                        <button className="btn-mini btn" title="Edit" onClick={() => startEditItem(it.id, it.label, it.target, it.type)}>✎</button>
                        <button className="btn-mini btn list-row-remove" title="Hapus"
                          onClick={() => selectedHotspot && deleteItem(selectedHotspot.id, activeGroup.id, it.id)}>×</button>
                        {groups.length > 1 && (
                          <select
                            className="btn-mini"
                            title="Pindah grup"
                            defaultValue=""
                            onChange={(e) => {
                              if (e.target.value && selectedHotspot)
                                moveItem(selectedHotspot.id, activeGroup.id, e.target.value, it.id)
                              e.target.value = ''
                            }}
                          >
                            <option value="" disabled>→</option>
                            {groups.filter((g) => g.id !== activeGroup.id).map((g) => (
                              <option key={g.id} value={g.id}>{g.name}</option>
                            ))}
                          </select>
                        )}
                      </span>
                    )}
                  </li>
                ))}
              </ul>
            )}

            {isEditMode && selectedHotspot?.kind === 'configurable' && (
              <>
                <form className="assign-form" onSubmit={submitItem}>
                  <label className="assign-field">
                    <span>{editItemId ? 'Edit item' : 'Item baru'} di “{activeGroup?.name ?? '—'}”</span>
                    <span className="list-row">
                      <input value={itemLabel} onChange={(e) => setItemLabel(e.target.value)}
                        placeholder="Nama" required className="list-row-name" />
                      <input value={itemTarget} onChange={(e) => setItemTarget(e.target.value)}
                        placeholder="Target (path / URL)" required className="list-row-url" />
                      <select value={itemType} onChange={(e) => setItemType(e.target.value as typeof itemType)}
                        className="list-row-kind" aria-label="Tipe">
                        <option value="url">url</option>
                        <option value="app">app</option>
                        <option value="file">file</option>
                        <option value="folder">folder</option>
                        <option value="shortcut">shortcut</option>
                      </select>
                      <button className="btn btn-mini list-row-browse" type="submit">
                        {editItemId ? 'Simpan' : 'Tambah'}
                      </button>
                      {editItemId && (
                        <button className="btn btn-mini list-row-remove" type="button"
                          onClick={() => { setEditItemId(null); setItemLabel(''); setItemTarget('') }}>Batal</button>
                      )}
                    </span>
                  </label>
                </form>

                <form className="assign-form" onSubmit={submitGroup}>
                  <label className="assign-field">
                    <span>Grup baru</span>
                    <span className="list-row">
                      <input value={groupName} onChange={(e) => setGroupName(e.target.value)}
                        placeholder="Nama grup" required />
                      <button className="btn btn-mini" type="submit">Buat grup</button>
                    </span>
                  </label>
                </form>

                {groups.length > 0 && (
                  <div className="group-manage">
                    {groups.map((g) => (
                      <span key={g.id} className="list-row">
                        {editGroupId === g.id ? (
                          <>
                            <input value={editGroupName} onChange={(e) => setEditGroupName(e.target.value)}
                              autoFocus aria-label="Nama grup" />
                            <button className="btn btn-mini" type="button"
                              onClick={() => {
                                if (selectedHotspot && editGroupName.trim())
                                  renameGroup(selectedHotspot.id, g.id, editGroupName.trim())
                                setEditGroupId(null)
                              }}>Simpan</button>
                            <button className="btn btn-mini" type="button" onClick={() => setEditGroupId(null)}>Batal</button>
                          </>
                        ) : (
                          <>
                            <span>{g.name} ({g.items.length})</span>
                            <button className="btn btn-mini" type="button"
                              onClick={() => { setEditGroupId(g.id); setEditGroupName(g.name) }}>Ganti nama</button>
                            <button className="btn btn-mini list-row-remove" type="button"
                              onClick={() => selectedHotspot && deleteGroup(selectedHotspot.id, g.id)}>Hapus</button>
                          </>
                        )}
                      </span>
                    ))}
                  </div>
                )}
              </>
            )}
          </div>
          <div className="actions">
            <button className="btn" onClick={closePopups}>Tutup</button>
          </div>
        </div>
      </div>

      <div className={`popup-overlay${isItemInfoOpen ? ' active' : ''}`} hidden={!isItemInfoOpen}
        onMouseDown={(e) => { if (e.target === e.currentTarget) closePopups() }}>
        <div className={`popup ${isPhotoPopup ? 'popup-photo' : 'popup-slim'}`} role="dialog" aria-modal="true" aria-labelledby="item-info-title">
          <button className="close-btn" aria-label="Tutup" onClick={closePopups}>×</button>
          <h2 id="item-info-title">{selectedItem?.label ?? 'Info'}</h2>
          <div className="content">
            {isPhotoPopup ? (
              selectedItem?.target
                ? <img src={selectedItem.target} alt={selectedItem.label} className="photo-preview" />
                : <p className="empty-hint">Belum ada foto. Tekan <b>E</b> lalu klik area ini untuk memilih foto.</p>
            ) : (
              <dl className="item-info-list">
                <dt>Tipe</dt><dd>{selectedItem?.type}</dd>
                <dt>Target</dt><dd className="item-info-target">{selectedItem?.target}</dd>
              </dl>
            )}
          </div>
          <div className="actions">
            {selectedItem?.target && <button className="btn" onClick={() => openItemTarget(selectedItem.target)}>Buka</button>}
            <button className="btn" onClick={closePopups}>Tutup</button>
          </div>
        </div>
      </div>

      {toastMsg && (
        <div className="toast-transient" role="status">{toastMsg}</div>
      )}

      {assignHotspot && (
        <AssignModal hotspot={assignHotspot} onClose={closeAssignModal} />
      )}
    </div>
  )
}