using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class CaHocControl : UserControl
    {
        private readonly CaHocRepository _caHocRepo = new CaHocRepository();

        public CaHocControl()
        {
            InitializeComponent();
            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            dgCaHoc.ItemsSource = _caHocRepo.LayTatCa();
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            TaiDuLieu();
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            var cuaSo = new ThemSuaCaHocWindow { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                TaiDuLieu();
            }
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            SuaCaHocDangChon();
        }

        private void dgCaHoc_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SuaCaHocDangChon();
        }

        private void SuaCaHocDangChon()
        {
            var caHocDangChon = dgCaHoc.SelectedItem as CaHoc;
            if (caHocDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 ca học cần sửa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var cuaSo = new ThemSuaCaHocWindow(caHocDangChon) { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                TaiDuLieu();
            }
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            var caHocDangChon = dgCaHoc.SelectedItem as CaHoc;
            if (caHocDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 ca học cần xóa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!_caHocRepo.CoTheXoa(caHocDangChon.CaHocId))
            {
                MessageBox.Show(
                    "Không thể xóa ca học này vì vẫn còn sinh viên đang học theo ca.",
                    "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var xacNhan = MessageBox.Show(
                $"Bạn có chắc muốn xóa \"{caHocDangChon.TenCa}\"?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (xacNhan != MessageBoxResult.Yes)
                return;

            _caHocRepo.Xoa(caHocDangChon.CaHocId);
            TaiDuLieu();
        }
    }
}
