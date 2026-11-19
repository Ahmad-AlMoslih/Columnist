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

    function showModal() {
        var modal = modalEl();
        if (modal && !modal.classList.contains('show')) bootstrap.Modal.getOrCreateInstance(modal).show();
    }

    // Open the popup only once it has something to show: the loading skeleton (a slow load) or _Profile.
    document.addEventListener('kb:skeleton-shown', function (evt) {
        if (evt.target === bodyEl()) showModal();
    });
    document.addEventListener('htmx:afterSwap', function (evt) {
        if (evt.target === bodyEl()) showModal();
    });

    function showLoadError(evt) {
        if (!targetsBody(evt)) return;
        bodyEl().innerHTML =
            '<div class="kb-modal-loading" role="alert">' +
            '<span class="kb-modal-loading-text">Could not load this. Try again</span>' +
            '<button type="button" class="kb-btn-secondary" data-bs-dismiss="modal">Close</button></div>';
        showModal();
    }
    document.addEventListener('htmx:responseError', showLoadError);
    document.addEventListener('htmx:sendError', showLoadError);
    // Move focus out of a closing modal so Bootstrap's aria-hidden doesn't hide a focused element
    document.addEventListener('hide.bs.modal', function (e) {
        if (e.target.contains(document.activeElement)) {
            document.activeElement.blur();
        }
    });
    // Stacked popups: put each newly opened popup and its backdrop above the ones already open.
    document.addEventListener('show.bs.modal', function (e) {
        var alreadyOpen = document.querySelectorAll('.modal.show').length;
        if (alreadyOpen === 0) return;
        e.target.style.zIndex = 1055 + alreadyOpen * 10;
    });

    document.addEventListener('shown.bs.modal', function (e) {
        var alreadyOpen = document.querySelectorAll('.modal.show').length - 1;
        if (alreadyOpen === 0) return;
        var backdrops = document.querySelectorAll('.modal-backdrop');
        var last = backdrops[backdrops.length - 1];
        if (last) last.style.zIndex = 1054 + alreadyOpen * 10;
    });

    document.addEventListener('hidden.bs.modal', function (e) {
        e.target.style.zIndex = '';
    });
    document.addEventListener('hidden.bs.modal', function (evt) {
        if (evt.target.id !== 'kb-profile-modal') return;
        var body = bodyEl();
        if (body) body.innerHTML = '';
    });

    // A successful "Save changes" re-renders the profile with data-profile-close: close the popup (its toast moves to the page).
    htmx.onLoad(function (elt) {
        if (!elt.matches || !elt.matches('#kb-profile[data-profile-close]')) return;
        var modal = modalEl();
        if (modal) bootstrap.Modal.getOrCreateInstance(modal).hide();
    });

    // "Save changes" is enabled only while the full name or username differs from the saved value.
    document.addEventListener('input', function (evt) {
        var form = evt.target.closest && evt.target.closest('[data-profile-details]');
        if (!form) return;
        var changed = Array.prototype.some.call(form.querySelectorAll('[data-saved]'), function (input) {
            return input.value.trim() !== input.dataset.saved;
        });
        var save = form.querySelector('[data-profile-save]');
        if (save) save.disabled = !changed;
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
