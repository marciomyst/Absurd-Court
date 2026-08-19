using System.Net.Http.Json;
using System.Text.Json;
using AbsurdCourt.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace AbsurdCourt.Infrastructure.Judging;

public sealed class GeminiCaseGenerator(HttpClient http, IOptions<GeminiOptions> options) : ICaseGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly object CasesSchema = new
    {
        type = "object",
        properties = new
        {
            cases = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        hint = new { type = "string" },
                        autos = new { type = "string" },
                    },
                    required = new[] { "hint", "autos" },
                },
            },
        },
        required = new[] { "cases" },
    };

    public async Task<IReadOnlyList<GeneratedCase>> GenerateAsync(int count, CancellationToken ct = default)
    {
        var request = new GeminiRequest(
            options.Value.Model,
            $"Gere exatamente {count} autos independentes para o Tribunal do Absurdo. Varie cenários, conflitos e a abertura do texto. " +
            "Cada autos deve ter 1-2 frases, até 450 caracteres, em português do Brasil, sem pessoas reais, discurso de ódio ou conteúdo sexual. " +
            "O hint deve ter 2-4 palavras e não pode se repetir.",
            "Você cria acusações fictícias, cômicas e burocraticamente sérias para um jogo. Responda somente no JSON estruturado solicitado.",
            new GeminiResponseFormat("text", "application/json", CasesSchema),
            new GeminiGenerationConfig(Math.Max(1024, count * 260)));

        using var response = await http.PostAsJsonAsync("/v1beta/interactions", request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, ct);
        var content = result?.Steps?
            .Where(step => string.Equals(step.Type, "model_output", StringComparison.OrdinalIgnoreCase))
            .SelectMany(step => step.Content ?? [])
            .FirstOrDefault(part => string.Equals(part.Type, "text", StringComparison.OrdinalIgnoreCase))?.Text
            ?? throw new InvalidOperationException("Resposta do Gemini não incluiu casos.");
        var generated = JsonSerializer.Deserialize<GeminiCases>(content, JsonOptions)
            ?? throw new InvalidOperationException("Não foi possível interpretar os casos do Gemini.");

        return generated.Cases
            .Where(caseFile => !string.IsNullOrWhiteSpace(caseFile.Hint) && !string.IsNullOrWhiteSpace(caseFile.Autos))
            .Select(caseFile => new GeneratedCase(caseFile.Hint.Trim(), caseFile.Autos.Trim()))
            .Take(count)
            .ToList();
    }

    private sealed record GeminiCases(IReadOnlyList<GeminiCase> Cases);
    private sealed record GeminiCase(string Hint, string Autos);
}
