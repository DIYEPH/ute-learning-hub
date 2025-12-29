using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UteLearningHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisiableConversation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaiDatHeThong",
                columns: table => new
                {
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GiaTri = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaiDatHeThong", x => x.Ten);
                });

            migrationBuilder.CreateTable(
                name: "DacTrungNguoiDung",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaTriVector = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiDiemTinhToan = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ConHieuLuc = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DacTrungNguoiDung", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Khoa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenKhoa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaKhoa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                name: "SuKien",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTaNgan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnhBanner = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DuongDan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BatDauLuc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    KetThucLuc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DangHoatDong = table.Column<bool>(type: "bit", nullable: false),
                    DoUuTien = table.Column<int>(type: "int", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuKien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TacGia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenTacGia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TacGia", x => x.Id);
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
                    KhoaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenNganh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaNganh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                        name: "FK_Nganh_Khoa_KhoaId",
                        column: x => x.KhoaId,
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
                    CapDoXacThuc = table.Column<int>(type: "int", nullable: false, computedColumnSql: "\r\n                CASE \r\n                    WHEN [DiemXacThuc] < 0 THEN 0\r\n                    WHEN [DiemXacThuc] < 5 THEN 1\r\n                    WHEN [DiemXacThuc] < 40 THEN 2\r\n                    WHEN [DiemXacThuc] < 100 THEN 3\r\n                    WHEN [DiemXacThuc] < 200 THEN 4\r\n                    ELSE 5\r\n                END", stored: true),
                    GioiTinh = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LanDangNhapGanNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                name: "LichSuDoTinCay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiemThayDoi = table.Column<double>(type: "float", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuDoTinCay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuDoTinCay_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonHoc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenMonHoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaMonHoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                    KichThuoc = table.Column<long>(type: "bigint", nullable: false),
                    LoaiFile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                        name: "FK_The_NguoiDung_TaoBoi",
                        column: x => x.TaoBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CuocTroChuyen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonHocId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TinNhanMoiNhat = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenCuocTroChuyen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoDuocTaoBoiAI = table.Column<bool>(type: "bit", nullable: false),
                    CoChoThanhVienGhimTinNhan = table.Column<bool>(type: "bit", nullable: false),
                    LoaiCuocTroChuyen = table.Column<int>(type: "int", nullable: false),
                    CheDo = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                name: "MonHoc_Nganh",
                columns: table => new
                {
                    MonHocId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NganhId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHoc_Nganh", x => new { x.MonHocId, x.NganhId });
                    table.ForeignKey(
                        name: "FK_MonHoc_Nganh_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonHoc_Nganh_Nganh_NganhId",
                        column: x => x.NganhId,
                        principalTable: "Nganh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaiLieu",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonHocId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LoaiTaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenChuanHoa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoHienThi = table.Column<int>(type: "int", nullable: false),
                    TepBiaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                        name: "FK_TaiLieu_LoaiTaiLieu_LoaiTaiLieuId",
                        column: x => x.LoaiTaiLieuId,
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
                    table.ForeignKey(
                        name: "FK_TaiLieu_TepDinhKem_TepBiaId",
                        column: x => x.TepBiaId,
                        principalTable: "TepDinhKem",
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
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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

            migrationBuilder.CreateTable(
                name: "DacTrungCuocTroChuyen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuocTroChuyenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaTriVector = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiDiemTinhToan = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ConHieuLuc = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DacTrungCuocTroChuyen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DacTrungCuocTroChuyen_CuocTroChuyen_CuocTroChuyenId",
                        column: x => x.CuocTroChuyenId,
                        principalTable: "CuocTroChuyen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    LoaiTinNhan = table.Column<int>(type: "int", nullable: true),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    DuyetBoiId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GhiChuDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                name: "TaiLieu_TacGia",
                columns: table => new
                {
                    TaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TacGiaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiLieu_TacGia", x => new { x.TaiLieuId, x.TacGiaId });
                    table.ForeignKey(
                        name: "FK_TaiLieu_TacGia_TacGia_TacGiaId",
                        column: x => x.TacGiaId,
                        principalTable: "TacGia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiLieu_TacGia_TaiLieu_TaiLieuId",
                        column: x => x.TaiLieuId,
                        principalTable: "TaiLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaiLieu_Tep",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TepDinhKemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TongSoTrang = table.Column<int>(type: "int", nullable: true),
                    LaChinh = table.Column<bool>(type: "bit", nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    TepBiaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoDaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayXoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BiXoaBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiLieu_Tep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaiLieu_Tep_TaiLieu_TaiLieuId",
                        column: x => x.TaiLieuId,
                        principalTable: "TaiLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiLieu_Tep_TepDinhKem_TepBiaId",
                        column: x => x.TepBiaId,
                        principalTable: "TepDinhKem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiLieu_Tep_TepDinhKem_TepDinhKemId",
                        column: x => x.TepDinhKemId,
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
                name: "BinhLuan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieu_TepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhanTuChaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                        name: "FK_BinhLuan_NguoiDung_TaoBoi",
                        column: x => x.TaoBoi,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BinhLuan_TaiLieu_Tep_TaiLieu_TepId",
                        column: x => x.TaiLieu_TepId,
                        principalTable: "TaiLieu_Tep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DanhGiaTaiLieu",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieu_TepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoaiDanhGia = table.Column<int>(type: "int", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_DanhGiaTaiLieu_TaiLieu_Tep_TaiLieu_TepId",
                        column: x => x.TaiLieu_TepId,
                        principalTable: "TaiLieu_Tep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichSuDocTaiLieu",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieu_TepId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrangCuoiCung = table.Column<int>(type: "int", nullable: false),
                    TongSoTrang = table.Column<int>(type: "int", nullable: true),
                    LanTruyCapGanNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuDocTaiLieu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuDocTaiLieu_TaiLieu_TaiLieuId",
                        column: x => x.TaiLieuId,
                        principalTable: "TaiLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LichSuDocTaiLieu_TaiLieu_Tep_TaiLieu_TepId",
                        column: x => x.TaiLieu_TepId,
                        principalTable: "TaiLieu_Tep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaoCao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieu_TepId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BinhLuanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaoBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapNhatBoi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    DuyetBoiId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DuyetLuc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GhiChuDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhienBanHang = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                        name: "FK_BaoCao_NguoiDung_DuyetBoiId",
                        column: x => x.DuyetBoiId,
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
                        name: "FK_BaoCao_TaiLieu_Tep_TaiLieu_TepId",
                        column: x => x.TaiLieu_TepId,
                        principalTable: "TaiLieu_Tep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_BinhLuanId",
                table: "BaoCao",
                column: "BinhLuanId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_DuyetBoiId",
                table: "BaoCao",
                column: "DuyetBoiId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_TaiLieu_TepId",
                table: "BaoCao",
                column: "TaiLieu_TepId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_TaoBoi",
                table: "BaoCao",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_PhanTuChaId",
                table: "BinhLuan",
                column: "PhanTuChaId");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_TaiLieu_TepId",
                table: "BinhLuan",
                column: "TaiLieu_TepId");

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
                name: "IX_CuocTroChuyen_The_CuocTroChuyenId",
                table: "CuocTroChuyen_The",
                column: "CuocTroChuyenId");

            migrationBuilder.CreateIndex(
                name: "IX_DacTrungCuocTroChuyen_ConHieuLuc",
                table: "DacTrungCuocTroChuyen",
                column: "ConHieuLuc");

            migrationBuilder.CreateIndex(
                name: "IX_DacTrungCuocTroChuyen_CuocTroChuyenId",
                table: "DacTrungCuocTroChuyen",
                column: "CuocTroChuyenId");

            migrationBuilder.CreateIndex(
                name: "IX_DacTrungNguoiDung_ConHieuLuc",
                table: "DacTrungNguoiDung",
                column: "ConHieuLuc");

            migrationBuilder.CreateIndex(
                name: "IX_DacTrungNguoiDung_NguoiDungId",
                table: "DacTrungNguoiDung",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_DangNhapNgoai_NguoiDungId",
                table: "DangNhapNgoai",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaTaiLieu_TaiLieu_TepId",
                table: "DanhGiaTaiLieu",
                column: "TaiLieu_TepId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaTaiLieu_TaiLieuId",
                table: "DanhGiaTaiLieu",
                column: "TaiLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaTaiLieu_TaoBoi",
                table: "DanhGiaTaiLieu",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDocTaiLieu_TaiLieu_TepId",
                table: "LichSuDocTaiLieu",
                column: "TaiLieu_TepId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDocTaiLieu_TaiLieuId",
                table: "LichSuDocTaiLieu",
                column: "TaiLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDoTinCay_NguoiDungId",
                table: "LichSuDoTinCay",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_MonHoc_TaoBoi",
                table: "MonHoc",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_MonHoc_Nganh_NganhId",
                table: "MonHoc_Nganh",
                column: "NganhId");

            migrationBuilder.CreateIndex(
                name: "IX_Nganh_KhoaId",
                table: "Nganh",
                column: "KhoaId");

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
                name: "IX_TaiLieu_LoaiTaiLieuId",
                table: "TaiLieu",
                column: "LoaiTaiLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_MonHocId",
                table: "TaiLieu",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_TaoBoi",
                table: "TaiLieu",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_TepBiaId",
                table: "TaiLieu",
                column: "TepBiaId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_TacGia_TacGiaId",
                table: "TaiLieu_TacGia",
                column: "TacGiaId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_Tep_TaiLieuId",
                table: "TaiLieu_Tep",
                column: "TaiLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_Tep_TepBiaId",
                table: "TaiLieu_Tep",
                column: "TepBiaId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_Tep_TepDinhKemId",
                table: "TaiLieu_Tep",
                column: "TepDinhKemId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_The_TaiLieuId",
                table: "TaiLieu_The",
                column: "TaiLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKem_TaoBoi",
                table: "TepDinhKem",
                column: "TaoBoi");

            migrationBuilder.CreateIndex(
                name: "IX_The_TaoBoi",
                table: "The",
                column: "TaoBoi");

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
                name: "CaiDatHeThong");

            migrationBuilder.DropTable(
                name: "CuocTroChuyen_ThanhVien");

            migrationBuilder.DropTable(
                name: "CuocTroChuyen_The");

            migrationBuilder.DropTable(
                name: "DacTrungCuocTroChuyen");

            migrationBuilder.DropTable(
                name: "DacTrungNguoiDung");

            migrationBuilder.DropTable(
                name: "DangNhapNgoai");

            migrationBuilder.DropTable(
                name: "DanhGiaTaiLieu");

            migrationBuilder.DropTable(
                name: "LichSuDocTaiLieu");

            migrationBuilder.DropTable(
                name: "LichSuDoTinCay");

            migrationBuilder.DropTable(
                name: "MonHoc_Nganh");

            migrationBuilder.DropTable(
                name: "NguoiDung_Quyen");

            migrationBuilder.DropTable(
                name: "NguoiDung_Token");

            migrationBuilder.DropTable(
                name: "NguoiDung_VaiTro");

            migrationBuilder.DropTable(
                name: "SuKien");

            migrationBuilder.DropTable(
                name: "TaiLieu_TacGia");

            migrationBuilder.DropTable(
                name: "TaiLieu_The");

            migrationBuilder.DropTable(
                name: "ThongBao_NguoiNhan");

            migrationBuilder.DropTable(
                name: "TinNhan_Tep");

            migrationBuilder.DropTable(
                name: "VaiTro_Quyen");

            migrationBuilder.DropTable(
                name: "YeuCauThamGiaNhom");

            migrationBuilder.DropTable(
                name: "BinhLuan");

            migrationBuilder.DropTable(
                name: "TacGia");

            migrationBuilder.DropTable(
                name: "The");

            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropTable(
                name: "TinNhan");

            migrationBuilder.DropTable(
                name: "VaiTro");

            migrationBuilder.DropTable(
                name: "TaiLieu_Tep");

            migrationBuilder.DropTable(
                name: "CuocTroChuyen");

            migrationBuilder.DropTable(
                name: "TaiLieu");

            migrationBuilder.DropTable(
                name: "LoaiTaiLieu");

            migrationBuilder.DropTable(
                name: "MonHoc");

            migrationBuilder.DropTable(
                name: "TepDinhKem");

            migrationBuilder.DropTable(
                name: "NguoiDung");

            migrationBuilder.DropTable(
                name: "Nganh");

            migrationBuilder.DropTable(
                name: "Khoa");
        }
    }
}
