-- =====================================================
-- UTE Learning Hub - SQL Seed Data Script
-- Generated: 2026-01-14
-- Purpose: Insert comprehensive test data
-- Note: Run AFTER DataSeeder has created base data
-- =====================================================

-- Get Role IDs
DECLARE @StudentRoleId UNIQUEIDENTIFIER;
SELECT @StudentRoleId = Id FROM VaiTro WHERE TenChuanHoa = 'STUDENT';

DECLARE @AdminId UNIQUEIDENTIFIER;
SELECT @AdminId = Id FROM NguoiDung WHERE Email = 'admin@ute.edu.vn';

-- Get Major IDs
DECLARE @CNTT UNIQUEIDENTIFIER, @CK UNIQUEIDENTIFIER, @DDT UNIQUEIDENTIFIER, @XD UNIQUEIDENTIFIER, @HH UNIQUEIDENTIFIER, @SPCN UNIQUEIDENTIFIER;
SELECT @CNTT = Id FROM Nganh WHERE MaNganh = '7480201';
SELECT @CK = Id FROM Nganh WHERE MaNganh = '7510201';
SELECT @DDT = Id FROM Nganh WHERE MaNganh = '7510302';
SELECT @XD = Id FROM Nganh WHERE MaNganh = '7510103';
SELECT @HH = Id FROM Nganh WHERE MaNganh = '7510401';
SELECT @SPCN = Id FROM Nganh WHERE MaNganh = '7140214';

-- Get Type IDs  
DECLARE @Type1 UNIQUEIDENTIFIER, @Type2 UNIQUEIDENTIFIER, @Type3 UNIQUEIDENTIFIER;
SELECT TOP 1 @Type1 = Id FROM LoaiTaiLieu ORDER BY NgayTao;
SELECT TOP 1 @Type2 = Id FROM LoaiTaiLieu WHERE Id != @Type1 ORDER BY NgayTao;
SELECT TOP 1 @Type3 = Id FROM LoaiTaiLieu WHERE Id NOT IN (@Type1, @Type2) ORDER BY NgayTao;

-- Get Subject IDs
DECLARE @Subj1 UNIQUEIDENTIFIER, @Subj2 UNIQUEIDENTIFIER, @Subj3 UNIQUEIDENTIFIER, @Subj4 UNIQUEIDENTIFIER, @Subj5 UNIQUEIDENTIFIER;
DECLARE @Subj6 UNIQUEIDENTIFIER, @Subj7 UNIQUEIDENTIFIER, @Subj8 UNIQUEIDENTIFIER;
SELECT @Subj1 = Id FROM MonHoc WHERE MaMonHoc = '5505166'; -- Lập trình C
SELECT @Subj2 = Id FROM MonHoc WHERE MaMonHoc = '5505127'; -- CSDL I
SELECT @Subj3 = Id FROM MonHoc WHERE MaMonHoc = '5505181'; -- Mạng
SELECT @Subj4 = Id FROM MonHoc WHERE MaMonHoc = '5505168'; -- OOP
SELECT @Subj5 = Id FROM MonHoc WHERE MaMonHoc = '5505226'; -- AI
SELECT @Subj6 = Id FROM MonHoc WHERE MaMonHoc = '5505175'; -- Web
SELECT @Subj7 = Id FROM MonHoc WHERE MaMonHoc = '5505119'; -- Toán rời rạc
SELECT @Subj8 = Id FROM MonHoc WHERE MaMonHoc = '5505148'; -- Cấu trúc dữ liệu

-- Get Tag IDs
DECLARE @Tag1 UNIQUEIDENTIFIER, @Tag2 UNIQUEIDENTIFIER, @Tag3 UNIQUEIDENTIFIER, @Tag4 UNIQUEIDENTIFIER, @Tag5 UNIQUEIDENTIFIER;
SELECT @Tag1 = Id FROM [The] WHERE TenTag = 'C#';
SELECT @Tag2 = Id FROM [The] WHERE TenTag = 'Java';
SELECT @Tag3 = Id FROM [The] WHERE TenTag = 'Python';
SELECT @Tag4 = Id FROM [The] WHERE TenTag = 'Machine Learning';
SELECT @Tag5 = Id FROM [The] WHERE TenTag = N'Cấu trúc dữ liệu';

-- Password hash for all users (password: Test@123)
DECLARE @Pwd NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEOCNOlUCTLnDzZABbb24M1L1V+5vKHk8xwSSxUiFN1XMHSO8hY/Ig2a4VO5q6kMCZA==';

-- =====================================================
-- INSERT 50 USERS
-- =====================================================

-- User table for easy reference
DECLARE @Users TABLE (Idx INT, UserId UNIQUEIDENTIFIER, MajorId UNIQUEIDENTIFIER, TrustScore INT);

