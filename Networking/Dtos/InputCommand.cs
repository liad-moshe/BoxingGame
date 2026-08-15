using BoxingGame.Domain.Boxers;

namespace BoxingGame.Networking.Dtos;

public record InputCommand(
    MovementDirection Movement,
    string?           AttackKey,   // "LeftJab" | "RightJab" | "LeftHook" | "RightHook" | null
    bool              DefenseHeld, // hold to raise both hands (BothHandsBlock)
    bool              DuckHeld     // hold to lower head below jab path (Duck)
);
