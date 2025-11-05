using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Khoa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenKhoa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaKhoa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khoa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoaiTaiLieu",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenLoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DuyetBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NoiDungDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDuyet = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrangThaiDuyet = table.Column<int>(type: "int", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiTaiLieu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Nganh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacultyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenNganh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaNganh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nganh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nganh_Khoa_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Khoa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaNganh = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GioiThieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrustScrore = table.Column<int>(type: "int", nullable: false),
                    CoGoiY = table.Column<bool>(type: "bit", nullable: false),
                    TrustLever = table.Column<int>(type: "int", nullable: false),
                    GioiTinh = table.Column<int>(type: "int", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenDangNhap = table.Column<string>(type: "varchar(100)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NguoiDung_Nganh_MaNganh",
                        column: x => x.MaNganh,
                        principalTable: "Nganh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_NguoiDung_UserId",
                        column: x => x.UserId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_NguoiDung_UserId",
                        column: x => x.UserId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_NguoiDung_UserId",
                        column: x => x.UserId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_NguoiDung_UserId",
                        column: x => x.UserId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonHoc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NganhId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenMonHoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaMonHoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DuyetBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NoiDungDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDuyet = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrangThaiDuyet = table.Column<int>(type: "int", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHoc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonHoc_Nganh_NganhId",
                        column: x => x.NganhId,
                        principalTable: "Nganh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonHoc_NguoiDung_TaoBoi",
                        column: x => x.TaoBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TepDinhKem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenTep = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkTruyCap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KichThuoc = table.Column<double>(type: "float", nullable: false),
                    LoaiFile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TepDinhKem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TepDinhKem_NguoiDung_TaoBoi",
                        column: x => x.TaoBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "The",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenTag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DuyetBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NoiDungDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDuyet = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrangThaiDuyet = table.Column<int>(type: "int", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_The", x => x.Id);
                    table.ForeignKey(
                        name: "FK_The_NguoiDung_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThongBao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoiTuongId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoToanBo = table.Column<bool>(type: "bit", nullable: false),
                    ThoiGianHetHan = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LoaiThongBao = table.Column<int>(type: "int", nullable: false),
                    CapDoThongBao = table.Column<int>(type: "int", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThongBao_NguoiDung_TaoBoi",
                        column: x => x.TaoBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTrustHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTrustHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTrustHistories_NguoiDung_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CuocTroChuyen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonHocId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TinNhanMoiNhat = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenCuocTroChuyen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoDuocTaoBoiAI = table.Column<bool>(type: "bit", nullable: false),
                    CoChoThanhVienGhimTinNhan = table.Column<bool>(type: "bit", nullable: false),
                    LoaiCuocTroChuyen = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuocTroChuyen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuocTroChuyen_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuocTroChuyen_NguoiDung_TaoBoi",
                        column: x => x.TaoBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProfileVectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmbeddingJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "TaiLieu",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonHocId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TheId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TacGia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTaTacGia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenThanThien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoDuocTai = table.Column<bool>(type: "bit", nullable: false),
                    CoHienThi = table.Column<int>(type: "int", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DuyetBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NgayDuyet = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NoiDungDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThaiDuyet = table.Column<int>(type: "int", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiLieu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaiLieu_LoaiTaiLieu_TheId",
                        column: x => x.TheId,
                        principalTable: "LoaiTaiLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiLieu_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiLieu_NguoiDung_TaoBoi",
                        column: x => x.TaoBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThongBao_NguoiNhan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThongBaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiNhanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoDaGui = table.Column<bool>(type: "bit", nullable: false),
                    CoDaDoc = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao_NguoiNhan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThongBao_NguoiNhan_NguoiDung_NguoiNhanId",
                        column: x => x.NguoiNhanId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThongBao_NguoiNhan_ThongBao_ThongBaoId",
                        column: x => x.ThongBaoId,
                        principalTable: "ThongBao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CuocTroChuyen_ThanhVien",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuocTroChuyenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TinNhanDocGanNhat = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CoBiChanChat = table.Column<bool>(type: "bit", nullable: false),
                    QuyenNhom = table.Column<int>(type: "int", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuocTroChuyen_ThanhVien", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuocTroChuyen_ThanhVien_CuocTroChuyen_CuocTroChuyenId",
                        column: x => x.CuocTroChuyenId,
                        principalTable: "CuocTroChuyen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuocTroChuyen_ThanhVien_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TinNhan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuocTroChuyenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoChinhSua = table.Column<bool>(type: "bit", nullable: false),
                    CoDaGhim = table.Column<bool>(type: "bit", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinNhan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TinNhan_CuocTroChuyen_CuocTroChuyenId",
                        column: x => x.CuocTroChuyenId,
                        principalTable: "CuocTroChuyen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TinNhan_NguoiDung_TaoBoi",
                        column: x => x.TaoBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TinNhan_TinNhan_ParentId",
                        column: x => x.ParentId,
                        principalTable: "TinNhan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "YeuCauThamGiaNhom",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuocTroChuyenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DuyetBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NoiDungDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDuyet = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrangThaiDuyet = table.Column<int>(type: "int", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauThamGiaNhom", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YeuCauThamGiaNhom_CuocTroChuyen_CuocTroChuyenId",
                        column: x => x.CuocTroChuyenId,
                        principalTable: "CuocTroChuyen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YeuCauThamGiaNhom_NguoiDung_TaoBoi",
                        column: x => x.TaoBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BinhLuan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RootId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DuyetBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NoiDungDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDuyet = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrangThaiDuyet = table.Column<int>(type: "int", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinhLuan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BinhLuan_BinhLuan_RootId",
                        column: x => x.RootId,
                        principalTable: "BinhLuan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BinhLuan_NguoiDung_DuyetBoi",
                        column: x => x.DuyetBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BinhLuan_NguoiDung_TaoBoi",
                        column: x => x.TaoBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BinhLuan_TaiLieu_TaiLieuId",
                        column: x => x.TaiLieuId,
                        principalTable: "TaiLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DanhGiaTaiLieu",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoaiDanhGia = table.Column<int>(type: "int", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGiaTaiLieu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanhGiaTaiLieu_NguoiDung_TaoBoi",
                        column: x => x.TaoBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DanhGiaTaiLieu_TaiLieu_TaiLieuId",
                        column: x => x.TaiLieuId,
                        principalTable: "TaiLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaiLieu_Tep",
                columns: table => new
                {
                    TepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiLieu_Tep", x => new { x.TepId, x.TaiLieuId });
                    table.ForeignKey(
                        name: "FK_TaiLieu_Tep_TaiLieu_TaiLieuId",
                        column: x => x.TaiLieuId,
                        principalTable: "TaiLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiLieu_Tep_TepDinhKem_TepId",
                        column: x => x.TepId,
                        principalTable: "TepDinhKem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaiLieu_The",
                columns: table => new
                {
                    TheId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiLieu_The", x => new { x.TheId, x.TaiLieuId });
                    table.ForeignKey(
                        name: "FK_TaiLieu_The_TaiLieu_TaiLieuId",
                        column: x => x.TaiLieuId,
                        principalTable: "TaiLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiLieu_The_The_TheId",
                        column: x => x.TheId,
                        principalTable: "The",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TinNhan_Tep",
                columns: table => new
                {
                    TepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TinNhanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinNhan_Tep", x => new { x.TepId, x.TinNhanId });
                    table.ForeignKey(
                        name: "FK_TinNhan_Tep_TepDinhKem_TepId",
                        column: x => x.TepId,
                        principalTable: "TepDinhKem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TinNhan_Tep_TinNhan_TinNhanId",
                        column: x => x.TinNhanId,
                        principalTable: "TinNhan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BaoCao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BinhLuanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DuyetBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NoiDungDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDuyet = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrangThaiDuyet = table.Column<int>(type: "int", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaoCao_BinhLuan_BinhLuanId",
                        column: x => x.BinhLuanId,
                        principalTable: "BinhLuan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaoCao_NguoiDung_DuyetBoi",
                        column: x => x.DuyetBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaoCao_NguoiDung_TaoBoi",
                        column: x => x.TaoBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaoCao_TaiLieu_TaiLieuId",
                        column: x => x.TaiLieuId,
                        principalTable: "TaiLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_BinhLuanId",
                table: "BaoCao",
                column: "BinhLuanId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_DuyetBoi",
                table: "BaoCao",
                column: "DuyetBoi");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_TaiLieuId",
                table: "BaoCao",
                column: "TaiLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_TaoBoi",
                table: "BaoCao",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_DuyetBoi",
                table: "BinhLuan",
                column: "DuyetBoi");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_RootId",
                table: "BinhLuan",
                column: "RootId");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_TaiLieuId",
                table: "BinhLuan",
                column: "TaiLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_TaoBoi",
                table: "BinhLuan",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_CuocTroChuyen_MonHocId",
                table: "CuocTroChuyen",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_CuocTroChuyen_TaoBoi",
                table: "CuocTroChuyen",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_CuocTroChuyen_ThanhVien_CuocTroChuyenId",
                table: "CuocTroChuyen_ThanhVien",
                column: "CuocTroChuyenId");

            migrationBuilder.CreateIndex(
                name: "IX_CuocTroChuyen_ThanhVien_NguoiDungId",
                table: "CuocTroChuyen_ThanhVien",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaTaiLieu_TaiLieuId",
                table: "DanhGiaTaiLieu",
                column: "TaiLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaTaiLieu_TaoBoi",
                table: "DanhGiaTaiLieu",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_MonHoc_NganhId",
                table: "MonHoc",
                column: "NganhId");

            migrationBuilder.CreateIndex(
                name: "IX_MonHoc_TaoBoi",
                table: "MonHoc",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_Nganh_FacultyId",
                table: "Nganh",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "NguoiDung",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_MaNganh",
                table: "NguoiDung",
                column: "MaNganh");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "NguoiDung",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileVectors_SubjectId",
                table: "ProfileVectors",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_MonHocId",
                table: "TaiLieu",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_TaoBoi",
                table: "TaiLieu",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_TheId",
                table: "TaiLieu",
                column: "TheId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_Tep_TaiLieuId",
                table: "TaiLieu_Tep",
                column: "TaiLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_The_TaiLieuId",
                table: "TaiLieu_The",
                column: "TaiLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKem_TaoBoi",
                table: "TepDinhKem",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_The_AppUserId",
                table: "The",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_TaoBoi",
                table: "ThongBao",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_NguoiNhan_NguoiNhanId",
                table: "ThongBao_NguoiNhan",
                column: "NguoiNhanId");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_NguoiNhan_ThongBaoId",
                table: "ThongBao_NguoiNhan",
                column: "ThongBaoId");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_CuocTroChuyenId",
                table: "TinNhan",
                column: "CuocTroChuyenId");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_ParentId",
                table: "TinNhan",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_TaoBoi",
                table: "TinNhan",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_Tep_TinNhanId",
                table: "TinNhan_Tep",
                column: "TinNhanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrustHistories_AppUserId",
                table: "UserTrustHistories",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauThamGiaNhom_CuocTroChuyenId",
                table: "YeuCauThamGiaNhom",
                column: "CuocTroChuyenId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauThamGiaNhom_TaoBoi",
                table: "YeuCauThamGiaNhom",
                column: "TaoBoi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BaoCao");

            migrationBuilder.DropTable(
                name: "CuocTroChuyen_ThanhVien");

            migrationBuilder.DropTable(
                name: "DanhGiaTaiLieu");

            migrationBuilder.DropTable(
                name: "ProfileVectors");

            migrationBuilder.DropTable(
                name: "TaiLieu_Tep");

            migrationBuilder.DropTable(
                name: "TaiLieu_The");

            migrationBuilder.DropTable(
                name: "ThongBao_NguoiNhan");

            migrationBuilder.DropTable(
                name: "TinNhan_Tep");

            migrationBuilder.DropTable(
                name: "UserTrustHistories");

            migrationBuilder.DropTable(
                name: "YeuCauThamGiaNhom");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "BinhLuan");

            migrationBuilder.DropTable(
                name: "The");

            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropTable(
                name: "TepDinhKem");

            migrationBuilder.DropTable(
                name: "TinNhan");

            migrationBuilder.DropTable(
                name: "TaiLieu");

            migrationBuilder.DropTable(
                name: "CuocTroChuyen");

            migrationBuilder.DropTable(
                name: "LoaiTaiLieu");

            migrationBuilder.DropTable(
                name: "MonHoc");

            migrationBuilder.DropTable(
                name: "NguoiDung");

            migrationBuilder.DropTable(
                name: "Nganh");

            migrationBuilder.DropTable(
                name: "Khoa");
        }
    }
}
