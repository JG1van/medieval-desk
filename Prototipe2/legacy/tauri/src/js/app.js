import { loadHotspots, saveHotspots, saveEditMode, loadEditMode, getDefaultDisplayMode } from './storage.js';
import { initHotspots, render as renderHotspots, setEditMode } from './hotspots.js';
import {
  initEditor, updateHotspotsList, updateHotspotType,
  updateHotspotTarget, updateHotspotName, updateHotspotItems,
  updateHotspotDisplayMode,
  getContentTypeOptions,
} from './editor.js';

let hotspots = [];
let editMode = false;
let assigningHotspotId = null;

const DISPLAY_MODES = [
  { value: 'popup-list', label: 'Popup Daftar (satu baris per item)' },
  { value: 'popup-grid', label: 'Popup Grid 3x3' },
  { value: 'direct', label: 'Langsung (buka item pertama)' },
];

// Mode yang dikunci untuk tipe non-daftar
const LOCKED_DISPLAY_MODE_TYPES = ['note', 'calendar', 'photo'];

function isLockedType(type) {
  return LOCKED_DISPLAY_MODE_TYPES.includes(type);
}

const PHOTO_FILTERS = [{
  name: 'Gambar',
  extensions: ['png', 'jpg', 'jpeg', 'webp', 'gif', 'bmp', 'svg'],
}];

function closePopup() {
  const overlay = document.getElementById('popup-overlay');
  if (!overlay) return;
  overlay.classList.remove('active');
  setTimeout(() => {
    overlay.hidden = true;
    overlay.innerHTML = '';
  }, 200);
}

function escapeHtml(str) {
  const div = document.createElement('div');
  div.textContent = str == null ? '' : String(str);
  return div.innerHTML;
}

async function tauriInvoke(command, ...args) {
  try {
    const tauri = window.__TAURI__;
    if (!tauri || !tauri.core) throw new Error('Tauri not initialized');
    return await tauri.core.invoke(command, ...args);
  } catch (e) {
    console.error('tauriInvoke error:', e);
    return null;
  }
}

// Cek apakah target berupa web link atau file lokal
function normalizePath(p) {
  const s = String(p || '').trim();
  // strip single leading backslash before drive letter: \C:\ -> C:\
  return s.replace(/^\\([A-Za-z]:)/, '$1');
}

function isWebUrl(url) {
  return /^https?:\/\//i.test(normalizePath(url));
}

// Buka target: web -> browser, file -> launch_file
async function launchTarget(url) {
  const value = normalizePath(url);
  if (!value) return;

  // Temporarily disable always-on-top so launched app appears on top
  async function toggleAlwaysOnTop(enabled) {
    try {
      const tauri = window.__TAURI__;
      if (tauri && tauri.core && typeof tauri.core.invoke === 'function') {
        await tauri.core.invoke('set_always_on_top', { state: enabled });
      }
    } catch (e) {
      console.error('toggleAlwaysOnTop error:', e);
    }
  }

  await toggleAlwaysOnTop(false);

  try {
    if (isWebUrl(value)) {
      try {
        const tauri = window.__TAURI__;
        if (tauri && tauri.opener && typeof tauri.opener.openUrl === 'function') {
          await tauri.opener.openUrl(value);
        } else if (tauri && tauri.core && typeof tauri.core.invoke === 'function') {
          await tauri.core.invoke('plugin:opener|open_url', { url: value });
        } else {
          window.open(value, '_blank');
        }
      } catch (e) {
        console.error('open web url error:', e);
        window.open(value, '_blank');
      }
    } else {
      await tauriInvoke('launch_file', { path: value });
    }
  } finally {
    // Re-enable always-on-top after a short delay for window to focus
    setTimeout(() => toggleAlwaysOnTop(true), 800);
  }
}

