using System.Collections.Generic;
using System.Linq;
using QuanLySinhVien.Data;
using QuanLySinhVien.Helpers;
using QuanLySinhVien.Models;

namespace QuanLySinhVien.Repositories
{
    /// <summary>
    /// CRUD cho bảng Tài khoản đăng nhập + hàm kiểm tra đăng nhập.
    /// </summary>
    public class TaiKhoanRepository
    {
        // Trả về tài khoản nếu đăng nhập đúng, null nếu sai tên đăng nhập/mật khẩu
        // hoặc tài khoản đang bị khóa.
        public TaiKhoan DangNhap(string tenDangNhap, string matKhauGoc)
        {
            using (var db = new AppDbContext())
            {
                var taiKhoan = db.TaiKhoans.FirstOrDefault(tk => tk.TenDangNhap == tenDangNhap);

                if (taiKhoan == null || !taiKhoan.DangHoatDong)
                    return null;

                if (!MaHoaMatKhau.KiemTra(matKhauGoc, taiKhoan.MatKhauMaHoa))
                    return null;

                return taiKhoan;
            }
        }

        public List<TaiKhoan> LayTatCa()
        {
            using (var db = new AppDbContext())
            {
                return db.TaiKhoans.OrderBy(tk => tk.TenDangNhap).ToList();
            }
        }

        public TaiKhoan LayTheoId(int taiKhoanId)
        {
            using (var db = new AppDbContext())
            {
                return db.TaiKhoans.FirstOrDefault(tk => tk.TaiKhoanId == taiKhoanId);
            }
        }

        public bool DaTonTaiTenDangNhap(string tenDangNhap, int taiKhoanIdBoQua = 0)
        {
            using (var db = new AppDbContext())
            {
                return db.TaiKhoans.Any(tk => tk.TenDangNhap == tenDangNhap && tk.TaiKhoanId != taiKhoanIdBoQua);
            }
        }

        // Thêm tài khoản mới - matKhauGoc là mật khẩu người dùng gõ vào, chưa băm
        public void Them(TaiKhoan taiKhoan, string matKhauGoc)
        {
            taiKhoan.MatKhauMaHoa = MaHoaMatKhau.Bam(matKhauGoc);

            using (var db = new AppDbContext())
            {
                db.TaiKhoans.Add(taiKhoan);
                db.SaveChanges();
            }
        }

        // Sửa thông tin tài khoản, không đổi mật khẩu ở đây (có hàm riêng DatLaiMatKhau)
        public void Sua(TaiKhoan taiKhoan)
        {
            using (var db = new AppDbContext())
            {
                var taiKhoanCu = db.TaiKhoans.FirstOrDefault(tk => tk.TaiKhoanId == taiKhoan.TaiKhoanId);
                if (taiKhoanCu == null) return;

                taiKhoanCu.TenDangNhap = taiKhoan.TenDangNhap;
                taiKhoanCu.HoTen = taiKhoan.HoTen;
                taiKhoanCu.VaiTro = taiKhoan.VaiTro;
                taiKhoanCu.DangHoatDong = taiKhoan.DangHoatDong;

                db.SaveChanges();
            }
        }

        public void DatLaiMatKhau(int taiKhoanId, string matKhauMoi)
        {
            using (var db = new AppDbContext())
            {
                var taiKhoan = db.TaiKhoans.FirstOrDefault(tk => tk.TaiKhoanId == taiKhoanId);
                if (taiKhoan == null) return;

                taiKhoan.MatKhauMaHoa = MaHoaMatKhau.Bam(matKhauMoi);
                db.SaveChanges();
            }
        }

        // Dùng cho màn hình "Đổi mật khẩu" - phải nhập đúng mật khẩu cũ mới cho đổi
        public bool DoiMatKhau(int taiKhoanId, string matKhauCu, string matKhauMoi)
        {
            using (var db = new AppDbContext())
            {
                var taiKhoan = db.TaiKhoans.FirstOrDefault(tk => tk.TaiKhoanId == taiKhoanId);
                if (taiKhoan == null) return false;

                if (!MaHoaMatKhau.KiemTra(matKhauCu, taiKhoan.MatKhauMaHoa))
                    return false;

                taiKhoan.MatKhauMaHoa = MaHoaMatKhau.Bam(matKhauMoi);
                db.SaveChanges();
                return true;
            }
        }

        public void Xoa(int taiKhoanId)
        {
            using (var db = new AppDbContext())
            {
                var taiKhoan = db.TaiKhoans.FirstOrDefault(tk => tk.TaiKhoanId == taiKhoanId);
                if (taiKhoan != null)
                {
                    db.TaiKhoans.Remove(taiKhoan);
                    db.SaveChanges();
                }
            }
        }
    }
}
