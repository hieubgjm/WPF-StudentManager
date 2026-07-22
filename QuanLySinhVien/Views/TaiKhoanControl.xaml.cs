using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLySinhVien.Helpers;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class TaiKhoanControl : UserControl
    {
        private readonly TaiKhoanRepository _taiKhoanRepo = new TaiKhoanRepository();

        public TaiKhoanControl()
        {
            InitializeComponent();
            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            dgTaiKhoan.ItemsSource = _taiKhoanRepo.LayTatCa();
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            TaiDuLieu();
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            var cuaSo = new ThemSuaTaiKhoanWindow { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                TaiDuLieu();
            }
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            SuaTaiKhoanDangChon();
        }

        private void dgTaiKhoan_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SuaTaiKhoanDangChon();
        }

        private void SuaTaiKhoanDangChon()
        {
            var taiKhoanDangChon = dgTaiKhoan.SelectedItem as TaiKhoan;
            if (taiKhoanDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 tài khoản cần sửa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var cuaSo = new ThemSuaTaiKhoanWindow(taiKhoanDangChon) { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                TaiDuLieu();
            }
        }

        private void btnDatLaiMatKhau_Click(object sender, RoutedEventArgs e)
        {
            var taiKhoanDangChon = dgTaiKhoan.SelectedItem as TaiKhoan;
            if (taiKhoanDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 tài khoản cần đặt lại mật khẩu.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Mật khẩu mặc định sau khi đặt lại, người dùng nên đổi lại ngay sau khi đăng nhập
            const string matKhauMacDinh = "123456";

            var xacNhan = MessageBox.Show(
                $"Đặt lại mật khẩu của tài khoản \"{taiKhoanDangChon.TenDangNhap}\" về \"{matKhauMacDinh}\"?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (xacNhan != MessageBoxResult.Yes)
                return;

            _taiKhoanRepo.DatLaiMatKhau(taiKhoanDangChon.TaiKhoanId, matKhauMacDinh);

            MessageBox.Show($"Đã đặt lại mật khẩu về \"{matKhauMacDinh}\".", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            var taiKhoanDangChon = dgTaiKhoan.SelectedItem as TaiKhoan;
            if (taiKhoanDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 tài khoản cần xóa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Không cho tự xóa tài khoản mình đang đăng nhập, tránh tự khóa mình ra khỏi hệ thống
            if (taiKhoanDangChon.TaiKhoanId == PhienDangNhap.NguoiDungHienTai.TaiKhoanId)
            {
                MessageBox.Show("Không thể xóa tài khoản bạn đang đăng nhập.", "Không thể xóa",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var xacNhan = MessageBox.Show(
                $"Bạn có chắc muốn xóa tài khoản \"{taiKhoanDangChon.TenDangNhap}\"?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (xacNhan != MessageBoxResult.Yes)
                return;

            _taiKhoanRepo.Xoa(taiKhoanDangChon.TaiKhoanId);
            TaiDuLieu();
        }
    }
}
