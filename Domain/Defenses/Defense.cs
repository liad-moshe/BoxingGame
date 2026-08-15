using BoxingGame.Domain.Strikes;

namespace BoxingGame.Domain.Defenses;

public abstract class Defense
{
    public abstract string Name { get; }
    public abstract DefenseAnimationFrames Frames { get; }
    public abstract DefenseEffectiveness EvaluateAgainst(Strike incoming);

    /// <summary>
    /// Returns true when the defense geometrically moves a body part out of
    /// the strike's path (e.g. duck lowers the head below a jab).
    /// Hit resolution treats this as a Miss, not a Block.
    /// Override in subclasses that evade rather than absorb.
    /// </summary>
    public virtual bool EvadesStrike(Strike incoming) => false;
}
