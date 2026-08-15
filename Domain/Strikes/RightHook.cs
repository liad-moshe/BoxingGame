using BoxingGame.Domain.Boxers;
using BoxingGame.Domain.Combat;

namespace BoxingGame.Domain.Strikes;

/// <summary>
/// Rear-hand hook.  Slightly slower and harder than the left hook.
/// Four-stage startup mirrors the left hook on the right side.
/// Active arc shows three stages of the glove swinging through.
/// </summary>
public sealed class RightHook : Strike
{
    public static readonly RightHook Instance = new();

    public override string Name        => "RightHook";
    public override Side   Hand        => Side.Right;
    public override int    StartupMs   => 250;   // heaviest punch — most wind-up
    public override int    ActiveMs    => 56;
    public override int    RecoveryMs  => 280;
    public override int    ScoreValue  => 4;
    public override float  BaseRangePx => 88f;
    public override float  Damage      => 22f;
    public override BodyLocation Target => BodyLocation.Head;

    // 110 ms loading tell — the rear-hand hook is the heaviest punch and the most
    // readable.  44 % of Startup is pure body coil with a large shoulder rotation.
    public override int   TelegraphMs        => 110;
    public override float TelegraphMagnitude => 0.95f;  // 11.4° peak rotation

    public override StrikeAnimationFrames Frames { get; } = new(
        Startup: new[]
        {
            // t=0.00 — guard
            new Keyframe(0f,
                StrikeAnimationFrames.GuardLeft,
                StrikeAnimationFrames.GuardRight,
                StrikeAnimationFrames.HeadNeutral),
            // t=0.22 — right shoulder dips, body loading
            new Keyframe(0.22f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(8f, -84f, 20f, -86f),
                Head:  new(-3f, 3f)),
            // t=0.58 — elbow flies wide to the right, body pivoting
            new Keyframe(0.58f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(32f, -88f, 16f, -87f),
                Head:  new(-4f, 4f)),
            // t=1.00 — fully cocked
            new Keyframe(1f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(40f, -90f, 14f, -88f),
                Head:  new(-6f, 4f))
        },
        Active: new[]
        {
            // t=0.0 — start of arc
            new Keyframe(0f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(38f, -86f, 18f, -92f),
                Head:  new(3f, 3f)),
            // t=0.5 — mid-arc
            new Keyframe(0.5f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(38f, -86f, 42f, -94f),
                Head:  new(7f, 2f)),
            // t=1.0 — impact
            new Keyframe(1f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(38f, -86f, 60f, -94f),
                Head:  new(10f, 2f))
        },
        Recovery: new[]
        {
            // t=0.00 — post-impact
            new Keyframe(0f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(38f, -86f, 60f, -94f),
                Head:  new(10f, 2f)),
            // t=0.30 — arm follows through
            new Keyframe(0.30f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(26f, -87f, 46f, -92f),
                Head:  new(6f, 2f)),
            // t=0.65 — arm pulling back
            new Keyframe(0.65f,
                Left:  StrikeAnimationFrames.GuardLeft,
                Right: new(8f, -85f, 20f, -86f),
                Head:  new(3f, 1f)),
            // t=1.00 — back to guard
            new Keyframe(1f,
                StrikeAnimationFrames.GuardLeft,
                StrikeAnimationFrames.GuardRight,
                StrikeAnimationFrames.HeadNeutral)
        }
    );
}