-- Tạo 50 users với tên Việt Nam thực tế
DECLARE @FirstNames NVARCHAR(MAX) = N'Minh,Hùng,Tuấn,Dũng,Hải,Long,Phúc,Thắng,Quang,Đức,Hoàng,Nam,Bình,Khoa,Tài,Hiếu,Trung,Kiên,Đạt,Vinh,Thành,Tín,Phong,Khang,Thiện,Nhật,Khánh,Tùng,Lộc,Huy,Linh,Hà,Mai,Hương,Thảo,Ngọc,Trang,Lan,Yến,Hồng,Anh,Thủy,Phương,Vy,Chi,Tâm,Uyên,Diễm,Như,Trinh';
DECLARE @LastNames NVARCHAR(MAX) = N'Nguyễn,Trần,Lê,Phạm,Hoàng,Huỳnh,Phan,Vũ,Võ,Đặng,Bùi,Đỗ,Hồ,Ngô,Dương,Lý,Đinh,Trương,Cao,Tạ';
DECLARE @MiddleNames NVARCHAR(MAX) = N'Văn,Thị,Hoàng,Minh,Đức,Thanh,Quốc,Ngọc,Bảo,Gia';

DECLARE @i INT = 1;
WHILE @i <= 50
BEGIN
    DECLARE @UserId UNIQUEIDENTIFIER = NEWID();
    DECLARE @Major UNIQUEIDENTIFIER = CASE (@i % 6) WHEN 0 THEN @CNTT WHEN 1 THEN @CK WHEN 2 THEN @DDT WHEN 3 THEN @XD WHEN 4 THEN @HH ELSE @SPCN END;
    DECLARE @Gender INT = @i % 3;
    DECLARE @TrustScore INT = (@i * 7) % 100;
    DECLARE @TrustLevel INT = CASE WHEN @TrustScore < 10 THEN 1 WHEN @TrustScore < 30 THEN 2 WHEN @TrustScore < 60 THEN 3 ELSE 4 END;
    
    -- Tạo MSSV format: 21110xxx (năm 2021, ngành 110, số thứ tự)
    DECLARE @StudentId NVARCHAR(20) = CONCAT('21110', FORMAT(@i, '000'));
    DECLARE @Username NVARCHAR(50) = @StudentId;
    DECLARE @Email NVARCHAR(100) = CONCAT(@StudentId, '@sv.ute.udn.vn');
    
    -- Tạo tên Việt Nam: Họ + Tên đệm + Tên
    DECLARE @LastName NVARCHAR(50) = (SELECT value FROM STRING_SPLIT(@LastNames, ',') ORDER BY NEWID() OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY);
    DECLARE @MiddleName NVARCHAR(50) = (SELECT value FROM STRING_SPLIT(@MiddleNames, ',') ORDER BY NEWID() OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY);
    DECLARE @FirstName NVARCHAR(50) = (SELECT value FROM STRING_SPLIT(@FirstNames, ',') ORDER BY NEWID() OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY);
    DECLARE @FullName NVARCHAR(100) = CONCAT(@LastName, N' ', @MiddleName, N' ', @FirstName);
    
    DECLARE @Avatar NVARCHAR(200) = CONCAT('https://i.pravatar.cc/150?img=', (@i % 70) + 1);
    
    INSERT INTO NguoiDung (Id, TenDangNhap, TenDangNhapChuanHoa, Email, EmailChuanHoa, CoXacThucEmail, MatKhauBam, MaBaoMat, DauKiemSoatDongBo, SoDienThoai, CoDaXacNhanSoDienThoai, CoKichHoatXacThucHaiLop, NgayHetKhoaTaiKhoan, ChoPhepKhoaTaiKhoan, SoLanDangNhapThatBai, MaNganh, GioiThieu, HinhDaiDien, HoVaTen, DiemXacThuc, CoGoiY, CapDoXacThuc, GioiTinh, NgayTao, CoDaXoa)
    VALUES (@UserId, @Username, UPPER(@Username), @Email, UPPER(@Email), 1, @Pwd, NEWID(), NEWID(), NULL, 0, 0, NULL, 1, 0, @Major, CONCAT(N'Sinh viên năm ', (@i % 4) + 1, N' - ', @StudentId), @Avatar, @FullName, @TrustScore, 1, @TrustLevel, @Gender, DATEADD(DAY, -@i, GETUTCDATE()), 0);
    
    INSERT INTO NguoiDung_VaiTro (NguoiDungId, VaiTroId) VALUES (@UserId, @StudentRoleId);
    INSERT INTO @Users (Idx, UserId, MajorId, TrustScore) VALUES (@i, @UserId, @Major, @TrustScore);
    
    SET @i = @i + 1;
END

PRINT N'Đã thêm 50 users!';

