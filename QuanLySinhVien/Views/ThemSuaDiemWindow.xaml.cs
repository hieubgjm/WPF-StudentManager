using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class ThemSuaDiemWindow : Window
    {
        private readonly DangKyMonHocRepository _dangKyMonHocRepo = new DangKyMonHocRepository();
        private readonly DiemSoRepository _diemSoRepo = new DiemSoRepository();

        private readonly int _sinhVienId;

        // null = đang thêm điểm môn mới, khác null = đang sửa dòng điểm này
        private readonly DiemSo _diemSoDangSua;

        public ThemSuaDiemWindow(int sinhVienId, DiemSo diemSoCanSua = null)
        {
            InitializeComponent();

            _sinhVienId = sinhVienId;
            _diemSoDangSua = diemSoCanSua;

            if (_diemSoDangSua != null)
            {
                Title = "Sửa điểm";

                // Không cho đổi môn học khi sửa, vì đổi môn thực chất là tạo dòng điểm khác
                cboMonHoc.ItemsSource = new List<MonHoc> { _diemSoDangSua.MonHoc };
                cboMonHoc.SelectedIndex = 0;
                cboMonHoc.IsEnabled = false;

                txtDiemGiuaKy.Text = _diemSoDangSua.DiemGiuaKy?.ToString(CultureInfo.CurrentCulture);
                txtDiemCuoiKy.Text = _diemSoDangSua.DiemCuoiKy?.ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                Title = "Nhập điểm môn học";

                // Chỉ cho chọn những môn sinh viên này ĐÃ đăng ký và CHƯA có điểm, tránh nhập trùng môn
                var maMonHocDaCo = _diemSoRepo.LayTheoSinhVien(sinhVienId)
                    .Select(d => d.MonHocId)
                    .ToHashSet();

                cboMonHoc.ItemsSource = _dangKyMonHocRepo.LayMonHocDaDangKy(sinhVienId)
                    .Where(m => !maMonHocDaCo.Contains(m.MonHocId))
                    .ToList();
            }
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!KiemTraDuLieuHopLe(out double diemGiuaKy, out double diemCuoiKy))
                return;

            try
            {
                if (_diemSoDangSua == null)
                {
                    var diemMoi = new DiemSo
                    {
                        SinhVienId = _sinhVienId,
                        MonHocId = (int)cboMonHoc.SelectedValue,
                        DiemGiuaKy = diemGiuaKy,
                        DiemCuoiKy = diemCuoiKy
                    };
                    _diemSoRepo.Them(diemMoi);
                }
                else
                {
                    _diemSoDangSua.DiemGiuaKy = diemGiuaKy;
                    _diemSoDangSua.DiemCuoiKy = diemCuoiKy;
                    _diemSoRepo.Sua(_diemSoDangSua);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lưu dữ liệu thất bại: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool KiemTraDuLieuHopLe(out double diemGiuaKy, out double diemCuoiKy)
        {
            diemGiuaKy = 0;
            diemCuoiKy = 0;

            if (cboMonHoc.SelectedItem == null)
                return BaoLoi("Vui lòng chọn môn học (nếu sinh viên chưa đăng ký môn nào, hoặc đã có điểm hết các môn đã đăng ký, thì không còn môn nào để chọn).");

            if (!KiemTraDiemHopLe(txtDiemGiuaKy.Text, out diemGiuaKy))
                return BaoLoi("Điểm giữa kỳ phải là số từ 0 đến 10.");

            if (!KiemTraDiemHopLe(txtDiemCuoiKy.Text, out diemCuoiKy))
                return BaoLoi("Điểm cuối kỳ phải là số từ 0 đến 10.");

            lblThongBao.Visibility = Visibility.Collapsed;
            return true;
        }

        private bool KiemTraDiemHopLe(string text, out double diem)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out diem)
                   && diem >= 0 && diem <= 10;
        }

        private bool BaoLoi(string thongDiep)
        {
            lblThongBao.Text = thongDiep;
            lblThongBao.Visibility = Visibility.Visible;
            return false;
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
