(function () {
    var modals = {
        'kb-delete-form': { modal: 'kb-delete-modal', name: 'kb-delete-name', confirm: 'kb-delete-confirm' },
        'kb-restore-form': { modal: 'kb-restore-modal', name: 'kb-restore-name', confirm: 'kb-restore-confirm' }
    };

    document.addEventListener('htmx:confirm', function (evt) {
        if (!evt.detail.question) return;

        var form = evt.detail.elt;
        var ids = null;
        for (var cls in modals) {
            if (form.classList.contains(cls)) ids = modals[cls];
        }
        if (!ids) return;

        var modalEl = document.getElementById(ids.modal);
        if (!modalEl) return;

        evt.preventDefault();
        var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        var confirmBtn = document.getElementById(ids.confirm);

        document.getElementById(ids.name).textContent = form.dataset.taskTitle;
        confirmBtn.disabled = false;
        confirmBtn.onclick = function () {
            this.disabled = true;
            modal.hide();
            evt.detail.issueRequest(true);
        };
        modal.show();
    });
})();
