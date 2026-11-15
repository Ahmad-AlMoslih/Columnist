// Task history in the View task panel: show/hide the timeline and reveal older entries.
// The open/closed and "Show older" state is kept on #kb-task-history (data-history-open / data-history-all)
// and re-applied after a live refresh replaces the timeline.
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

    // The swap resets the region's attributes to the server's, so hold the state across it.
    var saved = null;

    function save(target) {
        if (!target || target.id !== 'kb-task-history') return;
        saved = { open: target.dataset.historyOpen === 'true', all: target.dataset.historyAll === 'true' };
    }

    function restore() {
        if (!saved) return;
        var state = saved;
        saved = null;
        var region = document.getElementById('kb-task-history');
        if (!region) return;
        if (state.open) region.dataset.historyOpen = 'true';
        if (state.all) region.dataset.historyAll = 'true';
        applyOpen(region, state.open);
        if (state.all) applyAll(region);
    }

    document.addEventListener('htmx:beforeSwap', function (evt) { save(evt.detail.target); });
    document.addEventListener('htmx:afterSettle', restore);
})();
