using AbsurdCourt.Domain.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbsurdCourt.Infrastructure.Persistence.Configurations;

public sealed class DefenseConfiguration : IEntityTypeConfiguration<Defense>
{
    public void Configure(EntityTypeBuilder<Defense> builder)
    {
        builder.ToTable("Defenses");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.PlayerId);
        builder.Property(d => d.Text).HasMaxLength(DefenseText.MaxLength).IsRequired();
        builder.Property(d => d.WasSubmitted);
        builder.Property(d => d.SubmittedAtUtc);

        builder.OwnsOne(d => d.Verdict, v =>
        {
            v.Property(x => x.ParecerText).HasColumnName("VerdictParecerText").HasMaxLength(1000);
            v.Property(x => x.Points).HasColumnName("VerdictPoints");
        });
        builder.Navigation(d => d.Verdict).IsRequired(false);

        builder.Property<Guid>("MatchRoundId"); // shadow FK, see MatchRoundConfiguration
    }
}
