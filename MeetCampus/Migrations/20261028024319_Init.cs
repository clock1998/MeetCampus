using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeetCampus.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllowedEmailDomains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Domain = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedEmailDomains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "QuestionDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    PromptResourceKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Group = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    QuestionVisibility = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScoringWeight = table.Column<decimal>(type: "numeric(8,5)", precision: 8, scale: 5, nullable: false),
                    ExplanationTemplateKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    MaxSelections = table.Column<int>(type: "integer", nullable: true),
                    ScaleMin = table.Column<int>(type: "integer", nullable: true),
                    ScaleMax = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionDefinitions", x => x.Id);
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
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "AccountLifecycles",
                columns: table => new
                {
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DiscoverabilityOptIn = table.Column<bool>(type: "boolean", nullable: false),
                    EmailVerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeactivatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RemovalRequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RemovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SuspendedUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountLifecycles", x => x.ApplicationUserId);
                    table.ForeignKey(
                        name: "FK_AccountLifecycles_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "AuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DetailsJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditEvents_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecondUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_AspNetUsers_FirstUserId",
                        column: x => x.FirstUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Conversations_AspNetUsers_SecondUserId",
                        column: x => x.SecondUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BatchDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AlgorithmVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ScoringConfigurationVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScoringWeightsJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationBatches_AspNetUsers_ViewerUserId",
                        column: x => x.ViewerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBlocks_AspNetUsers_BlockedUserId",
                        column: x => x.BlockedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBlocks_AspNetUsers_BlockerUserId",
                        column: x => x.BlockerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReports_AspNetUsers_ReportedUserId",
                        column: x => x.ReportedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserReports_AspNetUsers_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LabelResourceKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ScoringValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionOptions_QuestionDefinitions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "QuestionDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GenderId = table.Column<Guid>(type: "uuid", nullable: true),
                    SexualityId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudyDomainId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uuid", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "IntentCompatibilityRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderIntentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientIntentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntentCompatibilityRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntentCompatibilityRules_UserIntentions_RecipientIntentId",
                        column: x => x.RecipientIntentId,
                        principalTable: "UserIntentions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IntentCompatibilityRules_UserIntentions_SenderIntentId",
                        column: x => x.SenderIntentId,
                        principalTable: "UserIntentions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContactRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderIntentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientIntentId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactRequests_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactRequests_AspNetUsers_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactRequests_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactRequests_UserIntentions_RecipientIntentId",
                        column: x => x.RecipientIntentId,
                        principalTable: "UserIntentions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactRequests_UserIntentions_SenderIntentId",
                        column: x => x.SenderIntentId,
                        principalTable: "UserIntentions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConversationMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationMessages_AspNetUsers_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConversationMessages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecommendationBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompatibilityScore = table.Column<int>(type: "integer", nullable: false),
                    EvidenceCoverage = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    CategoryScoresJson = table.Column<string>(type: "jsonb", nullable: false),
                    Decision = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationItems_AspNetUsers_CandidateUserId",
                        column: x => x.CandidateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecommendationItems_RecommendationBatches_RecommendationBat~",
                        column: x => x.RecommendationBatchId,
                        principalTable: "RecommendationBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModerationActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModeratorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationActions_AspNetUsers_ModeratorUserId",
                        column: x => x.ModeratorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ModerationActions_UserReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "UserReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireAnswerSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireAnswerSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireAnswerSets_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionnaireAnswerSets_QuestionOptions_QuestionOptionId",
                        column: x => x.QuestionOptionId,
                        principalTable: "QuestionOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfileImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsAvatar = table.Column<bool>(type: "boolean", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfileImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfileImages_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfileIntentions",
                columns: table => new
                {
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserIntentionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfileIntentions", x => new { x.UserProfileId, x.UserIntentionId });
                    table.ForeignKey(
                        name: "FK_UserProfileIntentions_UserIntentions_UserIntentionId",
                        column: x => x.UserIntentionId,
                        principalTable: "UserIntentions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserProfileIntentions_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "6ed3e4b6-231f-41dd-9403-1595f6b8e73d", "Administrator", "ADMINISTRATOR" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "8f3ec64a-af77-4f56-82f4-c9cea9eebd89", "Moderator", "MODERATOR" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "b7d5672e-24dc-4c11-a948-6c89de8c3a40", "Student", "STUDENT" }
                });

            migrationBuilder.InsertData(
                table: "CampusLanguages",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("70000000-0000-0000-0000-000000000001"), "English" },
                    { new Guid("70000000-0000-0000-0000-000000000002"), "French" },
                    { new Guid("70000000-0000-0000-0000-000000000003"), "Mandarin Chinese" },
                    { new Guid("70000000-0000-0000-0000-000000000004"), "Hindi" },
                    { new Guid("70000000-0000-0000-0000-000000000005"), "Spanish" },
                    { new Guid("70000000-0000-0000-0000-000000000006"), "Arabic" },
                    { new Guid("70000000-0000-0000-0000-000000000007"), "Bengali" },
                    { new Guid("70000000-0000-0000-0000-000000000008"), "Portuguese" },
                    { new Guid("70000000-0000-0000-0000-000000000009"), "Russian" },
                    { new Guid("70000000-0000-0000-0000-000000000010"), "Urdu" }
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
                table: "Schools",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("20000000-0000-0000-0000-000000000002"), "McGill University" });

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
                table: "StudyDomains",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "Accounting" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "Aerospace Engineering" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "Architecture" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "Art & Design" },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "Biology" },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "Business Administration" },
                    { new Guid("10000000-0000-0000-0000-000000000007"), "Chemical Engineering" },
                    { new Guid("10000000-0000-0000-0000-000000000008"), "Chemistry" },
                    { new Guid("10000000-0000-0000-0000-000000000009"), "Civil Engineering" },
                    { new Guid("10000000-0000-0000-0000-000000000010"), "Communications" },
                    { new Guid("10000000-0000-0000-0000-000000000011"), "Computer Science" },
                    { new Guid("10000000-0000-0000-0000-000000000012"), "Data Science" },
                    { new Guid("10000000-0000-0000-0000-000000000013"), "Economics" },
                    { new Guid("10000000-0000-0000-0000-000000000014"), "Education" },
                    { new Guid("10000000-0000-0000-0000-000000000015"), "Electrical Engineering" },
                    { new Guid("10000000-0000-0000-0000-000000000016"), "Environmental Science" },
                    { new Guid("10000000-0000-0000-0000-000000000017"), "Film & Media Studies" },
                    { new Guid("10000000-0000-0000-0000-000000000018"), "Finance" },
                    { new Guid("10000000-0000-0000-0000-000000000019"), "Fine Arts" },
                    { new Guid("10000000-0000-0000-0000-000000000020"), "Geography" },
                    { new Guid("10000000-0000-0000-0000-000000000021"), "History" },
                    { new Guid("10000000-0000-0000-0000-000000000022"), "Information Systems" },
                    { new Guid("10000000-0000-0000-0000-000000000023"), "Journalism" },
                    { new Guid("10000000-0000-0000-0000-000000000024"), "Kinesiology" },
                    { new Guid("10000000-0000-0000-0000-000000000025"), "Law" },
                    { new Guid("10000000-0000-0000-0000-000000000026"), "Linguistics" },
                    { new Guid("10000000-0000-0000-0000-000000000027"), "Marketing" },
                    { new Guid("10000000-0000-0000-0000-000000000028"), "Mathematics" },
                    { new Guid("10000000-0000-0000-0000-000000000029"), "Mechanical Engineering" },
                    { new Guid("10000000-0000-0000-0000-000000000030"), "Medicine" },
                    { new Guid("10000000-0000-0000-0000-000000000031"), "Music" },
                    { new Guid("10000000-0000-0000-0000-000000000032"), "Nursing" },
                    { new Guid("10000000-0000-0000-0000-000000000033"), "Nutrition & Dietetics" },
                    { new Guid("10000000-0000-0000-0000-000000000034"), "Pharmacy" },
                    { new Guid("10000000-0000-0000-0000-000000000035"), "Philosophy" },
                    { new Guid("10000000-0000-0000-0000-000000000036"), "Physics" },
                    { new Guid("10000000-0000-0000-0000-000000000037"), "Political Science" },
                    { new Guid("10000000-0000-0000-0000-000000000038"), "Psychology" },
                    { new Guid("10000000-0000-0000-0000-000000000039"), "Public Health" },
                    { new Guid("10000000-0000-0000-0000-000000000040"), "Social Work" },
                    { new Guid("10000000-0000-0000-0000-000000000041"), "Sociology" },
                    { new Guid("10000000-0000-0000-0000-000000000042"), "Software Engineering" },
                    { new Guid("10000000-0000-0000-0000-000000000043"), "Statistics" },
                    { new Guid("10000000-0000-0000-0000-000000000044"), "Theatre" },
                    { new Guid("10000000-0000-0000-0000-000000000045"), "Urban Planning" },
                    { new Guid("10000000-0000-0000-0000-000000000046"), "Other" }
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
                    { new Guid("30000000-0000-0000-0000-000000000003"), "study-partner", "Profile_Intention_StudyPartner", "Study partner" },
                    { new Guid("30000000-0000-0000-0000-000000000004"), "activity-partner", "Profile_Intention_ActivityPartner", "Activity partner" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountLifecycles_Status_DiscoverabilityOptIn_EmailVerified~",
                table: "AccountLifecycles",
                columns: new[] { "Status", "DiscoverabilityOptIn", "EmailVerifiedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountLifecycles_SuspendedUntil",
                table: "AccountLifecycles",
                column: "SuspendedUntil");

            migrationBuilder.CreateIndex(
                name: "IX_AllowedEmailDomains_Domain",
                table: "AllowedEmailDomains",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AllowedEmailDomains_IsEnabled",
                table: "AllowedEmailDomains",
                column: "IsEnabled");

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
                name: "IX_AuditEvents_ActorUserId",
                table: "AuditEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_EntityType_EntityId_OccurredAt",
                table: "AuditEvents",
                columns: new[] { "EntityType", "EntityId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_OccurredAt",
                table: "AuditEvents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_ConversationId",
                table: "ContactRequests",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_RecipientIntentId",
                table: "ContactRequests",
                column: "RecipientIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_RecipientUserId_CreatedAt",
                table: "ContactRequests",
                columns: new[] { "RecipientUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_SenderIntentId",
                table: "ContactRequests",
                column: "SenderIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_SenderUserId_RecipientUserId",
                table: "ContactRequests",
                columns: new[] { "SenderUserId", "RecipientUserId" },
                unique: true,
                filter: "\"State\" IN ('Pending', 'Accepted')");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_SenderUserId_RecipientUserId_CreatedAt",
                table: "ContactRequests",
                columns: new[] { "SenderUserId", "RecipientUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessages_ConversationId_CreatedAt_Id",
                table: "ConversationMessages",
                columns: new[] { "ConversationId", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessages_SenderUserId",
                table: "ConversationMessages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_FirstUserId_SecondUserId",
                table: "Conversations",
                columns: new[] { "FirstUserId", "SecondUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_SecondUserId",
                table: "Conversations",
                column: "SecondUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Genders_Code",
                table: "Genders",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntentCompatibilityRules_RecipientIntentId",
                table: "IntentCompatibilityRules",
                column: "RecipientIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_IntentCompatibilityRules_SenderIntentId_RecipientIntentId",
                table: "IntentCompatibilityRules",
                columns: new[] { "SenderIntentId", "RecipientIntentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModerationActions_ModeratorUserId_CreatedAt",
                table: "ModerationActions",
                columns: new[] { "ModeratorUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationActions_ReportId_CreatedAt",
                table: "ModerationActions",
                columns: new[] { "ReportId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionDefinitions_Code",
                table: "QuestionDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireAnswerSets_ApplicationUserId_CreatedAt",
                table: "QuestionnaireAnswerSets",
                columns: new[] { "ApplicationUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireAnswerSets_QuestionOptionId",
                table: "QuestionnaireAnswerSets",
                column: "QuestionOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_QuestionId_OptionCode",
                table: "QuestionOptions",
                columns: new[] { "QuestionId", "OptionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_QuestionId_SortOrder",
                table: "QuestionOptions",
                columns: new[] { "QuestionId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationBatches_CreatedAt",
                table: "RecommendationBatches",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationBatches_ViewerUserId_BatchDate",
                table: "RecommendationBatches",
                columns: new[] { "ViewerUserId", "BatchDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationItems_CandidateUserId_Decision",
                table: "RecommendationItems",
                columns: new[] { "CandidateUserId", "Decision" });

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationItems_RecommendationBatchId_CandidateUserId",
                table: "RecommendationItems",
                columns: new[] { "RecommendationBatchId", "CandidateUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sexualities_Code",
                table: "Sexualities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBlocks_BlockedUserId_BlockerUserId",
                table: "UserBlocks",
                columns: new[] { "BlockedUserId", "BlockerUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserBlocks_BlockerUserId_BlockedUserId",
                table: "UserBlocks",
                columns: new[] { "BlockerUserId", "BlockedUserId" },
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
                name: "IX_UserProfileImages_UserProfileId",
                table: "UserProfileImages",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfileIntentions_UserIntentionId",
                table: "UserProfileIntentions",
                column: "UserIntentionId");

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

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReportedUserId_CreatedAt",
                table: "UserReports",
                columns: new[] { "ReportedUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReporterUserId",
                table: "UserReports",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_Status_CreatedAt",
                table: "UserReports",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountLifecycles");

            migrationBuilder.DropTable(
                name: "AllowedEmailDomains");

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
                name: "AuditEvents");

            migrationBuilder.DropTable(
                name: "ContactRequests");

            migrationBuilder.DropTable(
                name: "ConversationMessages");

            migrationBuilder.DropTable(
                name: "IntentCompatibilityRules");

            migrationBuilder.DropTable(
                name: "ModerationActions");

            migrationBuilder.DropTable(
                name: "QuestionnaireAnswerSets");

            migrationBuilder.DropTable(
                name: "RecommendationItems");

            migrationBuilder.DropTable(
                name: "UserBlocks");

            migrationBuilder.DropTable(
                name: "UserProfileImages");

            migrationBuilder.DropTable(
                name: "UserProfileIntentions");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "UserReports");

            migrationBuilder.DropTable(
                name: "QuestionOptions");

            migrationBuilder.DropTable(
                name: "RecommendationBatches");

            migrationBuilder.DropTable(
                name: "UserIntentions");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "QuestionDefinitions");

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
        }
    }
}
