using System.Windows;
using System.Windows.Input;
using QuanLySinhVien.Helpers;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class DangNhapWindow : Window
    {
        private readonly TaiKhoanRepository _taiKhoanRepo = new TaiKhoanRepository();

        public DangNhapWindow()
        {
            InitializeComponent();
        }

        private void btnDangNhap_Click(object sender, RoutedEventArgs e)
        {
            ThucHienDangNhap();
        }

        // Cho phép nhấn Enter ở ô mật khẩu để đăng nhập luôn, khỏi phải với chuột bấm nút
        private void txtMatKhau_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ThucHienDangNhap();
            }
        }

        private void ThucHienDangNhap()
        {
            string tenDangNhap = txtTenDangNhap.Text.Trim();
            string matKhau = txtMatKhau.Password;

            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            {
                HienThiLoi("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
                return;
            }

            var taiKhoan = _taiKhoanRepo.DangNhap(tenDangNhap, matKhau);
            if (taiKhoan == null)
            {
                HienThiLoi("Sai tên đăng nhập, mật khẩu hoặc tài khoản đã bị khóa.");
                return;
            }

            // Đăng nhập thành công - lưu lại thông tin người dùng đang đăng nhập,
            // sau đó mở màn hình chính và đóng màn hình đăng nhập lại.
            PhienDangNhap.NguoiDungHienTai = taiKhoan;

            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void HienThiLoi(string thongDiep)
        {
            lblThongBao.Text = thongDiep;
            lblThongBao.Visibility = Visibility.Visible;
        }
    }
}
