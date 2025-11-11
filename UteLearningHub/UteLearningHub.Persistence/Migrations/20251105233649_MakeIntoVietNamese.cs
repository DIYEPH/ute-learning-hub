using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeIntoVietNamese : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_NguoiDung_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_NguoiDung_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_NguoiDung_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_NguoiDung_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_BinhLuan_BinhLuan_RootId",
                table: "BinhLuan");

            migrationBuilder.DropForeignKey(
                name: "FK_TinNhan_TinNhan_ParentId",
                table: "TinNhan");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTrustHistories_NguoiDung_AppUserId",
                table: "UserTrustHistories");

            migrationBuilder.DropTable(
                name: "ProfileVectors");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "NguoiDung");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTrustHistories",
                table: "UserTrustHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "UserTrustHistories",
                newName: "UserTrustHistory");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "NguoiDung_Token");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "NguoiDung_VaiTro");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "DangNhapNgoai");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "NguoiDung_Quyen");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "VaiTro");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "VaiTro_Quyen");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "TinNhan",
                newName: "PhanTuChaId");

            migrationBuilder.RenameIndex(
                name: "IX_TinNhan_ParentId",
                table: "TinNhan",
                newName: "IX_TinNhan_PhanTuChaId");

            migrationBuilder.RenameColumn(
                name: "TenThanThien",
                table: "TaiLieu",
                newName: "TenChuanHoa");

            migrationBuilder.RenameColumn(
                name: "TwoFactorEnabled",
                table: "NguoiDung",
                newName: "CoKichHoatXacThucHaiLop");

            migrationBuilder.RenameColumn(
                name: "TrustLever",
                table: "NguoiDung",
                newName: "CapDoXacThuc");

            migrationBuilder.RenameColumn(
                name: "SecurityStamp",
                table: "NguoiDung",
                newName: "MaBaoMat");

            migrationBuilder.RenameColumn(
                name: "PhoneNumberConfirmed",
                table: "NguoiDung",
                newName: "CoDaXacNhanSoDienThoai");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "NguoiDung",
                newName: "SoDienThoai");

            migrationBuilder.RenameColumn(
                name: "PhienBanHang",
                table: "NguoiDung",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "NguoiDung",
                newName: "MatKhauBam");

            migrationBuilder.RenameColumn(
                name: "NormalizedUserName",
                table: "NguoiDung",
                newName: "TenDangNhapChuanHoa");

            migrationBuilder.RenameColumn(
                name: "NormalizedEmail",
                table: "NguoiDung",
                newName: "EmailChuanHoa");

            migrationBuilder.RenameColumn(
                name: "NgayTao",
                table: "NguoiDung",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "NgayCapNhat",
                table: "NguoiDung",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "LockoutEnd",
                table: "NguoiDung",
                newName: "NgayHetKhoaTaiKhoan");

            migrationBuilder.RenameColumn(
                name: "LockoutEnabled",
                table: "NguoiDung",
                newName: "ChoPhepKhoaTaiKhoan");

            migrationBuilder.RenameColumn(
                name: "EmailConfirmed",
                table: "NguoiDung",
                newName: "CoXacThucEmail");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "NguoiDung",
                newName: "DauKiemSoatDongBo");

            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                table: "NguoiDung",
                newName: "HinhDaiDien");

            migrationBuilder.RenameColumn(
                name: "AccessFailedCount",
                table: "NguoiDung",
                newName: "SoLanDangNhapThatBai");

            migrationBuilder.RenameColumn(
                name: "TrustScrore",
                table: "NguoiDung",
                newName: "DiemXacThuc");

            migrationBuilder.RenameColumn(
                name: "RootId",
                table: "BinhLuan",
                newName: "PhanTuChaId");

            migrationBuilder.RenameIndex(
                name: "IX_BinhLuan_RootId",
                table: "BinhLuan",
                newName: "IX_BinhLuan_PhanTuChaId");

            migrationBuilder.RenameIndex(
                name: "IX_UserTrustHistories_AppUserId",
                table: "UserTrustHistory",
                newName: "IX_UserTrustHistory_AppUserId");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "NguoiDung_Token",
                newName: "GiaTriMa");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "NguoiDung_Token",
                newName: "LoaiMa");

            migrationBuilder.RenameColumn(
                name: "LoginProvider",
                table: "NguoiDung_Token",
                newName: "NhaCungCapDangNhap");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "NguoiDung_Token",
                newName: "NguoiDungId");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "NguoiDung_VaiTro",
                newName: "VaiTroId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "NguoiDung_VaiTro",
                newName: "NguoiDungId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "NguoiDung_VaiTro",
                newName: "IX_NguoiDung_VaiTro_VaiTroId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "DangNhapNgoai",
                newName: "NguoiDungId");

            migrationBuilder.RenameColumn(
                name: "ProviderDisplayName",
                table: "DangNhapNgoai",
                newName: "TenHienThiNhaCungCap");

            migrationBuilder.RenameColumn(
                name: "ProviderKey",
                table: "DangNhapNgoai",
                newName: "KhoaNhaCungCap");

            migrationBuilder.RenameColumn(
                name: "LoginProvider",
                table: "DangNhapNgoai",
                newName: "NhaCungCapDangNhap");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "DangNhapNgoai",
                newName: "IX_DangNhapNgoai_NguoiDungId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "NguoiDung_Quyen",
                newName: "NguoiDungId");

            migrationBuilder.RenameColumn(
                name: "ClaimValue",
                table: "NguoiDung_Quyen",
                newName: "GiaTriQuyen");

            migrationBuilder.RenameColumn(
                name: "ClaimType",
                table: "NguoiDung_Quyen",
                newName: "LoaiQuyen");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "NguoiDung_Quyen",
                newName: "IX_NguoiDung_Quyen_NguoiDungId");

            migrationBuilder.RenameColumn(
                name: "NormalizedName",
                table: "VaiTro",
                newName: "TenChuanHoa");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "VaiTro",
                newName: "TenVaiTro");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "VaiTro",
                newName: "DauKiemSoatDongBo");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "VaiTro_Quyen",
                newName: "VaiTroId");

            migrationBuilder.RenameColumn(
                name: "ClaimValue",
                table: "VaiTro_Quyen",
                newName: "GiaTriQuyen");

            migrationBuilder.RenameColumn(
                name: "ClaimType",
                table: "VaiTro_Quyen",
                newName: "LoaiQuyen");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "VaiTro_Quyen",
                newName: "IX_VaiTro_Quyen_VaiTroId");

            migrationBuilder.AlterColumn<string>(
                name: "TenDangNhap",
                table: "NguoiDung",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTrustHistory",
                table: "UserTrustHistory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NguoiDung_Token",
                table: "NguoiDung_Token",
                columns: new[] { "NguoiDungId", "NhaCungCapDangNhap", "LoaiMa" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_NguoiDung_VaiTro",
                table: "NguoiDung_VaiTro",
                columns: new[] { "NguoiDungId", "VaiTroId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DangNhapNgoai",
                table: "DangNhapNgoai",
                columns: new[] { "NhaCungCapDangNhap", "KhoaNhaCungCap" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_NguoiDung_Quyen",
                table: "NguoiDung_Quyen",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VaiTro",
                table: "VaiTro",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VaiTro_Quyen",
                table: "VaiTro_Quyen",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "NguoiDung",
                column: "TenDangNhapChuanHoa",
                unique: true,
                filter: "[TenDangNhapChuanHoa] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "VaiTro",
                column: "TenChuanHoa",
                unique: true,
                filter: "[TenChuanHoa] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_BinhLuan_BinhLuan_PhanTuChaId",
                table: "BinhLuan",
                column: "PhanTuChaId",
                principalTable: "BinhLuan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DangNhapNgoai_NguoiDung_NguoiDungId",
                table: "DangNhapNgoai",
                column: "NguoiDungId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDung_Quyen_NguoiDung_NguoiDungId",
                table: "NguoiDung_Quyen",
                column: "NguoiDungId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDung_Token_NguoiDung_NguoiDungId",
                table: "NguoiDung_Token",
                column: "NguoiDungId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDung_VaiTro_NguoiDung_NguoiDungId",
                table: "NguoiDung_VaiTro",
                column: "NguoiDungId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDung_VaiTro_VaiTro_VaiTroId",
                table: "NguoiDung_VaiTro",
                column: "VaiTroId",
                principalTable: "VaiTro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TinNhan_TinNhan_PhanTuChaId",
                table: "TinNhan",
                column: "PhanTuChaId",
                principalTable: "TinNhan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTrustHistory_NguoiDung_AppUserId",
                table: "UserTrustHistory",
                column: "AppUserId",
                principalTable: "NguoiDung",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VaiTro_Quyen_VaiTro_VaiTroId",
                table: "VaiTro_Quyen",
                column: "VaiTroId",
                principalTable: "VaiTro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BinhLuan_BinhLuan_PhanTuChaId",
                table: "BinhLuan");

            migrationBuilder.DropForeignKey(
                name: "FK_DangNhapNgoai_NguoiDung_NguoiDungId",
                table: "DangNhapNgoai");

            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_Quyen_NguoiDung_NguoiDungId",
                table: "NguoiDung_Quyen");

            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_Token_NguoiDung_NguoiDungId",
                table: "NguoiDung_Token");

            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_VaiTro_NguoiDung_NguoiDungId",
                table: "NguoiDung_VaiTro");

            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_VaiTro_VaiTro_VaiTroId",
                table: "NguoiDung_VaiTro");

            migrationBuilder.DropForeignKey(
                name: "FK_TinNhan_TinNhan_PhanTuChaId",
                table: "TinNhan");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTrustHistory_NguoiDung_AppUserId",
                table: "UserTrustHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_VaiTro_Quyen_VaiTro_VaiTroId",
                table: "VaiTro_Quyen");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "NguoiDung");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VaiTro_Quyen",
                table: "VaiTro_Quyen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VaiTro",
                table: "VaiTro");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "VaiTro");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTrustHistory",
                table: "UserTrustHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NguoiDung_VaiTro",
                table: "NguoiDung_VaiTro");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NguoiDung_Token",
                table: "NguoiDung_Token");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NguoiDung_Quyen",
                table: "NguoiDung_Quyen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DangNhapNgoai",
                table: "DangNhapNgoai");

            migrationBuilder.RenameTable(
                name: "VaiTro_Quyen",
                newName: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "VaiTro",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "UserTrustHistory",
                newName: "UserTrustHistories");

            migrationBuilder.RenameTable(
                name: "NguoiDung_VaiTro",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "NguoiDung_Token",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "NguoiDung_Quyen",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "DangNhapNgoai",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameColumn(
                name: "PhanTuChaId",
                table: "TinNhan",
                newName: "ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_TinNhan_PhanTuChaId",
                table: "TinNhan",
                newName: "IX_TinNhan_ParentId");

            migrationBuilder.RenameColumn(
                name: "TenChuanHoa",
                table: "TaiLieu",
                newName: "TenThanThien");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "NguoiDung",
                newName: "NgayCapNhat");

            migrationBuilder.RenameColumn(
                name: "TenDangNhapChuanHoa",
                table: "NguoiDung",
                newName: "NormalizedUserName");

            migrationBuilder.RenameColumn(
                name: "SoLanDangNhapThatBai",
                table: "NguoiDung",
                newName: "AccessFailedCount");

            migrationBuilder.RenameColumn(
                name: "SoDienThoai",
                table: "NguoiDung",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "NguoiDung",
                newName: "PhienBanHang");

            migrationBuilder.RenameColumn(
                name: "NgayHetKhoaTaiKhoan",
                table: "NguoiDung",
                newName: "LockoutEnd");

            migrationBuilder.RenameColumn(
                name: "MatKhauBam",
                table: "NguoiDung",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "MaBaoMat",
                table: "NguoiDung",
                newName: "SecurityStamp");

            migrationBuilder.RenameColumn(
                name: "HinhDaiDien",
                table: "NguoiDung",
                newName: "AvatarUrl");

            migrationBuilder.RenameColumn(
                name: "EmailChuanHoa",
                table: "NguoiDung",
                newName: "NormalizedEmail");

            migrationBuilder.RenameColumn(
                name: "DauKiemSoatDongBo",
                table: "NguoiDung",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "NguoiDung",
                newName: "NgayTao");

            migrationBuilder.RenameColumn(
                name: "CoXacThucEmail",
                table: "NguoiDung",
                newName: "EmailConfirmed");

            migrationBuilder.RenameColumn(
                name: "CoKichHoatXacThucHaiLop",
                table: "NguoiDung",
                newName: "TwoFactorEnabled");

            migrationBuilder.RenameColumn(
                name: "CoDaXacNhanSoDienThoai",
                table: "NguoiDung",
                newName: "PhoneNumberConfirmed");

            migrationBuilder.RenameColumn(
                name: "ChoPhepKhoaTaiKhoan",
                table: "NguoiDung",
                newName: "LockoutEnabled");

            migrationBuilder.RenameColumn(
                name: "CapDoXacThuc",
                table: "NguoiDung",
                newName: "TrustLever");

            migrationBuilder.RenameColumn(
                name: "DiemXacThuc",
                table: "NguoiDung",
                newName: "TrustScrore");

            migrationBuilder.RenameColumn(
                name: "PhanTuChaId",
                table: "BinhLuan",
                newName: "RootId");

            migrationBuilder.RenameIndex(
                name: "IX_BinhLuan_PhanTuChaId",
                table: "BinhLuan",
                newName: "IX_BinhLuan_RootId");

            migrationBuilder.RenameColumn(
                name: "VaiTroId",
                table: "AspNetRoleClaims",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "LoaiQuyen",
                table: "AspNetRoleClaims",
                newName: "ClaimType");

            migrationBuilder.RenameColumn(
                name: "GiaTriQuyen",
                table: "AspNetRoleClaims",
                newName: "ClaimValue");

            migrationBuilder.RenameIndex(
                name: "IX_VaiTro_Quyen_VaiTroId",
                table: "AspNetRoleClaims",
                newName: "IX_AspNetRoleClaims_RoleId");

            migrationBuilder.RenameColumn(
                name: "TenVaiTro",
                table: "AspNetRoles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "TenChuanHoa",
                table: "AspNetRoles",
                newName: "NormalizedName");

            migrationBuilder.RenameColumn(
                name: "DauKiemSoatDongBo",
                table: "AspNetRoles",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameIndex(
                name: "IX_UserTrustHistory_AppUserId",
                table: "UserTrustHistories",
                newName: "IX_UserTrustHistories_AppUserId");

            migrationBuilder.RenameColumn(
                name: "VaiTroId",
                table: "AspNetUserRoles",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "NguoiDungId",
                table: "AspNetUserRoles",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_NguoiDung_VaiTro_VaiTroId",
                table: "AspNetUserRoles",
                newName: "IX_AspNetUserRoles_RoleId");

            migrationBuilder.RenameColumn(
                name: "GiaTriMa",
                table: "AspNetUserTokens",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "LoaiMa",
                table: "AspNetUserTokens",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NhaCungCapDangNhap",
                table: "AspNetUserTokens",
                newName: "LoginProvider");

            migrationBuilder.RenameColumn(
                name: "NguoiDungId",
                table: "AspNetUserTokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "NguoiDungId",
                table: "AspNetUserClaims",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "LoaiQuyen",
                table: "AspNetUserClaims",
                newName: "ClaimType");

            migrationBuilder.RenameColumn(
                name: "GiaTriQuyen",
                table: "AspNetUserClaims",
                newName: "ClaimValue");

            migrationBuilder.RenameIndex(
                name: "IX_NguoiDung_Quyen_NguoiDungId",
                table: "AspNetUserClaims",
                newName: "IX_AspNetUserClaims_UserId");

            migrationBuilder.RenameColumn(
                name: "TenHienThiNhaCungCap",
                table: "AspNetUserLogins",
                newName: "ProviderDisplayName");

            migrationBuilder.RenameColumn(
                name: "NguoiDungId",
                table: "AspNetUserLogins",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "KhoaNhaCungCap",
                table: "AspNetUserLogins",
                newName: "ProviderKey");

            migrationBuilder.RenameColumn(
                name: "NhaCungCapDangNhap",
                table: "AspNetUserLogins",
                newName: "LoginProvider");

            migrationBuilder.RenameIndex(
                name: "IX_DangNhapNgoai_NguoiDungId",
                table: "AspNetUserLogins",
                newName: "IX_AspNetUserLogins_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "TenDangNhap",
                table: "NguoiDung",
                type: "varchar(100)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTrustHistories",
                table: "UserTrustHistories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.CreateTable(
                name: "ProfileVectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmbeddingJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileVectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileVectors_MonHoc_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "NguoiDung",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileVectors_SubjectId",
                table: "ProfileVectors",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_NguoiDung_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_NguoiDung_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_NguoiDung_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_NguoiDung_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BinhLuan_BinhLuan_RootId",
                table: "BinhLuan",
                column: "RootId",
                principalTable: "BinhLuan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TinNhan_TinNhan_ParentId",
                table: "TinNhan",
                column: "ParentId",
                principalTable: "TinNhan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTrustHistories_NguoiDung_AppUserId",
                table: "UserTrustHistories",
                column: "AppUserId",
                principalTable: "NguoiDung",
                principalColumn: "Id");
        }
    }
}
