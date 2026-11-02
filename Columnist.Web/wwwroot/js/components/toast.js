(function () {
    var KEY = 'kb-toasts';
    var DURATION = 4000;
    var stack = null;

    function load() {
        try { return JSON.parse(sessionStorage.getItem(KEY)) || []; } catch (e) { return []; }
    }

    function save(list) {
        try { sessionStorage.setItem(KEY, JSON.stringify(list)); } catch (e) { }
    }

    function show(t, now) {
        if (!stack) {
            stack = document.createElement('div');
            stack.className = 'kb-toast-stack';
            stack.setAttribute('data-kb-toast', 'ready');
            document.body.appendChild(stack);
        }

        var remaining = t.expiresAt - now;
        var isNew = remaining >= DURATION;

        var toast = document.createElement('div');
        toast.className = 'kb-toast kb-toast-' + t.type;
        toast.setAttribute('data-kb-toast', 'ready');
        toast.setAttribute('role', t.type === 'error' ? 'alert' : 'status');
        if (!isNew) toast.style.animation = 'none';
        toast.style.setProperty('--kb-toast-delay', (remaining - DURATION) + 'ms');

        var text = document.createElement('span');
        text.className = 'kb-toast-text';
        text.textContent = t.text;

        var close = document.createElement('button');
        close.type = 'button';
        close.className = 'kb-toast-close';
        close.setAttribute('aria-label', 'Close');
        close.innerHTML = '&times;';

        toast.appendChild(text);
        toast.appendChild(close);
        stack.appendChild(toast);

        function hide() {
            if (toast.classList.contains('kb-toast-hide')) return;
            toast.classList.add('kb-toast-hide');
            save(load().filter(function (s) { return s.id !== t.id; }));
            setTimeout(function () { toast.remove(); }, 400);
        }

        close.addEventListener('click', hide);
        setTimeout(hide, remaining);
    }

    function adopt(root) {
        var now = Date.now();
        var added = [];

        root.querySelectorAll('.kb-toast:not([data-kb-toast])').forEach(function (el) {
            added.push({
                id: now + '-' + Math.random().toString(36).slice(2),
                type: el.classList.contains('kb-toast-error') ? 'error' : 'success',
                text: el.querySelector('.kb-toast-text').textContent,
                expiresAt: now + DURATION
            });
        });
        root.querySelectorAll('.kb-toast-stack:not([data-kb-toast])').forEach(function (s) { s.remove(); });

        if (added.length === 0) return;
        save(load().concat(added));
        added.forEach(function (t) { show(t, now); });
    }

    var now = Date.now();
    var restored = load().filter(function (t) { return t.expiresAt > now; });
    save(restored);
    restored.forEach(function (t) { show(t, now); });
    adopt(document);

    document.addEventListener('htmx:beforeHistorySave', function () {
        if (stack) { stack.remove(); stack = null; }
    });

    document.addEventListener('htmx:load', function (evt) { adopt(evt.target); });
})();
