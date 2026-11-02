(function () {
    var modalEl = document.getElementById('kb-task-modal');
    if (!modalEl) return;
    var bodyEl = document.getElementById('kb-task-modal-body');

    document.addEventListener('htmx:afterSwap', function (evt) {
        if (evt.target === bodyEl) {
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
        } else if (evt.target.id === 'kb-board-region' && modalEl.classList.contains('show')) {
            bootstrap.Modal.getOrCreateInstance(modalEl).hide();
            bodyEl.innerHTML = '';
        }
    });

    modalEl.addEventListener('hidden.bs.modal', function () { bodyEl.innerHTML = ''; });

    // Client validation for forms loaded into the modal.
    htmx.onLoad(function (elt) {
        if (bodyEl.contains(elt) && window.jQuery && $.validator && $.validator.unobtrusive)
            $.validator.unobtrusive.parse(elt);
    });

    // htmx submits on its own listener, so stop the request when jquery.validate finds errors.
    document.addEventListener('htmx:beforeRequest', function (evt) {
        var form = evt.detail.elt;
        if (form.tagName !== 'FORM' || !bodyEl.contains(form) || !window.jQuery || !$.fn.valid) return;
        if (!$(form).valid()) evt.preventDefault();
    });
})();
