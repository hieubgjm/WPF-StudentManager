using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using QuanLySinhVien.Helpers;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    /// <summary>
    /// Màn hình "Nhập bảng điểm theo lớp": chọn 1 lớp + 1 môn học, hiện lưới toàn bộ
    /// sinh viên của lớp để gõ điểm giữa kỳ/cuối kỳ hàng loạt rồi Lưu tất cả 1 lần,
    /// thay vì phải mở dialog nhập từng sinh viên một như màn "Bảng điểm".
    /// Ngoài gõ tay, còn hỗ trợ xuất file mẫu CSV và nhập điểm ngược lại từ file CSV.
    /// </summary>
    public partial class NhapBangDiemWindow : Window
    {
        private readonly LopRepository _lopRepo = new LopRepository();
        private readonly MonHocRepository _monHocRepo = new MonHocRepository();
        private readonly SinhVienRepository _sinhVienRepo = new SinhVienRepository();
        private readonly DiemSoRepository _diemSoRepo = new DiemSoRepository();

        private List<DongNhapDiem> _danhSachDong;
        private bool _daLuu;

        public NhapBangDiemWindow()
        {
            InitializeComponent();

            cboLop.ItemsSource = _lopRepo.LayTatCa();
            cboMonHoc.ItemsSource = _monHocRepo.LayTatCa();
        }

        private void btnTaiDanhSach_Click(object sender, RoutedEventArgs e)
        {
            if (cboLop.SelectedValue == null || cboMonHoc.SelectedValue == null)
            {
                BaoLoi("Vui lòng chọn Lớp và Môn học trước khi tải danh sách.");
                return;
            }

            int lopId = (int)cboLop.SelectedValue;
            int monHocId = (int)cboMonHoc.SelectedValue;

            var sinhViens = _sinhVienRepo.Tim(null, lopId);
            if (sinhViens.Count == 0)
            {
                BaoLoi("Lớp này chưa có sinh viên nào.");
                dgDiem.ItemsSource = null;
                _danhSachDong = null;
                return;
            }

            var diemDaCo = _diemSoRepo.LayTheoLopVaMon(lopId, monHocId)
                .ToDictionary(d => d.SinhVienId);

            _danhSachDong = sinhViens.Select(sv =>
            {
                diemDaCo.TryGetValue(sv.SinhVienId, out var diem);
                return new DongNhapDiem
                {
                    SinhVienId = sv.SinhVienId,
                    MaSV = sv.MaSV,
                    HoTen = sv.HoTen,
                    DiemGiuaKy = diem?.DiemGiuaKy?.ToString(CultureInfo.CurrentCulture),
                    DiemCuoiKy = diem?.DiemCuoiKy?.ToString(CultureInfo.CurrentCulture)
                };
            }).ToList();

            dgDiem.ItemsSource = _danhSachDong;
            AnThongBao();
        }

        private void btnLuuTatCa_Click(object sender, RoutedEventArgs e)
        {
            if (_danhSachDong == null || _danhSachDong.Count == 0)
            {
                BaoLoi("Vui lòng tải danh sách sinh viên trước khi lưu.");
                return;
            }

            var danhSachLuu = new List<DiemNhapDto>();

            foreach (var dong in _danhSachDong)
            {
                if (!KiemTraDiemHopLe(dong.DiemGiuaKy, out double? diemGiuaKy))
                {
                    BaoLoi($"Điểm giữa kỳ của sinh viên {dong.MaSV} không hợp lệ (phải là số từ 0 đến 10, hoặc để trống).");
                    return;
                }

                if (!KiemTraDiemHopLe(dong.DiemCuoiKy, out double? diemCuoiKy))
                {
                    BaoLoi($"Điểm cuối kỳ của sinh viên {dong.MaSV} không hợp lệ (phải là số từ 0 đến 10, hoặc để trống).");
                    return;
                }

                danhSachLuu.Add(new DiemNhapDto
                {
                    SinhVienId = dong.SinhVienId,
                    MaSV = dong.MaSV,
                    HoTen = dong.HoTen,
                    DiemGiuaKy = diemGiuaKy,
                    DiemCuoiKy = diemCuoiKy
                });
            }

            try
            {
                int monHocId = (int)cboMonHoc.SelectedValue;
                _diemSoRepo.LuuHangLoat(monHocId, danhSachLuu);
                _daLuu = true;
                AnThongBao();

                MessageBox.Show("Đã lưu bảng điểm.", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                BaoLoi("Lưu dữ liệu thất bại: " + ex.Message);
            }
        }

        // Chuỗi rỗng -> null (chưa nhập điểm môn này), có chữ thì phải là số từ 0 đến 10
        private bool KiemTraDiemHopLe(string text, out double? diem)
        {
            diem = null;

            if (string.IsNullOrWhiteSpace(text))
                return true;

            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out double giaTri)
                || giaTri < 0 || giaTri > 10)
                return false;

            diem = giaTri;
            return true;
        }

        private void btnXuatPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_danhSachDong == null || _danhSachDong.Count == 0)
            {
                BaoLoi("Vui lòng tải danh sách sinh viên trước khi xuất PDF.");
                return;
            }

            var danhSachXuat = new List<(string MaSV, string HoTen, double? DiemGiuaKy, double? DiemCuoiKy)>();

            foreach (var dong in _danhSachDong)
            {
                if (!KiemTraDiemHopLe(dong.DiemGiuaKy, out double? diemGiuaKy))
                {
                    BaoLoi($"Điểm giữa kỳ của sinh viên {dong.MaSV} không hợp lệ (phải là số từ 0 đến 10, hoặc để trống).");
                    return;
                }

                if (!KiemTraDiemHopLe(dong.DiemCuoiKy, out double? diemCuoiKy))
                {
                    BaoLoi($"Điểm cuối kỳ của sinh viên {dong.MaSV} không hợp lệ (phải là số từ 0 đến 10, hoặc để trống).");
                    return;
                }

                danhSachXuat.Add((dong.MaSV, dong.HoTen, diemGiuaKy, diemCuoiKy));
            }

            var monHoc = (MonHoc)cboMonHoc.SelectedItem;
            var lop = (Lop)cboLop.SelectedItem;

            var hopThoai = new SaveFileDialog
            {
                Filter = "Tệp PDF (*.pdf)|*.pdf",
                FileName = $"BangDiem_{lop.MaLop}_{monHoc.MaMonHoc}.pdf"
            };

            if (hopThoai.ShowDialog() != true)
                return;

            try
            {
                PdfHelper.XuatBangDiemLop(hopThoai.FileName, lop, monHoc, danhSachXuat);
                AnThongBao();

                MessageBox.Show("Đã xuất bảng điểm.", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                BaoLoi("Xuất PDF thất bại: " + ex.Message);
            }
        }

        private void btnXuatMau_Click(object sender, RoutedEventArgs e)
        {
            if (_danhSachDong == null || _danhSachDong.Count == 0)
            {
                BaoLoi("Vui lòng tải danh sách sinh viên trước khi xuất file mẫu.");
                return;
            }

            var monHoc = (MonHoc)cboMonHoc.SelectedItem;
            var lop = (Lop)cboLop.SelectedItem;

            var hopThoai = new SaveFileDialog
            {
                Filter = "Tệp CSV (*.csv)|*.csv",
                FileName = $"BangDiem_{lop.MaLop}_{monHoc.MaMonHoc}.csv"
            };

            if (hopThoai.ShowDialog() != true)
                return;

            try
            {
                var dong = new List<string> { "MaSV;HoTen;DiemGiuaKy;DiemCuoiKy" };
                dong.AddRange(_danhSachDong.Select(d =>
                    $"{d.MaSV};{d.HoTen};{d.DiemGiuaKy};{d.DiemCuoiKy}"));

                File.WriteAllLines(hopThoai.FileName, dong, Encoding.UTF8);

                MessageBox.Show(
                    "Đã xuất file mẫu. Mở bằng Excel để nhập/sửa điểm rồi dùng nút \"Nhập từ file CSV...\" để đọc lại.",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                BaoLoi("Xuất file thất bại: " + ex.Message);
            }
        }

        private void btnNhapFile_Click(object sender, RoutedEventArgs e)
        {
            if (_danhSachDong == null || _danhSachDong.Count == 0)
            {
                BaoLoi("Vui lòng tải danh sách sinh viên (chọn Lớp + Môn học) trước khi nhập từ file.");
                return;
            }

            var hopThoai = new OpenFileDialog { Filter = "Tệp CSV (*.csv)|*.csv" };
            if (hopThoai.ShowDialog() != true)
                return;

            try
            {
                var dongTheoMaSV = _danhSachDong.ToDictionary(d => d.MaSV.Trim().ToLower());

                var cacDong = File.ReadAllLines(hopThoai.FileName, Encoding.UTF8);
                int soDongKhop = 0;

                // Bỏ qua dòng tiêu đề đầu tiên; mỗi dòng còn lại: MaSV;...;DiemGiuaKy;DiemCuoiKy
                // (2 cột điểm luôn là 2 cột cuối, để tương thích cả khi file có/không có cột Họ tên)
                foreach (var dongText in cacDong.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(dongText))
                        continue;

                    var cot = dongText.Split(';');
                    if (cot.Length < 3)
                        continue;

                    string maSV = cot[0].Trim().ToLower();
                    if (!dongTheoMaSV.TryGetValue(maSV, out var dong))
                        continue;

                    dong.DiemGiuaKy = cot[cot.Length - 2].Trim();
                    dong.DiemCuoiKy = cot[cot.Length - 1].Trim();
                    soDongKhop++;
                }

                // Ép DataGrid vẽ lại vì DongNhapDiem không cài INotifyPropertyChanged
                dgDiem.ItemsSource = null;
                dgDiem.ItemsSource = _danhSachDong;

                MessageBox.Show(
                    $"Đã cập nhật {soDongKhop}/{_danhSachDong.Count} sinh viên từ file.\n" +
                    "Kiểm tra lại lưới rồi bấm \"Lưu tất cả\" để ghi vào cơ sở dữ liệu.",
                    "Nhập từ file", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                BaoLoi("Đọc file thất bại: " + ex.Message);
            }
        }

        private void BaoLoi(string thongDiep)
        {
            lblThongBao.Text = thongDiep;
            lblThongBao.Visibility = Visibility.Visible;
        }

        private void AnThongBao()
        {
            lblThongBao.Visibility = Visibility.Collapsed;
        }

        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = _daLuu;
            Close();
        }
    }

    // 1 dòng trên lưới nhập điểm: giữ điểm dạng chuỗi vì đang gõ dở/đang import từ file,
    // chỉ parse + kiểm tra hợp lệ khi bấm Lưu tất cả (giống cách ThemSuaDiemWindow đang làm).
    public class DongNhapDiem
    {
        public int SinhVienId { get; set; }
        public string MaSV { get; set; }
        public string HoTen { get; set; }
        public string DiemGiuaKy { get; set; }
        public string DiemCuoiKy { get; set; }
    }
}
