using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentHospital.Migrations
{
    /// <inheritdoc />
    public partial class addTableInTimeSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Available",
                table: "TimeSlots",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Available",
                table: "TimeSlots");
        }
    }
}
