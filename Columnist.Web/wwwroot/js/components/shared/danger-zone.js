(function () {
    var form = null;
    var expected = '';

    function modal() {
        return document.getElementById('dz-modal');
    }

    // A "Delete" button with data-dz-open: show the popup; Delete only works once the exact name is typed.
    document.addEventListener('click', function (evt) {
        var modalEl = modal();
        if (!modalEl) return;

        var button = evt.target.closest && evt.target.closest('[data-dz-open]');
        if (button && !button.disabled) {
            form = document.getElementById(button.dataset.dzForm);
            expected = button.dataset.dzName;
            modalEl.querySelector('[data-dz-text]').textContent = button.dataset.dzText;
            modalEl.querySelector('[data-dz-expected]').textContent = expected;
            modalEl.querySelector('[data-dz-input]').value = '';
            modalEl.querySelector('[data-dz-confirm]').disabled = true;
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
            return;
        }

        var confirm = evt.target.closest && evt.target.closest('[data-dz-confirm]');
        if (confirm && !confirm.disabled && form) {
            form.querySelector('[data-dz-field="confirmName"]').value = modalEl.querySelector('[data-dz-input]').value.trim();
            confirm.disabled = true;
            form.submit();
        }
    });

    document.addEventListener('input', function (evt) {
        if (!evt.target.matches || !evt.target.matches('[data-dz-input]')) return;
        modal().querySelector('[data-dz-confirm]').disabled = evt.target.value.trim() !== expected;
    });

    document.addEventListener('shown.bs.modal', function (evt) {
        if (evt.target.id === 'dz-modal') evt.target.querySelector('[data-dz-input]').focus();
    });
})();
