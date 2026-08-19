using AbsurdCourt.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbsurdCourt.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CourtDbContext))]
[Migration("20260815171000_NormalizeRoomCodesToHex")]
public partial class NormalizeRoomCodesToHex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.Sql("""
                UPDATE "Rooms"
                SET "Code" = '0000-' || SPLIT_PART("Code", '-', 1) || '-' || SPLIT_PART("Code", '-', 2)
                WHERE "Code" ~ '^[0-9]{4}-[0-9]{4}$';
                """);
            return;
        }

        migrationBuilder.Sql("""
            UPDATE Rooms
            SET Code = '0000-' || substr(Code, 1, 4) || '-' || substr(Code, 6, 4)
            WHERE Code GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9]';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.Sql("""
                UPDATE "Rooms"
                SET "Code" = SPLIT_PART("Code", '-', 2) || '-' || SPLIT_PART("Code", '-', 3)
                WHERE "Code" ~ '^[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}$';
                """);
            return;
        }

        migrationBuilder.Sql("""
            UPDATE Rooms
            SET Code = substr(Code, 6, 4) || '-' || substr(Code, 11, 4)
            WHERE Code GLOB '[0-9A-F][0-9A-F][0-9A-F][0-9A-F]-[0-9A-F][0-9A-F][0-9A-F][0-9A-F]-[0-9A-F][0-9A-F][0-9A-F][0-9A-F]';
            """);
    }
}
