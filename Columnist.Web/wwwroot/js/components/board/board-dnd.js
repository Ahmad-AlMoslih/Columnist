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

    function putBack(evt) {
        if (origin.next && origin.next.parentNode === origin.list) {
            origin.list.insertBefore(evt.item, origin.next);
        } else {
            origin.list.appendChild(evt.item);
        }
    }

    function onEnd(evt) {
        document.body.removeAttribute('data-dragging');
        document.body.removeAttribute('data-drag-outside');

        if (cancelled || !droppedOnColumn(evt)) {
            putBack(evt);
            origin = null;
            cancelled = false;
            return;   // nothing is saved
        }
        origin = null;

        if (evt.from === evt.to && evt.oldDraggableIndex === evt.newDraggableIndex) return;
        var form = document.getElementById('kb-move-form');
        if (!form) return;
        form.elements['token'].value = evt.item.dataset.taskToken;
        form.elements['columnToken'].value = evt.to.dataset.columnToken;
        form.elements['position'].value = evt.newDraggableIndex + 1;
        htmx.trigger(form, 'submit');
    }

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
                filter: 'a, button, input, .dropdown-menu',
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

    // Keep scroll positions when the board is replaced (move, save, delete, live refresh).
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