-- Get user references
DECLARE @U1 UNIQUEIDENTIFIER, @U2 UNIQUEIDENTIFIER, @U3 UNIQUEIDENTIFIER, @U4 UNIQUEIDENTIFIER, @U5 UNIQUEIDENTIFIER;
DECLARE @U6 UNIQUEIDENTIFIER, @U7 UNIQUEIDENTIFIER, @U8 UNIQUEIDENTIFIER, @U9 UNIQUEIDENTIFIER, @U10 UNIQUEIDENTIFIER;
SELECT @U1 = UserId FROM @Users WHERE Idx = 1;
SELECT @U2 = UserId FROM @Users WHERE Idx = 2;
SELECT @U3 = UserId FROM @Users WHERE Idx = 3;
SELECT @U4 = UserId FROM @Users WHERE Idx = 4;
SELECT @U5 = UserId FROM @Users WHERE Idx = 5;
SELECT @U6 = UserId FROM @Users WHERE Idx = 6;
SELECT @U7 = UserId FROM @Users WHERE Idx = 7;
SELECT @U8 = UserId FROM @Users WHERE Idx = 8;
SELECT @U9 = UserId FROM @Users WHERE Idx = 9;
SELECT @U10 = UserId FROM @Users WHERE Idx = 10;

-- =====================================================
-- INSERT AUTHORS
-- =====================================================
DECLARE @Auth1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Auth2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Auth3 UNIQUEIDENTIFIER = NEWID();
DECLARE @Auth4 UNIQUEIDENTIFIER = NEWID();
DECLARE @Auth5 UNIQUEIDENTIFIER = NEWID();

INSERT INTO TacGia (Id, TenTacGia, MoTa, TrangThai, NgayTao, TaoBoi, CoDaXoa) VALUES 
(@Auth1, N'Nguyễn Văn Minh', N'Giảng viên Khoa CNTT', 0, GETUTCDATE(), @AdminId, 0),
(@Auth2, N'Trần Thị Hương', N'Giảng viên Khoa CNTT', 0, GETUTCDATE(), @AdminId, 0),
(@Auth3, N'Lê Văn Đức', N'Giảng viên Khoa Điện tử', 0, GETUTCDATE(), @AdminId, 0),
(@Auth4, N'Phạm Thị Lan', N'Giảng viên Khoa Xây dựng', 0, GETUTCDATE(), @AdminId, 0),
(@Auth5, N'Hoàng Văn Nam', N'Giảng viên Khoa CNTT', 0, GETUTCDATE(), @AdminId, 0);

PRINT N'Đã thêm 5 tác giả!';

-- =====================================================
-- INSERT 10 DOCUMENTS
-- =====================================================
DECLARE @Doc1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Doc2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Doc3 UNIQUEIDENTIFIER = NEWID();
DECLARE @Doc4 UNIQUEIDENTIFIER = NEWID();
DECLARE @Doc5 UNIQUEIDENTIFIER = NEWID();
DECLARE @Doc6 UNIQUEIDENTIFIER = NEWID();
DECLARE @Doc7 UNIQUEIDENTIFIER = NEWID();
DECLARE @Doc8 UNIQUEIDENTIFIER = NEWID();
DECLARE @Doc9 UNIQUEIDENTIFIER = NEWID();
DECLARE @Doc10 UNIQUEIDENTIFIER = NEWID();

-- Cover images (Unsplash - real URLs)
DECLARE @Cover1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Cover2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Cover3 UNIQUEIDENTIFIER = NEWID();
DECLARE @Cover4 UNIQUEIDENTIFIER = NEWID();
DECLARE @Cover5 UNIQUEIDENTIFIER = NEWID();
DECLARE @Cover6 UNIQUEIDENTIFIER = NEWID();
DECLARE @Cover7 UNIQUEIDENTIFIER = NEWID();
DECLARE @Cover8 UNIQUEIDENTIFIER = NEWID();
DECLARE @Cover9 UNIQUEIDENTIFIER = NEWID();
DECLARE @Cover10 UNIQUEIDENTIFIER = NEWID();

INSERT INTO TepDinhKem (Id, TenTep, KichThuoc, LoaiFile, LinkTruyCap, TaoBoi, NgayTao, CoDaXoa) VALUES 
(@Cover1, 'cover_c_programming.jpg', 102400, 'image/jpeg', 'https://images.unsplash.com/photo-1516116216624-53e697fedbea?w=400&h=300&fit=crop', @U1, DATEADD(DAY, -30, GETUTCDATE()), 0),
(@Cover2, 'cover_database.jpg', 102400, 'image/jpeg', 'https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=400&h=300&fit=crop', @U2, DATEADD(DAY, -28, GETUTCDATE()), 0),
(@Cover3, 'cover_network.jpg', 102400, 'image/jpeg', 'https://images.unsplash.com/photo-1544197150-b99a580bb7a8?w=400&h=300&fit=crop', @U3, DATEADD(DAY, -25, GETUTCDATE()), 0),
(@Cover4, 'cover_java.jpg', 102400, 'image/jpeg', 'https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=400&h=300&fit=crop', @U4, DATEADD(DAY, -22, GETUTCDATE()), 0),
(@Cover5, 'cover_ai.jpg', 102400, 'image/jpeg', 'https://images.unsplash.com/photo-1677442136019-21780ecad995?w=400&h=300&fit=crop', @U5, DATEADD(DAY, -20, GETUTCDATE()), 0),
(@Cover6, 'cover_web.jpg', 102400, 'image/jpeg', 'https://images.unsplash.com/photo-1547658719-da2b51169166?w=400&h=300&fit=crop', @U6, DATEADD(DAY, -18, GETUTCDATE()), 0),
(@Cover7, 'cover_math.jpg', 102400, 'image/jpeg', 'https://images.unsplash.com/photo-1509228468518-180dd4864904?w=400&h=300&fit=crop', @U7, DATEADD(DAY, -15, GETUTCDATE()), 0),
(@Cover8, 'cover_dsa.jpg', 102400, 'image/jpeg', 'https://images.unsplash.com/photo-1515879218367-8466d910aaa4?w=400&h=300&fit=crop', @U8, DATEADD(DAY, -12, GETUTCDATE()), 0),
(@Cover9, 'cover_project.jpg', 102400, 'image/jpeg', 'https://images.unsplash.com/photo-1461749280684-dccba630e2f6?w=400&h=300&fit=crop', @U9, DATEADD(DAY, -10, GETUTCDATE()), 0),
(@Cover10, 'cover_dl.jpg', 102400, 'image/jpeg', 'https://images.unsplash.com/photo-1620712943543-bcc4688e7485?w=400&h=300&fit=crop', @U10, DATEADD(DAY, -8, GETUTCDATE()), 0);

