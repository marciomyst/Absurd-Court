namespace AbsurdCourt.Domain.Common;

/// <summary>
/// Thrown by both the Room and Match aggregates: a match needs at least two
/// players, and Match.Start defensively re-checks what Room.BeginMatch already validated.
/// </summary>
public sealed class NotEnoughPlayersException(Guid roomId) : DomainException("São necessários pelo menos 2 jogadores para iniciar.")
{
    public Guid RoomId { get; } = roomId;
}
