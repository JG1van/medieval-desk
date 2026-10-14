import { useState, useCallback } from 'react'
import type { CSSProperties } from 'react'
import { invoke } from '@tauri-apps/api/core'
import { useDataStore, uid } from '../store/useDataStore'
import type { Hotspot, Group, Item } from '../types'

const toast = (msg: string) => window.dispatchEvent(new CustomEvent('md-toast', { detail: msg }))

export function useHotspots() {
  const { data, updateHotspot } = useDataStore()
  const [selectedHotspot, setSelectedHotspot] = useState<Hotspot | null>(null)
  const [selectedGroup, setSelectedGroup] = useState<Group | null>(null)
  const [selectedItem, setSelectedItem] = useState<Item | null>(null)
  const [isPopupOpen, setIsPopupOpen] = useState(false)
  const [isItemInfoOpen, setIsItemInfoOpen] = useState(false)
  const [assignHotspot, setAssignHotspot] = useState<Hotspot | null>(null)

  const openHotspot = useCallback((hs: Hotspot) => {
    setSelectedHotspot(hs)
    const isEditMode = document.body.classList.contains('edit-mode')
    const mode = hs.displayMode || (hs.kind === 'configurable' ? 'popup-list' : 'direct')

    if (isEditMode) {
      setAssignHotspot(hs)
      return
    }

    switch (hs.kind) {
      case 'fixed':
        if (hs.fixedType === 'note') {
          invoke('launch_file', { path: 'notepad' }).catch(() => {})
          toast('Notepad dibuka')
          return
        }
        if (hs.fixedType === 'calendar') {
          invoke('launch_file', { path: 'outlookcal:' }).catch(() => {})
          toast('Kalender dibuka')
          return
        }
        if (hs.fixedType === 'photo') {
          if (mode === 'direct' && hs.target) {
            invoke('launch_file', { path: hs.target }).catch(() => {})
          } else {
            // Photo popup handled in App.tsx
            setSelectedItem({ id: '', type: 'url', label: hs.label, target: hs.target || '' })
            setIsItemInfoOpen(true)
          }
          return
        }
        break
      case 'configurable':
        if (mode === 'popup-list') {
          // Show list popup - handled by isPopupOpen + groups
          if (hs.groups && hs.groups.length > 0) {
            setSelectedGroup(hs.groups[0])
          }
          setIsPopupOpen(true)
          return
        }
        if (mode === 'popup-grid') {
          // Same as popup-list but grid rendering in App
          if (hs.groups && hs.groups.length > 0) {
            setSelectedGroup(hs.groups[0])
          }
          setIsPopupOpen(true)
          return
        }
        if (mode === 'direct') {
          const item = hs.groups?.[0]?.items?.[0]
          if (item) {
            invoke('launch_file', { path: item.target }).catch(() => {})
          }
          return
        }
        break
    }
  }, [])

  const closeAssignModal = useCallback(() => {
    setAssignHotspot(null)
    setSelectedHotspot(null)
  }, [])

  const openGroup = useCallback((group: Group) => {
    setSelectedGroup(group)
  }, [])

  const openItem = useCallback((item: Item) => {
    setSelectedItem(item)
    setIsItemInfoOpen(true)
  }, [])

  const closePopups = useCallback(() => {
    setIsPopupOpen(false)
    setIsItemInfoOpen(false)
    setSelectedItem(null)
    setSelectedGroup(null)
    setSelectedHotspot(null)
  }, [])

  const getHotspotStyle = useCallback((hs: Hotspot): CSSProperties => {
    const { x, y, w, h } = hs.box
    return {
      left: `${x}%`,
      top: `${y}%`,
      width: `${w}%`,
      height: `${h}%`,
    }
  }, [])

  // Group operations
  const addGroup = useCallback(async (hotspotId: string, name: string) => {
    if (!name.trim() || !data) return
    await updateHotspot(hotspotId, (hs) => ({
      ...hs,
      groups: [...(hs.groups || []), { id: uid('g'), name: name.trim(), items: [] }]
    }))
  }, [data, updateHotspot])

  const renameGroup = useCallback(async (hotspotId: string, groupId: string, name: string) => {
    if (!name.trim() || !data) return
    await updateHotspot(hotspotId, (hs) => ({
      ...hs,
      groups: (hs.groups || []).map(g => g.id === groupId ? { ...g, name: name.trim() } : g)
    }))
  }, [data, updateHotspot])

  const deleteGroup = useCallback(async (hotspotId: string, groupId: string) => {
    if (!data) return
    await updateHotspot(hotspotId, (hs) => ({
      ...hs,
      groups: (hs.groups || []).filter(g => g.id !== groupId)
    }))
  }, [data, updateHotspot])

  // Item operations
  const addItem = useCallback(async (hotspotId: string, groupId: string, item: Omit<Item, 'id'>) => {
    if (!data) return
    await updateHotspot(hotspotId, (hs) => ({
      ...hs,
      groups: (hs.groups || []).map(g =>
        g.id === groupId ? { ...g, items: [...g.items, { ...item, id: uid('i') }] } : g
      )
    }))
  }, [data, updateHotspot])

  const editItem = useCallback(async (hotspotId: string, groupId: string, itemId: string, updates: Partial<Item>) => {
    if (!data) return
    await updateHotspot(hotspotId, (hs) => ({
      ...hs,
      groups: (hs.groups || []).map(g =>
        g.id === groupId ? { ...g, items: g.items.map(i => i.id === itemId ? { ...i, ...updates } : i) } : g
      )
    }))
  }, [data, updateHotspot])

  const deleteItem = useCallback(async (hotspotId: string, groupId: string, itemId: string) => {
    if (!data) return
    await updateHotspot(hotspotId, (hs) => ({
      ...hs,
      groups: (hs.groups || []).map(g =>
        g.id === groupId ? { ...g, items: g.items.filter(i => i.id !== itemId) } : g
      )
    }))
  }, [data, updateHotspot])

  const moveItem = useCallback(async (hotspotId: string, fromGroupId: string, toGroupId: string, itemId: string) => {
    const hs = data?.hotspots.find(h => h.id === hotspotId)
    const moving = hs?.groups?.find(g => g.id === fromGroupId)?.items.find(i => i.id === itemId)
    if (!moving) return
    await updateHotspot(hotspotId, (cur) => ({
      ...cur,
      groups: (cur.groups || [])
        .map(g => (g.id === fromGroupId ? { ...g, items: g.items.filter(i => i.id !== itemId) } : g))
        .map(g => (g.id === toGroupId ? { ...g, items: [...g.items, moving] } : g))
    }))
  }, [data, updateHotspot])

  const openItemTarget = useCallback(async (target: string) => {
    if (!target) return
    if (target.startsWith('http://') || target.startsWith('https://')) {
      window.open(target, '_blank')
    } else {
      invoke('launch_file', { path: target }).catch(() => {})
    }
  }, [])

  return {
    selectedHotspot,
    selectedGroup,
    selectedItem,
    isPopupOpen,
    isItemInfoOpen,
    openHotspot,
    openGroup,
    openItem,
    closePopups,
    getHotspotStyle,
    addGroup,
    renameGroup,
    deleteGroup,
    addItem,
    editItem,
    deleteItem,
    moveItem,
    openItemTarget,
    assignHotspot,
    closeAssignModal,
    saveAssignModal: (updates: Partial<Hotspot>) => {
      if (!assignHotspot) return
      updateHotspot(assignHotspot.id, (hs) => ({ ...hs, ...updates }))
      setAssignHotspot(null)
    },
  }
}