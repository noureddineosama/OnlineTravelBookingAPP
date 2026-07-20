using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatesInTableRoomAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "available_units",
                table: "room_availability");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "room_availability",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "room_availability");

            migrationBuilder.AddColumn<int>(
                name: "available_units",
                table: "room_availability",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
