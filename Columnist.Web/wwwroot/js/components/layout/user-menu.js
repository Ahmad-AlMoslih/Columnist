// Top bar account menu: opens on click, closes on outside click or Escape.
(function () {
    function close(toggle) {
        var menu = toggle.parentElement.querySelector('[data-user-menu]');
        if (menu) menu.hidden = true;
        toggle.setAttribute('aria-expanded', 'false');
    }

    function closeAll(except) {
        document.querySelectorAll('[data-user-menu-toggle][aria-expanded="true"]').forEach(function (toggle) {
            if (toggle !== except) close(toggle);
        });
    }

    document.addEventListener('click', function (e) {
        var toggle = e.target.closest('[data-user-menu-toggle]');
        if (toggle) {
            var menu = toggle.parentElement.querySelector('[data-user-menu]');
            var open = toggle.getAttribute('aria-expanded') !== 'true';
            closeAll(toggle);
            menu.hidden = !open;
            toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
            return;
        }
        if (!e.target.closest('[data-user-menu]')) closeAll(null);
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeAll(null);
    });
})();
