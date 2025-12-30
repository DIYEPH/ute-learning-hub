-- =====================================================
-- UTE Learning Hub - SQL Seed Data Script
-- Generated: 2025-12-30
-- Purpose: Insert test data (Vietnamese column names)
-- Note: Run AFTER DataSeeder has created base data
-- =====================================================

-- =====================================================
-- GET REFERENCE IDs
-- =====================================================

-- Get Student role ID
DECLARE @StudentRoleId UNIQUEIDENTIFIER;
SELECT @StudentRoleId = Id FROM VaiTro WHERE TenChuanHoa = 'STUDENT';

-- Get Admin ID
DECLARE @AdminId UNIQUEIDENTIFIER;
SELECT @AdminId = Id FROM NguoiDung WHERE Email = 'admin@ute.edu.vn';

-- Get Major IDs
DECLARE @CNTT_MajorId UNIQUEIDENTIFIER;
DECLARE @CK_MajorId UNIQUEIDENTIFIER;
DECLARE @DDT_MajorId UNIQUEIDENTIFIER;
DECLARE @XD_MajorId UNIQUEIDENTIFIER;
DECLARE @HH_MajorId UNIQUEIDENTIFIER;
DECLARE @SPCN_MajorId UNIQUEIDENTIFIER;

SELECT @CNTT_MajorId = Id FROM Nganh WHERE MaNganh = '7480201';
SELECT @CK_MajorId = Id FROM Nganh WHERE MaNganh = '7510201';
SELECT @DDT_MajorId = Id FROM Nganh WHERE MaNganh = '7510302';
SELECT @XD_MajorId = Id FROM Nganh WHERE MaNganh = '7510103';
SELECT @HH_MajorId = Id FROM Nganh WHERE MaNganh = '7510401';
SELECT @SPCN_MajorId = Id FROM Nganh WHERE MaNganh = '7140214';

-- Get Type IDs
DECLARE @Type1Id UNIQUEIDENTIFIER;
DECLARE @Type2Id UNIQUEIDENTIFIER;
DECLARE @Type3Id UNIQUEIDENTIFIER;
SELECT TOP 1 @Type1Id = Id FROM LoaiTaiLieu ORDER BY NgayTao;
SELECT TOP 1 @Type2Id = Id FROM LoaiTaiLieu WHERE Id != @Type1Id ORDER BY NgayTao;
SELECT TOP 1 @Type3Id = Id FROM LoaiTaiLieu WHERE Id NOT IN (@Type1Id, @Type2Id) ORDER BY NgayTao;

-- Get Subject IDs
DECLARE @Subject1Id UNIQUEIDENTIFIER;
DECLARE @Subject2Id UNIQUEIDENTIFIER;
DECLARE @Subject3Id UNIQUEIDENTIFIER;
DECLARE @Subject4Id UNIQUEIDENTIFIER;
DECLARE @Subject5Id UNIQUEIDENTIFIER;
SELECT @Subject1Id = Id FROM MonHoc WHERE MaMonHoc = '5505166'; -- Lập trình C
SELECT @Subject2Id = Id FROM MonHoc WHERE MaMonHoc = '5505127'; -- CSDL I
SELECT @Subject3Id = Id FROM MonHoc WHERE MaMonHoc = '5505181'; -- Mạng
SELECT @Subject4Id = Id FROM MonHoc WHERE MaMonHoc = '5505168'; -- OOP
SELECT @Subject5Id = Id FROM MonHoc WHERE MaMonHoc = '5505226'; -- AI

-- Get Tag IDs
DECLARE @Tag1Id UNIQUEIDENTIFIER;
DECLARE @Tag2Id UNIQUEIDENTIFIER;
DECLARE @Tag3Id UNIQUEIDENTIFIER;
DECLARE @Tag4Id UNIQUEIDENTIFIER;
DECLARE @Tag5Id UNIQUEIDENTIFIER;
SELECT @Tag1Id = Id FROM [The] WHERE TenTag = 'C#';
SELECT @Tag2Id = Id FROM [The] WHERE TenTag = 'Java';
SELECT @Tag3Id = Id FROM [The] WHERE TenTag = 'Python';
SELECT @Tag4Id = Id FROM [The] WHERE TenTag = 'Machine Learning';
SELECT @Tag5Id = Id FROM [The] WHERE TenTag = N'Cấu trúc dữ liệu';

