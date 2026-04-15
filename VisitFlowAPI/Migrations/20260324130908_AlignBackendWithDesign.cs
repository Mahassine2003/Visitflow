using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitFlowAPI.Migrations
{
    /// <inheritdoc />
    public partial class AlignBackendWithDesign : Migration
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

            migrationBuilder.DropForeignKey(
                name: "FK_Interventions_Suppliers_SupplierId",
                table: "Interventions");

            migrationBuilder.DropForeignKey(
                name: "FK_Interventions_TypeOfWorks_TypeOfWorkId",
                table: "Interventions");

            migrationBuilder.DropForeignKey(
                name: "FK_Interventions_Zones_ZoneId",
                table: "Interventions");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonnelTrainings_SupplierPersonnels_PersonnelId",
                table: "PersonnelTrainings");

            migrationBuilder.DropTable(
                name: "SupplierPersonnels");

            migrationBuilder.DropTable(
                name: "Validations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonnelTrainings",
                table: "PersonnelTrainings");

            migrationBuilder.DropIndex(
                name: "IX_PersonnelTrainings_PersonnelId_TrainingId",
                table: "PersonnelTrainings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InterventionPersonnels",
                table: "InterventionPersonnels");

            migrationBuilder.DropIndex(
                name: "IX_InterventionPersonnels_InterventionId",
                table: "InterventionPersonnels");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Zones");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Zones");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordSalt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TypeOfWorks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TypeOfWorks");

            migrationBuilder.DropColumn(
                name: "RequiresPermit",
                table: "TypeOfWorks");

            migrationBuilder.DropColumn(
                name: "RequiresTraining",
                table: "TypeOfWorks");

            migrationBuilder.DropColumn(
                name: "ICE",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PersonnelTrainings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PersonnelTrainings");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "InterventionPersonnels");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Insurances");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "IsValid",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Trainings",
                newName: "IsMandatory");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Interventions",
                newName: "IsHSEValidated");

            migrationBuilder.RenameColumn(
                name: "HSEApprovalStatus",
                table: "Interventions",
                newName: "HSEComment");

            migrationBuilder.RenameColumn(
                name: "SupplierPersonnelId",
                table: "InterventionPersonnels",
                newName: "PersonnelId");

            migrationBuilder.RenameIndex(
                name: "IX_InterventionPersonnels_SupplierPersonnelId",
                table: "InterventionPersonnels",
                newName: "IX_InterventionPersonnels_PersonnelId");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "Insurances",
                newName: "PersonnelId");

            migrationBuilder.RenameColumn(
                name: "EntityType",
                table: "Documents",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "Documents",
                newName: "InterventionId");

            migrationBuilder.RenameColumn(
                name: "DocumentType",
                table: "Documents",
                newName: "FileType");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Zones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TypeOfWorks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TypeOfWorks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Trainings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateOnly>(
                name: "ValidFrom",
                table: "Trainings",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "ValidTo",
                table: "Trainings",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Completed",
                table: "PersonnelTrainings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Interventions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Interventions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Interventions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "InterventionPersonnels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Insurances",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateOnly>(
                name: "IssueDate",
                table: "Insurances",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Insurances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Insurances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "Documents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Clean legacy duplicates/orphans before switching to composite keys and new FKs.
            migrationBuilder.Sql(@"
DELETE ip
FROM [InterventionPersonnels] ip
INNER JOIN (
    SELECT [InterventionId], [PersonnelId]
    FROM [InterventionPersonnels]
    GROUP BY [InterventionId], [PersonnelId]
    HAVING COUNT(*) > 1
) d ON d.[InterventionId] = ip.[InterventionId] AND d.[PersonnelId] = ip.[PersonnelId];
");

            migrationBuilder.Sql(@"
WITH cte AS (
    SELECT *,
           ROW_NUMBER() OVER (PARTITION BY [InterventionId], [PersonnelId] ORDER BY [AssignedAt] DESC) AS rn
    FROM [InterventionPersonnels]
)
DELETE FROM cte WHERE rn > 1;
");

            migrationBuilder.Sql("DELETE FROM [PersonnelTrainings];");
            migrationBuilder.Sql("DELETE FROM [Insurances];");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonnelTrainings",
                table: "PersonnelTrainings",
                columns: new[] { "PersonnelId", "TrainingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_InterventionPersonnels",
                table: "InterventionPersonnels",
                columns: new[] { "InterventionId", "PersonnelId" });

            migrationBuilder.CreateTable(
                name: "Personnels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cin = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBlacklisted = table.Column<bool>(type: "bit", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personnels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Personnels_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TypeOfWorkInsurances",
                columns: table => new
                {
                    TypeOfWorkId = table.Column<int>(type: "int", nullable: false),
                    InsuranceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeOfWorkInsurances", x => new { x.TypeOfWorkId, x.InsuranceId });
                    table.ForeignKey(
                        name: "FK_TypeOfWorkInsurances_Insurances_InsuranceId",
                        column: x => x.InsuranceId,
                        principalTable: "Insurances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TypeOfWorkInsurances_TypeOfWorks_TypeOfWorkId",
                        column: x => x.TypeOfWorkId,
                        principalTable: "TypeOfWorks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TypeOfWorkTrainings",
                columns: table => new
                {
                    TypeOfWorkId = table.Column<int>(type: "int", nullable: false),
                    TrainingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeOfWorkTrainings", x => new { x.TypeOfWorkId, x.TrainingId });
                    table.ForeignKey(
                        name: "FK_TypeOfWorkTrainings_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TypeOfWorkTrainings_TypeOfWorks_TypeOfWorkId",
                        column: x => x.TypeOfWorkId,
                        principalTable: "TypeOfWorks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_CreatedByUserId",
                table: "Interventions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Insurances_PersonnelId",
                table: "Insurances",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_InterventionId",
                table: "Documents",
                column: "InterventionId");

            migrationBuilder.CreateIndex(
                name: "IX_Personnels_Cin",
                table: "Personnels",
                column: "Cin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personnels_SupplierId",
                table: "Personnels",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_TypeOfWorkInsurances_InsuranceId",
                table: "TypeOfWorkInsurances",
                column: "InsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_TypeOfWorkTrainings_TrainingId",
                table: "TypeOfWorkTrainings",
                column: "TrainingId");

            // Ensure CreatedByUserId points to an existing user before adding FK.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Users])
BEGIN
    INSERT INTO [Users] ([FullName], [Email], [PasswordHash], [Role], [CreatedAt])
    VALUES ('System User', 'system@local', '', 'Admin', GETUTCDATE());
END;

DECLARE @uid INT = (SELECT TOP 1 [Id] FROM [Users] ORDER BY [Id]);
UPDATE [Interventions] SET [CreatedByUserId] = @uid WHERE [CreatedByUserId] = 0;
");

            migrationBuilder.Sql(@"
DELETE d
FROM [Documents] d
LEFT JOIN [Interventions] i ON i.[Id] = d.[InterventionId]
WHERE i.[Id] IS NULL;

DELETE ip
FROM [InterventionPersonnels] ip
LEFT JOIN [Interventions] i ON i.[Id] = ip.[InterventionId]
LEFT JOIN [Personnels] p ON p.[Id] = ip.[PersonnelId]
WHERE i.[Id] IS NULL OR p.[Id] IS NULL;

DELETE pt
FROM [PersonnelTrainings] pt
LEFT JOIN [Personnels] p ON p.[Id] = pt.[PersonnelId]
LEFT JOIN [Trainings] t ON t.[Id] = pt.[TrainingId]
WHERE p.[Id] IS NULL OR t.[Id] IS NULL;

DELETE ins
FROM [Insurances] ins
LEFT JOIN [Personnels] p ON p.[Id] = ins.[PersonnelId]
WHERE p.[Id] IS NULL;

DELETE i
FROM [Interventions] i
LEFT JOIN [Suppliers] s ON s.[Id] = i.[SupplierId]
LEFT JOIN [TypeOfWorks] tw ON tw.[Id] = i.[TypeOfWorkId]
LEFT JOIN [Zones] z ON z.[Id] = i.[ZoneId]
LEFT JOIN [Users] u ON u.[Id] = i.[CreatedByUserId]
WHERE s.[Id] IS NULL OR tw.[Id] IS NULL OR z.[Id] IS NULL OR u.[Id] IS NULL;
");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Interventions_InterventionId",
                table: "Documents",
                column: "InterventionId",
                principalTable: "Interventions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Insurances_Personnels_PersonnelId",
                table: "Insurances",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InterventionPersonnels_Interventions_InterventionId",
                table: "InterventionPersonnels",
                column: "InterventionId",
                principalTable: "Interventions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InterventionPersonnels_Personnels_PersonnelId",
                table: "InterventionPersonnels",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Interventions_Suppliers_SupplierId",
                table: "Interventions",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Interventions_TypeOfWorks_TypeOfWorkId",
                table: "Interventions",
                column: "TypeOfWorkId",
                principalTable: "TypeOfWorks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Interventions_Users_CreatedByUserId",
                table: "Interventions",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Interventions_Zones_ZoneId",
                table: "Interventions",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonnelTrainings_Personnels_PersonnelId",
                table: "PersonnelTrainings",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Interventions_InterventionId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Insurances_Personnels_PersonnelId",
                table: "Insurances");

            migrationBuilder.DropForeignKey(
                name: "FK_InterventionPersonnels_Interventions_InterventionId",
                table: "InterventionPersonnels");

            migrationBuilder.DropForeignKey(
                name: "FK_InterventionPersonnels_Personnels_PersonnelId",
                table: "InterventionPersonnels");

            migrationBuilder.DropForeignKey(
                name: "FK_Interventions_Suppliers_SupplierId",
                table: "Interventions");

            migrationBuilder.DropForeignKey(
                name: "FK_Interventions_TypeOfWorks_TypeOfWorkId",
                table: "Interventions");

            migrationBuilder.DropForeignKey(
                name: "FK_Interventions_Users_CreatedByUserId",
                table: "Interventions");

            migrationBuilder.DropForeignKey(
                name: "FK_Interventions_Zones_ZoneId",
                table: "Interventions");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonnelTrainings_Personnels_PersonnelId",
                table: "PersonnelTrainings");

            migrationBuilder.DropTable(
                name: "Personnels");

            migrationBuilder.DropTable(
                name: "TypeOfWorkInsurances");

            migrationBuilder.DropTable(
                name: "TypeOfWorkTrainings");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonnelTrainings",
                table: "PersonnelTrainings");

            migrationBuilder.DropIndex(
                name: "IX_Interventions_CreatedByUserId",
                table: "Interventions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InterventionPersonnels",
                table: "InterventionPersonnels");

            migrationBuilder.DropIndex(
                name: "IX_Insurances_PersonnelId",
                table: "Insurances");

            migrationBuilder.DropIndex(
                name: "IX_Documents_InterventionId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TypeOfWorks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "TypeOfWorks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "Completed",
                table: "PersonnelTrainings");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "InterventionPersonnels");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Insurances");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                table: "Insurances");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Insurances");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Insurances");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "IsMandatory",
                table: "Trainings",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "IsHSEValidated",
                table: "Interventions",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "HSEComment",
                table: "Interventions",
                newName: "HSEApprovalStatus");

            migrationBuilder.RenameColumn(
                name: "PersonnelId",
                table: "InterventionPersonnels",
                newName: "SupplierPersonnelId");

            migrationBuilder.RenameIndex(
                name: "IX_InterventionPersonnels_PersonnelId",
                table: "InterventionPersonnels",
                newName: "IX_InterventionPersonnels_SupplierPersonnelId");

            migrationBuilder.RenameColumn(
                name: "PersonnelId",
                table: "Insurances",
                newName: "SupplierId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Documents",
                newName: "EntityType");

            migrationBuilder.RenameColumn(
                name: "InterventionId",
                table: "Documents",
                newName: "EntityId");

            migrationBuilder.RenameColumn(
                name: "FileType",
                table: "Documents",
                newName: "DocumentType");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Zones",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Zones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Zones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<byte[]>(
                name: "PasswordHash",
                table: "Users",
                type: "varbinary(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordSalt",
                table: "Users",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TypeOfWorks",
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
                name: "RequiresPermit",
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

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ICE",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Suppliers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PersonnelTrainings",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "PersonnelTrainings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "StartDate",
                table: "Interventions",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "EndDate",
                table: "Interventions",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Interventions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "Interventions",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "Interventions",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "InterventionPersonnels",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Insurances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "Documents",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsValid",
                table: "Documents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "Documents",
                type: "date",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonnelTrainings",
                table: "PersonnelTrainings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InterventionPersonnels",
                table: "InterventionPersonnels",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SupplierPersonnels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    CIN = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FieldOfActivity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBlacklisted = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierPersonnels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierPersonnels_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Validations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterventionId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelTrainings_PersonnelId_TrainingId",
                table: "PersonnelTrainings",
                columns: new[] { "PersonnelId", "TrainingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterventionPersonnels_InterventionId",
                table: "InterventionPersonnels",
                column: "InterventionId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPersonnels_CIN",
                table: "SupplierPersonnels",
                column: "CIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPersonnels_SupplierId",
                table: "SupplierPersonnels",
                column: "SupplierId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Interventions_Suppliers_SupplierId",
                table: "Interventions",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Interventions_TypeOfWorks_TypeOfWorkId",
                table: "Interventions",
                column: "TypeOfWorkId",
                principalTable: "TypeOfWorks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Interventions_Zones_ZoneId",
                table: "Interventions",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonnelTrainings_SupplierPersonnels_PersonnelId",
                table: "PersonnelTrainings",
                column: "PersonnelId",
                principalTable: "SupplierPersonnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
