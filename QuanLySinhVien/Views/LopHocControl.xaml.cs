using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class LopHocControl : UserControl
    {
        private readonly LopRepository _lopRepo = new LopRepository();

        public LopHocControl()
        {
            InitializeComponent();
            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            dgLop.ItemsSource = _lopRepo.Tim(txtTuKhoa.Text);
        }

        private void txtTuKhoa_TextChanged(object sender, TextChangedEventArgs e)
        {
            TaiDuLieu();
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            txtTuKhoa.Text = string.Empty;
            TaiDuLieu();
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            var cuaSo = new ThemSuaLopWindow { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                TaiDuLieu();
            }
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            SuaLopDangChon();
        }

        private void dgLop_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SuaLopDangChon();
        }

        private void SuaLopDangChon()
        {
            var lopDangChon = dgLop.SelectedItem as Lop;
            if (lopDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 lớp cần sửa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var cuaSo = new ThemSuaLopWindow(lopDangChon) { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                TaiDuLieu();
            }
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            var lopDangChon = dgLop.SelectedItem as Lop;
            if (lopDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 lớp cần xóa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Không cho xóa nếu lớp vẫn còn sinh viên, tránh dữ liệu sinh viên bị "mồ côi"
            if (!_lopRepo.CoTheXoa(lopDangChon.LopId))
            {
                MessageBox.Show(
                    "Không thể xóa lớp này vì vẫn còn sinh viên thuộc lớp.\n" +
                    "Vui lòng chuyển hết sinh viên sang lớp khác trước khi xóa.",
                    "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var xacNhan = MessageBox.Show(
                $"Bạn có chắc muốn xóa lớp \"{lopDangChon.TenLop}\"?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (xacNhan != MessageBoxResult.Yes)
                return;

            _lopRepo.Xoa(lopDangChon.LopId);
            TaiDuLieu();
        }
    }
}
