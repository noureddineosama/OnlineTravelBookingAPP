using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteAndCancellation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE tours SET status = '1' WHERE status = 'active' OR status = 'Active'");
            migrationBuilder.Sql("UPDATE tours SET status = '0' WHERE status = 'draft' OR status = 'Draft'");
            migrationBuilder.Sql("UPDATE tours SET status = '2' WHERE status = 'cancelled' OR status = 'Cancelled'");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "tours",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10,
                oldDefaultValue: "draft");

            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason_details",
                table: "tours",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cancellation_reason_type",
                table: "tours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_at",
                table: "tours",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "cancelled_by",
                table: "tours",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "tours",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "deleted_by",
                table: "tours",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "tours",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "bookings",
                type: "int",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldUnicode: false,
                oldMaxLength: 10,
                oldDefaultValue: 2);

            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason_details",
                table: "bookings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cancellation_reason_type",
                table: "bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_at",
                table: "bookings",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancellation_reason_details",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "cancellation_reason_type",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "cancelled_by",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "cancellation_reason_details",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "cancellation_reason_type",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "bookings");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "tours",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: "draft",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "bookings",
                type: "int",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: 2,
                oldClrType: typeof(int),
                oldType: "int",
                oldUnicode: false,
                oldMaxLength: 10,
                oldDefaultValue: 1);
        }
    }
}
