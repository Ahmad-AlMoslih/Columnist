(function () {
    // Where the card was picked up, so a cancelled drag can put it back.
    var origin = null;
    var cancelled = false;

    function onStart(evt) {
        document.body.setAttribute('data-dragging', 'true');
        origin = { list: evt.from, next: evt.item.nextElementSibling };
        cancelled = false;
    }

    // While dragging: hide the dashed placeholder when the pointer isn't over any column.
    document.addEventListener('pointermove', function (e) {
        if (!origin) return;
        var under = document.elementFromPoint(e.clientX, e.clientY);
        if (under && under.closest('.kb-column-body')) {
            document.body.removeAttribute('data-drag-outside');
        } else {
            document.body.setAttribute('data-drag-outside', 'true');
        }
    });

    // Esc while dragging: mark it cancelled and end the drag now.
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Escape' || !origin) return;
        cancelled = true;
        document.dispatchEvent(new PointerEvent('pointerup', { bubbles: true }));
        document.dispatchEvent(new MouseEvent('mouseup', { bubbles: true }));
    });

    // Was the card let go over a column?
    function droppedOnColumn(evt) {
        var e = evt.originalEvent;
        if (!e || typeof e.clientX !== 'number') return true;
        var under = document.elementFromPoint(e.clientX, e.clientY);
        return !!(under && under.closest('.kb-column-body'));
    }

    function putBack(evt, from) {
        from = from || origin;
        if (from.next && from.next.parentNode === from.list) {
            from.list.insertBefore(evt.item, from.next);
        } else {
            from.list.appendChild(evt.item);
        }
    }

    // The header count and the "No tasks" text of a column body, after a card left or joined it.
    // Drag is off while the board is filtered, so the count is the plain number of cards.
    function refreshColumn(body) {
        var cards = body.querySelectorAll('[data-card-token]').length;
        var count = body.closest('.kb-column').querySelector('.kb-column-count');
        if (count) count.textContent = cards;
        var empty = body.querySelector('[data-column-empty]');
        if (cards > 0 && empty) {
            empty.remove();
        } else if (cards === 0 && !empty) {
            empty = document.createElement('div');
            empty.className = 'kb-empty';
            empty.setAttribute('data-column-empty', '');
            empty.textContent = 'No tasks';
            body.appendChild(empty);
        }
    }

    // Swaps in the card's new render (only that card, not the board). A 204 (task gone or no access) leaves it as it is.
    // The card is looked up by its token: an earlier re-render may have replaced the dragged element.
    function refreshCard(token) {
        var form = document.getElementById('kb-move-form');
        var item = document.querySelector('.kb-card[data-card-token="' + CSS.escape(token) + '"]');
        if (!form || !item) return;
        var url = new URL(form.dataset.cardUrl, window.location.href);
        url.searchParams.set('token', token);
        htmx.ajax('GET', url.pathname + url.search, { source: item, target: item, select: '.kb-card', swap: 'outerHTML' });
    }

    // Moves sent and not answered yet, oldest first (the form queues them): each with where its card came from.
    var pending = [];

    function onEnd(evt) {
        document.body.removeAttribute('data-dragging');
        document.body.removeAttribute('data-drag-outside');

        if (cancelled || !droppedOnColumn(evt)) {
            putBack(evt);
            origin = null;
            cancelled = false;
            return;   // nothing is saved
        }
        var from = origin;
        origin = null;

        if (evt.from === evt.to && evt.oldDraggableIndex === evt.newDraggableIndex) return;
        var form = document.getElementById('kb-move-form');
        if (!form) return;

        // The card stays where it was dropped; only the two columns' counts and empty texts change.
        refreshColumn(evt.from);
        if (evt.to !== evt.from) refreshColumn(evt.to);

        // Into or out of a Done column: the card's due chip and "No access" tag change, so it is re-rendered once saved.
        var doneChanged = evt.from.hasAttribute('data-column-done') !== evt.to.hasAttribute('data-column-done');
        pending.push({ item: evt.item, token: evt.item.dataset.cardToken, origin: from, doneChanged: doneChanged });
        htmx.ajax('POST', form.getAttribute('action'), {
            source: form,
            target: '#kb-toasts',
            select: '#kb-toasts',
            swap: 'outerHTML',
            values: {
                token: evt.item.dataset.taskToken,
                columnToken: evt.to.dataset.columnToken,
                position: evt.newDraggableIndex + 1
            }
        });
    }

    // Success = HX-Reswap: none (only the "Task moved." toast, swapped out of band). A failure answers with the
    // error toast (swapped into #kb-toasts): the card goes back to where it was picked up.
    document.addEventListener('htmx:afterRequest', function (evt) {
        if (evt.detail.elt !== document.getElementById('kb-move-form') || pending.length === 0) return;
        var move = pending.shift();
        var xhr = evt.detail.xhr;
        if (evt.detail.successful && xhr && xhr.getResponseHeader('HX-Reswap') === 'none') {
            if (move.doneChanged) refreshCard(move.token);
            return;
        }
        var now = move.item.parentNode;
        putBack({ item: move.item }, move.origin);
        if (now) refreshColumn(now);
        if (move.origin.list !== now) refreshColumn(move.origin.list);
    });

    // A count one lower: "N" → "N-1", "X of Y" → "X-1 of Y-1" (filtered: the deleted card was shown and is gone from both).
    function lessOne(el) {
        if (!el) return;
        el.textContent = el.textContent.replace(/(\d+) of (\d+)|\d+/, function (match, shown, total) {
            if (shown === undefined) return Math.max(0, parseInt(match, 10) - 1);
            return Math.max(0, parseInt(shown, 10) - 1) + ' of ' + Math.max(0, parseInt(total, 10) - 1);
        });
    }

    // Tasks/Delete removes the card out of band (hx-swap-oob="delete"): its column's count and "No tasks" text, the board
    // subtitle and, when filtered, "Showing X of Y" go one lower. The column is taken before the swap; the swap is
    // synchronous, so on the next tick the card is either gone (deleted) or replaced by a new render (same id: not a delete).
    document.addEventListener('htmx:oobBeforeSwap', function (evt) {
        var card = evt.detail.target;
        if (!card || !card.matches || !card.matches('.kb-card') || !card.id) return;
        var body = card.closest('.kb-column-body');
        if (!body) return;
        setTimeout(function () {
            if (document.getElementById(card.id)) return;
            lessOne(body.closest('.kb-column').querySelector('.kb-column-count'));
            lessOne(document.getElementById('kb-board-subtitle'));
            var filters = document.getElementById('kb-active-filters');
            var filtered = !!filters && !filters.hidden;
            if (filtered) lessOne(filters.querySelector('.kb-active-filters-count'));
            if (!body.querySelector('[data-card-token]') && !body.querySelector('[data-column-empty]')) {
                var empty = document.createElement('div');
                empty.className = 'kb-empty';
                empty.setAttribute('data-column-empty', '');
                empty.textContent = filtered ? 'No matching tasks' : 'No tasks';
                body.appendChild(empty);
            }
        }, 0);
    });

    function init(root) {
        // Sortable.get: a live refresh morphs the columns in place, so they keep their Sortable.
        root.querySelectorAll('.kb-column-body[data-can-move="true"]').forEach(function (el) {
            if (Sortable.get(el)) return;
            new Sortable(el, {
                group: 'kb-board',
                animation: 150,
                delay: 150,
                delayOnTouchOnly: true,
                ghostClass: 'kb-drag-ghost',
                chosenClass: 'kb-drag-chosen',
                draggable: '[data-task-token]',
                filter: 'a, button, input',
                preventOnFilter: false,
                forceFallback: true,
                fallbackOnBody: true,
                forceAutoScrollFallback: true,
                scrollSensitivity: 80,
                scrollSpeed: 14,
                onStart: onStart,
                onEnd: onEnd
            });
        });
    }

    init(document);
    htmx.onLoad(init);

    // Keep scroll positions when the board is replaced (live refresh, board tab, new column).
    var savedScroll = null;

    document.addEventListener('htmx:beforeSwap', function (evt) {
        var region = document.getElementById('kb-board-region');
        if (!region || evt.detail.target !== region) return;
        savedScroll = { x: window.scrollX, y: window.scrollY, items: [] };
        region.querySelectorAll('*').forEach(function (el) {
            if (el.scrollTop === 0 && el.scrollLeft === 0) return;
            var cls = el.classList && el.classList[0];
            if (!cls) return;
            var index = Array.prototype.indexOf.call(region.querySelectorAll('.' + CSS.escape(cls)), el);
            savedScroll.items.push({ cls: cls, index: index, top: el.scrollTop, left: el.scrollLeft });
        });
    });

    document.addEventListener('htmx:afterSwap', function () {
        if (!savedScroll) return;
        var saved = savedScroll;
        savedScroll = null;
        var region = document.getElementById('kb-board-region');
        if (!region) return;
        saved.items.forEach(function (item) {
            var el = region.querySelectorAll('.' + CSS.escape(item.cls))[item.index];
            if (el) {
                el.scrollTop = item.top;
                el.scrollLeft = item.left;
            }
        });
        window.scrollTo(saved.x, saved.y);
    });
})();