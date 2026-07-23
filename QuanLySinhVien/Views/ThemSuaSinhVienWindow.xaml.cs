using System;
using System.Linq;
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
        private readonly MonHocRepository _monHocRepo = new MonHocRepository();
        private readonly DangKyMonHocRepository _dangKyMonHocRepo = new DangKyMonHocRepository();
        private readonly DiemSoRepository _diemSoRepo = new DiemSoRepository();

        // Nếu null tức là đang Thêm mới, khác null tức là đang Sửa sinh viên này
        private readonly SinhVien _sinhVienDangSua;

        public ThemSuaSinhVienWindow(SinhVien sinhVienCanSua = null)
        {
            InitializeComponent();

            _sinhVienDangSua = sinhVienCanSua;

            cboLop.ItemsSource = _lopRepo.LayTatCa();
            TaiDanhSachMonHoc();

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
                cboTrangThai.SelectedIndex = 0;
            }
        }

        // Đổ danh sách môn học ra dạng checkbox, tick sẵn những môn sinh viên đã đăng ký.
        // Môn nào sinh viên đã có điểm rồi thì khóa checkbox lại, không cho bỏ đăng ký nữa.
        private void TaiDanhSachMonHoc()
        {
            var monHocIdDaDangKy = _sinhVienDangSua != null
                ? _dangKyMonHocRepo.LayDanhSachMonHocIdTheoSinhVien(_sinhVienDangSua.SinhVienId)
                : Enumerable.Empty<int>();

            pnlMonHoc.Children.Clear();

            foreach (var monHoc in _monHocRepo.LayTatCa())
            {
                bool daCoDiem = _sinhVienDangSua != null &&
                    _diemSoRepo.DaCoDiemMonHoc(_sinhVienDangSua.SinhVienId, monHoc.MonHocId);

                pnlMonHoc.Children.Add(new CheckBox
                {
                    Content = daCoDiem ? monHoc.TenMonHoc + " (đã có điểm)" : monHoc.TenMonHoc,
                    Tag = monHoc.MonHocId,
                    IsChecked = monHocIdDaDangKy.Contains(monHoc.MonHocId),
                    IsEnabled = !daCoDiem,
                    Margin = new Thickness(0, 0, 16, 6)
                });
            }
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

            foreach (ComboBoxItem item in cboTrangThai.Items)
            {
                if ((string)item.Content == _sinhVienDangSua.TrangThai)
                {
                    cboTrangThai.SelectedItem = item;
                    break;
                }
            }
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
                int sinhVienId;

                if (_sinhVienDangSua == null)
                {
                    var sinhVienMoi = TaoSinhVienTuForm();
                    _sinhVienRepo.Them(sinhVienMoi);
                    sinhVienId = sinhVienMoi.SinhVienId;
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
                    _sinhVienDangSua.TrangThai = (string)((ComboBoxItem)cboTrangThai.SelectedItem).Content;

                    _sinhVienRepo.Sua(_sinhVienDangSua);
                    sinhVienId = _sinhVienDangSua.SinhVienId;
                }

                var monHocIdDaChon = pnlMonHoc.Children.OfType<CheckBox>()
                    .Where(c => c.IsChecked == true)
                    .Select(c => (int)c.Tag)
                    .ToList();
                _dangKyMonHocRepo.CapNhatDangKy(sinhVienId, monHocIdDaChon);

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
                TrangThai = (string)((ComboBoxItem)cboTrangThai.SelectedItem).Content
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

            if (cboTrangThai.SelectedItem == null)
            {
                return BaoLoi("Vui lòng chọn trạng thái học tập.");
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
