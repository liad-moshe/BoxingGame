using BoxingGame.Domain.Boxers;
using BoxingGame.Domain.Combat;
using BoxingGame.Domain.Strikes;

namespace BoxingGame.Domain.Match;

/// <summary>
/// Simple rule-based AI that controls the Blue (P2) boxer.
/// Decides ~3× per second; holds decisions between decision ticks
/// so movement and block states persist naturally.
/// </summary>
public sealed class AiController
{
    private readonly Match  _match;
    private readonly string _aiId;
    private readonly Random _rng = new();

    // Decision interval with jitter so it doesn't feel mechanical.
    private const float BaseDecideMs  = 340f;
    private const float JitterMs      = 180f;

    // Ideal fighting distance (pixels). AI tries to stay inside jab range.
    private const float IdealRangePct = 0.88f;   // fraction of AI's effective jab reach

    // Persistent AI input state
    private MovementDirection _movement    = MovementDirection.None;
    private bool              _blockHeld;
    private float             _blockHoldMs;   // remaining ms to hold block after deciding
    private float             _decideTimer;

    // Slight delays for reactions — makes the AI beatable
    private float _reactionDelayMs;
    private const float ReactionDelayTarget = 110f;  // ms lag before acting on perceived threat

    public AiController(Match match, string aiPlayerId)
    {
        _match     = match;
        _aiId      = aiPlayerId;
        _decideTimer = BaseDecideMs;  // fire first decision quickly
    }

    public void Tick(float ms)
    {
        var ai    = _aiId == "P1" ? _match.P1 : _match.P2;
        var human = _aiId == "P1" ? _match.P2 : _match.P1;

        // Can't control boxer while knocked out.
        if (ai.State is BoxerStateKind.KnockedOut) return;

        // Block countdown
        if (_blockHoldMs > 0f)
        {
            _blockHoldMs -= ms;
            if (_blockHoldMs <= 0f) { _blockHeld = false; _blockHoldMs = 0f; }
        }

        // Reaction delay for threatening situations
        bool humanWindingUp = human.State == BoxerStateKind.Attacking &&
                              human.CurrentPhase == StrikePhase.Startup;
        bool humanActiveHit = human.State == BoxerStateKind.Attacking &&
                              human.CurrentPhase == StrikePhase.Active;
        bool incomingThreat = (humanWindingUp || humanActiveHit) &&
                              Math.Abs(ai.PositionX - human.PositionX) <
                              human.Stats.ReachMultiplier * 120f * human.HeightFactor;

        if (incomingThreat)
            _reactionDelayMs += ms;
        else
            _reactionDelayMs = 0f;

        // Decision timer
        _decideTimer -= ms;
        string? attack  = null;
        bool    dodge   = false;

        if (_decideTimer <= 0f)
        {
            _decideTimer = BaseDecideMs + (float)(_rng.NextDouble() * JitterMs);
            (attack, dodge) = Decide(ai, human);
        }
        // Also react mid-decision to active punches (emergency block)
        else if (humanActiveHit && !_blockHeld && _reactionDelayMs >= ReactionDelayTarget)
        {
            _blockHeld   = true;
            _blockHoldMs = 380f;
        }

        _match.ApplyInput(_aiId, _movement, attack, _blockHeld,
                          sprintHeld: false, bodyModifier: false, dodgePressed: dodge);
    }

    private (string? attack, bool dodge) Decide(Boxer ai, Boxer human)
    {
        if (ai.State is BoxerStateKind.KnockedDown or BoxerStateKind.Hurt or BoxerStateKind.Attacking)
        {
            _movement = MovementDirection.None;
            return (null, false);
        }

        float dist       = Math.Abs(ai.PositionX - human.PositionX);
        float jabReach   = ai.Stats.ReachMultiplier * 115f * ai.HeightFactor;
        float idealDist  = jabReach * IdealRangePct;

        bool humanPunching = human.State == BoxerStateKind.Attacking;
        bool inRange       = dist <= jabReach * 1.15f;

        // ── React to incoming punch ───────────────────────────────────────────
        if (humanPunching && inRange && _reactionDelayMs >= ReactionDelayTarget)
        {
            _movement = MovementDirection.None;
            double r = _rng.NextDouble();
            if (r < 0.30 && ai.DodgePhase == null)
            {
                return (null, true);    // slip/dodge
            }
            else
            {
                _blockHeld   = true;
                _blockHoldMs = 450f + (float)(_rng.NextDouble() * 200f);
                return (null, false);   // block
            }
        }

        // Drop block if threat is gone
        if (!humanPunching && _rng.NextDouble() < 0.6)
        {
            _blockHeld   = false;
            _blockHoldMs = 0f;
        }

        // ── Footwork ──────────────────────────────────────────────────────────
        if (dist > idealDist * 1.2f)
            _movement = MovementDirection.Forward;   // close the gap
        else if (dist < idealDist * 0.55f)
            _movement = MovementDirection.Backward;  // avoid clinch
        else if (_rng.NextDouble() < 0.25)
            _movement = MovementDirection.None;      // occasional pause

        // ── Attack selection ─────────────────────────────────────────────────
        if (inRange && dist > idealDist * 0.45f)
        {
            double r = _rng.NextDouble();
            // Weight towards faster punches; hooks used occasionally for variety.
            if      (r < 0.30) return ("LeftJab",      false);
            else if (r < 0.52) return ("RightJab",     false);
            else if (r < 0.64) return ("LeftHook",     false);
            else if (r < 0.72) return ("RightHook",    false);
            else if (r < 0.80) return ("BodyLeftJab",  false);
            else if (r < 0.87) return ("BodyRightJab", false);
            else if (r < 0.92) return ("BodyLeftHook", false);
            // else: wait
        }

        return (null, false);
    }
}