// File picker native via tauri-plugin-dialog
async function browseTarget(filters) {
  const t = window.__TAURI__;
  const opts = { multiple: false, directory: false, ...(filters ? { filters } : {}) };
  try {
    if (t && t.dialog && typeof t.dialog.open === 'function') return await t.dialog.open(opts);
    if (t && t.plugins && t.plugins.dialog && typeof t.plugins.dialog.open === 'function') return await t.plugins.dialog.open(opts);
    if (t && t.core && typeof t.core.invoke === 'function') {
      const r = await t.core.invoke('plugin:dialog|open', { options: opts });
      if (r) return r;
    }
  } catch (e) { console.error('browseTarget error:', e); }
  try {
    if (t && t.core && typeof t.core.invoke === 'function') {
      const r2 = await t.core.invoke('plugin:dialog|open', { options: opts });
      if (r2) return r2;
    }
  } catch {}
  return null;
}

function showTransientPopup(message) {
  const overlay = document.getElementById('popup-overlay');
  if (!overlay) return;
  overlay.innerHTML = `
    <div class="popup popup-slim" role="status">
      <h2 class="popup-slim-title">${escapeHtml(message)}</h2>
    </div>
  `;
  overlay.hidden = false;
  overlay.classList.add('active');
  setTimeout(() => { overlay.classList.remove('active'); }, 1400);
  setTimeout(() => { overlay.hidden = true; overlay.innerHTML = ''; }, 2400);
}

// ---------- Modal Assign (mode edit) ----------

function listRowHtml(item, index) {
  const it = item || { name: '', url: '' };
  const kind = isWebUrl(it.url) ? 'Web' : 'File';
  return `
    <div class="list-row" data-index="${index}">
      <input type="text" class="list-row-name" placeholder="Nama (mis. Claude)" value="${escapeHtml(it.name)}" />
      <input type="text" class="list-row-url" placeholder="http://... atau C:\path\app.lnk" value="${escapeHtml(it.url)}" />
      <span class="list-row-kind" title="Jenis target">${kind}</span>
      <button type="button" class="list-row-browse btn-mini" title="Pilih file">Browse</button>
      <button type="button" class="list-row-remove btn-mini btn-danger" title="Hapus baris">&times;</button>
    </div>
  `;
}

