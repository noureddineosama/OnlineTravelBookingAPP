using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TestMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the legacy 'refresh_token' (datetime2) column that was added by the
            // AddRefreshTokenToPassengers migration but is now orphaned.
            // The active column is 'refreshToken' (nvarchar(max)) added by UpdatePassengerAndAuditFields.
            // The type mismatch (datetime2 vs nvarchar) caused SqlBuffer.get_String() crashes on login.
            migrationBuilder.DropColumn(
                name: "refresh_token",
                table: "passengers");

            // Also drop the old string 'refresh_token_expiry' that duplicated the camelCase one
            // Only if it exists separately — EF snapshot already maps 'refresh_token_expiry' as nvarchar(max)
            // via the UpdatePassengerAndAuditFields migration, so nothing further needed here.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "refresh_token",
                table: "passengers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}

