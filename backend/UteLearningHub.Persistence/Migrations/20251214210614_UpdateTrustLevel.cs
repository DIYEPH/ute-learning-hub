using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTrustLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CapDoXacThuc",
                table: "NguoiDung",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComputedColumnSql: "\r\n                CASE \r\n                    WHEN [DiemXacThuc] < 5 THEN 0\r\n                    WHEN [DiemXacThuc] < 15 THEN 1\r\n                    WHEN [DiemXacThuc] < 60 THEN 2\r\n                    WHEN [DiemXacThuc] < 120 THEN 3\r\n                    ELSE 4\r\n                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CapDoXacThuc",
                table: "NguoiDung",
                type: "int",
                nullable: false,
                computedColumnSql: "\r\n                CASE \r\n                    WHEN [DiemXacThuc] < 5 THEN 0\r\n                    WHEN [DiemXacThuc] < 15 THEN 1\r\n                    WHEN [DiemXacThuc] < 60 THEN 2\r\n                    WHEN [DiemXacThuc] < 120 THEN 3\r\n                    ELSE 4\r\n                END",
                stored: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