function openAssignModal(hotspot) {
  assigningHotspotId = hotspot.id;
  const overlay = document.getElementById('popup-overlay');
  if (!overlay) return;

  const options = getContentTypeOptions().map(opt =>
    `<option value="${opt.value}" ${opt.value === hotspot.type ? 'selected' : ''}>${opt.label}</option>`
  ).join('');

  const lockedType = isLockedType(hotspot.type);
  const initialMode = lockedType ? 'direct' : (hotspot.displayMode || getDefaultDisplayMode(hotspot.id));
  const modeOptions = DISPLAY_MODES.map(m =>
    `<option value="${m.value}" ${m.value === initialMode ? 'selected' : ''}>${m.label}</option>`
  ).join('');

  let rows = Array.isArray(hotspot.items) && hotspot.items.length
    ? hotspot.items
    : [{ name: '', url: '' }];
  if (initialMode === 'direct' && rows.length > 1) rows = [rows[0]];

  overlay.innerHTML = `
    <div class="popup popup-wide" role="dialog" aria-modal="true" aria-labelledby="assign-title">
      <button class="close-btn" aria-label="Tutup">&times;</button>
      <h2 id="assign-title">Atur Area: ${escapeHtml(hotspot.name)}</h2>
      <div class="content">
        <div class="assign-form">
          <label class="assign-field">
            <span>Nama</span>
            <input type="text" id="assign-name" value="${escapeHtml(hotspot.name)}" />
          </label>
          <label class="assign-field">
            <span>Jenis</span>
            <select id="assign-type">${options}</select>
          </label>
          <label class="assign-field" id="field-display-mode" ${lockedType ? 'hidden' : ''}>
            <span>Mode tampil</span>
            <select id="assign-display-mode" ${lockedType ? 'disabled' : ''}>${modeOptions}</select>
          </label>

          <div class="assign-section" id="section-list" hidden>
            <div class="assign-hint">
              <span class="badge badge-web">Web</span> <code>http://</code> atau <code>https://</code>
              &nbsp;&middot;&nbsp;
              <span class="badge badge-file">File</span> <code>C:\Users\...</code>
              <br />
              <small>Isi <b>Nama</b> + <b>URL/Path</b>. Klik <b>Browse</b> untuk pilih file .lnk / folder, atau ketik URL web manual.</small>
            </div>
            <div id="list-rows">${rows.map((r, i) => listRowHtml(r, i)).join('')}</div>
            <button type="button" class="btn btn-mini" id="list-add">+ Tambah Baris</button>
          </div>

          <div class="assign-section" id="section-photo" hidden>
            <div class="assign-hint"><small>Pilih gambar yang akan tampil di dalam bingkai.</small></div>
            <div class="assign-inline">
              <input type="text" id="assign-target" placeholder="Path gambar" value="${escapeHtml(hotspot.target || '')}" />
              <button type="button" class="btn btn-mini" id="photo-browse">Browse</button>
            </div>
          </div>

          <div class="assign-section" id="section-simple" hidden>
            <div class="assign-hint"><small>Tipe ini memakai aplikasi bawaan Windows. Tidak perlu target.</small></div>
          </div>
        </div>
      </div>
      <div class="actions">
        <button class="btn" id="assign-save">Simpan</button>
        <button class="btn" id="assign-cancel" style="background:var(--paper-border);color:var(--paper-text);">Batal</button>
      </div>
    </div>
  `;

  overlay.hidden = false;
  requestAnimationFrame(() => overlay.classList.add('active'));

  const typeSelect = document.getElementById('assign-type');
  const displayModeSelect = document.getElementById('assign-display-mode');
  const fieldDisplayMode = document.getElementById('field-display-mode');
  const sectionList = document.getElementById('section-list');
  const sectionPhoto = document.getElementById('section-photo');
  const sectionSimple = document.getElementById('section-simple');
  const listRows = document.getElementById('list-rows');
  const listAdd = document.getElementById('list-add');
  const targetInput = document.getElementById('assign-target');

  function refreshKindBadges() {
    listRows.querySelectorAll('.list-row').forEach((row) => {
      const url = row.querySelector('.list-row-url').value;
      const badge = row.querySelector('.list-row-kind');
      badge.textContent = isWebUrl(url) ? 'Web' : 'File';
      badge.className = 'list-row-kind ' + (isWebUrl(url) ? 'kind-web' : 'kind-file');
    });
  }

  function currentMode() {
    return displayModeSelect.value || 'popup-grid';
  }

  function updateSections(type, displayMode) {
    sectionList.hidden = type !== 'list';
    sectionPhoto.hidden = type !== 'photo';
    sectionSimple.hidden = type !== 'list' && type !== 'photo';

    const locked = isLockedType(type);
    fieldDisplayMode.hidden = locked;
    displayModeSelect.disabled = locked;
    if (locked) {
      displayModeSelect.value = 'direct';
      displayMode = 'direct';
    }

    const isDirect = displayMode === 'direct';
    if (type === 'list') {
      if (isDirect) {
        listAdd.hidden = true;
        const rowsEls = [...listRows.querySelectorAll('.list-row')];
        if (rowsEls.length > 1) rowsEls.slice(1).forEach(r => r.remove());
      } else {
        listAdd.hidden = false;
      }
    }
  }

  typeSelect.addEventListener('change', () => {
    const type = typeSelect.value;
    if (isLockedType(type)) {
      displayModeSelect.value = 'direct';
    } else {
      const cur = hotspot.displayMode || getDefaultDisplayMode(hotspot.id);
      displayModeSelect.value = cur;
    }
    updateSections(type, currentMode());
  });

  displayModeSelect.addEventListener('change', () => {
    updateSections(typeSelect.value, currentMode());
  });

  updateSections(typeSelect.value, currentMode());
  refreshKindBadges();

  listRows.addEventListener('input', refreshKindBadges);

  listRows.addEventListener('click', async (e) => {
    const row = e.target.closest('.list-row');
    if (!row) return;

    if (e.target.classList.contains('list-row-remove')) {
      if (listRows.children.length > 1) row.remove();
      else {
        row.querySelector('.list-row-name').value = '';
        row.querySelector('.list-row-url').value = '';
        refreshKindBadges();
      }
      return;
    }

    if (e.target.classList.contains('list-row-browse')) {
      const picked = await browseTarget();
      if (picked) {
        row.querySelector('.list-row-url').value = picked;
        const nameInput = row.querySelector('.list-row-name');
        if (!nameInput.value.trim()) {
          nameInput.value = String(picked).split(/[\\/]/).pop().replace(/\.(lnk|exe)$/i, '');
        }
        refreshKindBadges();
      }
    }
  });

  document.getElementById('list-add').addEventListener('click', () => {
    listRows.insertAdjacentHTML('beforeend', listRowHtml({ name: '', url: '' }, listRows.children.length));
    refreshKindBadges();
  });

  document.getElementById('photo-browse').addEventListener('click', async () => {
    const picked = await browseTarget(PHOTO_FILTERS);
    if (!picked) return;
    const copied = await tauriInvoke('import_photo', { src: picked });
    targetInput.value = copied || picked;
  });

  const closeBtn = overlay.querySelector('.close-btn');
  if (closeBtn) closeBtn.addEventListener('click', closePopup);
  overlay.addEventListener('click', (e) => { if (e.target === overlay) closePopup(); });
  document.getElementById('assign-cancel').addEventListener('click', closePopup);

  document.getElementById('assign-save').addEventListener('click', () => {
    const name = document.getElementById('assign-name').value.trim();
    const type = typeSelect.value;
    if (!name) return;

    updateHotspotName(assigningHotspotId, name);
    updateHotspotType(assigningHotspotId, type);

    const mode = isLockedType(type) ? 'direct' : currentMode();
    updateHotspotDisplayMode(assigningHotspotId, mode);

    if (type === 'list') {
      const items = [...listRows.querySelectorAll('.list-row')]
        .map((row) => ({
          name: row.querySelector('.list-row-name').value.trim(),
          url: normalizePath(row.querySelector('.list-row-url').value),
        }))
        .filter((it) => it.url);
      updateHotspotItems(assigningHotspotId, items);
    } else if (type === 'photo') {
      updateHotspotTarget(assigningHotspotId, normalizePath(targetInput.value));
    }

    hotspots = loadHotspots();
    updateHotspotsList(hotspots);
renderHotspots();
      renderInlinePhotos();
      closePopup();
    assigningHotspotId = null;
  });
}

