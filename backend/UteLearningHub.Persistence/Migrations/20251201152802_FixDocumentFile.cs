using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixDocumentFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TepBiaId",
                table: "TaiLieu_Tep",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TepBiaId",
                table: "TaiLieu",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_Tep_TepBiaId",
                table: "TaiLieu_Tep",
                column: "TepBiaId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_TepBiaId",
                table: "TaiLieu",
                column: "TepBiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaiLieu_TepDinhKem_TepBiaId",
                table: "TaiLieu",
                column: "TepBiaId",
                principalTable: "TepDinhKem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaiLieu_Tep_TepDinhKem_TepBiaId",
                table: "TaiLieu_Tep",
                column: "TepBiaId",
                principalTable: "TepDinhKem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaiLieu_TepDinhKem_TepBiaId",
                table: "TaiLieu");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiLieu_Tep_TepDinhKem_TepBiaId",
                table: "TaiLieu_Tep");

            migrationBuilder.DropIndex(
                name: "IX_TaiLieu_Tep_TepBiaId",
                table: "TaiLieu_Tep");

            migrationBuilder.DropIndex(
                name: "IX_TaiLieu_TepBiaId",
                table: "TaiLieu");

            migrationBuilder.DropColumn(
                name: "TepBiaId",
                table: "TaiLieu_Tep");

            migrationBuilder.DropColumn(
                name: "TepBiaId",
                table: "TaiLieu");
        }
    }
}
