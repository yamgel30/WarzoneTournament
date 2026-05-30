// ── Confetti (canvas-confetti loaded lazily from CDN) ──────────────────────
async function _ensureConfetti() {
    if (window.confetti) return;
    await new Promise((resolve, reject) => {
        const s = document.createElement('script');
        s.src = 'https://cdn.jsdelivr.net/npm/canvas-confetti@1.9.3/dist/confetti.browser.min.js';
        s.onload = resolve;
        s.onerror = reject;
        document.head.appendChild(s);
    });
}

window.launchMatchPointConfetti = async function () {
    try {
        await _ensureConfetti();
        const colors = ['#ef4444', '#f97316', '#fbbf24', '#fff'];
        confetti({ particleCount: 130, spread: 80, origin: { x: 0.5, y: 0.45 }, colors });
        setTimeout(() => confetti({ particleCount: 80, spread: 110, origin: { x: 0.15, y: 0.5 }, colors }), 400);
        setTimeout(() => confetti({ particleCount: 80, spread: 110, origin: { x: 0.85, y: 0.5 }, colors }), 700);
    } catch {}
};

window.launchWinnerConfetti = async function () {
    try {
        await _ensureConfetti();
        const colors = ['#ffd700', '#fbbf24', '#fff', '#f59e0b', '#ea580c'];
        const end = Date.now() + 4000;
        (function frame() {
            confetti({ particleCount: 6, angle: 60,  spread: 55, origin: { x: 0 }, colors });
            confetti({ particleCount: 6, angle: 120, spread: 55, origin: { x: 1 }, colors });
            if (Date.now() < end) requestAnimationFrame(frame);
        }());
        confetti({ particleCount: 200, spread: 120, origin: { x: 0.5, y: 0.35 }, colors });
    } catch {}
};

// ── Sounds (Web Audio API) ─────────────────────────────────────────────────
window.playMatchPointSound = function () {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        [[523, 0], [659, 0.18], [784, 0.36], [1047, 0.54]].forEach(([freq, t]) => {
            const osc = ctx.createOscillator(), gain = ctx.createGain();
            osc.connect(gain); gain.connect(ctx.destination);
            osc.frequency.value = freq; osc.type = 'sine';
            gain.gain.setValueAtTime(0.22, ctx.currentTime + t);
            gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + t + 0.38);
            osc.start(ctx.currentTime + t);
            osc.stop(ctx.currentTime + t + 0.38);
        });
    } catch {}
};

window.playWinnerSound = function () {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        [[523,0],[659,.12],[784,.24],[659,.36],[1047,.52],[1047,.68],[1047,.85]].forEach(([freq, t]) => {
            const osc = ctx.createOscillator(), gain = ctx.createGain();
            osc.connect(gain); gain.connect(ctx.destination);
            osc.frequency.value = freq; osc.type = 'sine';
            gain.gain.setValueAtTime(0.28, ctx.currentTime + t);
            gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + t + 0.42);
            osc.start(ctx.currentTime + t);
            osc.stop(ctx.currentTime + t + 0.42);
        });
    } catch {}
};
