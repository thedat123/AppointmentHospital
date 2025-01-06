using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentHospital.Migrations
{
    /// <inheritdoc />
    public partial class addacquaintanceIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Acquaintances_AcquaintanceId",
                table: "Appointments");

            migrationBuilder.AlterColumn<Guid>(
                name: "AcquaintanceId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

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

            migrationBuilder.AlterColumn<Guid>(
                name: "AcquaintanceId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

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
