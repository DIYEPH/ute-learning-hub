using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectMajor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonHoc_Nganh_NganhId",
                table: "MonHoc");

            migrationBuilder.DropIndex(
                name: "IX_MonHoc_NganhId",
                table: "MonHoc");

            migrationBuilder.DropColumn(
                name: "NganhId",
                table: "MonHoc");

            migrationBuilder.CreateTable(
                name: "MonHoc_Nganh",
                columns: table => new
                {
                    MonHocId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NganhId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHoc_Nganh", x => new { x.MonHocId, x.NganhId });
                    table.ForeignKey(
                        name: "FK_MonHoc_Nganh_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonHoc_Nganh_Nganh_NganhId",
                        column: x => x.NganhId,
                        principalTable: "Nganh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonHoc_Nganh_NganhId",
                table: "MonHoc_Nganh",
                column: "NganhId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonHoc_Nganh");

            migrationBuilder.AddColumn<Guid>(
                name: "NganhId",
                table: "MonHoc",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonHoc_NganhId",
                table: "MonHoc",
                column: "NganhId");

            migrationBuilder.AddForeignKey(
                name: "FK_MonHoc_Nganh_NganhId",
                table: "MonHoc",
                column: "NganhId",
                principalTable: "Nganh",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