INSERT INTO TaiLieu (Id, MonHocId, LoaiTaiLieuId, MoTa, TenTaiLieu, TenChuanHoa, CoHienThi, TepBiaId, TaoBoi, NgayTao, CoDaXoa) VALUES 
(@Doc1, @Subj1, @Type1, N'Hướng dẫn lập trình C từ cơ bản đến nâng cao', N'Giáo trình Lập trình C', N'GIAO TRINH LAP TRINH C', 0, @Cover1, @U1, DATEADD(DAY, -30, GETUTCDATE()), 0),
(@Doc2, @Subj2, @Type1, N'Bài tập SQL và thiết kế CSDL đầy đủ', N'Bài tập CSDL có lời giải', N'BAI TAP CSDL CO LOI GIAI', 0, @Cover2, @U2, DATEADD(DAY, -28, GETUTCDATE()), 0),
(@Doc3, @Subj3, @Type2, N'Slide bài giảng mạng máy tính học kỳ 1', N'Slide Mạng máy tính', N'SLIDE MANG MAY TINH', 1, @Cover3, @U3, DATEADD(DAY, -25, GETUTCDATE()), 0),
(@Doc4, @Subj4, @Type1, N'Giáo trình OOP với Java đầy đủ', N'Lập trình hướng đối tượng Java', N'LAP TRINH HUONG DOI TUONG JAVA', 0, @Cover4, @U4, DATEADD(DAY, -22, GETUTCDATE()), 0),
(@Doc5, @Subj5, @Type3, N'Đồ án Machine Learning với Python', N'Đồ án AI - Face Recognition', N'DO AN AI FACE RECOGNITION', 1, @Cover5, @U5, DATEADD(DAY, -20, GETUTCDATE()), 0),
(@Doc6, @Subj6, @Type2, N'Slide ReactJS và NodeJS cơ bản', N'Slide Lập trình Web', N'SLIDE LAP TRINH WEB', 1, @Cover6, @U6, DATEADD(DAY, -18, GETUTCDATE()), 0),
(@Doc7, @Subj7, @Type1, N'Giáo trình toán rời rạc ứng dụng', N'Toán rời rạc cho CNTT', N'TOAN ROI RAC CHO CNTT', 0, @Cover7, @U7, DATEADD(DAY, -15, GETUTCDATE()), 0),
(@Doc8, @Subj8, @Type1, N'Cấu trúc dữ liệu và giải thuật với C++', N'Cấu trúc dữ liệu và Giải thuật', N'CAU TRUC DU LIEU VA GIAI THUAT', 0, @Cover8, @U8, DATEADD(DAY, -12, GETUTCDATE()), 0),
(@Doc9, @Subj1, @Type3, N'Đồ án quản lý sinh viên bằng C', N'Đồ án Lập trình C', N'DO AN LAP TRINH C', 1, @Cover9, @U9, DATEADD(DAY, -10, GETUTCDATE()), 0),
(@Doc10, @Subj5, @Type2, N'Slide Deep Learning với TensorFlow', N'Deep Learning cơ bản', N'DEEP LEARNING CO BAN', 1, @Cover10, @U10, DATEADD(DAY, -8, GETUTCDATE()), 0);

PRINT N'Đã thêm 10 cover images và 10 documents!';

-- =====================================================
-- INSERT DOCUMENT TAGS
-- =====================================================
INSERT INTO [TaiLieu_The] (TaiLieuId, TheId) VALUES 
(@Doc1, @Tag1), (@Doc1, @Tag5),
(@Doc2, @Tag1),
(@Doc3, @Tag3),
(@Doc4, @Tag2),
(@Doc5, @Tag3), (@Doc5, @Tag4),
(@Doc6, @Tag3),
(@Doc7, @Tag5),
(@Doc8, @Tag1), (@Doc8, @Tag5),
(@Doc9, @Tag1),
(@Doc10, @Tag3), (@Doc10, @Tag4);

