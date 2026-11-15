// Checklists in the task view: inline forms for "Add checklist", renaming a checklist, editing an item and "Add an item".
// Each form sits in a [data-cl-inline] box next to the [data-cl-open] element that opens it.
// Enter submits, Escape (or Cancel) closes. Event delegation only; the region is swapped by HTMX after every change.
(function () {
    function inputOf(box) {
        return box.querySelector('[data-cl-input]');
    }

    function focusInput(box) {
        var input = inputOf(box);
        if (!input) return;
        input.focus();
        input.select();
    }

    function close(box) {
        var form = box.querySelector('[data-cl-form]');
        var input = inputOf(box);
        if (input) input.value = input.defaultValue;
        if (box.hasAttribute('data-cl-new')) {
            box.hidden = true;
        } else if (form) {
            form.hidden = true;
        }
        var opener = box.querySelector('[data-cl-open]');
        if (opener) opener.hidden = false;
    }

    // One inline form open at a time.
    function closeOthers(region, keep) {
        region.querySelectorAll('[data-cl-inline]').forEach(function (box) {
            if (box === keep) return;
            var form = box.querySelector('[data-cl-form]');
            var open = box.hasAttribute('data-cl-new') ? !box.hidden : (form && !form.hidden);
            if (open) close(box);
        });
    }

    function open(box) {
        var region = box.closest('#kb-task-checklists');
        if (region) closeOthers(region, box);
        var form = box.querySelector('[data-cl-form]');
        if (form) form.hidden = false;
        box.hidden = false;
        var opener = box.querySelector('[data-cl-open]');
        if (opener) opener.hidden = true;
        focusInput(box);
    }

    document.addEventListener('click', function (evt) {
        if (!evt.target.closest) return;

        // "Add checklist" in the side actions opens the form at the end of the checklists.
        if (evt.target.closest('[data-cl-new-open]')) {
            var newBox = document.querySelector('#kb-task-checklists [data-cl-new]');
            if (newBox) open(newBox);
            return;
        }

        var opener = evt.target.closest('[data-cl-open]');
        if (opener) {
            var box = opener.closest('[data-cl-inline]');
            if (box) open(box);
            return;
        }

        var cancel = evt.target.closest('[data-cl-cancel]');
        if (cancel) {
            var cancelBox = cancel.closest('[data-cl-inline]');
            if (cancelBox) close(cancelBox);
        }
    });

    // Capture phase, so Escape closes the inline form and not the task popup around it.
    document.addEventListener('keydown', function (evt) {
        var input = evt.target.closest && evt.target.closest('[data-cl-input]');
        if (!input) return;
        var box = input.closest('[data-cl-inline]');
        if (!box) return;

        if (evt.key === 'Escape') {
            evt.preventDefault();
            evt.stopPropagation();
            close(box);
        } else if (evt.key === 'Enter') {
            evt.preventDefault();
            if (!input.value.trim()) return;
            var form = input.closest('form');
            if (form) form.requestSubmit();
        }
    }, true);

    // After a change, an inline form the server rendered open (a new checklist, or "Add an item" kept open) gets the focus.
    document.addEventListener('htmx:afterSettle', function (evt) {
        var config = evt.detail.requestConfig;
        if (!config || config.verb !== 'post') return;
        var settled = evt.detail.elt;
        if (!settled || settled.id !== 'kb-task-checklists') return;
        var form = settled.querySelector('[data-cl-autofocus]');
        if (!form) return;
        var box = form.closest('[data-cl-inline]');
        if (box) focusInput(box);
    });
})();
