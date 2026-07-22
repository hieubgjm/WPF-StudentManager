using System;
using System.Windows;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class ThemSuaMonHocWindow : Window
    {
        private readonly MonHocRepository _monHocRepo = new MonHocRepository();

        // null = đang thêm mới, khác null = đang sửa môn học này
        private readonly MonHoc _monHocDangSua;

        public ThemSuaMonHocWindow(MonHoc monHocCanSua = null)
        {
            InitializeComponent();

            _monHocDangSua = monHocCanSua;

            if (_monHocDangSua != null)
            {
                Title = "Sửa thông tin môn học";
                txtMaMonHoc.Text = _monHocDangSua.MaMonHoc;
                txtTenMonHoc.Text = _monHocDangSua.TenMonHoc;
                txtSoTinChi.Text = _monHocDangSua.SoTinChi.ToString();
                txtGhiChu.Text = _monHocDangSua.GhiChu;
            }
            else
            {
                Title = "Thêm môn học mới";
                txtSoTinChi.Text = "3";
            }
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!KiemTraDuLieuHopLe(out int soTinChi))
                return;

            try
            {
                if (_monHocDangSua == null)
                {
                    var monHocMoi = new MonHoc
                    {
                        MaMonHoc = txtMaMonHoc.Text.Trim(),
                        TenMonHoc = txtTenMonHoc.Text.Trim(),
                        SoTinChi = soTinChi,
                        GhiChu = txtGhiChu.Text.Trim()
                    };
                    _monHocRepo.Them(monHocMoi);
                }
                else
                {
                    _monHocDangSua.MaMonHoc = txtMaMonHoc.Text.Trim();
                    _monHocDangSua.TenMonHoc = txtTenMonHoc.Text.Trim();
                    _monHocDangSua.SoTinChi = soTinChi;
                    _monHocDangSua.GhiChu = txtGhiChu.Text.Trim();
                    _monHocRepo.Sua(_monHocDangSua);
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

        private bool KiemTraDuLieuHopLe(out int soTinChi)
        {
            soTinChi = 0;

            if (string.IsNullOrWhiteSpace(txtMaMonHoc.Text))
                return BaoLoi("Vui lòng nhập mã môn học.");

            if (string.IsNullOrWhiteSpace(txtTenMonHoc.Text))
                return BaoLoi("Vui lòng nhập tên môn học.");

            if (!int.TryParse(txtSoTinChi.Text.Trim(), out soTinChi) || soTinChi <= 0)
                return BaoLoi("Số tín chỉ phải là một số nguyên dương.");

            int idBoQua = _monHocDangSua?.MonHocId ?? 0;
            if (_monHocRepo.DaTonTaiMaMonHoc(txtMaMonHoc.Text.Trim(), idBoQua))
                return BaoLoi("Mã môn học này đã tồn tại, vui lòng chọn mã khác.");

            lblThongBao.Visibility = Visibility.Collapsed;
            return true;
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
