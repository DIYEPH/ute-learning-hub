using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixReviewDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm cột TaiLieu_TepId vào bảng DanhGiaTaiLieu
            migrationBuilder.AddColumn<Guid>(
                name: "TaiLieu_TepId",
                table: "DanhGiaTaiLieu",
                type: "uniqueidentifier",
                nullable: true);

            // Tạo index cho cột mới
            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaTaiLieu_TaiLieu_TepId",
                table: "DanhGiaTaiLieu",
                column: "TaiLieu_TepId");

            // Thêm khóa ngoại sang bảng TaiLieu_Tep (DocumentFile)
            migrationBuilder.AddForeignKey(
                name: "FK_DanhGiaTaiLieu_TaiLieu_Tep_TaiLieu_TepId",
                table: "DanhGiaTaiLieu",
                column: "TaiLieu_TepId",
                principalTable: "TaiLieu_Tep",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhGiaTaiLieu_TaiLieu_Tep_TaiLieu_TepId",
                table: "DanhGiaTaiLieu");

            migrationBuilder.DropIndex(
                name: "IX_DanhGiaTaiLieu_TaiLieu_TepId",
                table: "DanhGiaTaiLieu");

            migrationBuilder.DropColumn(
                name: "TaiLieu_TepId",
                table: "DanhGiaTaiLieu");
        }
    }
}