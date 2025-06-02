using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentHospital.Migrations
{
    /// <inheritdoc />
    public partial class AddInfoSpecialities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Aminities",
                table: "Specialities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Archivement",
                table: "Specialities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Equipment",
                table: "Specialities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Expertise",
                table: "Specialities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Introduction",
                table: "Specialities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Mission",
                table: "Specialities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Service",
                table: "Specialities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Treatment",
                table: "Specialities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SpecialitiesId",
                table: "Doctors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_SpecialitiesId",
                table: "Doctors",
                column: "SpecialitiesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Specialities_SpecialitiesId",
                table: "Doctors",
                column: "SpecialitiesId",
                principalTable: "Specialities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Specialities_SpecialitiesId",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_SpecialitiesId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Aminities",
                table: "Specialities");

            migrationBuilder.DropColumn(
                name: "Archivement",
                table: "Specialities");

            migrationBuilder.DropColumn(
                name: "Equipment",
                table: "Specialities");

            migrationBuilder.DropColumn(
                name: "Expertise",
                table: "Specialities");

            migrationBuilder.DropColumn(
                name: "Introduction",
                table: "Specialities");

            migrationBuilder.DropColumn(
                name: "Mission",
                table: "Specialities");

            migrationBuilder.DropColumn(
                name: "Service",
                table: "Specialities");

            migrationBuilder.DropColumn(
                name: "Treatment",
                table: "Specialities");

            migrationBuilder.DropColumn(
                name: "SpecialitiesId",
                table: "Doctors");
        }
    }
}
