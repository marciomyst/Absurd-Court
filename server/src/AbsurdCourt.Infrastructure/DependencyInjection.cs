using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Infrastructure.Judging;
using AbsurdCourt.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AbsurdCourt.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Court") ?? "Data Source=absurdcourt.db";
        services.AddDbContext<CourtDbContext>(options =>
        {
            if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString, postgres =>
                    postgres.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                return;
            }

            options.UseSqlite(connectionString, sqlite =>
                sqlite.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));

            // The committed history was created with SQLite. Keep the local MVP
            // database compatible while PostgreSQL uses its own current model.
            options.ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<ICaseBankRepository, CaseBankRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        services.Configure<AnthropicOptions>(configuration.GetSection(AnthropicOptions.SectionName));
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));

        services.AddHttpClient<AnthropicJudgeService>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<AnthropicOptions>>().Value;
            client.BaseAddress = new Uri("https://api.anthropic.com");
            client.DefaultRequestHeaders.Add("x-api-key", opts.ApiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        });
        services.AddHttpClient<OpenAiJudgeService>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<OpenAiOptions>>().Value;
            client.BaseAddress = new Uri("https://api.openai.com");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", opts.ApiKey);
        });
        services.AddHttpClient<GeminiJudgeService>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<GeminiOptions>>().Value;
            client.BaseAddress = new Uri("https://generativelanguage.googleapis.com");
            client.DefaultRequestHeaders.Add("x-goog-api-key", opts.ApiKey);
        });
        services.AddSingleton<MockAiProvider>();
        services.AddScoped<IAiProvider>(sp =>
        {
            var provider = sp.GetRequiredService<IOptions<AiOptions>>().Value.Provider;
            return provider.Trim().ToLowerInvariant() switch
            {
                "anthropic" => sp.GetRequiredService<AnthropicJudgeService>(),
                "openai" => sp.GetRequiredService<OpenAiJudgeService>(),
                "gemini" => sp.GetRequiredService<GeminiJudgeService>(),
                "mock" => sp.GetRequiredService<MockAiProvider>(),
                _ => throw new InvalidOperationException($"Provider de IA não suportado: '{provider}'. Use Anthropic, OpenAI ou Gemini."),
            };
        });
        services.AddScoped<IJudgeService, AiJudgeService>();

        return services;
    }
}
