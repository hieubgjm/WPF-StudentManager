using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class ThongKeControl : UserControl
    {
        private readonly SinhVienRepository _sinhVienRepo = new SinhVienRepository();
        private readonly LopRepository _lopRepo = new LopRepository();
        private readonly CaHocRepository _caHocRepo = new CaHocRepository();

        public ThongKeControl()
        {
            InitializeComponent();
            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            var danhSachLop = _lopRepo.LayTatCa();
            var danhSachCaHoc = _caHocRepo.LayTatCa();

            lblTongSinhVien.Text = _sinhVienRepo.Tim(tuKhoa: null, lopIdLoc: 0).Count.ToString();
            lblTongLop.Text = danhSachLop.Count.ToString();
            lblTongCaHoc.Text = danhSachCaHoc.Count.ToString();

            HienThiTheoGioiTinh();
            HienThiTheoTrangThai();

            dgSiSoLop.ItemsSource = _sinhVienRepo.ThongKeSiSoTheoLop();

            VeBieuDoCot(chartTheoKhoa, _sinhVienRepo.ThongKeTheoKhoa());
            VeBieuDoCot(chartTheoKhoaHoc, _sinhVienRepo.ThongKeTheoKhoaHoc());
        }

        // Đếm theo GroupBy trả về Dictionary với khóa đúng chữ "Nam"/"Nữ" đã lưu ở SinhVien.GioiTinh
        private void HienThiTheoGioiTinh()
        {
            var demTheoGioiTinh = _sinhVienRepo.DemTheoGioiTinh();

            lblSoNam.Text = demTheoGioiTinh.TryGetValue("Nam", out int soNam) ? soNam.ToString() : "0";
            lblSoNu.Text = demTheoGioiTinh.TryGetValue("Nữ", out int soNu) ? soNu.ToString() : "0";
        }

        private void HienThiTheoTrangThai()
        {
            var demTheoTrangThai = _sinhVienRepo.DemTheoTrangThai();

            lblDangHoc.Text = demTheoTrangThai.TryGetValue("Đang học", out int dangHoc) ? dangHoc.ToString() : "0";
            lblBaoLuu.Text = demTheoTrangThai.TryGetValue("Bảo lưu", out int baoLuu) ? baoLuu.ToString() : "0";
            lblTotNghiep.Text = demTheoTrangThai.TryGetValue("Tốt nghiệp", out int totNghiep) ? totNghiep.ToString() : "0";
        }

        // Vẽ 1 biểu đồ cột đơn giản: trục X là tên nhóm (Khoa/Khóa học), trục Y là số sinh viên
        private void VeBieuDoCot(CartesianChart chart, List<ThongKeNhom> duLieu)
        {
            chart.Series = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = duLieu.Select(x => x.SoLuong).ToArray(),
                    Name = "Số sinh viên"
                }
            };

            chart.XAxes = new Axis[]
            {
                new Axis { Labels = duLieu.Select(x => x.TenNhom).ToArray() }
            };
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            TaiDuLieu();
        }
    }
}
