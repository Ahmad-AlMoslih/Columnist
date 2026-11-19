(function () {
    // Look the popup up when an event fires: the board page may be swapped in after this script loads.
    function modalEl() {
        return document.getElementById('kb-task-modal');
    }

    function bodyEl() {
        return document.getElementById('kb-task-modal-body');
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

    // Open the popup only once it has something to show: the loading skeleton (skeleton.js, a slow load) or the content.
    document.addEventListener('kb:skeleton-shown', function (evt) {
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

    // The backdrop is a separate element: a body class lets the CSS give it the task popup's quick fade.
    document.addEventListener('show.bs.modal', function (evt) {
        if (evt.target.id === 'kb-task-modal') document.body.classList.add('kb-task-modal-open');
    });

    document.addEventListener('htmx:afterSwap', function (evt) {
        var body = bodyEl();
        if (body && evt.target === body) showModal();
    });

    // The popup's Delete succeeded (HX-Reswap: none; the toast and the card's removal came out of band): close the popup.
    // A failure (the error toast swapped into #kb-toasts) leaves it open.
    document.addEventListener('htmx:afterRequest', function (evt) {
        var form = evt.detail.elt;
        if (!form || !form.matches || !form.matches('.kb-view-delete-form')) return;
        var xhr = evt.detail.xhr;
        if (!evt.detail.successful || !xhr || xhr.getResponseHeader('HX-Reswap') !== 'none') return;
        var modal = modalEl();
        var body = bodyEl();
        if (!modal || !body) return;
        bootstrap.Modal.getOrCreateInstance(modal).hide();
        body.innerHTML = '';
    });

    document.addEventListener('hidden.bs.modal', function (evt) {
        if (evt.target.id !== 'kb-task-modal') return;
        document.body.classList.remove('kb-task-modal-open');
        var body = bodyEl();
        if (body) body.innerHTML = '';
    });

    // Client validation for any loaded content that contains a form (the popup, or a page swapped in from the side menu).
    htmx.onLoad(function (elt) {
        if (!window.jQuery || !$.validator || !$.validator.unobtrusive) return;
        var hasForm = (elt.matches && elt.matches('form')) || (elt.querySelector && elt.querySelector('form'));
        if (hasForm) $.validator.unobtrusive.parse(elt);
    });

    // htmx submits on its own listener, so stop the request when jquery.validate finds errors.
    document.addEventListener('htmx:beforeRequest', function (evt) {
        var form = evt.detail.elt;
        var body = bodyEl();
        if (form.tagName !== 'FORM' || !body || !body.contains(form) || !window.jQuery || !$.fn.valid) return;
        if (!$(form).valid()) evt.preventDefault();
    });

    // Keep the card's comment count in sync after the comments section loads or changes.
    htmx.onLoad(function (elt) {
        if (!elt.matches || !elt.matches('#kb-comments[data-comments-for]')) return;
        var card = document.querySelector('[data-card-token="' + CSS.escape(elt.dataset.commentsFor) + '"]');
        var badge = card && card.querySelector('[data-comment-badge]');
        if (!badge) return;
        var count = parseInt(elt.dataset.commentCount, 10) || 0;
        badge.hidden = count === 0;
        badge.title = count + (count === 1 ? ' comment' : ' comments');
        badge.querySelector('[data-comment-number]').textContent = count;
    });

    // A live refresh replaces only the list: copy its count into the comments header.
    htmx.onLoad(function (elt) {
        if (!elt.matches || !elt.matches('#kb-comment-feed[data-comment-count]')) return;
        var section = elt.closest('#kb-comments');
        var header = section && section.querySelector('[data-comments-header]');
        if (!header) return;
        header.textContent = elt.dataset.commentCount;
        section.dataset.commentCount = elt.dataset.commentCount;
    });

    // Clicking anywhere on a card opens the task (same request as its title link),
    // except the ⋯ menu, the cover image and links. A drag (pointer moved) is not a click.
    var down = null;
    document.addEventListener('pointerdown', function (evt) {
        down = { x: evt.clientX, y: evt.clientY };
    });

    document.addEventListener('click', function (evt) {
        var card = evt.target.closest && evt.target.closest('[data-card-token]');
        if (!card || evt.target.closest('a, button, input, form, [data-lightbox]')) return;
        if (down && (Math.abs(evt.clientX - down.x) > 5 || Math.abs(evt.clientY - down.y) > 5)) return;
        var link = card.querySelector('[data-card-link]');
        if (link) link.click();
    });
})();
