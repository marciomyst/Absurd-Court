namespace AbsurdCourt.Application.Common;

public sealed class ConnectionNotAuthorizedException(Guid playerId)
    : Exception($"A conexão atual não está autorizada para o jogador {playerId}.");
