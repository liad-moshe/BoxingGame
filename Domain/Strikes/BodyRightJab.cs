using BoxingGame.Domain.Boxers;
using BoxingGame.Domain.Combat;

namespace BoxingGame.Domain.Strikes;

/// <summary>
/// Right-hand jab aimed at the body.  Slightly slower than the lead-hand body jab
/// but harder-hitting.  Bypasses the high-guard block.
/// Three-stage startup mirrors the left-body jab on the right side.
/// </summary>
public sealed class BodyRightJab : Strike
{
    public static readonly BodyRightJab Instance = new();

    public override string Name        => "BodyRightJab";
    public override Side   Hand        => Side.Right;
    public override int    StartupMs   => 76;
    public override int    ActiveMs    => 40;
    public override int    RecoveryMs  => 148;
    public override int    ScoreValue  => 1;
    public override float  BaseRangePx => 118f;
    public override float  Damage      => 8f;
    public override BodyLocation Target => BodyLocation.Body;

    public override StrikeAnimationFrames Frames { get; } = new(
        Startup: new[]
        {
            // t=0.00 — guard
            new Keyframe(0f,    StrikeAnimationFrames.GuardLeft, StrikeAnimationFrames.GuardRight, StrikeAnimationFrames.HeadNeutral),
            // t=0.30 — right arm starts dipping toward body level
            new Keyframe(0.30f, StrikeAnimationFrames.GuardLeft, new(3f,  -79f, 16f, -76f), new(2f, 1f)),
            // t=0.65 — arm half-extended at body level
            new Keyframe(0.65f, StrikeAnimationFrames.GuardLeft, new(16f, -80f, 52f, -70f), new(3f, 2f)),
            // t=1.00 — arm 80 % extended at body height
            new Keyframe(1f,    StrikeAnimationFrames.GuardLeft, new(20f, -80f, 72f, -67f), new(4f, 2f))
        },
        Active: new[]
        {
            new Keyframe(0f,   StrikeAnimationFrames.GuardLeft, new(20f, -80f, 90f, -66f), new(4f, 2f)),
            new Keyframe(0.5f, StrikeAnimationFrames.GuardLeft, new(20f, -80f, 90f, -66f), new(4f, 2f)),
            new Keyframe(1f,   StrikeAnimationFrames.GuardLeft, new(20f, -80f, 90f, -66f), new(4f, 2f))
        },
        Recovery: new[]
        {
            new Keyframe(0f,    StrikeAnimationFrames.GuardLeft, new(20f, -80f, 90f, -66f), new(4f, 2f)),
            // t=0.35 — elbow snaps back first
            new Keyframe(0.35f, StrikeAnimationFrames.GuardLeft, new(14f, -80f, 55f, -68f), new(2f, 1f)),
            // t=0.70 — arm retracting
            new Keyframe(0.70f, StrikeAnimationFrames.GuardLeft, new(6f,  -79f, 24f, -74f), new(1f, 0f)),
            // t=1.00 — guard
            new Keyframe(1f,    StrikeAnimationFrames.GuardLeft, StrikeAnimationFrames.GuardRight, StrikeAnimationFrames.HeadNeutral)
        }
    );
}
