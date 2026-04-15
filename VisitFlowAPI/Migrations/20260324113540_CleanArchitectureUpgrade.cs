using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitFlowAPI.Migrations
{
    /// <inheritdoc />
    public partial class CleanArchitectureUpgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterventionPersonnels_Interventions_InterventionId",
                table: "InterventionPersonnels");

            migrationBuilder.DropForeignKey(
                name: "FK_InterventionPersonnels_SupplierPersonnels_SupplierPersonnelId",
                table: "InterventionPersonnels");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Zones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TypeOfWorks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresTraining",
                table: "TypeOfWorks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Suppliers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SupplierPersonnels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Interventions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Interventions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Insurances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insurances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trainings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Validations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterventionId = table.Column<int>(type: "int", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidatedByRole = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Validations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Validations_Interventions_InterventionId",
                        column: x => x.InterventionId,
                        principalTable: "Interventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonnelTrainings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonnelId = table.Column<int>(type: "int", nullable: false),
                    TrainingId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompletionDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnelTrainings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonnelTrainings_SupplierPersonnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "SupplierPersonnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonnelTrainings_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelTrainings_PersonnelId_TrainingId",
                table: "PersonnelTrainings",
                columns: new[] { "PersonnelId", "TrainingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelTrainings_TrainingId",
                table: "PersonnelTrainings",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_Validations_InterventionId",
                table: "Validations",
                column: "InterventionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InterventionPersonnels_Interventions_InterventionId",
                table: "InterventionPersonnels",
                column: "InterventionId",
                principalTable: "Interventions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InterventionPersonnels_SupplierPersonnels_SupplierPersonnelId",
                table: "InterventionPersonnels",
                column: "SupplierPersonnelId",
                principalTable: "SupplierPersonnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterventionPersonnels_Interventions_InterventionId",
                table: "InterventionPersonnels");

            migrationBuilder.DropForeignKey(
                name: "FK_InterventionPersonnels_SupplierPersonnels_SupplierPersonnelId",
                table: "InterventionPersonnels");

            migrationBuilder.DropTable(
                name: "Insurances");

            migrationBuilder.DropTable(
                name: "PersonnelTrainings");

            migrationBuilder.DropTable(
                name: "Validations");

            migrationBuilder.DropTable(
                name: "Trainings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Zones");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TypeOfWorks");

            migrationBuilder.DropColumn(
                name: "RequiresTraining",
                table: "TypeOfWorks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SupplierPersonnels");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Interventions");

            migrationBuilder.AddForeignKey(
                name: "FK_InterventionPersonnels_Interventions_InterventionId",
                table: "InterventionPersonnels",
                column: "InterventionId",
                principalTable: "Interventions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InterventionPersonnels_SupplierPersonnels_SupplierPersonnelId",
                table: "InterventionPersonnels",
                column: "SupplierPersonnelId",
                principalTable: "SupplierPersonnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
