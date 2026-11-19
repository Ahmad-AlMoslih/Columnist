// Editing in place in the task view (_TaskView, #kb-task-view).
// A field ([data-ti-field]) shows its text ([data-ti-display]); a click opens its editor ([data-ti-editor]).
// While an editor or a menu is open the field carries data-morph-skip, so no morph (a save's response or a
// live refresh) overwrites it; the newer version it skipped is kept and put in when the field closes unsaved.
// Every save is an htmx form whose response (the whole view) is morphed in: only what changed is updated.
(function () {
    // The checklists and the comments have their own requests: a morph of the whole view leaves them alone.
    var viewMorph = false;
    if (window.Idiomorph) {
        var beforeNodeMorphed = Idiomorph.defaults.callbacks.beforeNodeMorphed;
        Idiomorph.defaults.callbacks.beforeNodeMorphed = function (oldNode, newNode) {
            if (oldNode.nodeType === 1) {
                if (oldNode.hasAttribute('data-morph-skip')) {
                    if (oldNode.hasAttribute('data-ti-field')) oldNode._tiFresh = newNode.cloneNode(true);
                    return false;
                }
                if (viewMorph && (oldNode.id === 'kb-task-checklists' || oldNode.id === 'kb-comments')) return false;
            }
            return beforeNodeMorphed(oldNode, newNode);
        };
    }
    document.addEventListener('htmx:beforeSwap', function (evt) {
        if (evt.detail.target && evt.detail.target.id === 'kb-task-view') viewMorph = true;
    });
    document.addEventListener('htmx:afterSwap', function () { viewMorph = false; });

    function view() {
        return document.getElementById('kb-task-view');
    }

    function fieldOf(el) {
        return el && el.closest ? el.closest('#kb-task-view [data-ti-field]') : null;
    }

    function hold(field, on) {
        if (!field) return;
        if (on) field.setAttribute('data-morph-skip', '');
        else field.removeAttribute('data-morph-skip');
    }

    // A field closed without saving: put in the newer version a live refresh brought while it was held. No request.
    function catchUp(field) {
        var fresh = field && field._tiFresh;
        if (!fresh) return;
        field._tiFresh = null;
        Idiomorph.morph(field, fresh, { morphStyle: 'outerHTML' });
        htmx.process(field);
    }

    function focusInput(field, quiet) {
        var input = field.querySelector('[data-ti-editor] [data-ti-input]');
        if (!input) return;
        input.focus({ preventScroll: !!quiet });
        if (input.type === 'text' || input.tagName === 'TEXTAREA') {
            var end = input.value.length;
            input.setSelectionRange(end, end);
        }
    }

    function open(field) {
        var display = field.querySelector('[data-ti-display]');
        var editor = field.querySelector('[data-ti-editor]');
        if (!editor) return;
        hold(field, true);
        if (display) display.hidden = true;
        editor.hidden = false;
        focusInput(field);
    }

    // Back to the text, as it was (or as a live refresh left it): all in the browser, nothing is sent.
    function close(field) {
        var display = field.querySelector('[data-ti-display]');
        var editor = field.querySelector('[data-ti-editor]');
        if (!editor) return;
        editor.reset();
        editor.querySelectorAll('[data-valmsg-for]').forEach(function (msg) { msg.textContent = ''; });
        editor.querySelectorAll('.input-validation-error').forEach(function (input) { input.classList.remove('input-validation-error'); });
        editor.hidden = true;
        if (display) display.hidden = false;
        hold(field, false);
        catchUp(field);
        display = field.querySelector('[data-ti-display]');
        if (display) (display.querySelector('[data-ti-open]') || display).focus({ preventScroll: true });
    }

    // ---- Opening and closing the editors ----

    document.addEventListener('click', function (evt) {
        var target = evt.target;
        if (!target.closest) return;

        // A click outside the open people dropdown closes it (and saves the ticks, if they changed).
        // A chip's × in the same form sends the ticks itself, so the dropdown just closes.
        document.querySelectorAll('#kb-task-view [data-ti-menu-root]').forEach(function (root) {
            if (root.contains(target) || root.querySelector('[data-ti-menu]').hidden) return;
            var chipRemove = target.closest('[data-ti-chip-remove]');
            closeMenu(root, !(chipRemove && chipRemove.form === root.closest('form')));
        });

        var opener = target.closest('#kb-task-view [data-ti-open]');
        if (opener) {
            var field = fieldOf(opener);
            if (field) open(field);
            return;
        }

        var cancel = target.closest('#kb-task-view [data-ti-cancel]');
        if (cancel) {
            close(fieldOf(cancel));
            return;
        }

        var clear = target.closest('#kb-task-view [data-ti-clear]');
        if (clear) {
            var form = clear.closest('form');
            form.querySelector('[data-ti-input]').value = '';
            form.requestSubmit();
            return;
        }

        // The draft's column pill: only the choice changes, nothing is saved yet.
        var column = target.closest('#kb-task-view.kb-ti-draft [data-ti-column]');
        if (column) {
            pickDraftColumn(column);
            return;
        }

        var toggle = target.closest('#kb-task-view [data-ti-menu-toggle]');
        if (toggle) {
            var root = toggle.closest('[data-ti-menu-root]');
            if (root.querySelector('[data-ti-menu]').hidden) openMenu(root);
            else closeMenu(root, true);
        }
    });

    // The description is a div (it holds paragraphs): Enter or Space opens it like a button.
    document.addEventListener('keydown', function (evt) {
        if (evt.key !== 'Enter' && evt.key !== ' ') return;
        var opener = evt.target.matches && evt.target.matches('#kb-task-view div[data-ti-open]') ? evt.target : null;
        if (!opener) return;
        evt.preventDefault();
        open(fieldOf(opener));
    });

    // Escape cancels the open editor or closes the dropdown without closing the popup (capture: before Bootstrap's modal sees it).
    // In the draft's title it does close the popup, creating nothing.
    document.addEventListener('keydown', function (evt) {
        var target = evt.target;
        if (!target.closest) return;

        if (evt.key === 'Escape') {
            var menuRoot = target.closest('#kb-task-view [data-ti-menu-root]');
            if (menuRoot && !menuRoot.querySelector('[data-ti-menu]').hidden) {
                evt.preventDefault();
                evt.stopPropagation();
                menuRoot.querySelector('[data-ti-menu-toggle]').focus();
                closeMenu(menuRoot, true);
                return;
            }
            var editor = target.closest('#kb-task-view [data-ti-editor]');
            if (!editor || editor.hasAttribute('data-ti-draft')) return;
            evt.preventDefault();
            evt.stopPropagation();
            close(fieldOf(editor));
            return;
        }

        // Ctrl+Enter saves the description (Enter alone is a new line).
        if (evt.key === 'Enter' && (evt.ctrlKey || evt.metaKey) && target.matches('#kb-task-view textarea[data-ti-input]')) {
            evt.preventDefault();
            target.closest('form').requestSubmit();
            return;
        }

        // Enter in the dropdown must not submit the form (its first submit button is a chip's ×).
        // In the search it ticks or unticks the first person shown.
        if (evt.key === 'Enter' && target.matches('#kb-task-view [data-ti-menu] input')) {
            evt.preventDefault();
            if (!target.matches('[data-search-client]')) return;
            var first = Array.prototype.find.call(target.closest('[data-ti-menu]').querySelectorAll('[data-ti-person]'), function (li) { return !li.hidden; });
            if (!first) return;
            var box = first.querySelector('[data-ti-assign]');
            box.checked = !box.checked;
            box.dispatchEvent(new Event('change', { bubbles: true }));
        }
    }, true);

    // ---- The draft ----

    // "Create task" only once there is a title.
    document.addEventListener('input', function (evt) {
        if (!evt.target.matches || !evt.target.matches('#kb-task-view [data-ti-draft] [data-ti-input]')) return;
        var create = evt.target.form.querySelector('[data-ti-create]');
        if (create) create.disabled = evt.target.value.trim() === '';
    });

    function pickDraftColumn(option) {
        var root = view();
        var hidden = root.querySelector('[data-ti-draft-column]');
        if (hidden) hidden.value = option.value;
        var pill = root.querySelector('[data-ti-pill]');
        pill.style.background = option.dataset.color;
        pill.querySelector('[data-ti-pill-text]').textContent = option.dataset.status + ' · ' + option.dataset.name;
        pill.setAttribute('aria-label', 'Column: ' + option.dataset.status + ' · ' + option.dataset.name + '. Change column');
        root.querySelectorAll('[data-ti-column]').forEach(function (other) {
            if (other === option) other.setAttribute('aria-current', 'true');
            else other.removeAttribute('aria-current');
        });
        var where = root.querySelector('[data-ti-where]');
        if (where) where.textContent = where.textContent.replace(/›.*$/, '› ' + option.dataset.name);
        var input = root.querySelector('[data-ti-draft] [data-ti-input]');
        if (input) input.focus();
    }

    // ---- The column pill and the people dropdown stay put while open ----

    document.addEventListener('show.bs.dropdown', function (evt) {
        hold(fieldOf(evt.target), true);
    });
    document.addEventListener('hidden.bs.dropdown', function (evt) {
        var field = fieldOf(evt.target);
        hold(field, false);
        catchUp(field);
    });

    // The ticked people, in a fixed order (to tell whether anything changed).
    function ticked(form) {
        return Array.prototype.filter.call(form.querySelectorAll('[data-ti-assign]'), function (box) { return box.checked; })
            .map(function (box) { return box.dataset.tiAssign; })
            .sort()
            .join(',');
    }

    function openMenu(root) {
        var menu = root.querySelector('[data-ti-menu]');
        hold(fieldOf(root), true);
        root._tiBefore = ticked(root.closest('form'));
        menu.hidden = false;
        root.querySelector('[data-ti-menu-toggle]').setAttribute('aria-expanded', 'true');
        var search = menu.querySelector('[data-search-client]');
        if (search) {
            search.value = '';
            filterPeople(menu, '');
            search.focus();
        }
    }

    // save: send the ticks if they changed (one SetAssignees call; its response releases the hold).
    // Nothing changed: no request, the field just catches up with any live refresh it skipped.
    function closeMenu(root, save) {
        var form = root.closest('form');
        var changed = root._tiBefore !== undefined && ticked(form) !== root._tiBefore;
        root._tiBefore = undefined;
        root.querySelector('[data-ti-menu]').hidden = true;
        root.querySelector('[data-ti-menu-toggle]').setAttribute('aria-expanded', 'false');
        if (changed) {
            if (save) form.requestSubmit();
            return;
        }
        var field = fieldOf(root);
        hold(field, false);
        catchUp(field);
    }

    // A tick shows the person's chip, an untick hides it; nothing is saved until the dropdown closes.
    document.addEventListener('change', function (evt) {
        if (!evt.target.matches || !evt.target.matches('#kb-task-view [data-ti-assign]')) return;
        var chip = view().querySelector('[data-ti-chip="' + evt.target.dataset.tiAssign + '"]');
        if (chip) chip.hidden = !evt.target.checked;
    });

    function filterPeople(menu, text) {
        var query = text.trim().toLowerCase();
        var shown = 0;
        menu.querySelectorAll('[data-ti-person]').forEach(function (li) {
            li.hidden = query !== '' && li.dataset.name.toLowerCase().indexOf(query) === -1;
            if (!li.hidden) shown++;
        });
        var empty = menu.querySelector('[data-ti-people-empty]');
        if (empty) empty.hidden = shown > 0;
    }

    document.addEventListener('input', function (evt) {
        if (!evt.target.matches || !evt.target.matches('#kb-task-view [data-ti-menu] [data-search-client]')) return;
        filterPeople(evt.target.closest('[data-ti-menu]'), evt.target.value);
    });

    // ---- Saving ----

    var savingField = null;   // the field whose save is on its way (for the focus afterwards)

    // A save is on its way: let its response render the field (closed when saved, open with the error when not).
    document.addEventListener('htmx:beforeSend', function (evt) {
        var field = fieldOf(evt.detail.elt);
        if (!field) return;
        hold(field, false);
        field._tiFresh = null;
        savingField = field.getAttribute('data-ti-field');
    });

    // After the view is morphed: a field rendered open (the draft's title, a validation error) is held and focused.
    function settle(root, focus) {
        if (!root) return;
        root.querySelectorAll('[data-ti-field]').forEach(function (field) {
            var editor = field.querySelector('[data-ti-editor]');
            if (editor && !editor.hidden) {
                hold(field, true);
                if (focus) focusInput(field, true);
            }
        });
    }

    // The focus was on something the save hid or removed (Save, a chip's ×, an image's ×): move it to the saved field's
    // own control. Focus that is still somewhere visible stays where it is.
    function refocus(name) {
        var active = document.activeElement;
        if (active && active !== document.body && active.isConnected && active.offsetParent !== null) return;
        var field = name && view() && view().querySelector('[data-ti-field="' + name + '"]');
        if (!field) return;
        var editor = field.querySelector('[data-ti-editor]');
        if (editor && !editor.hidden) return;
        var control = field.querySelector('[data-ti-display]:not([hidden]) [data-ti-open], [data-ti-display][data-ti-open]:not([hidden]), [data-ti-menu-toggle], [data-ti-pill], [data-ti-pick]');
        if (control) control.focus({ preventScroll: true });
    }

    document.addEventListener('htmx:afterSettle', function (evt) {
        var target = evt.detail.target;
        if (!target) return;
        var config = evt.detail.requestConfig;
        var live = !!(config && config.headers && config.headers['X-Live-Refresh']);
        if (target.id === 'kb-task-view') {
            settle(target, !live);
            if (!live) {
                refocus(savingField);
                savingField = null;
            }
        } else if (target.id === 'kb-task-modal-body') {
            settle(view(), true);
        }
    });

    // Bootstrap focuses the popup itself once it is shown: give the focus back to an open editor (the draft's title).
    document.addEventListener('shown.bs.modal', function (evt) {
        if (evt.target.id === 'kb-task-modal') settle(view(), true);
    });

    // ---- Images: pick, preview, upload ----

    var upload = { files: [], urls: [], errors: [] };
    // The same × as the task view's round remove buttons (_TaskView, .kb-ti-x svg).
    var X_ICON = '<svg aria-hidden="true" width="10" height="10" viewBox="0 0 10 10" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"><path d="M1.5 1.5l7 7M8.5 1.5l-7 7"></path></svg>';

    function uploadModal() {
        return document.getElementById('kb-upload-modal');
    }

    function pickLimits() {
        var pick = document.querySelector('#kb-task-view [data-ti-pick]');
        return {
            maxFiles: parseInt(pick.dataset.maxFiles, 10),
            maxBytes: parseInt(pick.dataset.maxBytes, 10),
            extensions: pick.dataset.extensions.toLowerCase().split(',')
        };
    }

    // A new selection replaces the previous one. Files of the wrong type or size are left out, with the reason shown.
    function choose(fileList) {
        var limits = pickLimits();
        var megabytes = Math.round(limits.maxBytes / (1024 * 1024));
        upload.files = [];
        upload.errors = [];
        Array.prototype.forEach.call(fileList, function (file) {
            var dot = file.name.lastIndexOf('.');
            var extension = dot === -1 ? '' : file.name.slice(dot).toLowerCase();
            if (limits.extensions.indexOf(extension) === -1)
                upload.errors.push(file.name + ': allowed file types: ' + limits.extensions.join(', ') + '.');
            else if (file.size > limits.maxBytes)
                upload.errors.push(file.name + ': each file can\'t be larger than ' + megabytes + ' MB.');
            else
                upload.files.push(file);
        });
        renderUpload();
        var modal = uploadModal();
        if (modal && !modal.classList.contains('show')) bootstrap.Modal.getOrCreateInstance(modal).show();
    }

    function renderUpload() {
        var modal = uploadModal();
        if (!modal) return;
        var limits = pickLimits();
        var count = upload.files.length;

        upload.urls.forEach(function (url) { URL.revokeObjectURL(url); });
        upload.urls = [];

        var grid = modal.querySelector('[data-upload-grid]');
        grid.innerHTML = '';
        upload.files.forEach(function (file, index) {
            var url = URL.createObjectURL(file);
            upload.urls.push(url);

            var item = document.createElement('li');
            item.className = 'kb-ti-upload-item';
            var img = document.createElement('img');
            img.src = url;
            img.alt = '';
            var name = document.createElement('span');
            name.className = 'kb-ti-upload-name';
            name.textContent = file.name;
            name.title = file.name;
            var remove = document.createElement('button');
            remove.type = 'button';
            remove.className = 'kb-ti-x kb-ti-upload-x';
            remove.setAttribute('aria-label', 'Remove ' + file.name);
            remove.dataset.uploadRemove = index;
            remove.innerHTML = X_ICON;
            item.appendChild(img);
            item.appendChild(name);
            item.appendChild(remove);
            grid.appendChild(item);
        });

        var errors = upload.errors.slice();
        if (count > limits.maxFiles) errors.push('You can upload up to ' + limits.maxFiles + ' images at a time.');
        var error = modal.querySelector('[data-upload-error]');
        error.hidden = errors.length === 0;
        error.textContent = errors.join(' ');

        modal.querySelector('[data-upload-title]').textContent = 'Upload ' + count + (count === 1 ? ' image?' : ' images?');
        var submit = modal.querySelector('[data-upload-submit]');
        submit.textContent = 'Upload ' + count;
        submit.disabled = count === 0 || count > limits.maxFiles;
    }

    function pickFiles() {
        var picker = document.createElement('input');
        picker.type = 'file';
        picker.multiple = true;
        picker.accept = pickLimits().extensions.join(',');
        picker.addEventListener('change', function () {
            if (picker.files.length > 0) choose(picker.files);
        });
        picker.click();
    }

    document.addEventListener('change', function (evt) {
        if (!evt.target.matches || !evt.target.matches('#kb-task-view [data-ti-pick]')) return;
        var files = Array.prototype.slice.call(evt.target.files);
        evt.target.value = '';
        if (files.length > 0) choose(files);
    });

    document.addEventListener('click', function (evt) {
        if (!evt.target.closest) return;
        var remove = evt.target.closest('#kb-upload-modal [data-upload-remove]');
        if (remove) {
            upload.files.splice(parseInt(remove.dataset.uploadRemove, 10), 1);
            renderUpload();
            return;
        }
        if (evt.target.closest('#kb-upload-modal [data-upload-choose]')) {
            pickFiles();
            return;
        }
        var submit = evt.target.closest('#kb-upload-modal [data-upload-submit]');
        if (submit && !submit.disabled) {
            var form = document.querySelector('#kb-task-view [data-ti-upload-form]');
            if (form) {
                var transfer = new DataTransfer();
                upload.files.forEach(function (file) { transfer.items.add(file); });
                form.querySelector('[data-ti-upload-input]').files = transfer.files;
                form.requestSubmit();
            }
            bootstrap.Modal.getOrCreateInstance(uploadModal()).hide();
        }
    });

    // Stacked over the task popup (profile.js raises it and its backdrop): its backdrop is the newest one, given a darker shade.
    document.addEventListener('shown.bs.modal', function (evt) {
        if (evt.target.id !== 'kb-upload-modal') return;
        var backdrops = document.querySelectorAll('.modal-backdrop');
        var last = backdrops[backdrops.length - 1];
        if (last) last.classList.add('kb-ti-upload-backdrop');
    });
    document.addEventListener('hidden.bs.modal', function (evt) {
        if (evt.target.id !== 'kb-upload-modal') return;
        upload.urls.forEach(function (url) { URL.revokeObjectURL(url); });
        upload = { files: [], urls: [], errors: [] };
        // Hiding a modal unlocks the page: the task popup is still open, so lock it again.
        var task = document.getElementById('kb-task-modal');
        if (task && task.classList.contains('show')) {
            document.body.classList.add('modal-open');
            document.body.style.overflow = 'hidden';
        }
    });
})();
