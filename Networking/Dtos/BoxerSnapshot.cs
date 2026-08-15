using BoxingGame.Domain.Boxers;
using BoxingGame.Domain.Combat;

namespace BoxingGame.Networking.Dtos;

public record BoxerSnapshot(
    string            Id,
    string            DisplayName,
    float             PositionX,
    float             Health,
    float             MaxHealth,
    float             HeadHealth,          // current head sub-pool HP
    float             MaxHeadHealth,       // max head sub-pool HP (65 % of MaxHealth)
    float             BodyHealth,          // current body sub-pool HP
    float             MaxBodyHealth,       // max body sub-pool HP (35 % of MaxHealth)
    float             Stamina,
    float             MaxStamina,
    BoxerStateKind    State,
    Side              Stance,
    string?           CurrentStrikeName,
    StrikePhase?      CurrentPhase,
    float             PhaseProgress,       // 0..1 within the current strike phase
    string?           CurrentDefenseName,  // "BothHandsBlock" | "Duck" | null
    bool              IsSouthpaw,
    float             IdlePhase,           // 0..1 cycle for boxing-dance animation
    MovementDirection MovingDirection,
    float             WalkPhase,           // 0..1 cycle for foot-stepping animation
    StrikePhase?      DodgePhase,          // Startup / Active / Recovery while dodging
    float             DodgeProgress,       // 0..1 within the current dodge phase
    float             HeightFactor,        // 0.90..1.10 physical scale
    float             DefenseProgress      // 0..1 — arms rising from guard to full block
);
