using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetCampus.Migrations
{
    /// <inheritdoc />
    public partial class Init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_QuestionOptions_QuestionOptionId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_ApplicationUserId_CreatedAt",
                table: "UserAnswers");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "UserAnswers",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionOptionId",
                table: "UserAnswers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "QuestionId",
                table: "UserAnswers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "ScaleValue",
                table: "UserAnswers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_ApplicationUserId_QuestionId_UpdatedAt",
                table: "UserAnswers",
                columns: new[] { "ApplicationUserId", "QuestionId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_QuestionId",
                table: "UserAnswers",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_QuestionOptions_QuestionOptionId",
                table: "UserAnswers",
                column: "QuestionOptionId",
                principalTable: "QuestionOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Questions_QuestionId",
                table: "UserAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_QuestionOptions_QuestionOptionId",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Questions_QuestionId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_ApplicationUserId_QuestionId_UpdatedAt",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_QuestionId",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "QuestionId",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "ScaleValue",
                table: "UserAnswers");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "UserAnswers",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionOptionId",
                table: "UserAnswers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_ApplicationUserId_CreatedAt",
                table: "UserAnswers",
                columns: new[] { "ApplicationUserId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_QuestionOptions_QuestionOptionId",
                table: "UserAnswers",
                column: "QuestionOptionId",
                principalTable: "QuestionOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
