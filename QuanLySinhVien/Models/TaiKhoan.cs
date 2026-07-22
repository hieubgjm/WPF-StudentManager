using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLySinhVien.Models
{
    /// <summary>
    /// Tài khoản dùng để đăng nhập vào phần mềm (không phải tài khoản của sinh viên).
    /// VD: nhân viên phòng đào tạo, admin quản trị hệ thống.
    /// </summary>
    public class TaiKhoan
    {
        public int TaiKhoanId { get; set; }

        public string TenDangNhap { get; set; }

        // Chỉ lưu mật khẩu đã băm SHA256, KHÔNG bao giờ lưu mật khẩu gốc (plain text)
        public string MatKhauMaHoa { get; set; }

        public string HoTen { get; set; }

        // "Admin" hoặc "NhanVien" - Admin mới được vào mục Quản lý tài khoản
        public string VaiTro { get; set; }

        // Tài khoản bị khóa thì vẫn còn trong DB nhưng không đăng nhập được nữa
        public bool DangHoatDong { get; set; } = true;

        // Chỉ để hiển thị lên DataGrid cho dễ đọc, không lưu xuống database
        [NotMapped]
        public string TrangThaiHienThi => DangHoatDong ? "Đang hoạt động" : "Đã khóa";
    }
}
