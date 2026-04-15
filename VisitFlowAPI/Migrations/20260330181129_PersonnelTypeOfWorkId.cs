using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitFlowAPI.Migrations
{
    /// <inheritdoc />
    public partial class PersonnelTypeOfWorkId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeOfWorkId",
                table: "Personnels",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personnels_TypeOfWorkId",
                table: "Personnels",
                column: "TypeOfWorkId");

            migrationBuilder.AddForeignKey(
                name: "FK_Personnels_TypeOfWorks_TypeOfWorkId",
                table: "Personnels",
                column: "TypeOfWorkId",
                principalTable: "TypeOfWorks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personnels_TypeOfWorks_TypeOfWorkId",
                table: "Personnels");

            migrationBuilder.DropIndex(
                name: "IX_Personnels_TypeOfWorkId",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "TypeOfWorkId",
                table: "Personnels");
        }
    }
}
