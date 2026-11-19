// Task history in the View task panel: show/hide the timeline and reveal older entries.
// The open/closed and "Show older" state is kept on #kb-task-history (data-history-open / data-history-all)
// and carried into the new history while the task view is morphed (a save or a live refresh), so it never flickers.
(function () {
    function applyOpen(region, open) {
        var toggle = region.querySelector('[data-history-toggle]');
        var history = region.querySelector('[data-history]');
        if (!toggle || !history) return;
        history.hidden = !open;
        toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
        toggle.textContent = open ? 'Hide history' : 'View history';
    }

    function applyAll(region) {
        var list = region.querySelector('[data-history]');
        if (!list) return;
        list.querySelectorAll('[data-history-older]').forEach(function (item) { item.hidden = false; });
        var more = list.querySelector('[data-history-more]');
        if (more) more.remove();
    }

    document.addEventListener('click', function (e) {
        var toggle = e.target.closest('[data-history-toggle]');
        if (toggle) {
            var history = toggle.parentElement.querySelector('[data-history]');
            if (!history) return;
            var open = history.hidden;
            history.hidden = !open;
            toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
            toggle.textContent = open ? 'Hide history' : 'View history';
            var region = toggle.closest('#kb-task-history');
            if (region) region.dataset.historyOpen = open ? 'true' : 'false';
            return;
        }

        var more = e.target.closest('[data-history-more]');
        if (more) {
            var list = more.closest('[data-history]');
            if (!list) return;
            list.querySelectorAll('[data-history-older]').forEach(function (item) { item.hidden = false; });
            more.remove();
            var region = list.closest('#kb-task-history');
            if (region) region.dataset.historyAll = 'true';
        }
    });

    // The history is refreshed with the whole task view (#kb-task-view, morphed): the server renders it closed,
    // so the open state is put on the new version before the morph applies it.
    if (window.Idiomorph) {
        var beforeNodeMorphed = Idiomorph.defaults.callbacks.beforeNodeMorphed;
        Idiomorph.defaults.callbacks.beforeNodeMorphed = function (oldNode, newNode) {
            if (oldNode.nodeType === 1 && newNode.nodeType === 1 && oldNode.id === 'kb-task-history') {
                if (oldNode.dataset.historyOpen === 'true') {
                    newNode.dataset.historyOpen = 'true';
                    applyOpen(newNode, true);
                }
                if (oldNode.dataset.historyAll === 'true') {
                    newNode.dataset.historyAll = 'true';
                    applyAll(newNode);
                }
            }
            return beforeNodeMorphed(oldNode, newNode);
        };
    }
})();
