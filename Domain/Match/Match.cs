using BoxingGame.Domain.Boxers;
using BoxingGame.Domain.Combat;
using BoxingGame.Domain.Defenses;
using BoxingGame.Domain.Strikes;

namespace BoxingGame.Domain.Match;

public class Match
{
    // ── Ring bounds (must stay inside the SVG viewBox x=222..678) ───────────
    // 258 / 642 gives ~36 px clearance from each edge — enough for the widest
    // boxer body (torso + arms + scaled feet) to remain fully visible.
    private const float RingLeft  = 258f;
    private const float RingRight = 642f;

    // ── Round / break timing ──────────────────────────────────────────────────
    private const float RoundDurationMs    = 180_000f;  // 3-minute rounds (real boxing standard)
    private const float BetweenRoundsDurMs =   5_000f;  // 5-second corner break
    private const int   TotalRounds        = 12;

    // ── Referee count ─────────────────────────────────────────────────────────
    private const float RefereeTick = 1_000f;           // ms per referee count

    // ── Clinch system ─────────────────────────────────────────────────────────
    private const float ClinchSeparation  =  60f;  // gloves-touch boundary (px)
    private const float ClinchTriggerMs   = 550f;  // both must press forward at boundary before clinch
    private const float ClinchFreezeMs    = 380f;  // stage 0: referee steps in, fighters freeze
    private const float ClinchPushMs      = 480f;  // stages 1 & 2: each boxer slides apart
    private const float ClinchPushDistPx  =  40f;  // pixels each boxer slides per push stage

    // ── Head-slip dodge timing ────────────────────────────────────────────────
    public const int DodgeStartupMs  =  55;
    public const int DodgeActiveMs   = 160;
    public const int DodgeRecoveryMs = 200;

    // ── Duck timing (timed auto-return crouch) ────────────────────────────────
    public const int DuckStartupMs  =  80;
    public const int DuckActiveMs   = 350;
    public const int DuckRecoveryMs = 150;

    // ── Block raise animation ─────────────────────────────────────────────────
    // Arms travel from the idle guard to the full cover pose over this window.
    // Fast enough to feel responsive but slow enough that the movement is visible.
    public const int BlockRaiseMs = 90;

    // ── Starting positions ────────────────────────────────────────────────────
    private const float P1StartX = 360f;
    private const float P2StartX = 540f;

    private readonly object _lock = new();
    private readonly Random _rng  = new();

    public string     MatchCode    { get; }
    public Boxer      P1           { get; }
    public Boxer      P2           { get; }
    public MatchPhase Phase        { get; private set; } = MatchPhase.WaitingForPlayers;
    public int        RefereeCount { get; private set; }
    public bool       IsLocalMode  { get; private set; }

    // ── Rounds & scoring ──────────────────────────────────────────────────────
    public int   CurrentRound           { get; private set; } = 1;
    public int   P1Score                { get; private set; }
    public int   P2Score                { get; private set; }
    public float BetweenRoundsElapsedMs { get; private set; }

    // ── Round clock ───────────────────────────────────────────────────────────
    private float _roundElapsedMs;
    public float RoundRemainMs =>
        Math.Max(0f, RoundDurationMs - _roundElapsedMs);

    // ── Sound sequence counters ───────────────────────────────────────────────
    public int SoundGenThrow { get; private set; }
    public int SoundGenLand  { get; private set; }
    public int SoundGenBell  { get; private set; }
    public int SoundGenBlock { get; private set; }   // punch fully absorbed by guard

    // ── Clinch state ──────────────────────────────────────────────────────────
    private float _clinchPressMs;   // accumulated time both fighters press forward at boundary
    private float _clinchStageMs;   // elapsed ms within current clinch stage
    private float _clinchP1StartX, _clinchP1TargetX;
    private float _clinchP2StartX, _clinchP2TargetX;

    public bool  IsClinching         { get; private set; }
    public int   ClinchStage         { get; private set; }
    public float ClinchStageProgress { get; private set; }

    public event Action? StateChanged;

    private string?       _p1ConnectionId;
    private string?       _p2ConnectionId;
    private float         _refereeElapsedMs;
    private string?       _knockedDownPlayerId;

