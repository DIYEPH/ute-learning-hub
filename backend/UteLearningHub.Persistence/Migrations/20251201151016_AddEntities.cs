using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TacGia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenTacGia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DuyetBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NoiDungDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDuyet = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrangThaiDuyet = table.Column<int>(type: "int", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TacGia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaiLieu_Tep",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TepDinhKemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TongSoTrang = table.Column<int>(type: "int", nullable: true),
                    LaChinh = table.Column<bool>(type: "bit", nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiLieu_Tep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaiLieu_Tep_TaiLieu_TaiLieuId",
                        column: x => x.TaiLieuId,
                        principalTable: "TaiLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiLieu_Tep_TepDinhKem_TepDinhKemId",
                        column: x => x.TepDinhKemId,
                        principalTable: "TepDinhKem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaiLieu_TacGia",
                columns: table => new
                {
                    TaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TacGiaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiLieu_TacGia", x => new { x.TaiLieuId, x.TacGiaId });
                    table.ForeignKey(
                        name: "FK_TaiLieu_TacGia_TacGia_TacGiaId",
                        column: x => x.TacGiaId,
                        principalTable: "TacGia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiLieu_TacGia_TaiLieu_TaiLieuId",
                        column: x => x.TaiLieuId,
                        principalTable: "TaiLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichSuDocTaiLieu",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieu_TepId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrangCuoiCung = table.Column<int>(type: "int", nullable: false),
                    TongSoTrang = table.Column<int>(type: "int", nullable: true),
                    LanTruyCapGanNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuDocTaiLieu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuDocTaiLieu_TaiLieu_TaiLieuId",
                        column: x => x.TaiLieuId,
                        principalTable: "TaiLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuDocTaiLieu_TaiLieu_Tep_TaiLieu_TepId",
                        column: x => x.TaiLieu_TepId,
                        principalTable: "TaiLieu_Tep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDocTaiLieu_TaiLieu_TepId",
                table: "LichSuDocTaiLieu",
                column: "TaiLieu_TepId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDocTaiLieu_TaiLieuId",
                table: "LichSuDocTaiLieu",
                column: "TaiLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_TacGia_TacGiaId",
                table: "TaiLieu_TacGia",
                column: "TacGiaId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_Tep_TaiLieuId",
                table: "TaiLieu_Tep",
                column: "TaiLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_Tep_TepDinhKemId",
                table: "TaiLieu_Tep",
                column: "TepDinhKemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LichSuDocTaiLieu");

            migrationBuilder.DropTable(
                name: "TaiLieu_TacGia");

            migrationBuilder.DropTable(
                name: "TaiLieu_Tep");

            migrationBuilder.DropTable(
                name: "TacGia");
        }
    }
}