-- =====================================================
-- INSERT USERS (20 students)
-- =====================================================

DECLARE @User5Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO NguoiDung (Id, TenDangNhap, TenDangNhapChuanHoa, Email, EmailChuanHoa, CoXacThucEmail, MatKhauBam, MaBaoMat, DauKiemSoatDongBo, SoDienThoai, CoDaXacNhanSoDienThoai, CoKichHoatXacThucHaiLop, NgayHetKhoaTaiKhoan, ChoPhepKhoaTaiKhoan, SoLanDangNhapThatBai, MaNganh, GioiThieu, HinhDaiDien, HoVaTen, DiemXacThuc, CoGoiY, CapDoXacThuc, GioiTinh, NgayTao, CoDaXoa)
VALUES (@User5Id, 'nguyenvana', 'NGUYENVANA', 'nguyenvana@student.ute.edu.vn', 'NGUYENVANA@STUDENT.UTE.EDU.VN', 1, 'AQAAAAIAAYagAAAAEOXrWfWoQ2sBmKlKlXFYL5u5L5K3c8Q6p7R8S9T0U1V2W3X4Y5Z6a7b8c9d0e1f2g3h4', NEWID(), NEWID(), NULL, 0, 0, NULL, 1, 0, @CNTT_MajorId, N'Sinh viên năm 3 CNTT', 'https://i.pravatar.cc/150?img=1', N'Nguyễn Văn A', 15, 0, 2, 1, GETUTCDATE(), 0);
INSERT INTO NguoiDung_VaiTro (NguoiDungId, VaiTroId) VALUES (@User5Id, @StudentRoleId);

DECLARE @User6Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO NguoiDung (Id, TenDangNhap, TenDangNhapChuanHoa, Email, EmailChuanHoa, CoXacThucEmail, MatKhauBam, MaBaoMat, DauKiemSoatDongBo, SoDienThoai, CoDaXacNhanSoDienThoai, CoKichHoatXacThucHaiLop, NgayHetKhoaTaiKhoan, ChoPhepKhoaTaiKhoan, SoLanDangNhapThatBai, MaNganh, GioiThieu, HinhDaiDien, HoVaTen, DiemXacThuc, CoGoiY, CapDoXacThuc, GioiTinh, NgayTao, CoDaXoa)
VALUES (@User6Id, 'tranthib', 'TRANTHIB', 'tranthib@student.ute.edu.vn', 'TRANTHIB@STUDENT.UTE.EDU.VN', 1, 'AQAAAAIAAYagAAAAEOXrWfWoQ2sBmKlKlXFYL5u5L5K3c8Q6p7R8S9T0U1V2W3X4Y5Z6a7b8c9d0e1f2g3h4', NEWID(), NEWID(), NULL, 0, 0, NULL, 1, 0, @CK_MajorId, N'Sinh viên năm 4 Cơ khí', 'https://i.pravatar.cc/150?img=2', N'Trần Thị B', 25, 0, 2, 2, GETUTCDATE(), 0);
INSERT INTO NguoiDung_VaiTro (NguoiDungId, VaiTroId) VALUES (@User6Id, @StudentRoleId);

DECLARE @User7Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO NguoiDung (Id, TenDangNhap, TenDangNhapChuanHoa, Email, EmailChuanHoa, CoXacThucEmail, MatKhauBam, MaBaoMat, DauKiemSoatDongBo, SoDienThoai, CoDaXacNhanSoDienThoai, CoKichHoatXacThucHaiLop, NgayHetKhoaTaiKhoan, ChoPhepKhoaTaiKhoan, SoLanDangNhapThatBai, MaNganh, GioiThieu, HinhDaiDien, HoVaTen, DiemXacThuc, CoGoiY, CapDoXacThuc, GioiTinh, NgayTao, CoDaXoa)
VALUES (@User7Id, 'levanhung', 'LEVANHUNG', 'levanhung@student.ute.edu.vn', 'LEVANHUNG@STUDENT.UTE.EDU.VN', 1, 'AQAAAAIAAYagAAAAEOXrWfWoQ2sBmKlKlXFYL5u5L5K3c8Q6p7R8S9T0U1V2W3X4Y5Z6a7b8c9d0e1f2g3h4', NEWID(), NEWID(), NULL, 0, 0, NULL, 1, 0, @DDT_MajorId, N'Sinh viên năm 2 Điện tử', 'https://i.pravatar.cc/150?img=3', N'Lê Văn Hùng', 8, 0, 1, 1, GETUTCDATE(), 0);
INSERT INTO NguoiDung_VaiTro (NguoiDungId, VaiTroId) VALUES (@User7Id, @StudentRoleId);

