using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CapNhatBoi",
                table: "LoaiTaiLieu",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TaoBoi",
                table: "LoaiTaiLieu",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapNhatBoi",
                table: "LoaiTaiLieu");

            migrationBuilder.DropColumn(
                name: "TaoBoi",
                table: "LoaiTaiLieu");
        }
    }
}
