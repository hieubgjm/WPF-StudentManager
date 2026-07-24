using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using QuanLySinhVien.Helpers;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    /// <summary>
    /// Màn hình "Bảng điểm": chọn 1 sinh viên ở cột trái, xem/nhập điểm từng môn
    /// ở cột phải, GPA và xếp loại được tự tính lại mỗi khi điểm thay đổi.
    /// </summary>
    public partial class DiemSoControl : UserControl
    {
        private readonly SinhVienRepository _sinhVienRepo = new SinhVienRepository();
        private readonly LopRepository _lopRepo = new LopRepository();
        private readonly DiemSoRepository _diemSoRepo = new DiemSoRepository();

        // Cờ để tránh việc load dữ liệu bị gọi lặp lại khi đang tự set ComboBox bằng code
        private bool _dangKhoiTao = true;

        public DiemSoControl()
        {
            InitializeComponent();
            TaiDanhSachLop();
            _dangKhoiTao = false;
            TaiDanhSachSinhVien();
        }

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

        private void TaiDanhSachSinhVien()
        {
            int lopIdLoc = (int)(cboLocLop.SelectedValue ?? 0);
            dgSinhVien.ItemsSource = _sinhVienRepo.Tim(txtTuKhoaSV.Text, lopIdLoc);

            // Danh sách sinh viên vừa đổi thì dòng đang chọn ở bảng điểm không còn hợp lệ nữa
            HienThiBangDiem();
        }

        private void BoLoc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_dangKhoiTao) TaiDanhSachSinhVien();
        }

        private void BoLoc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_dangKhoiTao) TaiDanhSachSinhVien();
        }

        private void dgSinhVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HienThiBangDiem();
        }

        // Sinh viên đang được chọn ở cột trái (null nếu chưa chọn ai)
        private SinhVien SinhVienDangChon => dgSinhVien.SelectedItem as SinhVien;

        // Đổ lại toàn bộ thông tin cột phải (thông tin SV, GPA, xếp loại, bảng điểm)
        // theo đúng sinh viên đang được chọn ở cột trái.
        private void HienThiBangDiem()
        {
            var sv = SinhVienDangChon;
            if (sv == null)
            {
                lblChuaChonSV.Visibility = Visibility.Visible;
                pnlThongTinSV.Visibility = Visibility.Collapsed;
                pnlNutDiem.Visibility = Visibility.Collapsed;
                dgDiem.ItemsSource = null;
                return;
            }

            lblChuaChonSV.Visibility = Visibility.Collapsed;
            pnlThongTinSV.Visibility = Visibility.Visible;
            pnlNutDiem.Visibility = Visibility.Visible;

            lblHoTenSV.Text = sv.HoTen;
            lblThongTinPhuSV.Text = $"{sv.MaSV} - {sv.Lop?.TenLop ?? "Chưa xếp lớp"}";

            var danhSachDiem = _diemSoRepo.LayTheoSinhVien(sv.SinhVienId);
            dgDiem.ItemsSource = danhSachDiem;

            double? gpa = XepLoaiHelper.TinhGpa(danhSachDiem);
            lblGpa.Text = gpa.HasValue ? gpa.Value.ToString("N2") : "--";
            lblXepLoai.Text = XepLoaiHelper.XepLoaiTuGpa(gpa);
        }

        private void btnNhapBangDiem_Click(object sender, RoutedEventArgs e)
        {
            var cuaSo = new NhapBangDiemWindow { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                TaiDanhSachSinhVien();
            }
        }

        private void btnXuatPdf_Click(object sender, RoutedEventArgs e)
        {
            var sv = SinhVienDangChon;
            if (sv == null)
            {
                MessageBox.Show("Vui lòng chọn 1 sinh viên trước.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var hopThoai = new SaveFileDialog
            {
                Filter = "Tệp PDF (*.pdf)|*.pdf",
                FileName = $"PhieuDiem_{sv.MaSV}.pdf"
            };
            if (hopThoai.ShowDialog() != true)
                return;

            try
            {
                var danhSachDiem = _diemSoRepo.LayTheoSinhVien(sv.SinhVienId);
                double? gpa = XepLoaiHelper.TinhGpa(danhSachDiem);
                string xepLoai = XepLoaiHelper.XepLoaiTuGpa(gpa);

                PdfHelper.XuatPhieuDiemCaNhan(hopThoai.FileName, sv, danhSachDiem, gpa, xepLoai);

                MessageBox.Show("Đã xuất phiếu điểm.", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xuất PDF thất bại: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnThemDiem_Click(object sender, RoutedEventArgs e)
        {
            var sv = SinhVienDangChon;
            if (sv == null)
            {
                MessageBox.Show("Vui lòng chọn 1 sinh viên trước.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var cuaSo = new ThemSuaDiemWindow(sv.SinhVienId) { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                HienThiBangDiem();
            }
        }

        private void btnSuaDiem_Click(object sender, RoutedEventArgs e)
        {
            SuaDiemDangChon();
        }

        private void dgDiem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SuaDiemDangChon();
        }

        private void SuaDiemDangChon()
        {
            var sv = SinhVienDangChon;
            var diemDangChon = dgDiem.SelectedItem as DiemSo;
            if (sv == null || diemDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 dòng điểm cần sửa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var cuaSo = new ThemSuaDiemWindow(sv.SinhVienId, diemDangChon) { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                HienThiBangDiem();
            }
        }

        private void btnXoaDiem_Click(object sender, RoutedEventArgs e)
        {
            var diemDangChon = dgDiem.SelectedItem as DiemSo;
            if (diemDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 dòng điểm cần xóa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var xacNhan = MessageBox.Show(
                $"Bạn có chắc muốn xóa điểm môn \"{diemDangChon.MonHoc?.TenMonHoc}\"?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (xacNhan != MessageBoxResult.Yes)
                return;

            _diemSoRepo.Xoa(diemDangChon.DiemSoId);
            HienThiBangDiem();
        }
    }
}