-- =====================================================
-- INSERT DOCUMENT AUTHORS
-- =====================================================
INSERT INTO [TaiLieu_TacGia] (TaiLieuId, TacGiaId) VALUES 
(@Doc1, @Auth1),
(@Doc2, @Auth2),
(@Doc3, @Auth3),
(@Doc4, @Auth1), (@Doc4, @Auth4),
(@Doc5, @Auth5),
(@Doc6, @Auth2),
(@Doc7, @Auth3),
(@Doc8, @Auth1),
(@Doc9, @Auth4),
(@Doc10, @Auth5);

PRINT N'Đã thêm document tags và authors!';

-- =====================================================
-- INSERT FILES (Real external PDF URLs)
-- =====================================================
DECLARE @File1 UNIQUEIDENTIFIER = NEWID();
DECLARE @File2 UNIQUEIDENTIFIER = NEWID();
DECLARE @File3 UNIQUEIDENTIFIER = NEWID();
DECLARE @File4 UNIQUEIDENTIFIER = NEWID();
DECLARE @File5 UNIQUEIDENTIFIER = NEWID();
DECLARE @File6 UNIQUEIDENTIFIER = NEWID();
DECLARE @File7 UNIQUEIDENTIFIER = NEWID();
DECLARE @File8 UNIQUEIDENTIFIER = NEWID();
DECLARE @File9 UNIQUEIDENTIFIER = NEWID();
DECLARE @File10 UNIQUEIDENTIFIER = NEWID();

-- Using real external PDF URLs for testing
INSERT INTO TepDinhKem (Id, TenTep, KichThuoc, LoaiFile, LinkTruyCap, TaoBoi, NgayTao, CoDaXoa) VALUES 
(@File1, 'tu_hoc_python.pdf', 2097152, 'application/pdf', 'https://pyqt5.wordpress.com/wp-content/uploads/2017/07/tu-hoc-python.pdf', @U1, DATEADD(DAY, -30, GETUTCDATE()), 0),
(@File2, 'big_book_python_projects.pdf', 5242880, 'application/pdf', 'https://edu.anarcho-copy.org/Programming%20Languages/Python/BigBookSmallPythonProjects.pdf', @U2, DATEADD(DAY, -28, GETUTCDATE()), 0),
(@File3, 'tu_hoc_python_2.pdf', 2097152, 'application/pdf', 'https://pyqt5.wordpress.com/wp-content/uploads/2017/07/tu-hoc-python.pdf', @U3, DATEADD(DAY, -25, GETUTCDATE()), 0),
(@File4, 'python_projects.pdf', 5242880, 'application/pdf', 'https://edu.anarcho-copy.org/Programming%20Languages/Python/BigBookSmallPythonProjects.pdf', @U4, DATEADD(DAY, -22, GETUTCDATE()), 0),
(@File5, 'python_ml.pdf', 2097152, 'application/pdf', 'https://pyqt5.wordpress.com/wp-content/uploads/2017/07/tu-hoc-python.pdf', @U5, DATEADD(DAY, -20, GETUTCDATE()), 0),
(@File6, 'python_web.pdf', 5242880, 'application/pdf', 'https://edu.anarcho-copy.org/Programming%20Languages/Python/BigBookSmallPythonProjects.pdf', @U6, DATEADD(DAY, -18, GETUTCDATE()), 0),
(@File7, 'python_math.pdf', 2097152, 'application/pdf', 'https://pyqt5.wordpress.com/wp-content/uploads/2017/07/tu-hoc-python.pdf', @U7, DATEADD(DAY, -15, GETUTCDATE()), 0),
(@File8, 'python_dsa.pdf', 5242880, 'application/pdf', 'https://edu.anarcho-copy.org/Programming%20Languages/Python/BigBookSmallPythonProjects.pdf', @U8, DATEADD(DAY, -12, GETUTCDATE()), 0),
(@File9, 'python_project.pdf', 2097152, 'application/pdf', 'https://pyqt5.wordpress.com/wp-content/uploads/2017/07/tu-hoc-python.pdf', @U9, DATEADD(DAY, -10, GETUTCDATE()), 0),
(@File10, 'python_dl.pdf', 5242880, 'application/pdf', 'https://edu.anarcho-copy.org/Programming%20Languages/Python/BigBookSmallPythonProjects.pdf', @U10, DATEADD(DAY, -8, GETUTCDATE()), 0);

PRINT N'Đã thêm 10 files!';

-- =====================================================
-- INSERT DOCUMENT FILES (Chapters) - Status 1 = Approved
-- =====================================================
DECLARE @DocFile1 UNIQUEIDENTIFIER = NEWID();
DECLARE @DocFile2 UNIQUEIDENTIFIER = NEWID();
DECLARE @DocFile3 UNIQUEIDENTIFIER = NEWID();
DECLARE @DocFile4 UNIQUEIDENTIFIER = NEWID();
DECLARE @DocFile5 UNIQUEIDENTIFIER = NEWID();
DECLARE @DocFile6 UNIQUEIDENTIFIER = NEWID();
DECLARE @DocFile7 UNIQUEIDENTIFIER = NEWID();
DECLARE @DocFile8 UNIQUEIDENTIFIER = NEWID();
DECLARE @DocFile9 UNIQUEIDENTIFIER = NEWID();
DECLARE @DocFile10 UNIQUEIDENTIFIER = NEWID();

