using System;
using System.Windows;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class ThemSuaLopWindow : Window
    {
        private readonly LopRepository _lopRepo = new LopRepository();

        // null = đang thêm mới, khác null = đang sửa lớp này
        private readonly Lop _lopDangSua;

        public ThemSuaLopWindow(Lop lopCanSua = null)
        {
            InitializeComponent();

            _lopDangSua = lopCanSua;

            if (_lopDangSua != null)
            {
                Title = "Sửa thông tin lớp học";
                txtMaLop.Text = _lopDangSua.MaLop;
                txtTenLop.Text = _lopDangSua.TenLop;
                txtKhoa.Text = _lopDangSua.Khoa;
                txtKhoaHoc.Text = _lopDangSua.KhoaHoc;
                txtSiSoToiDa.Text = _lopDangSua.SiSoToiDa.ToString();
                txtGhiChu.Text = _lopDangSua.GhiChu;
            }
            else
            {
                Title = "Thêm lớp học mới";
                txtSiSoToiDa.Text = "40";
            }
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!KiemTraDuLieuHopLe(out int siSoToiDa))
                return;

            try
            {
                if (_lopDangSua == null)
                {
                    var lopMoi = new Lop
                    {
                        MaLop = txtMaLop.Text.Trim(),
                        TenLop = txtTenLop.Text.Trim(),
                        Khoa = txtKhoa.Text.Trim(),
                        KhoaHoc = txtKhoaHoc.Text.Trim(),
                        SiSoToiDa = siSoToiDa,
                        GhiChu = txtGhiChu.Text.Trim()
                    };
                    _lopRepo.Them(lopMoi);
                }
                else
                {
                    _lopDangSua.MaLop = txtMaLop.Text.Trim();
                    _lopDangSua.TenLop = txtTenLop.Text.Trim();
                    _lopDangSua.Khoa = txtKhoa.Text.Trim();
                    _lopDangSua.KhoaHoc = txtKhoaHoc.Text.Trim();
                    _lopDangSua.SiSoToiDa = siSoToiDa;
                    _lopDangSua.GhiChu = txtGhiChu.Text.Trim();
                    _lopRepo.Sua(_lopDangSua);
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

        private bool KiemTraDuLieuHopLe(out int siSoToiDa)
        {
            siSoToiDa = 0;

            if (string.IsNullOrWhiteSpace(txtMaLop.Text))
                return BaoLoi("Vui lòng nhập mã lớp.");

            if (string.IsNullOrWhiteSpace(txtTenLop.Text))
                return BaoLoi("Vui lòng nhập tên lớp.");

            if (!int.TryParse(txtSiSoToiDa.Text.Trim(), out siSoToiDa) || siSoToiDa <= 0)
                return BaoLoi("Sĩ số tối đa phải là một số nguyên dương.");

            int idBoQua = _lopDangSua?.LopId ?? 0;
            if (_lopRepo.DaTonTaiMaLop(txtMaLop.Text.Trim(), idBoQua))
                return BaoLoi("Mã lớp này đã tồn tại, vui lòng chọn mã khác.");

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
