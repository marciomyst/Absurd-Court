using AbsurdCourt.Domain.Common;

namespace AbsurdCourt.Domain.Rooms;

public sealed class RoomFullException(Guid roomId) : DomainException("A sala já atingiu o número máximo de jogadores.")
{
    public Guid RoomId { get; } = roomId;
}

public sealed class RoomNotJoinableException(Guid roomId) : DomainException("A sala não está mais aceitando jogadores.")
{
    public Guid RoomId { get; } = roomId;
}

public sealed class InvalidReconnectTokenException(Guid roomId) : DomainException("Token de reconexão inválido.")
{
    public Guid RoomId { get; } = roomId;
}

public sealed class RematchNotAvailableException(Guid roomId) : DomainException("O rematch ainda não está disponível para esta sala.")
{
    public Guid RoomId { get; } = roomId;
}
