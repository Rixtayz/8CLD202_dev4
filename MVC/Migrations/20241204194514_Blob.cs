using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC.Migrations
{
    /// <inheritdoc />
    public partial class Blob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Image",
                table: "Posts",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "BlobImage",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: -4,
                columns: new[] { "BlobImage", "Created" },
                values: new object[] { null, new DateTime(2024, 12, 4, 14, 45, 13, 228, DateTimeKind.Local).AddTicks(8430) });

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: -3,
                columns: new[] { "BlobImage", "Created" },
                values: new object[] { null, new DateTime(2024, 12, 4, 14, 45, 13, 228, DateTimeKind.Local).AddTicks(308) });

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: -2,
                columns: new[] { "BlobImage", "Created" },
                values: new object[] { null, new DateTime(2024, 12, 4, 14, 45, 13, 227, DateTimeKind.Local).AddTicks(2032) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlobImage",
                table: "Posts");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Image",
                table: "Posts",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: -4,
                column: "Created",
                value: new DateTime(2024, 12, 1, 12, 22, 27, 802, DateTimeKind.Local).AddTicks(8675));

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: -3,
                column: "Created",
                value: new DateTime(2024, 12, 1, 12, 22, 27, 801, DateTimeKind.Local).AddTicks(7802));

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: -2,
                column: "Created",
                value: new DateTime(2024, 12, 1, 12, 22, 27, 798, DateTimeKind.Local).AddTicks(2590));
        }
    }
}
