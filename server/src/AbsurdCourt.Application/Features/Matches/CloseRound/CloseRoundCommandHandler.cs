using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Application.Common;
using AbsurdCourt.Domain.Matches;
using MediatR;

namespace AbsurdCourt.Application.Features.Matches.CloseRound;

public sealed class CloseRoundCommandHandler(
    IMatchRepository matches, ICaseBankRepository caseBank, IJudgeService judge, IUnitOfWork uow)
    : IRequestHandler<CloseRoundCommand>
{
    public async Task Handle(CloseRoundCommand request, CancellationToken ct)
    {
        var match = await matches.GetActiveByRoomIdAsync(request.RoomId, ct)
            ?? throw new MatchNotFoundException(request.RoomId);

        if (!match.TryCloseRound(out var round))
            return; // someone else (the other race path) already won this transition

        try
        {
            // Persist the Open->Judging transition NOW: this is what lets a concurrent
            // caller who loaded the same Open snapshot discover — via ConcurrencyConflictException —
            // that it lost the race, instead of both callers proceeding to judge and reveal.
            await uow.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return;
        }

        var caseFile = await caseBank.GetByIdAsync(round.CaseFileId, ct);
        var submissions = round.Defenses
            .Where(d => d.WasSubmitted)
            .Select(d => new DefenseSubmission(d.PlayerId, d.Text))
            .ToList();

        var verdicts = submissions.Count == 0
            ? new Dictionary<Guid, Verdict>()
            : (await judge.JudgeRoundAsync(caseFile!.Autos, caseFile.Hint, submissions, ct)).VerdictsByPlayer;

        match.RevealRound(verdicts, DateTime.UtcNow);
        await uow.SaveChangesAsync(ct);
    }
}
