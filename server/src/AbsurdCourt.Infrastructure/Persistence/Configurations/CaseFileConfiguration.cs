using AbsurdCourt.Domain.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbsurdCourt.Infrastructure.Persistence.Configurations;

public sealed class CaseFileConfiguration : IEntityTypeConfiguration<CaseFile>
{
    public void Configure(EntityTypeBuilder<CaseFile> builder)
    {
        builder.ToTable("CaseFiles");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Autos).HasMaxLength(500).IsRequired();
        builder.Property(c => c.Hint).HasMaxLength(80).IsRequired();
    }
}
