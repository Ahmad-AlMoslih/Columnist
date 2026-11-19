// Escape closes one layer at a time, the top one first: the lightbox (lightbox.js), then the top open popup (the confirm,
// the task popup, the Dashboard's "View all" popup; stacked by z-index in profile.js), and inside "View all" its filter
// panel before the popup itself (dashboard.js). Loaded before the other scripts, so its listeners run first.
(function () {
    var escape = null;   // the Escape being handled: { top: the popup it may close, handled: a popup already took it }

    function lightboxOpen() {
        var root = document.querySelector('[data-lightbox-root]');
        return !!root && !root.hidden;
    }

    // The top popup = the open one with the highest z-index.
    function topModal() {
        var open = Array.prototype.slice.call(document.querySelectorAll('.modal.show'));
        if (open.length === 0) return null;
        open.sort(function (a, b) {
            return (parseInt(getComputedStyle(a).zIndex, 10) || 0) - (parseInt(getComputedStyle(b).zIndex, 10) || 0);
        });
        return open[open.length - 1];
    }

    // Capture on window: before every other handler, the popup Escape may close is noted.
    window.addEventListener('keydown', function (evt) {
        if (evt.key !== 'Escape' || lightboxOpen()) return;
        escape = { top: topModal(), handled: false };
        setTimeout(function () { escape = null; }, 0);
    }, true);

    // During that Escape, only the top popup closes, and only once (Bootstrap's own handler and the fallback below can
    // both try; a popup below must not close with it).
    document.addEventListener('hide.bs.modal', function (evt) {
        if (!escape) return;
        if (escape.handled || evt.target !== escape.top) {
            evt.preventDefault();
            return;
        }
        escape.handled = true;
    });

    // Focus has fallen out of the top popup (e.g. the popup above it closed, or its button was hidden): Bootstrap's
    // handler (on the popup) does not see the key, so the top popup is closed here.
    document.addEventListener('keydown', function (evt) {
        if (evt.key !== 'Escape' || !escape || escape.handled || evt.defaultPrevented) return;
        var top = escape.top;
        if (!top || !top.classList.contains('show') || top.contains(document.activeElement)) return;
        bootstrap.Modal.getOrCreateInstance(top).hide();
    });
})();