    public Match(string code)
    {
        MatchCode = code;

        P1 = new Boxer { Id = "P1", DisplayName = "Red",  Stance = Side.Left,  Stats = new() };
        P2 = new Boxer { Id = "P2", DisplayName = "Blue", Stance = Side.Right, Stats = new() };

        P1.Reset(P1StartX, Side.Left);
        P2.Reset(P2StartX, Side.Right);

        // Offset P2's idle dance so the two boxers move out of phase.
        P2.IdleAnimMs = 600f;

        // Assign hidden knockdown-recovery limits (not shown to players).
        P1.MaxGetups = _rng.Next(1, 5);   // 1–4 getups before TKO
        P2.MaxGetups = _rng.Next(1, 5);

        // Random heights: 0.90–1.10 — affects reach and SVG scale.
        P1.HeightFactor = 0.90f + (float)_rng.NextDouble() * 0.20f;
        P2.HeightFactor = 0.90f + (float)_rng.NextDouble() * 0.20f;
    }

    /// <summary>Starts a local (same-tab, two-player) match.</summary>
    public void StartLocalMatch(bool p1Southpaw = false, bool p2Southpaw = false)
    {
        lock (_lock)
        {
            P1.IsSouthpaw   = p1Southpaw;
            P2.IsSouthpaw   = p2Southpaw;
            IsLocalMode     = true;
            _p1ConnectionId = "local-p1";
            _p2ConnectionId = "local-p2";
            Phase           = MatchPhase.InRound;
            SoundGenBell++;   // 🔔 opening bell
        }
    }

    /// <summary>Returns assigned slot ("P1"/"P2") or null when full.</summary>
    public string? TryRegisterPlayer(string connectionId, bool isSouthpaw = false)
    {
        lock (_lock)
        {
            if (_p1ConnectionId == null)
            {
                _p1ConnectionId = connectionId;
                P1.IsSouthpaw   = isSouthpaw;
                return "P1";
            }
            if (_p2ConnectionId == null)
            {
                _p2ConnectionId = connectionId;
                P2.IsSouthpaw   = isSouthpaw;
                Phase           = MatchPhase.InRound;
                SoundGenBell++;   // 🔔 opening bell
                return "P2";
            }
            return null;
        }
    }

    public void UnregisterPlayer(string connectionId)
    {
        lock (_lock)
        {
            if (_p1ConnectionId == connectionId) _p1ConnectionId = null;
            if (_p2ConnectionId == connectionId) _p2ConnectionId = null;
        }
    }