INSERT INTO Chuong (Id, TaiLieuId, TepDinhKemId, TieuDe, TongSoTrang, ThuTu, TrangThai, TaoBoi, NgayTao, CoDaXoa) VALUES 
(@DocFile1, @Doc1, @File1, N'Chương 1 - Nhập môn C', 25, 1, 1, @U1, DATEADD(DAY, -30, GETUTCDATE()), 0),
(@DocFile2, @Doc2, @File2, N'Bài tập SQL cơ bản', 45, 1, 1, @U2, DATEADD(DAY, -28, GETUTCDATE()), 0),
(@DocFile3, @Doc3, @File3, N'Slide tổng quan Mạng', 80, 1, 1, @U3, DATEADD(DAY, -25, GETUTCDATE()), 0),
(@DocFile4, @Doc4, @File4, N'Phần 1 - Class và Object', 35, 1, 1, @U4, DATEADD(DAY, -22, GETUTCDATE()), 0),
(@DocFile5, @Doc5, @File5, N'Đồ án Face Recognition', 120, 1, 1, @U5, DATEADD(DAY, -20, GETUTCDATE()), 0),
(@DocFile6, @Doc6, @File6, N'Slide ReactJS cơ bản', 60, 1, 1, @U6, DATEADD(DAY, -18, GETUTCDATE()), 0),
(@DocFile7, @Doc7, @File7, N'Chương 1 - Logic mệnh đề', 40, 1, 1, @U7, DATEADD(DAY, -15, GETUTCDATE()), 0),
(@DocFile8, @Doc8, @File8, N'Phần 1 - Mảng và Danh sách', 55, 1, 1, @U8, DATEADD(DAY, -12, GETUTCDATE()), 0),
(@DocFile9, @Doc9, @File9, N'Báo cáo đồ án', 30, 1, 1, @U9, DATEADD(DAY, -10, GETUTCDATE()), 0),
(@DocFile10, @Doc10, @File10, N'Slide Neural Networks', 90, 1, 1, @U10, DATEADD(DAY, -8, GETUTCDATE()), 0);

PRINT N'Đã thêm 10 document files!';

-- =====================================================
-- INSERT 8 CONVERSATIONS (Active groups)
-- =====================================================
DECLARE @Conv1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Conv2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Conv3 UNIQUEIDENTIFIER = NEWID();
DECLARE @Conv4 UNIQUEIDENTIFIER = NEWID();
DECLARE @Conv5 UNIQUEIDENTIFIER = NEWID();
DECLARE @Conv6 UNIQUEIDENTIFIER = NEWID();
DECLARE @Conv7 UNIQUEIDENTIFIER = NEWID();
DECLARE @Conv8 UNIQUEIDENTIFIER = NEWID();

INSERT INTO CuocTroChuyen (Id, MonHocId, TinNhanMoiNhat, TenCuocTroChuyen, AnhDaiDien, CoDuocTaoBoiAI, CoChoThanhVienGhimTinNhan, LoaiCuocTroChuyen, CheDo, TrangThai, TaoBoi, NgayTao, CoDaXoa) VALUES 
(@Conv1, @Subj1, NULL, N'Nhóm học Lập trình C', 'https://i.pravatar.cc/150?img=30', 0, 1, 1, 1, 0, @U1, DATEADD(DAY, -25, GETUTCDATE()), 0),
(@Conv2, @Subj2, NULL, N'CSDL - Lớp 21ĐHCNTT01', 'https://i.pravatar.cc/150?img=31', 0, 1, 1, 1, 0, @U2, DATEADD(DAY, -23, GETUTCDATE()), 0),
(@Conv3, @Subj3, NULL, N'Mạng máy tính - HK1 2024', 'https://i.pravatar.cc/150?img=32', 0, 0, 1, 1, 0, @U3, DATEADD(DAY, -20, GETUTCDATE()), 0),
(@Conv4, @Subj5, NULL, N'AI/ML Research Group', 'https://i.pravatar.cc/150?img=33', 1, 1, 1, 1, 0, @U5, DATEADD(DAY, -18, GETUTCDATE()), 0),
(@Conv5, @Subj4, NULL, N'OOP Java Study', 'https://i.pravatar.cc/150?img=34', 0, 1, 1, 0, 0, @U4, DATEADD(DAY, -15, GETUTCDATE()), 0),
(@Conv6, @Subj6, NULL, N'Web Development Club', 'https://i.pravatar.cc/150?img=35', 1, 1, 1, 1, 0, @U6, DATEADD(DAY, -12, GETUTCDATE()), 0),
(@Conv7, @Subj8, NULL, N'DSA Practice Group', 'https://i.pravatar.cc/150?img=36', 0, 1, 1, 1, 0, @U8, DATEADD(DAY, -10, GETUTCDATE()), 0),
(@Conv8, @Subj7, NULL, N'Toán rời rạc K21', 'https://i.pravatar.cc/150?img=37', 0, 0, 1, 0, 0, @U7, DATEADD(DAY, -8, GETUTCDATE()), 0);

