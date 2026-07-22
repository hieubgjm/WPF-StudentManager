/*
    Script này KHÔNG bắt buộc phải chạy tay.

    Khi chương trình chạy lần đầu, App.xaml.cs sẽ tự gọi
    db.Database.EnsureCreated() để tự tạo database + các bảng
    dựa theo các class trong thư mục Models, và tự tạo sẵn 1 tài
    khoản admin/123456.

    File .sql này chỉ để tham khảo cấu trúc database sinh ra trông
    như thế nào (hữu ích khi cần xem lại bằng SQL Server Management
    Studio), hoặc dùng nếu bạn muốn tự tạo database bằng tay.

    LƯU Ý QUAN TRỌNG: EnsureCreated() chỉ tạo database khi nó CHƯA
    tồn tại. Nếu máy bạn đã từng chạy chương trình trước khi có các
    cột/bảng mới (SinhViens.TrangThai, Lops.KhoaHoc, MonHocs, DiemSos)
    thì EnsureCreated() sẽ KHÔNG tự thêm các cột/bảng này vào database
    cũ. Cách đơn giản nhất khi đang phát triển: xóa hẳn database
    QuanLySinhVienDB đi rồi chạy lại chương trình để nó tự tạo mới
    theo đúng cấu trúc hiện tại.
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
    KhoaHoc      NVARCHAR(MAX) NULL,
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
    TrangThai     NVARCHAR(MAX) NULL, -- "Đang học" / "Bảo lưu" / "Tốt nghiệp"
    LopId         INT NULL,
    CaHocId       INT NULL,
    CONSTRAINT UQ_SinhViens_MaSV UNIQUE (MaSV),
    CONSTRAINT FK_SinhViens_Lops FOREIGN KEY (LopId) REFERENCES Lops(LopId),
    CONSTRAINT FK_SinhViens_CaHocs FOREIGN KEY (CaHocId) REFERENCES CaHocs(CaHocId)
);
GO

CREATE TABLE MonHocs (
    MonHocId     INT IDENTITY(1,1) PRIMARY KEY,
    MaMonHoc     NVARCHAR(450) NOT NULL,
    TenMonHoc    NVARCHAR(MAX) NULL,
    SoTinChi     INT NOT NULL,
    GhiChu       NVARCHAR(MAX) NULL,
    CONSTRAINT UQ_MonHocs_MaMonHoc UNIQUE (MaMonHoc)
);
GO

CREATE TABLE DiemSos (
    DiemSoId     INT IDENTITY(1,1) PRIMARY KEY,
    SinhVienId   INT NOT NULL,
    MonHocId     INT NOT NULL,
    DiemGiuaKy   FLOAT NULL,
    DiemCuoiKy   FLOAT NULL,
    -- Điểm tổng kết KHÔNG lưu ở đây, chương trình tự tính lại từ 2 cột trên
    -- (xem DiemSo.DiemTongKet và Helpers/XepLoaiHelper.cs)
    CONSTRAINT UQ_DiemSos_SinhVien_MonHoc UNIQUE (SinhVienId, MonHocId),
    CONSTRAINT FK_DiemSos_SinhViens FOREIGN KEY (SinhVienId) REFERENCES SinhViens(SinhVienId) ON DELETE CASCADE,
    CONSTRAINT FK_DiemSos_MonHocs FOREIGN KEY (MonHocId) REFERENCES MonHocs(MonHocId)
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
