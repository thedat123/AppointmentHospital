using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentHospital.Migrations
{
    /// <inheritdoc />
    public partial class fixAcquaintanceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Acquaintances_AcquaintanceId",
                table: "Appointments");


            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Acquaintances_AcquaintanceId",
                table: "Appointments",
                column: "AcquaintanceId",
                principalTable: "Acquaintances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Acquaintances_AcquaintanceId",
                table: "Appointments");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Acquaintances_AcquaintanceId",
                table: "Appointments",
                column: "AcquaintanceId",
                principalTable: "Acquaintances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
