using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AbsurdCourt.Infrastructure.Persistence;

/// <summary>Lets `dotnet ef` construct CourtDbContext directly at design time, without spinning up the whole app's DI container (which needs services — like the LLM judge — that migrations don't care about).</summary>
public sealed class CourtDbContextFactory : IDesignTimeDbContextFactory<CourtDbContext>
{
    public CourtDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CourtDbContext>()
            .UseSqlite("Data Source=absurdcourt.db")
            .Options;

        return new CourtDbContext(options);
    }
}