    /// <summary>
    /// Called per input event from the page or AI controller.
    /// defenseHeld is hold-based; attackKey, dodgePressed, and duckPressed are one-shot.
    /// sprintHeld boosts speed; bodyModifier redirects punch to body variant.
    /// </summary>
    public void ApplyInput(string playerId, MovementDirection movement, string? attackKey,
                           bool defenseHeld, bool sprintHeld = false,
                           bool bodyModifier = false, bool dodgePressed = false,
                           bool duckPressed = false)
    {
        lock (_lock)
        {
            if (Phase != MatchPhase.InRound) return;
            if (IsClinching) return;   // no inputs during referee separation

            var boxer = playerId == "P1" ? P1 : P2;

            // Persist flags so Hurt-window pre-input and sprint state survive across ticks.
            boxer.PendingDefenseHeld = defenseHeld;
            boxer.SprintHeld         = sprintHeld;

            bool dodgeStarted = false;
            bool duckStarted  = false;

            // ── Duck (one-shot, timed auto-return) — Idle / Moving / Defending ─
            if (duckPressed &&
                boxer.State is BoxerStateKind.Idle or BoxerStateKind.Moving or BoxerStateKind.Defending &&
                boxer.DodgePhase == null)
            {
                boxer.State          = BoxerStateKind.Ducking;
                boxer.DodgePhase     = StrikePhase.Startup;
                boxer.DodgeElapsedMs = 0f;
                boxer.CurrentDefense = null;
                boxer.CurrentStrike  = null;
                boxer.CurrentPhase   = null;
                duckStarted          = true;
            }

            // ── Head-slip dodge (one-shot, timed) — Idle / Moving / Defending ─
            if (!duckStarted && dodgePressed &&
                boxer.State is BoxerStateKind.Idle or BoxerStateKind.Moving or BoxerStateKind.Defending &&
                boxer.DodgePhase == null)
            {
                boxer.State          = BoxerStateKind.Dodging;
                boxer.DodgePhase     = StrikePhase.Startup;
                boxer.DodgeElapsedMs = 0f;
                boxer.CurrentDefense = null;
                boxer.CurrentStrike  = null;
                boxer.CurrentPhase   = null;
                dodgeStarted         = true;
            }

            // ── Block (hold-based) ─────────────────────────────────────────────
            // Can enter from: Idle, Moving, Defending.
            // Can also cancel dodge/duck during Active or Recovery (Startup protected).
            bool inCancellableDodge = !dodgeStarted && !duckStarted &&
                                       boxer.State == BoxerStateKind.Dodging &&
                                       boxer.DodgePhase is StrikePhase.Active or StrikePhase.Recovery;
            bool inCancellableDuck  = !dodgeStarted && !duckStarted &&
                                       boxer.State == BoxerStateKind.Ducking &&
                                       boxer.DodgePhase is StrikePhase.Active or StrikePhase.Recovery;

            if (defenseHeld &&
                (boxer.State is BoxerStateKind.Idle or BoxerStateKind.Moving or BoxerStateKind.Defending
                 || inCancellableDodge || inCancellableDuck))
            {
                boxer.State          = BoxerStateKind.Defending;
                boxer.CurrentDefense = BothHandsBlock.Instance;
                boxer.CurrentStrike  = null;
                boxer.CurrentPhase   = null;
                boxer.DodgePhase     = null;
                boxer.DodgeElapsedMs = 0f;
            }
            else if (!defenseHeld && boxer.State == BoxerStateKind.Defending)
            {
                boxer.State          = boxer.PendingMovement != MovementDirection.None
                                         ? BoxerStateKind.Moving : BoxerStateKind.Idle;
                boxer.CurrentDefense = null;
            }

            // Body modifier: "LeftJab" → "BodyLeftJab", etc.
            if (bodyModifier && attackKey != null)
                attackKey = "Body" + attackKey;

            // ── Strike — Idle / Moving / Defending / Dodging / Ducking ─────────
            // Attacking out of a slip or duck simulates a counter: evasion window
            // ends the moment the punch starts.
            if (attackKey != null &&
                boxer.State is BoxerStateKind.Idle     or BoxerStateKind.Moving
                            or BoxerStateKind.Defending or BoxerStateKind.Dodging
                            or BoxerStateKind.Ducking &&
                boxer.PostHitCooldownMs <= 0f)
            {
                var strike = StrikeCatalog.Get(attackKey);
                if (strike != null)
                {
                    boxer.State          = BoxerStateKind.Attacking;
                    boxer.CurrentStrike  = strike;
                    boxer.CurrentPhase   = StrikePhase.Startup;
                    boxer.PhaseElapsedMs = 0f;
                    boxer.HitResolved    = false;
                    boxer.CurrentDefense = null;
                    boxer.DodgePhase     = null;
                    boxer.DodgeElapsedMs = 0f;
                    SoundGenThrow++;
                }
            }

            // ── Movement ─────────────────────────────────────────────────────
            // While Defending, state stays Defending; TickBoxer applies a 50% speed
            // penalty and still allows foot movement.
            if (boxer.State is BoxerStateKind.Idle or BoxerStateKind.Moving)
            {
                boxer.State = movement != MovementDirection.None
                    ? BoxerStateKind.Moving : BoxerStateKind.Idle;
            }
            // Always propagate direction — TickBoxer reads it for position updates.
            boxer.PendingMovement = movement;
        }
    }

