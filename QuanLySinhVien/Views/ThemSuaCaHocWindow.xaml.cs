using System;
using System.Windows;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class ThemSuaCaHocWindow : Window
    {
        private readonly CaHocRepository _caHocRepo = new CaHocRepository();

        // null = đang thêm mới, khác null = đang sửa ca học này
        private readonly CaHoc _caHocDangSua;

        public ThemSuaCaHocWindow(CaHoc caHocCanSua = null)
        {
            InitializeComponent();

            _caHocDangSua = caHocCanSua;

            if (_caHocDangSua != null)
            {
                Title = "Sửa ca học";
                txtTenCa.Text = _caHocDangSua.TenCa;
                txtGioBatDau.Text = _caHocDangSua.GioBatDau.ToString(@"hh\:mm");
                txtGioKetThuc.Text = _caHocDangSua.GioKetThuc.ToString(@"hh\:mm");
                txtGhiChu.Text = _caHocDangSua.GhiChu;
            }
            else
            {
                Title = "Thêm ca học mới";
            }
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!KiemTraDuLieuHopLe(out TimeSpan gioBatDau, out TimeSpan gioKetThuc))
                return;

            try
            {
                if (_caHocDangSua == null)
                {
                    var caHocMoi = new CaHoc
                    {
                        TenCa = txtTenCa.Text.Trim(),
                        GioBatDau = gioBatDau,
                        GioKetThuc = gioKetThuc,
                        GhiChu = txtGhiChu.Text.Trim()
                    };
                    _caHocRepo.Them(caHocMoi);
                }
                else
                {
                    _caHocDangSua.TenCa = txtTenCa.Text.Trim();
                    _caHocDangSua.GioBatDau = gioBatDau;
                    _caHocDangSua.GioKetThuc = gioKetThuc;
                    _caHocDangSua.GhiChu = txtGhiChu.Text.Trim();
                    _caHocRepo.Sua(_caHocDangSua);
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

        private bool KiemTraDuLieuHopLe(out TimeSpan gioBatDau, out TimeSpan gioKetThuc)
        {
            gioBatDau = TimeSpan.Zero;
            gioKetThuc = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(txtTenCa.Text))
                return BaoLoi("Vui lòng nhập tên ca học.");

            if (!TimeSpan.TryParse(txtGioBatDau.Text.Trim(), out gioBatDau))
                return BaoLoi("Giờ bắt đầu không đúng định dạng, nhập theo dạng HH:mm (vd 07:00).");

            if (!TimeSpan.TryParse(txtGioKetThuc.Text.Trim(), out gioKetThuc))
                return BaoLoi("Giờ kết thúc không đúng định dạng, nhập theo dạng HH:mm (vd 11:00).");

            if (gioKetThuc <= gioBatDau)
                return BaoLoi("Giờ kết thúc phải sau giờ bắt đầu.");

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
