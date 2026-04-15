using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitFlowAPI.Migrations
{
    /// <inheritdoc />
    public partial class InterventionWorkflowPermits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirePermitDetails",
                table: "Interventions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeightPermitDetails",
                table: "Interventions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HseFormDetails",
                table: "Interventions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirePermitDetails",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "HeightPermitDetails",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "HseFormDetails",
                table: "Interventions");
        }
    }
}
