// Board panning: drag the board's empty background (left button) or hold the
// middle button anywhere on it to scroll the columns left and right.
(function () {
    var NO_PAN = 'a, button, input, select, textarea, label, [contenteditable], [draggable="true"], [data-card-token], [data-task-token]';
    var MOVE_THRESHOLD = 4;
    var pan = null;          // { board, pointerId, startX, startScroll, moved }
    var suppressClick = false;

    // True when the pointer is on an element's own scrollbar rather than its content.
    function onScrollbar(el, e) {
        var rect = el.getBoundingClientRect();
        return e.clientX - rect.left - el.clientLeft >= el.clientWidth ||
               e.clientY - rect.top - el.clientTop >= el.clientHeight;
    }

    function end() {
        if (!pan) return;
        var board = pan.board;
        suppressClick = pan.moved;
        board.removeAttribute('data-panning');
        if (board.hasPointerCapture && board.hasPointerCapture(pan.pointerId)) board.releasePointerCapture(pan.pointerId);
        pan = null;
    }

    document.addEventListener('pointerdown', function (e) {
        if (pan || e.pointerType !== 'mouse') return;
        var board = e.target.closest('[data-board-pan]');
        if (!board) return;

        if (e.button === 1) {
            e.preventDefault();
        } else if (e.button !== 0 || e.target.closest(NO_PAN) || onScrollbar(e.target, e)) {
            return;
        }

        suppressClick = false;
        pan = { board: board, pointerId: e.pointerId, startX: e.clientX, startScroll: board.scrollLeft, moved: false };
        board.setPointerCapture(e.pointerId);
    });

    // Stops the browser's middle-button autoscroll icon.
    document.addEventListener('mousedown', function (e) {
        if (e.button === 1 && e.target.closest('[data-board-pan]')) e.preventDefault();
    });

    document.addEventListener('pointermove', function (e) {
        if (!pan || e.pointerId !== pan.pointerId) return;
        var dx = e.clientX - pan.startX;
        if (!pan.moved && Math.abs(dx) < MOVE_THRESHOLD) return;
        if (!pan.moved) {
            pan.moved = true;
            pan.board.setAttribute('data-panning', '');
        }
        e.preventDefault();
        pan.board.scrollLeft = pan.startScroll - dx;
    });

    document.addEventListener('pointerup', function (e) { if (pan && e.pointerId === pan.pointerId) end(); });
    document.addEventListener('pointercancel', function (e) { if (pan && e.pointerId === pan.pointerId) end(); });
    document.addEventListener('lostpointercapture', function (e) { if (pan && e.pointerId === pan.pointerId) end(); }, true);

    // The click (or middle-button auxclick) that follows a pan is not a real click.
    function swallow(e) {
        if (!suppressClick) return;
        suppressClick = false;
        e.preventDefault();
        e.stopPropagation();
    }
    document.addEventListener('click', swallow, true);
    document.addEventListener('auxclick', swallow, true);
})();
