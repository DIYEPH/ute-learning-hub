using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_Nganh_MaNganh",
                table: "NguoiDung");

            migrationBuilder.AlterColumn<Guid>(
                name: "MaNganh",
                table: "NguoiDung",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDung_Nganh_MaNganh",
                table: "NguoiDung",
                column: "MaNganh",
                principalTable: "Nganh",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_Nganh_MaNganh",
                table: "NguoiDung");

            migrationBuilder.AlterColumn<Guid>(
                name: "MaNganh",
                table: "NguoiDung",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDung_Nganh_MaNganh",
                table: "NguoiDung",
                column: "MaNganh",
                principalTable: "Nganh",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
