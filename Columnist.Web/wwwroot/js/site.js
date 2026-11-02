// Live limit checks: show the error while the user is filling the form, not after submit.
(function () {
    function messageFor(input) {
        return document.querySelector('[data-valmsg-for="' + input.name + '"]');
    }

    function showError(input, text) {
        var span = messageFor(input);
        if (!span) return;
        span.textContent = text || '';
        span.classList.toggle('field-validation-error', !!text);
        span.classList.toggle('field-validation-valid', !text);
        input.classList.toggle('input-validation-error', !!text);
    }

    window.kbShowError = showError;

    function init(root) {
        // Text fields with [StringLength]: block typing past the limit and show a counter.
        root.querySelectorAll('input[data-val-length-max]:not([data-kb-counter]), textarea[data-val-length-max]:not([data-kb-counter])').forEach(function (input) {
            input.setAttribute('data-kb-counter', 'ready');
            var max = parseInt(input.getAttribute('data-val-length-max'), 10);
            input.setAttribute('maxlength', max);

            var counter = document.createElement('div');
            counter.className = 'kb-char-counter';
            input.insertAdjacentElement('afterend', counter);

            function update(clearError) {
                var length = input.value.length;
                counter.textContent = length + ' / ' + max;
                counter.classList.toggle('kb-char-counter-limit', length >= max);
                if (length >= max)
                    showError(input, input.getAttribute('data-val-length'));
                else if (clearError)
                    showError(input, '');
            }

            input.addEventListener('input', function () { update(true); });
            update(false);   // on page load, keep any error the server rendered (e.g. "Please enter a title.")
        });

        // Date inputs with a minimum: typed dates bypass the picker, so check on change.
        root.querySelectorAll('input[type="date"][min][data-min-message]:not([data-kb-min-check])').forEach(function (input) {
            input.setAttribute('data-kb-min-check', 'ready');
            input.addEventListener('change', function () {
                var tooEarly = input.value && input.value < input.min;
                showError(input, tooEarly ? input.dataset.minMessage : '');
            });
        });
    }

    init(document);
    htmx.onLoad(init);
})();
