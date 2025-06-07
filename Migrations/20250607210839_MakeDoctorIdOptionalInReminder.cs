using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace healthmate_backend.Migrations
{
    /// <inheritdoc />
    public partial class MakeDoctorIdOptionalInReminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "AvailableSlots");

            migrationBuilder.AddColumn<string>(
                name: "DayOfWeek",
                table: "AvailableSlots",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "AvailableSlots");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "AvailableSlots",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
