using System.Windows;
using QuanLySinhVien.Helpers;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class DoiMatKhauWindow : Window
    {
        private readonly TaiKhoanRepository _taiKhoanRepo = new TaiKhoanRepository();

        public DoiMatKhauWindow()
        {
            InitializeComponent();
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            string matKhauCu = txtMatKhauCu.Password;
            string matKhauMoi = txtMatKhauMoi.Password;
            string nhapLai = txtNhapLaiMatKhauMoi.Password;

            if (string.IsNullOrEmpty(matKhauCu) || string.IsNullOrEmpty(matKhauMoi))
            {
                HienThiLoi("Vui lòng nhập đầy đủ thông tin.");
                return;
            }

            if (matKhauMoi.Length < 6)
            {
                HienThiLoi("Mật khẩu mới phải có ít nhất 6 ký tự.");
                return;
            }

            if (matKhauMoi != nhapLai)
            {
                HienThiLoi("Mật khẩu nhập lại không khớp.");
                return;
            }

            int taiKhoanId = PhienDangNhap.NguoiDungHienTai.TaiKhoanId;
            bool thanhCong = _taiKhoanRepo.DoiMatKhau(taiKhoanId, matKhauCu, matKhauMoi);

            if (!thanhCong)
            {
                HienThiLoi("Mật khẩu hiện tại không đúng.");
                return;
            }

            MessageBox.Show("Đổi mật khẩu thành công.", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HienThiLoi(string thongDiep)
        {
            lblThongBao.Text = thongDiep;
            lblThongBao.Visibility = Visibility.Visible;
        }
    }
}
