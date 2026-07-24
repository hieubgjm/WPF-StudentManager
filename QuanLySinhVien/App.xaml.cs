using System.Linq;
using System.Windows;
using QuanLySinhVien.Data;
using QuanLySinhVien.Helpers;
using QuanLySinhVien.Models;
using QuanLySinhVien.Views;

namespace QuanLySinhVien
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Bắt buộc phải khai báo license (miễn phí - Community) trước khi dùng QuestPDF
            // để xuất PDF ở bất kỳ đâu trong app, xem Helpers/PdfHelper.cs
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            // Trước khi cho người dùng làm gì cả, đảm bảo database đã tồn tại
            // và có sẵn ít nhất 1 tài khoản admin để đăng nhập lần đầu.
            try
            {
                using (var db = new AppDbContext())
                {
                    // EnsureCreated: nếu DB chưa có thì tạo mới đúng theo cấu trúc các Model,
                    // nếu đã có rồi thì không làm gì cả. Không cần chạy migration bằng tay.
                    db.Database.EnsureCreated();

                    TaoTaiKhoanAdminMacDinh(db);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    "Không thể kết nối tới SQL Server.\n" +
                    "Kiểm tra lại chuỗi kết nối trong file Data/AppDbContext.cs.\n\n" +
                    "Chi tiết lỗi: " + ex.Message,
                    "Lỗi kết nối database",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
                return;
            }

            // Mọi thứ sẵn sàng, mở màn hình đăng nhập
            var cuaSoDangNhap = new DangNhapWindow();
            cuaSoDangNhap.Show();
        }

        // Nếu database mới tinh, chưa có tài khoản nào thì tự tạo 1 tài khoản admin
        // để lần đầu chạy chương trình còn có cái mà đăng nhập vào.
        private void TaoTaiKhoanAdminMacDinh(AppDbContext db)
        {
            bool chuaCoTaiKhoanNao = !db.TaiKhoans.Any();
            if (!chuaCoTaiKhoanNao)
                return;

            var admin = new TaiKhoan
            {
                TenDangNhap = "admin",
                MatKhauMaHoa = MaHoaMatKhau.Bam("123456"),
                HoTen = "Quản trị viên",
                VaiTro = "Admin",
                DangHoatDong = true
            };

            db.TaiKhoans.Add(admin);
            db.SaveChanges();
        }
    }
}
