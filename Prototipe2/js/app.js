(function () {
  'use strict';

  var state = { data: null, editMode: false };

  function ready(fn) {
    var img = document.getElementById('wallpaper');
    if (img.complete && img.naturalWidth) { fn(); return; }
    img.addEventListener('load', fn, { once: true });
    img.addEventListener('error', fn, { once: true });
  }

  function init() {
    window.Platform.loadData().then(function (data) {
      state.data = data;
      setupDataStore();
      window.Stage.onHotspotClick = onHotspotClick;
      ready(function () { window.Stage.render(state.data.hotspots); });
      wireControls();
    });
  }

  function setupDataStore() {
    window.DataStore = {
      getHotspots: function () { return state.data.hotspots; },
      save: function () { return window.Platform.saveData(state.data); }
    };
  }

  function onHotspotClick(hs) {
    if (state.editMode) return;
    if (hs.kind === 'fixed') window.Popup.openFixed(hs);
    else window.Popup.open(hs);
  }

  function wireControls() {
    var toggle = document.getElementById('edit-toggle');
    toggle.onclick = function () { toggleEdit(); };

    window.addEventListener('resize', function () {
      if (state.data) window.Stage.render(state.data.hotspots);
    });

    document.addEventListener('keydown', function (e) {
      var tag = document.activeElement ? document.activeElement.tagName : '';
      if (tag === 'INPUT' || tag === 'TEXTAREA') return;
      if (e.key === 'e' || e.key === 'E') { e.preventDefault(); toggleEdit(); }
      if (e.key === 'Escape') closeAll();
    });

    document.querySelectorAll('.window-controls .win-btn').forEach(function (b) {
      b.onclick = function () {
        var c = b.className;
        if (c.indexOf('win-minimize') > -1) window.Platform.windowMinimize();
        else if (c.indexOf('win-maximize') > -1) window.Platform.windowMaximize();
        else if (c.indexOf('win-close') > -1) window.Platform.windowClose();
      };
    });
  }

  function toggleEdit() {
    state.editMode = !state.editMode;
    window.Stage.setEditMode(state.editMode);
    var toggle = document.getElementById('edit-toggle');
    toggle.classList.toggle('active', state.editMode);
    toggle.textContent = state.editMode ? '✓' : '✏️';
    toggle.setAttribute('aria-label', state.editMode ? 'Keluar mode edit' : 'Masuk mode edit (E)');
  }

  function closeAll() {
    window.Popup.closeGroup();
    ['fixed-popup', 'item-info-popup'].forEach(function (id) {
      var n = document.getElementById(id);
      if (n && !n.hidden) {
        n.classList.remove('active');
        setTimeout(function () { n.hidden = true; }, 200);
      }
    });
  }

  window.App = {
    save: function () { return window.Platform.saveData(state.data); },
    getSettings: function () {
      return state.data ? state.data.settings : { minimizeAfterLaunch: true };
    },
    getData: function () { return state.data; }
  };

  init();
})();