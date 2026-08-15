using BoxingGame.Domain.Combat;
using BoxingGame.Domain.Strikes;

namespace BoxingGame.Domain.Defenses;

public sealed class BothHandsBlock : Defense
{
    public static readonly BothHandsBlock Instance = new();

    public override string Name => "BothHandsBlock";

    public override DefenseAnimationFrames Frames { get; } = new(
        Hold: new Keyframe(0f, new(-8f, -100f, -5f, -115f), new(8f, -100f, 5f, -115f), new(0f, 0f))
    );

    public override DefenseEffectiveness EvaluateAgainst(Strike incoming) =>
        incoming.Target == BodyLocation.Head ? DefenseEffectiveness.Full : DefenseEffectiveness.None;
}
