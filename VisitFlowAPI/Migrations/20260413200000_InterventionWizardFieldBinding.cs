using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VisitFlowAPI.Data;

#nullable disable

namespace VisitFlowAPI.Migrations;

[DbContext(typeof(VisitFlowDbContext))]
[Migration("20260413200000_InterventionWizardFieldBinding")]
public partial class InterventionWizardFieldBinding : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "CreationMode",
            table: "InterventionWizardFieldDefinitions",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "SourceSchema",
            table: "InterventionWizardFieldDefinitions",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SourceTable",
            table: "InterventionWizardFieldDefinitions",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SourceColumn",
            table: "InterventionWizardFieldDefinitions",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "SourceColumn", table: "InterventionWizardFieldDefinitions");
        migrationBuilder.DropColumn(name: "SourceTable", table: "InterventionWizardFieldDefinitions");
        migrationBuilder.DropColumn(name: "SourceSchema", table: "InterventionWizardFieldDefinitions");
        migrationBuilder.DropColumn(name: "CreationMode", table: "InterventionWizardFieldDefinitions");
    }
}
