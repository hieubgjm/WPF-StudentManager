using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    /// <summary>
    /// Màn hình "Nhập sinh viên từ file CSV": đọc file, hiện lưới xem trước để người
    /// dùng sửa nếu cần, kiểm tra hợp lệ từng dòng rồi thêm hàng loạt những dòng hợp lệ.
    /// Dòng nào lỗi thì bị bỏ qua và ghi rõ lý do ở cột "Kết quả kiểm tra", không chặn
    /// các dòng còn lại.
    /// </summary>
    public partial class NhapSinhVienTuFileWindow : Window
    {
        private readonly SinhVienRepository _sinhVienRepo = new SinhVienRepository();
        private readonly LopRepository _lopRepo = new LopRepository();
        private readonly CaHocRepository _caHocRepo = new CaHocRepository();

        private List<DongNhapSinhVien> _danhSach;
        private bool _daThem;

        public NhapSinhVienTuFileWindow()
        {
            InitializeComponent();
        }

        private void btnChonFile_Click(object sender, RoutedEventArgs e)
        {
            var hopThoai = new OpenFileDialog { Filter = "Tệp CSV (*.csv)|*.csv" };
            if (hopThoai.ShowDialog() != true)
                return;

            try
            {
                var cacDong = File.ReadAllLines(hopThoai.FileName, Encoding.UTF8);

                _danhSach = cacDong.Skip(1)
                    .Where(dong => !string.IsNullOrWhiteSpace(dong))
                    .Select(ParseDong)
                    .ToList();

                dgDanhSach.ItemsSource = _danhSach;
                AnThongBao();

                if (_danhSach.Count == 0)
                    BaoLoi("File không có dòng dữ liệu nào (dòng đầu tiên được coi là tiêu đề).");
            }
            catch (Exception ex)
            {
                BaoLoi("Đọc file thất bại: " + ex.Message);
            }
        }

        private static DongNhapSinhVien ParseDong(string dongText)
        {
            var cot = dongText.Split(';');
            return new DongNhapSinhVien
            {
                MaSV = LayCot(cot, 0),
                HoTen = LayCot(cot, 1),
                NgaySinh = LayCot(cot, 2),
                GioiTinh = LayCot(cot, 3),
                DiaChi = LayCot(cot, 4),
                SoDienThoai = LayCot(cot, 5),
                Email = LayCot(cot, 6),
                MaLop = LayCot(cot, 7),
                TenCaHoc = LayCot(cot, 8),
                TrangThai = LayCot(cot, 9),
                KetQua = "Chưa kiểm tra"
            };
        }

        private static string LayCot(string[] cot, int chiSo) => chiSo < cot.Length ? cot[chiSo].Trim() : "";

        private void btnXuatMau_Click(object sender, RoutedEventArgs e)
        {
            var hopThoai = new SaveFileDialog
            {
                Filter = "Tệp CSV (*.csv)|*.csv",
                FileName = "MauNhapSinhVien.csv"
            };
            if (hopThoai.ShowDialog() != true)
                return;

            try
            {
                var maLopMauVaTenCaMau = LayMaLopVaTenCaMau();

                var dong = new List<string>
                {
                    "MaSV;HoTen;NgaySinh;GioiTinh;DiaChi;SoDienThoai;Email;MaLop;TenCaHoc;TrangThai",
                    $"SV0001;Nguyễn Văn A;01/01/2003;Nam;Hà Nội;0912345678;a@example.com;{maLopMauVaTenCaMau.Item1};{maLopMauVaTenCaMau.Item2};Đang học"
                };

                File.WriteAllLines(hopThoai.FileName, dong, Encoding.UTF8);

                MessageBox.Show(
                    "Đã xuất file mẫu. Mở bằng Excel để điền danh sách sinh viên rồi dùng nút \"Chọn file CSV...\" để đọc lại.",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                BaoLoi("Xuất file thất bại: " + ex.Message);
            }
        }

        // Lấy tạm 1 mã lớp + 1 tên ca học đang có sẵn (nếu có) để điền vào dòng ví dụ
        // trong file mẫu, giúp người dùng biết đúng định dạng cần gõ là gì.
        private Tuple<string, string> LayMaLopVaTenCaMau()
        {
            string maLop = _lopRepo.LayTatCa().FirstOrDefault()?.MaLop ?? "";
            string tenCa = _caHocRepo.LayTatCa().FirstOrDefault()?.TenCa ?? "";
            return Tuple.Create(maLop, tenCa);
        }

        private void btnNhapTatCa_Click(object sender, RoutedEventArgs e)
        {
            if (_danhSach == null || _danhSach.Count == 0)
            {
                BaoLoi("Vui lòng chọn file CSV trước.");
                return;
            }

            var lopTheoMa = _lopRepo.LayTatCa()
                .ToDictionary(l => l.MaLop.Trim().ToLower());
            var caHocTheoTen = _caHocRepo.LayTatCa()
                .ToDictionary(c => c.TenCa.Trim().ToLower());
            var maSVDaXuLy = _sinhVienRepo.LayTatCaMaSV();

            var danhSachThem = new List<SinhVien>();

            foreach (var dong in _danhSach)
            {
                if (KiemTraVaTaoSinhVien(dong, lopTheoMa, caHocTheoTen, maSVDaXuLy, out var sinhVien))
                {
                    danhSachThem.Add(sinhVien);
                }
            }

            // Ép DataGrid vẽ lại cột "Kết quả kiểm tra" vì DongNhapSinhVien không cài INotifyPropertyChanged
            dgDanhSach.ItemsSource = null;
            dgDanhSach.ItemsSource = _danhSach;

            if (danhSachThem.Count == 0)
            {
                BaoLoi("Không có dòng nào hợp lệ để thêm. Xem cột \"Kết quả kiểm tra\" để biết lý do.");
                return;
            }

            try
            {
                _sinhVienRepo.ThemHangLoat(danhSachThem);
                _daThem = true;
                AnThongBao();

                int soLoi = _danhSach.Count - danhSachThem.Count;
                string thongDiep = $"Đã thêm {danhSachThem.Count} sinh viên.";
                if (soLoi > 0)
                    thongDiep += $"\nBỏ qua {soLoi} dòng lỗi (xem cột \"Kết quả kiểm tra\").";

                MessageBox.Show(thongDiep, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                BaoLoi("Lưu dữ liệu thất bại: " + ex.Message);
            }
        }

        // Kiểm tra hợp lệ 1 dòng và dựng đối tượng SinhVien tương ứng nếu hợp lệ.
        // maSVDaXuLy dùng để chặn trùng mã SV cả với DB lẫn với các dòng khác trong file.
        private bool KiemTraVaTaoSinhVien(DongNhapSinhVien dong, Dictionary<string, Lop> lopTheoMa,
            Dictionary<string, CaHoc> caHocTheoTen, HashSet<string> maSVDaXuLy, out SinhVien sinhVien)
        {
            sinhVien = null;

            if (string.IsNullOrWhiteSpace(dong.MaSV))
                return DanhDauLoi(dong, "Thiếu mã SV");

            string maSVChuan = dong.MaSV.Trim();

            if (maSVDaXuLy.Contains(maSVChuan))
                return DanhDauLoi(dong, "Trùng mã SV (đã có trong hệ thống hoặc trùng dòng khác trong file)");

            if (string.IsNullOrWhiteSpace(dong.HoTen))
                return DanhDauLoi(dong, "Thiếu họ tên");

            if (!DateTime.TryParseExact(dong.NgaySinh, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime ngaySinh))
                return DanhDauLoi(dong, "Ngày sinh không đúng định dạng dd/MM/yyyy");

            string gioiTinh = dong.GioiTinh?.Trim();
            if (gioiTinh != "Nam" && gioiTinh != "Nữ")
                return DanhDauLoi(dong, "Giới tính phải là \"Nam\" hoặc \"Nữ\"");

            if (!string.IsNullOrWhiteSpace(dong.Email) &&
                !Regex.IsMatch(dong.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return DanhDauLoi(dong, "Email không đúng định dạng");

            if (!string.IsNullOrWhiteSpace(dong.SoDienThoai) &&
                !Regex.IsMatch(dong.SoDienThoai.Trim(), @"^[0-9]{9,11}$"))
                return DanhDauLoi(dong, "Số điện thoại phải là 9-11 chữ số");

            int? lopId = null;
            if (!string.IsNullOrWhiteSpace(dong.MaLop))
            {
                if (!lopTheoMa.TryGetValue(dong.MaLop.Trim().ToLower(), out var lop))
                    return DanhDauLoi(dong, $"Không tìm thấy lớp có mã \"{dong.MaLop}\"");
                lopId = lop.LopId;
            }

            int? caHocId = null;
            if (!string.IsNullOrWhiteSpace(dong.TenCaHoc))
            {
                if (!caHocTheoTen.TryGetValue(dong.TenCaHoc.Trim().ToLower(), out var caHoc))
                    return DanhDauLoi(dong, $"Không tìm thấy ca học \"{dong.TenCaHoc}\"");
                caHocId = caHoc.CaHocId;
            }

            string trangThai = string.IsNullOrWhiteSpace(dong.TrangThai) ? "Đang học" : dong.TrangThai.Trim();
            if (trangThai != "Đang học" && trangThai != "Bảo lưu" && trangThai != "Tốt nghiệp")
                return DanhDauLoi(dong, "Trạng thái phải là \"Đang học\", \"Bảo lưu\" hoặc \"Tốt nghiệp\"");

            maSVDaXuLy.Add(maSVChuan);
            dong.KetQua = "Hợp lệ - đã thêm";

            sinhVien = new SinhVien
            {
                MaSV = maSVChuan,
                HoTen = dong.HoTen.Trim(),
                NgaySinh = ngaySinh,
                GioiTinh = gioiTinh,
                DiaChi = string.IsNullOrWhiteSpace(dong.DiaChi) ? null : dong.DiaChi.Trim(),
                SoDienThoai = string.IsNullOrWhiteSpace(dong.SoDienThoai) ? null : dong.SoDienThoai.Trim(),
                Email = string.IsNullOrWhiteSpace(dong.Email) ? null : dong.Email.Trim(),
                LopId = lopId,
                CaHocId = caHocId,
                TrangThai = trangThai
            };
            return true;
        }

        private static bool DanhDauLoi(DongNhapSinhVien dong, string loi)
        {
            dong.KetQua = loi;
            return false;
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
            DialogResult = _daThem;
            Close();
        }
    }

    // 1 dòng xem trước trong màn "Nhập sinh viên từ file": giữ nguyên dạng chuỗi thô
    // đọc từ CSV, cho phép sửa trên lưới trước khi kiểm tra + thêm vào hệ thống.
    public class DongNhapSinhVien
    {
        public string MaSV { get; set; }
        public string HoTen { get; set; }
        public string NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string MaLop { get; set; }
        public string TenCaHoc { get; set; }
        public string TrangThai { get; set; }
        public string KetQua { get; set; }
    }
}
