// boxing.js — Web Audio API sound effects for Boxing Game
// Two sounds: playThrow (punch thrown) and playLand (punch landed)
window.boxingGame = (() => {
    let _ctx = null;

    function getCtx() {
        if (!_ctx) _ctx = new (window.AudioContext || window.webkitAudioContext)();
        if (_ctx.state === 'suspended') _ctx.resume();
        return _ctx;
    }

    // Short sawtooth whoosh: punch thrown
    function playThrow() {
        try {
            const ctx = getCtx();
            const osc  = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.type = 'sawtooth';
            osc.frequency.setValueAtTime(130, ctx.currentTime);
            osc.frequency.exponentialRampToValueAtTime(55, ctx.currentTime + 0.07);
            gain.gain.setValueAtTime(0.12, ctx.currentTime);
            gain.gain.linearRampToValueAtTime(0, ctx.currentTime + 0.07);
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start(ctx.currentTime);
            osc.stop(ctx.currentTime + 0.07);
        } catch (_) {}
    }

    // Thuddy low-pass noise burst: punch landed
    function playLand() {
        try {
            const ctx     = getCtx();
            const dur     = 0.06;
            const samples = Math.ceil(ctx.sampleRate * dur);
            const buffer  = ctx.createBuffer(1, samples, ctx.sampleRate);
            const data    = buffer.getChannelData(0);
            for (let i = 0; i < samples; i++) {
                // decaying random noise
                data[i] = (Math.random() * 2 - 1) * (1 - i / samples);
            }
            const src    = ctx.createBufferSource();
            src.buffer   = buffer;
            const filter = ctx.createBiquadFilter();
            filter.type  = 'lowpass';
            filter.frequency.value = 380;
            filter.Q.value = 1.2;
            const gain   = ctx.createGain();
            gain.gain.setValueAtTime(0.7, ctx.currentTime);
            src.connect(filter);
            filter.connect(gain);
            gain.connect(ctx.destination);
            src.start(ctx.currentTime);
        } catch (_) {}
    }

    // Boxing bell: struck metal timbre — sharp transient + harmonic ring
    function playBell() {
        try {
            const ctx = getCtx();

            // Real boxing bells have a cluster of inharmonic partials that
            // decay at different rates, giving that bright clang → warm ring.
            const partials = [
                { freq: 1060, amp: 0.55, decay: 1.8  },  // fundamental ring
                { freq: 1590, amp: 0.30, decay: 1.3  },  // 3rd partial
                { freq: 2120, amp: 0.20, decay: 0.9  },  // 5th partial
                { freq: 2820, amp: 0.14, decay: 0.6  },  // upper shimmer
                { freq: 4240, amp: 0.07, decay: 0.35 },  // bright attack overtone
            ];

            partials.forEach(p => {
                const osc  = ctx.createOscillator();
                const gain = ctx.createGain();
                osc.type = 'sine';
                osc.frequency.value = p.freq;
                gain.gain.setValueAtTime(p.amp, ctx.currentTime);
                gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + p.decay);
                osc.connect(gain);
                gain.connect(ctx.destination);
                osc.start(ctx.currentTime);
                osc.stop(ctx.currentTime + p.decay + 0.05);
            });

            // Strike transient: very short high-frequency noise burst for the "clang" attack
            const bufSize = Math.ceil(ctx.sampleRate * 0.018);
            const buf     = ctx.createBuffer(1, bufSize, ctx.sampleRate);
            const data    = buf.getChannelData(0);
            for (let i = 0; i < bufSize; i++)
                data[i] = (Math.random() * 2 - 1) * Math.pow(1 - i / bufSize, 3);
            const noise     = ctx.createBufferSource();
            noise.buffer    = buf;
            const bandpass  = ctx.createBiquadFilter();
            bandpass.type   = 'bandpass';
            bandpass.frequency.value = 3500;
            bandpass.Q.value         = 1.5;
            const nGain = ctx.createGain();
            nGain.gain.value = 0.45;
            noise.connect(bandpass);
            bandpass.connect(nGain);
            nGain.connect(ctx.destination);
            noise.start(ctx.currentTime);
        } catch (_) {}
    }

    // Leather-on-leather thud: glove strikes covering hands
    function playBlock() {
        try {
            const ctx     = getCtx();
            const dur     = 0.05;
            const samples = Math.ceil(ctx.sampleRate * dur);
            const buffer  = ctx.createBuffer(1, samples, ctx.sampleRate);
            const data    = buffer.getChannelData(0);
            for (let i = 0; i < samples; i++) {
                // Faster decay than a landed punch — tighter, drier thud
                data[i] = (Math.random() * 2 - 1) * Math.pow(1 - i / samples, 2.5);
            }
            const src    = ctx.createBufferSource();
            src.buffer   = buffer;
            // Midrange bandpass: gloves sound less meaty than flesh impact
            const filter = ctx.createBiquadFilter();
            filter.type  = 'bandpass';
            filter.frequency.value = 520;
            filter.Q.value = 0.8;
            const gain   = ctx.createGain();
            gain.gain.setValueAtTime(0.45, ctx.currentTime);
            src.connect(filter);
            filter.connect(gain);
            gain.connect(ctx.destination);
            src.start(ctx.currentTime);
        } catch (_) {}
    }

    // ── Global keyboard capture ─────────────────────────────────────────────────
    // Attaches document-level listeners so key input works regardless of which
    // element has focus (avoids the "div must be clicked first" problem).

    let _keyDownHandler = null;
    let _keyUpHandler   = null;

    // Keys whose browser default (page scroll) we want to suppress in-game
    const _scrollKeys = new Set(['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', ' ']);

    function initKeyCapture(dotnetRef) {
        disposeKeyCapture();   // remove any stale listeners from a previous mount

        _keyDownHandler = function (e) {
            if (_scrollKeys.has(e.key)) e.preventDefault();
            dotnetRef.invokeMethodAsync('HandleKeyDown', e.key).catch(() => {});
        };
        _keyUpHandler = function (e) {
            dotnetRef.invokeMethodAsync('HandleKeyUp', e.key).catch(() => {});
        };

        document.addEventListener('keydown', _keyDownHandler);
        document.addEventListener('keyup',   _keyUpHandler);
    }

    function disposeKeyCapture() {
        if (_keyDownHandler) { document.removeEventListener('keydown', _keyDownHandler); _keyDownHandler = null; }
        if (_keyUpHandler)   { document.removeEventListener('keyup',   _keyUpHandler);   _keyUpHandler   = null; }
    }

    // ── Device detection ────────────────────────────────────────────────────────
    // Returns true if the current device has a touch screen (phone / tablet).
    // Checked once at startup; result drives the mobile UI in Blazor.
    function isMobile() {
        return navigator.maxTouchPoints > 0 || 'ontouchstart' in window;
    }

    return { playThrow, playLand, playBell, playBlock, initKeyCapture, disposeKeyCapture, isMobile };
})();
