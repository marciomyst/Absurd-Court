namespace AbsurdCourt.Application.Abstractions;

/// <summary>Pedido neutro de julgamento, independente do SDK ou API do provider.</summary>
public sealed record AiJudgeRequest(
    string SystemPrompt,
    string UserPrompt);

/// <summary>Resultado estruturado devolvido por um provider de IA.</summary>
public sealed record AiJudgeResponse(IReadOnlyList<AiRuling> Rulings);

public sealed record AiRuling(
    string Label,
    bool IsWinner,
    string Opinion,
    int Points);

/// <summary>
/// Adaptador para um provider de IA. Implementações não devem conhecer o domínio do jogo;
/// devem apenas traduzir o pedido neutro para a API escolhida e devolver o resultado estruturado.
/// </summary>
public interface IAiProvider
{
    Task<AiJudgeResponse> JudgeAsync(AiJudgeRequest request, CancellationToken ct = default);
}