PRINT N'Đã thêm 8 conversations!';

-- =====================================================
-- INSERT CONVERSATION MEMBERS (Multiple members per group)
-- QuyenNhom: 0=Owner, 1=Admin, 2=Member | TrangThaiLoiMoi: 3=Joined
-- =====================================================
-- Conv1: 5 members
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, TrangThaiLoiMoi, NgayTao, CoDaXoa) VALUES
(NEWID(), @U1, @Conv1, NULL, 0, 0, 3, DATEADD(DAY, -25, GETUTCDATE()), 0),
(NEWID(), @U2, @Conv1, NULL, 0, 2, 3, DATEADD(DAY, -24, GETUTCDATE()), 0),
(NEWID(), @U3, @Conv1, NULL, 0, 2, 3, DATEADD(DAY, -23, GETUTCDATE()), 0),
(NEWID(), @U4, @Conv1, NULL, 0, 2, 3, DATEADD(DAY, -22, GETUTCDATE()), 0),
(NEWID(), @U5, @Conv1, NULL, 0, 2, 3, DATEADD(DAY, -21, GETUTCDATE()), 0);

-- Conv2: 4 members
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, TrangThaiLoiMoi, NgayTao, CoDaXoa) VALUES
(NEWID(), @U2, @Conv2, NULL, 0, 0, 3, DATEADD(DAY, -23, GETUTCDATE()), 0),
(NEWID(), @U1, @Conv2, NULL, 0, 2, 3, DATEADD(DAY, -22, GETUTCDATE()), 0),
(NEWID(), @U6, @Conv2, NULL, 0, 2, 3, DATEADD(DAY, -21, GETUTCDATE()), 0),
(NEWID(), @U7, @Conv2, NULL, 0, 2, 3, DATEADD(DAY, -20, GETUTCDATE()), 0);

-- Conv3: 3 members
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, TrangThaiLoiMoi, NgayTao, CoDaXoa) VALUES
(NEWID(), @U3, @Conv3, NULL, 0, 0, 3, DATEADD(DAY, -20, GETUTCDATE()), 0),
(NEWID(), @U8, @Conv3, NULL, 0, 2, 3, DATEADD(DAY, -19, GETUTCDATE()), 0),
(NEWID(), @U9, @Conv3, NULL, 0, 2, 3, DATEADD(DAY, -18, GETUTCDATE()), 0);

-- Conv4: AI Group - 6 members
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, TrangThaiLoiMoi, NgayTao, CoDaXoa) VALUES
(NEWID(), @U5, @Conv4, NULL, 0, 0, 3, DATEADD(DAY, -18, GETUTCDATE()), 0),
(NEWID(), @U1, @Conv4, NULL, 0, 1, 3, DATEADD(DAY, -17, GETUTCDATE()), 0),
(NEWID(), @U2, @Conv4, NULL, 0, 2, 3, DATEADD(DAY, -16, GETUTCDATE()), 0),
(NEWID(), @U10, @Conv4, NULL, 0, 2, 3, DATEADD(DAY, -15, GETUTCDATE()), 0);

-- Conv5-8: 3 members each
INSERT INTO [CuocTroChuyen_ThanhVien] (Id, NguoiDungId, CuocTroChuyenId, TinNhanDocGanNhat, CoBiChanChat, QuyenNhom, TrangThaiLoiMoi, NgayTao, CoDaXoa) VALUES
(NEWID(), @U4, @Conv5, NULL, 0, 0, 3, DATEADD(DAY, -15, GETUTCDATE()), 0),
(NEWID(), @U9, @Conv5, NULL, 0, 2, 3, DATEADD(DAY, -14, GETUTCDATE()), 0),
(NEWID(), @U6, @Conv6, NULL, 0, 0, 3, DATEADD(DAY, -12, GETUTCDATE()), 0),
(NEWID(), @U7, @Conv6, NULL, 0, 2, 3, DATEADD(DAY, -11, GETUTCDATE()), 0),
(NEWID(), @U8, @Conv7, NULL, 0, 0, 3, DATEADD(DAY, -10, GETUTCDATE()), 0),
(NEWID(), @U3, @Conv7, NULL, 0, 2, 3, DATEADD(DAY, -9, GETUTCDATE()), 0),
(NEWID(), @U7, @Conv8, NULL, 0, 0, 3, DATEADD(DAY, -8, GETUTCDATE()), 0),
(NEWID(), @U4, @Conv8, NULL, 0, 2, 3, DATEADD(DAY, -7, GETUTCDATE()), 0);

PRINT N'Đã thêm conversation members!';

