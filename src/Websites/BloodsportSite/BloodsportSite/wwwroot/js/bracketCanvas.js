// Bracket canvas renderer — pan, zoom, click-to-navigate

const HEADER_H    = 32;
const CARD_W      = 180;
const CARD_H      = 56;
const ROW_H       = 28;
const ROUND_GAP   = 40;
const BASE_SLOT_H = 80;
const RADIUS      = 4;

const C_LINE    = '#dee2e6';
const C_CARD_BG = '#ffffff';
const C_CARD_BD = '#dee2e6';
const C_WIN_BG  = 'rgba(25,135,84,0.08)';
const C_WIN_FG  = '#198754';
const C_SEED    = '#6c757d';
const C_TEXT    = '#212529';
const C_TBD     = '#adb5bd';
const C_HEADER  = '#6c757d';

const instances = new Map();

// ── Public API ────────────────────────────────────────────────────────

export function init(canvasId, data) {
    // Tear down any previous instance for this canvas (e.g. enhanced-nav revisit)
    destroy(canvasId);

    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const parent = canvas.parentElement;
    canvas.width  = parent.clientWidth;
    canvas.height = parent.clientHeight;

    const s = {
        canvas,
        ctx: canvas.getContext('2d'),
        data,
        zoom: 1,
        panX: 0,
        panY: 0,
        isPanning: false,
        didPan: false,
        lastMX: 0,
        lastMY: 0,
        hitBoxes: [],
        images: new Map()
    };

    s.layout = computeLayout(data);
    fitView(s);
    attachEvents(s);
    draw(s);
    preloadImages(s);
    instances.set(canvasId, s);
}

function preloadImages(s) {
    const urls = new Set();
    for (const round of s.data.rounds) {
        for (const m of round.matchups) {
            if (m.teamOne?.logoUrl) urls.add(m.teamOne.logoUrl);
            if (m.teamTwo?.logoUrl) urls.add(m.teamTwo.logoUrl);
        }
    }
    const promises = [...urls].map(url => new Promise(resolve => {
        const img = new Image();
        img.onload  = () => { s.images.set(url, img); resolve(); };
        img.onerror = () => resolve();
        img.src = url;
    }));
    Promise.all(promises).then(() => draw(s));
}

export function destroy(canvasId) {
    const s = instances.get(canvasId);
    if (!s) return;
    s._ro?.disconnect();
    detachEvents(s);
    instances.delete(canvasId);
}

export function resetView(canvasId) {
    const s = instances.get(canvasId);
    if (!s) return;
    fitView(s);
    draw(s);
}

export function zoomIn(canvasId) {
    const s = instances.get(canvasId);
    if (!s) return;
    applyZoom(s, 1.25, s.canvas.width / 2, s.canvas.height / 2);
    draw(s);
}

export function zoomOut(canvasId) {
    const s = instances.get(canvasId);
    if (!s) return;
    applyZoom(s, 0.8, s.canvas.width / 2, s.canvas.height / 2);
    draw(s);
}

// ── Layout ────────────────────────────────────────────────────────────

function computeLayout(data) {
    const rounds = data.rounds;
    if (!rounds || rounds.length === 0) return { rounds: [], totalW: 0, totalH: HEADER_H };

    const firstCount = rounds[0].matchups.length || 1;

    const lRounds = rounds.map((round, ri) => {
        const count  = round.matchups.length || 1;
        const factor = firstCount / count;
        const slotH  = BASE_SLOT_H * factor;
        const x      = ri * (CARD_W + ROUND_GAP);

        return {
            name: round.name,
            x,
            matchups: round.matchups.map((m, mi) => ({
                ...m,
                cardX: x,
                cardY: HEADER_H + (mi + 0.5) * slotH - CARD_H / 2,
                cy:    HEADER_H + (mi + 0.5) * slotH
            }))
        };
    });

    return {
        rounds:  lRounds,
        totalW:  lRounds[lRounds.length - 1].x + CARD_W,
        totalH:  HEADER_H + firstCount * BASE_SLOT_H
    };
}

