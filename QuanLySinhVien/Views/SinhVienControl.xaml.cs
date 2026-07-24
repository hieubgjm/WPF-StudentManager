using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using QuanLySinhVien.Helpers;
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
            TaiDanhSachKhoa();
            _dangKhoiTao = false;
            TaiDuLieu();
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

        // Khoa không có bảng riêng, chỉ là 1 cột chữ trong bảng Lớp, nên lấy distinct
        // từ danh sách lớp hiện có để đổ lên ComboBox lọc theo khoa.
        private void TaiDanhSachKhoa()
        {
            var danhSachKhoa = new List<string> { "-- Tất cả các khoa --" };
            danhSachKhoa.AddRange(
                _lopRepo.LayTatCa()
                    .Select(l => l.Khoa)
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Distinct()
                    .OrderBy(k => k));

            cboLocKhoa.ItemsSource = danhSachKhoa;
            cboLocKhoa.SelectedIndex = 0;
        }

        // Gom toàn bộ tiêu chí đang chọn trên khu vực bộ lọc thành 1 BoLocSinhVien
        // rồi gọi Repository lọc nâng cao, đổ kết quả lên DataGrid.
        private void TaiDuLieu()
        {
            var boLoc = new BoLocSinhVien
            {
                TuKhoa = txtTuKhoa.Text,
                LopId = (int?)cboLocLop.SelectedValue,
                Khoa = LayGiaTriKhoaDangChon(),
                GioiTinh = LayNoiDungComboBoxItem(cboLocGioiTinh),
                TrangThai = LayNoiDungComboBoxItem(cboLocTrangThai),
                NgaySinhTu = dpNgaySinhTu.SelectedDate,
                NgaySinhDen = dpNgaySinhDen.SelectedDate,
                GpaTu = ChuyenSangSo(txtGpaTu.Text),
                GpaDen = ChuyenSangSo(txtGpaDen.Text)
            };

            dgSinhVien.ItemsSource = _sinhVienRepo.TimNangCao(boLoc);
        }

        // Dòng đầu tiên trong combo Khoa là "-- Tất cả các khoa --", coi như không lọc
        private string LayGiaTriKhoaDangChon()
        {
            if (cboLocKhoa.SelectedIndex <= 0) return null;
            return cboLocKhoa.SelectedItem as string;
        }

        // Dòng đầu tiên (ComboBoxItem mặc định, không phải Content thật) coi như không lọc
        private string LayNoiDungComboBoxItem(ComboBox combo)
        {
            if (combo.SelectedIndex <= 0) return null;
            return (string)((ComboBoxItem)combo.SelectedItem).Content;
        }

        // Đổi chuỗi người dùng gõ (ô GPA) sang số, gõ sai định dạng thì coi như bỏ trống
        // (không lọc theo tiêu chí đó) thay vì báo lỗi làm gián đoạn việc gõ.
        private double? ChuyenSangSo(string text)
        {
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out double ketQua))
                return ketQua;
            return null;
        }

        // 2 handler dùng chung cho mọi ô lọc (tách riêng vì TextBox.TextChanged và
        // ComboBox/DatePicker.SelectionChanged có kiểu EventArgs khác nhau),
        // hễ đổi tiêu chí nào là lọc lại ngay.
        private void BoLoc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_dangKhoiTao) TaiDuLieu();
        }

        private void BoLoc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_dangKhoiTao) TaiDuLieu();
        }

        private void btnXoaLoc_Click(object sender, RoutedEventArgs e)
        {
            _dangKhoiTao = true;

            txtTuKhoa.Text = string.Empty;
            cboLocLop.SelectedIndex = 0;
            cboLocKhoa.SelectedIndex = 0;
            cboLocGioiTinh.SelectedIndex = 0;
            cboLocTrangThai.SelectedIndex = 0;
            dpNgaySinhTu.SelectedDate = null;
            dpNgaySinhDen.SelectedDate = null;
            txtGpaTu.Text = string.Empty;
            txtGpaDen.Text = string.Empty;

            _dangKhoiTao = false;
            TaiDuLieu();
        }

        private void btnXuatPdf_Click(object sender, RoutedEventArgs e)
        {
            var danhSach = dgSinhVien.ItemsSource as List<SinhVienHienThi>;
            if (danhSach == null || danhSach.Count == 0)
            {
                MessageBox.Show("Không có sinh viên nào để xuất (kiểm tra lại bộ lọc).", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var hopThoai = new SaveFileDialog
            {
                Filter = "Tệp PDF (*.pdf)|*.pdf",
                FileName = $"DanhSachSinhVien_{DateTime.Now:yyyyMMdd}.pdf"
            };
            if (hopThoai.ShowDialog() != true)
                return;

            try
            {
                PdfHelper.XuatDanhSachSinhVien(hopThoai.FileName, danhSach);

                MessageBox.Show("Đã xuất danh sách sinh viên.", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xuất PDF thất bại: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnNhapTuFile_Click(object sender, RoutedEventArgs e)
        {
            var cuaSo = new NhapSinhVienTuFileWindow { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                TaiDuLieu();
            }
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
            var dongDangChon = dgSinhVien.SelectedItem as SinhVienHienThi;
            if (dongDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 sinh viên cần sửa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Lấy lại bản ghi mới nhất từ DB (kèm Lop) để tránh sửa trên dữ liệu cũ
            var sinhVien = _sinhVienRepo.LayTheoId(dongDangChon.SinhVienId);

            var cuaSo = new ThemSuaSinhVienWindow(sinhVien) { Owner = Window.GetWindow(this) };
            if (cuaSo.ShowDialog() == true)
            {
                TaiDuLieu();
            }
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            var dongDangChon = dgSinhVien.SelectedItem as SinhVienHienThi;
            if (dongDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn 1 sinh viên cần xóa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var xacNhan = MessageBox.Show(
                $"Bạn có chắc muốn xóa sinh viên \"{dongDangChon.HoTen}\" ({dongDangChon.MaSV})?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (xacNhan != MessageBoxResult.Yes)
                return;

            _sinhVienRepo.Xoa(dongDangChon.SinhVienId);
            TaiDuLieu();
        }
    }
}
