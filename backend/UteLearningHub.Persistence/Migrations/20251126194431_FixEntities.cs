using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nganh_Khoa_FacultyId",
                table: "Nganh");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiLieu_LoaiTaiLieu_TheId",
                table: "TaiLieu");

            migrationBuilder.DropForeignKey(
                name: "FK_The_NguoiDung_AppUserId",
                table: "The");

            migrationBuilder.DropIndex(
                name: "IX_The_AppUserId",
                table: "The");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "The");

            migrationBuilder.DropColumn(
                name: "CapNhatBoi",
                table: "LoaiTaiLieu");

            migrationBuilder.DropColumn(
                name: "DuyetBoi",
                table: "LoaiTaiLieu");

            migrationBuilder.DropColumn(
                name: "NgayDuyet",
                table: "LoaiTaiLieu");

            migrationBuilder.DropColumn(
                name: "NoiDungDuyet",
                table: "LoaiTaiLieu");

            migrationBuilder.DropColumn(
                name: "TaoBoi",
                table: "LoaiTaiLieu");

            migrationBuilder.DropColumn(
                name: "TrangThaiDuyet",
                table: "LoaiTaiLieu");

            migrationBuilder.DropColumn(
                name: "BiXoaBoi",
                table: "DacTrungNguoiDung");

            migrationBuilder.DropColumn(
                name: "CoDaXoa",
                table: "DacTrungNguoiDung");

            migrationBuilder.DropColumn(
                name: "KieuDoTuongDong",
                table: "DacTrungNguoiDung");

            migrationBuilder.DropColumn(
                name: "NgayCapNhat",
                table: "DacTrungNguoiDung");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "DacTrungNguoiDung");

            migrationBuilder.DropColumn(
                name: "NgayXoa",
                table: "DacTrungNguoiDung");

            migrationBuilder.DropColumn(
                name: "PhienBanHang",
                table: "DacTrungNguoiDung");

            migrationBuilder.DropColumn(
                name: "PhienBanMoHinh",
                table: "DacTrungNguoiDung");

            migrationBuilder.DropColumn(
                name: "BiXoaBoi",
                table: "DacTrungCuocTroChuyen");

            migrationBuilder.DropColumn(
                name: "CoDaXoa",
                table: "DacTrungCuocTroChuyen");

            migrationBuilder.DropColumn(
                name: "KieuDoTuongDong",
                table: "DacTrungCuocTroChuyen");

            migrationBuilder.DropColumn(
                name: "NgayCapNhat",
                table: "DacTrungCuocTroChuyen");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "DacTrungCuocTroChuyen");

            migrationBuilder.DropColumn(
                name: "NgayXoa",
                table: "DacTrungCuocTroChuyen");

            migrationBuilder.DropColumn(
                name: "PhienBanHang",
                table: "DacTrungCuocTroChuyen");

            migrationBuilder.DropColumn(
                name: "PhienBanMoHinh",
                table: "DacTrungCuocTroChuyen");

            migrationBuilder.RenameColumn(
                name: "TheId",
                table: "TaiLieu",
                newName: "LoaiTaiLieuId");

            migrationBuilder.RenameIndex(
                name: "IX_TaiLieu_TheId",
                table: "TaiLieu",
                newName: "IX_TaiLieu_LoaiTaiLieuId");

            migrationBuilder.RenameColumn(
                name: "FacultyId",
                table: "Nganh",
                newName: "KhoaId");

            migrationBuilder.RenameIndex(
                name: "IX_Nganh_FacultyId",
                table: "Nganh",
                newName: "IX_Nganh_KhoaId");

            migrationBuilder.AlterColumn<bool>(
                name: "ConHieuLuc",
                table: "DacTrungNguoiDung",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "ConHieuLuc",
                table: "DacTrungCuocTroChuyen",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_The_TaoBoi",
                table: "The",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_DacTrungNguoiDung_ConHieuLuc",
                table: "DacTrungNguoiDung",
                column: "ConHieuLuc");

            migrationBuilder.CreateIndex(
                name: "IX_DacTrungNguoiDung_NguoiDungId",
                table: "DacTrungNguoiDung",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_DacTrungNguoiDung_NguoiDungId_LoaiVector",
                table: "DacTrungNguoiDung",
                columns: new[] { "NguoiDungId", "LoaiVector" });

            migrationBuilder.CreateIndex(
                name: "IX_DacTrungNguoiDung_NguoiDungId_MonHocId",
                table: "DacTrungNguoiDung",
                columns: new[] { "NguoiDungId", "MonHocId" });

            migrationBuilder.CreateIndex(
                name: "IX_DacTrungCuocTroChuyen_ConHieuLuc",
                table: "DacTrungCuocTroChuyen",
                column: "ConHieuLuc");

            migrationBuilder.AddForeignKey(
                name: "FK_Nganh_Khoa_KhoaId",
                table: "Nganh",
                column: "KhoaId",
                principalTable: "Khoa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaiLieu_LoaiTaiLieu_LoaiTaiLieuId",
                table: "TaiLieu",
                column: "LoaiTaiLieuId",
                principalTable: "LoaiTaiLieu",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_The_NguoiDung_TaoBoi",
                table: "The",
                column: "TaoBoi",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nganh_Khoa_KhoaId",
                table: "Nganh");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiLieu_LoaiTaiLieu_LoaiTaiLieuId",
                table: "TaiLieu");

            migrationBuilder.DropForeignKey(
                name: "FK_The_NguoiDung_TaoBoi",
                table: "The");

            migrationBuilder.DropIndex(
                name: "IX_The_TaoBoi",
                table: "The");

            migrationBuilder.DropIndex(
                name: "IX_DacTrungNguoiDung_ConHieuLuc",
                table: "DacTrungNguoiDung");

            migrationBuilder.DropIndex(
                name: "IX_DacTrungNguoiDung_NguoiDungId",
                table: "DacTrungNguoiDung");

            migrationBuilder.DropIndex(
                name: "IX_DacTrungNguoiDung_NguoiDungId_LoaiVector",
                table: "DacTrungNguoiDung");

            migrationBuilder.DropIndex(
                name: "IX_DacTrungNguoiDung_NguoiDungId_MonHocId",
                table: "DacTrungNguoiDung");

            migrationBuilder.DropIndex(
                name: "IX_DacTrungCuocTroChuyen_ConHieuLuc",
                table: "DacTrungCuocTroChuyen");

            migrationBuilder.RenameColumn(
                name: "LoaiTaiLieuId",
                table: "TaiLieu",
                newName: "TheId");

            migrationBuilder.RenameIndex(
                name: "IX_TaiLieu_LoaiTaiLieuId",
                table: "TaiLieu",
                newName: "IX_TaiLieu_TheId");

            migrationBuilder.RenameColumn(
                name: "KhoaId",
                table: "Nganh",
                newName: "FacultyId");

            migrationBuilder.RenameIndex(
                name: "IX_Nganh_KhoaId",
                table: "Nganh",
                newName: "IX_Nganh_FacultyId");

            migrationBuilder.AddColumn<Guid>(
                name: "AppUserId",
                table: "The",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CapNhatBoi",
                table: "LoaiTaiLieu",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DuyetBoi",
                table: "LoaiTaiLieu",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayDuyet",
                table: "LoaiTaiLieu",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoiDungDuyet",
                table: "LoaiTaiLieu",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TaoBoi",
                table: "LoaiTaiLieu",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "TrangThaiDuyet",
                table: "LoaiTaiLieu",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "ConHieuLuc",
                table: "DacTrungNguoiDung",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BiXoaBoi",
                table: "DacTrungNguoiDung",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CoDaXoa",
                table: "DacTrungNguoiDung",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "KieuDoTuongDong",
                table: "DacTrungNguoiDung",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayCapNhat",
                table: "DacTrungNguoiDung",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayTao",
                table: "DacTrungNguoiDung",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayXoa",
                table: "DacTrungNguoiDung",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "PhienBanHang",
                table: "DacTrungNguoiDung",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "PhienBanMoHinh",
                table: "DacTrungNguoiDung",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ConHieuLuc",
                table: "DacTrungCuocTroChuyen",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BiXoaBoi",
                table: "DacTrungCuocTroChuyen",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CoDaXoa",
                table: "DacTrungCuocTroChuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "KieuDoTuongDong",
                table: "DacTrungCuocTroChuyen",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayCapNhat",
                table: "DacTrungCuocTroChuyen",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayTao",
                table: "DacTrungCuocTroChuyen",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayXoa",
                table: "DacTrungCuocTroChuyen",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "PhienBanHang",
                table: "DacTrungCuocTroChuyen",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "PhienBanMoHinh",
                table: "DacTrungCuocTroChuyen",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_The_AppUserId",
                table: "The",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Nganh_Khoa_FacultyId",
                table: "Nganh",
                column: "FacultyId",
                principalTable: "Khoa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaiLieu_LoaiTaiLieu_TheId",
                table: "TaiLieu",
                column: "TheId",
                principalTable: "LoaiTaiLieu",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_The_NguoiDung_AppUserId",
                table: "The",
                column: "AppUserId",
                principalTable: "NguoiDung",
                principalColumn: "Id");
        }
    }
}
