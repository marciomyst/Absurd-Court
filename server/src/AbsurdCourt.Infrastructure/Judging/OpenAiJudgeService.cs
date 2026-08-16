using System.Net.Http.Json;
using System.Text.Json;
using AbsurdCourt.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace AbsurdCourt.Infrastructure.Judging;

/// <summary>Adaptador OpenAI usando Chat Completions com resposta JSON estruturada.</summary>
public sealed class OpenAiJudgeService(HttpClient http, IOptions<OpenAiOptions> options) : IAiProvider
{
    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(8);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly object RulingsSchema = new
    {
        type = "object",
        properties = new
        {
            rulings = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        reu = new { type = "string" },
                        vencedor = new { type = "boolean" },
                        parecer = new { type = "string" },
                        pontos = new { type = "integer" },
                    },
                    required = new[] { "reu", "vencedor", "parecer", "pontos" },
                    additionalProperties = false,
                },
            },
        },
        required = new[] { "rulings" },
        additionalProperties = false,
    };

    public async Task<AiJudgeResponse> JudgeAsync(AiJudgeRequest request, CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(CallTimeout);
        var body = new OpenAiRequest(
            options.Value.Model,
            [new OpenAiMessage("system", request.SystemPrompt), new OpenAiMessage("user", request.UserPrompt)],
            new OpenAiResponseFormat("json_schema", new OpenAiJsonSchema("tribunal_rulings", true, RulingsSchema)));
        using var response = await http.PostAsJsonAsync("/v1/chat/completions", body, JsonOptions, cts.Token);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<OpenAiResponse>(JsonOptions, cts.Token);
        var content = result?.Choices.FirstOrDefault()?.Message.Content
            ?? throw new InvalidOperationException("Resposta da IA não incluiu um julgamento.");
        var rulings = JsonSerializer.Deserialize<OpenAiRulings>(content, JsonOptions)
            ?? throw new InvalidOperationException("Não foi possível interpretar o julgamento da IA.");
        return new AiJudgeResponse(rulings.Rulings.Select(r => new AiRuling(r.Reu, r.Vencedor, r.Parecer, r.Pontos)).ToList());
    }
}
