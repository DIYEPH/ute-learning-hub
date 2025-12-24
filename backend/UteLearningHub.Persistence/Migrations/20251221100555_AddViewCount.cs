using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddViewCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "TaiLieu_Tep",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LoiMoiVaoNhom",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuocTroChuyenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDuocMoiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoiNhan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    ThoiGianPhanHoi = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GhiChuPhanHoi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoiMoiVaoNhom", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoiMoiVaoNhom_CuocTroChuyen_CuocTroChuyenId",
                        column: x => x.CuocTroChuyenId,
                        principalTable: "CuocTroChuyen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoiMoiVaoNhom_NguoiDung_NguoiDuocMoiId",
                        column: x => x.NguoiDuocMoiId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoiMoiVaoNhom_NguoiDung_TaoBoi",
                        column: x => x.TaoBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoiMoiVaoNhom_CuocTroChuyenId",
                table: "LoiMoiVaoNhom",
                column: "CuocTroChuyenId");

            migrationBuilder.CreateIndex(
                name: "IX_LoiMoiVaoNhom_NguoiDuocMoiId",
                table: "LoiMoiVaoNhom",
                column: "NguoiDuocMoiId");

            migrationBuilder.CreateIndex(
                name: "IX_LoiMoiVaoNhom_TaoBoi",
                table: "LoiMoiVaoNhom",
                column: "TaoBoi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoiMoiVaoNhom");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "TaiLieu_Tep");
        }
    }
}
