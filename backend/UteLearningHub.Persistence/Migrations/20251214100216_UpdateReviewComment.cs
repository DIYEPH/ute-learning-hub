using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReviewComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GhiChuDuyet",
                table: "BinhLuan",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NguoiDuyetId",
                table: "BinhLuan",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ThoiGianDuyet",
                table: "BinhLuan",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GhiChuDuyet",
                table: "BinhLuan");

            migrationBuilder.DropColumn(
                name: "NguoiDuyetId",
                table: "BinhLuan");

            migrationBuilder.DropColumn(
                name: "ThoiGianDuyet",
                table: "BinhLuan");
        }
    }
}
