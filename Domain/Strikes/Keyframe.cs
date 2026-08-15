namespace BoxingGame.Domain.Strikes;

// T is normalised [0, 1] within the phase.
public record Keyframe(float T, ArmPose Left, ArmPose Right, HeadPose Head);
