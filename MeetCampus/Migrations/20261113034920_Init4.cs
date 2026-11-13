using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeetCampus.Migrations
{
    /// <inheritdoc />
    public partial class Init4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "Questions");

            migrationBuilder.AlterColumn<decimal>(
                name: "CompatibilityScore",
                table: "RecommendationItems",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Questions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "QuestionCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NameFr = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MatchingWeight = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionCategories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "QuestionCategories",
                columns: new[] { "Id", "Code", "MatchingWeight", "NameEn", "NameFr", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("80000000-0000-0000-0000-000000000001"), "interests", 0.25m, "Interests and activities", "Intérêts et activités", 0 },
                    { new Guid("80000000-0000-0000-0000-000000000002"), "values", 0.30m, "Values and priorities", "Valeurs et priorités", 1 },
                    { new Guid("80000000-0000-0000-0000-000000000003"), "social", 0.20m, "Social and communication preferences", "Préférences sociales et de communication", 2 },
                    { new Guid("80000000-0000-0000-0000-000000000004"), "lifestyle", 0.15m, "Lifestyle and availability", "Mode de vie et disponibilités", 3 },
                    { new Guid("80000000-0000-0000-0000-000000000005"), "connection-intent", 0.10m, "Connection intent and expectations", "Intentions et attentes relationnelles", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CategoryId",
                table: "Questions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionCategories_Code",
                table: "QuestionCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionCategories_SortOrder",
                table: "QuestionCategories",
                column: "SortOrder",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_QuestionCategories_CategoryId",
                table: "Questions",
                column: "CategoryId",
                principalTable: "QuestionCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_QuestionCategories_CategoryId",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "QuestionCategories");

            migrationBuilder.DropIndex(
                name: "IX_Questions_CategoryId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Questions");

            migrationBuilder.AlterColumn<int>(
                name: "CompatibilityScore",
                table: "RecommendationItems",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Questions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "Questions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
