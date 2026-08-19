using AbsurdCourt.Domain.Common;

namespace AbsurdCourt.Domain.Matches;

/// <summary>Reference data (the accusation bank) — no invariants of its own, so it's a plain Entity, not an aggregate.</summary>
public sealed class CaseFile : Entity
{
    public string Autos { get; private set; } = string.Empty;
    public string Hint { get; private set; } = string.Empty;

    private CaseFile() { }

    public CaseFile(Guid id, string autos, string hint) : base(id)
    {
        Autos = autos;
        Hint = hint;
    }
}
