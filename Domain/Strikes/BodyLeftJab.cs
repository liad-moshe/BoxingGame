using BoxingGame.Domain.Boxers;
using BoxingGame.Domain.Combat;

namespace BoxingGame.Domain.Strikes;

/// <summary>
/// Left-hand jab aimed at the body (torso) instead of the head.
/// Three-stage startup: arm starts dropping toward body level, then extends
/// diagonally downward, then reaches near-full low extension.
/// Bypasses the BothHandsBlock high guard entirely.
/// </summary>
public sealed class BodyLeftJab : Strike
{
    public static readonly BodyLeftJab Instance = new();

    public override string Name        => "BodyLeftJab";
    public override Side   Hand        => Side.Left;
    public override int    StartupMs   => 62;
    public override int    ActiveMs    => 40;
    public override int    RecoveryMs  => 130;
    public override int    ScoreValue  => 1;
    public override float  BaseRangePx => 118f;
    public override float  Damage      => 6f;
    public override BodyLocation Target => BodyLocation.Body;

    public override StrikeAnimationFrames Frames { get; } = new(
        Startup: new[]
        {
            // t=0.00 — guard
            new Keyframe(0f,    StrikeAnimationFrames.GuardLeft,  StrikeAnimationFrames.GuardRight, StrikeAnimationFrames.HeadNeutral),
            // t=0.30 — arm starts dipping, shoulder loads downward
            new Keyframe(0.30f, new(10f, -81f, 22f, -78f), StrikeAnimationFrames.GuardRight, new(-2f, 1f)),
            // t=0.65 — arm half-extended at body level, travelling diagonally
            new Keyframe(0.65f, new(18f, -82f, 55f, -72f), StrikeAnimationFrames.GuardRight, new(-3f, 2f)),
            // t=1.00 — arm 80 % extended, glove near target body height
            new Keyframe(1f,    new(22f, -82f, 72f, -67f), StrikeAnimationFrames.GuardRight, new(-4f, 2f))
        },
        Active: new[]
        {
            new Keyframe(0f,   new(22f, -82f, 90f, -66f), StrikeAnimationFrames.GuardRight, new(-4f, 2f)),
            new Keyframe(0.5f, new(22f, -82f, 90f, -66f), StrikeAnimationFrames.GuardRight, new(-4f, 2f)),
            new Keyframe(1f,   new(22f, -82f, 90f, -66f), StrikeAnimationFrames.GuardRight, new(-4f, 2f))
        },
        Recovery: new[]
        {
            new Keyframe(0f,    new(22f, -82f, 90f, -66f), StrikeAnimationFrames.GuardRight, new(-4f, 2f)),
            // t=0.35 — elbow snaps back first
            new Keyframe(0.35f, new(16f, -81f, 56f, -68f), StrikeAnimationFrames.GuardRight, new(-2f, 1f)),
            // t=0.70 — arm rapidly retracting
            new Keyframe(0.70f, new(8f,  -80f, 24f, -75f), StrikeAnimationFrames.GuardRight, new(-1f, 0f)),
            // t=1.00 — guard
            new Keyframe(1f,    StrikeAnimationFrames.GuardLeft, StrikeAnimationFrames.GuardRight, StrikeAnimationFrames.HeadNeutral)
        }
    );
}
