using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentHospital.Migrations
{
    /// <inheritdoc />
    public partial class addAcquaintenceForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AcquaintanceId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AcquaintanceId",
                table: "Appointments",
                column: "AcquaintanceId",
                unique: true,
                filter: "[AcquaintanceId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Acquaintances_AcquaintanceId",
                table: "Appointments",
                column: "AcquaintanceId",
                principalTable: "Acquaintances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Acquaintances_AcquaintanceId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_AcquaintanceId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "AcquaintanceId",
                table: "Appointments");
        }
    }
}
