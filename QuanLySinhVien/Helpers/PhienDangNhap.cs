using QuanLySinhVien.Models;

namespace QuanLySinhVien.Helpers
{
    /// <summary>
    /// Lưu thông tin tài khoản đang đăng nhập trong lúc chương trình chạy.
    /// Dùng static cho đơn giản vì cả app chỉ có 1 người dùng đăng nhập tại 1 thời điểm
    /// (không cần phải truyền tới truyền lui giữa các Window/Control).
    /// </summary>
    public static class PhienDangNhap
    {
        public static TaiKhoan NguoiDungHienTai { get; set; }

        public static bool LaAdmin => NguoiDungHienTai != null && NguoiDungHienTai.VaiTro == "Admin";

        // Gọi hàm này khi bấm Đăng xuất
        public static void DangXuat()
        {
            NguoiDungHienTai = null;
        }
    }
}
