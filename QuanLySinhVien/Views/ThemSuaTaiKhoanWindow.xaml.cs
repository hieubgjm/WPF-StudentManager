using System;
using System.Windows;
using System.Windows.Controls;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class ThemSuaTaiKhoanWindow : Window
    {
        private readonly TaiKhoanRepository _taiKhoanRepo = new TaiKhoanRepository();

        // null = đang thêm mới, khác null = đang sửa tài khoản này
        private readonly TaiKhoan _taiKhoanDangSua;

        public ThemSuaTaiKhoanWindow(TaiKhoan taiKhoanCanSua = null)
        {
            InitializeComponent();

            _taiKhoanDangSua = taiKhoanCanSua;

            if (_taiKhoanDangSua != null)
            {
                Title = "Sửa tài khoản";

                // Sửa tài khoản thì không cho đổi mật khẩu ở đây,
                // dùng nút "Đặt lại mật khẩu" ở màn hình danh sách cho rõ ràng
                pnlMatKhau.Visibility = Visibility.Collapsed;

                txtTenDangNhap.Text = _taiKhoanDangSua.TenDangNhap;
                txtHoTen.Text = _taiKhoanDangSua.HoTen;
                chkDangHoatDong.IsChecked = _taiKhoanDangSua.DangHoatDong;

                foreach (ComboBoxItem item in cboVaiTro.Items)
                {
                    if ((string)item.Content == _taiKhoanDangSua.VaiTro)
                    {
                        cboVaiTro.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                Title = "Thêm tài khoản mới";
                cboVaiTro.SelectedIndex = 0;
            }
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!KiemTraDuLieuHopLe())
                return;

            string vaiTro = (string)((ComboBoxItem)cboVaiTro.SelectedItem).Content;

            try
            {
                if (_taiKhoanDangSua == null)
                {
                    var taiKhoanMoi = new TaiKhoan
                    {
                        TenDangNhap = txtTenDangNhap.Text.Trim(),
                        HoTen = txtHoTen.Text.Trim(),
                        VaiTro = vaiTro,
                        DangHoatDong = chkDangHoatDong.IsChecked == true
                    };
                    _taiKhoanRepo.Them(taiKhoanMoi, txtMatKhau.Password);
                }
                else
                {
                    _taiKhoanDangSua.TenDangNhap = txtTenDangNhap.Text.Trim();
                    _taiKhoanDangSua.HoTen = txtHoTen.Text.Trim();
                    _taiKhoanDangSua.VaiTro = vaiTro;
                    _taiKhoanDangSua.DangHoatDong = chkDangHoatDong.IsChecked == true;
                    _taiKhoanRepo.Sua(_taiKhoanDangSua);
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

        private bool KiemTraDuLieuHopLe()
        {
            if (string.IsNullOrWhiteSpace(txtTenDangNhap.Text))
                return BaoLoi("Vui lòng nhập tên đăng nhập.");

            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
                return BaoLoi("Vui lòng nhập họ tên.");

            if (cboVaiTro.SelectedItem == null)
                return BaoLoi("Vui lòng chọn vai trò.");

            int idBoQua = _taiKhoanDangSua?.TaiKhoanId ?? 0;
            if (_taiKhoanRepo.DaTonTaiTenDangNhap(txtTenDangNhap.Text.Trim(), idBoQua))
                return BaoLoi("Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác.");

            // Chỉ bắt buộc nhập mật khẩu khi đang thêm mới
            if (_taiKhoanDangSua == null)
            {
                if (string.IsNullOrEmpty(txtMatKhau.Password) || txtMatKhau.Password.Length < 6)
                    return BaoLoi("Mật khẩu phải có ít nhất 6 ký tự.");

                if (txtMatKhau.Password != txtNhapLaiMatKhau.Password)
                    return BaoLoi("Mật khẩu nhập lại không khớp.");
            }

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
