using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "DoTuongDong",
                table: "CuocTroChuyen_ThanhVien",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ThoiDiemPhanHoi",
                table: "CuocTroChuyen_ThanhVien",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrangThaiLoiMoi",
                table: "CuocTroChuyen_ThanhVien",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ThoiHanDeXuat",
                table: "CuocTroChuyen",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoTuongDong",
                table: "CuocTroChuyen_ThanhVien");

            migrationBuilder.DropColumn(
                name: "ThoiDiemPhanHoi",
                table: "CuocTroChuyen_ThanhVien");

            migrationBuilder.DropColumn(
                name: "TrangThaiLoiMoi",
                table: "CuocTroChuyen_ThanhVien");

            migrationBuilder.DropColumn(
                name: "ThoiHanDeXuat",
                table: "CuocTroChuyen");
        }
    }
}
