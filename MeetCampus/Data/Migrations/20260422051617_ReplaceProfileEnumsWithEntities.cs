using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeetCampus.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceProfileEnumsWithEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ethnicity",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Intention",
                table: "UserProfiles");

            migrationBuilder.AddColumn<Guid>(
                name: "EthnicityId",
                table: "UserProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "IntentionId",
                table: "UserProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "UserEthnicities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEthnicities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserIntentions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIntentions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "UserEthnicities",
                columns: new[] { "Id", "Code", "DisplayKey", "Name" },
                values: new object[,]
                {
                    { new Guid("40000000-0000-0000-0000-000000000001"), "prefer-not-to-say", "Profile_Ethnicity_PreferNotToSay", "Prefer not to say" },
                    { new Guid("40000000-0000-0000-0000-000000000002"), "arab", "Profile_Ethnicity_Arab", "Arab" },
                    { new Guid("40000000-0000-0000-0000-000000000003"), "black", "Profile_Ethnicity_Black", "Black" },
                    { new Guid("40000000-0000-0000-0000-000000000004"), "east-asian", "Profile_Ethnicity_EastAsian", "East Asian" },
                    { new Guid("40000000-0000-0000-0000-000000000005"), "indigenous", "Profile_Ethnicity_Indigenous", "Indigenous" },
                    { new Guid("40000000-0000-0000-0000-000000000006"), "latino", "Profile_Ethnicity_Latino", "Latino" },
                    { new Guid("40000000-0000-0000-0000-000000000007"), "middle-eastern", "Profile_Ethnicity_MiddleEastern", "Middle Eastern" },
                    { new Guid("40000000-0000-0000-0000-000000000008"), "south-asian", "Profile_Ethnicity_SouthAsian", "South Asian" },
                    { new Guid("40000000-0000-0000-0000-000000000009"), "southeast-asian", "Profile_Ethnicity_SoutheastAsian", "Southeast Asian" },
                    { new Guid("40000000-0000-0000-0000-000000000010"), "white", "Profile_Ethnicity_White", "White" },
                    { new Guid("40000000-0000-0000-0000-000000000011"), "mixed", "Profile_Ethnicity_Mixed", "Mixed" },
                    { new Guid("40000000-0000-0000-0000-000000000012"), "other", "Profile_Ethnicity_Other", "Other" }
                });

            migrationBuilder.InsertData(
                table: "UserIntentions",
                columns: new[] { "Id", "Code", "DisplayKey", "Name" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), "friendship", "Profile_Intention_Friendship", "Friendship" },
                    { new Guid("30000000-0000-0000-0000-000000000002"), "dating", "Profile_Intention_Dating", "Dating" },
                    { new Guid("30000000-0000-0000-0000-000000000003"), "relationship", "Profile_Intention_Relationship", "Relationship" },
                    { new Guid("30000000-0000-0000-0000-000000000004"), "networking", "Profile_Intention_Networking", "Networking" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_EthnicityId",
                table: "UserProfiles",
                column: "EthnicityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_IntentionId",
                table: "UserProfiles",
                column: "IntentionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEthnicities_Code",
                table: "UserEthnicities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserIntentions_Code",
                table: "UserIntentions",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_UserEthnicities_EthnicityId",
                table: "UserProfiles",
                column: "EthnicityId",
                principalTable: "UserEthnicities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_UserIntentions_IntentionId",
                table: "UserProfiles",
                column: "IntentionId",
                principalTable: "UserIntentions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_UserEthnicities_EthnicityId",
                table: "UserProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_UserIntentions_IntentionId",
                table: "UserProfiles");

            migrationBuilder.DropTable(
                name: "UserEthnicities");

            migrationBuilder.DropTable(
                name: "UserIntentions");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_EthnicityId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_IntentionId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "EthnicityId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IntentionId",
                table: "UserProfiles");

            migrationBuilder.AddColumn<string>(
                name: "Ethnicity",
                table: "UserProfiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Intention",
                table: "UserProfiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");
        }
    }
}
