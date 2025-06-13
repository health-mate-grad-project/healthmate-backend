using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace healthmate_backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMedicalHistoryColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "medicalHistory",
                table: "Patients");

            migrationBuilder.AddColumn<int>(
                name: "AvailableSlotId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DoseTakens",
                columns: table => new
                {
                    ReminderId = table.Column<int>(type: "int", nullable: false),
                    ScheduledTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TakenTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoseTakens", x => new { x.ReminderId, x.ScheduledTimeUtc });
                    table.ForeignKey(
                        name: "FK_DoseTakens_Reminders_ReminderId",
                        column: x => x.ReminderId,
                        principalTable: "Reminders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OtpVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Otp = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiryTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsVerified = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpVerifications", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AvailableSlotId",
                table: "Appointments",
                column: "AvailableSlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AvailableSlots_AvailableSlotId",
                table: "Appointments",
                column: "AvailableSlotId",
                principalTable: "AvailableSlots",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_AvailableSlots_AvailableSlotId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "DoseTakens");

            migrationBuilder.DropTable(
                name: "OtpVerifications");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_AvailableSlotId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "AvailableSlotId",
                table: "Appointments");

            migrationBuilder.AddColumn<string>(
                name: "medicalHistory",
                table: "Patients",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
