using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC.Migrations
{
    /// <inheritdoc />
    public partial class RevertCommentsToComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ParentCommentId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                table: "Comments");

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: -4,
                columns: new[] { "Category", "Created" },
                values: new object[] { 0, new DateTime(2024, 12, 1, 12, 22, 27, 802, DateTimeKind.Local).AddTicks(8675) });

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: -3,
                columns: new[] { "Category", "Created" },
                values: new object[] { 0, new DateTime(2024, 12, 1, 12, 22, 27, 801, DateTimeKind.Local).AddTicks(7802) });

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: -2,
                column: "Created",
                value: new DateTime(2024, 12, 1, 12, 22, 27, 798, DateTimeKind.Local).AddTicks(2590));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "Comments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ParentCommentId",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: -4,
                columns: new[] { "Category", "Created" },
                values: new object[] { 2, new DateTime(2024, 11, 29, 12, 13, 18, 57, DateTimeKind.Local).AddTicks(4505) });

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: -3,
                columns: new[] { "Category", "Created" },
                values: new object[] { 2, new DateTime(2024, 11, 29, 12, 13, 18, 57, DateTimeKind.Local).AddTicks(3259) });

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: -2,
                column: "Created",
                value: new DateTime(2024, 11, 29, 12, 13, 18, 54, DateTimeKind.Local).AddTicks(1354));

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId",
                principalTable: "Comments",
                principalColumn: "Id");
        }
    }
}