    public void Tick(TimeSpan dt)
    {
        lock (_lock)
        {
            float ms = (float)dt.TotalMilliseconds;

            switch (Phase)
            {
                case MatchPhase.WaitingForPlayers:
                case MatchPhase.Finished:
                    return;

                case MatchPhase.InRound:
                    if (IsClinching)
                    {
                        TickClinch(ms);
                    }
                    else
                    {
                        TickBoxer(P1, P2, ms);
                        TickBoxer(P2, P1, ms);
                        CheckClinch(ms);
                    }
                    _roundElapsedMs += ms;
                    if (_roundElapsedMs >= RoundDurationMs)
                        EndRound();
                    break;

                case MatchPhase.RefereeCount:
                    TickRefereeCount(ms);
                    break;

                case MatchPhase.BetweenRounds:
                    BetweenRoundsElapsedMs += ms;
                    if (BetweenRoundsElapsedMs >= BetweenRoundsDurMs)
                        StartNextRound();
                    break;
            }
        }

        StateChanged?.Invoke();
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private void TickBoxer(Boxer boxer, Boxer opponent, float ms)
    {
        boxer.IdleAnimMs = (boxer.IdleAnimMs + ms) % 1200f;

        // Post-hit cooldown countdown
        if (boxer.PostHitCooldownMs > 0f)
            boxer.PostHitCooldownMs = Math.Max(0f, boxer.PostHitCooldownMs - ms);

        // Stamina: drain on sprint, recover otherwise
        if (boxer.SprintHeld)
            boxer.Stamina = Math.Max(0f,
                boxer.Stamina - boxer.Stats.SprintDrainPerSec * (ms / 1000f));
        else
            boxer.Stamina = Math.Min(boxer.Stats.MaxStamina,
                boxer.Stamina + boxer.Stats.StaminaRecoveryPerSec * (ms / 1000f));

        float staminaFactor  = 0.5f + 0.5f * (boxer.Stamina / boxer.Stats.MaxStamina);
        float speedFactor    = staminaFactor * (boxer.SprintHeld ? boxer.Stats.SprintSpeedMultiplier : 1.0f);
        // While blocking the boxer shuffles at 50% speed — enough to adjust range
        // without making cover-and-walk as effective as free movement.
        float defenseFactor  = boxer.State == BoxerStateKind.Defending ? 0.5f : 1.0f;
        // While executing a strike that has WalkSpeedMultiplier < 1 (e.g. uppercut),
        // the boxer can only shuffle very slowly — rooting them in place for heavy punches.
        float strikeFactor   = boxer.State == BoxerStateKind.Attacking && boxer.CurrentStrike != null
                                   ? boxer.CurrentStrike.WalkSpeedMultiplier : 1.0f;
        float effectiveSpeed = speedFactor * defenseFactor * strikeFactor;

        // Walk cycle — advances while moving, attacking, dodging, or defending
        bool isMoving = boxer.PendingMovement != MovementDirection.None &&
                        boxer.State is BoxerStateKind.Moving   or BoxerStateKind.Attacking
                                    or BoxerStateKind.Dodging  or BoxerStateKind.Defending;
        if (isMoving)
            boxer.WalkCycleMs = (boxer.WalkCycleMs + ms * effectiveSpeed) % 350f;
        else
            boxer.WalkCycleMs = 0f;

        // ── Defense raise counter ─────────────────────────────────────────────
        // Accumulates while in Defending so BoxerSvg can lerp the arm raise.
        // Resets the instant the boxer leaves Defending — fresh entry next time.
        boxer.DefenseElapsedMs = boxer.State == BoxerStateKind.Defending
            ? Math.Min(boxer.DefenseElapsedMs + ms, BlockRaiseMs)
            : 0f;

        // Position update — also allowed while defending (shuffling with guard up)
        if (isMoving)
        {
            float dir = boxer.PendingMovement == MovementDirection.Forward ? 1f : -1f;
            if (boxer.Stance == Side.Right) dir = -dir;
            float newX = Math.Clamp(
                boxer.PositionX + dir * boxer.Stats.MoveSpeedPxPerSec * effectiveSpeed * (ms / 1000f),
                RingLeft, RingRight);

            // Boxers cannot walk through each other; ClinchSeparation is the
            // gloves-touch boundary — pressing into it long enough triggers a clinch.
            if (boxer.PositionX <= opponent.PositionX)
                newX = Math.Min(newX, opponent.PositionX - ClinchSeparation);
            else
                newX = Math.Max(newX, opponent.PositionX + ClinchSeparation);

            boxer.PositionX = newX;
        }

        // ── Duck phase tick (timed auto-return crouch) ───────────────────────
        if (boxer.State == BoxerStateKind.Ducking && boxer.DodgePhase.HasValue)
        {
            boxer.DodgeElapsedMs += ms;
            switch (boxer.DodgePhase)
            {
                case StrikePhase.Startup:
                    if (boxer.DodgeElapsedMs >= DuckStartupMs)
                    {
                        boxer.DodgePhase     = StrikePhase.Active;
                        boxer.DodgeElapsedMs = 0f;
                    }
                    break;

                case StrikePhase.Active:
                    if (boxer.DodgeElapsedMs >= DuckActiveMs)
                    {
                        boxer.DodgePhase     = StrikePhase.Recovery;
                        boxer.DodgeElapsedMs = 0f;
                    }
                    break;

                case StrikePhase.Recovery:
                    if (boxer.DodgeElapsedMs >= DuckRecoveryMs)
                    {
                        boxer.DodgePhase = null;
                        boxer.State = boxer.PendingMovement != MovementDirection.None
                            ? BoxerStateKind.Moving : BoxerStateKind.Idle;
                    }
                    break;
            }
            return;
        }

        // ── Dodge (head-slip) phase tick ──────────────────────────────────────
        if (boxer.State == BoxerStateKind.Dodging && boxer.DodgePhase.HasValue)
        {
            boxer.DodgeElapsedMs += ms;
            switch (boxer.DodgePhase)
            {
                case StrikePhase.Startup:
                    if (boxer.DodgeElapsedMs >= DodgeStartupMs)
                    {
                        boxer.DodgePhase     = StrikePhase.Active;
                        boxer.DodgeElapsedMs = 0f;
                    }
                    break;

                case StrikePhase.Active:
                    if (boxer.DodgeElapsedMs >= DodgeActiveMs)
                    {
                        boxer.DodgePhase     = StrikePhase.Recovery;
                        boxer.DodgeElapsedMs = 0f;
                    }
                    break;

                case StrikePhase.Recovery:
                    // If block is still held, snap directly into Defending — slip → block.
                    if (boxer.PendingDefenseHeld)
                    {
                        boxer.DodgePhase     = null;
                        boxer.DodgeElapsedMs = 0f;
                        boxer.State          = BoxerStateKind.Defending;
                        boxer.CurrentDefense = BothHandsBlock.Instance;
                    }
                    else if (boxer.DodgeElapsedMs >= DodgeRecoveryMs)
                    {
                        boxer.DodgePhase = null;
                        boxer.State      = boxer.PendingMovement != MovementDirection.None
                            ? BoxerStateKind.Moving : BoxerStateKind.Idle;
                    }
                    break;
            }
            return;
        }

        // ── Hurt state ────────────────────────────────────────────────────────
        if (boxer.State == BoxerStateKind.Hurt)
        {
            boxer.HurtElapsedMs += ms;
            if (boxer.HurtElapsedMs >= boxer.Stats.HurtRecoveryMs)
            {
                boxer.HurtElapsedMs = 0f;
                // A landed punch always resets stance to neutral — the player must
                // consciously re-engage their guard.  (PendingDefenseHeld was already
                // cleared in ApplyDamage so this is just the definitive exit state.)
                boxer.State = boxer.PendingMovement != MovementDirection.None
                    ? BoxerStateKind.Moving : BoxerStateKind.Idle;
            }
            return;
        }

        if (boxer.State != BoxerStateKind.Attacking) return;

        var strike = boxer.CurrentStrike!;
        boxer.PhaseElapsedMs += ms;

        // Phase transitions — thresholds divided by speedFactor
        switch (boxer.CurrentPhase)
        {
            case StrikePhase.Startup:
                if (boxer.PhaseElapsedMs >= strike.StartupMs / speedFactor)
                {
                    boxer.CurrentPhase   = StrikePhase.Active;
                    boxer.PhaseElapsedMs = 0f;
                    boxer.HitResolved    = false;
                    ResolveHit(boxer, opponent, strike);
                }
                break;

            case StrikePhase.Active:
                if (!boxer.HitResolved)
                    ResolveHit(boxer, opponent, strike);
                if (boxer.PhaseElapsedMs >= strike.ActiveMs / speedFactor)
                {
                    boxer.CurrentPhase   = StrikePhase.Recovery;
                    boxer.PhaseElapsedMs = 0f;
                }
                break;

            case StrikePhase.Recovery:
                if (boxer.PhaseElapsedMs >= strike.RecoveryMs / speedFactor)
                    ResetToIdle(boxer);
                break;
        }
    }

    private void ResolveHit(Boxer attacker, Boxer defender, Strike strike)
    {
        attacker.HitResolved = true;

        float dist  = Math.Abs(attacker.PositionX - defender.PositionX);
        float range = attacker.Stats.ReachMultiplier * strike.BaseRangePx * attacker.HeightFactor;
        if (dist > range) return;

        // ── Evasion: dodge (head-slip) and duck both evade head strikes during Active ──
        if (strike.Target == BodyLocation.Head &&
            defender.DodgePhase == StrikePhase.Active &&
            defender.State is BoxerStateKind.Dodging or BoxerStateKind.Ducking)
            return;

        float damage = strike.Damage;
        if (defender.State == BoxerStateKind.Defending && defender.CurrentDefense != null)
        {
            if (defender.CurrentDefense.EvadesStrike(strike)) return;

            var eff = defender.CurrentDefense.EvaluateAgainst(strike);
            if (eff == DefenseEffectiveness.Full)
            {
                SoundGenBlock++;   // 🧤 leather-on-leather thud
                return;
            }
            if (eff == DefenseEffectiveness.Partial) damage *= 0.3f;
        }

        // Score the landed punch (only punches that pass all defenses count)
        if (attacker.Id == "P1") P1Score += strike.ScoreValue;
        else                     P2Score += strike.ScoreValue;

        SoundGenLand++;                       // 🔊 impact sound
        attacker.PostHitCooldownMs = 200f;    // brief anti-spam pause
        ApplyDamage(defender, damage, strike.Target);
    }

    private void ApplyDamage(Boxer boxer, float damage, BodyLocation target)
    {
        // Route damage to the matching sub-pool
        if (target == BodyLocation.Head)
            boxer.HeadHealth = Math.Max(0f, boxer.HeadHealth - damage);
        else
            boxer.BodyHealth = Math.Max(0f, boxer.BodyHealth - damage);

        // Keep the combined health field in sync
        boxer.Health = boxer.HeadHealth + boxer.BodyHealth;

        // A hit always breaks the boxer's stance contract — clear the auto-defend
        // flag so Hurt recovery exits to neutral, not back into Defending.
        boxer.PendingDefenseHeld = false;

        // Either sub-pool reaching zero is a knockdown
        if (boxer.HeadHealth <= 0f || boxer.BodyHealth <= 0f)
        {
            boxer.State          = BoxerStateKind.KnockedDown;
            boxer.CurrentStrike  = null;
            boxer.CurrentDefense = null;
            boxer.CurrentPhase   = null;
            boxer.DodgePhase     = null;
            boxer.KnockdownCount++;
            _knockedDownPlayerId = boxer.Id;
            _refereeElapsedMs    = 0f;
            RefereeCount         = 0;
            // Leave InRound — clear any active clinch
            IsClinching      = false;
            ClinchStage      = 0;
            ClinchStageProgress = 0f;
            _clinchPressMs   = 0f;
            _clinchStageMs   = 0f;
            Phase                = MatchPhase.RefereeCount;
        }
        else
        {
            boxer.State          = BoxerStateKind.Hurt;
            boxer.HurtElapsedMs  = 0f;
            boxer.CurrentStrike  = null;
            boxer.CurrentDefense = null;
            boxer.CurrentPhase   = null;
            boxer.DodgePhase     = null;
        }
    }

    private void TickRefereeCount(float ms)
    {
        _refereeElapsedMs += ms;
        RefereeCount = (int)(_refereeElapsedMs / RefereeTick);

        var knockedDown = _knockedDownPlayerId == "P1" ? P1 : P2;
        int getUpAt     = AutoGetUpCount(knockedDown);

        if (RefereeCount >= 10)
        {
            // Count reached 10 without getting up → TKO / KO
            knockedDown.State = BoxerStateKind.KnockedOut;
            Phase = MatchPhase.Finished;
        }
        else if (RefereeCount >= getUpAt)
        {
            // Boxer rises — restore health proportional to knockdown history.
            // First getup: 55 %; each subsequent KD cuts 13 % more; floor at 12 %.
            float restoreFrac  = Math.Max(0.12f, 0.55f - (knockedDown.KnockdownCount - 1) * 0.13f);
            float staminaFrac  = Math.Max(0.30f, 0.55f - (knockedDown.KnockdownCount - 1) * 0.08f);
            knockedDown.State      = BoxerStateKind.Idle;
            knockedDown.HeadHealth = knockedDown.Stats.MaxHeadHealth * restoreFrac;
            knockedDown.BodyHealth = knockedDown.Stats.MaxBodyHealth * restoreFrac;
            knockedDown.Health     = knockedDown.HeadHealth + knockedDown.BodyHealth;
            knockedDown.Stamina    = knockedDown.Stats.MaxStamina * staminaFrac;
            _refereeElapsedMs   = 0f;
            RefereeCount        = 0;

            // Return both fighters to their starting corners
            P1.PositionX = P1StartX;
            P2.PositionX = P2StartX;

            Phase = MatchPhase.InRound;
        }
    }

    /// <summary>
    /// Returns the count at which the boxer automatically rises.
    /// Returns 11 (never reached before a 10-count KO) when all getups are used.
    /// </summary>
    private static int AutoGetUpCount(Boxer boxer) =>
        boxer.KnockdownCount > boxer.MaxGetups
            ? 11
            : Math.Min(2 + boxer.KnockdownCount * 2, 9);

    private void EndRound()
    {
        // Clear any active clinch before freezing the round
        IsClinching      = false;
        ClinchStage      = 0;
        ClinchStageProgress = 0f;
        _clinchPressMs   = 0f;
        _clinchStageMs   = 0f;
        _roundElapsedMs        = 0f;
        BetweenRoundsElapsedMs = 0f;
        SoundGenBell++;   // 🔔 end-of-round bell

        // Freeze both boxers cleanly
        foreach (var b in new[] { P1, P2 })
        {
            b.State              = BoxerStateKind.Idle;
            b.CurrentStrike      = null;
            b.CurrentPhase       = null;
            b.CurrentDefense     = null;
            b.DodgePhase         = null;
            b.PhaseElapsedMs     = 0f;
            b.DodgeElapsedMs     = 0f;
            b.DefenseElapsedMs   = 0f;
            b.HitResolved        = false;
            b.HurtElapsedMs      = 0f;
            b.PostHitCooldownMs  = 0f;
            b.PendingMovement    = MovementDirection.None;
            b.PendingDefenseHeld = false;
            b.SprintHeld         = false;
            b.WalkCycleMs        = 0f;
        }

        Phase = CurrentRound >= TotalRounds
            ? MatchPhase.Finished
            : MatchPhase.BetweenRounds;
    }

    private void StartNextRound()
    {
        CurrentRound++;
        BetweenRoundsElapsedMs = 0f;
        _roundElapsedMs        = 0f;
        SoundGenBell++;   // 🔔 start-of-round bell

        // Reset positions; fully restore health/stamina; keep KD history.
        ResetBoxerForRound(P1, P1StartX);
        ResetBoxerForRound(P2, P2StartX);

        Phase = MatchPhase.InRound;
    }

    private static void ResetBoxerForRound(Boxer boxer, float startX)
    {
        boxer.PositionX   = startX;
        boxer.HeadHealth  = boxer.Stats.MaxHeadHealth;
        boxer.BodyHealth  = boxer.Stats.MaxBodyHealth;
        boxer.Health      = boxer.Stats.MaxHealth;
        boxer.Stamina     = boxer.Stats.MaxStamina;
        boxer.WalkCycleMs = 0f;
        // KnockdownCount and MaxGetups intentionally preserved across rounds
    }

    /// <summary>
    /// Called when a strike's Recovery phase completes naturally (no hit taken).
    /// If the block key was held throughout the punch, the boxer immediately re-enters
    /// Defending — so hold-S + throw-punch returns them to guard on completion.
    /// A hit always breaks this contract (see ApplyDamage).
    /// </summary>
    private static void ResetToIdle(Boxer boxer)
    {
        boxer.CurrentStrike  = null;
        boxer.CurrentPhase   = null;
        boxer.PhaseElapsedMs = 0f;

        if (boxer.PendingDefenseHeld)
        {
            boxer.State          = BoxerStateKind.Defending;
            boxer.CurrentDefense = BothHandsBlock.Instance;
        }
        else
        {
            boxer.State = boxer.PendingMovement != MovementDirection.None
                ? BoxerStateKind.Moving : BoxerStateKind.Idle;
        }
    }

    // ── Clinch helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Accumulate time when both fighters press into each other at the contact
    /// boundary.  After ClinchTriggerMs the referee steps in.
    /// </summary>
    private void CheckClinch(float ms)
    {
        float dist        = Math.Abs(P1.PositionX - P2.PositionX);
        bool  atMinSep    = dist <= ClinchSeparation + 3f;
        bool  bothForward = P1.PendingMovement == MovementDirection.Forward &&
                            P2.PendingMovement == MovementDirection.Forward;

        if (atMinSep && bothForward)
        {
            _clinchPressMs += ms;
            if (_clinchPressMs >= ClinchTriggerMs)
                EnterClinch();
        }
        else
        {
            _clinchPressMs = 0f;
        }
    }

    private void EnterClinch()
    {
        IsClinching         = true;
        ClinchStage         = 0;
        ClinchStageProgress = 0f;
        _clinchStageMs      = 0f;
        _clinchPressMs      = 0f;

        // Interrupt both boxers — freeze them in the clinch embrace
        foreach (var b in new[] { P1, P2 })
        {
            b.State              = BoxerStateKind.Clinching;
            b.CurrentStrike      = null;
            b.CurrentPhase       = null;
            b.CurrentDefense     = null;
            b.DodgePhase         = null;
            b.PhaseElapsedMs     = 0f;
            b.DodgeElapsedMs     = 0f;
            b.HurtElapsedMs      = 0f;
            b.PendingMovement    = MovementDirection.None;
        }

        // Record positions for the two-stage separation animation
        _clinchP1StartX  = P1.PositionX;
        _clinchP1TargetX = Math.Max(P1.PositionX - ClinchPushDistPx, RingLeft);
        _clinchP2StartX  = P2.PositionX;
        _clinchP2TargetX = Math.Min(P2.PositionX + ClinchPushDistPx, RingRight);
    }

    private void TickClinch(float ms)
    {
        _clinchStageMs += ms;

        switch (ClinchStage)
        {
            case 0: // Stage 0: referee steps in, "BREAK!" shown, fighters freeze
                ClinchStageProgress = Math.Clamp(_clinchStageMs / ClinchFreezeMs, 0f, 1f);
                if (_clinchStageMs >= ClinchFreezeMs)
                {
                    ClinchStage    = 1;
                    _clinchStageMs = 0f;
                }
                break;

            case 1: // Stage 1: referee guides P1 backward (ease-out slide)
            {
                float t    = Math.Clamp(_clinchStageMs / ClinchPushMs, 0f, 1f);
                ClinchStageProgress = t;
                P1.PositionX = Lerp(_clinchP1StartX, _clinchP1TargetX, 1f - (1f - t) * (1f - t));
                if (_clinchStageMs >= ClinchPushMs)
                {
                    P1.PositionX   = _clinchP1TargetX;
                    ClinchStage    = 2;
                    _clinchStageMs = 0f;
                }
                break;
            }

            case 2: // Stage 2: referee guides P2 backward
            {
                float t    = Math.Clamp(_clinchStageMs / ClinchPushMs, 0f, 1f);
                ClinchStageProgress = t;
                P2.PositionX = Lerp(_clinchP2StartX, _clinchP2TargetX, 1f - (1f - t) * (1f - t));
                if (_clinchStageMs >= ClinchPushMs)
                {
                    P2.PositionX = _clinchP2TargetX;
                    ExitClinch();
                }
                break;
            }
        }
    }

    private void ExitClinch()
    {
        IsClinching         = false;
        ClinchStage         = 0;
        ClinchStageProgress = 0f;
        _clinchStageMs      = 0f;
        _clinchPressMs      = 0f;

        foreach (var b in new[] { P1, P2 })
        {
            b.State              = BoxerStateKind.Idle;
            b.PendingMovement    = MovementDirection.None;
            b.PendingDefenseHeld = false;
            b.CurrentStrike      = null;
            b.CurrentPhase       = null;
            b.CurrentDefense     = null;
            b.DodgePhase         = null;
            b.PhaseElapsedMs     = 0f;
            b.DodgeElapsedMs     = 0f;
            b.DefenseElapsedMs   = 0f;
            b.HurtElapsedMs      = 0f;
        }
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
