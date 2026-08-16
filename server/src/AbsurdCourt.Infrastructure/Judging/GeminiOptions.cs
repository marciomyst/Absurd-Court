namespace AbsurdCourt.Infrastructure.Judging;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    /// <summary>Chave criada no Google AI Studio. O projeto/chave deve estar no plano Free Tier.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Modelo econômico adequado para desenvolvimento e free tier.</summary>
    public string Model { get; set; } = "gemini-3.1-flash-lite";
}
