namespace AbsurdCourt.Application.Common;

public sealed class RoomNotFoundException(string roomCode) : Exception($"Sala '{roomCode}' não encontrada.")
{
    public string RoomCode { get; } = roomCode;
}

public sealed class MatchNotFoundException(Guid roomId) : Exception($"Nenhuma partida ativa para a sala {roomId}.")
{
    public Guid RoomId { get; } = roomId;
}

public sealed class NotHostException(Guid roomId, Guid playerId) : Exception($"Apenas o anfitrião pode realizar esta ação (sala {roomId}, jogador {playerId}).")
{
    public Guid RoomId { get; } = roomId;
    public Guid PlayerId { get; } = playerId;
}

/// <summary>Thrown by IUnitOfWork.SaveChangesAsync when a tracked aggregate was concurrently modified — this is what makes CloseRound's Open→Judging transition safe to race.</summary>
public sealed class ConcurrencyConflictException() : Exception("Conflito de concorrência ao salvar.");
