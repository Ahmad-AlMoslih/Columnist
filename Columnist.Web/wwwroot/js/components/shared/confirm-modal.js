(function () {
    // One shared confirm popup (#kb-confirm-modal in _Layout). Any button or form with
    // data-confirm-title, data-confirm-text and data-confirm-yes asks here first and only
    // continues (submits / sends the HTMX request) when the action button is pressed.
    var HTMX = '[hx-get],[hx-post],[hx-put],[hx-patch],[hx-delete]';
    var confirmed = new WeakSet();
    var pending = null;

    function modalEl() {
        return document.getElementById('kb-confirm-modal');
    }

    function viaHtmx(el) {
        return el.matches(HTMX) || !!el.closest('[hx-boost="true"]');
    }

    // options: { confirmTitle, confirmText, confirmYes } (an element's dataset fits).
    function open(options, onYes) {
        var modal = modalEl();
        if (!modal) return;
        modal.querySelector('[data-confirm-modal-title]').textContent = options.confirmTitle || '';
        modal.querySelector('[data-confirm-modal-text]').textContent = options.confirmText || '';
        var yes = modal.querySelector('[data-confirm-modal-yes]');
        yes.textContent = options.confirmYes || 'OK';
        yes.disabled = false;
        pending = onYes;
        bootstrap.Modal.getOrCreateInstance(modal).show();
    }

    // For scripts that start an action without a click on a marked element (e.g. a drag and drop).
    window.kbConfirm = open;

    document.addEventListener('click', function (evt) {
        var yes = evt.target.closest && evt.target.closest('[data-confirm-modal-yes]');
        if (yes) {
            if (!pending || yes.disabled) return;
            var run = pending;
            pending = null;
            yes.disabled = true;
            bootstrap.Modal.getOrCreateInstance(modalEl()).hide();
            run();
            return;
        }

        // A plain (non-HTMX) button: stop the click, ask, then click it again.
        var button = evt.target.closest && evt.target.closest('button[data-confirm-title]');
        if (!button || viaHtmx(button)) return;
        if (confirmed.has(button)) {
            confirmed.delete(button);
            return;
        }
        evt.preventDefault();
        evt.stopImmediatePropagation();
        open(button.dataset, function () {
            confirmed.add(button);
            button.click();
        });
    }, true);

    // A plain (non-HTMX) form: stop the submit, ask, then submit it again.
    document.addEventListener('submit', function (evt) {
        var form = evt.target;
        if (!form.dataset || !form.dataset.confirmTitle || viaHtmx(form)) return;
        if (confirmed.has(form)) {
            confirmed.delete(form);
            return;
        }
        evt.preventDefault();
        evt.stopImmediatePropagation();
        var submitter = evt.submitter;
        open(form.dataset, function () {
            confirmed.add(form);
            if (submitter) form.requestSubmit(submitter);
            else form.requestSubmit();
        });
    }, true);

    // HTMX elements: hold the request until the action button is pressed.
    document.addEventListener('htmx:confirm', function (evt) {
        var elt = evt.detail.elt;
        if (!elt.dataset || !elt.dataset.confirmTitle) return;
        evt.preventDefault();
        open(elt.dataset, function () { evt.detail.issueRequest(true); });
    });

    document.addEventListener('hidden.bs.modal', function (evt) {
        if (evt.target.id === 'kb-confirm-modal') pending = null;
    });

    // Deleted tasks → Restore keeps its own popup (#kb-restore-modal).
    document.addEventListener('htmx:confirm', function (evt) {
        var form = evt.detail.elt;
        if (form.dataset.confirm !== 'restore') return;

        var modalEl = document.getElementById('kb-restore-modal');
        if (!modalEl) return;

        evt.preventDefault();
        var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        var confirmBtn = document.getElementById('kb-restore-confirm');

        document.getElementById('kb-restore-name').textContent = form.dataset.taskTitle;
        confirmBtn.disabled = false;
        confirmBtn.onclick = function () {
            this.disabled = true;
            modal.hide();
            evt.detail.issueRequest(true);
        };
        modal.show();
    });
})();
