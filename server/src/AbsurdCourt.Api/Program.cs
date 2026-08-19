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
builder.Services.AddApi(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CourtDbContext>();

    // The MVP uses SQLite migrations locally. The production PostgreSQL database
    // starts empty and is provisioned from the current relational model until a
    // provider-specific migration history is introduced.
    if (db.Database.IsNpgsql())
    {
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        await db.Database.MigrateAsync();
    }

    await CaseBankSeeder.SeedAsync(db);
}

app.UseHttpsRedirection();
app.UseCors(AbsurdCourt.Api.DependencyInjection.AngularDevCorsPolicy);
app.UseMiddleware<PlayerSessionCookieMiddleware>();
app.MapHub<CourtHub>("/hubs/court");

app.Run();
