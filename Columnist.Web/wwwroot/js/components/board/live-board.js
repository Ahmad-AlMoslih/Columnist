(function () {
    if (!window.signalR || !window.htmx) return;

    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/board')
        .withAutomaticReconnect()
        .build();

    // Tell the server which connection we are, so our own changes aren't sent back to us.
    document.addEventListener('htmx:configRequest', function (evt) {
        if (connection.connectionId)
            evt.detail.headers['X-Board-Connection'] = connection.connectionId;
    });

    // One live refresh at a time: board first, then one card, then comments, then the open task's checklists, then the task view.
    var busy = false;
    var pendingBoard = false;
    var pendingCard = null;           // the stable key of the one card to re-render (my own checklist change)
    var pendingComments = false;
    var pendingChecklists = false;
    var pendingTask = false;
    var deferredChecklists = false;   // skipped while typing in the checklists region; runs once focus leaves it

    function run() {
        while (!busy && (pendingBoard || pendingCard || pendingComments || pendingChecklists || pendingTask)) {
            var next = null;
            if (pendingBoard) {
                pendingBoard = false;
                pendingCard = null;   // the board re-render has every card
                next = refreshBoard();
            } else if (pendingCard) {
                next = refreshCard(pendingCard);
                pendingCard = null;
            } else if (pendingComments) {
                pendingComments = false;
                next = refreshComments();
            } else if (pendingChecklists) {
                pendingChecklists = false;
                next = refreshChecklists();
            } else if (pendingTask) {
                pendingTask = false;
                next = refreshTask();
            }
            if (next) {
                busy = true;
                next.finally(function () {
                    busy = false;
                    run();
                });
            }
        }
    }

    function refreshBoard() {
        var region = document.getElementById('kb-board-region');
        // The columns carry the URL with the shown search and filter; it is re-rendered with every swap.
        var columns = document.querySelector('#kb-board-columns[data-refresh-url]');
        if (!region || !columns) return refreshReport();
        if (document.body.hasAttribute('data-dragging')) return null;
        // Source = the region, so its hx-ext="morph" applies: the board is morphed in place, not replaced.
        // The open filter panel is left alone (data-morph-skip, board-filter.js).
        return htmx.ajax('GET', columns.dataset.refreshUrl, {
            source: region,
            target: '#kb-board-region',
            select: '#kb-board-region',
            swap: 'morph:outerHTML',
            headers: { 'X-Live-Refresh': '1' }
        });
    }

    // One board card, swapped in from the board's Card URL (the same re-render as board-dnd.js after a drag).
    // A 204 (task gone or no access) leaves it as it is.
    function refreshCard(key) {
        var form = document.getElementById('kb-move-form');
        var card = document.getElementById('kb-card-' + key);
        if (!form || !card || document.body.hasAttribute('data-dragging')) return null;
        var url = new URL(form.dataset.cardUrl, window.location.href);
        url.searchParams.set('token', card.dataset.cardToken);
        return htmx.ajax('GET', url.pathname + url.search, { source: card, target: card, select: '.kb-card', swap: 'outerHTML' });
    }

    // The Dashboard / Report region, morphed in place from its URL (it carries the shown filter).
    function refreshReport() {
        var report = document.querySelector('#kb-report-region[data-refresh-url]');
        if (!report) return null;
        return htmx.ajax('GET', report.dataset.refreshUrl, {
            source: report,
            target: '#kb-report-region',
            select: '#kb-report-region',
            swap: 'morph:outerHTML',
            headers: { 'X-Live-Refresh': '1' }
        });
    }

    function refreshComments() {
        var comments = document.querySelector('#kb-comments[data-refresh-url]');
        if (!comments || !document.getElementById('kb-comment-feed')) return null;
        if (comments.querySelector('[name="commentToken"]')) return null;
        return htmx.ajax('GET', comments.dataset.refreshUrl, {
            target: '#kb-comment-feed',
            select: '#kb-comment-feed',
            swap: 'outerHTML',
            headers: { 'X-Live-Refresh': '1' }
        });
    }

    // Source = the region, so its hx-ext="morph" applies. An open edit box keeps its text: wait until it loses focus.
    function refreshChecklists() {
        var region = document.querySelector('#kb-task-checklists[data-task-checklists-url]');
        if (!region) return null;
        var active = document.activeElement;
        if (active && active.tagName === 'INPUT' && region.contains(active)) {
            deferredChecklists = true;
            return null;
        }
        return htmx.ajax('GET', region.dataset.taskChecklistsUrl, {
            source: region,
            target: '#kb-task-checklists',
            select: '#kb-task-checklists',
            swap: 'morph:outerHTML',
            headers: { 'X-Live-Refresh': '1' }
        });
    }

    document.addEventListener('focusout', function (evt) {
        if (!deferredChecklists || !evt.target.closest || !evt.target.closest('#kb-task-checklists')) return;
        // focusout fires before focus moves: check where it went on the next tick.
        setTimeout(function () {
            var region = document.getElementById('kb-task-checklists');
            var active = document.activeElement;
            if (region && active && active.tagName === 'INPUT' && region.contains(active)) return;
            deferredChecklists = false;
            pendingChecklists = true;
            run();
        }, 0);
    });

    // The open task view, morphed in (fields, history, RowVersion): only what changed is updated. A field being edited
    // is left alone and catches up when it closes (data-morph-skip, task-inline.js); the checklists and the comments
    // keep their own refreshes.
    function refreshTask() {
        var view = document.querySelector('#kb-task-view[data-task-view-url]');
        if (!view) return null;
        return htmx.ajax('GET', view.dataset.taskViewUrl, {
            source: view,
            target: '#kb-task-view',
            select: '#kb-task-view',
            swap: 'morph:outerHTML',
            headers: { 'X-Live-Refresh': '1' }
        });
    }

    connection.on('BoardChanged', function () {
        pendingBoard = true;
        pendingChecklists = true;
        pendingTask = true;
        run();
    });
    connection.on('CommentsChanged', function () {
        pendingBoard = true;
        pendingComments = true;
        pendingTask = true;
        run();
    });

    // My own changes aren't sent back to me. A field save on the board brings its card out of band (FieldSavedAsync):
    // the whole board behind the popup is refreshed only for a column change or a new task (data-board-refresh on the form)
    // or when the board is filtered. After a comment or checklist change refresh the task view too, for its history and
    // the new RowVersion; a checklist change also re-renders the task's card (its checklist badge).
    document.addEventListener('htmx:afterSettle', function (evt) {
        var config = evt.detail.requestConfig;
        if (!config || config.verb !== 'post') return;
        var settled = evt.detail.elt;
        if (!settled) return;
        // On the Dashboard and the Report every change made from the popup (a delete included) refreshes the page.
        var onReport = !!document.getElementById('kb-report-region');
        if (settled.id === 'kb-task-view') {
            var filters = document.getElementById('kb-active-filters');
            var filtered = !!filters && !filters.hidden;
            var source = config.elt;
            if (onReport || filtered || (source && source.hasAttribute && source.hasAttribute('data-board-refresh'))) {
                pendingBoard = true;
                run();
            }
            return;
        }
        if (onReport && settled.id === 'kb-toasts') {
            pendingBoard = true;
            run();
            return;
        }
        var comments = settled.id === 'kb-comments' || !!(settled.querySelector && settled.querySelector('#kb-comments'));
        var checklistsRegion = settled.id === 'kb-task-checklists' ? settled : (settled.querySelector && settled.querySelector('#kb-task-checklists'));
        if (!comments && !checklistsRegion) return;
        if (onReport) pendingBoard = true;
        else if (checklistsRegion && checklistsRegion.dataset.clTask) pendingCard = checklistsRegion.dataset.clTask;
        pendingTask = true;
        run();
    });

    // The Super Admin changed my access: one popup lists every change; "Got it" reloads the page.
    connection.on('AccessChanged', function (message) {
        var modalEl = document.getElementById('kb-access-modal');
        if (!modalEl) {
            window.location.reload();
            return;
        }
        var item = document.createElement('li');
        item.textContent = message;
        modalEl.querySelector('[data-access-list]').appendChild(item);

        // Close any other open popup (e.g. a task) so the two don't stack.
        document.querySelectorAll('.modal.show').forEach(function (other) {
            if (other !== modalEl) bootstrap.Modal.getOrCreateInstance(other).hide();
        });
        bootstrap.Modal.getOrCreateInstance(modalEl).show();   // already open: it stays open and the list grows
    });

    document.addEventListener('click', function (evt) {
        if (evt.target.closest && evt.target.closest('[data-access-ok]'))
            window.location.reload();
    });

    // Only viewers of a board get its changes: join the shown board's group, or leave when no board is shown.
    // joined = the token the server last heard for this connection ('' = left); null = unknown (new connection).
    var joined = null;

    function syncBoard() {
        if (connection.state !== signalR.HubConnectionState.Connected) return;
        // The board page's region first; otherwise any page root that names its board (e.g. the full-page task view).
        var root = document.querySelector('#kb-board-region[data-board-token]') || document.querySelector('[data-board-token]');
        var token = (root && root.dataset.boardToken) || '';
        if (token === joined) return;
        joined = token;
        var call = token ? connection.invoke('JoinBoard', token) : connection.invoke('LeaveBoard');
        call.catch(function () { joined = null; });
    }

    // hx-boost navigation, board tab switches and other swaps.
    document.addEventListener('htmx:afterSettle', syncBoard);
    connection.onreconnected(function () {
        joined = null;
        syncBoard();
    });

    // Live updates are optional: if the connection can't start or drops for good, retry every 30 s.
    function start() {
        connection.start()
            .then(function () {
                joined = null;
                syncBoard();
            })
            .catch(function () { setTimeout(start, 30000); });
    }
    connection.onclose(function () { setTimeout(start, 30000); });
    start();
})();
