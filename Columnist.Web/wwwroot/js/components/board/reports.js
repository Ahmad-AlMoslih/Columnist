// Report: "View all N" / "Show less" on the cards, "Clear filters", and the Export CSV link that follows the filter.
// A card has only its first 5 rows; "View all" loads the full list (the region's data-card-url, Reports/Card) and
// "Show less" drops the extra rows. Event delegation only. A refresh (filter change or live update) morphs the region
// back to 5 rows, so the open cards are remembered and their full lists loaded again.
(function () {
    var open = new Set();      // data-report-card values of the expanded cards
    var loading = new Set();   // the cards whose full list is on its way

    // The card's full list, unless it is there or on its way.
    function loadAll(card) {
        var key = card.dataset.reportCard;
        var list = card.querySelector('[data-report-rows]');
        var region = document.getElementById('kb-report-region');
        if (!list || list.dataset.reportRows === 'all' || !region || loading.has(key)) return;
        loading.add(key);
        htmx.ajax('GET', region.dataset.cardUrl + '&card=' + encodeURIComponent(key), { source: card, target: list, swap: 'outerHTML' })
            .finally(function () { loading.delete(key); });
    }

    // Back to the first 5 rows.
    function showFirst(card) {
        var list = card.querySelector('[data-report-rows="all"]');
        if (!list) return;
        list.querySelectorAll(':scope > .rp-row-more').forEach(function (row) { row.remove(); });
        list.dataset.reportRows = 'first';
    }

    function setOpen(card, isOpen) {
        card.classList.toggle('is-open', isOpen);
        fitRows(card, isOpen);
        var button = card.querySelector('[data-report-more]');
        if (!button) return;
        button.textContent = isOpen ? 'Show less' : button.dataset.moreText;
        button.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
    }

    // An open card keeps its size: its list scrolls at the height of the first rows (those without rp-row-more), like
    // a board column. Rows differ in height, so it is measured up to the end of the last of those rows. Closing it
    // scrolls the list back to the top.
    function fitRows(card, isOpen) {
        var list = card.querySelector('.rp-rows');
        if (!list) return;
        var shown = list.querySelectorAll(':scope > .rp-row:not(.rp-row-more)');
        var scroll = isOpen && shown.length > 0 && shown.length < list.children.length;
        var scrolled = list.scrollTop;   // kept through a refresh or a resize while open
        list.classList.toggle('rp-rows-scroll', scroll);
        list.style.maxHeight = '';
        if (!scroll) {
            list.scrollTop = 0;
            return;
        }
        var last = shown[shown.length - 1];
        list.style.maxHeight = (last.getBoundingClientRect().bottom - list.getBoundingClientRect().top) + 'px';
        list.scrollTop = scrolled;
    }

    // A new card width can change the rows' heights.
    window.addEventListener('resize', function () {
        document.querySelectorAll('[data-report-card].is-open').forEach(function (card) { fitRows(card, true); });
    });

    document.addEventListener('click', function (evt) {
        var more = evt.target.closest && evt.target.closest('[data-report-more]');
        if (more) {
            var card = more.closest('[data-report-card]');
            var key = card.dataset.reportCard;
            if (open.has(key)) open.delete(key);
            else open.add(key);
            setOpen(card, open.has(key));
            if (open.has(key)) loadAll(card);
            else showFirst(card);
            return;
        }

        var clear = evt.target.closest && evt.target.closest('[data-report-clear]');
        if (clear) {
            var form = clear.closest('[data-report-filters]');
            form.querySelectorAll('input[type="search"], input[type="date"]').forEach(function (input) { input.value = ''; });
            form.querySelectorAll('select').forEach(function (select) { select.value = ''; });
            form.querySelectorAll('input[type="checkbox"]').forEach(function (box) { box.checked = false; });
            htmx.trigger(form, 'change');
        }
    });

    // A new page (side menu navigation) starts with every card closed.
    document.addEventListener('htmx:beforeSwap', function (evt) {
        if (evt.detail.target && evt.detail.target.id === 'kb-page') open.clear();
    });

    document.addEventListener('htmx:afterSettle', function () {
        var region = document.getElementById('kb-report-region');
        if (!region) return;
        region.querySelectorAll('[data-report-card]').forEach(function (card) {
            var isOpen = open.has(card.dataset.reportCard);
            if (isOpen) loadAll(card);
            else showFirst(card);
            setOpen(card, isOpen);
        });
        var exportLink = document.querySelector('[data-report-export]');
        if (exportLink && region.dataset.exportUrl) exportLink.setAttribute('href', region.dataset.exportUrl);
    });
})();
