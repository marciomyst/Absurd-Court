using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Application.Features.Matches.CloseRound;
using AbsurdCourt.Domain.Matches;
using AbsurdCourt.Domain.Rooms;
using AbsurdCourt.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AbsurdCourt.Api.BackgroundServices;

/// <summary>
/// The other half of CloseRound's two race paths (the first being "last player
/// submitted", handled inline in SubmitDefenseCommandHandler): polls for rounds whose
/// deadline has passed and closes them. Also auto-closes rooms that finished a match and
/// sat waiting for a rematch past the timeout window.
/// </summary>
public sealed class RoundDeadlineSweeper(IServiceScopeFactory scopeFactory, ILogger<RoundDeadlineSweeper> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan RematchWindow = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SweepOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao varrer rodadas/salas vencidas.");
            }
        }
    }

    private async Task SweepOnceAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CourtDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var now = DateTime.UtcNow;

        var expiredRoomIds = await db.Matches
            .Where(m => m.Status == MatchStatus.InProgress)
            .SelectMany(m => m.Rounds, (m, r) => new { m.RoomId, Round = r })
            .Where(x => x.Round.Status == RoundStatus.Open && x.Round.DeadlineUtc <= now)
            .Select(x => x.RoomId)
            .Distinct()
            .ToListAsync(ct);

        foreach (var roomId in expiredRoomIds)
            await sender.Send(new CloseRoundCommand(roomId), ct);

        var idleRooms = await db.Rooms
            .Where(r => r.Status == RoomStatus.Lobby)
            .ToListAsync(ct);

        var anyClosed = false;
        foreach (var room in idleRooms.Where(r => r.HasRematchTimedOut(now, RematchWindow)))
        {
            room.Close("Tempo esgotado para revanche.");
            anyClosed = true;
        }

        if (anyClosed)
            await uow.SaveChangesAsync(ct);
    }
}
