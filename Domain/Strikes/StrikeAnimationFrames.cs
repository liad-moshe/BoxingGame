using BoxingGame.Domain.Combat;

namespace BoxingGame.Domain.Strikes;

public record StrikeAnimationFrames(Keyframe[] Startup, Keyframe[] Active, Keyframe[] Recovery)
{
    // ── Orthodox guard (left-hand lead) ─────────────────────────────────────
    // Left  arm (shoulder −18): extends toward opponent → elbow fwd, glove fwd.
    public static readonly ArmPose GuardLeft  = new(5f,  -78f, 30f, -82f);
    // Right arm (shoulder +18): wraps inward to chin → elbow goes backward, glove near face.
    public static readonly ArmPose GuardRight = new(-5f, -78f, 10f, -80f);

    // ── Southpaw guard (right-hand lead) ────────────────────────────────────
    // Right arm (shoulder +18) is now the LEAD: arm extends toward opponent.
    //   shoulder(18) → elbow(30) → glove(42): arm travels forward at head height.
    public static readonly ArmPose SouthpawLeadGuard = new(30f, -90f, 42f, -94f);
    // Left  arm (shoulder −18) is now the REAR: wraps across to guard chin.
    //   shoulder(−18) → elbow(−6) → glove(12): arm curves across in front of face.
    public static readonly ArmPose SouthpawRearGuard = new(-6f, -82f, 12f, -84f);

    public static readonly HeadPose HeadNeutral = new(0f, 0f);

    /// <param name="guardLeft">
    /// Optional: the boxer's actual left-arm guard pose.  When supplied, the t=0 Startup
    /// keyframe's arm poses are replaced so southpaw boxers don't snap from their shifted
    /// guard into the orthodox guard at the first frame of a strike.
    /// </param>
    public Keyframe Sample(StrikePhase phase, float t,
                           ArmPose? guardLeft = null, ArmPose? guardRight = null)
    {
        var frames = phase switch
        {
            StrikePhase.Startup  => Startup,
            StrikePhase.Active   => Active,
            StrikePhase.Recovery => Recovery,
            _ => Recovery
        };
        if (frames.Length == 0)
            return new Keyframe(0,
                new(GuardLeft.ElbowX,  GuardLeft.ElbowY,  GuardLeft.GloveX,  GuardLeft.GloveY),
                new(GuardRight.ElbowX, GuardRight.ElbowY, GuardRight.GloveX, GuardRight.GloveY),
                HeadNeutral);

        // Substitute the t=0 Startup frame with the boxer's actual guard so the arm lerps
        // smoothly from wherever it was in guard rather than snapping to orthodox guard first.
        var f0 = (guardLeft.HasValue && guardRight.HasValue &&
                  phase == StrikePhase.Startup && frames[0].T < 0.001f)
            ? frames[0] with { Left = guardLeft.Value, Right = guardRight.Value }
            : frames[0];

        if (frames.Length == 1) return f0;
        for (int i = 1; i < frames.Length; i++)
        {
            if (t <= frames[i].T)
            {
                var   prev  = (i == 1) ? f0 : frames[i - 1];
                float span  = frames[i].T - prev.T;
                float local = span < 0.0001f ? 1f : (t - prev.T) / span;
                return LerpFrames(prev, frames[i], local);
            }
        }
        return frames[^1];
    }

    private static Keyframe LerpFrames(Keyframe a, Keyframe b, float t) =>
        new(t, LerpArm(a.Left, b.Left, t), LerpArm(a.Right, b.Right, t),
            new HeadPose(Lerp(a.Head.OffsetX, b.Head.OffsetX, t), Lerp(a.Head.OffsetY, b.Head.OffsetY, t)));

    private static ArmPose LerpArm(ArmPose a, ArmPose b, float t) =>
        new(Lerp(a.ElbowX, b.ElbowX, t), Lerp(a.ElbowY, b.ElbowY, t),
            Lerp(a.GloveX, b.GloveX, t), Lerp(a.GloveY, b.GloveY, t));

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
