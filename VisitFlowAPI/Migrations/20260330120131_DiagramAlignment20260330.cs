using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitFlowAPI.Migrations
{
    /// <inheritdoc />
    public partial class DiagramAlignment20260330 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interventions_Zones_ZoneId",
                table: "Interventions");

            migrationBuilder.DropIndex(
                name: "IX_Interventions_ZoneId",
                table: "Interventions");

            migrationBuilder.DropIndex(
                name: "IX_Documents_InterventionId",
                table: "Documents");

            migrationBuilder.Sql("""
                WITH cte AS (
                    SELECT Id, ROW_NUMBER() OVER (PARTITION BY InterventionId ORDER BY Id) AS rn
                    FROM Documents
                )
                DELETE FROM Documents WHERE Id IN (SELECT Id FROM cte WHERE rn > 1);
                """);

            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InterventionZones",
                columns: table => new
                {
                    InterventionId = table.Column<int>(type: "int", nullable: false),
                    ZoneId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterventionZones", x => new { x.InterventionId, x.ZoneId });
                    table.ForeignKey(
                        name: "FK_InterventionZones_Interventions_InterventionId",
                        column: x => x.InterventionId,
                        principalTable: "Interventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterventionZones_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                INSERT INTO [InterventionZones] ([InterventionId], [ZoneId])
                SELECT [Id], [ZoneId] FROM [Interventions];
                """);

            migrationBuilder.DropColumn(
                name: "ZoneId",
                table: "Interventions");

            migrationBuilder.AddColumn<string>(
                name: "ComponentId",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ComponentName",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Personnels",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Personnels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Personnels",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Interventions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MinPersonnel",
                table: "Interventions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinZone",
                table: "Interventions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Ppi",
                table: "Interventions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VisitId",
                table: "Interventions",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO [Visits] ([VisitId], [UserId], [CreatedAt])
                SELECT CONCAT(N'mig-', CAST([Id] AS nvarchar(20))), [CreatedByUserId], [CreatedAt] FROM [Interventions];

                UPDATE [i]
                SET [i].[VisitId] = [v].[Id]
                FROM [Interventions] AS [i]
                INNER JOIN [Visits] AS [v] ON [v].[VisitId] = CONCAT(N'mig-', CAST([i].[Id] AS nvarchar(20)));

                ALTER TABLE [Interventions] ALTER COLUMN [VisitId] int NOT NULL;
                """);

            migrationBuilder.CreateTable(
                name: "Approveds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterventionId = table.Column<int>(type: "int", nullable: false),
                    ApproverName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApproverRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredApproversCount = table.Column<int>(type: "int", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Approveds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Approveds_Interventions_InterventionId",
                        column: x => x.InterventionId,
                        principalTable: "Interventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlacklistRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: false),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlacklistRequests_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlacklistRequests_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TitleOrFilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    ValidatedByAI = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PersonnelId = table.Column<int>(type: "int", nullable: true),
                    TypeOfWorkId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceItems", x => x.Id);
                    table.CheckConstraint("CK_ComplianceItem_Owner", "([PersonnelId] IS NOT NULL AND [TypeOfWorkId] IS NULL) OR ([PersonnelId] IS NULL AND [TypeOfWorkId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ComplianceItems_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComplianceItems_TypeOfWorks_TypeOfWorkId",
                        column: x => x.TypeOfWorkId,
                        principalTable: "TypeOfWorks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ElementTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElementTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Forms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Forms_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SafetyMeasures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterventionId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyMeasures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyMeasures_Interventions_InterventionId",
                        column: x => x.InterventionId,
                        principalTable: "Interventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ElementOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ElementTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElementOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElementOptions_ElementTypes_ElementTypeId",
                        column: x => x.ElementTypeId,
                        principalTable: "ElementTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterventionElements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterventionId = table.Column<int>(type: "int", nullable: false),
                    FormId = table.Column<int>(type: "int", nullable: true),
                    ElementTypeId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsChecked = table.Column<bool>(type: "bit", nullable: false),
                    Context = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterventionElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterventionElements_ElementTypes_ElementTypeId",
                        column: x => x.ElementTypeId,
                        principalTable: "ElementTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterventionElements_Forms_FormId",
                        column: x => x.FormId,
                        principalTable: "Forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterventionElements_Interventions_InterventionId",
                        column: x => x.InterventionId,
                        principalTable: "Interventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterventionPlants",
                columns: table => new
                {
                    InterventionId = table.Column<int>(type: "int", nullable: false),
                    PlantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterventionPlants", x => new { x.InterventionId, x.PlantId });
                    table.ForeignKey(
                        name: "FK_InterventionPlants_Interventions_InterventionId",
                        column: x => x.InterventionId,
                        principalTable: "Interventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterventionPlants_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_InterventionId",
                table: "Documents",
                column: "InterventionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Approveds_InterventionId",
                table: "Approveds",
                column: "InterventionId");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistRequests_PersonnelId",
                table: "BlacklistRequests",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistRequests_ReviewedByUserId",
                table: "BlacklistRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceItems_PersonnelId",
                table: "ComplianceItems",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceItems_TypeOfWorkId",
                table: "ComplianceItems",
                column: "TypeOfWorkId");

            migrationBuilder.CreateIndex(
                name: "IX_ElementOptions_ElementTypeId",
                table: "ElementOptions",
                column: "ElementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_DocumentId",
                table: "Forms",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_InterventionElements_ElementTypeId",
                table: "InterventionElements",
                column: "ElementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InterventionElements_FormId",
                table: "InterventionElements",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_InterventionElements_InterventionId",
                table: "InterventionElements",
                column: "InterventionId");

            migrationBuilder.CreateIndex(
                name: "IX_InterventionPlants_PlantId",
                table: "InterventionPlants",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_InterventionZones_ZoneId",
                table: "InterventionZones",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyMeasures_InterventionId",
                table: "SafetyMeasures",
                column: "InterventionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId",
                table: "Sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_UserId",
                table: "Visits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_VisitId",
                table: "Visits",
                column: "VisitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_VisitId",
                table: "Interventions",
                column: "VisitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Interventions_Visits_VisitId",
                table: "Interventions",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interventions_Visits_VisitId",
                table: "Interventions");

            migrationBuilder.DropIndex(
                name: "IX_Interventions_VisitId",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "VisitId",
                table: "Interventions");

            migrationBuilder.DropTable(
                name: "Approveds");

            migrationBuilder.DropTable(
                name: "BlacklistRequests");

            migrationBuilder.DropTable(
                name: "ComplianceItems");

            migrationBuilder.DropTable(
                name: "ElementOptions");

            migrationBuilder.DropTable(
                name: "InterventionElements");

            migrationBuilder.DropTable(
                name: "InterventionPlants");

            migrationBuilder.DropTable(
                name: "InterventionZones");

            migrationBuilder.DropTable(
                name: "SafetyMeasures");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Visits");

            migrationBuilder.DropTable(
                name: "ElementTypes");

            migrationBuilder.DropTable(
                name: "Forms");

            migrationBuilder.DropTable(
                name: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_Documents_InterventionId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ComponentId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "ComponentName",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "MinPersonnel",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "MinZone",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "Ppi",
                table: "Interventions");

            migrationBuilder.AddColumn<int>(
                name: "ZoneId",
                table: "Interventions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_InterventionId",
                table: "Documents",
                column: "InterventionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Interventions_Zones_ZoneId",
                table: "Interventions",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