function fitView(s) {
    const { layout, canvas } = s;
    if (!layout || layout.totalW === 0) return;
    const pad    = 24;
    const fitAll = Math.min(
        (canvas.width  - pad * 2) / layout.totalW,
        (canvas.height - pad * 2) / layout.totalH,
        1.5
    );

    if (fitAll >= 1.0) {
        // Bracket is small — show everything centered
        s.zoom = fitAll;
        s.panX = (canvas.width  - layout.totalW * s.zoom) / 2;
        s.panY = (canvas.height - layout.totalH * s.zoom) / 2;
    } else {
        // Bracket is large — zoom to show ~2 rounds centered on the first incomplete round
        s.zoom = 0.72;
        const firstIncomplete = layout.rounds.find(r =>
            r.matchups.some(m => !m.teamOneWinner && !m.teamTwoWinner)
        );
        const target = firstIncomplete ?? layout.rounds[layout.rounds.length - 1];
        const cx = target ? target.x + CARD_W / 2 : layout.totalW / 2;
        const cy = layout.totalH / 2;
        s.panX = canvas.width  / 2 - cx * s.zoom;
        s.panY = canvas.height / 2 - cy * s.zoom;
    }
}

// ── Drawing ───────────────────────────────────────────────────────────

function draw(s) {
    const { ctx, canvas, layout } = s;
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.save();
    ctx.translate(s.panX, s.panY);
    ctx.scale(s.zoom, s.zoom);

    s.hitBoxes = [];
    drawRoundHeaders(ctx, layout);
    drawConnectors(ctx, layout);
    for (const round of layout.rounds) {
        for (const m of round.matchups) drawCard(ctx, m, s);
    }

    ctx.restore();
}

function drawRoundHeaders(ctx, layout) {
    ctx.font         = 'bold 12px system-ui, sans-serif';
    ctx.textAlign    = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillStyle    = C_HEADER;
    for (const r of layout.rounds) {
        ctx.fillText(r.name, r.x + CARD_W / 2, HEADER_H / 2, CARD_W);
    }
}

function drawConnectors(ctx, layout) {
    ctx.strokeStyle = C_LINE;
    ctx.lineWidth   = 1.5;

    for (let ri = 0; ri < layout.rounds.length - 1; ri++) {
        const cur  = layout.rounds[ri];
        const next = layout.rounds[ri + 1];
        const xR   = cur.x + CARD_W;
        const xMid = xR + ROUND_GAP / 2;
        const xL   = next.x;

        for (let pi = 0; pi < Math.floor(cur.matchups.length / 2); pi++) {
            const top = cur.matchups[pi * 2];
            const bot = cur.matchups[pi * 2 + 1];
            const nxt = next.matchups[pi];
            if (!top || !bot || !nxt) continue;

            ctx.beginPath();
            // Right stub from top card
            ctx.moveTo(xR,   top.cy);
            ctx.lineTo(xMid, top.cy);
            // Right stub from bottom card
            ctx.moveTo(xR,   bot.cy);
            ctx.lineTo(xMid, bot.cy);
            // Vertical bridge
            ctx.moveTo(xMid, top.cy);
            ctx.lineTo(xMid, bot.cy);
            // Left stub into next-round card
            ctx.moveTo(xMid, nxt.cy);
            ctx.lineTo(xL,   nxt.cy);
            ctx.stroke();
        }
    }
}

function drawCard(ctx, m, s) {
    const { cardX, cardY } = m;

    roundRect(ctx, cardX, cardY, CARD_W, CARD_H, RADIUS);
    ctx.fillStyle = C_CARD_BG;
    ctx.fill();
    ctx.strokeStyle = C_CARD_BD;
    ctx.lineWidth   = 1;
    ctx.stroke();

    // Row divider
    ctx.beginPath();
    ctx.moveTo(cardX + 1, cardY + ROW_H);
    ctx.lineTo(cardX + CARD_W - 1, cardY + ROW_H);
    ctx.strokeStyle = C_LINE;
    ctx.stroke();

    drawTeamRow(ctx, m.teamOne, cardX, cardY,        m.teamOneWinner, s.images);
    drawTeamRow(ctx, m.teamTwo, cardX, cardY + ROW_H, m.teamTwoWinner, s.images);

    s.hitBoxes.push({
        x: cardX, y: cardY, w: CARD_W, h: CARD_H,
        url: `/playoffs/${s.data.playoffId}/matchups/${m.id}`
    });
}

