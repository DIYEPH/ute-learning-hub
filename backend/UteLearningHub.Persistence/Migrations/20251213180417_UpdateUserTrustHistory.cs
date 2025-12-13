using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserTrustHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DiemCu",
                table: "LichSuDoTinCay",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DiemMoi",
                table: "LichSuDoTinCay",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "ThucTheId",
                table: "LichSuDoTinCay",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CapDoXacThuc",
                table: "NguoiDung",
                type: "int",
                nullable: false,
                computedColumnSql: "\r\n                CASE \r\n                    WHEN [DiemXacThuc] < 5 THEN 0\r\n                    WHEN [DiemXacThuc] < 15 THEN 1\r\n                    WHEN [DiemXacThuc] < 60 THEN 2\r\n                    WHEN [DiemXacThuc] < 120 THEN 3\r\n                    ELSE 4\r\n                END",
                stored: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldComputedColumnSql: "\r\n                CASE \r\n                    WHEN [DiemXacThuc] < 0 THEN 0\r\n                    WHEN [DiemXacThuc] < 5 THEN 1\r\n                    WHEN [DiemXacThuc] < 40 THEN 2\r\n                    WHEN [DiemXacThuc] < 100 THEN 3\r\n                    WHEN [DiemXacThuc] < 200 THEN 4\r\n                    ELSE 5\r\n                END",
                oldStored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiemCu",
                table: "LichSuDoTinCay");

            migrationBuilder.DropColumn(
                name: "DiemMoi",
                table: "LichSuDoTinCay");

            migrationBuilder.DropColumn(
                name: "ThucTheId",
                table: "LichSuDoTinCay");

            migrationBuilder.AlterColumn<int>(
                name: "CapDoXacThuc",
                table: "NguoiDung",
                type: "int",
                nullable: false,
                computedColumnSql: "\r\n                CASE \r\n                    WHEN [DiemXacThuc] < 0 THEN 0\r\n                    WHEN [DiemXacThuc] < 5 THEN 1\r\n                    WHEN [DiemXacThuc] < 40 THEN 2\r\n                    WHEN [DiemXacThuc] < 100 THEN 3\r\n                    WHEN [DiemXacThuc] < 200 THEN 4\r\n                    ELSE 5\r\n                END",
                stored: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldComputedColumnSql: "\r\n                CASE \r\n                    WHEN [DiemXacThuc] < 5 THEN 0\r\n                    WHEN [DiemXacThuc] < 15 THEN 1\r\n                    WHEN [DiemXacThuc] < 60 THEN 2\r\n                    WHEN [DiemXacThuc] < 120 THEN 3\r\n                    ELSE 4\r\n                END",
                oldStored: true);
        }
    }
}
