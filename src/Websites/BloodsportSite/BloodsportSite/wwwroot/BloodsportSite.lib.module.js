async function initBrackets() {
    const canvases = document.querySelectorAll('canvas[data-bracket]');
    if (!canvases.length) return;

    const { init, zoomIn, zoomOut, resetView } = await import('/js/bracketCanvas.js');
    window._bc = { init, zoomIn, zoomOut, fit: resetView };

    canvases.forEach(canvas => {
        init(canvas.id, JSON.parse(canvas.dataset.bracket));
    });
}

export function afterWebStarted(blazor) {
    initBrackets();
    blazor.addEventListener('enhancedload', initBrackets);
}
