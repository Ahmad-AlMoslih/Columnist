// Account menu → "Profile" popup (#kb-profile-modal in _Layout). Opens with a spinner while _Profile loads,
// picks a photo (the file input submits its form on change), and shows/hides the "Change password" fields.
// Event delegation only; elements are looked up when an event fires.
(function () {
    function modalEl() {
        return document.getElementById('kb-profile-modal');
    }

    function bodyEl() {
        return document.getElementById('kb-profile-modal-body');
    }

    function targetsBody(evt) {
        var body = bodyEl();
        var target = evt.detail.target || (evt.detail.requestConfig && evt.detail.requestConfig.target);
        return !!body && target === body;
    }

    // Open the popup (and close the account menu) as soon as the request starts; the spinner shows until _Profile swaps in.
    document.addEventListener('htmx:beforeRequest', function (evt) {
        if (evt.defaultPrevented || !targetsBody(evt)) return;
        var modal = modalEl();
        if (!modal) return;

        document.querySelectorAll('[data-user-menu-toggle][aria-expanded="true"]').forEach(function (toggle) {
            var menu = toggle.parentElement.querySelector('[data-user-menu]');
            if (menu) menu.hidden = true;
            toggle.setAttribute('aria-expanded', 'false');
        });

        bodyEl().innerHTML =
            '<div class="kb-modal-loading" role="status" aria-live="polite">' +
            '<span class="kb-spinner" aria-hidden="true"></span>' +
            '<span class="kb-modal-loading-text">Loading…</span></div>';
        if (!modal.classList.contains('show')) bootstrap.Modal.getOrCreateInstance(modal).show();
    });

    function showLoadError(evt) {
        if (!targetsBody(evt)) return;
        bodyEl().innerHTML =
            '<div class="kb-modal-loading" role="alert">' +
            '<span class="kb-modal-loading-text">Could not load this. Try again</span>' +
            '<button type="button" class="kb-btn-secondary" data-bs-dismiss="modal">Close</button></div>';
    }
    document.addEventListener('htmx:responseError', showLoadError);
    document.addEventListener('htmx:sendError', showLoadError);
    // Move focus out of a closing modal so Bootstrap's aria-hidden doesn't hide a focused element
    document.addEventListener('hide.bs.modal', function (e) {
        if (e.target.contains(document.activeElement)) {
            document.activeElement.blur();
        }
    });
    document.addEventListener('hidden.bs.modal', function (evt) {
        if (evt.target.id !== 'kb-profile-modal') return;
        var body = bodyEl();
        if (body) body.innerHTML = '';
    });

    function passwordForm(root) {
        return root.querySelector('[data-profile-password]');
    }

    document.addEventListener('click', function (evt) {
        if (!evt.target.closest) return;

        // "Upload photo" / "Change photo": open the file picker; choosing a file submits the form (hx-trigger="change").
        var pick = evt.target.closest('[data-profile-photo-pick]');
        if (pick) {
            var input = pick.form && pick.form.querySelector('[data-profile-photo-input]');
            if (input) input.click();
            return;
        }

        var open = evt.target.closest('[data-profile-password-open]');
        if (open) {
            var form = passwordForm(open.parentElement);
            if (!form) return;
            open.hidden = true;
            form.hidden = false;
            var first = form.querySelector('input[type="password"]');
            if (first) first.focus();
            return;
        }

        // Cancel: hide the fields again, empty them and drop their errors.
        var cancel = evt.target.closest('[data-profile-password-cancel]');
        if (cancel) {
            var cancelForm = cancel.closest('[data-profile-password]');
            if (!cancelForm) return;
            cancelForm.reset();
            cancelForm.querySelectorAll('.field-validation-error').forEach(function (message) { message.textContent = ''; });
            cancelForm.hidden = true;
            var button = cancelForm.parentElement.querySelector('[data-profile-password-open]');
            if (button) button.hidden = false;
        }
    });
})();
