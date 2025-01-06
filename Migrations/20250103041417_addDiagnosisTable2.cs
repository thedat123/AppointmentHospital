using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentHospital.Migrations
{
    /// <inheritdoc />
    public partial class addDiagnosisTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drugs_DiagnosisHistory_DiagnosisHistoryId",
                table: "Drugs");

            migrationBuilder.DropIndex(
                name: "IX_Drugs_DiagnosisHistoryId",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "DiagnosisHistoryId",
                table: "Drugs");

            migrationBuilder.AddColumn<string>(
                name: "Prescription",
                table: "DiagnosisHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prescription",
                table: "DiagnosisHistory");

            migrationBuilder.AddColumn<Guid>(
                name: "DiagnosisHistoryId",
                table: "Drugs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_DiagnosisHistoryId",
                table: "Drugs",
                column: "DiagnosisHistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drugs_DiagnosisHistory_DiagnosisHistoryId",
                table: "Drugs",
                column: "DiagnosisHistoryId",
                principalTable: "DiagnosisHistory",
                principalColumn: "Id");
        }
    }
}
