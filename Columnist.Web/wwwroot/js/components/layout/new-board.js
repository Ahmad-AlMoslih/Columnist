(function () {
    // Side panel → BOARDS "＋" opens the "New board" popup (#kb-new-board-modal in _Layout).
    // Escape and Cancel close it (Bootstrap modal). Elements are looked up when an event fires.
    document.addEventListener('click', function (evt) {
        var button = evt.target.closest && evt.target.closest('[data-new-board-open]');
        var modal = document.getElementById('kb-new-board-modal');
        if (!button || !modal) return;

        var name = modal.querySelector('[data-new-board-name]');
        if (name) name.value = '';
        var error = modal.querySelector('.kb-add-column-error');
        if (error) error.remove();

        bootstrap.Modal.getOrCreateInstance(modal).show();
    });

    document.addEventListener('shown.bs.modal', function (evt) {
        if (evt.target.id !== 'kb-new-board-modal') return;
        var name = evt.target.querySelector('[data-new-board-name]');
        if (name) name.focus();
    });

    // Creating failed: the form came back with its error; keep the focus on the name.
    document.addEventListener('htmx:afterSwap', function (evt) {
        var modal = document.getElementById('kb-new-board-modal');
        if (!modal || !evt.detail.target || evt.detail.target.id !== 'kb-new-board-body') return;
        var name = modal.querySelector('[data-new-board-name]');
        if (name) name.focus();
    });
})();