const LOGO_W = 28;
const LOGO_H = 18;
const SEED_AREA = 18; // width reserved for seed number
const LOGO_X_OFF = 6 + SEED_AREA + 3; // left pad + seed area + gap
const NAME_X_OFF = LOGO_X_OFF + LOGO_W + 4;

function drawTeamRow(ctx, team, x, y, winner, images) {
    ctx.save();
    ctx.beginPath();
    ctx.rect(x + 1, y + 1, CARD_W - 2, ROW_H - 1);
    ctx.clip();

    if (winner) {
        ctx.fillStyle = C_WIN_BG;
        ctx.fillRect(x + 1, y + 1, CARD_W - 2, ROW_H - 1);
    }

    const cy = y + ROW_H / 2;

    if (team) {
        // Seed
        ctx.font         = '10px system-ui, sans-serif';
        ctx.fillStyle    = C_SEED;
        ctx.textAlign    = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(String(team.seed ?? ''), x + 6 + SEED_AREA / 2, cy);

        // Logo
        const lx = x + LOGO_X_OFF;
        const ly = y + (ROW_H - LOGO_H) / 2;
        const img = team.logoUrl ? images?.get(team.logoUrl) : null;
        if (img) {
            ctx.save();
            roundRect(ctx, lx, ly, LOGO_W, LOGO_H, 2);
            ctx.clip();
            ctx.drawImage(img, lx, ly, LOGO_W, LOGO_H);
            ctx.restore();
            // Gold border
            ctx.strokeStyle = '#f0b429';
            ctx.lineWidth   = 2;
            roundRect(ctx, lx, ly, LOGO_W, LOGO_H, 2);
            ctx.stroke();
        } else {
            // Placeholder
            ctx.fillStyle = '#dee2e6';
            roundRect(ctx, lx, ly, LOGO_W, LOGO_H, 2);
            ctx.fill();
        }

        // Name
        ctx.font      = winner ? 'bold 12px system-ui, sans-serif' : '12px system-ui, sans-serif';
        ctx.fillStyle = winner ? C_WIN_FG : C_TEXT;
        ctx.textAlign = 'left';
        ctx.fillText(team.name ?? 'Unknown', x + NAME_X_OFF, cy, CARD_W - NAME_X_OFF - 6);
    } else {
        ctx.font         = '12px system-ui, sans-serif';
        ctx.fillStyle    = C_TBD;
        ctx.textAlign    = 'left';
        ctx.textBaseline = 'middle';
        ctx.fillText('TBD', x + NAME_X_OFF, cy);
    }

    ctx.restore();
}

function roundRect(ctx, x, y, w, h, r) {
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.lineTo(x + w - r, y);
    ctx.arcTo(x + w, y,     x + w, y + r,     r);
    ctx.lineTo(x + w, y + h - r);
    ctx.arcTo(x + w, y + h, x + w - r, y + h, r);
    ctx.lineTo(x + r, y + h);
    ctx.arcTo(x,     y + h, x,     y + h - r, r);
    ctx.lineTo(x,     y + r);
    ctx.arcTo(x,     y,     x + r, y,         r);
    ctx.closePath();
}

// ── Zoom & pan ────────────────────────────────────────────────────────

function applyZoom(s, factor, cx, cy) {
    const nz = Math.min(4, Math.max(0.15, s.zoom * factor));
    const af = nz / s.zoom;
    s.panX   = cx - af * (cx - s.panX);
    s.panY   = cy - af * (cy - s.panY);
    s.zoom   = nz;
}

