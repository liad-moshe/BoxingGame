using BoxingGame.Domain.Boxers;
using BoxingGame.Domain.Combat;

namespace BoxingGame.Domain.Strikes;

/// <summary>
/// Lead-hand uppercut.  The arm loads downward (body dips into the punch),
/// then drives explosively upward into the opponent's chin.
/// Highly telegraphed — the arm drop is visibly readable.
/// Executing an uppercut greatly limits walking speed (WalkSpeedMultiplier = 0.25).
///
/// Range is close (must be inside hook range): effective reach is short,
/// making this a power punch for when you are already inside.
/// </summary>
public sealed class LeftUppercut : Strike
{
    public static readonly LeftUppercut Instance = new();

    public override string Name        => "LeftUppercut";
    public override Side   Hand        => Side.Left;
    public override int    StartupMs   => 280;   // slow load — very readable
    public override int    ActiveMs    => 55;
    public override int    RecoveryMs  => 270;
    public override int    ScoreValue  => 5;
    public override float  BaseRangePx => 84f;   // close-range power punch
    public override float  Damage      => 22f;
    public override BodyLocation Target => BodyLocation.Head;

    // The arm-drop loading tell is very obvious.
    public override int   TelegraphMs        => 110;
    public override float TelegraphMagnitude => 0.95f;   // 11.4° peak rotation

    // Uppercut greatly restricts walking — plant and punch.
    public override float WalkSpeedMultiplier => 0.25f;

    public override StrikeAnimationFrames Frames { get; } = new(
        Startup: new[]
        {
            // t=0.00 — guard
            new Keyframe(0f,
                StrikeAnimationFrames.GuardLeft,
                StrikeAnimationFrames.GuardRight,
                StrikeAnimationFrames.HeadNeutral),
            // t=0.22 — arm begins to drop, body starts loading downward
            new Keyframe(0.22f,
                Left:  new ArmPose(8f, -66f, 26f, -58f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new HeadPose(2f, 3f)),
            // t=0.58 — arm fully loaded at waist level, body coiled
            new Keyframe(0.58f,
                Left:  new ArmPose(10f, -52f, 22f, -40f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new HeadPose(3f, 5f)),
            // t=1.00 — maximum load, fist tight at hip, ready to drive
            new Keyframe(1f,
                Left:  new ArmPose(12f, -48f, 20f, -36f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new HeadPose(4f, 5f))
        },
        Active: new[]
        {
            // t=0.0 — fist starts driving upward from low position
            new Keyframe(0f,
                Left:  new ArmPose(14f, -58f, 26f, -68f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new HeadPose(-2f, 2f)),
            // t=0.5 — arm rising fast, elbow coming up
            new Keyframe(0.5f,
                Left:  new ArmPose(16f, -70f, 30f, -82f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new HeadPose(-5f, 1f)),
            // t=1.0 — impact: fist drives into chin from below
            new Keyframe(1f,
                Left:  new ArmPose(18f, -76f, 36f, -95f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new HeadPose(-8f, 0f))
        },
        Recovery: new[]
        {
            // t=0.00 — at impact, arm extended high
            new Keyframe(0f,
                Left:  new ArmPose(18f, -76f, 36f, -95f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new HeadPose(-8f, 0f)),
            // t=0.28 — arm pulling back down
            new Keyframe(0.28f,
                Left:  new ArmPose(14f, -78f, 32f, -88f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new HeadPose(-4f, 1f)),
            // t=0.62 — halfway back to guard
            new Keyframe(0.62f,
                Left:  new ArmPose(8f, -76f, 28f, -84f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new HeadPose(-2f, 1f)),
            // t=1.00 — back to guard
            new Keyframe(1f,
                StrikeAnimationFrames.GuardLeft,
                StrikeAnimationFrames.GuardRight,
                StrikeAnimationFrames.HeadNeutral)
        }
    );
}
