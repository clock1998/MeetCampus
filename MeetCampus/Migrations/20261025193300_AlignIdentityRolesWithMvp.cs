using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetCampus.Migrations
{
    /// <inheritdoc />
    public partial class AlignIdentityRolesWithMvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "10000000-0000-0000-0000-000000000001",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Administrator", "ADMINISTRATOR" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "10000000-0000-0000-0000-000000000002",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Moderator", "MODERATOR" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "10000000-0000-0000-0000-000000000003",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Student", "STUDENT" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "10000000-0000-0000-0000-000000000001",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Admin", "ADMIN" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "10000000-0000-0000-0000-000000000002",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "PowerUser", "POWERUSER" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "10000000-0000-0000-0000-000000000003",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "User", "USER" });
        }
    }
}
