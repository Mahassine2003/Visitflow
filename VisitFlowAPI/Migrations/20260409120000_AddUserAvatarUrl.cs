using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VisitFlowAPI.Data;

#nullable disable

namespace VisitFlowAPI.Migrations;

[DbContext(typeof(VisitFlowDbContext))]
[Migration("20260409120000_AddUserAvatarUrl")]
public class AddUserAvatarUrl : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AvatarUrl",
            table: "Users",
            type: "nvarchar(max)",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AvatarUrl",
            table: "Users");
    }
}
