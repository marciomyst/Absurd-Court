using System.Net.Http.Json;
using System.Text.Json;
using AbsurdCourt.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace AbsurdCourt.Infrastructure.Judging;

/// <summary>Adaptador Gemini GenerateContent com saída JSON estruturada.</summary>
public sealed class GeminiJudgeService(HttpClient http, IOptions<GeminiOptions> options) : IAiProvider
{
    // O Gemini normalmente responde em poucos segundos, mas oito segundos deixava pouca
    // margem para picos transitórios e transformava uma resposta válida em parecer de reserva.
    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(12);
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
                },
            },
        },
        required = new[] { "rulings" },
    };

    public async Task<AiJudgeResponse> JudgeAsync(AiJudgeRequest request, CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(CallTimeout);

        var body = new GeminiRequest(
            options.Value.Model,
            request.UserPrompt,
            request.SystemPrompt,
            new GeminiResponseFormat("text", "application/json", RulingsSchema),
            new GeminiGenerationConfig(1024));

        var endpoint = "/v1beta/interactions";
        using var response = await http.PostAsJsonAsync(endpoint, body, JsonOptions, cts.Token);
        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync(cts.Token);
            throw new InvalidOperationException($"Gemini retornou {(int)response.StatusCode}: {details[..Math.Min(details.Length, 500)]}");
        }

        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, cts.Token);
        var content = result?.Steps?
            .Where(step => string.Equals(step.Type, "model_output", StringComparison.OrdinalIgnoreCase))
            .SelectMany(step => step.Content ?? [])
            .FirstOrDefault(part => string.Equals(part.Type, "text", StringComparison.OrdinalIgnoreCase))?.Text
            ?? throw new InvalidOperationException("Resposta do Gemini não incluiu um julgamento.");
        var rulings = JsonSerializer.Deserialize<GeminiRulings>(content, JsonOptions)
            ?? throw new InvalidOperationException("Não foi possível interpretar o julgamento do Gemini.");

        return new AiJudgeResponse(rulings.Rulings
            .Select(r => new AiRuling(r.Reu, r.Vencedor, r.Parecer, r.Pontos))
            .ToList());
    }
}
