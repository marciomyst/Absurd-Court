using System.Net.Http.Json;
using System.Text.Json;
using AbsurdCourt.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AbsurdCourt.Infrastructure.Judging;

/// <summary>
/// Calls Claude once per round with every submitted defense at once, so the ruling is
/// comparative (like a real court favoring one argument over the others) rather than N
/// independent per-defense scores. It only adapts the neutral provider contract to Anthropic;
/// fallback and domain validation live in AiJudgeService.
/// </summary>
public sealed class AnthropicJudgeService(HttpClient http, IOptions<AnthropicOptions> options)
    : IAiProvider
{
    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(8);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly object RulingToolSchema = new
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
                        reu = new { type = "string", description = "O identificador do réu (ex: 'A', 'B')." },
                        vencedor = new { type = "boolean", description = "Se este réu venceu o caso — exatamente um réu deve vencer." },
                        parecer = new { type = "string", description = "Parecer do juiz sobre esta defesa especificamente, em tom absurdo e burocrático, 1-2 frases." },
                        pontos = new { type = "integer", description = "Pontuação de 0 a 1000. O vencedor deve receber exatamente 1000." },
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
        var providerRequest = new AnthropicRequest(
            options.Value.Model,
            1024,
            request.SystemPrompt,
            [new AnthropicMessage("user", request.UserPrompt)],
            [new AnthropicTool("julgar", "Registra o julgamento do tribunal para este caso.", RulingToolSchema)],
            new AnthropicToolChoice("tool", "julgar"));
        using var response = await http.PostAsJsonAsync("/v1/messages", providerRequest, JsonOptions, cts.Token);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AnthropicResponse>(JsonOptions, cts.Token);
        var toolBlock = body?.Content.FirstOrDefault(c => c.Type == "tool_use")
            ?? throw new InvalidOperationException("Resposta da IA não incluiu o uso da ferramenta esperada.");
        var rulings = toolBlock.Input.Deserialize<JudgeRulings>(JsonOptions)
            ?? throw new InvalidOperationException("Não foi possível interpretar o julgamento da IA.");
        return new AiJudgeResponse(rulings.Rulings.Select(r => new AiRuling(r.Reu, r.Vencedor, r.Parecer, r.Pontos)).ToList());
    }
}
