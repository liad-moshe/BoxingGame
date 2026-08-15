using BoxingGame.Domain.Boxers;
using BoxingGame.Domain.Combat;

namespace BoxingGame.Domain.Strikes;

public abstract class Strike
{
    public abstract string Name { get; }
    public abstract Side Hand { get; }

    // Phase durations in milliseconds.
    public abstract int StartupMs { get; }
    public abstract int ActiveMs  { get; }
    public abstract int RecoveryMs { get; }

    // Range and damage.
    public abstract float BaseRangePx { get; }
    public abstract float Damage { get; }
    public abstract BodyLocation Target { get; }

    public abstract StrikeAnimationFrames Frames { get; }

    /// <summary>
    /// Judging points awarded when this punch lands.
    /// Head hook = 4, head jab = 2, body hook = 2, body jab = 1.
    /// </summary>
    public abstract int ScoreValue { get; }

    /// <summary>
    /// How many milliseconds at the START of Startup show a visible body-loading tell
    /// before the arm actually travels.  0 = no telegraph (fast jabs).
    /// The tell is rendered as a backward upper-body rotation in BoxerSvg.
    /// </summary>
    public virtual int TelegraphMs => 0;

    /// <summary>
    /// Peak rotation magnitude for the loading tell (0..1 maps to 0°..12°).
    /// 0 = invisible; 1 = very obvious coil.  Combined with TelegraphMs to control
    /// how "readable" a punch is to an attentive opponent.
    /// </summary>
    public virtual float TelegraphMagnitude => 0f;

    /// <summary>
    /// Multiplier applied to walk speed while this strike is executing (all phases).
    /// 1.0 = full speed (jabs, hooks). 0.25 = very slow shuffle (uppercuts).
    /// </summary>
    public virtual float WalkSpeedMultiplier => 1.0f;

    public int TotalMs => StartupMs + ActiveMs + RecoveryMs;
}
