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

export function afterWebStarted(blazor) {
    initAll();
    blazor.addEventListener('enhancedload', initAll);
}
