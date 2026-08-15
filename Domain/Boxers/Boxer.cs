using BoxingGame.Domain.Combat;
using BoxingGame.Domain.Defenses;
using BoxingGame.Domain.Strikes;

namespace BoxingGame.Domain.Boxers;

public class Boxer
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string DisplayName { get; set; } = "Boxer";
    public BoxerStats Stats { get; init; } = new();
    public Side Stance { get; init; } = Side.Left;

    /// <summary>
    /// Orthodox = left-hand lead (standard). Southpaw = right-hand lead.
    /// Affects the idle guard pose and which foot is forward in the stance.
    /// </summary>
    public bool IsSouthpaw { get; set; }

    /// <summary>Idle animation clock (ms). Cycles 0..1200 continuously; drives the boxing dance.</summary>
    public float IdleAnimMs { get; set; }

    // World X position (pixels). Y is fixed at the ring floor.
    public float PositionX { get; set; }

    /// <summary>Combined health (HeadHealth + BodyHealth). Kept in sync by Match.ApplyDamage.</summary>
    public float Health { get; set; }
    /// <summary>Head hit-point sub-pool (max = Stats.MaxHeadHealth). Depleted by head punches.</summary>
    public float HeadHealth { get; set; }
    /// <summary>Body hit-point sub-pool (max = Stats.MaxBodyHealth). Depleted by body punches.</summary>
    public float BodyHealth { get; set; }

    public BoxerStateKind State { get; set; } = BoxerStateKind.Idle;

    public Strike?    CurrentStrike  { get; set; }
    public Defense?   CurrentDefense { get; set; }
    public StrikePhase? CurrentPhase { get; set; }
    public float PhaseElapsedMs { get; set; }
    public bool HitResolved { get; set; }   // true once per Active phase so we don't hit twice

    public float HurtElapsedMs { get; set; }
    public float KnockdownElapsedMs { get; set; }
    public int KnockdownCount { get; set; }

    // Set from ApplyInput each tick; consumed by Match.Tick for position update.
    public MovementDirection PendingMovement { get; set; }

    /// <summary>Walk-cycle clock (ms, 0..350). Advances while moving; drives foot-stepping animation.</summary>
    public float WalkCycleMs { get; set; }

    /// <summary>
    /// Hidden secret: maximum number of times this boxer can beat the referee count.
    /// Set randomly at match start; not displayed.
    /// </summary>
    public int MaxGetups { get; set; } = 3;

    // ── Head-slip dodge / duck (timed evasion) ───────────────────────────────
    public StrikePhase? DodgePhase     { get; set; }
    public float        DodgeElapsedMs { get; set; }

    // ── Defense raise animation ───────────────────────────────────────────────
    /// <summary>
    /// Accumulates (ms) while State == Defending; reset to 0 the moment the boxer
    /// leaves Defending.  Used to drive the guard-raise lerp in BoxerSvg — arms
    /// travel from the idle guard pose to the full block pose over BlockRaiseMs.
    /// </summary>
    public float DefenseElapsedMs { get; set; }

    // ── Physical attributes (set once at match start; not displayed) ─────────
    /// <summary>Height scaling factor (0.90–1.10). Affects reach and SVG scale.</summary>
    public float HeightFactor { get; set; } = 1.0f;

    // ── Stamina & sprint ──────────────────────────────────────────────────────
    public float Stamina    { get; set; }
    /// <summary>True while the sprint key is held — drains stamina, boosts all action speeds.</summary>
    public bool  SprintHeld { get; set; }

    // ── Anti-spam cooldown ────────────────────────────────────────────────────
    /// <summary>
    /// Counts down (ms) after landing a hit.  While > 0 the boxer cannot start a new
    /// attack even though they are in Idle/Moving — prevents punch-spam after a hit.
    /// </summary>
    public float PostHitCooldownMs { get; set; }

    // ── Pre-input defense (applied the moment Hurt ends) ─────────────────────
    /// <summary>Block key was held during the Hurt window; activate Defending immediately on recovery.</summary>
    public bool PendingDefenseHeld { get; set; }

    public void Reset(float startX, Side stance)
    {
        PositionX          = startX;
        HeadHealth         = Stats.MaxHeadHealth;
        BodyHealth         = Stats.MaxBodyHealth;
        Health             = Stats.MaxHealth;
        Stamina            = Stats.MaxStamina;
        State              = BoxerStateKind.Idle;
        CurrentStrike      = null;
        CurrentDefense     = null;
        CurrentPhase       = null;
        PhaseElapsedMs     = 0f;
        HitResolved        = false;
        HurtElapsedMs      = 0f;
        KnockdownElapsedMs = 0f;
        KnockdownCount     = 0;
        IdleAnimMs         = 0f;
        SprintHeld         = false;
        PostHitCooldownMs  = 0f;
        PendingDefenseHeld = false;
        WalkCycleMs        = 0f;
        DodgePhase         = null;
        DodgeElapsedMs     = 0f;
        DefenseElapsedMs   = 0f;
    }
}
