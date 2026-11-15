// Task form assignee picker: chips + checkbox dropdown with search. Selected people are posted
// as one hidden AssigneeTokens input per chip. Closes on outside click or Escape.
(function () {
    // "Sara Hassan" → "Sara H." (same rule as TaskCardHelpers.ShortName).
    function shortName(fullName) {
        var words = fullName.split(' ').filter(Boolean);
        if (words.length === 0) return '';
        return words.length > 1 ? words[0] + ' ' + words[words.length - 1][0].toUpperCase() + '.' : words[0];
    }

    function setOpen(picker, open) {
        var menu = picker.querySelector('[data-assignee-menu]');
        var field = picker.querySelector('[data-assignee-field]');
        menu.hidden = !open;
        field.setAttribute('aria-expanded', open ? 'true' : 'false');
        if (open) picker.querySelector('[data-assignee-search]').focus();
    }

    function closeAll(except) {
        var closed = false;
        document.querySelectorAll('[data-assignee-picker]').forEach(function (picker) {
            if (picker === except || picker.querySelector('[data-assignee-menu]').hidden) return;
            setOpen(picker, false);
            closed = true;
        });
        return closed;
    }

    function updatePlaceholder(picker) {
        var hasChips = picker.querySelector('[data-assignee-chip]') !== null;
        picker.querySelector('[data-assignee-placeholder]').textContent = hasChips ? 'Add people…' : 'Unassigned';
    }

    function findByToken(picker, selector, token) {
        var match = null;
        picker.querySelectorAll(selector).forEach(function (el) {
            if (el.getAttribute('data-token') === token) match = el;
        });
        return match;
    }

    function addChip(picker, option) {
        var token = option.getAttribute('data-token');
        var name = option.getAttribute('data-name');
        if (findByToken(picker, '[data-assignee-chip]', token)) return;

        var chip = document.createElement('span');
        chip.className = 'kb-ap-chip';
        chip.setAttribute('data-assignee-chip', '');
        chip.setAttribute('data-token', token);

        chip.appendChild(option.querySelector('[data-assignee-avatar]').cloneNode(true));

        var label = document.createElement('span');
        label.className = 'kb-ap-chip-name';
        label.textContent = shortName(name);
        chip.appendChild(label);

        if (option.hasAttribute('data-no-access')) {
            var tag = document.createElement('span');
            tag.className = 'kb-noaccess-tag';
            tag.textContent = 'No access';
            chip.appendChild(tag);
        }

        var remove = document.createElement('button');
        remove.type = 'button';
        remove.className = 'kb-ap-chip-x';
        remove.setAttribute('aria-label', 'Remove ' + name);
        remove.setAttribute('data-assignee-remove', '');
        remove.textContent = '×';
        chip.appendChild(remove);

        var input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'AssigneeTokens';
        input.value = token;
        chip.appendChild(input);

        picker.querySelector('[data-assignee-field]').insertBefore(chip, picker.querySelector('[data-assignee-placeholder]'));
    }

    document.addEventListener('click', function (e) {
        var remove = e.target.closest('[data-assignee-remove]');
        if (remove) {
            var picker = remove.closest('[data-assignee-picker]');
            var chip = remove.closest('[data-assignee-chip]');
            var option = findByToken(picker, '[data-assignee-option]', chip.getAttribute('data-token'));
            if (option) option.querySelector('input[type="checkbox"]').checked = false;
            chip.remove();
            updatePlaceholder(picker);
            return;
        }

        var field = e.target.closest('[data-assignee-field]');
        if (field) {
            var fieldPicker = field.closest('[data-assignee-picker]');
            closeAll(fieldPicker);
            setOpen(fieldPicker, fieldPicker.querySelector('[data-assignee-menu]').hidden);
            return;
        }

        closeAll(e.target.closest('[data-assignee-picker]'));
    });

    document.addEventListener('change', function (e) {
        var option = e.target.closest('[data-assignee-option]');
        if (!option) return;
        var picker = option.closest('[data-assignee-picker]');
        if (e.target.checked) {
            addChip(picker, option);
        } else {
            var chip = findByToken(picker, '[data-assignee-chip]', option.getAttribute('data-token'));
            if (chip) chip.remove();
        }
        updatePlaceholder(picker);
    });

    document.addEventListener('input', function (e) {
        if (!e.target.matches('[data-assignee-search]')) return;
        var term = e.target.value.trim().toLowerCase();
        e.target.closest('[data-assignee-picker]').querySelectorAll('[data-assignee-option]').forEach(function (option) {
            option.parentElement.hidden = term !== '' && option.getAttribute('data-name').toLowerCase().indexOf(term) === -1;
        });
    });

    // Capture phase so Escape closes the picker before the task modal sees it.
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' && e.target.matches('[data-assignee-search]')) {
            e.preventDefault();   // don't submit the task form from the search box
            return;
        }
        if (e.key === 'Escape' && closeAll(null)) e.stopPropagation();
    }, true);
})();
