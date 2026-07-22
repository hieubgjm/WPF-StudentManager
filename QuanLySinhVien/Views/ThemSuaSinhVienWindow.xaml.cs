using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class ThemSuaSinhVienWindow : Window
    {
        private readonly SinhVienRepository _sinhVienRepo = new SinhVienRepository();
        private readonly LopRepository _lopRepo = new LopRepository();
        private readonly CaHocRepository _caHocRepo = new CaHocRepository();

        // Nếu null tức là đang Thêm mới, khác null tức là đang Sửa sinh viên này
        private readonly SinhVien _sinhVienDangSua;

        public ThemSuaSinhVienWindow(SinhVien sinhVienCanSua = null)
        {
            InitializeComponent();

            _sinhVienDangSua = sinhVienCanSua;

            TaiDanhSachLopVaCaHoc();

            if (_sinhVienDangSua != null)
            {
                Title = "Sửa thông tin sinh viên";
                HienThiDuLieuLenForm();
            }
            else
            {
                Title = "Thêm sinh viên mới";
                dpNgaySinh.SelectedDate = DateTime.Now.AddYears(-18);
                cboGioiTinh.SelectedIndex = 0;
            }
        }

        private void TaiDanhSachLopVaCaHoc()
        {
            cboLop.ItemsSource = _lopRepo.LayTatCa();
            cboCaHoc.ItemsSource = _caHocRepo.LayTatCa();
        }

        // Đổ dữ liệu sinh viên đang sửa lên các ô nhập liệu
        private void HienThiDuLieuLenForm()
        {
            txtMaSV.Text = _sinhVienDangSua.MaSV;
            txtHoTen.Text = _sinhVienDangSua.HoTen;
            dpNgaySinh.SelectedDate = _sinhVienDangSua.NgaySinh;
            txtDiaChi.Text = _sinhVienDangSua.DiaChi;
            txtSoDienThoai.Text = _sinhVienDangSua.SoDienThoai;
            txtEmail.Text = _sinhVienDangSua.Email;

            foreach (ComboBoxItem item in cboGioiTinh.Items)
            {
                if ((string)item.Content == _sinhVienDangSua.GioiTinh)
                {
                    cboGioiTinh.SelectedItem = item;
                    break;
                }
            }

            cboLop.SelectedValue = _sinhVienDangSua.LopId;
            cboCaHoc.SelectedValue = _sinhVienDangSua.CaHocId;
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!KiemTraDuLieuHopLe())
                return;

            var lopDaChon = cboLop.SelectedItem as Lop;

            // Không bắt buộc, chỉ cảnh báo cho người dùng biết lớp sắp đầy/đã đầy sĩ số
            if (lopDaChon != null && lopDaChon.SiSoToiDa > 0)
            {
                int siSoHienTai = _sinhVienRepo.DemSiSoLop(lopDaChon.LopId);
                bool laThemMoiVaoLopNay = _sinhVienDangSua == null || _sinhVienDangSua.LopId != lopDaChon.LopId;

                if (laThemMoiVaoLopNay && siSoHienTai >= lopDaChon.SiSoToiDa)
                {
                    var tiepTuc = MessageBox.Show(
                        $"Lớp {lopDaChon.TenLop} đã đạt sĩ số tối đa ({lopDaChon.SiSoToiDa}). Bạn vẫn muốn tiếp tục?",
                        "Cảnh báo sĩ số", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (tiepTuc != MessageBoxResult.Yes)
                        return;
                }
            }

            try
            {
                if (_sinhVienDangSua == null)
                {
                    var sinhVienMoi = TaoSinhVienTuForm();
                    _sinhVienRepo.Them(sinhVienMoi);
                }
                else
                {
                    _sinhVienDangSua.MaSV = txtMaSV.Text.Trim();
                    _sinhVienDangSua.HoTen = txtHoTen.Text.Trim();
                    _sinhVienDangSua.NgaySinh = dpNgaySinh.SelectedDate.Value;
                    _sinhVienDangSua.GioiTinh = (string)((ComboBoxItem)cboGioiTinh.SelectedItem).Content;
                    _sinhVienDangSua.DiaChi = txtDiaChi.Text.Trim();
                    _sinhVienDangSua.SoDienThoai = txtSoDienThoai.Text.Trim();
                    _sinhVienDangSua.Email = txtEmail.Text.Trim();
                    _sinhVienDangSua.LopId = (int?)cboLop.SelectedValue;
                    _sinhVienDangSua.CaHocId = (int?)cboCaHoc.SelectedValue;

                    _sinhVienRepo.Sua(_sinhVienDangSua);
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

        private SinhVien TaoSinhVienTuForm()
        {
            return new SinhVien
            {
                MaSV = txtMaSV.Text.Trim(),
                HoTen = txtHoTen.Text.Trim(),
                NgaySinh = dpNgaySinh.SelectedDate.Value,
                GioiTinh = (string)((ComboBoxItem)cboGioiTinh.SelectedItem).Content,
                DiaChi = txtDiaChi.Text.Trim(),
                SoDienThoai = txtSoDienThoai.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                LopId = (int?)cboLop.SelectedValue,
                CaHocId = (int?)cboCaHoc.SelectedValue
            };
        }

        // Kiểm tra dữ liệu người dùng nhập trước khi cho lưu xuống database
        private bool KiemTraDuLieuHopLe()
        {
            if (string.IsNullOrWhiteSpace(txtMaSV.Text))
            {
                return BaoLoi("Vui lòng nhập mã sinh viên.");
            }

            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                return BaoLoi("Vui lòng nhập họ tên.");
            }

            if (dpNgaySinh.SelectedDate == null)
            {
                return BaoLoi("Vui lòng chọn ngày sinh.");
            }

            if (cboGioiTinh.SelectedItem == null)
            {
                return BaoLoi("Vui lòng chọn giới tính.");
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text) &&
                !Regex.IsMatch(txtEmail.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return BaoLoi("Email không đúng định dạng.");
            }

            if (!string.IsNullOrWhiteSpace(txtSoDienThoai.Text) &&
                !Regex.IsMatch(txtSoDienThoai.Text.Trim(), @"^[0-9]{9,11}$"))
            {
                return BaoLoi("Số điện thoại phải là 9-11 chữ số.");
            }

            // ID bỏ qua = 0 nếu đang thêm mới, hoặc chính ID sinh viên đang sửa
            int idBoQua = _sinhVienDangSua?.SinhVienId ?? 0;
            if (_sinhVienRepo.DaTonTaiMaSV(txtMaSV.Text.Trim(), idBoQua))
            {
                return BaoLoi("Mã sinh viên này đã tồn tại, vui lòng chọn mã khác.");
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
