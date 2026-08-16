using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Domain.Matches;
using Microsoft.Extensions.Logging;

namespace AbsurdCourt.Infrastructure.Judging;

/// <summary>Orquestra o julgamento e mantém as regras comuns a todos os providers.</summary>
public sealed class AiJudgeService(IAiProvider provider, ILogger<AiJudgeService> logger) : IJudgeService
{
    private const string SystemPrompt = """
        Você é o juiz do "Tribunal do Absurdo", um tribunal fictício e cômico que julga acusações
        absurdas do cotidiano. Seu tom é seco, burocrático e absurdamente sério diante de situações
        ridículas — como um juiz de verdade que leva a sério até o caso mais bizarro. Nunca quebre o
        personagem, nunca explique a piada, nunca seja meta. Responda sempre em português do Brasil.
        """;

    public async Task<RoundJudgment> JudgeRoundAsync(
        string autos,
        string hint,
        IReadOnlyList<DefenseSubmission> submissions,
        CancellationToken ct = default)
    {
        var labels = submissions.Select((s, i) => (Label: IndexToLabel(i), s.PlayerId)).ToList();
        var defensesText = string.Join("\n\n", submissions.Select((s, i) => $"Réu {labels[i].Label}: \"{s.Text}\""));
        var userPrompt = $"""
            CASO: {hint}

            AUTOS: {autos}

            DEFESAS APRESENTADAS:
            {defensesText}

            Julgue este caso. Escolha exatamente um réu vencedor — o mais convincente ou engraçado — e
            dê a ele 1000 pontos. Dê aos demais uma pontuação de consolação entre 100 e 600, proporcional
            ao mérito da defesa. Escreva um parecer individual e cômico para cada réu.
            """;

        try
        {
            var response = await provider.JudgeAsync(new AiJudgeRequest(SystemPrompt, userPrompt), ct);
            return BuildJudgment(labels, response.Rulings);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Falha ao consultar o juiz IA — aplicando parecer de reserva.");
            return Fallback(submissions);
        }
    }

    private static RoundJudgment BuildJudgment(List<(string Label, Guid PlayerId)> labels, IReadOnlyList<AiRuling> rulings)
    {
        var verdicts = new Dictionary<Guid, Verdict>();
        var usedOpinions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (label, playerId) in labels)
        {
            var ruling = rulings.FirstOrDefault(r => string.Equals(NormalizeLabel(r.Label), label, StringComparison.OrdinalIgnoreCase));
            var opinion = ruling?.Opinion;
            if (string.IsNullOrWhiteSpace(opinion)) opinion = FallbackOpinion(label);
            if (!usedOpinions.Add(opinion.Trim()))
                opinion = $"{opinion.Trim()} Análise individual registrada para o réu {label}.";

            verdicts[playerId] = ruling switch
            {
                { IsWinner: true } => Verdict.Winner(opinion),
                not null => Verdict.Create(opinion, ruling.Points),
                null => Verdict.Create(opinion, 300),
            };
        }

        if (verdicts.Count > 0 && verdicts.Values.Count(v => v.Points == Verdict.MaxPoints) != 1)
        {
            var topPlayerId = verdicts.OrderByDescending(kv => kv.Value.Points).First().Key;
            verdicts[topPlayerId] = Verdict.Winner(verdicts[topPlayerId].ParecerText);
        }

        return new RoundJudgment(verdicts);
    }

    private static RoundJudgment Fallback(IReadOnlyList<DefenseSubmission> submissions) =>
        new(submissions
            .Select((submission, index) => new
            {
                submission.PlayerId,
                Verdict = Verdict.Create(FallbackOpinion(IndexToLabel(index)), 300),
            })
            .ToDictionary(x => x.PlayerId, x => x.Verdict));

    private static string FallbackOpinion(string label) =>
        $"O tribunal registrou a defesa do réu {label} e determina análise complementar específica para esta manifestação.";

    private static string NormalizeLabel(string label) =>
        label.Trim().Replace("réu", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

    private static string IndexToLabel(int i) => ((char)('A' + i)).ToString();
}
