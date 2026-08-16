using AbsurdCourt.Api.RealTime;
using AbsurdCourt.Application.Contracts;
using AbsurdCourt.Application.Features.Rooms.CreateRoom;
using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Infrastructure.Judging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace AbsurdCourt.Api.Tests;

public class SecurityInfrastructureTests
{
    [Fact]
    public async Task Judge_service_keeps_individual_opinions_when_provider_decorates_labels()
    {
        var firstPlayer = Guid.NewGuid();
        var secondPlayer = Guid.NewGuid();
        var provider = new StubAiProvider([
            new AiRuling("Réu A", true, "A defesa A foi protocolada com pompa cartorial.", 1000),
            new AiRuling("Réu B", false, "A defesa B foi recebida sob protesto do carimbo.", 300),
        ]);
        var judge = new AiJudgeService(provider, NullLogger<AiJudgeService>.Instance);

        var judgment = await judge.JudgeRoundAsync("Autos", "Caso", [
            new DefenseSubmission(firstPlayer, "Defesa A"),
            new DefenseSubmission(secondPlayer, "Defesa B"),
        ]);

        Assert.Equal("A defesa A foi protocolada com pompa cartorial.", judgment.VerdictsByPlayer[firstPlayer].ParecerText);
        Assert.Equal("A defesa B foi recebida sob protesto do carimbo.", judgment.VerdictsByPlayer[secondPlayer].ParecerText);
    }

    [Fact]
    public async Task Mock_judge_returns_deterministic_rulings_without_external_calls()
    {
        var judge = new MockAiProvider();

        var result = await judge.JudgeAsync(new AiJudgeRequest(
            "Use português do Brasil.",
            "Réu A: defesa A\nRéu B: defesa B"));

        Assert.Collection(result.Rulings,
            ruling =>
            {
                Assert.Equal("A", ruling.Label);
                Assert.True(ruling.IsWinner);
                Assert.Equal(1000, ruling.Points);
            },
            ruling =>
            {
                Assert.Equal("B", ruling.Label);
                Assert.False(ruling.IsWinner);
            });
    }

    [Fact]
    public void Session_store_round_trips_and_removes_sessions()
    {
        var store = new PlayerSessionStore();
        var token = Guid.NewGuid();

        store.Set("session-1", "0000-1234-5678", token);

        Assert.True(store.TryGet("session-1", out var session));
        Assert.Equal("0000-1234-5678", session.RoomCode);
        Assert.Equal(token, session.ReconnectToken);

        store.Remove("session-1");
        Assert.False(store.TryGet("session-1", out _));
    }

    [Fact]
    public void Rate_limiter_rejects_calls_after_the_window_limit()
    {
        var limiter = new HubInvocationRateLimiter();

        Assert.True(limiter.TryConsume("127.0.0.1:entry", 2, TimeSpan.FromMinutes(1)));
        Assert.True(limiter.TryConsume("127.0.0.1:entry", 2, TimeSpan.FromMinutes(1)));
        Assert.False(limiter.TryConsume("127.0.0.1:entry", 2, TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public async Task Middleware_issues_an_httponly_session_cookie_for_hub_requests()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/hubs/court/negotiate";
        var middleware = new PlayerSessionCookieMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        var setCookie = context.Response.Headers.SetCookie.ToString();
        Assert.Contains(PlayerSessionCookieMiddleware.CookieName, setCookie);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Reconnect_token_is_not_serialized_to_client_contracts()
    {
        var token = Guid.NewGuid();
        var room = new RoomSnapshotDto(
            Guid.NewGuid(), "0000-1234-5678", "Lobby", Guid.NewGuid(), [], 3, 60);
        var result = new CreateRoomResult(room, Guid.NewGuid(), token);

        var json = JsonSerializer.Serialize(result);

        Assert.DoesNotContain(token.ToString(), json, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class StubAiProvider(IReadOnlyList<AiRuling> rulings) : IAiProvider
    {
        public Task<AiJudgeResponse> JudgeAsync(AiJudgeRequest request, CancellationToken ct = default) =>
            Task.FromResult(new AiJudgeResponse(rulings));
    }
}
