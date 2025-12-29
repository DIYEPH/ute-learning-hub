using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Reason",
                table: "BaoCao",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "BaoCao");
        }
    }
}
