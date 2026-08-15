using BoxingGame.Domain.Match;

namespace BoxingGame.Networking.Dtos;

public record MatchSnapshot(
    string        MatchCode,
    MatchPhase    Phase,
    int           RefereeCount,
    BoxerSnapshot P1,
    BoxerSnapshot P2,
    int           CurrentRound,           // 1-12
    int           P1Score,
    int           P2Score,
    float         BetweenRoundsRemainSec, // seconds left in inter-round break
    float         RoundRemainSec,         // seconds left in current round
    int           SoundGenThrow,          // version counter: increment = play throw sound
    int           SoundGenLand,           // version counter: increment = play land sound
    int           SoundGenBell,           // version counter: increment = ring the bell
    int           SoundGenBlock,          // version counter: increment = play guard-block thud
    bool          IsClinching,            // true while referee is separating fighters
    int           ClinchStage,            // 0=freeze  1=push P1  2=push P2
    float         ClinchStageProgress     // 0..1 within the current clinch stage
);
