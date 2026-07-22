/*
    Script này KHÔNG bắt buộc phải chạy tay.

    Khi chương trình chạy lần đầu, App.xaml.cs sẽ tự gọi
    db.Database.EnsureCreated() để tự tạo database + các bảng
    dựa theo các class trong thư mục Models, và tự tạo sẵn 1 tài
    khoản admin/123456.

    File .sql này chỉ để tham khảo cấu trúc database sinh ra trông
    như thế nào (hữu ích khi cần xem lại bằng SQL Server Management
    Studio), hoặc dùng nếu bạn muốn tự tạo database bằng tay.
*/

CREATE DATABASE QuanLySinhVienDB;
GO

USE QuanLySinhVienDB;
GO

CREATE TABLE Lops (
    LopId        INT IDENTITY(1,1) PRIMARY KEY,
    MaLop        NVARCHAR(450) NOT NULL,
    TenLop       NVARCHAR(MAX) NULL,
    Khoa         NVARCHAR(MAX) NULL,
    SiSoToiDa    INT NOT NULL,
    GhiChu       NVARCHAR(MAX) NULL,
    CONSTRAINT UQ_Lops_MaLop UNIQUE (MaLop)
);
GO

CREATE TABLE CaHocs (
    CaHocId      INT IDENTITY(1,1) PRIMARY KEY,
    TenCa        NVARCHAR(MAX) NULL,
    GioBatDau    TIME NOT NULL,
    GioKetThuc   TIME NOT NULL,
    GhiChu       NVARCHAR(MAX) NULL
);
GO

CREATE TABLE SinhViens (
    SinhVienId    INT IDENTITY(1,1) PRIMARY KEY,
    MaSV          NVARCHAR(450) NOT NULL,
    HoTen         NVARCHAR(MAX) NULL,
    NgaySinh      DATETIME2 NOT NULL,
    GioiTinh      NVARCHAR(MAX) NULL,
    DiaChi        NVARCHAR(MAX) NULL,
    SoDienThoai   NVARCHAR(MAX) NULL,
    Email         NVARCHAR(MAX) NULL,
    LopId         INT NULL,
    CaHocId       INT NULL,
    CONSTRAINT UQ_SinhViens_MaSV UNIQUE (MaSV),
    CONSTRAINT FK_SinhViens_Lops FOREIGN KEY (LopId) REFERENCES Lops(LopId),
    CONSTRAINT FK_SinhViens_CaHocs FOREIGN KEY (CaHocId) REFERENCES CaHocs(CaHocId)
);
GO

CREATE TABLE TaiKhoans (
    TaiKhoanId     INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap    NVARCHAR(450) NOT NULL,
    MatKhauMaHoa   NVARCHAR(MAX) NULL,
    HoTen          NVARCHAR(MAX) NULL,
    VaiTro         NVARCHAR(MAX) NULL,
    DangHoatDong   BIT NOT NULL,
    CONSTRAINT UQ_TaiKhoans_TenDangNhap UNIQUE (TenDangNhap)
);
GO

-- Tài khoản admin mặc định, mật khẩu gốc là "123456" (đã băm SHA256 sẵn ở đây)
INSERT INTO TaiKhoans (TenDangNhap, MatKhauMaHoa, HoTen, VaiTro, DangHoatDong)
VALUES (N'admin', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', N'Quản trị viên', N'Admin', 1);
GO
