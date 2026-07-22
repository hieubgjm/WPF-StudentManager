using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class SinhVienControl : UserControl
    {
        private readonly SinhVienRepository _sinhVienRepo = new SinhVienRepository();
        private readonly LopRepository _lopRepo = new LopRepository();

        // Cờ để tránh việc load dữ liệu bị gọi lặp lại khi đang tự set ComboBox bằng code
        private bool _dangKhoiTao = true;

        public SinhVienControl()
        {
            InitializeComponent();
            TaiDanhSachLop();
            TaiDuLieu();
            _dangKhoiTao = false;
        }

        // Đổ danh sách lớp vào ComboBox lọc, thêm 1 dòng "Tất cả các lớp" ở đầu
        private void TaiDanhSachLop()
        {
            var danhSach = new List<Lop>
            {
                new Lop { LopId = 0, TenLop = "-- Tất cả các lớp --" }
            };
            danhSach.AddRange(_lopRepo.LayTatCa());

            cboLocLop.ItemsSource = danhSach;
            cboLocLop.SelectedIndex = 0;
        }

        private void TaiDuLieu()
        {
            string tuKhoa = txtTuKhoa.Text;
            int lopIdLoc = (int)(cboLocLop.SelectedValue ?? 0);

            dgSinhVien.ItemsSource = _sinhVienRepo.Tim(tuKhoa, lopIdLoc);
        }

        private void txtTuKhoa_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_dangKhoiTao) TaiDuLieu();
        }

        private void cboLocLop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_dangKhoiTao) TaiDuLieu();
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            txtTuKhoa.Text = string.Empty;
            TaiDanhSachLop();
            TaiDuLieu();
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            var cuaSo = new ThemSuaSinhVienWindow { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                TaiDuLieu();
            }
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            SuaSinhVienDangChon();
        }

        private void dgSinhVien_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SuaSinhVienDangChon();
        }

        private void SuaSinhVienDangChon()
        {
            var sinhVienDangChon = dgSinhVien.SelectedItem as SinhVien;
            if (sinhVienDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 sinh viên cần sửa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Lấy lại bản ghi mới nhất từ DB (kèm Lop, CaHoc) để tránh sửa trên dữ liệu cũ
            var sinhVien = _sinhVienRepo.LayTheoId(sinhVienDangChon.SinhVienId);

            var cuaSo = new ThemSuaSinhVienWindow(sinhVien) { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                TaiDuLieu();
            }
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            var sinhVienDangChon = dgSinhVien.SelectedItem as SinhVien;
            if (sinhVienDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 sinh viên cần xóa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var xacNhan = MessageBox.Show(
                $"Bạn có chắc muốn xóa sinh viên \"{sinhVienDangChon.HoTen}\" ({sinhVienDangChon.MaSV})?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (xacNhan != MessageBoxResult.Yes)
                return;

            _sinhVienRepo.Xoa(sinhVienDangChon.SinhVienId);
            TaiDuLieu();
        }
    }
}
