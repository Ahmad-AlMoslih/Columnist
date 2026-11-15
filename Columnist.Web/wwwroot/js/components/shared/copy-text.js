(function () {
    // Any button with data-copy-text copies that text and briefly shows "Copied".
    document.addEventListener('click', function (evt) {
        var button = evt.target.closest && evt.target.closest('[data-copy-text]');
        if (!button || !navigator.clipboard) return;
        navigator.clipboard.writeText(button.dataset.copyText).then(function () {
            var label = button.querySelector('[data-copy-label]');
            if (!label) return;
            var original = label.textContent;
            label.textContent = 'Copied';
            setTimeout(function () { label.textContent = original; }, 2000);
        });
    });
})();
