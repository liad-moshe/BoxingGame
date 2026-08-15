namespace BoxingGame.Domain.Strikes;

// Elbow and glove positions relative to the boxer's base point (feet center).
// Positive X is forward (toward opponent), negative Y is upward.
public record struct ArmPose(float ElbowX, float ElbowY, float GloveX, float GloveY);
