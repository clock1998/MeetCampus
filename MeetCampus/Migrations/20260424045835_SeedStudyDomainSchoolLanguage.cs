using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeetCampus.Migrations
{
    /// <inheritdoc />
    public partial class SeedStudyDomainSchoolLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                table: "Schools",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "Concordia University" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "McGill University" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "Université du Québec à Montréal (UQAM)" },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "Université de Montréal (UdeM)" }
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CampusLanguages",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CampusLanguages",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "CampusLanguages",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "CampusLanguages",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "CampusLanguages",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "CampusLanguages",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "CampusLanguages",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "CampusLanguages",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "CampusLanguages",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "CampusLanguages",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000022"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000024"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000025"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000026"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000027"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000028"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000029"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000030"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000031"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000032"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000033"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000034"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000035"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000036"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000037"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000038"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000039"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000040"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000041"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000042"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000043"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000044"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000045"));

            migrationBuilder.DeleteData(
                table: "StudyDomains",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000046"));
        }
    }
}
