using System.Windows;
using System.Windows.Controls;
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
            var tatCaSinhVien = _sinhVienRepo.Tim(tuKhoa: null, lopIdLoc: 0);

            lblTongSinhVien.Text = tatCaSinhVien.Count.ToString();
            lblTongLop.Text = danhSachLop.Count.ToString();
            lblTongCaHoc.Text = danhSachCaHoc.Count.ToString();

            dgSiSoLop.ItemsSource = _sinhVienRepo.ThongKeSiSoTheoLop();
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            TaiDuLieu();
        }
    }
}
