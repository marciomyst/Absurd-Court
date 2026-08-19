using System;
using AbsurdCourt.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbsurdCourt.Infrastructure.Persistence.Migrations;

[Migration("20260815153000_HardenReconnectTokens")]
[DbContext(typeof(CourtDbContext))]
public partial class HardenReconnectTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Existing GUID values are intentionally not migrated to hashes. They are
        // invalid under the new verifier, which revokes every pre-hardening token.
        migrationBuilder.AddColumn<DateTime>(
            name: "ReconnectTokenExpiresAtUtc",
            table: "Players",
            type: "TEXT",
            nullable: false,
            defaultValue: DateTime.MinValue);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ReconnectTokenExpiresAtUtc",
            table: "Players");
    }
}
