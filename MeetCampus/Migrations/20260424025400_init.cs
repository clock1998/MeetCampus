using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeetCampus.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampusLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampusLanguages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sexualities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sexualities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudyDomains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyDomains", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserPasskeys",
                columns: table => new
                {
                    CredentialId = table.Column<byte[]>(type: "bytea", maxLength: 1024, nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserPasskeys", x => x.CredentialId);
                    table.ForeignKey(
                        name: "FK_AspNetUserPasskeys_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    GenderId = table.Column<Guid>(type: "uuid", nullable: true),
                    SexualityId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudyDomainId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntentionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EthnicityId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProfiles_CampusLanguages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "CampusLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Genders_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Sexualities_SexualityId",
                        column: x => x.SexualityId,
                        principalTable: "Sexualities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserProfiles_StudyDomains_StudyDomainId",
                        column: x => x.StudyDomainId,
                        principalTable: "StudyDomains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserProfiles_UserEthnicities_EthnicityId",
                        column: x => x.EthnicityId,
                        principalTable: "UserEthnicities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserProfiles_UserIntentions_IntentionId",
                        column: x => x.IntentionId,
                        principalTable: "UserIntentions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "10000000-0000-0000-0000-000000000001", "6ed3e4b6-231f-41dd-9403-1595f6b8e73d", "Admin", "ADMIN" },
                    { "10000000-0000-0000-0000-000000000002", "8f3ec64a-af77-4f56-82f4-c9cea9eebd89", "PowerUser", "POWERUSER" },
                    { "10000000-0000-0000-0000-000000000003", "b7d5672e-24dc-4c11-a948-6c89de8c3a40", "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "Genders",
                columns: new[] { "Id", "Code", "DisplayKey", "Name" },
                values: new object[,]
                {
                    { new Guid("50000000-0000-0000-0000-000000000001"), "man", "Profile_Gender_Man", "Man" },
                    { new Guid("50000000-0000-0000-0000-000000000002"), "woman", "Profile_Gender_Woman", "Woman" },
                    { new Guid("50000000-0000-0000-0000-000000000003"), "other", "Profile_Gender_Other", "Other" }
                });

            migrationBuilder.InsertData(
                table: "Sexualities",
                columns: new[] { "Id", "Code", "DisplayKey", "Name" },
                values: new object[,]
                {
                    { new Guid("60000000-0000-0000-0000-000000000001"), "straight", "Profile_Sexuality_Straight", "Straight" },
                    { new Guid("60000000-0000-0000-0000-000000000002"), "gay", "Profile_Sexuality_Gay", "Gay" },
                    { new Guid("60000000-0000-0000-0000-000000000003"), "lesbian", "Profile_Sexuality_Lesbian", "Lesbian" },
                    { new Guid("60000000-0000-0000-0000-000000000004"), "bisexual", "Profile_Sexuality_Bisexual", "Bisexual" },
                    { new Guid("60000000-0000-0000-0000-000000000005"), "pansexual", "Profile_Sexuality_Pansexual", "Pansexual" },
                    { new Guid("60000000-0000-0000-0000-000000000006"), "asexual", "Profile_Sexuality_Asexual", "Asexual" },
                    { new Guid("60000000-0000-0000-0000-000000000007"), "queer", "Profile_Sexuality_Queer", "Queer" },
                    { new Guid("60000000-0000-0000-0000-000000000008"), "prefer-not-to-say", "Profile_Sexuality_PreferNotToSay", "Prefer not to say" }
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
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserPasskeys_UserId",
                table: "AspNetUserPasskeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genders_Code",
                table: "Genders",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sexualities_Code",
                table: "Sexualities",
                column: "Code",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_ApplicationUserId",
                table: "UserProfiles",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_EthnicityId",
                table: "UserProfiles",
                column: "EthnicityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_GenderId",
                table: "UserProfiles",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_IntentionId",
                table: "UserProfiles",
                column: "IntentionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_LanguageId",
                table: "UserProfiles",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_SchoolId",
                table: "UserProfiles",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_SexualityId",
                table: "UserProfiles",
                column: "SexualityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_StudyDomainId",
                table: "UserProfiles",
                column: "StudyDomainId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserPasskeys");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CampusLanguages");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropTable(
                name: "Sexualities");

            migrationBuilder.DropTable(
                name: "StudyDomains");

            migrationBuilder.DropTable(
                name: "UserEthnicities");

            migrationBuilder.DropTable(
                name: "UserIntentions");
        }
    }
}
