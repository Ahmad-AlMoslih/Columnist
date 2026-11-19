// Board filter (BoardFilter view component): the "Filter" button, its panel, the assignee dropdown and the active-filter chips.
// Every change reloads the board columns through the panel form (hx-trigger="kb-filter-apply"), which also sends the search box
// (its hx-include). The panel is never swapped or morphed, so its inputs hold the filter. Event delegation only.
(function () {
    // Live refresh morphs the board region: leave elements marked data-morph-skip (the open filter panel) as they are.
    if (window.Idiomorph) {
        var beforeNodeMorphed = Idiomorph.defaults.callbacks.beforeNodeMorphed;
        Idiomorph.defaults.callbacks.beforeNodeMorphed = function (oldNode, newNode) {
            if (oldNode.nodeType === 1 && oldNode.hasAttribute('data-morph-skip')) return false;
            return beforeNodeMorphed(oldNode, newNode);
        };
    }

    function rootOf() {
        return document.querySelector('[data-board-filter]');
    }

    function panelOf(root) {
        return root.querySelector('[data-filter-panel]');
    }

    function menuOf(root) {
        return root.querySelector('[data-filter-menu]');
    }

    function searchInput(root) {
        var include = panelOf(root).getAttribute('hx-include');
        var form = include && document.querySelector(include);
        return form && form.querySelector('[name="search"]');
    }

    function apply(root) {
        htmx.trigger(panelOf(root), 'kb-filter-apply');
    }

    function filterMembers(root, text) {
        var query = text.trim().toLowerCase();
        var any = false;
        root.querySelectorAll('[data-filter-option]').forEach(function (option) {
            var match = !query || option.dataset.name.toLowerCase().indexOf(query) !== -1;
            option.hidden = !match;
            if (match) any = true;
        });
        root.querySelector('[data-filter-nomatch]').hidden = any;
    }

    // The panel's checked people (assignees, unassigned) with the board, as a query string.
    function chosenQuery(root) {
        var panel = panelOf(root);
        var params = new URLSearchParams();
        panel.querySelectorAll('[data-filter-option] input:checked').forEach(function (input) {
            params.append(input.name, input.value);
        });
        var include = panel.getAttribute('hx-include');
        var form = include && document.querySelector(include);
        var board = form && form.querySelector('[name="boardToken"]');
        if (board) params.set('boardToken', board.value);
        return params.toString();
    }

    // The board renders only the chosen people; the first time the dropdown opens the full list replaces them
    // (Tasks/FilterMembers), keeping them checked. A choice made while it loads: it is loaded again with it.
    function loadMembers(root) {
        var list = root.querySelector('[data-filter-options]');
        if (!list || list.hasAttribute('data-filter-loaded') || list.dataset.loading) return;
        list.dataset.loading = '1';
        var sent = chosenQuery(root);
        fetch(list.dataset.membersUrl + '?' + sent)
            .then(function (response) { return response.ok ? response.text() : ''; })
            .catch(function () { return ''; })
            .then(function (html) {
                delete list.dataset.loading;
                if (!html || !list.isConnected) return;
                if (chosenQuery(root) !== sent) {
                    loadMembers(root);
                    return;
                }
                var parsed = document.createElement('template');
                parsed.innerHTML = html;
                var full = parsed.content.querySelector('[data-filter-options]');
                if (!full) return;
                list.replaceWith(full);
                var search = menuOf(root).querySelector('[data-search-client]');
                filterMembers(root, search ? search.value : '');
            });
    }

    function setMenu(root, open) {
        var menu = menuOf(root);
        menu.hidden = !open;
        root.querySelector('[data-filter-select-toggle]').setAttribute('aria-expanded', open ? 'true' : 'false');
        var search = menu.querySelector('[data-search-client]');
        if (open) {
            loadMembers(root);
            if (search) search.focus();
        } else {
            if (search) search.value = '';
            filterMembers(root, '');
        }
    }

    function setPanel(root, open) {
        panelOf(root).hidden = !open;
        root.querySelector('[data-filter-toggle]').setAttribute('aria-expanded', open ? 'true' : 'false');
        if (!open) setMenu(root, false);
    }

    // The dropdown text, the custom range fields and the badge, from the panel's inputs.
    function sync(root) {
        var panel = panelOf(root);

        var names = [];
        panel.querySelectorAll('[data-filter-option] input:checked').forEach(function (input) {
            names.push(input.dataset.filterLabel);
        });
        root.querySelector('[data-filter-select-text]').textContent = names.length === 0
            ? 'Anyone'
            : names.slice(0, 2).join(', ') + (names.length > 2 ? ' +' + (names.length - 2) : '');

        var due = panel.querySelector('input[name="due"]:checked');
        var dueValue = due ? due.value : '';
        var custom = dueValue === 'custom';
        var from = panel.querySelector('[data-filter-from]');
        var to = panel.querySelector('[data-filter-to]');
        panel.querySelector('[data-filter-range]').hidden = !custom;
        from.disabled = !custom;
        to.disabled = !custom;
        from.max = to.value;
        to.min = from.value;

        // Assignee, Due, Status, Checklist; the search isn't counted. A custom range counts once it has a date.
        var count = 0;
        if (names.length > 0) count++;
        if (dueValue && (!custom || from.value || to.value)) count++;
        if (panel.querySelector('input[name="status"]:checked')) count++;
        if (panel.querySelector('input[name="unfinished"]:checked')) count++;
        var badge = root.querySelector('[data-filter-badge]');
        badge.textContent = count;
        badge.hidden = count === 0;
    }

    function clearDue(panel) {
        var any = panel.querySelector('input[name="due"][value=""]');
        if (any) any.checked = true;
        panel.querySelector('[data-filter-from]').value = '';
        panel.querySelector('[data-filter-to]').value = '';
    }

    function clearAll(root) {
        var panel = panelOf(root);
        panel.querySelectorAll('input[type="checkbox"]').forEach(function (input) { input.checked = false; });
        clearDue(panel);
        var search = searchInput(root);
        if (search) search.value = '';
    }

    // A chip: clear only the value it shows.
    function remove(root, name, value) {
        var panel = panelOf(root);
        if (name === 'search') {
            var search = searchInput(root);
            if (search) search.value = '';
        } else if (name === 'due') {
            clearDue(panel);
        } else {
            panel.querySelectorAll('input[name="' + name + '"]').forEach(function (input) {
                if (value === undefined || input.value === value) input.checked = false;
            });
        }
    }

    document.addEventListener('click', function (evt) {
        if (!evt.target.closest) return;
        var root = rootOf();
        if (!root) return;
        // A click inside a popup (e.g. the task popup) is that popup's.
        if (evt.target.closest('.modal')) return;

        // A click outside closes the dropdown first, then the panel.
        if (!menuOf(root).hidden) {
            if (!evt.target.closest('[data-filter-select]')) setMenu(root, false);
        } else if (!panelOf(root).hidden && !root.contains(evt.target)) {
            setPanel(root, false);
        }

        if (evt.target.closest('[data-filter-toggle]')) {
            setPanel(root, panelOf(root).hidden);
            return;
        }
        if (evt.target.closest('[data-filter-select-toggle]')) {
            setMenu(root, menuOf(root).hidden);
            return;
        }
        if (evt.target.closest('[data-filter-close]')) {
            setPanel(root, false);
            return;
        }

        var clear = evt.target.closest('[data-filter-clear]');
        if (clear) {
            clearAll(root);
            sync(root);
            apply(root);
            return;
        }

        var chip = evt.target.closest('[data-filter-remove]');
        if (chip) {
            remove(root, chip.dataset.filterRemove, chip.dataset.filterValue);
            sync(root);
            apply(root);
        }
    });

    // Filters apply immediately.
    document.addEventListener('change', function (evt) {
        if (!evt.target.closest || !evt.target.closest('[data-filter-input]')) return;
        var root = evt.target.closest('[data-board-filter]');
        if (!root) return;
        sync(root);
        apply(root);
    });

    // The people search inside the dropdown filters the list in the browser.
    document.addEventListener('input', function (evt) {
        if (!evt.target.closest || !evt.target.matches('[data-search-client]')) return;
        var root = evt.target.closest('[data-board-filter]');
        if (root) filterMembers(root, evt.target.value);
    });

    // Enter (e.g. in a date field) would submit the panel as a normal page load: apply instead.
    document.addEventListener('submit', function (evt) {
        if (!evt.target.matches || !evt.target.matches('[data-filter-panel]')) return;
        evt.preventDefault();
        var root = evt.target.closest('[data-board-filter]');
        if (root) apply(root);
    });

    // While the search or a filter reloads the columns (not a live refresh), the real columns and headers stay and
    // each column's cards give way to two skeleton cards (skeleton.js) until the new columns swap in.
    document.addEventListener('htmx:beforeSend', function (evt) {
        var columns = evt.detail.target;
        var config = evt.detail.requestConfig;
        if (!columns || columns.id !== 'kb-board-columns' || !window.KbSkeleton) return;
        if (config && config.headers && config.headers['X-Live-Refresh']) return;

        KbSkeleton.whenSlow(evt, function () {
            if (!columns.isConnected) return null;
            var undo = [KbSkeleton.mark(columns, 'Loading tasks')];
            columns.querySelectorAll('[data-column-token]').forEach(function (body) {
                var cards = document.createDocumentFragment();
                [0, 1].forEach(function () {
                    var card = KbSkeleton.build('board-card');
                    if (card) cards.appendChild(card);
                });
                undo.push(KbSkeleton.fill(body, cards));
            });
            return function () {
                undo.forEach(function (fn) { fn(); });
            };
        });
    });

    // Escape closes the dropdown first, then the panel. With a popup (e.g. the task) open, Escape stays the popup's.
    document.addEventListener('keydown', function (evt) {
        if (evt.key === 'Enter' && evt.target.matches && evt.target.matches('[data-board-filter] [data-search-client]')) {
            evt.preventDefault();
            return;
        }
        if (evt.key !== 'Escape' || document.querySelector('.modal.show')) return;
        var root = rootOf();
        if (!root) return;
        if (!menuOf(root).hidden) {
            evt.preventDefault();
            setMenu(root, false);
            root.querySelector('[data-filter-select-toggle]').focus();
        } else if (!panelOf(root).hidden) {
            evt.preventDefault();
            setPanel(root, false);
            root.querySelector('[data-filter-toggle]').focus();
        }
    });
})();
