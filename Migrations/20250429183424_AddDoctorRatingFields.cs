using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace healthmate_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorRatingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "Doctors",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TotalRatings",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "TotalRatings",
                table: "Doctors");
        }
    }
}
