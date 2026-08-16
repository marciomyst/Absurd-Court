using AbsurdCourt.Domain.Common;

namespace AbsurdCourt.Domain.Matches;

public sealed class RoundNotOpenException(Guid id) : DomainException("Não há rodada aberta para esta ação.")
{
    public Guid Id { get; } = id;
}

public sealed class DefenseAlreadySubmittedException(Guid roundId, Guid playerId) : DomainException("O jogador já protocolou defesa nesta rodada.")
{
    public Guid RoundId { get; } = roundId;
    public Guid PlayerId { get; } = playerId;
}

public sealed class RoundNotJudgingException(Guid roundId) : DomainException("A rodada não está em julgamento.")
{
    public Guid RoundId { get; } = roundId;
}

public sealed class RoundNotRevealedException(Guid matchId) : DomainException("A rodada atual ainda não foi revelada.")
{
    public Guid MatchId { get; } = matchId;
}

public sealed class NoMoreRoundsException(Guid matchId) : DomainException("Esta partida não tem mais casos.")
{
    public Guid MatchId { get; } = matchId;
}

public sealed class MoreRoundsRemainException(Guid matchId) : DomainException("Esta partida ainda tem casos pendentes.")
{
    public Guid MatchId { get; } = matchId;
}

public sealed class NoCasesAvailableException(Guid roomId) : DomainException("Não há casos suficientes no banco para iniciar a partida.")
{
    public Guid RoomId { get; } = roomId;
}

public sealed class PlayerNotInMatchException(Guid matchId, Guid playerId) : DomainException("O jogador não faz parte desta partida.")
{
    public Guid MatchId { get; } = matchId;
    public Guid PlayerId { get; } = playerId;
}
