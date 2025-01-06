using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentHospital.Migrations
{
    /// <inheritdoc />
    public partial class addDiagnosisTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "DiagnosisHistory",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: false),
            DoctorNote = table.Column<string>(type: "nvarchar(max)", nullable: false),
            dateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_DiagnosisHistory", x => x.Id);
            table.ForeignKey(
                name: "FK_DiagnosisHistory_Appointments_AppointmentId",
                column: x => x.AppointmentId,
                principalTable: "Appointments",
                principalColumn: "AppointmentId",
                onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                name: "FK_DiagnosisHistory_Doctors_DoctorId",
                column: x => x.DoctorId,
                principalTable: "Doctors",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                name: "FK_DiagnosisHistory_Patients_PatientId",
                column: x => x.PatientId,
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict); // Change from Cascade to Restrict
        });

    migrationBuilder.CreateTable(
        name: "Drugs",
        columns: table => new
        {
            DrugId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            DrugName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Quantity = table.Column<int>(type: "int", nullable: false),
            UnitPrice = table.Column<double>(type: "float", nullable: false),
            DiagnosisHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Drugs", x => x.DrugId);
            table.ForeignKey(
                name: "FK_Drugs_DiagnosisHistory_DiagnosisHistoryId",
                column: x => x.DiagnosisHistoryId,
                principalTable: "DiagnosisHistory",
                principalColumn: "Id");
        });

    migrationBuilder.CreateIndex(
        name: "IX_DiagnosisHistory_AppointmentId",
        table: "DiagnosisHistory",
        column: "AppointmentId");

    migrationBuilder.CreateIndex(
        name: "IX_DiagnosisHistory_DoctorId",
        table: "DiagnosisHistory",
        column: "DoctorId");

    migrationBuilder.CreateIndex(
        name: "IX_DiagnosisHistory_PatientId",
        table: "DiagnosisHistory",
        column: "PatientId");

    migrationBuilder.CreateIndex(
        name: "IX_Drugs_DiagnosisHistoryId",
        table: "Drugs",
        column: "DiagnosisHistoryId");
}


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Acquaintances_AcquaintanceId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "Drugs");

            migrationBuilder.DropTable(
                name: "DiagnosisHistory");

            migrationBuilder.DropColumn(
                name: "AcquaintanceId",
                table: "Appointments");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Id",
                table: "Appointments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Acquaintances_Id",
                table: "Appointments",
                column: "Id",
                principalTable: "Acquaintances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
