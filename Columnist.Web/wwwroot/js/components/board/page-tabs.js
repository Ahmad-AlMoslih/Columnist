// Board · Dashboard · Report tabs (_BoardPageTabs, in #kb-page-tabs). A tab swaps only #kb-tab-page, pushing the URL;
// the tab row stays. The active tab is never kept here: it is always the page shown (#kb-tab-page's data-page), set
// after every swap (a tab, any link to the board / Dashboard / Report, a live refresh, the board's first-load shell) and
// on Back / Forward. A full page load renders it on the server. The new content fades in (CSS, kb-tab-fade).
// The board's columns carry its URL with the current search and filter (data-refresh-url): it is kept per board, and the
// Board tab goes back to it.
// Leaving the board for the Dashboard or the Report keeps the board's content (its own nodes, so its images stay loaded)
// in memory; the Board tab shows it at once and morphs the fresh board in from the background (only what changed
// updates). A full page load keeps nothing.
(function () {
    function keyOf(token) {
        return 'kb-board-url:' + token;
    }

    function remember() {
        var columns = document.querySelector('#kb-board-columns[data-refresh-url]');
        var tab = document.querySelector('[data-page-tab="board"][data-board-token]');
        if (!columns || !tab) return;
        try {
            sessionStorage.setItem(keyOf(tab.dataset.boardToken), columns.dataset.refreshUrl);
        } catch (e) { /* no storage: the Board tab opens the plain board */ }
    }

    remember();
    htmx.onLoad(remember);
    document.addEventListener('htmx:afterSettle', remember);

    function shownPage() {
        var page = document.querySelector('#kb-tab-page[data-page]');
        return page ? page.dataset.page : null;
    }

    // The active tab (and aria-current="page", on it only) = the page shown.
    function sync() {
        var key = shownPage();
        if (!key) return;
        document.querySelectorAll('#kb-page-tabs [data-page-tab]').forEach(function (tab) {
            var active = tab.dataset.pageTab === key;
            tab.classList.toggle('active', active);
            if (active) tab.setAttribute('aria-current', 'page');
            else tab.removeAttribute('aria-current');
        });
    }

    sync();
    htmx.onLoad(sync);
    document.addEventListener('htmx:afterSettle', sync);
    document.addEventListener('htmx:historyRestore', sync);
    window.addEventListener('popstate', sync);
    window.addEventListener('pageshow', sync);

    function tabOf(evt) {
        var elt = evt.detail && evt.detail.elt;
        return elt && elt.closest ? elt.closest('#kb-page-tabs [data-page-tab]') : null;
    }

    document.addEventListener('htmx:configRequest', function (evt) {
        var tab = evt.detail.elt && evt.detail.elt.closest && evt.detail.elt.closest('[data-page-tab="board"][data-board-token]');
        if (!tab) return;
        var url = null;
        try {
            url = sessionStorage.getItem(keyOf(tab.dataset.boardToken));
        } catch (e) { /* no storage */ }
        if (url) evt.detail.path = url;
    });

    // The kept board: { token, page (#kb-tab-page), topbar (its top bar actions), url, title, scrolls }.
    var kept = null;

    // Leaving the board with a tab: htmx swaps the new page into a placeholder, so the board's node is kept as it is.
    // Another page (the side menu) drops it.
    document.addEventListener('htmx:beforeSwap', function (evt) {
        var target = evt.detail.target;
        if (target && target.id === 'kb-page') {
            kept = null;
            return;
        }
        if (!evt.detail.shouldSwap || !tabOf(evt) || !target || target.id !== 'kb-tab-page' || shownPage() !== 'board') return;
        var region = target.querySelector('#kb-board-region[data-board-token]');
        if (!region) return;

        var topbar = document.getElementById('kb-topbar-actions');
        var scrolls = [];   // a node out of the page loses its scroll position: put back on return
        target.querySelectorAll('*').forEach(function (el) {
            if (el.scrollTop || el.scrollLeft) scrolls.push([el, el.scrollTop, el.scrollLeft]);
        });
        kept = {
            token: region.dataset.boardToken,
            page: target,
            topbar: topbar ? topbar.cloneNode(true) : null,
            url: window.location.href,
            title: document.title,
            scrolls: scrolls
        };
        var placeholder = document.createElement('div');
        target.replaceWith(placeholder);
        evt.detail.target = placeholder;
    });

    function fade(page) {
        page.classList.add('kb-tab-fade');
        page.addEventListener('animationend', function () { page.classList.remove('kb-tab-fade'); }, { once: true });
    }

    // The kept board back in place of the page shown, with its URL, title and top bar; then the fresh board is
    // morphed in from the background (as a live refresh: no skeleton).
    function restore() {
        var shown = document.getElementById('kb-tab-page');
        if (!shown) return false;
        var board = kept;
        kept = null;

        htmx.trigger(document.body, 'htmx:beforeHistoryUpdate', { history: { type: 'push', path: board.url } });
        shown.replaceWith(board.page);
        var topbar = document.getElementById('kb-topbar-actions');
        if (topbar && board.topbar) {
            topbar.replaceWith(board.topbar);
            htmx.process(board.topbar);
        }
        board.scrolls.forEach(function (s) {
            s[0].scrollTop = s[1];
            s[0].scrollLeft = s[2];
        });
        document.title = board.title;
        history.pushState({ htmx: true }, '', board.url);
        window.scrollTo(0, 0);
        sync();
        fade(board.page);

        var region = board.page.querySelector('#kb-board-region');
        var columns = board.page.querySelector('#kb-board-columns[data-refresh-url]');
        if (region && columns) {
            htmx.ajax('GET', columns.dataset.refreshUrl, {
                source: region,
                target: region,
                select: '#kb-board-region',
                swap: 'morph:outerHTML',
                headers: { 'X-Live-Refresh': '1' }
            });
        }
        return true;
    }

    // The tab of the page shown loads nothing; the Board tab with its board kept shows it instead of loading it.
    document.addEventListener('htmx:beforeRequest', function (evt) {
        var tab = tabOf(evt);
        if (!tab) return;
        if (tab.dataset.pageTab === shownPage()) {
            evt.preventDefault();
            return;
        }
        if (tab.dataset.pageTab === 'board' && kept && kept.token === tab.dataset.boardToken && restore())
            evt.preventDefault();
    });

    document.addEventListener('htmx:afterSwap', function (evt) {
        if (!tabOf(evt)) return;
        var page = document.getElementById('kb-tab-page');
        if (page) fade(page);
    });
})();
