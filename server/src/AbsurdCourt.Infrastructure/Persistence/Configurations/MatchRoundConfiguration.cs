using AbsurdCourt.Domain.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbsurdCourt.Infrastructure.Persistence.Configurations;

public sealed class MatchRoundConfiguration : IEntityTypeConfiguration<MatchRound>
{
    public void Configure(EntityTypeBuilder<MatchRound> builder)
    {
        builder.ToTable("MatchRounds");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.CaseFileId);
        builder.Property(r => r.OrderIndex);
        builder.Property(r => r.DeadlineUtc);

        // Concurrency token, not just a status flag: this is what makes the Open->Judging
        // transition safe when "last player submitted" and the deadline sweeper race —
        // whichever SaveChanges loses sees a stale Status and throws, see UnitOfWork.
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(16).IsConcurrencyToken();

        builder.Property<Guid>("MatchId"); // shadow FK, see MatchConfiguration

        builder.HasMany(r => r.Defenses)
            .WithOne()
            .HasForeignKey("MatchRoundId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Defenses).HasField("_defenses").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
