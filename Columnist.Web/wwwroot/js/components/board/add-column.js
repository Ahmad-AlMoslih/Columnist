(function () {
    // Board page → "＋ Add another column" (owner only). The popup (#kb-add-column-modal) sits outside
    // #kb-board-region, so it is looked up when an event fires. Escape and Cancel close it (Bootstrap modal).
    function modalEl() {
        return document.getElementById('kb-add-column-modal');
    }

    // The statuses, loaded the first time the popup opens (Boards/ColumnStatuses); then the first one is chosen.
    function loadStatuses(form) {
        var select = form.querySelector('[data-add-column-status]');
        if (select.options.length > 0 || select.dataset.loading) return;
        select.dataset.loading = '1';
        var token = form.querySelector('[data-add-column-board]').value;
        fetch(select.dataset.statusesUrl + '?boardToken=' + encodeURIComponent(token))
            .then(function (response) { return response.ok ? response.text() : ''; })
            .then(function (html) {
                if (html && select.options.length === 0) {
                    select.innerHTML = html;
                    select.selectedIndex = 0;
                    syncDot(form);
                }
            })
            .catch(function () { /* nothing loaded: opening the popup again retries */ })
            .finally(function () { delete select.dataset.loading; });
    }

    function syncDot(form) {
        var select = form.querySelector('[data-add-column-status]');
        var option = select.options[select.selectedIndex];
        form.querySelector('[data-column-dot]').style.background = option ? option.dataset.color : '';
    }

    document.addEventListener('click', function (evt) {
        var button = evt.target.closest && evt.target.closest('[data-add-column-open]');
        var modal = modalEl();
        if (!button || !modal) return;

        var form = modal.querySelector('[data-add-column-form]');
        var region = button.closest('[data-board-token]');
        if (region) form.querySelector('[data-add-column-board]').value = region.dataset.boardToken;
        form.querySelector('[data-add-column-name]').value = '';
        form.querySelector('[data-add-column-status]').selectedIndex = 0;
        syncDot(form);
        loadStatuses(form);
        var error = form.querySelector('#kb-add-column-error');
        if (error) error.textContent = '';

        bootstrap.Modal.getOrCreateInstance(modal).show();
    });

    document.addEventListener('shown.bs.modal', function (evt) {
        if (evt.target.id !== 'kb-add-column-modal') return;
        evt.target.querySelector('[data-add-column-name]').focus();
    });

    document.addEventListener('change', function (evt) {
        if (!evt.target.matches || !evt.target.matches('[data-add-column-status]')) return;
        syncDot(evt.target.closest('[data-add-column-form]'));
    });

    // Saved: the board region was refreshed, so close the popup.
    document.addEventListener('htmx:afterSwap', function (evt) {
        var elt = evt.detail.requestConfig && evt.detail.requestConfig.elt;
        var modal = modalEl();
        if (!modal || !elt || !elt.matches || !elt.matches('[data-add-column-form]')) return;
        if (evt.detail.target && evt.detail.target.id === 'kb-board-region') {
            bootstrap.Modal.getOrCreateInstance(modal).hide();
        }
    });
})();
