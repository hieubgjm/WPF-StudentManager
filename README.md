# Quản Lý Sinh Viên (WPF .NET 8 + Entity Framework Core)

Đồ án quản lý sinh viên: quản lý Sinh viên, Lớp học, Ca học, có đăng nhập
và quản lý tài khoản, thống kê sĩ số.

## Công nghệ sử dụng

- WPF, .NET 8 (`net8.0-windows`)
- Entity Framework Core 8 + SQL Server
- Không dùng MVVM framework ngoài, code-behind thuần cho dễ đọc, dễ theo dõi

## Cấu trúc thư mục

```
QuanLySinhVien/
├── QuanLySinhVien.sln
└── QuanLySinhVien/
    ├── Models/          -> Các class SinhVien, Lop, CaHoc, TaiKhoan
    ├── Data/            -> AppDbContext (kết nối EF Core tới SQL Server)
    ├── Repositories/     -> Tầng CRUD, thao tác với database
    ├── Helpers/          -> Băm mật khẩu, lưu thông tin phiên đăng nhập
    ├── Views/            -> Toàn bộ giao diện (Window, UserControl)
    └── Database/         -> Script SQL tham khảo cấu trúc database
```

## Chức năng

- Đăng nhập / Đăng xuất / Đổi mật khẩu
- Quản lý Sinh viên: thêm, sửa, xóa, tìm kiếm theo mã/họ tên, lọc theo lớp
- Quản lý Lớp học: thêm, sửa, xóa (chặn xóa nếu lớp còn sinh viên)
- Quản lý Ca học: thêm, sửa, xóa (chặn xóa nếu ca học còn sinh viên đang học)
- Quản lý tài khoản (chỉ Admin thấy được): thêm, sửa, khóa, xóa, đặt lại mật khẩu
- Thống kê: tổng số sinh viên/lớp/ca học, sĩ số theo từng lớp

## Cách chạy dự án

### 1. Cài đặt cần thiết

- Visual Studio 2022 (bản Community miễn phí là đủ), cài kèm workload
  ".NET desktop development"
- SQL Server: dùng **LocalDB** đi kèm sẵn khi cài Visual Studio là được,
  không cần cài SQL Server riêng.

### 2. Mở dự án

Mở file `QuanLySinhVien.sln` bằng Visual Studio. Visual Studio sẽ tự tải về
các gói NuGet cần thiết (Entity Framework Core...) trong lần build đầu tiên.

### 3. Cấu hình kết nối database

Mở file `QuanLySinhVien/Data/AppDbContext.cs`, sửa biến `ConnectionString`
cho phù hợp với máy của bạn. Mặc định đang trỏ tới LocalDB:

```csharp
private const string ConnectionString =
    @"Server=(localdb)\mssqllocaldb;Database=QuanLySinhVienDB;Trusted_Connection=True;TrustServerCertificate=True;";
```

Nếu dùng SQL Server Express, đổi thành `Server=.\SQLEXPRESS;...`.

### 4. Chạy chương trình

Nhấn F5 (hoặc nút Start). Lần chạy đầu tiên chương trình sẽ tự tạo database
`QuanLySinhVienDB` và tạo sẵn 1 tài khoản quản trị:

- Tên đăng nhập: `admin`
- Mật khẩu: `123456`

Đăng nhập xong nên vào mục "Đổi mật khẩu" để đổi lại mật khẩu khác cho an toàn.

## Ghi chú

- Không cần chạy `Add-Migration` / `Update-Database` gì cả, chương trình tự
  lo việc tạo database bằng `Database.EnsureCreated()` lúc khởi động
  (xem `App.xaml.cs`).
- File `Database/QuanLySinhVien.sql` chỉ để tham khảo cấu trúc bảng, không
  bắt buộc phải chạy.
