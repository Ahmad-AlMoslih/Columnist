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

    // Open/closed state, remembered per task in this browser: { open: section open, lists: { checklist key: open } }.
    // The keys are the stable data-cl-task / data-cl-key values (encrypted tokens change on every render).
    var storePrefix = 'kb-checklists:';

    function readState(taskKey) {
        try {
            var state = JSON.parse(localStorage.getItem(storePrefix + taskKey));
            if (state && typeof state === 'object' && state.lists && typeof state.lists === 'object') return state;
        } catch (e) { }
        return null;
    }

    function writeState(region, state) {
        try {
            localStorage.setItem(storePrefix + region.dataset.clTask, JSON.stringify(state));
        } catch (e) { }
    }

    function isExpanded(toggle) {
        return !!toggle && toggle.getAttribute('aria-expanded') === 'true';
    }

    // The remembered state; nothing remembered = section collapsed, first checklist open.
    function storedState(region) {
        var stored = readState(region.dataset.clTask);
        var state = { open: !!stored && stored.open === true, lists: {} };
        region.querySelectorAll('[data-cl-key]').forEach(function (checklist, index) {
            state.lists[checklist.dataset.clKey] = stored ? stored.lists[checklist.dataset.clKey] === true : index === 0;
        });
        return state;
    }

    // The state as shown.
    function shownState(region) {
        var state = { open: isExpanded(region.querySelector('[data-cl-section-toggle]')), lists: {} };
        region.querySelectorAll('[data-cl-key]').forEach(function (checklist) {
            state.lists[checklist.dataset.clKey] = isExpanded(checklist.querySelector('[data-cl-toggle]'));
        });
        return state;
    }

    function apply(region, state) {
        var sectionToggle = region.querySelector('[data-cl-section-toggle]');
        if (sectionToggle) sectionToggle.setAttribute('aria-expanded', String(state.open));
        var list = region.querySelector('[data-cl-list]');
        if (list) list.hidden = !state.open;
        region.querySelectorAll('[data-cl-key]').forEach(function (checklist) {
            var open = state.lists[checklist.dataset.clKey] === true;
            var toggle = checklist.querySelector('[data-cl-toggle]');
            if (toggle) toggle.setAttribute('aria-expanded', String(open));
            checklist.querySelectorAll('[data-cl-collapsible]').forEach(function (el) { el.hidden = !open; });
        });
    }

    function save(region, state) {
        writeState(region, state);
        apply(region, state);
    }

    // Inline forms left open inside a part being collapsed are closed.
    function closeOpenBoxes(container) {
        container.querySelectorAll('[data-cl-inline]').forEach(function (box) {
            var form = box.querySelector('[data-cl-form]');
            if (form && !form.hidden && !box.hasAttribute('data-cl-new')) close(box);
        });
    }

    function toggleChecklist(checklist) {
        var region = checklist.closest('[data-cl-task]');
        if (!region) return;
        var state = shownState(region);
        var key = checklist.dataset.clKey;
        state.lists[key] = !state.lists[key];
        if (!state.lists[key]) closeOpenBoxes(checklist);
        save(region, state);
    }

    // After every swap, before the browser paints: the remembered state. A checklist the server marked
    // data-cl-force-open="true" (just added, or its "Add an item" box kept open) is opened, with the section.
    function restore() {
        document.querySelectorAll('[data-cl-task]').forEach(function (region) {
            var state = storedState(region);
            var forced = region.querySelectorAll('[data-cl-force-open="true"]');
            forced.forEach(function (checklist) {
                state.open = true;
                state.lists[checklist.dataset.clKey] = true;
                checklist.dataset.clForceOpen = 'false';
            });
            if (forced.length) writeState(region, state);
            apply(region, state);
        });
    }

    document.addEventListener('htmx:afterSwap', restore);
    document.addEventListener('htmx:load', restore);

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

        // "Add checklist" in the side actions opens the section and the form at the end of the checklists.
        if (evt.target.closest('[data-cl-new-open]')) {
            var newBox = document.querySelector('#kb-task-checklists [data-cl-new]');
            if (!newBox) return;
            var newRegion = newBox.closest('[data-cl-task]');
            if (newRegion) {
                var newState = shownState(newRegion);
                newState.open = true;
                save(newRegion, newState);
            }
            open(newBox);
            return;
        }

        var sectionToggle = evt.target.closest('[data-cl-section-toggle]');
        if (sectionToggle) {
            var sectionRegion = sectionToggle.closest('[data-cl-task]');
            if (!sectionRegion) return;
            var sectionState = shownState(sectionRegion);
            sectionState.open = !sectionState.open;
            if (!sectionState.open) {
                var list = sectionRegion.querySelector('[data-cl-list]');
                if (list) closeOpenBoxes(list);
            }
            save(sectionRegion, sectionState);
            return;
        }

        // A checklist's title row toggles it. Its title renames only while it is open; collapsed, it opens it.
        var head = evt.target.closest('[data-cl-head]');
        var checklist = head && head.closest('[data-cl-key]');
        if (checklist) {
            var collapsed = !isExpanded(checklist.querySelector('[data-cl-toggle]'));
            if (evt.target.closest('[data-cl-toggle]')
                || (collapsed && evt.target.closest('[data-cl-open]'))
                || !evt.target.closest('button, form, input, a')) {
                toggleChecklist(checklist);
                return;
            }
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

    document.addEventListener('htmx:afterSettle', function () {
        var active = document.activeElement;
        if (active && active !== document.body) return;
        var openModal = document.querySelector('.modal.show');
        if (openModal) openModal.focus({ preventScroll: true });
    });

})();
