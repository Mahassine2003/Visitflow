using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitFlowAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddInterventionWizardFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomFieldsJson",
                table: "Interventions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InterventionWizardFieldDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FieldType = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterventionWizardFieldDefinitions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterventionWizardFieldDefinitions_Key",
                table: "InterventionWizardFieldDefinitions",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterventionWizardFieldDefinitions");

            migrationBuilder.DropColumn(
                name: "CustomFieldsJson",
                table: "Interventions");
        }
    }
}
