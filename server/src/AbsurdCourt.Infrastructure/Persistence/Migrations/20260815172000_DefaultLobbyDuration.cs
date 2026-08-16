using AbsurdCourt.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbsurdCourt.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CourtDbContext))]
[Migration("20260815172000_DefaultLobbyDuration")]
public partial class DefaultLobbyDuration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.Sql("""
                UPDATE "Rooms"
                SET "SettingsRoundDurationSeconds" = 15
                WHERE "Status" = 'Lobby' AND "SettingsRoundDurationSeconds" = 60;
                """);
            return;
        }

        migrationBuilder.Sql("""
            UPDATE Rooms
            SET SettingsRoundDurationSeconds = 15
            WHERE Status = 'Lobby' AND SettingsRoundDurationSeconds = 60;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.Sql("""
                UPDATE "Rooms"
                SET "SettingsRoundDurationSeconds" = 60
                WHERE "Status" = 'Lobby' AND "SettingsRoundDurationSeconds" = 15;
                """);
            return;
        }

        migrationBuilder.Sql("""
            UPDATE Rooms
            SET SettingsRoundDurationSeconds = 60
            WHERE Status = 'Lobby' AND SettingsRoundDurationSeconds = 15;
            """);
    }
}
