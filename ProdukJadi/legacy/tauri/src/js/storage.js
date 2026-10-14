const CURRENT_LAYOUT_VERSION = 2;

const STORAGE_KEYS = {
  HOTSPOTS: 'medieval-desk-hotspots-v2',
  EDIT_MODE: 'medieval-desk-edit-mode',
  POPUP_DATA_PREFIX: 'medieval-desk-popup-',
  LAYOUT_VERSION: 'medieval-desk-layout-version',
};

function normalizePath(p) {
  return String(p == null ? '' : p).trim().replace(/^\\([A-Za-z]:)/, '$1');
}

function migrateHotspot(h) {
  // Normalisasi type lama ke baru
  const typeMap = {
    file: 'list',
    folder: 'list',
    browser: 'list',
    ai: 'list',
    calendar: 'calendar',
    note: 'note',
    photo: 'photo',
    locker: 'list',
  };
  const newType = typeMap[h.type] || h.type;

  // Kalau type lama 'locker' jadi 'list', pastikan items ada
  if (newType === 'list' && !h.items) {
    h.items = [];
  }
  // Kalau type lama photo, pastikan target ada
  if (newType === 'photo' && h.target === undefined) {
    h.target = '';
  }

  return {
    ...h,
    type: newType,
    items: newType === 'list' ? (h.items || []) : undefined,
    target: newType === 'photo' ? (h.target !== undefined ? h.target : '') : undefined,
  };
}

// Default displayMode per ID. Dipakai saat config lama belum punya displayMode.
const DEFAULT_DISPLAY_MODE_BY_ID = {
  rack: 'popup-list',
  scroll: 'popup-list',
  locker: 'popup-grid',
  calendar: 'direct',
  frame: 'direct',
  note: 'direct',
  crystal: 'direct',
  shield: 'direct',
  raven: 'direct',
  chest: 'direct',
};

export function getDefaultDisplayMode(id) {
  return DEFAULT_DISPLAY_MODE_BY_ID[id] || 'popup-grid';
}

function getDefaultHotspots() {
  return [
    { id: 'rack', name: 'Rak Buku', x: 3.2, y: 18.5, w: 21.0, h: 66.5, type: 'list', target: '', items: [], displayMode: 'popup-list' },
    { id: 'calendar', name: 'Kalender', x: 26.9, y: 36.1, w: 5.3, h: 11.9, type: 'calendar', target: '', displayMode: 'direct' },
    { id: 'scroll', name: 'Gulungan (Tool AI)', x: 26.4, y: 52.0, w: 6.6, h: 7.4, type: 'list', target: '', items: [], displayMode: 'popup-list' },
    { id: 'frame', name: 'Bingkai (Foto)', x: 40.5, y: 23.9, w: 19.2, h: 44.0, type: 'photo', target: '', displayMode: 'direct' },
    { id: 'note', name: 'Note', x: 67.4, y: 36.9, w: 6.0, h: 10.6, type: 'note', target: '', displayMode: 'direct' },
    { id: 'crystal', name: 'Bola Kristal (Browser)', x: 68.8, y: 47.9, w: 4.4, h: 9.5, type: 'list', target: '', items: [], displayMode: 'direct' },
    { id: 'locker', name: 'Loker (Berkas)', x: 78.0, y: 29.3, w: 19.8, h: 56.0, type: 'list', target: '', items: [], displayMode: 'popup-grid' },
    { id: 'shield', name: 'Perisai', x: 45.2, y: 4.4, w: 9.9, h: 16.1, type: 'list', target: '', items: [{ name: '', url: '' }], displayMode: 'direct' },
    { id: 'raven', name: 'Gagak', x: 32.1, y: 52.4, w: 7.4, h: 32.4, type: 'list', target: '', items: [{ name: '', url: '' }], displayMode: 'direct' },
    { id: 'chest', name: 'Peti Harta Karun', x: 65.4, y: 66.9, w: 10.1, h: 16.7, type: 'list', target: '', items: [{ name: '', url: '' }], displayMode: 'direct' },
  ];
}

