using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentHospital.Migrations
{
    /// <inheritdoc />
    public partial class addDiagnosisTable4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AcquaintanceId",
                table: "DiagnosisHistory",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosisHistory_AcquaintanceId",
                table: "DiagnosisHistory",
                column: "AcquaintanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiagnosisHistory_Acquaintances_AcquaintanceId",
                table: "DiagnosisHistory",
                column: "AcquaintanceId",
                principalTable: "Acquaintances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiagnosisHistory_Acquaintances_AcquaintanceId",
                table: "DiagnosisHistory");

            migrationBuilder.DropIndex(
                name: "IX_DiagnosisHistory_AcquaintanceId",
                table: "DiagnosisHistory");

            migrationBuilder.DropColumn(
                name: "AcquaintanceId",
                table: "DiagnosisHistory");
        }
    }
}
