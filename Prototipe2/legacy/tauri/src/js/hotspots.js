import { loadHotspots, saveHotspots } from './storage.js';

let hotspots = [];
let container = null;
let onHotspotClick = null;
let editMode = false;

function getContainerRect() {
  const wallpaper = document.getElementById('wallpaper');
  if (!wallpaper) return null;
  return wallpaper.getBoundingClientRect();
}

function percentToPixels(pctX, pctY, pctW, pctH, rect) {
  return {
    left: rect.left + (pctX / 100) * rect.width,
    top: rect.top + (pctY / 100) * rect.height,
    width: (pctW / 100) * rect.width,
    height: (pctH / 100) * rect.height,
  };
}

function createHotspotElement(hotspot, rect) {
  const pos = percentToPixels(hotspot.x, hotspot.y, hotspot.w, hotspot.h, rect);
  const el = document.createElement('div');
  el.className = 'hotspot-area';
  el.dataset.id = hotspot.id;
  el.style.left = `${pos.left}px`;
  el.style.top = `${pos.top}px`;
  el.style.width = `${pos.width}px`;
  el.style.height = `${pos.height}px`;
  el.tabIndex = 0;

  const tooltip = document.createElement('span');
  tooltip.className = 'tooltip';
  tooltip.textContent = hotspot.name;
  el.appendChild(tooltip);

  el.addEventListener('click', (e) => {
    e.stopPropagation();
    if (onHotspotClick) onHotspotClick(hotspot);
  });

  return el;
}

function createEditOutline(hotspot, rect) {
  const pos = percentToPixels(hotspot.x, hotspot.y, hotspot.w, hotspot.h, rect);
  const outline = document.createElement('div');
  outline.className = 'hotspot-outline';
  outline.dataset.id = hotspot.id;
  outline.style.left = `${pos.left}px`;
  outline.style.top = `${pos.top}px`;
  outline.style.width = `${pos.width}px`;
  outline.style.height = `${pos.height}px`;

  // Label dan tombol assign hanya di edit mode
  const label = document.createElement('span');
  label.className = 'hotspot-label';
  label.textContent = hotspot.name;
  outline.appendChild(label);

  const assignBtn = document.createElement('button');
  assignBtn.className = 'assign-btn';
  assignBtn.textContent = '⚙️';
  assignBtn.title = 'Atur isi area';
  assignBtn.dataset.id = hotspot.id;
  outline.appendChild(assignBtn);

  return outline;
}

export function initHotspots(containerEl, clickCallback) {
  container = containerEl;
  onHotspotClick = clickCallback;
  hotspots = loadHotspots();
  render();
  window.addEventListener('resize', render);
}

export function render() {
  if (!container) return;
  container.innerHTML = '';
  const rect = getContainerRect();
  if (!rect) return;

  hotspots = loadHotspots(); // reload from localStorage to reflect edits from assign modal

  const layer = document.getElementById('hotspots-layer');
  if (!layer) return;

  hotspots.forEach((hs) => {
    layer.appendChild(createHotspotElement(hs, rect));
    if (editMode) {
      layer.appendChild(createEditOutline(hs, rect));
    }
  });

  window.dispatchEvent(new CustomEvent('hotspots-rendered'));
}

export function setEditMode(enabled) {
  editMode = enabled;
  document.body.classList.toggle('edit-mode', enabled);
  const toggle = document.getElementById('edit-toggle');
  if (toggle) toggle.classList.toggle('active', enabled);
  render();
}

export function getHotspots() {
  return hotspots;
}

export function updateHotspot(id, updates) {
  const idx = hotspots.findIndex((h) => h.id === id);
  if (idx >= 0) {
    hotspots[idx] = { ...hotspots[idx], ...updates };
    saveHotspots(hotspots);
    render();
  }
}

export function addHotspot(hotspot) {
  hotspots.push(hotspot);
  saveHotspots(hotspots);
  render();
}

export function removeHotspot(id) {
  hotspots = hotspots.filter((h) => h.id !== id);
  saveHotspots(hotspots);
  render();
}