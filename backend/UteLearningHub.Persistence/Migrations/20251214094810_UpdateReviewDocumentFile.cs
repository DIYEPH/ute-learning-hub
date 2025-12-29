using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReviewDocumentFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LyDoAn",
                table: "TaiLieu_Tep",
                newName: "GhiChuDuyet");

            migrationBuilder.AddColumn<Guid>(
                name: "NguoiDuyetId",
                table: "TaiLieu_Tep",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ThoiGianDuyet",
                table: "TaiLieu_Tep",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NguoiDuyetId",
                table: "TaiLieu_Tep");

            migrationBuilder.DropColumn(
                name: "ThoiGianDuyet",
                table: "TaiLieu_Tep");

            migrationBuilder.RenameColumn(
                name: "GhiChuDuyet",
                table: "TaiLieu_Tep",
                newName: "LyDoAn");
        }
    }
}
