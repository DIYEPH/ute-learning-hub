using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileVectorAndConversationVector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DacTrungCuocTroChuyen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuocTroChuyenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonHocId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LoaiVector = table.Column<int>(type: "int", nullable: false),
                    SoChieu = table.Column<int>(type: "int", nullable: false),
                    GiaTriVector = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NguonDuLieu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhienBanMoHinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KieuDoTuongDong = table.Column<int>(type: "int", nullable: false),
                    ThoiDiemTinhToan = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ConHieuLuc = table.Column<bool>(type: "bit", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DacTrungCuocTroChuyen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DacTrungCuocTroChuyen_CuocTroChuyen_CuocTroChuyenId",
                        column: x => x.CuocTroChuyenId,
                        principalTable: "CuocTroChuyen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DacTrungCuocTroChuyen_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DacTrungNguoiDung",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonHocId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LoaiVector = table.Column<int>(type: "int", nullable: false),
                    SoChieu = table.Column<int>(type: "int", nullable: false),
                    GiaTriVector = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NguonDuLieu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhienBanMoHinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KieuDoTuongDong = table.Column<int>(type: "int", nullable: false),
                    ThoiDiemTinhToan = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ConHieuLuc = table.Column<bool>(type: "bit", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DacTrungNguoiDung", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DacTrungNguoiDung_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DacTrungCuocTroChuyen_CuocTroChuyenId",
                table: "DacTrungCuocTroChuyen",
                column: "CuocTroChuyenId");

            migrationBuilder.CreateIndex(
                name: "IX_DacTrungCuocTroChuyen_MonHocId",
                table: "DacTrungCuocTroChuyen",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_DacTrungNguoiDung_MonHocId",
                table: "DacTrungNguoiDung",
                column: "MonHocId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DacTrungCuocTroChuyen");

            migrationBuilder.DropTable(
                name: "DacTrungNguoiDung");
        }
    }
}
