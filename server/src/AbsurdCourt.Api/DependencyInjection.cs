using AbsurdCourt.Api.BackgroundServices;
using AbsurdCourt.Api.RealTime;
using AbsurdCourt.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace AbsurdCourt.Api;

public static class DependencyInjection
{
    public const string AngularDevCorsPolicy = "AngularDev";

    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddSingleton<HubInvocationRateLimiter>();
        services.AddSingleton<PlayerSessionStore>();
        services.AddSignalR(options => options.AddFilter(typeof(InvocationRateLimitFilter)));
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