DECLARE @User8Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO NguoiDung (Id, TenDangNhap, TenDangNhapChuanHoa, Email, EmailChuanHoa, CoXacThucEmail, MatKhauBam, MaBaoMat, DauKiemSoatDongBo, SoDienThoai, CoDaXacNhanSoDienThoai, CoKichHoatXacThucHaiLop, NgayHetKhoaTaiKhoan, ChoPhepKhoaTaiKhoan, SoLanDangNhapThatBai, MaNganh, GioiThieu, HinhDaiDien, HoVaTen, DiemXacThuc, CoGoiY, CapDoXacThuc, GioiTinh, NgayTao, CoDaXoa)
VALUES (@User8Id, 'phamthilan', 'PHAMTHILAN', 'phamthilan@student.ute.edu.vn', 'PHAMTHILAN@STUDENT.UTE.EDU.VN', 1, 'AQAAAAIAAYagAAAAEOXrWfWoQ2sBmKlKlXFYL5u5L5K3c8Q6p7R8S9T0U1V2W3X4Y5Z6a7b8c9d0e1f2g3h4', NEWID(), NEWID(), NULL, 0, 0, NULL, 1, 0, @XD_MajorId, N'Sinh viên năm 3 Xây dựng', 'https://i.pravatar.cc/150?img=4', N'Phạm Thị Lan', 35, 0, 3, 2, GETUTCDATE(), 0);
INSERT INTO NguoiDung_VaiTro (NguoiDungId, VaiTroId) VALUES (@User8Id, @StudentRoleId);

DECLARE @User9Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO NguoiDung (Id, TenDangNhap, TenDangNhapChuanHoa, Email, EmailChuanHoa, CoXacThucEmail, MatKhauBam, MaBaoMat, DauKiemSoatDongBo, SoDienThoai, CoDaXacNhanSoDienThoai, CoKichHoatXacThucHaiLop, NgayHetKhoaTaiKhoan, ChoPhepKhoaTaiKhoan, SoLanDangNhapThatBai, MaNganh, GioiThieu, HinhDaiDien, HoVaTen, DiemXacThuc, CoGoiY, CapDoXacThuc, GioiTinh, NgayTao, CoDaXoa)
VALUES (@User9Id, 'hoangvanduc', 'HOANGVANDUC', 'hoangvanduc@student.ute.edu.vn', 'HOANGVANDUC@STUDENT.UTE.EDU.VN', 1, 'AQAAAAIAAYagAAAAEOXrWfWoQ2sBmKlKlXFYL5u5L5K3c8Q6p7R8S9T0U1V2W3X4Y5Z6a7b8c9d0e1f2g3h4', NEWID(), NEWID(), NULL, 0, 0, NULL, 1, 0, @HH_MajorId, N'Sinh viên năm 4 Hóa học', 'https://i.pravatar.cc/150?img=5', N'Hoàng Văn Đức', 50, 0, 3, 1, GETUTCDATE(), 0);
INSERT INTO NguoiDung_VaiTro (NguoiDungId, VaiTroId) VALUES (@User9Id, @StudentRoleId);

DECLARE @User10Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO NguoiDung (Id, TenDangNhap, TenDangNhapChuanHoa, Email, EmailChuanHoa, CoXacThucEmail, MatKhauBam, MaBaoMat, DauKiemSoatDongBo, SoDienThoai, CoDaXacNhanSoDienThoai, CoKichHoatXacThucHaiLop, NgayHetKhoaTaiKhoan, ChoPhepKhoaTaiKhoan, SoLanDangNhapThatBai, MaNganh, GioiThieu, HinhDaiDien, HoVaTen, DiemXacThuc, CoGoiY, CapDoXacThuc, GioiTinh, NgayTao, CoDaXoa)
VALUES (@User10Id, 'vuthimai', 'VUTHIMAI', 'vuthimai@student.ute.edu.vn', 'VUTHIMAI@STUDENT.UTE.EDU.VN', 1, 'AQAAAAIAAYagAAAAEOXrWfWoQ2sBmKlKlXFYL5u5L5K3c8Q6p7R8S9T0U1V2W3X4Y5Z6a7b8c9d0e1f2g3h4', NEWID(), NEWID(), NULL, 0, 0, NULL, 1, 0, @SPCN_MajorId, N'Sinh viên năm 3 SPKT', 'https://i.pravatar.cc/150?img=6', N'Vũ Thị Mai', 20, 0, 2, 2, GETUTCDATE(), 0);
INSERT INTO NguoiDung_VaiTro (NguoiDungId, VaiTroId) VALUES (@User10Id, @StudentRoleId);

