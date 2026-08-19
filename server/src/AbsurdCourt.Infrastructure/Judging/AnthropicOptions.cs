namespace AbsurdCourt.Infrastructure.Judging;

public sealed class AnthropicOptions
{
    public const string SectionName = "Anthropic";

    /// <summary>Set via User Secrets in dev (`dotnet user-secrets set Anthropic:ApiKey "..."`) or an environment variable in production — never committed.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Haiku by default: the judge is called once per round and latency/cost add up — bump to a Sonnet model in config for richer verdicts.</summary>
    public string Model { get; set; } = "claude-haiku-4-5-20251001";
}
