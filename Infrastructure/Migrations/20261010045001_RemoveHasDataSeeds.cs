using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHasDataSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: HasData was removed from the model so EF no longer manages
            // these rows, but the data already exists in the real database and must
            // remain. The model snapshot is updated so EF stops tracking them.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "locations",
                columns: new[] { "id", "address_line", "city", "country", "latitude", "longitude" },
                values: new object[,]
                {
                    { 1, "Tahrir Square", "Cairo", "Egypt", 30.0444m, 31.2357m },
                    { 2, "Champs-Élysées", "Paris", "France", 48.8566m, 2.3522m },
                    { 3, "Manhattan", "New York", "United States", 40.7128m, -74.0060m },
                    { 4, "Shinjuku", "Tokyo", "Japan", 35.6762m, 139.6503m },
                    { 5, "Westminster", "London", "United Kingdom", 51.5074m, -0.1278m }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "created_at", "name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Passenger" },
                    { 2, new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Admin" }
                });
        }
    }
}
