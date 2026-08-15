namespace BoxingGame.Domain.Boxers;

public class BoxerStats
{
    public float MoveSpeedPxPerSec    { get; init; } = 280f;   // was 200
    public float ReachMultiplier      { get; init; } = 1.0f;
    public float MaxHealth            { get; init; } = 100f;

    /// <summary>Head sub-pool: 65 % of total HP. Cuts and knockdowns typically target the head.</summary>
    public float MaxHeadHealth => MaxHealth * 0.65f;
    /// <summary>Body sub-pool: 35 % of total HP. Body shots wear fighters down.</summary>
    public float MaxBodyHealth => MaxHealth * 0.35f;

    /// <summary>
    /// Short enough that the defender is out of Hurt well before the attacker can
    /// throw a second punch (attacker's PostHitCooldown + Recovery covers the gap).
    /// </summary>
    public float HurtRecoveryMs       { get; init; } = 120f;   // was 180

    // ── Stamina ──────────────────────────────────────────────────────────────
    public float MaxStamina            { get; init; } = 100f;
    /// <summary>Stamina recovered per second when the sprint key is NOT held.</summary>
    public float StaminaRecoveryPerSec { get; init; } = 25f;
    /// <summary>Stamina drained per second while the sprint key IS held.</summary>
    public float SprintDrainPerSec     { get; init; } = 50f;
    /// <summary>Speed multiplier applied to all actions when sprint is active (pre-stamina factor).</summary>
    public float SprintSpeedMultiplier { get; init; } = 1.5f;
}
