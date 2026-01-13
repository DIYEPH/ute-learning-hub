using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChuong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaoCao_TaiLieu_Tep_TaiLieu_TepId",
                table: "BaoCao");

            migrationBuilder.DropForeignKey(
                name: "FK_BinhLuan_TaiLieu_Tep_TaiLieu_TepId",
                table: "BinhLuan");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGiaTaiLieu_TaiLieu_Tep_TaiLieu_TepId",
                table: "DanhGiaTaiLieu");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuDocTaiLieu_TaiLieu_Tep_TaiLieu_TepId",
                table: "LichSuDocTaiLieu");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiLieu_Tep_TaiLieu_TaiLieuId",
                table: "TaiLieu_Tep");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiLieu_Tep_TepDinhKem_TepBiaId",
                table: "TaiLieu_Tep");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiLieu_Tep_TepDinhKem_TepDinhKemId",
                table: "TaiLieu_Tep");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaiLieu_Tep",
                table: "TaiLieu_Tep");

            migrationBuilder.RenameTable(
                name: "TaiLieu_Tep",
                newName: "Chuong");

            migrationBuilder.RenameColumn(
                name: "DocumentName",
                table: "TaiLieu",
                newName: "TenTaiLieu");

            migrationBuilder.RenameColumn(
                name: "TaiLieu_TepId",
                table: "LichSuDocTaiLieu",
                newName: "ChuongId");

            migrationBuilder.RenameIndex(
                name: "IX_LichSuDocTaiLieu_TaiLieu_TepId",
                table: "LichSuDocTaiLieu",
                newName: "IX_LichSuDocTaiLieu_ChuongId");

            migrationBuilder.RenameColumn(
                name: "TaiLieu_TepId",
                table: "DanhGiaTaiLieu",
                newName: "ChuongId");

            migrationBuilder.RenameIndex(
                name: "IX_DanhGiaTaiLieu_TaiLieu_TepId",
                table: "DanhGiaTaiLieu",
                newName: "IX_DanhGiaTaiLieu_ChuongId");

            migrationBuilder.RenameColumn(
                name: "TaiLieu_TepId",
                table: "BinhLuan",
                newName: "ChuongId");

            migrationBuilder.RenameIndex(
                name: "IX_BinhLuan_TaiLieu_TepId",
                table: "BinhLuan",
                newName: "IX_BinhLuan_ChuongId");

            migrationBuilder.RenameColumn(
                name: "TaiLieu_TepId",
                table: "BaoCao",
                newName: "ChuongId");

            migrationBuilder.RenameIndex(
                name: "IX_BaoCao_TaiLieu_TepId",
                table: "BaoCao",
                newName: "IX_BaoCao_ChuongId");

            migrationBuilder.RenameIndex(
                name: "IX_TaiLieu_Tep_TepDinhKemId",
                table: "Chuong",
                newName: "IX_Chuong_TepDinhKemId");

            migrationBuilder.RenameIndex(
                name: "IX_TaiLieu_Tep_TepBiaId",
                table: "Chuong",
                newName: "IX_Chuong_TepBiaId");

            migrationBuilder.RenameIndex(
                name: "IX_TaiLieu_Tep_TaiLieuId",
                table: "Chuong",
                newName: "IX_Chuong_TaiLieuId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chuong",
                table: "Chuong",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BaoCao_Chuong_ChuongId",
                table: "BaoCao",
                column: "ChuongId",
                principalTable: "Chuong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BinhLuan_Chuong_ChuongId",
                table: "BinhLuan",
                column: "ChuongId",
                principalTable: "Chuong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chuong_TaiLieu_TaiLieuId",
                table: "Chuong",
                column: "TaiLieuId",
                principalTable: "TaiLieu",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chuong_TepDinhKem_TepBiaId",
                table: "Chuong",
                column: "TepBiaId",
                principalTable: "TepDinhKem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chuong_TepDinhKem_TepDinhKemId",
                table: "Chuong",
                column: "TepDinhKemId",
                principalTable: "TepDinhKem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGiaTaiLieu_Chuong_ChuongId",
                table: "DanhGiaTaiLieu",
                column: "ChuongId",
                principalTable: "Chuong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuDocTaiLieu_Chuong_ChuongId",
                table: "LichSuDocTaiLieu",
                column: "ChuongId",
                principalTable: "Chuong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaoCao_Chuong_ChuongId",
                table: "BaoCao");

            migrationBuilder.DropForeignKey(
                name: "FK_BinhLuan_Chuong_ChuongId",
                table: "BinhLuan");

            migrationBuilder.DropForeignKey(
                name: "FK_Chuong_TaiLieu_TaiLieuId",
                table: "Chuong");

            migrationBuilder.DropForeignKey(
                name: "FK_Chuong_TepDinhKem_TepBiaId",
                table: "Chuong");

            migrationBuilder.DropForeignKey(
                name: "FK_Chuong_TepDinhKem_TepDinhKemId",
                table: "Chuong");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGiaTaiLieu_Chuong_ChuongId",
                table: "DanhGiaTaiLieu");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuDocTaiLieu_Chuong_ChuongId",
                table: "LichSuDocTaiLieu");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chuong",
                table: "Chuong");

            migrationBuilder.RenameTable(
                name: "Chuong",
                newName: "TaiLieu_Tep");

            migrationBuilder.RenameColumn(
                name: "TenTaiLieu",
                table: "TaiLieu",
                newName: "DocumentName");

            migrationBuilder.RenameColumn(
                name: "ChuongId",
                table: "LichSuDocTaiLieu",
                newName: "TaiLieu_TepId");

            migrationBuilder.RenameIndex(
                name: "IX_LichSuDocTaiLieu_ChuongId",
                table: "LichSuDocTaiLieu",
                newName: "IX_LichSuDocTaiLieu_TaiLieu_TepId");

            migrationBuilder.RenameColumn(
                name: "ChuongId",
                table: "DanhGiaTaiLieu",
                newName: "TaiLieu_TepId");

            migrationBuilder.RenameIndex(
                name: "IX_DanhGiaTaiLieu_ChuongId",
                table: "DanhGiaTaiLieu",
                newName: "IX_DanhGiaTaiLieu_TaiLieu_TepId");

            migrationBuilder.RenameColumn(
                name: "ChuongId",
                table: "BinhLuan",
                newName: "TaiLieu_TepId");

            migrationBuilder.RenameIndex(
                name: "IX_BinhLuan_ChuongId",
                table: "BinhLuan",
                newName: "IX_BinhLuan_TaiLieu_TepId");

            migrationBuilder.RenameColumn(
                name: "ChuongId",
                table: "BaoCao",
                newName: "TaiLieu_TepId");

            migrationBuilder.RenameIndex(
                name: "IX_BaoCao_ChuongId",
                table: "BaoCao",
                newName: "IX_BaoCao_TaiLieu_TepId");

            migrationBuilder.RenameIndex(
                name: "IX_Chuong_TepDinhKemId",
                table: "TaiLieu_Tep",
                newName: "IX_TaiLieu_Tep_TepDinhKemId");

            migrationBuilder.RenameIndex(
                name: "IX_Chuong_TepBiaId",
                table: "TaiLieu_Tep",
                newName: "IX_TaiLieu_Tep_TepBiaId");

            migrationBuilder.RenameIndex(
                name: "IX_Chuong_TaiLieuId",
                table: "TaiLieu_Tep",
                newName: "IX_TaiLieu_Tep_TaiLieuId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaiLieu_Tep",
                table: "TaiLieu_Tep",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BaoCao_TaiLieu_Tep_TaiLieu_TepId",
                table: "BaoCao",
                column: "TaiLieu_TepId",
                principalTable: "TaiLieu_Tep",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BinhLuan_TaiLieu_Tep_TaiLieu_TepId",
                table: "BinhLuan",
                column: "TaiLieu_TepId",
                principalTable: "TaiLieu_Tep",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGiaTaiLieu_TaiLieu_Tep_TaiLieu_TepId",
                table: "DanhGiaTaiLieu",
                column: "TaiLieu_TepId",
                principalTable: "TaiLieu_Tep",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuDocTaiLieu_TaiLieu_Tep_TaiLieu_TepId",
                table: "LichSuDocTaiLieu",
                column: "TaiLieu_TepId",
                principalTable: "TaiLieu_Tep",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaiLieu_Tep_TaiLieu_TaiLieuId",
                table: "TaiLieu_Tep",
                column: "TaiLieuId",
                principalTable: "TaiLieu",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaiLieu_Tep_TepDinhKem_TepBiaId",
                table: "TaiLieu_Tep",
                column: "TepBiaId",
                principalTable: "TepDinhKem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaiLieu_Tep_TepDinhKem_TepDinhKemId",
                table: "TaiLieu_Tep",
                column: "TepDinhKemId",
                principalTable: "TepDinhKem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
