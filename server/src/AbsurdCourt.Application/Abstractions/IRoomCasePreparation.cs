namespace AbsurdCourt.Application.Abstractions;

/// <summary>Coordinates non-blocking case preparation for a room in the single-instance MVP.</summary>
public interface IRoomCasePreparation
{
    void PrepareInitial(Guid roomId);
    IReadOnlyList<Guid> TakeInitial(Guid roomId, int count);
    void PrepareRemaining(Guid roomId, int count, int startIndex);
}
