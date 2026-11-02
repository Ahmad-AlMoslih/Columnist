(function () {
    function init(root) {
        // File inputs: check count, size and type as soon as files are picked.
        root.querySelectorAll('input[type="file"][data-max-files]:not([data-kb-upload])').forEach(function (input) {
            input.setAttribute('data-kb-upload', 'ready');
            input.addEventListener('change', function () {
                var files = Array.from(input.files);
                var maxFiles = parseInt(input.dataset.maxFiles, 10);
                var maxSize = parseInt(input.dataset.maxSize, 10);
                var allowed = input.dataset.allowedExt.split(',');

                var error = '';
                if (files.length > maxFiles)
                    error = input.dataset.maxFilesMessage;
                else if (files.some(function (f) { return allowed.indexOf(f.name.slice(f.name.lastIndexOf('.')).toLowerCase()) < 0; }))
                    error = input.dataset.allowedExtMessage;
                else if (files.some(function (f) { return f.size > maxSize; }))
                    error = input.dataset.maxSizeMessage;

                if (error)
                    input.value = '';/btw 
                window.kbShowError(input, error);
            });
        });
    }

    init(document);
    htmx.onLoad(init);
})();
