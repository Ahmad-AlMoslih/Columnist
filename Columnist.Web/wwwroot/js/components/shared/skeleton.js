(function () {
    var DELAY = 200;
    var pending = new WeakMap();

    // The templates hold repeated parts once: data-skeleton-repeat="N" makes N copies of the element (outer ones first),
    // and data-skeleton-cycle="a|b|…" inside a copy (or on it) adds the classes for its place (copy i takes entry i mod the
    // count; "-" leaves the element out).
    function expand(root) {
        var el;
        while ((el = root.querySelector('[data-skeleton-repeat]'))) {
            var count = parseInt(el.getAttribute('data-skeleton-repeat'), 10) || 1;
            var copies = [el];
            for (var i = 1; i < count; i++) {
                var copy = el.cloneNode(true);
                copies[copies.length - 1].after(copy);
                copies.push(copy);
            }
            copies.forEach(function (copy, index) {
                var cycled = Array.prototype.slice.call(copy.querySelectorAll('[data-skeleton-cycle]'));
                if (copy.hasAttribute('data-skeleton-cycle')) cycled.unshift(copy);
                cycled.forEach(function (node) {
                    if (node.closest('[data-skeleton-repeat]') !== copy) return;   // a nested repeat's own
                    var options = node.getAttribute('data-skeleton-cycle').split('|');
                    var value = options[index % options.length];
                    node.removeAttribute('data-skeleton-cycle');
                    if (value === '-') node.remove();
                    else if (value) node.classList.add.apply(node.classList, value.split(' '));
                });
                copy.removeAttribute('data-skeleton-repeat');
            });
        }
    }

    function build(name) {
        var template = document.querySelector('template[data-skeleton-template="' + name + '"]');
        if (!template) return null;
        var fragment = template.content.cloneNode(true);
        expand(fragment);
        return fragment;
    }

    // Adds the skeleton to the area and hides the area's own children; returns the undo.
    function fill(area, fragment) {
        var nodes = Array.prototype.slice.call(fragment.children);
        area.classList.add('kb-skel-busy');
        area.appendChild(fragment);
        return function () {
            nodes.forEach(function (node) { node.remove(); });
            area.classList.remove('kb-skel-busy');
        };
    }

    // aria-busy and the label on the area; returns the undo.
    function mark(area, label) {
        var oldLabel = area.getAttribute('aria-label');
        area.setAttribute('aria-busy', 'true');
        if (label) area.setAttribute('aria-label', label);
        return function () {
            area.removeAttribute('aria-busy');
            if (!label) return;
            if (oldLabel === null) area.removeAttribute('aria-label');
            else area.setAttribute('aria-label', oldLabel);
        };
    }

    // Runs show() if the request is still going after the delay; show returns the undo, run when the request ends.
    function whenSlow(evt, show) {
        var xhr = evt.detail.xhr;
        if (!xhr) return;
        var entry = { undo: null };
        entry.timer = setTimeout(function () {
            if (xhr.readyState !== 4) entry.undo = show() || null;
        }, DELAY);
        var list = pending.get(xhr) || [];
        list.push(entry);
        pending.set(xhr, list);
    }

    // The content is about to swap in (or the request failed / was aborted): drop the skeleton.
    function finish(evt) {
        var xhr = evt.detail.xhr;
        var list = xhr && pending.get(xhr);
        if (!list) return;
        pending.delete(xhr);
        list.forEach(function (entry) {
            clearTimeout(entry.timer);
            if (entry.undo) entry.undo();
        });
    }
    document.addEventListener('htmx:beforeSwap', finish);
    document.addEventListener('htmx:afterRequest', finish);

    function isLive(evt) {
        var config = evt.detail.requestConfig;
        return !!(config && config.headers && config.headers['X-Live-Refresh']);
    }

    // beforeSend: the request is really going out (beforeRequest can still be cancelled, e.g. by form validation).
    document.addEventListener('htmx:beforeSend', function (evt) {
        var source = evt.detail.elt && evt.detail.elt.closest && evt.detail.elt.closest('[data-skeleton]');
        if (!source || isLive(evt)) return;
        var target = evt.detail.target;

        whenSlow(evt, function () {
            var into = source.getAttribute('data-skeleton-into');
            var area = into ? document.querySelector(into) : target;
            var skeleton = build(source.getAttribute('data-skeleton'));
            if (!area || !area.isConnected || !skeleton) return null;

            var unmark = mark(area, source.getAttribute('data-skeleton-label'));
            var unfill = fill(area, skeleton);
            area.dispatchEvent(new CustomEvent('kb:skeleton-shown', { bubbles: true }));
            return function () {
                unfill();
                unmark();
            };
        });
    });

    window.KbSkeleton = { build: build, fill: fill, mark: mark, whenSlow: whenSlow };
})();
