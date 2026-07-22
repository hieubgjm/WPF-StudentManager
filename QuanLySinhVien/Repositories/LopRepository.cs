using System.Collections.Generic;
using System.Linq;
using QuanLySinhVien.Data;
using QuanLySinhVien.Models;

namespace QuanLySinhVien.Repositories
{
    /// <summary>
    /// Xử lý các thao tác CRUD (Thêm - Sửa - Xóa - Xem) cho bảng Lớp học.
    /// </summary>
    public class LopRepository
    {
        public List<Lop> LayTatCa()
        {
            using (var db = new AppDbContext())
            {
                return db.Lops
                    .OrderBy(l => l.TenLop)
                    .ToList();
            }
        }

        public List<Lop> Tim(string tuKhoa)
        {
            using (var db = new AppDbContext())
            {
                var query = db.Lops.AsQueryable();

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    tuKhoa = tuKhoa.Trim().ToLower();
                    query = query.Where(l =>
                        l.MaLop.ToLower().Contains(tuKhoa) ||
                        l.TenLop.ToLower().Contains(tuKhoa));
                }

                return query.OrderBy(l => l.TenLop).ToList();
            }
        }

        public Lop LayTheoId(int lopId)
        {
            using (var db = new AppDbContext())
            {
                return db.Lops.FirstOrDefault(l => l.LopId == lopId);
            }
        }

        // Kiểm tra trùng mã lớp trước khi thêm/sửa, bỏ qua chính bản ghi đang sửa
        public bool DaTonTaiMaLop(string maLop, int lopIdBoQua = 0)
        {
            using (var db = new AppDbContext())
            {
                return db.Lops.Any(l => l.MaLop == maLop && l.LopId != lopIdBoQua);
            }
        }

        public void Them(Lop lop)
        {
            using (var db = new AppDbContext())
            {
                db.Lops.Add(lop);
                db.SaveChanges();
            }
        }

        public void Sua(Lop lop)
        {
            using (var db = new AppDbContext())
            {
                db.Lops.Update(lop);
                db.SaveChanges();
            }
        }

        // Không cho xóa lớp nếu trong lớp vẫn còn sinh viên, tránh mất liên kết dữ liệu
        public bool CoTheXoa(int lopId)
        {
            using (var db = new AppDbContext())
            {
                return !db.SinhViens.Any(sv => sv.LopId == lopId);
            }
        }

        public void Xoa(int lopId)
        {
            using (var db = new AppDbContext())
            {
                var lop = db.Lops.FirstOrDefault(l => l.LopId == lopId);
                if (lop != null)
                {
                    db.Lops.Remove(lop);
                    db.SaveChanges();
                }
            }
        }
    }
}
