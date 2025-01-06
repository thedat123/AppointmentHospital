using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentHospital.Migrations
{
    /// <inheritdoc />
    public partial class addDiagnosisTable7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiagnosisHistory_Acquaintances_AcquaintanceId",
                table: "DiagnosisHistory");

            migrationBuilder.AddForeignKey(
                name: "FK_DiagnosisHistory_Acquaintances_AcquaintanceId",
                table: "DiagnosisHistory",
                column: "AcquaintanceId",
                principalTable: "Acquaintances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiagnosisHistory_Acquaintances_AcquaintanceId",
                table: "DiagnosisHistory");

            migrationBuilder.AddForeignKey(
                name: "FK_DiagnosisHistory_Acquaintances_AcquaintanceId",
                table: "DiagnosisHistory",
                column: "AcquaintanceId",
                principalTable: "Acquaintances",
                principalColumn: "Id");
        }
    }
}
