using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Common.Migrations
{
    /// <inheritdoc />
    public partial class AddIsUnpublishedToQuestionnaire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmailId",
                table: "Questionnaires");

            migrationBuilder.AddColumn<bool>(
                name: "IsUnpublished",
                table: "Questionnaires",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUnpublished",
                table: "Questionnaires");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmailId",
                table: "Questionnaires",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }
    }
}
