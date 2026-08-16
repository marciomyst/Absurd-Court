using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbsurdCourt.Infrastructure.Judging;

internal sealed record GeminiRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("input")] string Input,
    [property: JsonPropertyName("system_instruction")] string SystemInstruction,
    [property: JsonPropertyName("response_format")] GeminiResponseFormat ResponseFormat,
    [property: JsonPropertyName("generation_config")] GeminiGenerationConfig GenerationConfig);

internal sealed record GeminiResponseFormat(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("mime_type")] string MimeType,
    [property: JsonPropertyName("schema")] object Schema);

internal sealed record GeminiGenerationConfig(
    [property: JsonPropertyName("max_output_tokens")] int MaxOutputTokens);

internal sealed record GeminiResponse(
    [property: JsonPropertyName("steps")] IReadOnlyList<GeminiStep>? Steps);

internal sealed record GeminiStep(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("content")] IReadOnlyList<GeminiContentPart>? Content);

internal sealed record GeminiContentPart(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text")] string? Text);

internal sealed record GeminiRulings(
    [property: JsonPropertyName("rulings")] IReadOnlyList<GeminiRuling> Rulings);

internal sealed record GeminiRuling(
    [property: JsonPropertyName("reu")] string Reu,
    [property: JsonPropertyName("vencedor")] bool Vencedor,
    [property: JsonPropertyName("parecer")] string Parecer,
    [property: JsonPropertyName("pontos")] int Pontos);
