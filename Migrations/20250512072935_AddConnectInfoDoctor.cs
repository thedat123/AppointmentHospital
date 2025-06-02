using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentHospital.Migrations
{
    /// <inheritdoc />
    public partial class AddConnectInfoDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Specializaiton",
                table: "Doctors");

            migrationBuilder.AddColumn<string>(
                name: "Awards",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Expertise",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Introduction",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrganizationMember",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResearchProject",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrainingProcess",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkExperience",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Awards",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Expertise",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Introduction",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "OrganizationMember",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ResearchProject",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "TrainingProcess",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "WorkExperience",
                table: "Doctors");

            migrationBuilder.AddColumn<int>(
                name: "Specializaiton",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
