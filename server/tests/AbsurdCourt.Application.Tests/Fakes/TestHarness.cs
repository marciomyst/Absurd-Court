using AbsurdCourt.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AbsurdCourt.Application.Tests.Fakes;

/// <summary>Wires the real MediatR pipeline (via AddApplication) against fake ports — exercises the actual command handlers and their cross-handler orchestration (e.g. SubmitDefense cascading into CloseRound), not just isolated units.</summary>
public sealed class TestHarness : IDisposable
{
    public FakeRoomRepository Rooms { get; } = new();
    public FakeMatchRepository Matches { get; } = new();
    public FakeCaseBankRepository CaseBank { get; } = new();
    public FakeJudgeService Judge { get; } = new();
    public FakeUnitOfWork UnitOfWork { get; } = new();

    private readonly ServiceProvider _provider;

    public TestHarness()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddSingleton<IRoomRepository>(Rooms);
        services.AddSingleton<IMatchRepository>(Matches);
        services.AddSingleton<ICaseBankRepository>(CaseBank);
        services.AddSingleton<IRoomCasePreparation, FakeRoomCasePreparation>();
        services.AddSingleton<IJudgeService>(Judge);
        services.AddSingleton<IUnitOfWork>(UnitOfWork);
        _provider = services.BuildServiceProvider();
    }

    public ISender Sender => _provider.GetRequiredService<ISender>();

    public void Dispose() => _provider.Dispose();
}
