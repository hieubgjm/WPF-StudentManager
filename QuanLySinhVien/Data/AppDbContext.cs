using Microsoft.EntityFrameworkCore;
using QuanLySinhVien.Models;

namespace QuanLySinhVien.Data
{
    /// <summary>
    /// DbContext của EF Core - đại diện cho kết nối tới database và các bảng.
    /// Mỗi lần cần đọc/ghi dữ liệu thì tạo mới 1 instance của lớp này (dùng using),
    /// dùng xong là đóng kết nối luôn, không giữ context sống lâu cho nhẹ.
    /// </summary>
    public class AppDbContext : DbContext
    {
        // ĐỔI CHUỖI KẾT NỐI NÀY CHO ĐÚNG VỚI MÁY CỦA BẠN:
        // - Nếu dùng LocalDB đi kèm Visual Studio (mặc định, không cần cài gì thêm):
        //      Server=(localdb)\mssqllocaldb;Database=QuanLySinhVienDB;Trusted_Connection=True;TrustServerCertificate=True;
        // - Nếu dùng SQL Server Express cài riêng:
        //      Server=.\SQLEXPRESS;Database=QuanLySinhVienDB;Trusted_Connection=True;TrustServerCertificate=True;
        // - Nếu SQL Server dùng tài khoản sa/mật khẩu:
        //      Server=.;Database=QuanLySinhVienDB;User Id=sa;Password=matkhau;TrustServerCertificate=True;
        private const string ConnectionString =
            @"Server=(localdb)\mssqllocaldb;Database=QuanLySinhVienDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public DbSet<SinhVien> SinhViens { get; set; }
        public DbSet<Lop> Lops { get; set; }
        public DbSet<CaHoc> CaHocs { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<MonHoc> MonHocs { get; set; }
        public DbSet<DiemSo> DiemSos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);

            // In câu lệnh SQL ra cửa sổ Output của Visual Studio lúc debug,
            // hữu ích khi cần xem EF Core sinh ra câu SQL như thế nào.
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mã sinh viên, mã lớp, tên đăng nhập là những giá trị không được phép trùng
            modelBuilder.Entity<SinhVien>()
                .HasIndex(sv => sv.MaSV)
                .IsUnique();

            modelBuilder.Entity<Lop>()
                .HasIndex(l => l.MaLop)
                .IsUnique();

            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(tk => tk.TenDangNhap)
                .IsUnique();

            modelBuilder.Entity<MonHoc>()
                .HasIndex(m => m.MaMonHoc)
                .IsUnique();

            // 1 sinh viên chỉ có tối đa 1 dòng điểm cho mỗi môn học
            modelBuilder.Entity<DiemSo>()
                .HasIndex(d => new { d.SinhVienId, d.MonHocId })
                .IsUnique();

            // Sinh viên xóa Lớp thì không được xóa luôn sinh viên trong lớp đó (tránh mất dữ liệu),
            // mình sẽ tự kiểm tra ở tầng Repository trước khi cho xóa Lớp/Ca học.
            modelBuilder.Entity<SinhVien>()
                .HasOne(sv => sv.Lop)
                .WithMany(l => l.DanhSachSinhVien)
                .HasForeignKey(sv => sv.LopId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SinhVien>()
                .HasOne(sv => sv.CaHoc)
                .WithMany(c => c.DanhSachSinhVien)
                .HasForeignKey(sv => sv.CaHocId)
                .OnDelete(DeleteBehavior.Restrict);

            // Điểm là dữ liệu con phụ thuộc vào sinh viên (khác với Lớp/Ca học là dữ liệu
            // tham chiếu), nên xóa sinh viên thì cho xóa cascade luôn toàn bộ điểm của SV đó.
            modelBuilder.Entity<DiemSo>()
                .HasOne(d => d.SinhVien)
                .WithMany(sv => sv.DanhSachDiem)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.Cascade);

            // Môn học vẫn là dữ liệu tham chiếu dùng chung, không cho xóa nếu còn điểm
            // tham chiếu tới (Repository tự kiểm tra bằng CoTheXoa trước khi xóa).
            modelBuilder.Entity<DiemSo>()
                .HasOne(d => d.MonHoc)
                .WithMany(m => m.DanhSachDiem)
                .HasForeignKey(d => d.MonHocId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