// Merge config pengguna dengan default:
//  (a) tambah default yang ID-nya belum ada
//  (b) isi displayMode yang kosong dari default berdasarkan ID
//  (c) JANGAN timpa pos/nama/items yang sudah ada
export function mergeWithDefaults(userList) {
  const defaults = getDefaultHotspots();
  const defaultById = new Map(defaults.map((d) => [d.id, d]));
  const recentLayout = getLayoutVersion() >= CURRENT_LAYOUT_VERSION;

  const merged = userList.map((h) => {
    const d = defaultById.get(h.id);
    // Sekali saat layout default berubah: refresh posisi hotspot bawaan,
    // tapi JANGAN sentuh nama/target/items milik pengguna.
    const pos = d && !recentLayout ? { x: d.x, y: d.y, w: d.w, h: d.h } : {};
    return {
      ...h,
      ...pos,
      displayMode: h.displayMode || getDefaultDisplayMode(h.id),
    };
  });

  defaults.forEach((d) => {
    if (!userList.some((h) => h.id === d.id)) merged.push({ ...d });
  });

  return merged;
}

function getLayoutVersion() {
  try {
    return Number(localStorage.getItem(STORAGE_KEYS.LAYOUT_VERSION)) || 0;
  } catch {
    return 0;
  }
}

function setLayoutVersion(v) {
  try {
    localStorage.setItem(STORAGE_KEYS.LAYOUT_VERSION, String(v));
  } catch (e) {
    console.error('setLayoutVersion error:', e);
  }
}

export function loadHotspots() {
  try {
    const data = localStorage.getItem(STORAGE_KEYS.HOTSPOTS);
    if (!data) {
      setLayoutVersion(CURRENT_LAYOUT_VERSION);
      return getDefaultHotspots();
    }
    const parsed = JSON.parse(data);
    if (!Array.isArray(parsed)) {
      setLayoutVersion(CURRENT_LAYOUT_VERSION);
      return getDefaultHotspots();
    }
    const normalized = parsed.map((h, i) => ({
      ...migrateHotspot(h),
      id: h.id || `hotspot-${i}`,
      x: Number(h.x) || 0,
      y: Number(h.y) || 0,
      w: Number(h.w) || 0,
      h: Number(h.h) || 0,
      target: h.target !== undefined ? h.target : '',
    }));
    const merged = mergeWithDefaults(normalized);
    setLayoutVersion(CURRENT_LAYOUT_VERSION);
    return merged;
  } catch (e) {
    console.error('loadHotspots error:', e);
    setLayoutVersion(CURRENT_LAYOUT_VERSION);
    return getDefaultHotspots();
  }
}

export function saveHotspots(hotspots) {
  try {
    // Simpan semua field agar tidak ada data hilang (pos, nama, type, items, target, displayMode)
    const saveData = hotspots.map((h) => {
      const base = {
        id: h.id,
        name: h.name,
        x: h.x,
        y: h.y,
        w: h.w,
        h: h.h,
        type: h.type,
        displayMode: h.displayMode || getDefaultDisplayMode(h.id),
      };
      if (h.type === 'photo') {
        base.target = h.target || '';
      }
      if (h.type === 'list') {
        base.items = Array.isArray(h.items) ? h.items : [];
      }
      return base;
    });
    localStorage.setItem(STORAGE_KEYS.HOTSPOTS, JSON.stringify(saveData));
    return true;
  } catch (e) {
    console.error('saveHotspots error:', e);
    return false;
  }
}

export function loadEditMode() {
  try {
    const data = localStorage.getItem(STORAGE_KEYS.EDIT_MODE);
    return data === 'true';
  } catch {
    return false;
  }
}

export function saveEditMode(enabled) {
  try {
    localStorage.setItem(STORAGE_KEYS.EDIT_MODE, String(enabled));
    return true;
  } catch (e) {
    console.error('saveEditMode error:', e);
    return false;
  }
}

export function loadPopupData(key) {
  try {
    const data = localStorage.getItem(STORAGE_KEYS.POPUP_DATA_PREFIX + key);
    return data ? JSON.parse(data) : null;
  } catch {
    return null;
  }
}

export function savePopupData(key, data) {
  try {
    localStorage.setItem(STORAGE_KEYS.POPUP_DATA_PREFIX + key, JSON.stringify(data));
    return true;
  } catch (e) {
    console.error('savePopupData error:', e);
    return false;
  }
}

export function exportConfig() {
  try {
    const hotspots = loadHotspots();
    const editMode = loadEditMode();
    return JSON.stringify({ hotspots, editMode, version: 1 }, null, 2);
  } catch (e) {
    console.error('exportConfig error:', e);
    return null;
  }
}

export function importConfig(json) {
  try {
    const { hotspots, editMode } = JSON.parse(json);
    if (hotspots) saveHotspots(hotspots);
    if (typeof editMode === 'boolean') saveEditMode(editMode);
    return true;
  } catch (e) {
    console.error('importConfig error:', e);
    return false;
  }
}