DECLARE @User11Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO NguoiDung (Id, TenDangNhap, TenDangNhapChuanHoa, Email, EmailChuanHoa, CoXacThucEmail, MatKhauBam, MaBaoMat, DauKiemSoatDongBo, SoDienThoai, CoDaXacNhanSoDienThoai, CoKichHoatXacThucHaiLop, NgayHetKhoaTaiKhoan, ChoPhepKhoaTaiKhoan, SoLanDangNhapThatBai, MaNganh, GioiThieu, HinhDaiDien, HoVaTen, DiemXacThuc, CoGoiY, CapDoXacThuc, GioiTinh, NgayTao, CoDaXoa)
VALUES (@User11Id, 'dangvantuan', 'DANGVANTUAN', 'dangvantuan@student.ute.edu.vn', 'DANGVANTUAN@STUDENT.UTE.EDU.VN', 1, 'AQAAAAIAAYagAAAAEOXrWfWoQ2sBmKlKlXFYL5u5L5K3c8Q6p7R8S9T0U1V2W3X4Y5Z6a7b8c9d0e1f2g3h4', NEWID(), NEWID(), NULL, 0, 0, NULL, 1, 0, @CNTT_MajorId, N'Sinh viên năm 4 CNTT, AI/ML', 'https://i.pravatar.cc/150?img=7', N'Đặng Văn Tuấn', 70, 0, 4, 1, GETUTCDATE(), 0);
INSERT INTO NguoiDung_VaiTro (NguoiDungId, VaiTroId) VALUES (@User11Id, @StudentRoleId);

DECLARE @User12Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO NguoiDung (Id, TenDangNhap, TenDangNhapChuanHoa, Email, EmailChuanHoa, CoXacThucEmail, MatKhauBam, MaBaoMat, DauKiemSoatDongBo, SoDienThoai, CoDaXacNhanSoDienThoai, CoKichHoatXacThucHaiLop, NgayHetKhoaTaiKhoan, ChoPhepKhoaTaiKhoan, SoLanDangNhapThatBai, MaNganh, GioiThieu, HinhDaiDien, HoVaTen, DiemXacThuc, CoGoiY, CapDoXacThuc, GioiTinh, NgayTao, CoDaXoa)
VALUES (@User12Id, 'ngothihuong', 'NGOTHIHUONG', 'ngothihuong@student.ute.edu.vn', 'NGOTHIHUONG@STUDENT.UTE.EDU.VN', 1, 'AQAAAAIAAYagAAAAEOXrWfWoQ2sBmKlKlXFYL5u5L5K3c8Q6p7R8S9T0U1V2W3X4Y5Z6a7b8c9d0e1f2g3h4', NEWID(), NEWID(), NULL, 0, 0, NULL, 1, 0, @CK_MajorId, N'Sinh viên năm 2 Cơ khí', 'https://i.pravatar.cc/150?img=8', N'Ngô Thị Hương', 5, 0, 1, 2, GETUTCDATE(), 0);
INSERT INTO NguoiDung_VaiTro (NguoiDungId, VaiTroId) VALUES (@User12Id, @StudentRoleId);

