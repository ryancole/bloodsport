async function initBrackets() {
    const canvases = document.querySelectorAll('canvas[data-bracket]');
    if (!canvases.length) return;

    const { init, zoomIn, zoomOut, resetView } = await import('/js/bracketCanvas.js');
    window._bc = { init, zoomIn, zoomOut, fit: resetView };

    canvases.forEach(canvas => {
        init(canvas.id, JSON.parse(canvas.dataset.bracket));
    });
}

function initEasyMDE() {
    const el = document.getElementById('markdown-editor');
    if (!el || !window.EasyMDE) return;

    new EasyMDE({
        element: el,
        placeholder: 'Write your post content here...',
        spellChecker: false,
        autofocus: false,
        toolbar: [
            'bold', 'italic', 'heading', '|',
            'quote', 'unordered-list', 'ordered-list', '|',
            'link', 'image', '|',
            'preview', 'side-by-side', 'fullscreen', '|',
            'guide'
        ]
    });
}

function initAll() {
    initBrackets();
    initEasyMDE();
}

// Top-of-page loading bar, driven by Blazor's enhanced-navigation events.
// Because the app renders with static SSR, link clicks are handled by an
// AJAX fetch (enhanced navigation) with no native browser spinner, so we
// surface progress ourselves. See the .nav-progress rules in Styles/app.css.
function initNavProgress(blazor) {
    const bar = document.createElement('div');
    bar.className = 'nav-progress';
    document.body.appendChild(bar);

    let hideTimer, trickle, width = 0;

    function start() {
        clearTimeout(hideTimer);
        width = 8;
        bar.classList.add('nav-progress--active');
        bar.style.width = width + '%';
        clearInterval(trickle);
        // Creep toward 90% while we wait on the server, easing as it goes.
        trickle = setInterval(() => {
            width = Math.min(width + (90 - width) * 0.1, 90);
            bar.style.width = width + '%';
        }, 200);
    }

    function done() {
        clearInterval(trickle);
        if (!bar.classList.contains('nav-progress--active')) return;
        bar.style.width = '100%';
        hideTimer = setTimeout(() => {
            bar.classList.remove('nav-progress--active');
            bar.style.width = '0';
        }, 250);
    }

    blazor.addEventListener('enhancednavigationstart', start);
    blazor.addEventListener('enhancednavigationend', done);
}

export function afterWebStarted(blazor) {
    initAll();
    initNavProgress(blazor);
    blazor.addEventListener('enhancedload', initAll);
}
