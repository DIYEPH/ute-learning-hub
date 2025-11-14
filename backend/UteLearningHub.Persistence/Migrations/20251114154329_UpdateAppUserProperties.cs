using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppUserProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "VaiTro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenVaiTro = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TenChuanHoa = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DauKiemSoatDongBo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.Id);
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
                name: "VaiTro_Quyen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VaiTroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoaiQuyen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaTriQuyen = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro_Quyen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VaiTro_Quyen_VaiTro_VaiTroId",
                        column: x => x.VaiTroId,
                        principalTable: "VaiTro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaNganh = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GioiThieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HinhDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoVaTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiemXacThuc = table.Column<int>(type: "int", nullable: false),
                    CoGoiY = table.Column<bool>(type: "bit", nullable: false),
                    CapDoXacThuc = table.Column<int>(type: "int", nullable: false),
                    GioiTinh = table.Column<int>(type: "int", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenDangNhap = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TenDangNhapChuanHoa = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailChuanHoa = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CoXacThucEmail = table.Column<bool>(type: "bit", nullable: false),
                    MatKhauBam = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaBaoMat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DauKiemSoatDongBo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoDaXacNhanSoDienThoai = table.Column<bool>(type: "bit", nullable: false),
                    CoKichHoatXacThucHaiLop = table.Column<bool>(type: "bit", nullable: false),
                    NgayHetKhoaTaiKhoan = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ChoPhepKhoaTaiKhoan = table.Column<bool>(type: "bit", nullable: false),
                    SoLanDangNhapThatBai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NguoiDung_Nganh_MaNganh",
                        column: x => x.MaNganh,
                        principalTable: "Nganh",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DangNhapNgoai",
                columns: table => new
                {
                    NhaCungCapDangNhap = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    KhoaNhaCungCap = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenHienThiNhaCungCap = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangNhapNgoai", x => new { x.NhaCungCapDangNhap, x.KhoaNhaCungCap });
                    table.ForeignKey(
                        name: "FK_DangNhapNgoai_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
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
                name: "NguoiDung_Quyen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoaiQuyen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaTriQuyen = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung_Quyen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NguoiDung_Quyen_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung_Token",
                columns: table => new
                {
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NhaCungCapDangNhap = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoaiMa = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GiaTriMa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung_Token", x => new { x.NguoiDungId, x.NhaCungCapDangNhap, x.LoaiMa });
                    table.ForeignKey(
                        name: "FK_NguoiDung_Token_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung_VaiTro",
                columns: table => new
                {
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VaiTroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung_VaiTro", x => new { x.NguoiDungId, x.VaiTroId });
                    table.ForeignKey(
                        name: "FK_NguoiDung_VaiTro_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NguoiDung_VaiTro_VaiTro_VaiTroId",
                        column: x => x.VaiTroId,
                        principalTable: "VaiTro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "UserTrustHistory",
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
                    table.PrimaryKey("PK_UserTrustHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTrustHistory_NguoiDung_AppUserId",
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
                    TenChuanHoa = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    PhanTuChaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                        name: "FK_TinNhan_TinNhan_PhanTuChaId",
                        column: x => x.PhanTuChaId,
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
                    PhanTuChaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                        name: "FK_BinhLuan_BinhLuan_PhanTuChaId",
                        column: x => x.PhanTuChaId,
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
                name: "IX_BinhLuan_PhanTuChaId",
                table: "BinhLuan",
                column: "PhanTuChaId");

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
                name: "IX_DangNhapNgoai_NguoiDungId",
                table: "DangNhapNgoai",
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
                column: "EmailChuanHoa");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_MaNganh",
                table: "NguoiDung",
                column: "MaNganh");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "NguoiDung",
                column: "TenDangNhapChuanHoa",
                unique: true,
                filter: "[TenDangNhapChuanHoa] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_Quyen_NguoiDungId",
                table: "NguoiDung_Quyen",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_VaiTro_VaiTroId",
                table: "NguoiDung_VaiTro",
                column: "VaiTroId");

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
                name: "IX_TinNhan_PhanTuChaId",
                table: "TinNhan",
                column: "PhanTuChaId");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_TaoBoi",
                table: "TinNhan",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_Tep_TinNhanId",
                table: "TinNhan_Tep",
                column: "TinNhanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrustHistory_AppUserId",
                table: "UserTrustHistory",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "VaiTro",
                column: "TenChuanHoa",
                unique: true,
                filter: "[TenChuanHoa] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VaiTro_Quyen_VaiTroId",
                table: "VaiTro_Quyen",
                column: "VaiTroId");

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
                name: "BaoCao");

            migrationBuilder.DropTable(
                name: "CuocTroChuyen_ThanhVien");

            migrationBuilder.DropTable(
                name: "DangNhapNgoai");

            migrationBuilder.DropTable(
                name: "DanhGiaTaiLieu");

            migrationBuilder.DropTable(
                name: "NguoiDung_Quyen");

            migrationBuilder.DropTable(
                name: "NguoiDung_Token");

            migrationBuilder.DropTable(
                name: "NguoiDung_VaiTro");

            migrationBuilder.DropTable(
                name: "TaiLieu_Tep");

            migrationBuilder.DropTable(
                name: "TaiLieu_The");

            migrationBuilder.DropTable(
                name: "ThongBao_NguoiNhan");

            migrationBuilder.DropTable(
                name: "TinNhan_Tep");

            migrationBuilder.DropTable(
                name: "UserTrustHistory");

            migrationBuilder.DropTable(
                name: "VaiTro_Quyen");

            migrationBuilder.DropTable(
                name: "YeuCauThamGiaNhom");

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
                name: "VaiTro");

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
