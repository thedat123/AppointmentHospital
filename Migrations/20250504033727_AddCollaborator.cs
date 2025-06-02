using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentHospital.Migrations
{
    /// <inheritdoc />
    public partial class AddCollaborator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CollaboratorId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Collaborators",
                columns: table => new
                {
                    CollaboratorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollaboratorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<bool>(type: "bit", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collaborators", x => x.CollaboratorId);
                    table.ForeignKey(
                        name: "FK_Collaborators_Users_CollaboratorId",
                        column: x => x.CollaboratorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CollaboratorId",
                table: "Appointments",
                column: "CollaboratorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Collaborators_CollaboratorId",
                table: "Appointments",
                column: "CollaboratorId",
                principalTable: "Collaborators",
                principalColumn: "CollaboratorId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Collaborators_CollaboratorId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "Collaborators");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_CollaboratorId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CollaboratorId",
                table: "Appointments");
        }
    }
}
