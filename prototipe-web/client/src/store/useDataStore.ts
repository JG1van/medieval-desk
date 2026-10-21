import { create } from 'zustand'
import { invoke } from '@tauri-apps/api/core'
import type { DataStore, Hotspot } from '../types'

interface DataState {
  data: DataStore | null
  loading: boolean
  error: string | null
  load: () => Promise<void>
  save: () => Promise<void>
  exportData: () => Promise<void>
  importData: () => Promise<void>
  updateHotspot: (id: string, fn: (hs: Hotspot) => Hotspot) => Promise<void>
}

export const useDataStore = create<DataState>((set, get) => ({
  data: null,
  loading: false,
  error: null,

  load: async () => {
    set({ loading: true, error: null })
    try {
      const data = await invoke<DataStore>('load_data')
      set({ data, loading: false })
    } catch (e) {
      set({ loading: false, error: String(e) })
    }
  },

  save: async () => {
    const { data } = get()
    if (!data) return
    try {
      await invoke('save_data', { data })
    } catch (e) {
      set({ error: String(e) })
    }
  },

  exportData: async () => {
    try {
      await invoke('export_data')
    } catch (e) {
      set({ error: String(e) })
    }
  },

  importData: async () => {
    set({ loading: true, error: null })
    try {
      const data = await invoke<DataStore>('import_data')
      set({ data, loading: false })
    } catch (e) {
      set({ loading: false, error: String(e) })
    }
  },

  // ponytail: whole-store write on each change; fine at this data size.
  // Add field-level patching only if save latency shows up in profiling.
  updateHotspot: async (id, fn) => {
    const { data, save } = get()
    if (!data) return
    const next: DataStore = {
      ...data,
      hotspots: data.hotspots.map((hs) => (hs.id === id ? fn(hs) : hs)),
    }
    set({ data: next })
    await save()
  },
}))

export const uid = (prefix = '') =>
  `${prefix}${Date.now().toString(36)}${Math.random().toString(36).slice(2, 7)}`