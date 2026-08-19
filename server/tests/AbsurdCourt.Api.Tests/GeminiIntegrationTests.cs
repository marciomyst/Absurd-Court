using AbsurdCourt.Api.RealTime;
using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Infrastructure.Judging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace AbsurdCourt.Api.Tests;

public sealed class GeminiIntegrationTests
{
    [Fact]
    [Trait("Category", "ExternalIntegration")]
    public async Task JudgeAsync_returns_a_structured_ruling_using_the_configured_api_key()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_GEMINI_INTEGRATION_TESTS"), "1", StringComparison.Ordinal))
            return;

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(typeof(PlayerSessionCookieMiddleware).Assembly, optional: true)
            .Build();
        var apiKey = configuration["Gemini:ApiKey"];

        Assert.False(string.IsNullOrWhiteSpace(apiKey), "Gemini:ApiKey deve estar configurada nos User Secrets da API.");

        using var client = new HttpClient { BaseAddress = new Uri("https://generativelanguage.googleapis.com") };
        client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

        var model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? new GeminiOptions().Model;
        var service = new GeminiJudgeService(
            client,
            Options.Create(new GeminiOptions { ApiKey = apiKey, Model = model }));

        var response = await service.JudgeAsync(new AiJudgeRequest(
            "Responda sempre em português do Brasil.",
            "Julgue a defesa do réu A: 'Cheguei atrasado porque o despertador pediu mais cinco minutos.' " +
            "Retorne exatamente um parecer para o réu A."));

        var ruling = Assert.Single(response.Rulings);
        Assert.Matches("(?i)^\\s*(réu\\s*)?A\\s*$", ruling.Label);
        Assert.False(string.IsNullOrWhiteSpace(ruling.Opinion));
        Assert.InRange(ruling.Points, 0, 1000);
    }
}