-- =====================================================
-- INSERT CONVERSATION TAGS
-- =====================================================
INSERT INTO [CuocTroChuyen_The] (CuocTroChuyenId, TheId) VALUES 
(@Conv1, @Tag1), (@Conv1, @Tag5),
(@Conv2, @Tag1),
(@Conv3, @Tag3),
(@Conv4, @Tag3), (@Conv4, @Tag4),
(@Conv5, @Tag2),
(@Conv6, @Tag3),
(@Conv7, @Tag1), (@Conv7, @Tag5),
(@Conv8, @Tag5);

PRINT N'Đã thêm conversation tags!';

-- =====================================================
-- INSERT MESSAGES (Sample messages)
-- =====================================================
INSERT INTO TinNhan (Id, CuocTroChuyenId, PhanTuChaId, NoiDung, CoChinhSua, CoDaGhim, LoaiTinNhan, TaoBoi, NgayTao, CoDaXoa) VALUES 
(NEWID(), @Conv1, NULL, N'Chào mọi người! Nhóm học Lập trình C!', 0, 0, 0, @U1, DATEADD(DAY, -25, GETUTCDATE()), 0),
(NEWID(), @Conv1, NULL, N'Xin chào! Em mới join nhóm ạ', 0, 0, NULL, @U2, DATEADD(DAY, -24, GETUTCDATE()), 0),
(NEWID(), @Conv1, NULL, N'Có ai có tài liệu về pointer không ạ?', 0, 0, NULL, @U3, DATEADD(DAY, -23, GETUTCDATE()), 0),
(NEWID(), @Conv2, NULL, N'Welcome to CSDL group!', 0, 0, 0, @U2, DATEADD(DAY, -23, GETUTCDATE()), 0),
(NEWID(), @Conv2, NULL, N'Bài tập về normalization khó quá 😭', 0, 0, NULL, @U1, DATEADD(DAY, -21, GETUTCDATE()), 0),
(NEWID(), @Conv3, NULL, N'Nhóm Mạng máy tính - Chào mọi người!', 0, 0, 0, @U3, DATEADD(DAY, -20, GETUTCDATE()), 0),
(NEWID(), @Conv4, NULL, N'AI/ML Research Group - Share papers hàng tuần!', 0, 0, 0, @U5, DATEADD(DAY, -18, GETUTCDATE()), 0),
(NEWID(), @Conv4, NULL, N'Paper hay: Attention Is All You Need', 0, 1, NULL, @U5, DATEADD(DAY, -16, GETUTCDATE()), 0),
(NEWID(), @Conv5, NULL, N'OOP Study Group created!', 0, 0, 0, @U4, DATEADD(DAY, -15, GETUTCDATE()), 0),
(NEWID(), @Conv6, NULL, N'Web Dev Club - ReactJS, NodeJS, Next.js!', 0, 0, 0, @U6, DATEADD(DAY, -12, GETUTCDATE()), 0);

PRINT N'Đã thêm 10 messages!';

-- =====================================================
-- INSERT DOCUMENT REVIEWS (Some users review documents)
-- LoaiDanhGia: 0=NotUseful, 1=Useful
-- =====================================================
INSERT INTO DanhGiaTaiLieu (Id, TaiLieuId, ChuongId, LoaiDanhGia, TaoBoi, NgayTao) VALUES
(NEWID(), @Doc1, @DocFile1, 1, @U2, DATEADD(DAY, -28, GETUTCDATE())),
(NEWID(), @Doc1, @DocFile1, 1, @U3, DATEADD(DAY, -27, GETUTCDATE())),
(NEWID(), @Doc1, @DocFile1, 1, @U4, DATEADD(DAY, -26, GETUTCDATE())),
(NEWID(), @Doc2, @DocFile2, 1, @U1, DATEADD(DAY, -26, GETUTCDATE())),
(NEWID(), @Doc2, @DocFile2, 1, @U5, DATEADD(DAY, -25, GETUTCDATE())),
(NEWID(), @Doc3, @DocFile3, 1, @U4, DATEADD(DAY, -23, GETUTCDATE())),
(NEWID(), @Doc4, @DocFile4, 1, @U1, DATEADD(DAY, -20, GETUTCDATE())),
(NEWID(), @Doc4, @DocFile4, 1, @U3, DATEADD(DAY, -19, GETUTCDATE())),
(NEWID(), @Doc5, @DocFile5, 1, @U1, DATEADD(DAY, -18, GETUTCDATE())),
(NEWID(), @Doc5, @DocFile5, 1, @U6, DATEADD(DAY, -17, GETUTCDATE()));

PRINT N'Đã thêm 10 document reviews!';

-- =====================================================
-- SUMMARY
-- =====================================================
PRINT N'';
PRINT N'===========================================';
PRINT N'KẾT QUẢ SEED DATA:';
PRINT N'- 50 Users';
PRINT N'- 5 Authors';
PRINT N'- 10 Documents với tags và authors';
PRINT N'- 10 Files (external PDF URLs)';
PRINT N'- 10 Document Files (chapters)';
PRINT N'- 8 Conversations';
PRINT N'- 28 Conversation Members';
PRINT N'- 11 Conversation Tags';
PRINT N'- 10 Messages';
PRINT N'- 10 Document Reviews';
PRINT N'===========================================';
GO
