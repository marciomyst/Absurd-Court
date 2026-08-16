namespace AbsurdCourt.Domain.Matches;

public sealed record DefenseText
{
    public const int MaxLength = 200;

    public string Value { get; }

    private DefenseText(string value) => Value = value;

    public static DefenseText Create(string raw)
    {
        var trimmed = (raw ?? string.Empty).Trim();
        if (trimmed.Length == 0)
            throw new ArgumentException("A defesa não pode ser vazia.", nameof(raw));
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"A defesa não pode ter mais de {MaxLength} caracteres.", nameof(raw));

        return new DefenseText(trimmed);
    }
}
