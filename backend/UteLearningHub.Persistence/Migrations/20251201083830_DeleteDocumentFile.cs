using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeleteDocumentFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaiLieu_Tep");

            migrationBuilder.AddColumn<Guid>(
                name: "FileId",
                table: "TaiLieu",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_FileId",
                table: "TaiLieu",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaiLieu_TepDinhKem_FileId",
                table: "TaiLieu",
                column: "FileId",
                principalTable: "TepDinhKem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaiLieu_TepDinhKem_FileId",
                table: "TaiLieu");

            migrationBuilder.DropIndex(
                name: "IX_TaiLieu_FileId",
                table: "TaiLieu");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "TaiLieu");

            migrationBuilder.CreateTable(
                name: "TaiLieu_Tep",
                columns: table => new
                {
                    TepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiLieu_Tep", x => new { x.TepId, x.TaiLieuId });
                    table.ForeignKey(
                        name: "FK_TaiLieu_Tep_TaiLieu_TaiLieuId",
                        column: x => x.TaiLieuId,
                        principalTable: "TaiLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiLieu_Tep_TepDinhKem_TepId",
                        column: x => x.TepId,
                        principalTable: "TepDinhKem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_Tep_TaiLieuId",
                table: "TaiLieu_Tep",
                column: "TaiLieuId");
        }
    }
}
