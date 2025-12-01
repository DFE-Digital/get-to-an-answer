using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Common.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Questionnaires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Contributors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PublishedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionTrackingMap = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    BackgroundColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    PrimaryButtonColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    SecondaryButtonColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    StateColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    ErrorMessageColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    DecorativeImage = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ContinueButtonText = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsAccessibilityAgreementAccepted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questionnaires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionnaireJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ChangeDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeLog = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contents_Questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "Questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "Questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<float>(type: "real", nullable: false),
                    DestinationType = table.Column<int>(type: "int", nullable: true),
                    DestinationUrl = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DestinationQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DestinationContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId_IsDeleted",
                table: "Answers",
                columns: new[] { "QuestionId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionnaireId_IsDeleted",
                table: "Answers",
                columns: new[] { "QuestionnaireId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Contents_QuestionnaireId",
                table: "Contents",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Questionnaires_Slug",
                table: "Questionnaires",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Questionnaires_Slug_Status",
                table: "Questionnaires",
                columns: new[] { "Slug", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireVersions_QuestionnaireId",
                table: "QuestionnaireVersions",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireVersions_QuestionnaireId_Version",
                table: "QuestionnaireVersions",
                columns: new[] { "QuestionnaireId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuestionnaireId_Id",
                table: "Questions",
                columns: new[] { "QuestionnaireId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuestionnaireId_IsDeleted",
                table: "Questions",
                columns: new[] { "QuestionnaireId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuestionnaireId_Order",
                table: "Questions",
                columns: new[] { "QuestionnaireId", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "Contents");

            migrationBuilder.DropTable(
                name: "QuestionnaireVersions");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Questionnaires");
        }
    }
}
