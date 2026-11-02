(function () {
    function onEnd(evt) {
        if (evt.from === evt.to) return;
        var form = document.getElementById('kb-move-form');
        if (!form) return;
        form.elements['token'].value = evt.item.dataset.taskToken;
        form.elements['columnToken'].value = evt.to.dataset.columnToken;
        htmx.trigger(form, 'submit');
    }

    function init(root) {
        root.querySelectorAll('.kb-column-body:not([data-dnd])').forEach(function (el) {
            el.setAttribute('data-dnd', 'ready');
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
                onEnd: onEnd
            });
        });
    }

    init(document);
    htmx.onLoad(init);
})();
