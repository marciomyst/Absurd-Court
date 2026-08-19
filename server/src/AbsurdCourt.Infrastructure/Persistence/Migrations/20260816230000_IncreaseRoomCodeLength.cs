using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbsurdCourt.Infrastructure.Persistence.Migrations;

[Migration("20260816230000_IncreaseRoomCodeLength")]
public partial class IncreaseRoomCodeLength : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Rooms"
                ALTER COLUMN "Code" TYPE character varying(14);
                """);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Rooms"
                ALTER COLUMN "Code" TYPE character varying(7);
                """);
        }
    }
}
