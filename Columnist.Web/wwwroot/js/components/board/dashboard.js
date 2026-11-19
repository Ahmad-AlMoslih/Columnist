(function () {
    var current = null;   // the open card's key

    function modalEl() {
        return document.getElementById('kb-dash-modal');
    }

    function cardOf(key) {
        return document.querySelector('[data-dash-card="' + CSS.escape(key) + '"]');
    }

    function isShown(modal) {
        return !!modal && modal.classList.contains('show');
    }

    // The open card's rows into the popup, with their count. keepScroll: a live refresh, not a new list.
    function load(keepScroll) {
        var modal = modalEl();
        var card = current && cardOf(current);
        if (!modal || !card) return;
        var key = current;
        var body = modal.querySelector('[data-dash-modal-body]');
        var list = modal.querySelector('[data-dash-modal-list]');
        var empty = modal.querySelector('[data-dash-modal-empty]');
        var scroll = body.scrollTop;
        htmx.ajax('GET', card.dataset.dashUrl, { source: card, target: list, swap: 'innerHTML' }).then(function () {
            if (current !== key) return;
            var count = list.children.length;
            modal.querySelector('[data-dash-modal-count]').textContent = count;
            empty.textContent = card.hasAttribute('data-dash-activity') ? 'No recent activity.' : 'No tasks.';
            empty.hidden = count > 0;
            body.scrollTop = keepScroll ? scroll : 0;
        });
    }

    function open(card) {
        var modal = modalEl();
        if (!modal) return;
        current = card.dataset.dashCard;

        modal.querySelector('[data-dash-modal-title]').textContent = card.dataset.dashTitle;
        modal.querySelector('[data-dash-modal-icon]').innerHTML = card.querySelector('[data-dash-icon]').innerHTML;
        modal.querySelector('.db-modal').setAttribute('style', card.getAttribute('style') || '');
        modal.querySelector('[data-dash-modal-count]').textContent = card.querySelector('.db-count').textContent;
        modal.querySelector('[data-dash-modal-list]').innerHTML = '';
        modal.querySelector('[data-dash-modal-empty]').hidden = true;

        load(false);
        bootstrap.Modal.getOrCreateInstance(modal).show();
    }

    document.addEventListener('click', function (evt) {
        var button = evt.target.closest && evt.target.closest('[data-dash-open]');
        if (!button) return;
        var card = button.closest('[data-dash-card]');
        if (card) open(card);
    });

    // Only the open card's rows go in: a late answer for a card closed since is dropped.
    document.addEventListener('htmx:beforeSwap', function (evt) {
        var modal = modalEl();
        if (!modal || evt.detail.target !== modal.querySelector('[data-dash-modal-list]')) return;
        var card = current && cardOf(current);
        if (!card || evt.detail.requestConfig.path !== card.dataset.dashUrl) evt.detail.shouldSwap = false;
    });

    document.addEventListener('hidden.bs.modal', function (evt) {
        var modal = modalEl();
        if (!modal) return;
        if (evt.target === modal) {
            current = null;
            modal.querySelector('[data-dash-modal-list]').innerHTML = '';
            return;
        }
        // The task popup (on top) closed: Bootstrap unlocked the page, but this popup is still open.
        if (isShown(modal)) {
            document.body.classList.add('modal-open');
            document.body.style.overflow = 'hidden';
        }
    });

    // A live refresh morphed the cards: the open popup re-loads its card's rows, keeping the scroll.
    document.addEventListener('htmx:afterSettle', function (evt) {
        var target = evt.detail && evt.detail.target;
        if (!current || !target || target.id !== 'kb-report-region') return;
        if (isShown(modalEl())) load(true);
    });
})();
