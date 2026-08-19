namespace AbsurdCourt.Domain.Matches;

public sealed record Verdict
{
    public const int MinPoints = 0;
    public const int MaxPoints = 1000;

    public string ParecerText { get; }
    public int Points { get; }

    private Verdict(string parecerText, int points)
    {
        ParecerText = parecerText;
        Points = points;
    }

    public static Verdict Create(string parecerText, int points)
    {
        if (string.IsNullOrWhiteSpace(parecerText))
            throw new ArgumentException("O parecer não pode ser vazio.", nameof(parecerText));

        return new Verdict(parecerText.Trim(), Math.Clamp(points, MinPoints, MaxPoints));
    }

    /// <summary>The case's ruling: exactly one submitted defense per round wins outright, at full marks.</summary>
    public static Verdict Winner(string parecerText) => Create(parecerText, MaxPoints);

    /// <summary>Player didn't submit anything before the round closed.</summary>
    public static Verdict Revelia() =>
        new("Ausência de defesa registrada. O tribunal presume o pior, com pesar.", 0);

    /// <summary>The LLM judge call failed or timed out — a single API hiccup shouldn't stall the room.</summary>
    public static Verdict Fallback() =>
        new("O tribunal registra a manifestação e reserva o parecer para instância superior.", 300);
}
