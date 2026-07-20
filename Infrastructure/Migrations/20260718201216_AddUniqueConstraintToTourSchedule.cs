using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToTourSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tour_schedules_tour_start",
                table: "tour_schedules");

            migrationBuilder.CreateIndex(
                name: "IX_tour_schedules_tour_start",
                table: "tour_schedules",
                columns: new[] { "tour_id", "start_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tour_schedules_tour_start",
                table: "tour_schedules");

            migrationBuilder.CreateIndex(
                name: "IX_tour_schedules_tour_start",
                table: "tour_schedules",
                columns: new[] { "tour_id", "start_date" });
        }
    }
}
