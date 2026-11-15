// Image lightbox: an image marked data-lightbox opens full screen. The ✕ button, a click outside the image and Escape close it.
// Event delegation only; the lightbox (data-lightbox-root, _Layout) is looked up when an event fires.
(function () {
    function openLightbox() {
        var root = document.querySelector('[data-lightbox-root]');
        return root && !root.hidden ? root : null;
    }

    function close(root) {
        root.hidden = true;
        var image = root.querySelector('[data-lightbox-image]');
        if (image) image.removeAttribute('src');
    }

    // Open
    document.addEventListener('click', function (evt) {
        var trigger = evt.target.closest && evt.target.closest('[data-lightbox]');
        if (!trigger) return;
        var root = document.querySelector('[data-lightbox-root]');
        if (!root) return;
        root.querySelector('[data-lightbox-image]').src = trigger.src;
        root.hidden = false;
    });

    // Close on ✕ or a click outside the image. Capture on window: runs before every document handler
    // (the task popup, checklists.js, the assignee picker), and stops them, so only the lightbox closes.
    window.addEventListener('click', function (evt) {
        var root = openLightbox();
        if (!root) return;
        evt.stopPropagation();
        var target = evt.target.closest ? evt.target : null;
        if (target && target.closest('[data-lightbox-image]') && !target.closest('[data-lightbox-close]')) return;
        evt.preventDefault();
        close(root);
    }, true);

    // Close on Escape, the same way.
    window.addEventListener('keydown', function (evt) {
        if (evt.key !== 'Escape') return;
        var root = openLightbox();
        if (!root) return;
        evt.stopPropagation();
        evt.preventDefault();
        close(root);
    }, true);
})();
