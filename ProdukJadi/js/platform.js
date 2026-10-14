(function () {
  'use strict';

  var STORAGE_KEY = 'medieval-desk:data';
  var WEATHER_CACHE_PREFIX = 'medieval-desk:weather:';
  var WEATHER_CACHE_MS = 15 * 60 * 1000;

  function toast(msg) {
    var el = document.createElement('div');
    el.textContent = msg;
    el.style.cssText = 'position:fixed;bottom:24px;left:50%;transform:translateX(-50%);' +
      'background:#285A48;color:#B0E4CC;padding:10px 16px;border-radius:6px;z-index:9999;' +
      'font-family:Georgia,serif;font-size:13px;max-width:80vw;text-align:center;';
    document.body.appendChild(el);
    setTimeout(function () { el.remove(); }, 3200);
  }

  function loadData() {
    return new Promise(function (resolve) {
      var raw = null;
      try {
        raw = localStorage.getItem(STORAGE_KEY);
      } catch (e) { /* abaikan */ }
      if (raw) {
        try {
          resolve(JSON.parse(raw));
          return;
        } catch (e) { /* lanjut ke fetch */ }
      }
      fetch('data/medieval-desk.json')
        .then(function (res) {
          if (!res.ok) throw new Error('HTTP ' + res.status);
          return res.json();
        })
        .then(function (data) {
          try { localStorage.setItem(STORAGE_KEY, JSON.stringify(data)); } catch (e) { /* abaikan */ }
          resolve(data);
        })
        .catch(function () {
          toast('Data default tidak bisa dimuat. Memakai data kosong.');
          resolve({ version: 1, reference: { width: 1376, height: 768 }, settings: { minimizeAfterLaunch: true, hotkey: 'Ctrl+Alt+D', location: { name: '', lat: null, lon: null } }, hotspots: [], widgets: [] });
        });
    });
  }

  function saveData(data) {
    return new Promise(function (resolve, reject) {
      var err = (window.Data && Data.validate) ? Data.validate(data) : null;
      if (err) {
        toast('Validasi gagal: ' + err);
        reject(new Error(err));
        return;
      }
      try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(data));
        resolve();
      } catch (e) {
        toast('Gagal menyimpan: ' + e.message);
        reject(e);
      }
    });
  }

  function exportData() {
    var raw = null;
    try { raw = localStorage.getItem(STORAGE_KEY); } catch (e) { /* abaikan */ }
    if (!raw) { toast('Tidak ada data untuk diekspor.'); return; }
    var blob = new Blob([raw], { type: 'application/json' });
    var a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = 'medieval-desk-backup.json';
    document.body.appendChild(a);
    a.click();
    setTimeout(function () { URL.revokeObjectURL(a.href); a.remove(); }, 100);
  }

  function importData(file) {
    return new Promise(function (resolve, reject) {
      var reader = new FileReader();
      reader.onload = function () {
        var data;
        try {
          data = JSON.parse(reader.result);
        } catch (e) {
          toast('File bukan JSON yang valid.');
          reject(e);
          return;
        }
        var err = (window.Data && Data.validate) ? Data.validate(data) : null;
        if (err) {
          toast('Validasi gagal: ' + err);
          reject(new Error(err));
          return;
        }
        saveData(data).then(function () { resolve(data); }, reject);
      };
      reader.onerror = function () {
        toast('Gagal membaca file.');
        reject(reader.error);
      };
      reader.readAsText(file);
    });
  }

  function openItem(item) {
    if (item.type === 'url') {
      window.open(item.target, '_blank', 'noopener');
      return;
    }
    showItemInfo(item);
  }

  var lastItemTarget = '';

  function showItemInfo(item) {
    lastItemTarget = item.target || '';
    var ov = document.getElementById('item-info-popup');
    document.getElementById('item-info-title').textContent = item.label || '(tanpa label)';
    document.getElementById('item-info-content').innerHTML =
      '<p><strong>Tipe:</strong> ' + escapeHtml(item.type) + '</p>' +
      '<p><strong>Target:</strong> ' + escapeHtml(item.target || '(kosong)') + '</p>' +
      '<p class="assign-hint">Prototipe web tidak bisa membuka file lokal. Versi asli akan membukanya.</p>';
    ov.hidden = false;
    requestAnimationFrame(function () { ov.classList.add('active'); });
  }

  function escapeHtml(s) {
    return String(s == null ? '' : s)
      .replace(/&/g, '&amp;').replace(/</g, '&lt;')
      .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
  }

  function copyLastTarget() {
    if (!lastItemTarget) { toast('Tidak ada target untuk disalin.'); return; }
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(lastItemTarget).then(
        function () { toast('Target disalin.'); },
        function () { toast('Gagal menyalin.'); });
    } else {
      var ta = document.createElement('textarea');
      ta.value = lastItemTarget;
      document.body.appendChild(ta);
      ta.select();
      try {
        document.execCommand('copy');
        toast('Target disalin.');
      } catch (e) { toast('Gagal menyalin.'); }
      ta.remove();
    }
  }

  function windowMinimize() { toast('Hanya tersedia di versi asli'); }
  function windowMaximize() { toast('Hanya tersedia di versi asli'); }
  function windowClose() { toast('Hanya tersedia di versi asli'); }

  function getSystemStats() { return null; }

  function getWeather(lat, lon) {
    if (lat == null || lon == null) return Promise.resolve(null);
    var cacheKey = WEATHER_CACHE_PREFIX + lat + ':' + lon;
    try {
      var cached = localStorage.getItem(cacheKey);
      if (cached) {
        var c = JSON.parse(cached);
        if (Date.now() - c.time < WEATHER_CACHE_MS) return Promise.resolve(c.data);
      }
    } catch (e) { /* abaikan */ }
    var url = 'https://api.open-meteo.com/v1/forecast?latitude=' +
      encodeURIComponent(lat) + '&longitude=' + encodeURIComponent(lon) +
      '&current=temperature_2m,weather_code';
    return fetch(url)
      .then(function (res) {
        if (!res.ok) throw new Error('HTTP ' + res.status);
        return res.json();
      })
      .then(function (data) {
        try { localStorage.setItem(cacheKey, JSON.stringify({ time: Date.now(), data: data })); } catch (e) { /* abaikan */ }
        return data;
      })
      .catch(function () { return null; });
  }

  function now() { return new Date(); }

  window.Platform = {
    loadData: loadData,
    saveData: saveData,
    exportData: exportData,
    importData: importData,
    openItem: openItem,
    copyLastTarget: copyLastTarget,
    windowMinimize: windowMinimize,
    windowMaximize: windowMaximize,
    windowClose: windowClose,
    getSystemStats: getSystemStats,
    getWeather: getWeather,
    now: now,
    toast: toast
  };
})();
