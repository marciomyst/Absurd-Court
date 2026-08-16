using AbsurdCourt.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbsurdCourt.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CourtDbContext))]
[Migration("20260815170000_NormalizeRoomCodes")]
public partial class NormalizeRoomCodes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE Rooms
            SET Code = printf('%04d-%04d',
                CAST(substr(Code, 1, 3) AS INTEGER),
                CAST(substr(Code, 5, 3) AS INTEGER))
            WHERE Code GLOB '[0-9][0-9][0-9]-[0-9][0-9][0-9]';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE Rooms
            SET Code = printf('%03d-%03d',
                CAST(substr(Code, 1, 4) AS INTEGER),
                CAST(substr(Code, 6, 4) AS INTEGER))
            WHERE Code GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9]';
            """);
    }
}
