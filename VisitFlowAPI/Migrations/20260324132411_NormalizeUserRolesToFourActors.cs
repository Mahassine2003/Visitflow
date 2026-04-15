using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitFlowAPI.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeUserRolesToFourActors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE [Users]
SET [Role] = 'User'
WHERE UPPER([Role]) = 'SUPPLIER';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: legacy Supplier role is intentionally removed.
        }
    }
}
