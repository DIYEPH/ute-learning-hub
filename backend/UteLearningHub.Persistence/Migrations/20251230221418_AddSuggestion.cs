using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSuggestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SuggestionPool",
                table: "SuggestionPool");

            migrationBuilder.DropIndex(
                name: "IX_SuggestionPool_NguoiDungId_DaXuLy",
                table: "SuggestionPool");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SuggestionPool");

            migrationBuilder.DropColumn(
                name: "DaXuLy",
                table: "SuggestionPool");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SuggestionPool");

            migrationBuilder.DropColumn(
                name: "ThoiGianXuLy",
                table: "SuggestionPool");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SuggestionPool");

            migrationBuilder.RenameTable(
                name: "SuggestionPool",
                newName: "GoiYPool");

            migrationBuilder.RenameColumn(
                name: "ThoiGianThem",
                table: "GoiYPool",
                newName: "ThoiDiemThem");

            migrationBuilder.AddColumn<string>(
                name: "LyDoKichHoat",
                table: "GoiYPool",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GoiYPool",
                table: "GoiYPool",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_GoiYPool_NguoiDungId",
                table: "GoiYPool",
                column: "NguoiDungId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GoiYPool",
                table: "GoiYPool");

            migrationBuilder.DropIndex(
                name: "IX_GoiYPool_NguoiDungId",
                table: "GoiYPool");

            migrationBuilder.DropColumn(
                name: "LyDoKichHoat",
                table: "GoiYPool");

            migrationBuilder.RenameTable(
                name: "GoiYPool",
                newName: "SuggestionPool");

            migrationBuilder.RenameColumn(
                name: "ThoiDiemThem",
                table: "SuggestionPool",
                newName: "ThoiGianThem");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "SuggestionPool",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "DaXuLy",
                table: "SuggestionPool",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SuggestionPool",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ThoiGianXuLy",
                table: "SuggestionPool",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "SuggestionPool",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SuggestionPool",
                table: "SuggestionPool",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SuggestionPool_NguoiDungId_DaXuLy",
                table: "SuggestionPool",
                columns: new[] { "NguoiDungId", "DaXuLy" });
        }
    }
}