DECLARE @User13Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO NguoiDung (Id, TenDangNhap, TenDangNhapChuanHoa, Email, EmailChuanHoa, CoXacThucEmail, MatKhauBam, MaBaoMat, DauKiemSoatDongBo, SoDienThoai, CoDaXacNhanSoDienThoai, CoKichHoatXacThucHaiLop, NgayHetKhoaTaiKhoan, ChoPhepKhoaTaiKhoan, SoLanDangNhapThatBai, MaNganh, GioiThieu, HinhDaiDien, HoVaTen, DiemXacThuc, CoGoiY, CapDoXacThuc, GioiTinh, NgayTao, CoDaXoa)
VALUES (@User13Id, 'buivankhanh', 'BUIVANKHANH', 'buivankhanh@student.ute.edu.vn', 'BUIVANKHANH@STUDENT.UTE.EDU.VN', 1, 'AQAAAAIAAYagAAAAEOXrWfWoQ2sBmKlKlXFYL5u5L5K3c8Q6p7R8S9T0U1V2W3X4Y5Z6a7b8c9d0e1f2g3h4', NEWID(), NEWID(), NULL, 0, 0, NULL, 1, 0, @DDT_MajorId, N'Sinh viên năm 3 Điện tử', 'https://i.pravatar.cc/150?img=9', N'Bùi Văn Khánh', 28, 0, 2, 1, GETUTCDATE(), 0);
INSERT INTO NguoiDung_VaiTro (NguoiDungId, VaiTroId) VALUES (@User13Id, @StudentRoleId);

DECLARE @User14Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO NguoiDung (Id, TenDangNhap, TenDangNhapChuanHoa, Email, EmailChuanHoa, CoXacThucEmail, MatKhauBam, MaBaoMat, DauKiemSoatDongBo, SoDienThoai, CoDaXacNhanSoDienThoai, CoKichHoatXacThucHaiLop, NgayHetKhoaTaiKhoan, ChoPhepKhoaTaiKhoan, SoLanDangNhapThatBai, MaNganh, GioiThieu, HinhDaiDien, HoVaTen, DiemXacThuc, CoGoiY, CapDoXacThuc, GioiTinh, NgayTao, CoDaXoa)
VALUES (@User14Id, 'dothithuy', 'DOTHITHUY', 'dothithuy@student.ute.edu.vn', 'DOTHITHUY@STUDENT.UTE.EDU.VN', 1, 'AQAAAAIAAYagAAAAEOXrWfWoQ2sBmKlKlXFYL5u5L5K3c8Q6p7R8S9T0U1V2W3X4Y5Z6a7b8c9d0e1f2g3h4', NEWID(), NEWID(), NULL, 0, 0, NULL, 1, 0, @XD_MajorId, N'Sinh viên năm 2 Xây dựng', 'https://i.pravatar.cc/150?img=10', N'Đỗ Thị Thủy', 12, 0, 2, 2, GETUTCDATE(), 0);
INSERT INTO NguoiDung_VaiTro (NguoiDungId, VaiTroId) VALUES (@User14Id, @StudentRoleId);

PRINT N'Đã thêm 10 users!';

-- =====================================================
-- INSERT DOCUMENTS (10 documents)
-- =====================================================

DECLARE @Doc1Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TaiLieu (Id, MonHocId, LoaiTaiLieuId, MoTa, DocumentName, TenChuanHoa, CoHienThi, TepBiaId, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Doc1Id, @Subject1Id, @Type1Id, N'Hướng dẫn lập trình C', N'Giáo trình Lập trình C', N'GIAO TRINH LAP TRINH C', 0, NULL, @User5Id, DATEADD(DAY, -30, GETUTCDATE()), 0);

DECLARE @Doc2Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TaiLieu (Id, MonHocId, LoaiTaiLieuId, MoTa, DocumentName, TenChuanHoa, CoHienThi, TepBiaId, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Doc2Id, @Subject2Id, @Type1Id, N'Bài tập SQL và thiết kế CSDL', N'Bài tập CSDL có lời giải', N'BAI TAP CSDL CO LOI GIAI', 0, NULL, @User11Id, DATEADD(DAY, -28, GETUTCDATE()), 0);

DECLARE @Doc3Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TaiLieu (Id, MonHocId, LoaiTaiLieuId, MoTa, DocumentName, TenChuanHoa, CoHienThi, TepBiaId, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Doc3Id, @Subject3Id, @Type2Id, N'Slide bài giảng mạng máy tính', N'Slide Mạng máy tính', N'SLIDE MANG MAY TINH', 1, NULL, @User8Id, DATEADD(DAY, -25, GETUTCDATE()), 0);

DECLARE @Doc4Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TaiLieu (Id, MonHocId, LoaiTaiLieuId, MoTa, DocumentName, TenChuanHoa, CoHienThi, TepBiaId, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Doc4Id, @Subject4Id, @Type1Id, N'Giáo trình OOP với Java', N'Lập trình hướng đối tượng Java', N'LAP TRINH HUONG DOI TUONG JAVA', 0, NULL, @User11Id, DATEADD(DAY, -22, GETUTCDATE()), 0);

