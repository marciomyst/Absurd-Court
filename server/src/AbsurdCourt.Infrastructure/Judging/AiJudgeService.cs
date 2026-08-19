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

            Julgue comparativamente todas as defesas. Antes de pontuar, avalie cada uma pela rubrica:
            aderência aos autos (0-400), coerência e qualidade do argumento (0-300), criatividade cômica
            pertinente ao caso (0-200) e clareza (0-100). Some os critérios e use o total exato no campo
            "pontos" (0-1000). O vencedor é obrigatoriamente a defesa com a maior nota total — nunca escolha
            por rótulo, ordem de apresentação ou sorte. Não empate: em igualdade de nota, compare primeiro
            aderência, depois coerência, criatividade e clareza; persistindo a igualdade, vença o menor rótulo.
            Marque "vencedor" apenas para essa maior nota. Devolva exatamente {submissions.Count} parecer(es),
            um para cada defesa, explicando brevemente no parecer o mérito que determinou a nota. No campo
            "reu" da resposta estruturada, use exclusivamente o rótulo de uma letra de cada defesa (por exemplo,
            "A" e "B"), na mesma ordem em que as defesas foram apresentadas.
            """;

        try
        {
            var response = await provider.JudgeAsync(new AiJudgeRequest(SystemPrompt, userPrompt), ct);
            logger.LogInformation(
                "Juiz IA retornou {RulingCount} parecer(es) para {SubmissionCount} defesa(s). Estrutura: {Rulings}.",
                response.Rulings.Count,
                labels.Count,
                string.Join(", ", response.Rulings.Select((ruling, index) =>
                    $"{index + 1}:{ruling.Label} (parecer={(string.IsNullOrWhiteSpace(ruling.Opinion) ? "ausente" : "ok")})")));
            return BuildJudgment(labels, response.Rulings);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Falha ao consultar o juiz IA — aplicando parecer de reserva.");
            return Fallback(submissions);
        }
    }

    private RoundJudgment BuildJudgment(List<(string Label, Guid PlayerId)> labels, IReadOnlyList<AiRuling> rulings)
    {
        var evaluated = new List<(string Label, Guid PlayerId, string Opinion, int Points)>();
        var usedOpinions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < labels.Count; index++)
        {
            var (label, playerId) = labels[index];
            // Gemini occasionally decorates the structured label ("Réu A", for
            // example). Responses are requested in submission order, so index is
            // a safe fallback that preserves the individual AI opinion instead of
            // replacing it with the generic reserve text.
            var ruling = rulings.FirstOrDefault(r => string.Equals(NormalizeLabel(r.Label), label, StringComparison.OrdinalIgnoreCase))
                ?? rulings.ElementAtOrDefault(index);
            var opinion = ruling?.Opinion;
            if (string.IsNullOrWhiteSpace(opinion))
            {
                logger.LogWarning(
                    "Resposta do juiz IA sem parecer utilizável para o réu {Label}; rulings recebidos: {RulingCount}.",
                    label,
                    rulings.Count);
                opinion = FallbackOpinion(label);
            }
            if (!usedOpinions.Add(opinion.Trim()))
                opinion = $"{opinion.Trim()} Análise individual registrada para o réu {label}.";

            evaluated.Add((label, playerId, opinion, Math.Clamp(ruling?.Points ?? 300, 0, Verdict.MaxPoints)));
        }

        var winner = evaluated
            .OrderByDescending(candidate => candidate.Points)
            .ThenBy(candidate => candidate.Label, StringComparer.Ordinal)
            .FirstOrDefault();

        var verdicts = evaluated.ToDictionary(
            candidate => candidate.PlayerId,
            candidate => candidate.PlayerId == winner.PlayerId
                ? Verdict.Winner(candidate.Opinion)
                : Verdict.Create(candidate.Opinion, Math.Min(candidate.Points, Verdict.MaxPoints - 1)));

        return new RoundJudgment(verdicts);
    }

    private static RoundJudgment Fallback(IReadOnlyList<DefenseSubmission> submissions)
    {
        var evaluated = submissions
            .Select((submission, index) => new
            {
                Label = IndexToLabel(index),
                submission.PlayerId,
                Opinion = FallbackOpinion(IndexToLabel(index)),
                Points = Math.Min(600, 100 + submission.Text.Trim().Length * 3),
            })
            .ToList();
        var winner = evaluated
            .OrderByDescending(candidate => candidate.Points)
            .ThenBy(candidate => candidate.Label, StringComparer.Ordinal)
            .FirstOrDefault();

        return new RoundJudgment(evaluated.ToDictionary(
            candidate => candidate.PlayerId,
            candidate => candidate.PlayerId == winner?.PlayerId
                ? Verdict.Winner(candidate.Opinion)
                : Verdict.Create(candidate.Opinion, candidate.Points)));
    }

    private static string FallbackOpinion(string label) =>
        $"O tribunal registrou a defesa do réu {label} e determina análise complementar específica para esta manifestação.";

    private static string NormalizeLabel(string label) =>
        label.Trim().Replace("réu", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

    private static string IndexToLabel(int i) => ((char)('A' + i)).ToString();
}
