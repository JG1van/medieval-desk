import { useState, useCallback, useEffect } from 'react'
import { useDataStore, uid } from '../store/useDataStore'
import type { Hotspot } from '../types'

interface AssignForm {
  name: string
  kind: 'configurable' | 'fixed'
  fixedType?: 'note' | 'calendar' | 'photo'
  displayMode: 'popup-list' | 'popup-grid' | 'direct'
  rows: Array<{ name: string; url: string }>
  target: string
}

export function AssignModal({ hotspot, onClose }: { hotspot: Hotspot; onClose: () => void }) {
  const { updateHotspot } = useDataStore()
  const [form, setForm] = useState<AssignForm>({
    name: hotspot.label,
    kind: hotspot.kind,
    fixedType: hotspot.fixedType || 'photo',
    displayMode: hotspot.displayMode || 'popup-list',
    rows: hotspot.groups?.[0]?.items?.map((it) => ({ name: it.label, url: it.target })) ?? [],
    target: hotspot.target || '',
  })

  useEffect(() => {
    if (!hotspot.target) {
      setForm((f) => ({ ...f, target: '' }))
    }
  }, [hotspot])

  const update = useCallback((partial: Partial<AssignForm>) => setForm((f) => ({ ...f, ...partial })), [])

  const save = useCallback(async () => {
    const updated: Partial<Hotspot> = {
      label: form.name,
      kind: form.kind,
      displayMode: form.displayMode,
    }
    if (form.kind === 'fixed') {
      updated.fixedType = form.fixedType
      if (form.fixedType === 'photo') updated.target = form.target
    } else {
      updated.groups = form.rows.length > 0
        ? [{
            id: hotspot.groups?.[0]?.id || uid('g'),
            name: hotspot.groups?.[0]?.name || 'Grup',
            items: form.rows.map((rr, ri) => ({
              id: uid('i'),
              type: 'url' as const,
              label: rr.name || `Item ${ri + 1}`,
              target: rr.url,
            }))
          }]
        : hotspot.groups || []
    }
    await updateHotspot(hotspot.id, (hs) => ({ ...hs, ...updated }))
    onClose()
  }, [form, hotspot, updateHotspot, onClose])

  return (
    <div className="assign-overlay" onClick={(e) => { if (e.target === e.currentTarget) onClose() }}>
      <div className="assign-form" onClick={(e) => e.stopPropagation()}>
        <h2>Atur Hotspot</h2>
        <button className="close-btn" aria-label="Batal" onClick={onClose}>×</button>

        <div className="assign-field">
          <span>Nama</span>
          <input value={form.name} onChange={(e) => update({ name: e.target.value })} placeholder="Nama hotspot" required />
        </div>

        <div className="assign-field">
          <span>Tipe</span>
          <select value={form.kind} onChange={(e) => update({ kind: e.target.value as 'configurable' | 'fixed' })}>
            <option value="configurable">Konfigurabel (Daftar)</option>
            <option value="fixed">Fixed (Note/Kalender/Foto)</option>
          </select>
        </div>

        {form.kind === 'configurable' && (
          <>
            <div className="assign-field">
              <span>Tampilkan</span>
              <select value={form.displayMode} onChange={(e) => update({ displayMode: e.target.value as AssignForm['displayMode'] })}>
                <option value="popup-list">Daftar (list)</option>
                <option value="popup-grid">Grid (grid)</option>
                <option value="direct">Langsung (direct)</option>
              </select>
            </div>

            <div className="assign-field">
              <span>Item</span>
              {form.rows.map((r, ri) => (
                <div key={ri} className="assign-row">
                  <input
                    value={r.name}
                    onChange={(e) => {
                      const rows = [...form.rows]
                      rows[ri] = { ...rows[ri], name: e.target.value }
                      update({ rows })
                    }}
                    placeholder="Nama"
                  />
                  <input
                    value={r.url}
                    onChange={(e) => {
                      const rows = [...form.rows]
                      rows[ri] = { ...rows[ri], url: e.target.value }
                      update({ rows })
                    }}
                    placeholder="Target (path/URL)"
                  />
                </div>
              ))}
              <button className="btn" onClick={() => update({ rows: [...form.rows, { name: '', url: '' }] })}>
                Tambah item
              </button>
            </div>
          </>
        )}

        {form.kind === 'fixed' && form.fixedType === 'photo' && (
          <div className="assign-field">
            <span>Target Foto</span>
            <input value={form.target} onChange={(e) => update({ target: e.target.value })} placeholder="Path/URL foto" />
          </div>
        )}

        {form.kind === 'fixed' && form.fixedType !== 'photo' && (
          <div className="assign-field">
            <span>Jenis Fixed</span>
            <select value={form.fixedType} onChange={(e) => update({ fixedType: e.target.value as AssignForm['fixedType'] })}>
              <option value="note">Note</option>
              <option value="calendar">Kalender</option>
              <option value="photo">Foto</option>
            </select>
          </div>
        )}

        <div className="actions">
          <button className="btn" onClick={onClose}>Batal</button>
          <button className="btn-primary" onClick={save}>Simpan</button>
        </div>
      </div>
    </div>
  )
}