-- Kịch bản cài đặt Cơ sở dữ liệu cho Đồ án Chuyên đề ASP.NET
-- Sinh viên: Hoàng Phong Phúc - MSSV: 170124447
-- Lớp: DK24TT8017

USE master;
GO

-- 1. Tạo cơ sở dữ liệu nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'do an')
BEGIN
    CREATE DATABASE [do an];
END
GO

USE [do an];
GO

-- 2. Tạo bảng tài khoản người dùng DoAn (Bảng có sẵn)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DoAn]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DoAn] (
        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Username] NVARCHAR(250) NOT NULL UNIQUE,
        [Password] NVARCHAR(250) NOT NULL,
        [HoTen] NVARCHAR(250) NOT NULL,
        [Email] NVARCHAR(250) NOT NULL,
        [ChucVu] NVARCHAR(100) NOT NULL, -- SinhVien, GiangVien, Admin
        [GithubUrl] NVARCHAR(MAX) NULL,
        [Bio] NVARCHAR(MAX) NULL,
        [Avatar] NVARCHAR(MAX) NULL
    );
END
GO

-- 3. Khởi tạo tài khoản dữ liệu mẫu nếu chưa có
IF NOT EXISTS (SELECT * FROM [dbo].[DoAn] WHERE [Username] = 'phuc.hp@school.vn')
BEGIN
    INSERT INTO [dbo].[DoAn] ([Username], [Password], [HoTen], [Email], [ChucVu], [GithubUrl], [Bio])
    VALUES ('phuc.hp@school.vn', '123', N'Hoàng Phong Phúc', 'phuc.hp@school.vn', 'SinhVien', 'https://github.com/phuc-hoangphong', N'Sinh viên lớp DK24TT8017, chuyên ngành Công nghệ thông tin');
END

IF NOT EXISTS (SELECT * FROM [dbo].[DoAn] WHERE [Username] = 'mien.dp@school.vn')
BEGIN
    INSERT INTO [dbo].[DoAn] ([Username], [Password], [HoTen], [Email], [ChucVu])
    VALUES ('mien.dp@school.vn', '123', N'Đoàn Phước Miền', 'mien.dp@school.vn', 'GiangVien');
END

IF NOT EXISTS (SELECT * FROM [dbo].[DoAn] WHERE [Username] = 'admin')
BEGIN
    INSERT INTO [dbo].[DoAn] ([Username], [Password], [HoTen], [Email], [ChucVu])
    VALUES ('admin', 'admin', N'Quản trị viên', 'admin@diendantinhoc.vn', 'Admin');
END
GO

PRINT 'Cơ sở dữ liệu đã được cài đặt thành công!';
