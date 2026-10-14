(function () {
  'use strict';

  var state = { hotspot: null, groupId: null };

  var els = {};

  function el(id) { return document.getElementById(id); }

  function esc(s) {
    return String(s == null ? '' : s)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  var ICONS = { app: '⚙️', file: '📄', folder: '📁', shortcut: '🔗', url: '🌐' };

  function toast(msg) { window.Platform.toast(msg); }

  /* ---------- group popup ---------- */

  function open(hotspot) {
    if (!hotspot) return;
    if (hotspot.kind === 'fixed') { openFixed(hotspot); return; }
    state.hotspot = hotspot;
    state.groupId = hotspot.groups && hotspot.groups.length ? hotspot.groups[0].id : null;
    el('group-popup-title').textContent = hotspot.label;
    renderGroupPopup();
    show(el('group-popup'));
  }

  function closeGroup() {
    state.hotspot = null;
    state.groupId = null;
    hide(el('group-popup'));
  }

  function renderGroupPopup() {
    var hs = state.hotspot;
    if (!hs) return;
    if (!hs.groups) hs.groups = [];

    var tabs = hs.groups.map(function (g) {
      return '<button class="tab' + (g.id === state.groupId ? ' active' : '') +
        '" data-tab="' + esc(g.id) + '">' + esc(g.name) + '</button>';
    }).join('');
    if (!tabs) tabs = '<span class="empty-hint">Belum ada grup.</span>';

    var group = hs.groups.filter(function (g) { return g.id === state.groupId; })[0];
    var items = '';
    if (group) {
      if (group.items.length) {
        items = group.items.map(function (it) {
          return '<button class="item-card" data-item="' + esc(it.id) + '" title="' + esc(it.label) + '">' +
            '<span class="item-icon">' + (ICONS[it.type] || '•') + '</span>' +
            '<span class="item-label">' + esc(it.label) + '</span>' +
            '</button>';
        }).join('');
      } else {
        items = '<p class="empty-hint">Grup ini belum berisi item.</p>';
      }
    }

    var groupTools = group
      ? '<div class="group-tools">' +
        '<button class="btn-mini" id="gp-rename">Ubah Nama</button>' +
        '<button class="btn-mini" id="gp-delete">Hapus Grup</button>' +
        '<button class="btn-mini" id="gp-move">Pindah Item Terpilih</button>' +
        '</div>'
      : '';

    el('group-popup-content').innerHTML =
      '<div class="tabs">' + tabs + '</div>' +
      '<div class="item-grid">' + items + '</div>' +
      '<div class="gp-actions">' +
      '<button class="btn" id="gp-add-group">+ Grup</button>' +
      '<button class="btn" id="gp-add-item">+ Item</button>' +
      '</div>' + groupTools;

    bindGroupPopup(group);
  }

  function bindGroupPopup(group) {
    var box = el('group-popup-content');

    box.querySelectorAll('[data-tab]').forEach(function (t) {
      t.onclick = function () { state.groupId = t.dataset.tab; renderGroupPopup(); };
    });

    box.querySelectorAll('[data-item]').forEach(function (c) {
      c.onclick = function () { onItemClick(group, c.dataset.item); };
    });

    el('gp-add-group').onclick = function () {
      var name = prompt('Nama grup baru:');
      if (!name || !name.trim()) return;
      state.hotspot.groups.push({ id: window.Data.uid('g'), name: name.trim(), items: [] });
      save(renderGroupPopup);
    };

    if (!group) return;

    el('gp-add-item').onclick = function () { editItem(group, null); };

    el('gp-rename').onclick = function () {
      var name = prompt('Nama grup baru:', group.name);
      if (name === null) return;
      if (!name.trim()) { toast('Nama grup tidak boleh kosong.'); return; }
      group.name = name.trim();
      save(renderGroupPopup);
    };

    el('gp-delete').onclick = function () {
      var count = group.items.length;
      var msg = 'Hapus grup "' + group.name + '"?';
      if (count) msg += ' ' + count + ' item di dalamnya juga dihapus.';
      if (!confirm(msg)) return;
      state.hotspot.groups = state.hotspot.groups.filter(function (g) { return g.id !== group.id; });
      state.groupId = state.hotspot.groups.length ? state.hotspot.groups[0].id : null;
      save(renderGroupPopup);
    };

    el('gp-move').onclick = function () { moveItem(group); };
  }

  function save(after) {
    window.App.save().then(function () { if (after) after(); });
  }

  /* ---------- item ---------- */

  function onItemClick(group, itemId) {
    var item = group.items.filter(function (i) { return i.id === itemId; })[0];
    if (!item) return;
    var choice = prompt(
      'Klik "OK" untuk membuka.\nKetik "E" lalu Enter untuk edit.\nKetik "H" lalu Enter untuk hapus.',
      'OK'
    );
    var cmd = (choice || '').trim().toLowerCase();
    if (cmd === 'e') { editItem(group, item); return; }
    if (cmd === 'h') {
      if (!confirm('Hapus item "' + item.label + '"?')) return;
      group.items = group.items.filter(function (i) { return i.id !== itemId; });
      save(renderGroupPopup);
      return;
    }
    window.Platform.openItem(item);
    if (window.App.getSettings().minimizeAfterLaunch) window.Platform.windowMinimize();
  }

  function editItem(group, item) {
    var isNew = !item;
    var it = item || { id: window.Data.uid('i'), type: 'url', label: '', target: '', icon: null };

    var type = prompt('Tipe (url / app / file / folder / shortcut):', it.type);
    if (type === null) return;
    type = type.trim().toLowerCase();
    if (['url', 'app', 'file', 'folder', 'shortcut'].indexOf(type) === -1) {
      toast('Tipe tidak dikenal: ' + type);
      return;
    }

    var label = prompt('Label:', it.label);
    if (label === null) return;
    label = label.trim();
    if (!label) { toast('Label tidak boleh kosong.'); return; }

    var target = prompt('Target (URL atau path lokal):', it.target);
    if (target === null) return;
    target = target.trim();
    if (!target) { toast('Target tidak boleh kosong.'); return; }

    it.type = type;
    it.label = label;
    it.target = target;

    if (isNew) group.items.push(it);
    save(renderGroupPopup);
  }

  function moveItem(group) {
    var others = state.hotspot.groups.filter(function (g) { return g.id !== group.id; });
    if (!others.length) { toast('Tidak ada grup tujuan.'); return; }
    if (!group.items.length) { toast('Grup ini belum punya item.'); return; }

    var itemId = prompt(
      'ID item yang dipindah:\n' + group.items.map(function (i) { return i.id + ' = ' + i.label; }).join('\n')
    );
    if (!itemId) return;
    itemId = itemId.trim();

    var moving = group.items.filter(function (i) { return i.id === itemId; })[0];
    if (!moving) { toast('Item tidak ditemukan: ' + itemId); return; }

    var targetId = prompt(
      'Pindah ke grup mana?\n' + others.map(function (g) { return g.id + ' = ' + g.name; }).join('\n')
    );
    if (!targetId) return;
    targetId = targetId.trim();

    var dest = others.filter(function (g) { return g.id === targetId || g.name === targetId; })[0];
    if (!dest) { toast('Grup tujuan tidak ditemukan: ' + targetId); return; }

    group.items = group.items.filter(function (i) { return i.id !== moving.id; });
    dest.items.push(moving);
    save(renderGroupPopup);
  }

  /* ---------- fixed popup ---------- */

  function openFixed(hotspot) {
    el('fixed-popup-title').textContent = hotspot.label;
    show(el('fixed-popup'));
  }

  /* ---------- overlay helpers ---------- */

  function show(node) {
    node.hidden = false;
    requestAnimationFrame(function () { node.classList.add('active'); });
  }

  function hide(node) {
    node.classList.remove('active');
    setTimeout(function () { node.hidden = true; }, 220);
  }

  /* ---------- wiring ---------- */

  document.addEventListener('DOMContentLoaded', function () {
    el('group-popup').querySelector('.close-btn').onclick = closeGroup;
    el('group-popup-close').onclick = closeGroup;
    el('group-popup-add-group').onclick = el('group-popup-add-group').onclick || function () {};
    el('group-popup-add-group').onclick = function () {
      if (state.hotspot && el('gp-add-group')) el('gp-add-group').click();
      else {
        var name = prompt('Nama grup baru:');
        if (!name || !name.trim()) return;
        state.hotspot.groups.push({ id: window.Data.uid('g'), name: name.trim(), items: [] });
        save(renderGroupPopup);
      }
    };

    el('fixed-popup').querySelector('.close-btn').onclick = function () { hide(el('fixed-popup')); };

    el('item-info-close').onclick = function () { hide(el('item-info-popup')); };
    el('item-info-copy').onclick = function () { window.Platform.copyLastTarget(); };

    ['group-popup', 'fixed-popup', 'item-info-popup'].forEach(function (id) {
      el(id).addEventListener('click', function (e) {
        if (e.target === el(id)) {
          if (id === 'group-popup') closeGroup();
          else hide(el(id));
        }
      });
    });
  });

  window.Popup = {
    open: open,
    closeGroup: closeGroup,
    openFixed: openFixed
  };
})();