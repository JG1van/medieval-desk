(function () {
  'use strict';

  function getContainerRect() {
    var wallpaper = document.getElementById('wallpaper');
    if (!wallpaper) return null;
    return wallpaper.getBoundingClientRect();
  }

  function percentToPixels(pctX, pctY, pctW, pctH, rect) {
    return {
      left: rect.left + (pctX / 100) * rect.width,
      top: rect.top + (pctY / 100) * rect.height,
      width: (pctW / 100) * rect.width,
      height: (pctH / 100) * rect.height
    };
  }

  function createHotspotElement(hotspot, rect) {
    var box = hotspot.box;
    if (!box) return null;
    var pos = percentToPixels(box.x, box.y, box.w, box.h, rect);
    var el = document.createElement('div');
    el.className = 'hotspot-area';
    el.dataset.id = hotspot.id;
    el.style.left = pos.left + 'px';
    el.style.top = pos.top + 'px';
    el.style.width = pos.width + 'px';
    el.style.height = pos.height + 'px';
    el.tabIndex = 0;

    var tooltip = document.createElement('span');
    tooltip.className = 'tooltip';
    tooltip.textContent = hotspot.label;
    el.appendChild(tooltip);

    el.addEventListener('click', function (e) {
      e.stopPropagation();
      if (window.Stage && typeof window.Stage.onHotspotClick === 'function') {
        window.Stage.onHotspotClick(hotspot);
      }
    });

    return el;
  }

  function createEditOutline(hotspot, rect) {
    var box = hotspot.box;
    if (!box) return null;
    var pos = percentToPixels(box.x, box.y, box.w, box.h, rect);
    var outline = document.createElement('div');
    outline.className = 'hotspot-outline';
    outline.dataset.id = hotspot.id;
    outline.style.left = pos.left + 'px';
    outline.style.top = pos.top + 'px';
    outline.style.width = pos.width + 'px';
    outline.style.height = pos.height + 'px';

    var label = document.createElement('span');
    label.className = 'hotspot-label';
    label.textContent = hotspot.label;
    outline.appendChild(label);

    var handle = document.createElement('span');
    handle.className = 'resize-handle';
    outline.appendChild(handle);

    return outline;
  }

  var editMode = false;
  var drag = null;

  function currentHotspots() {
    return window.DataStore ? window.DataStore.getHotspots() : [];
  }

  function render(hotspots) {
    var layer = document.getElementById('hotspots-layer');
    if (!layer) return;
    layer.innerHTML = '';
    var rect = getContainerRect();
    if (!rect) return;

    (hotspots || currentHotspots()).forEach(function (hs) {
      var el = createHotspotElement(hs, rect);
      if (el) layer.appendChild(el);
      if (editMode) {
        var ol = createEditOutline(hs, rect);
        if (ol) layer.appendChild(ol);
      }
    });

    window.dispatchEvent(new CustomEvent('hotspots-rendered'));
  }

  function setEditMode(enabled) {
    editMode = enabled;
    document.body.classList.toggle('edit-mode', enabled);
    var toggle = document.getElementById('edit-toggle');
    if (toggle) toggle.classList.toggle('active', enabled);
    render();
  }

  function onMouseDown(e) {
    if (!editMode) return;
    var outline = e.target.closest('.hotspot-outline');
    if (!outline) return;
    if (e.target.classList.contains('assign-btn')) return;
    var id = outline.dataset.id;
    var hs = currentHotspots().find(function (h) { return h.id === id; });
    if (!hs) return;
    var rect = getContainerRect();
    if (!rect) return;

    drag = {
      hs: hs,
      mode: e.target.classList.contains('resize-handle') ? 'resize' : 'move',
      startX: e.clientX,
      startY: e.clientY,
      x: hs.box.x, y: hs.box.y, w: hs.box.w, h: hs.box.h,
      rect: rect
    };
    e.preventDefault();
    e.stopPropagation();
    document.body.style.userSelect = 'none';
  }

  function onMouseMove(e) {
    if (!drag) return;
    var rect = drag.rect;
    var dx = (e.clientX - drag.startX) / rect.width * 100;
    var dy = (e.clientY - drag.startY) / rect.height * 100;
    var b = drag.hs.box;

    if (drag.mode === 'resize') {
      var newW = Math.max(1, Math.min(100 - drag.x, drag.w + dx));
      var newH = Math.max(1, Math.min(100 - drag.y, drag.h + dy));
      b.w = round1(newW);
      b.h = round1(newH);
    } else {
      b.x = round1(Math.max(0, Math.min(100 - b.w, drag.x + dx)));
      b.y = round1(Math.max(0, Math.min(100 - b.h, drag.y + dy)));
    }
    render();
  }

  function onMouseUp() {
    if (drag) {
      if (window.DataStore && window.DataStore.save) window.DataStore.save();
      drag = null;
      document.body.style.userSelect = '';
    }
  }

  function round1(n) { return Math.round(n * 10) / 10; }

  document.addEventListener('mousedown', onMouseDown);
  document.addEventListener('mousemove', onMouseMove);
  document.addEventListener('mouseup', onMouseUp);

  window.Stage = {
    render: render,
    setEditMode: setEditMode,
    getEditMode: function () { return editMode; },
    onHotspotClick: null,
    percentToPixels: percentToPixels,
    getContainerRect: getContainerRect
  };
})();