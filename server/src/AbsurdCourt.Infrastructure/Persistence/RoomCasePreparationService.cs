using System.Collections.Concurrent;
using System.Threading.Channels;
using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Domain.Matches;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AbsurdCourt.Infrastructure.Persistence;

public sealed class RoomCasePreparationService(
    IServiceScopeFactory scopes,
    ILogger<RoomCasePreparationService> logger) : BackgroundService, IRoomCasePreparation
{
    private const int InitialCaseCount = 3;
    private readonly Channel<PreparationRequest> requests = Channel.CreateUnbounded<PreparationRequest>();
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<Guid>> initialCases = new();

    public void PrepareInitial(Guid roomId) => requests.Writer.TryWrite(new PreparationRequest(roomId, InitialCaseCount, null));

    public IReadOnlyList<Guid> TakeInitial(Guid roomId, int count)
    {
        if (!initialCases.TryGetValue(roomId, out var prepared)) return [];

        var result = new List<Guid>(count);
        while (result.Count < count && prepared.TryDequeue(out var caseId)) result.Add(caseId);
        return result;
    }

    public void PrepareRemaining(Guid roomId, int count, int startIndex)
    {
        if (count > 0) requests.Writer.TryWrite(new PreparationRequest(roomId, count, startIndex));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in requests.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                var generator = scope.ServiceProvider.GetRequiredService<ICaseGenerator>();
                var generated = await generator.GenerateAsync(request.Count, stoppingToken);
                if (generated.Count == 0) continue;

                var db = scope.ServiceProvider.GetRequiredService<CourtDbContext>();
                var created = generated
                    .Select(caseFile => new CaseFile(Guid.NewGuid(), caseFile.Autos[..Math.Min(caseFile.Autos.Length, 500)], caseFile.Hint[..Math.Min(caseFile.Hint.Length, 80)]))
                    .ToList();
                db.CaseFiles.AddRange(created);
                await db.SaveChangesAsync(stoppingToken);

                if (request.StartIndex is null)
                {
                    var prepared = initialCases.GetOrAdd(request.RoomId, _ => new ConcurrentQueue<Guid>());
                    foreach (var caseFile in created) prepared.Enqueue(caseFile.Id);
                    continue;
                }

                var matches = scope.ServiceProvider.GetRequiredService<IMatchRepository>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var match = await matches.GetActiveByRoomIdAsync(request.RoomId, stoppingToken);
                if (match is null) continue;
                match.ReplaceFutureCaseFiles(request.StartIndex.Value, created.Select(caseFile => caseFile.Id).ToList());
                await uow.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Falha ao preparar casos IA para a sala {RoomId}; o catálogo de reserva será usado.", request.RoomId);
            }
        }
    }

    private sealed record PreparationRequest(Guid RoomId, int Count, int? StartIndex);
}
