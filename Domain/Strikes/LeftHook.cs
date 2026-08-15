using BoxingGame.Domain.Boxers;
using BoxingGame.Domain.Combat;

namespace BoxingGame.Domain.Strikes;

/// <summary>
/// Lead-hand hook.  Slower and shorter-range than a jab but deals much more damage.
/// Four-stage startup: guard → shoulder dip (loading) → elbow cocking wide → full cock.
/// Active arc: three keyframes show the glove swinging through mid-arc to impact.
/// Recovery follows through then snaps back.
///
/// Range: GloveX 60 at Active + glove radius 10 + head radius 18 = 88 px
/// </summary>
public sealed class LeftHook : Strike
{
    public static readonly LeftHook Instance = new();

    public override string Name        => "LeftHook";
    public override Side   Hand        => Side.Left;
    public override int    StartupMs   => 220;   // slow enough to react to
    public override int    ActiveMs    => 56;
    public override int    RecoveryMs  => 260;
    public override int    ScoreValue  => 4;
    public override float  BaseRangePx => 88f;
    public override float  Damage      => 20f;
    public override BodyLocation Target => BodyLocation.Head;

    // 90 ms loading tell — the first 41 % of Startup is pure body coil with no
    // arm travel.  At 12° × 0.85 the shoulder drop is clearly visible.
    public override int   TelegraphMs        => 90;
    public override float TelegraphMagnitude => 0.85f;  // 10.2° peak rotation

    public override StrikeAnimationFrames Frames { get; } = new(
        Startup: new[]
        {
            // t=0.00 — guard
            new Keyframe(0f,
                StrikeAnimationFrames.GuardLeft,
                StrikeAnimationFrames.GuardRight,
                StrikeAnimationFrames.HeadNeutral),
            // t=0.22 — shoulder dips, body loads (head bobs down with weight shift)
            new Keyframe(0.22f,
                Left:  new(2f, -84f, 24f, -86f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(3f, 3f)),
            // t=0.58 — elbow flies wide to the left, body pivoting
            new Keyframe(0.58f,
                Left:  new(-26f, -88f, -4f, -87f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(5f, 4f)),
            // t=1.00 — fully cocked, maximum coil
            new Keyframe(1f,
                Left:  new(-38f, -90f, -12f, -88f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(6f, 4f))
        },
        Active: new[]
        {
            // t=0.0 — start of arc: elbow wide, glove just beginning to swing forward
            new Keyframe(0f,
                Left:  new(-36f, -86f, 18f, -92f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(-3f, 3f)),
            // t=0.5 — mid-arc, glove passing through centerline
            new Keyframe(0.5f,
                Left:  new(-36f, -86f, 44f, -94f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(-7f, 2f)),
            // t=1.0 — impact position
            new Keyframe(1f,
                Left:  new(-36f, -86f, 60f, -94f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(-10f, 2f))
        },
        Recovery: new[]
        {
            // t=0.00 — post-impact
            new Keyframe(0f,
                Left:  new(-36f, -86f, 60f, -94f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(-10f, 2f)),
            // t=0.30 — arm following through, pulling around
            new Keyframe(0.30f,
                Left:  new(-24f, -87f, 48f, -92f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(-6f, 2f)),
            // t=0.65 — arm pulling back to neutral
            new Keyframe(0.65f,
                Left:  new(4f, -85f, 24f, -86f),
                Right: StrikeAnimationFrames.GuardRight,
                Head:  new(-3f, 1f)),
            // t=1.00 — back to guard
            new Keyframe(1f,
                StrikeAnimationFrames.GuardLeft,
                StrikeAnimationFrames.GuardRight,
                StrikeAnimationFrames.HeadNeutral)
        }
    );
}
