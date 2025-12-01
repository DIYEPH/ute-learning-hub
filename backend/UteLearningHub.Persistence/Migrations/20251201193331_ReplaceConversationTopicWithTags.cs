using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceConversationTopicWithTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChuDe",
                table: "CuocTroChuyen");

            migrationBuilder.CreateTable(
                name: "CuocTroChuyen_The",
                columns: table => new
                {
                    TheId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuocTroChuyenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuocTroChuyen_The", x => new { x.TheId, x.CuocTroChuyenId });
                    table.ForeignKey(
                        name: "FK_CuocTroChuyen_The_CuocTroChuyen_CuocTroChuyenId",
                        column: x => x.CuocTroChuyenId,
                        principalTable: "CuocTroChuyen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuocTroChuyen_The_The_TheId",
                        column: x => x.TheId,
                        principalTable: "The",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuocTroChuyen_The_CuocTroChuyenId",
                table: "CuocTroChuyen_The",
                column: "CuocTroChuyenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CuocTroChuyen_The");

            migrationBuilder.AddColumn<string>(
                name: "ChuDe",
                table: "CuocTroChuyen",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
