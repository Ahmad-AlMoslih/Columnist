(function () {
    // Works for any Board settings form, including one swapped in by HTMX.
    function formOf(el) {
        return el.closest ? el.closest('[data-board-form]') : null;
    }

    // The row's dot takes the chosen status's colour; none = the neutral dot (site.css).
    function syncDot(row) {
        var select = row.querySelector('[data-column-status]');
        var option = select.options[select.selectedIndex];
        row.querySelector('[data-column-dot]').style.background = (option && option.dataset.statusColor) || '';
    }

    function addColumn(form) {
        var input = form.querySelector('[data-new-column-name]');
        var name = input.value.trim();
        if (!name) { input.focus(); return; }
        var row = form.querySelector('[data-column-template]').content.firstElementChild.cloneNode(true);
        row.querySelector('[data-field="Name"]').value = name;
        form.querySelector('[data-columns]').appendChild(row);
        syncDot(row);
        input.value = '';
        input.focus();
    }

    // Make the column list draggable once per form.
    htmx.onLoad(function (elt) {
        if (!window.Sortable || !elt.querySelectorAll) return;
        var lists = Array.prototype.slice.call(elt.querySelectorAll('[data-board-form] [data-columns]:not([data-sortable])'));
        if (elt.matches && elt.matches('[data-board-form] [data-columns]:not([data-sortable])')) lists.push(elt);
        lists.forEach(function (list) {
            list.setAttribute('data-sortable', 'ready');
            new Sortable(list, { handle: '[data-column-handle]', animation: 150, ghostClass: 'kb-drag-ghost' });
        });
    });

    document.addEventListener('change', function (evt) {
        if (formOf(evt.target) && evt.target.matches('[data-column-status]')) syncDot(evt.target.closest('[data-column]'));
    });

    document.addEventListener('click', function (evt) {
        var form = formOf(evt.target);
        if (!form) return;
        var del = evt.target.closest('[data-column-delete]');
        if (del) {
            if (del.disabled) return;
            // Ask first (shared confirm popup); the row goes only when "Delete" is pressed.
            var row = del.closest('[data-column]');
            var name = row.querySelector('[data-field="Name"]').value.trim();
            window.kbConfirm({
                confirmTitle: 'Delete column?',
                confirmText: (name ? '"' + name + '"' : 'This column') + ' will be removed when you save the board.',
                confirmYes: 'Delete'
            }, function () { row.remove(); });
            return;
        }
        if (evt.target.closest('[data-column-add]')) addColumn(form);
    });

    // Enter in the "New column name" box adds the column instead of submitting the form.
    document.addEventListener('keydown', function (evt) {
        var form = formOf(evt.target);
        if (form && evt.key === 'Enter' && evt.target.matches('[data-new-column-name]')) {
            evt.preventDefault();
            addColumn(form);
        }
    });

    // Name the fields Columns[0].Name, Columns[0].StatusId … in their current order so MVC binds the list.
    document.addEventListener('submit', function (evt) {
        if (!evt.target.matches || !evt.target.matches('[data-board-form]')) return;
        evt.target.querySelectorAll('[data-columns] [data-column]').forEach(function (row, index) {
            row.querySelectorAll('[data-field]').forEach(function (field) {
                field.name = 'Columns[' + index + '].' + field.dataset.field;
            });
        });
    });
})();
