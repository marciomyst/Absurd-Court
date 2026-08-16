using AbsurdCourt.Api.BackgroundServices;
using AbsurdCourt.Api.RealTime;
using AbsurdCourt.Application.Abstractions;
using MediatR;
using Microsoft.Azure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AbsurdCourt.Api;

public static class DependencyInjection
{
    public const string AngularDevCorsPolicy = "AngularDev";

    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<HubInvocationRateLimiter>();
        services.AddSingleton<PlayerSessionStore>();
        var signalR = services.AddSignalR(options => options.AddFilter(typeof(InvocationRateLimitFilter)));
        if (!string.IsNullOrWhiteSpace(configuration["Azure:SignalR:ConnectionString"]))
        {
            signalR.AddAzureSignalR();
        }
        services.AddScoped<IRoomNotifier, SignalRRoomNotifier>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CourtHub).Assembly));
        services.AddHostedService<RoundDeadlineSweeper>();

        services.AddCors(options =>
        {
            // SignalR needs credentialed CORS (cookies/auth headers aren't used here, but the
            // negotiate handshake still requires AllowCredentials), so no wildcard origin.
            options.AddPolicy(AngularDevCorsPolicy, policy => policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });

        return services;
    }
}
