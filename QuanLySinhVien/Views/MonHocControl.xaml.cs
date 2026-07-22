using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class MonHocControl : UserControl
    {
        private readonly MonHocRepository _monHocRepo = new MonHocRepository();

        public MonHocControl()
        {
            InitializeComponent();
            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            dgMonHoc.ItemsSource = _monHocRepo.Tim(txtTuKhoa.Text);
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
            var cuaSo = new ThemSuaMonHocWindow { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                TaiDuLieu();
            }
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            SuaMonHocDangChon();
        }

        private void dgMonHoc_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SuaMonHocDangChon();
        }

        private void SuaMonHocDangChon()
        {
            var monHocDangChon = dgMonHoc.SelectedItem as MonHoc;
            if (monHocDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 môn học cần sửa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var cuaSo = new ThemSuaMonHocWindow(monHocDangChon) { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                TaiDuLieu();
            }
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            var monHocDangChon = dgMonHoc.SelectedItem as MonHoc;
            if (monHocDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 môn học cần xóa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Không cho xóa nếu đã có sinh viên được nhập điểm cho môn này
            if (!_monHocRepo.CoTheXoa(monHocDangChon.MonHocId))
            {
                MessageBox.Show(
                    "Không thể xóa môn học này vì đã có sinh viên được nhập điểm.\n" +
                    "Vui lòng xóa hết điểm liên quan trước khi xóa môn học.",
                    "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var xacNhan = MessageBox.Show(
                $"Bạn có chắc muốn xóa môn học \"{monHocDangChon.TenMonHoc}\"?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (xacNhan != MessageBoxResult.Yes)
                return;

            _monHocRepo.Xoa(monHocDangChon.MonHocId);
            TaiDuLieu();
        }
    }
}
