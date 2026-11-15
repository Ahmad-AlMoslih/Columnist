(function () {
    var drag = null;

    function closest(evt, selector) {
        return evt.target.closest ? evt.target.closest(selector) : null;
    }

    function submit(formId, values) {
        var form = document.getElementById(formId);
        if (!form) return;
        Object.keys(values).forEach(function (key) {
            var input = form.querySelector('[data-ba-field="' + key + '"]');
            if (input) input.value = values[key];
            else console.warn('board-access: no field "' + key + '" in #' + formId);
        });
        htmx.trigger(form, 'submit');
    }

    // Removing someone asks first (shared confirm popup); the text comes from the row's Remove button.
    function removeMember(row) {
        var button = row.querySelector('[data-ba-remove-member]');
        window.kbConfirm(button.dataset, function () {
            submit('ba-remove-form', { member: row.dataset.baMember, name: row.dataset.baName });
        });
    }

    function has(row, permissionId) {
        return (row.dataset.baHas || '').split(',').indexOf(String(permissionId)) !== -1;
    }

    function clearHints() {
        document.querySelectorAll('[data-ba-over]').forEach(function (el) { el.removeAttribute('data-ba-over'); });
        document.querySelectorAll('[data-ba-remove]').forEach(function (el) { el.removeAttribute('data-ba-remove'); });
        document.querySelectorAll('[data-ba-row-hint]').forEach(function (el) { el.textContent = ''; });
    }

    document.addEventListener('dragstart', function (evt) {
        var source = closest(evt, '[data-ba-source]');
        if (!source) return;
        var row = source.closest('[data-ba-row]');
        drag = {
            kind: source.dataset.baSource,   // person | permission | chip | member
            memberId: source.dataset.baMember || (row && row.dataset.baMember),
            name: source.dataset.baName || (row && row.dataset.baName),
            row: row,
            permissionId: source.dataset.baPermission,
            label: source.dataset.baLabel,
            requires: source.dataset.baRequires,
            requiresLabel: source.dataset.baRequiresLabel
        };
        evt.dataTransfer.effectAllowed = 'copyMove';
        evt.dataTransfer.setData('text/plain', drag.label || drag.name || '');
    });

    document.addEventListener('dragover', function (evt) {
        if (!drag) return;
        clearHints();

        if (drag.kind === 'person') {
            var list = closest(evt, '[data-ba-members]');
            if (!list) return;
            list.setAttribute('data-ba-over', 'add');
            var hint = list.querySelector('[data-ba-add-hint]');
            if (hint) hint.textContent = 'Drop to add ' + drag.name + ' (view only to start)';
            evt.preventDefault();
            return;
        }

        if (drag.kind === 'permission') {
            var row = closest(evt, '[data-ba-row][data-ba-member]');
            if (!row) return;
            var already = has(row, drag.permissionId);
            var missing = !already && !!drag.requires && !has(row, drag.requires);
            row.setAttribute('data-ba-over', already || missing ? 'blocked' : 'grant');
            var rowHint = row.querySelector('[data-ba-row-hint]');
            if (rowHint) rowHint.textContent = already
                ? 'Already has "' + drag.label + '"'
                : missing
                    ? 'Needs "' + drag.requiresLabel + '" first'
                    : '← Drop to give "' + drag.label + '"';
            evt.preventDefault();
            evt.dataTransfer.dropEffect = already ? 'none' : 'copy';
            return;
        }

        // A chip or a member: drop anywhere on the panel to remove it.
        var panel = closest(evt, '[data-ba-panel]');
        if (!panel) return;
        panel.setAttribute('data-ba-remove', 'true');
        var banner = panel.querySelector('[data-ba-remove-banner]');
        if (banner) banner.textContent = drag.kind === 'chip'
            ? 'Release to remove "' + drag.label + '" from ' + drag.name
            : 'Release to remove ' + drag.name + ' from this board';
        evt.preventDefault();
        evt.dataTransfer.dropEffect = 'move';
    });

    document.addEventListener('drop', function (evt) {
        if (!drag) return;
        evt.preventDefault();
        var current = drag;

        if (current.kind === 'person') {
            if (closest(evt, '[data-ba-members]'))
                submit('ba-add-form', { member: current.memberId, name: current.name });
        } else if (current.kind === 'permission') {
            var row = closest(evt, '[data-ba-row][data-ba-member]');
            if (row && !has(row, current.permissionId))
                submit('ba-permission-form', { member: row.dataset.baMember, permission: current.permissionId, grant: 'true', name: row.dataset.baName, label: current.label });
        } else if (closest(evt, '[data-ba-panel]')) {
            if (current.kind === 'chip')
                submit('ba-permission-form', { member: current.memberId, permission: current.permissionId, grant: 'false', name: current.name, label: current.label });
            else if (current.kind === 'member')
                removeMember(current.row);
        }
    });

    document.addEventListener('dragend', function () {
        drag = null;
        clearHints();
    });

    document.addEventListener('click', function (evt) {
        var chipButton = closest(evt, '[data-ba-remove-chip]');
        if (chipButton) {
            var chip = chipButton.closest('[data-ba-source="chip"]');
            var chipRow = chipButton.closest('[data-ba-row]');
            submit('ba-permission-form', { member: chipRow.dataset.baMember, permission: chip.dataset.baPermission, grant: 'false', name: chipRow.dataset.baName, label: chip.dataset.baLabel });
            return;
        }
        var memberButton = closest(evt, '[data-ba-remove-member]');
        if (memberButton) {
            var memberRow = memberButton.closest('[data-ba-row]');
            submit('ba-remove-form', { member: memberRow.dataset.baMember, name: memberRow.dataset.baName });
        }
    });

    // Filter the "Not on this board" list as you type.
    document.addEventListener('input', function (evt) {
        if (!evt.target.matches || !evt.target.matches('[data-ba-filter]')) return;
        var text = evt.target.value.trim().toLowerCase();
        document.querySelectorAll('[data-ba-source="person"]').forEach(function (item) {
            item.hidden = text !== '' && item.dataset.baName.toLowerCase().indexOf(text) === -1;
        });
    });
})();
