using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTracker.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropTaskUserIdDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """ALTER TABLE "Tasks" ALTER COLUMN "UserId" DROP DEFAULT;""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """ALTER TABLE "Tasks" ALTER COLUMN "UserId" SET DEFAULT 0;""");
        }
    }
}
