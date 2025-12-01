using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDocumentAndComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BinhLuan_TaiLieu_TaiLieuId",
                table: "BinhLuan");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiLieu_TepDinhKem_FileId",
                table: "TaiLieu");

            migrationBuilder.DropIndex(
                name: "IX_TaiLieu_FileId",
                table: "TaiLieu");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "TaiLieu");

            migrationBuilder.DropColumn(
                name: "MoTaTacGia",
                table: "TaiLieu");

            migrationBuilder.DropColumn(
                name: "TacGia",
                table: "TaiLieu");

            migrationBuilder.RenameColumn(
                name: "TaiLieuId",
                table: "BinhLuan",
                newName: "TaiLieu_TepId");

            migrationBuilder.RenameIndex(
                name: "IX_BinhLuan_TaiLieuId",
                table: "BinhLuan",
                newName: "IX_BinhLuan_TaiLieu_TepId");

            migrationBuilder.AddForeignKey(
                name: "FK_BinhLuan_TaiLieu_Tep_TaiLieu_TepId",
                table: "BinhLuan",
                column: "TaiLieu_TepId",
                principalTable: "TaiLieu_Tep",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BinhLuan_TaiLieu_Tep_TaiLieu_TepId",
                table: "BinhLuan");

            migrationBuilder.RenameColumn(
                name: "TaiLieu_TepId",
                table: "BinhLuan",
                newName: "TaiLieuId");

            migrationBuilder.RenameIndex(
                name: "IX_BinhLuan_TaiLieu_TepId",
                table: "BinhLuan",
                newName: "IX_BinhLuan_TaiLieuId");

            migrationBuilder.AddColumn<Guid>(
                name: "FileId",
                table: "TaiLieu",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "MoTaTacGia",
                table: "TaiLieu",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TacGia",
                table: "TaiLieu",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_FileId",
                table: "TaiLieu",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_BinhLuan_TaiLieu_TaiLieuId",
                table: "BinhLuan",
                column: "TaiLieuId",
                principalTable: "TaiLieu",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaiLieu_TepDinhKem_FileId",
                table: "TaiLieu",
                column: "FileId",
                principalTable: "TepDinhKem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
