using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentHospital.Migrations
{
    /// <inheritdoc />
    public partial class addDiagnosisTable10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiagnosisHistory_Acquaintances_AcquaintanceId",
                table: "DiagnosisHistory");

            migrationBuilder.DropIndex(
                name: "IX_DiagnosisHistory_AcquaintanceId",
                table: "DiagnosisHistory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DiagnosisHistory_AcquaintanceId",
                table: "DiagnosisHistory",
                column: "AcquaintanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiagnosisHistory_Acquaintances_AcquaintanceId",
                table: "DiagnosisHistory",
                column: "AcquaintanceId",
                principalTable: "Acquaintances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
