using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentHospital.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAcquaintance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Acquaintances_Patients_Id",
                table: "Acquaintances");

            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "Acquaintances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Acquaintances_PatientId",
                table: "Acquaintances",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Acquaintances_Patients_PatientId",
                table: "Acquaintances",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Acquaintances_Patients_PatientId",
                table: "Acquaintances");

            migrationBuilder.DropIndex(
                name: "IX_Acquaintances_PatientId",
                table: "Acquaintances");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "Acquaintances");

            migrationBuilder.AddForeignKey(
                name: "FK_Acquaintances_Patients_Id",
                table: "Acquaintances",
                column: "Id",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
