using BoxingGame.Domain.Boxers;
using BoxingGame.Domain.Combat;

namespace BoxingGame.Domain.Strikes;

/// <summary>
/// Rear-hand uppercut.  The power hand loads deeply from below and drives into
/// the chin with maximum force — the most damaging punch in the game.
/// Slower than the lead uppercut and even more telegraphed.
/// Executing an uppercut greatly limits walking speed (WalkSpeedMultiplier = 0.25).
/// </summary>
public sealed class RightUppercut : Strike
{
    public static readonly RightUppercut Instance = new();

    public override string Name        => "RightUppercut";
    public override Side   Hand        => Side.Right;
    public override int    StartupMs   => 310;   // rear-hand: slower, more powerful
    public override int    ActiveMs    => 55;
    public override int    RecoveryMs  => 295;
    public override int    ScoreValue  => 5;
    public override float  BaseRangePx => 82f;   // close-range power punch
    public override float  Damage      => 26f;
    public override BodyLocation Target => BodyLocation.Head;

    // Rear hand loads deeper — very obvious tell.
    public override int   TelegraphMs        => 130;
    public override float TelegraphMagnitude => 1.05f;   // 12.6° peak rotation (past maximum)

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
            // t=0.22 — rear arm begins to drop away from chin
            new Keyframe(0.22f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new ArmPose(-8f, -66f, 6f, -58f),
                Head:  new HeadPose(-2f, 3f)),
            // t=0.58 — fully loaded, rear fist at hip level
            new Keyframe(0.58f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new ArmPose(-10f, -52f, 4f, -40f),
                Head:  new HeadPose(-3f, 5f)),
            // t=1.00 — maximum load, body torqued, ready to explode upward
            new Keyframe(1f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new ArmPose(-12f, -48f, 2f, -36f),
                Head:  new HeadPose(-4f, 5f))
        },
        Active: new[]
        {
            // t=0.0 — rear fist drives upward
            new Keyframe(0f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new ArmPose(-14f, -58f, 6f, -68f),
                Head:  new HeadPose(2f, 2f)),
            // t=0.5 — power hand rising, body rotating through the punch
            new Keyframe(0.5f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new ArmPose(-16f, -70f, 10f, -82f),
                Head:  new HeadPose(5f, 1f)),
            // t=1.0 — impact: rear fist connects from below the chin
            new Keyframe(1f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new ArmPose(-18f, -76f, 14f, -96f),
                Head:  new HeadPose(8f, 0f))
        },
        Recovery: new[]
        {
            // t=0.00 — at impact
            new Keyframe(0f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new ArmPose(-18f, -76f, 14f, -96f),
                Head:  new HeadPose(8f, 0f)),
            // t=0.28 — arm settling back down
            new Keyframe(0.28f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new ArmPose(-14f, -78f, 12f, -88f),
                Head:  new HeadPose(4f, 1f)),
            // t=0.62 — halfway back to guard
            new Keyframe(0.62f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new ArmPose(-8f, -76f, 8f, -84f),
                Head:  new HeadPose(2f, 1f)),
            // t=1.00 — back to guard
            new Keyframe(1f,
                StrikeAnimationFrames.GuardLeft,
                StrikeAnimationFrames.GuardRight,
                StrikeAnimationFrames.HeadNeutral)
        }
    );
}