// ---------- Popup normal mode ----------

function openListPopup(hotspot) {
  const overlay = document.getElementById('popup-overlay');
  if (!overlay) return;

  const items = Array.isArray(hotspot.items) ? hotspot.items.filter((it) => it.url) : [];
  const PAGE_SIZE = 9;
  let page = 0;
  const totalPages = Math.max(1, Math.ceil(items.length / PAGE_SIZE));

  function basename(url) {
    if (/^https?:\/\//i.test(url)) {
      try { return new URL(url).hostname || url; } catch { return url; }
    }
    const p = String(url).replace(/[/\\]+$/, '').split(/[/\\]/);
    return p[p.length - 1] || url;
  }

  function emojiFallback(url) {
    if (/^https?:\/\//i.test(url)) return '🌐';
    if (/\.lnk$/i.test(url)) return '🔗';
    if (/^[A-Za-z]:[\\/]/.test(url) || url.startsWith('//')) {
      if (/[/\\][^/\\]+\.[A-Za-z0-9]{1,5}$/.test(url)) return '⚙️';
      return '📁';
    }
    return '⚙️';
  }

  async function fillIcons(pageItems) {
    const deduped = [...new Set(pageItems.map((it) => normalizePath(it.url)).filter((u) => !/^https?:\/\//i.test(u) && u))];
    let iconMap = {};
    if (deduped.length) {
      try { iconMap = (await tauriInvoke('get_icons', { paths: deduped })) || {}; } catch {}
    }
    const boxes = overlay.querySelectorAll('.app-card-icon');
    pageItems.forEach((it, i) => {
      const box = boxes[i];
      if (!box) return;
      const key = normalizePath(it.url);
      const b64 = iconMap[key];
      if (typeof b64 === 'string' && b64) {
        box.innerHTML = '';
        const img = document.createElement('img');
        img.alt = '';
        img.src = `data:image/png;base64,${b64}`;
        box.appendChild(img);
      } else {
        box.textContent = emojiFallback(it.url);
      }
    });
  }

  function render() {
    if (!items.length) {
      overlay.innerHTML = `
        <div class="popup popup-grid" role="dialog" aria-modal="true" aria-labelledby="list-title-${hotspot.id}">
          <button class="close-btn" aria-label="Tutup">&times;</button>
          <h2 id="list-title-${hotspot.id}">${escapeHtml(hotspot.name)}</h2>
          <div class="content"><p class="empty-hint">Belum ada daftar. Tekan <b>Ctrl+E</b> lalu klik area ini untuk menambahkan.</p></div>
        </div>`;
      overlay.hidden = false;
      requestAnimationFrame(() => overlay.classList.add('active'));
      overlay.querySelector('.close-btn')?.addEventListener('click', closePopup);
      overlay.addEventListener('click', (e) => { if (e.target === overlay) closePopup(); });
      return;
    }

    const start = page * PAGE_SIZE;
    const pageItems = items.slice(start, start + PAGE_SIZE);
    overlay.innerHTML = `
      <div class="popup popup-grid" role="dialog" aria-modal="true" aria-labelledby="list-title-${hotspot.id}">
        <button class="close-btn" aria-label="Tutup">&times;</button>
        <h2 id="list-title-${hotspot.id}">${escapeHtml(hotspot.name)}</h2>
        <div class="content">
          <div class="app-grid">
            ${pageItems
              .map(
                (it, idx) => `
              <button type="button" class="app-card" data-index="${start + idx}">
                <span class="app-card-icon"></span>
                <span class="app-card-name" title="${escapeHtml(it.name || basename(it.url))}">${escapeHtml(it.name || basename(it.url))}</span>
              </button>`
              )
              .join('')}
          </div>
          <div class="app-pager">
            <button type="button" class="app-pager-btn" id="pg-prev" ${page === 0 ? 'disabled' : ''}>◀</button>
            <span class="app-pager-label">${page + 1}/${totalPages}</span>
            <button type="button" class="app-pager-btn" id="pg-next" ${page >= totalPages - 1 ? 'disabled' : ''}>▶</button>
          </div>
        </div>
      </div>`;
    overlay.hidden = false;
    requestAnimationFrame(() => overlay.classList.add('active'));
    overlay.querySelector('.close-btn')?.addEventListener('click', closePopup);
    overlay.addEventListener('click', (e) => { if (e.target === overlay) closePopup(); });
    overlay.querySelectorAll('.app-card').forEach((btn) => {
      btn.addEventListener('click', () => {
        const idx = Number(btn.dataset.index);
        const it = items[idx];
        if (it) launchTarget(it.url);
      });
    });
    const prev = document.getElementById('pg-prev');
    const next = document.getElementById('pg-next');
    if (prev) prev.addEventListener('click', () => { if (page > 0) { page -= 1; render(); } });
    if (next) next.addEventListener('click', () => { if (page < totalPages - 1) { page += 1; render(); } });
    fillIcons(pageItems);
  }

  render();
}

function openListPopupDirect(hotspot) {
  const items = Array.isArray(hotspot.items) ? hotspot.items.filter((it) => it.url) : [];
  if (!items.length) {
    showTransientPopup('Belum ada item. Tekan Ctrl+E lalu klik area ini untuk menambahkan.');
    return;
  }
  launchTarget(items[0].url);
}

function openListPopupList(hotspot) {
  const overlay = document.getElementById('popup-overlay');
  if (!overlay) return;

  const items = Array.isArray(hotspot.items) ? hotspot.items.filter((it) => it.url) : [];

  function basename(url) {
    if (/^https?:\/\//i.test(url)) {
      try { return new URL(url).hostname || url; } catch { return url; }
    }
    const p = String(url).replace(/[/\\]+$/, '').split(/[/\\]/);
    return p[p.length - 1] || url;
  }

  function emojiFallback(url) {
    if (/^https?:\/\//i.test(url)) return '🌐';
    if (/\.lnk$/i.test(url)) return '🔗';
    if (/^[A-Za-z]:[\\/]/.test(url) || url.startsWith('//')) {
      if (/[/\\][^/\\]+\.[A-Za-z0-9]{1,5}$/.test(url)) return '⚙️';
      return '📁';
    }
    return '⚙️';
  }

  async function fillIcons(pageItems) {
    const deduped = [...new Set(pageItems.map((it) => normalizePath(it.url)).filter((u) => !/^https?:\/\//i.test(u) && u))];
    let iconMap = {};
    if (deduped.length) {
      try { iconMap = (await tauriInvoke('get_icons', { paths: deduped })) || {}; } catch {}
    }
    const boxes = overlay.querySelectorAll('.app-card-icon');
    pageItems.forEach((it, i) => {
      const box = boxes[i];
      if (!box) return;
      const key = normalizePath(it.url);
      const b64 = iconMap[key];
      if (typeof b64 === 'string' && b64) {
        box.innerHTML = '';
        const img = document.createElement('img');
        img.alt = '';
        img.src = `data:image/png;base64,${b64}`;
        box.appendChild(img);
      } else {
        box.textContent = emojiFallback(it.url);
      }
    });
  }

  function render() {
    if (!items.length) {
      overlay.innerHTML = `
        <div class="popup popup-list" role="dialog" aria-modal="true" aria-labelledby="list-title-${hotspot.id}">
          <button class="close-btn" aria-label="Tutup">&times;</button>
          <h2 id="list-title-${hotspot.id}">${escapeHtml(hotspot.name)}</h2>
          <div class="content"><p class="empty-hint">Belum ada daftar. Tekan <b>Ctrl+E</b> lalu klik area ini untuk menambahkan.</p></div>
        </div>`;
      overlay.hidden = false;
      requestAnimationFrame(() => overlay.classList.add('active'));
      overlay.querySelector('.close-btn')?.addEventListener('click', closePopup);
      overlay.addEventListener('click', (e) => { if (e.target === overlay) closePopup(); });
      return;
    }

    overlay.innerHTML = `
      <div class="popup popup-list" role="dialog" aria-modal="true" aria-labelledby="list-title-${hotspot.id}">
        <button class="close-btn" aria-label="Tutup">&times;</button>
        <h2 id="list-title-${hotspot.id}">${escapeHtml(hotspot.name)}</h2>
        <div class="content">
          <div class="app-list">
            ${items.map((it, idx) => `
              <button type="button" class="app-card" data-index="${idx}">
                <span class="app-card-icon"></span>
                <span class="app-card-name" title="${escapeHtml(it.name || basename(it.url))}">${escapeHtml(it.name || basename(it.url))}</span>
              </button>`).join('')}
          </div>
        </div>
      </div>`;
    overlay.hidden = false;
    requestAnimationFrame(() => overlay.classList.add('active'));
    overlay.querySelector('.close-btn')?.addEventListener('click', closePopup);
    overlay.addEventListener('click', (e) => { if (e.target === overlay) closePopup(); });
    overlay.querySelectorAll('.app-card').forEach((btn) => {
      btn.addEventListener('click', () => {
        const idx = Number(btn.dataset.index);
        const it = items[idx];
        if (it) launchTarget(it.url);
      });
    });
    fillIcons(items);
  }

  render();
}

let currentPhotoImg = null;
function openPhotoPopupDirect(hotspot) {
  const target = String(hotspot.target || '').trim();
  if (!target) return;
  if (currentPhotoImg) {
    currentPhotoImg.remove();
    currentPhotoImg = null;
  }
  let src = target;
  if (!src.startsWith('http') && !src.startsWith('data:') && !src.startsWith('asset:') && !src.startsWith('file:')) {
    const tauri = window.__TAURI__;
    if (tauri && tauri.core && tauri.core.convertFileSrc) {
      src = tauri.core.convertFileSrc(src);
    }
  }
  const wallpaper = document.getElementById('wallpaper');
  if (!wallpaper) return;
  const rect = wallpaper.getBoundingClientRect();
  const img = document.createElement('img');
  img.src = src;
  img.alt = hotspot.name;
  img.style.position = 'absolute';
  img.style.left = `${rect.left + rect.width * 0.432}px`;
  img.style.top = `${rect.top + rect.height * 0.287}px`;
  img.style.width = `${rect.width * (0.570 - 0.432)}px`;
  img.style.height = `${rect.height * (0.637 - 0.287)}px`;
  img.style.objectFit = 'contain';
  img.style.pointerEvents = 'none';
  img.style.zIndex = '5';
  img.style.transition = 'opacity 0.2s';
  wallpaper.parentNode.appendChild(img);
  currentPhotoImg = img;
}

function openSimplePopup(hotspot, message) {
  const overlay = document.getElementById('popup-overlay');
  if (!overlay) return;
  overlay.innerHTML = `
    <div class="popup popup-slim" role="dialog" aria-modal="true">
      <h2 class="popup-slim-title">${escapeHtml(hotspot.name)}</h2>
      <p class="popup-slim-note">${escapeHtml(message)}</p>
      <div class="actions"><button class="btn" id="simple-close">Tutup</button></div>
    </div>
  `;
  overlay.hidden = false;
  requestAnimationFrame(() => overlay.classList.add('active'));
  document.getElementById('simple-close').addEventListener('click', closePopup);
  overlay.addEventListener('click', (e) => { if (e.target === overlay) closePopup(); });
}

function openPhotoPopup(hotspot) {
  const target = String(hotspot.target || '').trim();
  if (!target) {
    openSimplePopup(hotspot, 'Belum ada foto. Tekan Ctrl+E lalu klik area ini untuk memilih foto.');
    return;
  }

  let src = target;
  if (!src.startsWith('http') && !src.startsWith('data:') && !src.startsWith('asset:') && !src.startsWith('file:')) {
    const tauri = window.__TAURI__;
    if (tauri && tauri.core && tauri.core.convertFileSrc) {
      src = tauri.core.convertFileSrc(src);
    }
  }

  const overlay = document.getElementById('popup-overlay');
  if (!overlay) return;
  overlay.innerHTML = `
    <div class="popup popup-photo" role="dialog" aria-modal="true" aria-label="${escapeHtml(hotspot.name)}">
      <button class="close-btn" aria-label="Tutup">&times;</button>
      <h2>${escapeHtml(hotspot.name)}</h2>
      <div class="content">
        <img id="photo-preview" alt="${escapeHtml(hotspot.name)}">
      </div>
    </div>
  `;

  const img = document.getElementById('photo-preview');
  img.addEventListener('error', () => {
    img.remove();
    overlay.querySelector('.content').innerHTML =
      '<p class="empty-hint">Foto gagal dimuat.</p>';
  });
  img.src = src;

  overlay.hidden = false;
  requestAnimationFrame(() => overlay.classList.add('active'));

  const closeBtn = overlay.querySelector('.close-btn');
  if (closeBtn) closeBtn.addEventListener('click', closePopup);
  overlay.addEventListener('click', (e) => { if (e.target === overlay) closePopup(); });
}

function openPopup(hotspot) {
  if (editMode) {
    openAssignModal(hotspot);
    return;
  }

  const mode = hotspot.displayMode || getDefaultDisplayMode(hotspot.id);

  switch (hotspot.type) {
    case 'note':
      launchTarget('notepad');
      showTransientPopup('Notepad dibuka');
      return;
    case 'calendar':
      launchTarget('outlookcal:');
      showTransientPopup('Kalender dibuka');
      return;
    case 'photo':
      if (mode === 'direct') {
        const target = String(hotspot.target || '').trim();
        if (target) launchTarget(target);
        return;
      } else {
        openPhotoPopup(hotspot);
      }
      return;
    case 'list':
      if (mode === 'popup-list') {
        openListPopupList(hotspot);
      } else if (mode === 'direct') {
        openListPopupDirect(hotspot);
      } else {
        openListPopup(hotspot);
      }
      return;
  }
}

// Render foto inline di area bingkai (mode direct). Idempoten.
let inlinePhotoImgs = [];
function renderInlinePhotos() {
  inlinePhotoImgs.forEach((el) => el.remove());
  inlinePhotoImgs = [];
  const wallpaper = document.getElementById('wallpaper');
  if (!wallpaper) return;
  const rect = wallpaper.getBoundingClientRect();
  hotspots.forEach((hs) => {
    if (hs.type !== 'photo') return;
    const mode = hs.displayMode || getDefaultDisplayMode(hs.id);
    if (mode !== 'direct') return;
    const target = String(hs.target || '').trim();
    if (!target) return;
    let src = target;
    if (!src.startsWith('http') && !src.startsWith('data:') && !src.startsWith('asset:') && !src.startsWith('file:')) {
      const tauri = window.__TAURI__;
      if (tauri && tauri.core && tauri.core.convertFileSrc) src = tauri.core.convertFileSrc(src);
    }
    const img = document.createElement('img');
    img.src = src;
    img.alt = hs.name;
    img.className = 'inline-photo';
    img.style.position = 'absolute';
    img.style.left = `${rect.left + rect.width * 0.432}px`;
img.style.top = `${rect.top + rect.height * 0.287}px`;
    img.style.width = `${rect.width * (0.570 - 0.432)}px`;
img.style.height = `${rect.height * (0.637 - 0.287)}px`;
    img.style.objectFit = 'contain';
    img.style.pointerEvents = 'none';
    img.style.zIndex = '5';
    wallpaper.parentNode.appendChild(img);
    inlinePhotoImgs.push(img);
  });
}

function toggleEditMode() {
  editMode = !editMode;
  setEditMode(editMode);
  saveEditMode(editMode);
}

document.addEventListener('keydown', (e) => {
  if (e.ctrlKey && e.key.toLowerCase() === 'e') {
    e.preventDefault();
    toggleEditMode();
  }
  if (e.key === 'Escape') {
    closePopup();
  }
});

document.addEventListener('DOMContentLoaded', () => {
  const editBtn = document.getElementById('edit-toggle');
  if (editBtn) editBtn.addEventListener('click', toggleEditMode);

  hotspots = loadHotspots();
  editMode = loadEditMode();
  updateHotspotsList(hotspots);

  initHotspots(document.getElementById('hotspots-layer'), openPopup);

  initEditor(() => {
    saveHotspots(hotspots);
    hotspots = loadHotspots();
    updateHotspotsList(hotspots);
  }, hotspots);

  if (editMode) setEditMode(true);
  renderInlinePhotos();
  window.addEventListener('resize', () => {
    renderInlinePhotos();
    renderHotspots();
  });

  // ---------- Window controls (minimize / maximize / close) ----------
  initWindowControls();
});

function initWindowControls() {
  const t = window.__TAURI__;
  if (!t || !t.window || typeof t.window.getCurrentWindow !== 'function') {
    return;
  }
  const appWin = t.window.getCurrentWindow();

  const btnMin = document.querySelector('.win-minimize');
  const btnMax = document.querySelector('.win-maximize');
  const btnClose = document.querySelector('.win-close');
  if (!btnMin || !btnMax || !btnClose) return;

  let isMax = false;

  async function refreshMaxIcon() {
    try {
      isMax = await appWin.isMaximized();
    } catch {
      isMax = false;
    }
    btnMax.innerHTML = isMax ? '&#10687;' : '&#9634;';
    btnMax.setAttribute('aria-label', isMax ? 'Restore' : 'Maximize');
    btnMax.title = isMax ? 'Restore' : 'Maximize';
  }

  btnMin.addEventListener('click', () => {
    appWin.minimize().catch((e) => console.error('minimize error:', e));
  });

  btnMax.addEventListener('click', async () => {
    try {
      await appWin.toggleMaximize();
    } catch (e) {
      console.error('toggleMaximize error:', e);
    }
    await refreshMaxIcon();
  });

  btnClose.addEventListener('click', () => {
    appWin.close().catch((e) => console.error('close error:', e));
  });

  const dragRegion = document.querySelector('.drag-region');
  if (dragRegion) {
    dragRegion.addEventListener('dblclick', () => {
      appWin.toggleMaximize().catch((e) => console.error('toggleMaximize error:', e));
    });
  }

  appWin.onResized(async () => {
    renderHotspots();
    renderInlinePhotos();
    await refreshMaxIcon();
  });

  refreshMaxIcon();
}