DECLARE @Doc5Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TaiLieu (Id, MonHocId, LoaiTaiLieuId, MoTa, DocumentName, TenChuanHoa, CoHienThi, TepBiaId, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Doc5Id, @Subject5Id, @Type3Id, N'Đồ án Machine Learning', N'Đồ án AI - Face Recognition', N'DO AN AI FACE RECOGNITION', 1, NULL, @User11Id, DATEADD(DAY, -20, GETUTCDATE()), 0);

PRINT N'Đã thêm 5 documents!';

-- =====================================================
-- INSERT DOCUMENT TAGS
-- =====================================================
INSERT INTO [TaiLieu_The] (TaiLieuId, TheId) VALUES (@Doc1Id, @Tag1Id);
INSERT INTO [TaiLieu_The] (TaiLieuId, TheId) VALUES (@Doc1Id, @Tag5Id);
INSERT INTO [TaiLieu_The] (TaiLieuId, TheId) VALUES (@Doc2Id, @Tag1Id);
INSERT INTO [TaiLieu_The] (TaiLieuId, TheId) VALUES (@Doc3Id, @Tag3Id);
INSERT INTO [TaiLieu_The] (TaiLieuId, TheId) VALUES (@Doc4Id, @Tag2Id);
INSERT INTO [TaiLieu_The] (TaiLieuId, TheId) VALUES (@Doc5Id, @Tag3Id);
INSERT INTO [TaiLieu_The] (TaiLieuId, TheId) VALUES (@Doc5Id, @Tag4Id);

PRINT N'Đã thêm document tags!';

-- =====================================================
-- INSERT CONVERSATIONS (10 conversations)
-- =====================================================

DECLARE @Conv1Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO CuocTroChuyen (Id, MonHocId, TinNhanMoiNhat, TenCuocTroChuyen, AnhDaiDien, CoDuocTaoBoiAI, CoChoThanhVienGhimTinNhan, LoaiCuocTroChuyen, CheDo, TrangThai, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Conv1Id, @Subject1Id, NULL, N'Nhóm học Lập trình C', 'https://i.pravatar.cc/150?img=30', 0, 1, 1, 1, 0, @User5Id, DATEADD(DAY, -25, GETUTCDATE()), 0);

DECLARE @Conv2Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO CuocTroChuyen (Id, MonHocId, TinNhanMoiNhat, TenCuocTroChuyen, AnhDaiDien, CoDuocTaoBoiAI, CoChoThanhVienGhimTinNhan, LoaiCuocTroChuyen, CheDo, TrangThai, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Conv2Id, @Subject2Id, NULL, N'CSDL - Lớp 21ĐHCNTT01', 'https://i.pravatar.cc/150?img=31', 0, 1, 1, 1, 0, @User11Id, DATEADD(DAY, -23, GETUTCDATE()), 0);

DECLARE @Conv3Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO CuocTroChuyen (Id, MonHocId, TinNhanMoiNhat, TenCuocTroChuyen, AnhDaiDien, CoDuocTaoBoiAI, CoChoThanhVienGhimTinNhan, LoaiCuocTroChuyen, CheDo, TrangThai, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Conv3Id, @Subject3Id, NULL, N'Mạng máy tính - HK1 2024', 'https://i.pravatar.cc/150?img=32', 0, 0, 1, 1, 0, @User8Id, DATEADD(DAY, -20, GETUTCDATE()), 0);

DECLARE @Conv4Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO CuocTroChuyen (Id, MonHocId, TinNhanMoiNhat, TenCuocTroChuyen, AnhDaiDien, CoDuocTaoBoiAI, CoChoThanhVienGhimTinNhan, LoaiCuocTroChuyen, CheDo, TrangThai, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Conv4Id, @Subject5Id, NULL, N'AI/ML Research Group', 'https://i.pravatar.cc/150?img=33', 1, 1, 1, 1, 0, @User11Id, DATEADD(DAY, -18, GETUTCDATE()), 0);

