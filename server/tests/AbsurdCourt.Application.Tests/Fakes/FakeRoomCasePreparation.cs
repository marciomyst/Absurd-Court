using AbsurdCourt.Application.Abstractions;

namespace AbsurdCourt.Application.Tests.Fakes;

public sealed class FakeRoomCasePreparation : IRoomCasePreparation
{
    public void PrepareInitial(Guid roomId) { }
    public IReadOnlyList<Guid> TakeInitial(Guid roomId, int count) => [];
    public void PrepareRemaining(Guid roomId, int count, int startIndex) { }
}
