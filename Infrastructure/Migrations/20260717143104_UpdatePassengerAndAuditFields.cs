using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePassengerAndAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "refreshToken",
                table: "passengers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "refresh_token_expiry",
                table: "passengers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "bookings",
                type: "int",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: 2,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10,
                oldDefaultValue: "pending");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "refreshToken",
                table: "passengers");

            migrationBuilder.DropColumn(
                name: "refresh_token_expiry",
                table: "passengers");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "bookings",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: "pending",
                oldClrType: typeof(int),
                oldType: "int",
                oldUnicode: false,
                oldMaxLength: 10,
                oldDefaultValue: 2);
        }
    }
}
