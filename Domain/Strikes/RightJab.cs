using BoxingGame.Domain.Boxers;
using BoxingGame.Domain.Combat;

namespace BoxingGame.Domain.Strikes;

/// <summary>
/// Rear-hand (right) jab — slightly slower and harder than the left jab.
/// Same three-stage startup: shoulder load → arm extends 60 % → arm 80 %.
/// Recovery whips the elbow back before the glove retracts.
/// </summary>
public sealed class RightJab : Strike
{
    public static readonly RightJab Instance = new();

    public override string Name => "RightJab";
    public override Side Hand => Side.Right;
    public override int StartupMs  => 76;
    public override int ActiveMs   => 40;
    public override int RecoveryMs => 148;
    public override int ScoreValue => 2;
    public override float BaseRangePx => 118f;
    public override float Damage => 10f;
    public override BodyLocation Target => BodyLocation.Head;

    public override StrikeAnimationFrames Frames { get; } = new(
        Startup: new[]
        {
            // t=0.00 — guard
            new Keyframe(0f,    StrikeAnimationFrames.GuardLeft, StrikeAnimationFrames.GuardRight, StrikeAnimationFrames.HeadNeutral),
            // t=0.30 — right shoulder loads, elbow starts moving
            new Keyframe(0.30f, StrikeAnimationFrames.GuardLeft, new(3f, -79f, 16f, -82f),  new(2f, 1f)),
            // t=0.65 — arm 55 % extended, head turns right
            new Keyframe(0.65f, StrikeAnimationFrames.GuardLeft, new(18f, -86f, 60f, -89f), new(4f, 2f)),
            // t=1.00 — arm 80 % extended
            new Keyframe(1f,    StrikeAnimationFrames.GuardLeft, new(21f, -88f, 78f, -92f), new(5f, 2f))
        },
        Active: new[]
        {
            new Keyframe(0f,   StrikeAnimationFrames.GuardLeft, new(22f, -90f, 90f, -94f), new(6f, 3f)),
            new Keyframe(0.5f, StrikeAnimationFrames.GuardLeft, new(22f, -90f, 90f, -94f), new(6f, 3f)),
            new Keyframe(1f,   StrikeAnimationFrames.GuardLeft, new(22f, -90f, 90f, -94f), new(6f, 3f))
        },
        Recovery: new[]
        {
            new Keyframe(0f,    StrikeAnimationFrames.GuardLeft, new(22f, -90f, 90f, -94f), new(6f, 3f)),
            // t=0.35 — elbow snaps back, glove still extended
            new Keyframe(0.35f, StrikeAnimationFrames.GuardLeft, new(16f, -87f, 60f, -91f), new(4f, 2f)),
            // t=0.70 — arm rapidly retracting
            new Keyframe(0.70f, StrikeAnimationFrames.GuardLeft, new(8f,  -82f, 26f, -85f), new(2f, 1f)),
            // t=1.00 — guard
            new Keyframe(1f,    StrikeAnimationFrames.GuardLeft, StrikeAnimationFrames.GuardRight, StrikeAnimationFrames.HeadNeutral)
        }
    );
}
