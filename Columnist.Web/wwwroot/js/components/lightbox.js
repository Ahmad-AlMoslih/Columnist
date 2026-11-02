(function () {
    var lightbox = document.getElementById('kb-lightbox');
    if (!lightbox) return;
    var lightboxImg = lightbox.querySelector('img');

    document.addEventListener('click', function (e) {
        if (!e.target.matches('.kb-gallery img, .kb-cover img')) return;
        lightboxImg.src = e.target.src;
        lightbox.hidden = false;
    });

    lightbox.querySelector('.kb-lightbox-close').addEventListener('click', function () {
        lightbox.hidden = true;
        lightboxImg.removeAttribute('src');
    });
})();
