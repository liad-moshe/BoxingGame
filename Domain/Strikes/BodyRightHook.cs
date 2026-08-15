using BoxingGame.Domain.Boxers;
using BoxingGame.Domain.Combat;

namespace BoxingGame.Domain.Strikes;

/// <summary>
/// Rear-hand hook to the body.  Slightly slower than the lead-hand body hook
/// but hits harder.  Four-stage startup on the right side; three-stage arc.
/// Bypasses the high guard.
/// </summary>
public sealed class BodyRightHook : Strike
{
    public static readonly BodyRightHook Instance = new();

    public override string Name        => "BodyRightHook";
    public override Side   Hand        => Side.Right;
    public override int    StartupMs   => 230;
    public override int    ActiveMs    => 56;
    public override int    RecoveryMs  => 268;
    public override int    ScoreValue  => 2;
    public override float  BaseRangePx => 88f;
    public override float  Damage      => 18f;
    public override BodyLocation Target => BodyLocation.Body;

    // Rear-hand body hook: largest body punch, most coil needed before firing.
    public override int   TelegraphMs        => 80;
    public override float TelegraphMagnitude => 0.70f;  // 8.4° peak rotation

    public override StrikeAnimationFrames Frames { get; } = new(
        Startup: new[]
        {
            // t=0.00 — guard
            new Keyframe(0f,
                StrikeAnimationFrames.GuardLeft,
                StrikeAnimationFrames.GuardRight,
                StrikeAnimationFrames.HeadNeutral),
            // t=0.22 — right shoulder loads and dips toward hip
            new Keyframe(0.22f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(8f, -83f, 18f, -80f),
                Head:  new(-3f, 3f)),
            // t=0.58 — elbow cocks wide right and low
            new Keyframe(0.58f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(32f, -82f, 10f, -75f),
                Head:  new(-4f, 3f)),
            // t=1.00 — fully cocked at body height
            new Keyframe(1f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(40f, -80f, 12f, -70f),
                Head:  new(-5f, 3f))
        },
        Active: new[]
        {
            // t=0.0 — start of body-level swing
            new Keyframe(0f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(37f, -77f, 18f, -72f),
                Head:  new(3f, 2f)),
            // t=0.5 — mid-arc at rib height
            new Keyframe(0.5f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(37f, -76f, 42f, -68f),
                Head:  new(6f, 2f)),
            // t=1.0 — impact
            new Keyframe(1f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(37f, -76f, 60f, -68f),
                Head:  new(9f, 2f))
        },
        Recovery: new[]
        {
            // t=0.00 — post-impact
            new Keyframe(0f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(37f, -76f, 60f, -68f),
                Head:  new(9f, 2f)),
            // t=0.30 — arm follows through low
            new Keyframe(0.30f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(24f, -77f, 44f, -70f),
                Head:  new(5f, 2f)),
            // t=0.65 — arm pulling back up
            new Keyframe(0.65f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(6f, -80f, 20f, -78f),
                Head:  new(2f, 1f)),
            // t=1.00 — guard
            new Keyframe(1f,
                StrikeAnimationFrames.GuardLeft,
                StrikeAnimationFrames.GuardRight,
                StrikeAnimationFrames.HeadNeutral)
        }
    );
}
