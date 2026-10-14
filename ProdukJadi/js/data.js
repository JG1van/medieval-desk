(function () {
  'use strict';

  function validate(data) {
    if (!data || typeof data !== 'object') return 'data harus object';
    if (data.version !== 1) return 'version harus 1';
    if (!data.reference || typeof data.reference.width !== 'number' || typeof data.reference.height !== 'number') return 'reference.width/height harus number';
    if (!data.settings || typeof data.settings !== 'object') return 'settings wajib';
    if (typeof data.settings.minimizeAfterLaunch !== 'boolean') return 'settings.minimizeAfterLaunch harus boolean';
    if (typeof data.settings.hotkey !== 'string') return 'settings.hotkey harus string';
    if (!Array.isArray(data.hotspots)) return 'hotspots harus array';
    if (!Array.isArray(data.widgets)) return 'widgets harus array';
    for (var i = 0; i < data.hotspots.length; i++) {
      var h = data.hotspots[i];
      if (typeof h.id !== 'string' || !h.id) return 'hotspots[' + i + '].id wajib';
      if (typeof h.label !== 'string') return 'hotspots[' + i + '].label wajib string';
      if (h.kind !== 'configurable' && h.kind !== 'fixed') return 'hotspots[' + i + '].kind harus configurable atau fixed';
      if (h.kind === 'fixed' && ['calendar', 'note', 'photo'].indexOf(h.fixedType) === -1) return 'hotspots[' + i + '].fixedType tidak valid';
      if (h.box !== null) {
        if (!h.box || typeof h.box !== 'object') return 'hotspots[' + i + '].box harus object atau null';
        var b = h.box;
        if (typeof b.x !== 'number' || typeof b.y !== 'number' || typeof b.w !== 'number' || typeof b.h !== 'number') return 'hotspots[' + i + '].box harus x,y,w,h number';
      }
      if (h.kind === 'configurable') {
        if (!Array.isArray(h.groups)) return 'hotspots[' + i + '].groups harus array';
        for (var gi = 0; gi < h.groups.length; gi++) {
          var g = h.groups[gi];
          if (typeof g.id !== 'string' || !g.id) return 'hotspots[' + i + '].groups[' + gi + '].id wajib';
          if (typeof g.name !== 'string') return 'hotspots[' + i + '].groups[' + gi + '].name wajib string';
          if (!Array.isArray(g.items)) return 'hotspots[' + i + '].groups[' + gi + '].items harus array';
          for (var ii = 0; ii < g.items.length; ii++) {
            var it = g.items[ii];
            if (typeof it.id !== 'string' || !it.id) return 'hotspots[' + i + '].groups[' + gi + '].items[' + ii + '].id wajib';
            if (['app', 'file', 'folder', 'shortcut', 'url'].indexOf(it.type) === -1) return 'hotspots[' + i + '].groups[' + gi + '].items[' + ii + '].type tidak valid';
            if (typeof it.label !== 'string') return 'items[' + ii + '].label wajib string';
            if (typeof it.target !== 'string') return 'items[' + ii + '].target wajib string';
          }
        }
      }
    }
    return null;
  }

  var uidCounter = 0;
  function uid(prefix) {
    uidCounter++;
    return prefix + '-' + Date.now().toString(36) + '-' + uidCounter.toString(36);
  }

  function createDefaultData() {
    return {
      version: 1,
      reference: { width: 1376, height: 768 },
      settings: {
        minimizeAfterLaunch: true,
        hotkey: 'Ctrl+Alt+D',
        location: { name: '', lat: null, lon: null }
      },
      hotspots: [
        { id: 'rak_buku', label: 'Rak Buku', kind: 'configurable', fixedType: null, box: { x: 2.9, y: 19.5, w: 21, h: 67.5 }, groups: [{ id: 'g1', name: 'Rak 1', items: [] }, { id: 'g2', name: 'Rak 2', items: [] }, { id: 'g3', name: 'Rak 3', items: [] }, { id: 'g4', name: 'Rak 4', items: [] }] },
        { id: 'kalender', label: 'Kalender', kind: 'fixed', fixedType: 'calendar', box: { x: 26.9, y: 36.1, w: 5.3, h: 11.9 }, groups: undefined },
        { id: 'gulungan', label: 'Gulungan', kind: 'configurable', fixedType: null, box: { x: 26.4, y: 52.0, w: 6.6, h: 7.4 }, groups: [{ id: 'g_gulungan', name: 'Umum', items: [] }] },
        { id: 'bingkai', label: 'Bingkai', kind: 'fixed', fixedType: 'photo', box: { x: 40.5, y: 23.9, w: 19.2, h: 44.0 }, groups: undefined },
        { id: 'note', label: 'Note', kind: 'fixed', fixedType: 'note', box: { x: 67.4, y: 36.9, w: 6.0, h: 10.6 }, groups: undefined },
        { id: 'bola_kristal', label: 'Bola Kristal', kind: 'configurable', fixedType: null, box: { x: 68.8, y: 47.9, w: 4.4, h: 9.5 }, groups: [{ id: 'g_kristal', name: 'Umum', items: [] }] },
        { id: 'loker', label: 'Loker', kind: 'configurable', fixedType: null, box: { x: 78.0, y: 29.3, w: 19.8, h: 56.0 }, groups: [{ id: 'g_loker_kiri', name: 'Pintu Kiri', items: [] }, { id: 'g_loker_kanan', name: 'Pintu Kanan', items: [] }] },
        { id: 'perisai', label: 'Perisai', kind: 'configurable', fixedType: null, box: { x: 45.2, y: 4.4, w: 9.9, h: 16.1 }, groups: [{ id: 'g_perisai', name: 'Umum', items: [] }] },
        { id: 'burung_gagak', label: 'Burung Gagak', kind: 'configurable', fixedType: null, box: { x: 32.1, y: 52.4, w: 7.4, h: 32.4 }, groups: [{ id: 'g_gagak', name: 'Umum', items: [] }] },
        { id: 'peti', label: 'Peti', kind: 'configurable', fixedType: null, box: { x: 65.4, y: 66.9, w: 10.1, h: 16.7 }, groups: [{ id: 'g_peti', name: 'Downloads', items: [] }] }
      ],
      widgets: []
    };
  }

  window.Data = {
    validate: validate,
    uid: uid,
    createDefaultData: createDefaultData
  };
})();
