using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using Microsoft.Win32;
using QuanLySinhVien.Helpers;
using QuanLySinhVien.Repositories;

namespace QuanLySinhVien.Views
{
    public partial class ThongKeControl : UserControl
    {
        private readonly SinhVienRepository _sinhVienRepo = new SinhVienRepository();
        private readonly LopRepository _lopRepo = new LopRepository();
        private readonly DangKyMonHocRepository _dangKyMonHocRepo = new DangKyMonHocRepository();

        public ThongKeControl()
        {
            InitializeComponent();
            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            var danhSachLop = _lopRepo.LayTatCa();

            lblTongSinhVien.Text = _sinhVienRepo.Tim(tuKhoa: null, lopIdLoc: 0).Count.ToString();
            lblTongLop.Text = danhSachLop.Count.ToString();
            lblTongDangKy.Text = _dangKyMonHocRepo.DemTongSoDangKy().ToString();

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

        private void btnXuatPdf_Click(object sender, RoutedEventArgs e)
        {
            var hopThoai = new SaveFileDialog
            {
                Filter = "Tệp PDF (*.pdf)|*.pdf",
                FileName = $"BaoCaoThongKe_{DateTime.Now:yyyyMMdd}.pdf"
            };
            if (hopThoai.ShowDialog() != true)
                return;

            try
            {
                var demGioiTinh = _sinhVienRepo.DemTheoGioiTinh();
                var demTrangThai = _sinhVienRepo.DemTheoTrangThai();

                var baoCao = new BaoCaoThongKeDto
                {
                    TongSinhVien = _sinhVienRepo.Tim(tuKhoa: null, lopIdLoc: 0).Count,
                    TongLop = _lopRepo.LayTatCa().Count,
                    TongDangKy = _dangKyMonHocRepo.DemTongSoDangKy(),
                    SoNam = demGioiTinh.TryGetValue("Nam", out int soNam) ? soNam : 0,
                    SoNu = demGioiTinh.TryGetValue("Nữ", out int soNu) ? soNu : 0,
                    DangHoc = demTrangThai.TryGetValue("Đang học", out int dangHoc) ? dangHoc : 0,
                    BaoLuu = demTrangThai.TryGetValue("Bảo lưu", out int baoLuu) ? baoLuu : 0,
                    TotNghiep = demTrangThai.TryGetValue("Tốt nghiệp", out int totNghiep) ? totNghiep : 0,
                    SiSoTheoLop = _sinhVienRepo.ThongKeSiSoTheoLop(),
                    AnhBieuDoTheoKhoa = ChupAnh(chartTheoKhoa),
                    AnhBieuDoTheoKhoaHoc = ChupAnh(chartTheoKhoaHoc)
                };

                PdfHelper.XuatBaoCaoThongKe(hopThoai.FileName, baoCao);

                MessageBox.Show("Đã xuất báo cáo thống kê.", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xuất PDF thất bại: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Chụp lại 1 control đang hiển thị (2 biểu đồ cột) thành ảnh PNG để nhúng vào PDF,
        // vì QuestPDF không tự vẽ lại được control WPF/LiveCharts.
        private static byte[] ChupAnh(FrameworkElement element)
        {
            if (element.ActualWidth <= 0 || element.ActualHeight <= 0)
                return null;

            var bitmap = new RenderTargetBitmap(
                (int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(element);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                return ms.ToArray();
            }
        }
    }
}
