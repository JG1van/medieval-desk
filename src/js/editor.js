// editor.js - Simplified: only assigns content type (app/file) to hotspots.
// Positions are fixed and not draggable. Edit mode just shows hotspots and allows assignment.

let hotspots = [];
let onHotspotUpdate = null;

const CONTENT_TYPES = {
  list: 'Daftar URL (file/app/web/.lnk)',
  note: 'Notepad bawaan',
  calendar: 'Kalender Windows',
  photo: 'Foto (tampil di bingkai)',
};

function getContentTypeOptions() {
  return Object.entries(CONTENT_TYPES).map(([value, label]) => ({ value, label }));
}

function updateHotspotType(id, newType) {
  const idx = hotspots.findIndex((h) => h.id === id);
  if (idx >= 0) {
    hotspots[idx].type = newType;
    // Normalisasi field saat ganti type
    if (newType === 'list') {
      hotspots[idx].items = hotspots[idx].items || [];
    } else if (newType === 'photo') {
      hotspots[idx].target = hotspots[idx].target || '';
    } else {
      hotspots[idx].items = undefined;
      hotspots[idx].target = undefined;
    }
    if (onHotspotUpdate) onHotspotUpdate();
  }
}

function updateHotspotTarget(id, target) {
  const idx = hotspots.findIndex((h) => h.id === id);
  if (idx >= 0) {
    hotspots[idx].target = target;
    if (onHotspotUpdate) onHotspotUpdate();
  }
}

function updateHotspotName(id, name) {
  const idx = hotspots.findIndex((h) => h.id === id);
  if (idx >= 0) {
    hotspots[idx].name = name;
    if (onHotspotUpdate) onHotspotUpdate();
  }
}

function updateHotspotItems(id, items) {
  const idx = hotspots.findIndex((h) => h.id === id);
  if (idx >= 0) {
    hotspots[idx].items = items;
    if (onHotspotUpdate) onHotspotUpdate();
  }
}

function updateHotspotDisplayMode(id, displayMode) {
  const idx = hotspots.findIndex((h) => h.id === id);
  if (idx >= 0) {
    hotspots[idx].displayMode = displayMode;
    if (onHotspotUpdate) onHotspotUpdate();
  }
}

export function initEditor(onUpdate, hotspotList) {
  onHotspotUpdate = onUpdate;
  hotspots = hotspotList;
}

export function updateHotspotsList(list) {
  hotspots = list;
}

export { updateHotspotType, updateHotspotTarget, updateHotspotName, updateHotspotItems, updateHotspotDisplayMode, getContentTypeOptions, CONTENT_TYPES };