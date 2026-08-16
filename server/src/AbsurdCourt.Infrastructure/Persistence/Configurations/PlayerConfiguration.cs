using AbsurdCourt.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbsurdCourt.Infrastructure.Persistence.Configurations;

public sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name).HasMaxLength(24).IsRequired();
        builder.Property(p => p.Initials).HasMaxLength(2).IsRequired();
        builder.Property(p => p.IsHost);
        builder.Property(p => p.ConnectionId).HasMaxLength(64);
        builder.Ignore(p => p.ReconnectToken);
        builder.Property(p => p.ReconnectTokenHash).HasColumnName("ReconnectToken").HasMaxLength(64).IsRequired();
        builder.Property(p => p.ReconnectTokenExpiresAtUtc).IsRequired();
        builder.Property(p => p.IsConnected);
        builder.Property(p => p.JoinedAtUtc);

        builder.Property<Guid>("RoomId"); // shadow FK, see RoomConfiguration
    }
}
