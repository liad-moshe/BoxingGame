using BoxingGame.Domain.Combat;
using BoxingGame.Domain.Strikes;

namespace BoxingGame.Domain.Defenses;

/// <summary>
/// Boxer crouches: head drops ~70 px below its normal position, moving it
/// below the horizontal path of any head-targeted strike.
/// Does NOT block — it evades.  Body shots (future) will still land.
/// </summary>
public sealed class Duck : Defense
{
    public static readonly Duck Instance = new();

    public override string Name => "Duck";

    // Head offset +70 → visual head-centre Y = -120 + 70 = -50.
    // The renderer applies crouchDY = 70 × 0.3 = 21 to body geometry so the whole figure
    // crouches.  Arm poses are specified BEFORE the crouchDY shift; after the shift:
    //   elbow Y = -79 + 21 = -58  (just below shoulder at -67)
    //   glove  Y = -67 + 21 = -46  (≈ head level at -50) → gloves flank the ducked head ✓
    // Jab glove Y ≈ -94  →  gap |(-94) − (-50)| = 44 > gloveR(10)+headR(18) = 28 → Miss ✓
    public override DefenseAnimationFrames Frames { get; } = new(
        Hold: new Keyframe(
            T: 0f,
            Left:  new(-12f, -79f, -18f, -67f),   // left arm raised, flanking the head
            Right: new( 12f, -79f,  18f, -67f),   // right arm raised, flanking the head
            Head:  new(0f, 70f))                   // head drops 70 px
    );

    // Duck doesn't absorb damage — it moves the head out of the way.
    public override DefenseEffectiveness EvaluateAgainst(Strike incoming) =>
        DefenseEffectiveness.None;

    // Head is below the travel path of any head-targeted punch → geometric miss.
    public override bool EvadesStrike(Strike incoming) =>
        incoming.Target == BodyLocation.Head;
}
