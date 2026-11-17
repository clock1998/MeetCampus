using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetCampus.Migrations
{
    /// <inheritdoc />
    public partial class Init6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryScoresJson",
                table: "RecommendationItems");

            migrationBuilder.DropColumn(
                name: "AlgorithmVersion",
                table: "RecommendationBatches");

            migrationBuilder.DropColumn(
                name: "ScoringConfigurationVersionId",
                table: "RecommendationBatches");

            migrationBuilder.DropColumn(
                name: "ScoringWeightsJson",
                table: "RecommendationBatches");

            migrationBuilder.CreateTable(
                name: "RecommendationExplanationCategories",
                columns: table => new
                {
                    RecommendationItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationExplanationCategories", x => new { x.RecommendationItemId, x.QuestionCategoryId });
                    table.ForeignKey(
                        name: "FK_RecommendationExplanationCategories_QuestionCategories_Ques~",
                        column: x => x.QuestionCategoryId,
                        principalTable: "QuestionCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecommendationExplanationCategories_RecommendationItems_Rec~",
                        column: x => x.RecommendationItemId,
                        principalTable: "RecommendationItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationExplanationCategories_QuestionCategoryId",
                table: "RecommendationExplanationCategories",
                column: "QuestionCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationExplanationCategories_RecommendationItemId_So~",
                table: "RecommendationExplanationCategories",
                columns: new[] { "RecommendationItemId", "SortOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecommendationExplanationCategories");

            migrationBuilder.AddColumn<string>(
                name: "CategoryScoresJson",
                table: "RecommendationItems",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AlgorithmVersion",
                table: "RecommendationBatches",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ScoringConfigurationVersionId",
                table: "RecommendationBatches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScoringWeightsJson",
                table: "RecommendationBatches",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
