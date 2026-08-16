using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Application.Common;
using AbsurdCourt.Application.Contracts;
using AbsurdCourt.Domain.Matches;
using AbsurdCourt.Domain.Rooms;
using MediatR;

namespace AbsurdCourt.Application.Features.Rooms.Rejoin;

public sealed class RejoinCommandHandler(
    IRoomRepository rooms, IMatchRepository matches, ICaseBankRepository caseBank, IUnitOfWork uow)
    : IRequestHandler<RejoinCommand, RejoinResult>
{
    public async Task<RejoinResult> Handle(RejoinCommand request, CancellationToken ct)
    {
        var code = RoomCode.Create(request.RoomCode);
        var room = await rooms.GetByCodeAsync(code, ct) ?? throw new RoomNotFoundException(request.RoomCode);

        var nowUtc = DateTime.UtcNow;
        var previousConnectionId = room.Players.FirstOrDefault(p => p.HasValidReconnectToken(request.ReconnectToken, nowUtc))?.ConnectionId;
        var player = room.Rejoin(request.ReconnectToken, request.ConnectionId, nowUtc);
        await uow.SaveChangesAsync(ct);

        var matchSnapshot = room.Status == RoomStatus.InProgress
            ? await BuildMatchSnapshotAsync(room.Id, ct)
            : null;

        return new RejoinResult(room.ToSnapshot(), player.Id, player.ReconnectToken, matchSnapshot)
        {
            PreviousConnectionId = previousConnectionId,
        };
    }

    private async Task<MatchSnapshotDto?> BuildMatchSnapshotAsync(Guid roomId, CancellationToken ct)
    {
        var match = await matches.GetActiveByRoomIdAsync(roomId, ct);
        if (match?.CurrentRound is not { } round) return null;

        var caseFile = await caseBank.GetByIdAsync(round.CaseFileId, ct);

        // Reconnecting mid-Revealed shouldn't strand the client on the "waiting" screen —
        // replay the verdict it would otherwise only get from the (already-missed) broadcast.
        var results = round.Status == RoundStatus.Revealed
            ? round.Defenses
                .Where(d => d.Verdict is not null)
                .Select(d => new RoundResultDto(d.PlayerId, d.Text, d.WasSubmitted, d.Verdict!.ParecerText, d.Verdict.Points))
                .ToList()
            : null;

        return new MatchSnapshotDto(
            match.Id,
            match.CaseCount,
            round.OrderIndex,
            round.Status.ToString(),
            round.Status == RoundStatus.Open ? round.DeadlineUtc : null,
            caseFile?.Autos,
            caseFile?.Hint,
            round.Defenses.Where(d => d.WasSubmitted).Select(d => d.PlayerId).ToList(),
            match.Scores,
            results);
    }
}
