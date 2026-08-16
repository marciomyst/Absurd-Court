using System.Reflection;
using AbsurdCourt.Domain.Matches;
using AbsurdCourt.Domain.Rooms;
using Microsoft.EntityFrameworkCore;

namespace AbsurdCourt.Infrastructure.Persistence;

public sealed class CourtDbContext(DbContextOptions<CourtDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<CaseFile> CaseFiles => Set<CaseFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
