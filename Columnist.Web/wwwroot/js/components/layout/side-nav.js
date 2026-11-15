(function () {
    var root = document.documentElement;

    function isOpen() {
        return root.getAttribute('data-nav') !== 'closed';
    }

    function setOpen(open) {
        if (open) root.removeAttribute('data-nav');
        else root.setAttribute('data-nav', 'closed');
        try { localStorage.setItem('kb-nav', open ? 'open' : 'closed'); } catch (e) { }
        document.querySelectorAll('[data-nav-toggle]').forEach(function (button) {
            button.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
    }

    // The menu button is inside #kb-page, so set its state on first load and after every swap.
    htmx.onLoad(function (elt) {
        if (!elt.querySelectorAll) return;
        elt.querySelectorAll('[data-nav-toggle]').forEach(function (button) {
            button.setAttribute('aria-expanded', isOpen() ? 'true' : 'false');
        });
    });

    // ☰ opens/closes the menu.
    document.addEventListener('click', function (evt) {
        if (!(evt.target.closest && evt.target.closest('[data-nav-toggle]'))) return;
        setOpen(!isOpen());
    });

    // After a side-menu navigation, reload the menu so its highlight follows the new page.
    document.addEventListener('htmx:pushedIntoHistory', function () {
        htmx.trigger(document.body, 'kb:navigated');
    });
})();
