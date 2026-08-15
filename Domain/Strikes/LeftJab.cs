using BoxingGame.Domain.Boxers;
using BoxingGame.Domain.Combat;

namespace BoxingGame.Domain.Strikes;

/// <summary>
/// Lead-hand (left) jab.  Fast and long but low damage.
/// The startup phase shows three visible stages:
///   1) shoulder rotates forward, elbow advances
///   2) arm 60 % extended, weight shifting
///   3) arm 80 % out, near full reach
/// The recovery whips the elbow back first before the glove retracts — giving
/// the classic "snap" look of a real jab.
/// </summary>
public sealed class LeftJab : Strike
{
    public static readonly LeftJab Instance = new();

    public override string Name => "LeftJab";
    public override Side Hand => Side.Left;
    public override int StartupMs  => 62;
    public override int ActiveMs   => 40;
    public override int RecoveryMs => 130;
    public override int ScoreValue => 2;
    public override float BaseRangePx => 118f;
    public override float Damage => 8f;
    public override BodyLocation Target => BodyLocation.Head;

    public override StrikeAnimationFrames Frames { get; } = new(
        Startup: new[]
        {
            // t=0.00 — guard (replaced at runtime with boxer's actual guard pose)
            new Keyframe(0f,   StrikeAnimationFrames.GuardLeft, StrikeAnimationFrames.GuardRight, StrikeAnimationFrames.HeadNeutral),
            // t=0.30 — shoulder rotates forward, elbow advances, slight hip weight-shift
            new Keyframe(0.30f, new(10f, -80f, 22f, -85f), StrikeAnimationFrames.GuardRight, new(-2f, 1f)),
            // t=0.65 — arm 60 % extended; head turns toward target
            new Keyframe(0.65f, new(20f, -87f, 62f, -90f), StrikeAnimationFrames.GuardRight, new(-4f, 2f)),
            // t=1.00 — arm 80 % extended, almost at full reach
            new Keyframe(1f,    new(23f, -89f, 78f, -92f), StrikeAnimationFrames.GuardRight, new(-5f, 2f))
        },
        Active: new[]
        {
            // Full extension held — three keyframes so the glove visibly "presses"
            new Keyframe(0f,   new(25f, -90f, 90f, -94f), StrikeAnimationFrames.GuardRight, new(-6f, 3f)),
            new Keyframe(0.5f, new(25f, -90f, 90f, -94f), StrikeAnimationFrames.GuardRight, new(-6f, 3f)),
            new Keyframe(1f,   new(25f, -90f, 90f, -94f), StrikeAnimationFrames.GuardRight, new(-6f, 3f))
        },
        Recovery: new[]
        {
            // t=0.00 — still fully extended
            new Keyframe(0f,    new(25f, -90f, 90f, -94f), StrikeAnimationFrames.GuardRight, new(-6f, 3f)),
            // t=0.35 — elbow snaps back first ("whip" retraction), glove still forward
            new Keyframe(0.35f, new(18f, -88f, 62f, -91f), StrikeAnimationFrames.GuardRight, new(-4f, 2f)),
            // t=0.70 — arm rapidly retracting
            new Keyframe(0.70f, new(10f, -83f, 28f, -86f), StrikeAnimationFrames.GuardRight, new(-2f, 1f)),
            // t=1.00 — back to guard
            new Keyframe(1f,    StrikeAnimationFrames.GuardLeft, StrikeAnimationFrames.GuardRight, StrikeAnimationFrames.HeadNeutral)
        }
    );
}
