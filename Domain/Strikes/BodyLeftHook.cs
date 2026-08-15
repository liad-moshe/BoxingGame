using BoxingGame.Domain.Boxers;
using BoxingGame.Domain.Combat;

namespace BoxingGame.Domain.Strikes;

/// <summary>
/// Lead-hand hook to the body.  The arm cocks wide and LOW, then swings in at
/// rib/liver height in a visible three-stage arc.
/// Close-range, high damage, completely bypasses the high guard.
/// </summary>
public sealed class BodyLeftHook : Strike
{
    public static readonly BodyLeftHook Instance = new();

    public override string Name        => "BodyLeftHook";
    public override Side   Hand        => Side.Left;
    public override int    StartupMs   => 205;
    public override int    ActiveMs    => 56;
    public override int    RecoveryMs  => 255;
    public override int    ScoreValue  => 2;
    public override float  BaseRangePx => 88f;
    public override float  Damage      => 16f;
    public override BodyLocation Target => BodyLocation.Body;

    // Body hooks are partially disguised by the downward dip — shorter window and
    // smaller rotation than head hooks, but still readable to an attentive opponent.
    public override int   TelegraphMs        => 68;
    public override float TelegraphMagnitude => 0.62f;  // 7.4° peak rotation

    public override StrikeAnimationFrames Frames { get; } = new(
        Startup: new[]
        {
            // t=0.00 — guard
            new Keyframe(0f,
                StrikeAnimationFrames.GuardLeft,
                StrikeAnimationFrames.GuardRight,
                StrikeAnimationFrames.HeadNeutral),
            // t=0.22 — shoulder loads and dips lower — arm bends toward hip
            new Keyframe(0.22f,
                Left:  new(2f, -83f, 22f, -80f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(3f, 3f)),
            // t=0.58 — elbow cocks wide and low, aimed at body level
            new Keyframe(0.58f,
                Left:  new(-24f, -82f, -8f, -75f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(4f, 3f)),
            // t=1.00 — fully cocked at body height
            new Keyframe(1f,
                Left:  new(-38f, -80f, -10f, -70f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(5f, 3f))
        },
        Active: new[]
        {
            // t=0.0 — start of body-level swing
            new Keyframe(0f,
                Left:  new(-35f, -77f, 18f, -72f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(-3f, 2f)),
            // t=0.5 — mid-arc at rib height
            new Keyframe(0.5f,
                Left:  new(-35f, -76f, 42f, -68f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(-6f, 2f)),
            // t=1.0 — impact at rib/liver
            new Keyframe(1f,
                Left:  new(-35f, -76f, 60f, -68f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(-8f, 2f))
        },
        Recovery: new[]
        {
            // t=0.00 — post-impact
            new Keyframe(0f,
                Left:  new(-35f, -76f, 60f, -68f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(-8f, 2f)),
            // t=0.30 — arm follows through at low level
            new Keyframe(0.30f,
                Left:  new(-22f, -77f, 44f, -70f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(-4f, 2f)),
            // t=0.65 — arm pulling back up
            new Keyframe(0.65f,
                Left:  new(3f, -80f, 22f, -78f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(-2f, 1f)),
            // t=1.00 — guard
            new Keyframe(1f,
                StrikeAnimationFrames.GuardLeft,
                StrikeAnimationFrames.GuardRight,
                StrikeAnimationFrames.HeadNeutral)
        }
    );
}
