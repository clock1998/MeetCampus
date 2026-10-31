using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetCampus.Migrations
{
    /// <inheritdoc />
    public partial class Init1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireAnswerSets_AspNetUsers_ApplicationUserId",
                table: "QuestionnaireAnswerSets");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireAnswerSets_QuestionOptions_QuestionOptionId",
                table: "QuestionnaireAnswerSets");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionOptions_QuestionDefinitions_QuestionId",
                table: "QuestionOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuestionnaireAnswerSets",
                table: "QuestionnaireAnswerSets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuestionDefinitions",
                table: "QuestionDefinitions");

            migrationBuilder.DropColumn(
                name: "LabelResourceKey",
                table: "QuestionOptions");

            migrationBuilder.DropColumn(
                name: "ExplanationTemplateKey",
                table: "QuestionDefinitions");

            migrationBuilder.DropColumn(
                name: "PromptResourceKey",
                table: "QuestionDefinitions");

            migrationBuilder.DropColumn(
                name: "QuestionVisibility",
                table: "QuestionDefinitions");

            migrationBuilder.RenameTable(
                name: "QuestionnaireAnswerSets",
                newName: "UserAnswers");

            migrationBuilder.RenameTable(
                name: "QuestionDefinitions",
                newName: "Questions");

            migrationBuilder.RenameIndex(
                name: "IX_QuestionnaireAnswerSets_QuestionOptionId",
                table: "UserAnswers",
                newName: "IX_UserAnswers_QuestionOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_QuestionnaireAnswerSets_ApplicationUserId_CreatedAt",
                table: "UserAnswers",
                newName: "IX_UserAnswers_ApplicationUserId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_QuestionDefinitions_Code",
                table: "Questions",
                newName: "IX_Questions_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAnswers",
                table: "UserAnswers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Questions",
                table: "Questions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "QuestionOptionTranslations",
                columns: table => new
                {
                    QuestionOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CultureCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Text = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionOptionTranslations", x => new { x.QuestionOptionId, x.CultureCode });
                    table.ForeignKey(
                        name: "FK_QuestionOptionTranslations_QuestionOptions_QuestionOptionId",
                        column: x => x.QuestionOptionId,
                        principalTable: "QuestionOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionTranslations",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CultureCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionTranslations", x => new { x.QuestionId, x.CultureCode });
                    table.ForeignKey(
                        name: "FK_QuestionTranslations_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionOptions_Questions_QuestionId",
                table: "QuestionOptions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_AspNetUsers_ApplicationUserId",
                table: "UserAnswers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_QuestionOptions_QuestionOptionId",
                table: "UserAnswers",
                column: "QuestionOptionId",
                principalTable: "QuestionOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionOptions_Questions_QuestionId",
                table: "QuestionOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_AspNetUsers_ApplicationUserId",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_QuestionOptions_QuestionOptionId",
                table: "UserAnswers");

            migrationBuilder.DropTable(
                name: "QuestionOptionTranslations");

            migrationBuilder.DropTable(
                name: "QuestionTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAnswers",
                table: "UserAnswers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Questions",
                table: "Questions");

            migrationBuilder.RenameTable(
                name: "UserAnswers",
                newName: "QuestionnaireAnswerSets");

            migrationBuilder.RenameTable(
                name: "Questions",
                newName: "QuestionDefinitions");

            migrationBuilder.RenameIndex(
                name: "IX_UserAnswers_QuestionOptionId",
                table: "QuestionnaireAnswerSets",
                newName: "IX_QuestionnaireAnswerSets_QuestionOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAnswers_ApplicationUserId_CreatedAt",
                table: "QuestionnaireAnswerSets",
                newName: "IX_QuestionnaireAnswerSets_ApplicationUserId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_Code",
                table: "QuestionDefinitions",
                newName: "IX_QuestionDefinitions_Code");

            migrationBuilder.AddColumn<string>(
                name: "LabelResourceKey",
                table: "QuestionOptions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExplanationTemplateKey",
                table: "QuestionDefinitions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PromptResourceKey",
                table: "QuestionDefinitions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuestionVisibility",
                table: "QuestionDefinitions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuestionnaireAnswerSets",
                table: "QuestionnaireAnswerSets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuestionDefinitions",
                table: "QuestionDefinitions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireAnswerSets_AspNetUsers_ApplicationUserId",
                table: "QuestionnaireAnswerSets",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireAnswerSets_QuestionOptions_QuestionOptionId",
                table: "QuestionnaireAnswerSets",
                column: "QuestionOptionId",
                principalTable: "QuestionOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionOptions_QuestionDefinitions_QuestionId",
                table: "QuestionOptions",
                column: "QuestionId",
                principalTable: "QuestionDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
