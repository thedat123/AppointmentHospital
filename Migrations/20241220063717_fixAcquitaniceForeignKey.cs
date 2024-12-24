using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentHospital.Migrations
{
    /// <inheritdoc />
    public partial class fixAcquitaniceForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_AcquaintanceId",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AcquaintanceId",
                table: "Appointments",
                column: "AcquaintanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_AcquaintanceId",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AcquaintanceId",
                table: "Appointments",
                column: "AcquaintanceId",
                unique: true,
                filter: "[AcquaintanceId] IS NOT NULL");
        }
    }
}
