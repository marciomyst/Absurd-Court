namespace AbsurdCourt.Infrastructure.Judging;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    public string Provider { get; set; } = "Anthropic";
}
