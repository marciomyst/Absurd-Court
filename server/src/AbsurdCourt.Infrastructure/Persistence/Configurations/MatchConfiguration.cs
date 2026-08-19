using System.Text.Json;
using AbsurdCourt.Domain.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbsurdCourt.Infrastructure.Persistence.Configurations;

public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.RoomId);
        builder.HasIndex(m => m.RoomId);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(m => m.StartedAtUtc);
        builder.Property(m => m.EndedAtUtc);
        builder.Property(m => m.RoundDurationSeconds);

        builder.Property<List<Guid>>("_caseFileSequence")
            .HasColumnName("CaseFileSequence")
            .HasConversion(GuidCollectionConverters.ListConverter)
            .Metadata.SetValueComparer(GuidCollectionConverters.ListComparer);

        builder.Property<Dictionary<Guid, int>>("_scores")
            .HasColumnName("Scores")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<Guid, int>>(v, (JsonSerializerOptions?)null) ?? new())
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<Guid, int>>(
                (a, b) => (a ?? new()).OrderBy(x => x.Key).SequenceEqual((b ?? new()).OrderBy(x => x.Key)),
                v => v.Aggregate(0, (h, kv) => HashCode.Combine(h, kv.Key, kv.Value)),
                v => v.ToDictionary(kv => kv.Key, kv => kv.Value)));

        builder.HasMany(m => m.Rounds)
            .WithOne()
            .HasForeignKey("MatchId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(m => m.Rounds).HasField("_rounds").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(m => m.DomainEvents);
        builder.Ignore(m => m.Scores);
        builder.Ignore(m => m.CaseCount);
        builder.Ignore(m => m.CurrentRound);
        builder.Ignore(m => m.HasMoreRounds);
    }
}