function hitTest(s, mx, my) {
    const lx = (mx - s.panX) / s.zoom;
    const ly = (my - s.panY) / s.zoom;
    return s.hitBoxes.find(hb => lx >= hb.x && lx <= hb.x + hb.w && ly >= hb.y && ly <= hb.y + hb.h);
}

function attachEvents(s) {
    const { canvas } = s;

    s._wheel = e => {
        e.preventDefault();
        const r = canvas.getBoundingClientRect();
        applyZoom(s, e.deltaY < 0 ? 1.12 : 0.893, e.clientX - r.left, e.clientY - r.top);
        draw(s);
    };

    s._mdown = e => {
        s.isPanning = true;
        s.didPan    = false;
        s.lastMX    = e.clientX;
        s.lastMY    = e.clientY;
        canvas.style.cursor = 'grabbing';
    };

    s._mmove = e => {
        if (!s.isPanning) {
            const r   = canvas.getBoundingClientRect();
            const hit = hitTest(s, e.clientX - r.left, e.clientY - r.top);
            canvas.style.cursor = hit ? 'pointer' : 'grab';
            return;
        }
        const dx = e.clientX - s.lastMX;
        const dy = e.clientY - s.lastMY;
        s.panX  += dx;
        s.panY  += dy;
        s.lastMX = e.clientX;
        s.lastMY = e.clientY;
        if (Math.abs(dx) > 2 || Math.abs(dy) > 2) s.didPan = true;
        draw(s);
    };

    s._mup = e => {
        const wasPanning = s.isPanning;
        s.isPanning = false;
        canvas.style.cursor = 'grab';
        if (wasPanning && !s.didPan) {
            const r   = canvas.getBoundingClientRect();
            const hit = hitTest(s, e.clientX - r.left, e.clientY - r.top);
            if (hit) window.location.href = hit.url;
        }
    };

    s._mleave = () => {
        s.isPanning = false;
        canvas.style.cursor = 'grab';
    };

    // Touch pan
    s._tstart = e => {
        if (e.touches.length !== 1) return;
        s.isPanning = true;
        s.didPan    = false;
        s.lastMX    = e.touches[0].clientX;
        s.lastMY    = e.touches[0].clientY;
    };
    s._tmove = e => {
        if (!s.isPanning || e.touches.length !== 1) return;
        e.preventDefault();
        const dx = e.touches[0].clientX - s.lastMX;
        const dy = e.touches[0].clientY - s.lastMY;
        s.panX  += dx;
        s.panY  += dy;
        s.lastMX = e.touches[0].clientX;
        s.lastMY = e.touches[0].clientY;
        if (Math.abs(dx) > 2 || Math.abs(dy) > 2) s.didPan = true;
        draw(s);
    };
    s._tend = () => { s.isPanning = false; };

    canvas.addEventListener('wheel',      s._wheel,  { passive: false });
    canvas.addEventListener('mousedown',  s._mdown);
    window.addEventListener('mousemove',  s._mmove);
    window.addEventListener('mouseup',    s._mup);
    canvas.addEventListener('mouseleave', s._mleave);
    canvas.addEventListener('touchstart', s._tstart, { passive: true  });
    canvas.addEventListener('touchmove',  s._tmove,  { passive: false });
    canvas.addEventListener('touchend',   s._tend,   { passive: true  });

    canvas.style.cursor = 'grab';

    // Resize observer — keep canvas sized to container
    s._ro = new ResizeObserver(() => {
        const p = canvas.parentElement;
        canvas.width  = p.clientWidth;
        canvas.height = p.clientHeight;
        draw(s);
    });
    s._ro.observe(canvas.parentElement);
}

function detachEvents(s) {
    const { canvas } = s;
    canvas.removeEventListener('wheel',      s._wheel);
    canvas.removeEventListener('mousedown',  s._mdown);
    window.removeEventListener('mousemove',  s._mmove);
    window.removeEventListener('mouseup',    s._mup);
    canvas.removeEventListener('mouseleave', s._mleave);
    canvas.removeEventListener('touchstart', s._tstart);
    canvas.removeEventListener('touchmove',  s._tmove);
    canvas.removeEventListener('touchend',   s._tend);
}
