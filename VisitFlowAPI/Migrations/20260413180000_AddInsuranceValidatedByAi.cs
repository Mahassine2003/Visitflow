using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VisitFlowAPI.Data;

#nullable disable

namespace VisitFlowAPI.Migrations;

[DbContext(typeof(VisitFlowDbContext))]
[Migration("20260413180000_AddInsuranceValidatedByAi")]
public partial class AddInsuranceValidatedByAi : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "ValidatedByAi",
            table: "Insurances",
            type: "bit",
            nullable: false,
            defaultValue: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ValidatedByAi",
            table: "Insurances");
    }
}
