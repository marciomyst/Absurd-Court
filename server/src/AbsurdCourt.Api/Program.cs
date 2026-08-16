using AbsurdCourt.Api;
using AbsurdCourt.Api.RealTime;
using AbsurdCourt.Application;
using AbsurdCourt.Infrastructure;
using AbsurdCourt.Infrastructure.Persistence;
using AbsurdCourt.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Keep API diagnostics available in local/container environments where the Windows
// Event Log provider may be installed but not writable by the application identity.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CourtDbContext>();
    await db.Database.MigrateAsync();
    await CaseBankSeeder.SeedAsync(db);
}

app.UseHttpsRedirection();
app.UseCors(AbsurdCourt.Api.DependencyInjection.AngularDevCorsPolicy);
app.UseMiddleware<PlayerSessionCookieMiddleware>();
app.MapHub<CourtHub>("/hubs/court");

app.Run();
