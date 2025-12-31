using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSuggestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoiYPool");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoiYPool",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThoiDiemThem = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LyDoKichHoat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoiYPool", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoiYPool_NguoiDungId",
                table: "GoiYPool",
                column: "NguoiDungId",
                unique: true);
        }
    }
}
