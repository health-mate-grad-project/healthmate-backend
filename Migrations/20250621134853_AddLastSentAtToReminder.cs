using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace healthmate_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLastSentAtToReminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSentAt",
                table: "Reminders",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSentAt",
                table: "Reminders");
        }
    }
}