DECLARE @Conv5Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO CuocTroChuyen (Id, MonHocId, TinNhanMoiNhat, TenCuocTroChuyen, AnhDaiDien, CoDuocTaoBoiAI, CoChoThanhVienGhimTinNhan, LoaiCuocTroChuyen, CheDo, TrangThai, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Conv5Id, @Subject4Id, NULL, N'OOP Java Study', 'https://i.pravatar.cc/150?img=34', 0, 1, 1, 0, 0, @User11Id, DATEADD(DAY, -15, GETUTCDATE()), 0);

PRINT N'Đã thêm 5 conversations!';

-- =====================================================
-- INSERT CONVERSATION MEMBERS
-- =====================================================

-- Conv1 members (QuyenNhom: 0=Admin, 1=Member)
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, NgayTao, CoDaXoa) VALUES (NEWID(), @User5Id, @Conv1Id, NULL, 0, 0, DATEADD(DAY, -25, GETUTCDATE()), 0);
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, NgayTao, CoDaXoa) VALUES (NEWID(), @User6Id, @Conv1Id, NULL, 0, 1, DATEADD(DAY, -24, GETUTCDATE()), 0);
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, NgayTao, CoDaXoa) VALUES (NEWID(), @User7Id, @Conv1Id, NULL, 0, 1, DATEADD(DAY, -23, GETUTCDATE()), 0);

-- Conv2 members
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, NgayTao, CoDaXoa) VALUES (NEWID(), @User11Id, @Conv2Id, NULL, 0, 0, DATEADD(DAY, -23, GETUTCDATE()), 0);
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, NgayTao, CoDaXoa) VALUES (NEWID(), @User5Id, @Conv2Id, NULL, 0, 1, DATEADD(DAY, -22, GETUTCDATE()), 0);

-- Conv3 members
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, NgayTao, CoDaXoa) VALUES (NEWID(), @User8Id, @Conv3Id, NULL, 0, 0, DATEADD(DAY, -20, GETUTCDATE()), 0);
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, NgayTao, CoDaXoa) VALUES (NEWID(), @User13Id, @Conv3Id, NULL, 0, 1, DATEADD(DAY, -19, GETUTCDATE()), 0);

-- Conv4 members (AI group)
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, NgayTao, CoDaXoa) VALUES (NEWID(), @User11Id, @Conv4Id, NULL, 0, 0, DATEADD(DAY, -18, GETUTCDATE()), 0);
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, NgayTao, CoDaXoa) VALUES (NEWID(), @User5Id, @Conv4Id, NULL, 0, 1, DATEADD(DAY, -15, GETUTCDATE()), 0);

-- Conv5 members
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, NgayTao, CoDaXoa) VALUES (NEWID(), @User11Id, @Conv5Id, NULL, 0, 0, DATEADD(DAY, -15, GETUTCDATE()), 0);
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, NgayTao, CoDaXoa) VALUES (NEWID(), @User9Id, @Conv5Id, NULL, 0, 1, DATEADD(DAY, -13, GETUTCDATE()), 0);

PRINT N'Đã thêm conversation members!';

-- =====================================================
-- INSERT CONVERSATION TAGS
-- =====================================================
INSERT INTO [CuocTroChuyen_The] (CuocTroChuyenId, TheId) VALUES (@Conv1Id, @Tag1Id);
INSERT INTO [CuocTroChuyen_The] (CuocTroChuyenId, TheId) VALUES (@Conv1Id, @Tag5Id);
INSERT INTO [CuocTroChuyen_The] (CuocTroChuyenId, TheId) VALUES (@Conv2Id, @Tag1Id);
INSERT INTO [CuocTroChuyen_The] (CuocTroChuyenId, TheId) VALUES (@Conv3Id, @Tag3Id);
INSERT INTO [CuocTroChuyen_The] (CuocTroChuyenId, TheId) VALUES (@Conv4Id, @Tag3Id);
INSERT INTO [CuocTroChuyen_The] (CuocTroChuyenId, TheId) VALUES (@Conv4Id, @Tag4Id);
INSERT INTO [CuocTroChuyen_The] (CuocTroChuyenId, TheId) VALUES (@Conv5Id, @Tag2Id);

PRINT N'Đã thêm conversation tags!';

-- =====================================================
-- INSERT MESSAGES (LoaiTinNhan: 0=ConversationCreated, NULL=normal message)
-- =====================================================

