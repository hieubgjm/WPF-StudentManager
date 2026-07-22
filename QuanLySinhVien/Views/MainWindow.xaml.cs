using System.Windows;
using QuanLySinhVien.Helpers;

namespace QuanLySinhVien.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var nguoiDung = PhienDangNhap.NguoiDungHienTai;
            lblXinChao.Text = $"Xin chào, {nguoiDung.HoTen} ({nguoiDung.VaiTro})";

            KiemTraQuyenAdmin();
        }

        // Chỉ Admin mới được thấy tab Quản lý tài khoản, nhân viên bình thường thì ẩn đi
        private void KiemTraQuyenAdmin()
        {
            if (!PhienDangNhap.LaAdmin)
            {
                tabTaiKhoan.Visibility = Visibility.Collapsed;
            }
        }

        private void btnDoiMatKhau_Click(object sender, RoutedEventArgs e)
        {
            var cuaSo = new DoiMatKhauWindow { Owner = this };
            cuaSo.ShowDialog();
        }

        private void btnDangXuat_Click(object sender, RoutedEventArgs e)
        {
            var ketQua = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (ketQua != MessageBoxResult.Yes)
                return;

            PhienDangNhap.DangXuat();

            var cuaSoDangNhap = new DangNhapWindow();
            cuaSoDangNhap.Show();
            Close();
        }
    }
}
