using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeetCampus.Migrations
{
    /// <inheritdoc />
    public partial class AlignLookupSeedsWithMvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"));

            migrationBuilder.UpdateData(
                table: "UserIntentions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                columns: new[] { "Code", "DisplayKey", "Name" },
                values: new object[] { "study-partner", "Profile_Intention_StudyPartner", "Study partner" });

            migrationBuilder.UpdateData(
                table: "UserIntentions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                columns: new[] { "Code", "DisplayKey", "Name" },
                values: new object[] { "activity-partner", "Profile_Intention_ActivityPartner", "Activity partner" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Schools",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "Concordia University" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "Université du Québec à Montréal (UQAM)" },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "Université de Montréal (UdeM)" }
                });

            migrationBuilder.UpdateData(
                table: "UserIntentions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                columns: new[] { "Code", "DisplayKey", "Name" },
                values: new object[] { "relationship", "Profile_Intention_Relationship", "Relationship" });

            migrationBuilder.UpdateData(
                table: "UserIntentions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                columns: new[] { "Code", "DisplayKey", "Name" },
                values: new object[] { "networking", "Profile_Intention_Networking", "Networking" });
        }
    }
}