DECLARE @Msg1Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TinNhan (Id, CuocTroChuyenId, PhanTuChaId, NoiDung, CoChinhSua, CoDaGhim, LoaiTinNhan, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Msg1Id, @Conv1Id, NULL, N'Chào mọi người! Nhóm học Lập trình C!', 0, 0, 0, @User5Id, DATEADD(DAY, -25, GETUTCDATE()), 0);

DECLARE @Msg2Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TinNhan (Id, CuocTroChuyenId, PhanTuChaId, NoiDung, CoChinhSua, CoDaGhim, LoaiTinNhan, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Msg2Id, @Conv1Id, NULL, N'Xin chào! Em mới join nhóm ạ', 0, 0, NULL, @User6Id, DATEADD(DAY, -24, GETUTCDATE()), 0);

DECLARE @Msg3Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TinNhan (Id, CuocTroChuyenId, PhanTuChaId, NoiDung, CoChinhSua, CoDaGhim, LoaiTinNhan, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Msg3Id, @Conv1Id, NULL, N'Có ai có tài liệu về pointer không ạ?', 0, 0, NULL, @User7Id, DATEADD(DAY, -23, GETUTCDATE()), 0);

DECLARE @Msg4Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TinNhan (Id, CuocTroChuyenId, PhanTuChaId, NoiDung, CoChinhSua, CoDaGhim, LoaiTinNhan, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Msg4Id, @Conv1Id, @Msg3Id, N'Có nè! Em check trong Documents nhé', 0, 0, NULL, @User5Id, DATEADD(DAY, -23, GETUTCDATE()), 0);

DECLARE @Msg5Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TinNhan (Id, CuocTroChuyenId, PhanTuChaId, NoiDung, CoChinhSua, CoDaGhim, LoaiTinNhan, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Msg5Id, @Conv2Id, NULL, N'Welcome to CSDL group!', 0, 0, 0, @User11Id, DATEADD(DAY, -23, GETUTCDATE()), 0);

DECLARE @Msg6Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TinNhan (Id, CuocTroChuyenId, PhanTuChaId, NoiDung, CoChinhSua, CoDaGhim, LoaiTinNhan, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Msg6Id, @Conv2Id, NULL, N'Bài tập về normalization khó quá 😭', 0, 0, NULL, @User5Id, DATEADD(DAY, -21, GETUTCDATE()), 0);

DECLARE @Msg7Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TinNhan (Id, CuocTroChuyenId, PhanTuChaId, NoiDung, CoChinhSua, CoDaGhim, LoaiTinNhan, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Msg7Id, @Conv3Id, NULL, N'Nhóm Mạng máy tính - Chào mọi người!', 0, 0, 0, @User8Id, DATEADD(DAY, -20, GETUTCDATE()), 0);

DECLARE @Msg8Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TinNhan (Id, CuocTroChuyenId, PhanTuChaId, NoiDung, CoChinhSua, CoDaGhim, LoaiTinNhan, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Msg8Id, @Conv4Id, NULL, N'AI/ML Research Group - Share papers hàng tuần!', 0, 0, 0, @User11Id, DATEADD(DAY, -18, GETUTCDATE()), 0);

DECLARE @Msg9Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TinNhan (Id, CuocTroChuyenId, PhanTuChaId, NoiDung, CoChinhSua, CoDaGhim, LoaiTinNhan, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Msg9Id, @Conv4Id, NULL, N'Paper hay: Attention Is All You Need', 0, 1, NULL, @User11Id, DATEADD(DAY, -16, GETUTCDATE()), 0);

DECLARE @Msg10Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO TinNhan (Id, CuocTroChuyenId, PhanTuChaId, NoiDung, CoChinhSua, CoDaGhim, LoaiTinNhan, TaoBoi, NgayTao, CoDaXoa)
VALUES (@Msg10Id, @Conv5Id, NULL, N'OOP Study Group created!', 0, 0, 0, @User11Id, DATEADD(DAY, -15, GETUTCDATE()), 0);

PRINT N'Đã thêm 10 messages!';

-- =====================================================
-- SUMMARY
-- =====================================================
PRINT N'';
PRINT N'===========================================';
PRINT N'KẾT QUẢ SEED DATA (Vietnamese columns):';
PRINT N'- 10 Users mới';
PRINT N'- 5 Documents';
PRINT N'- 7 Document Tags';
PRINT N'- 5 Conversations';
PRINT N'- 12 Conversation Members';
PRINT N'- 7 Conversation Tags';
PRINT N'- 10 Messages';
PRINT N'===========================================';
GO
