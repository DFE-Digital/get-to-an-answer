using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Common.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShowResultsSummaryToContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowResultsSummary",
                table: "Contents",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowResultsSummary",
                table: "Contents");
        }
    }
}
