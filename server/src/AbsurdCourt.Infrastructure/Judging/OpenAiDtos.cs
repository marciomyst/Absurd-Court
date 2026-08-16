using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbsurdCourt.Infrastructure.Judging;

internal sealed record OpenAiRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] IReadOnlyList<OpenAiMessage> Messages,
    [property: JsonPropertyName("response_format")] OpenAiResponseFormat ResponseFormat);

internal sealed record OpenAiMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

internal sealed record OpenAiResponseFormat(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("json_schema")] OpenAiJsonSchema JsonSchema);

internal sealed record OpenAiJsonSchema(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("strict")] bool Strict,
    [property: JsonPropertyName("schema")] object Schema);

internal sealed record OpenAiResponse(
    [property: JsonPropertyName("choices")] IReadOnlyList<OpenAiChoice> Choices);

internal sealed record OpenAiChoice(
    [property: JsonPropertyName("message")] OpenAiMessage Message);

internal sealed record OpenAiRulings(
    [property: JsonPropertyName("rulings")] IReadOnlyList<OpenAiRuling> Rulings);

internal sealed record OpenAiRuling(
    [property: JsonPropertyName("reu")] string Reu,
    [property: JsonPropertyName("vencedor")] bool Vencedor,
    [property: JsonPropertyName("parecer")] string Parecer,
    [property: JsonPropertyName("pontos")] int Pontos);
