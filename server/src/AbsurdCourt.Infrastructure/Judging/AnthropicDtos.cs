using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbsurdCourt.Infrastructure.Judging;

internal sealed record AnthropicRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("max_tokens")] int MaxTokens,
    [property: JsonPropertyName("system")] string System,
    [property: JsonPropertyName("messages")] IReadOnlyList<AnthropicMessage> Messages,
    [property: JsonPropertyName("tools")] IReadOnlyList<AnthropicTool> Tools,
    [property: JsonPropertyName("tool_choice")] AnthropicToolChoice ToolChoice);

internal sealed record AnthropicMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

internal sealed record AnthropicTool(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("input_schema")] object InputSchema);

internal sealed record AnthropicToolChoice(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string Name);

internal sealed record AnthropicResponse(
    [property: JsonPropertyName("content")] IReadOnlyList<AnthropicContentBlock> Content);

internal sealed record AnthropicContentBlock(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("input")] JsonElement Input);

internal sealed record JudgeRuling(
    [property: JsonPropertyName("reu")] string Reu,
    [property: JsonPropertyName("vencedor")] bool Vencedor,
    [property: JsonPropertyName("parecer")] string Parecer,
    [property: JsonPropertyName("pontos")] int Pontos);

internal sealed record JudgeRulings(
    [property: JsonPropertyName("rulings")] IReadOnlyList<JudgeRuling> Rulings);
