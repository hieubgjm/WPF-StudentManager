using System.Collections.Generic;
using System.Linq;
using QuanLySinhVien.Data;
using QuanLySinhVien.Models;

namespace QuanLySinhVien.Repositories
{
    /// <summary>
    /// CRUD cho bảng Môn học. Cấu trúc y hệt LopRepository vì cùng là danh mục
    /// tham chiếu đơn giản (mã + tên + vài thuộc tính phụ).
    /// </summary>
    public class MonHocRepository
    {
        public List<MonHoc> LayTatCa()
        {
            using (var db = new AppDbContext())
            {
                return db.MonHocs
                    .OrderBy(m => m.TenMonHoc)
                    .ToList();
            }
        }

        public List<MonHoc> Tim(string tuKhoa)
        {
            using (var db = new AppDbContext())
            {
                var query = db.MonHocs.AsQueryable();

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    tuKhoa = tuKhoa.Trim().ToLower();
                    query = query.Where(m =>
                        m.MaMonHoc.ToLower().Contains(tuKhoa) ||
                        m.TenMonHoc.ToLower().Contains(tuKhoa));
                }

                return query.OrderBy(m => m.TenMonHoc).ToList();
            }
        }

        public MonHoc LayTheoId(int monHocId)
        {
            using (var db = new AppDbContext())
            {
                return db.MonHocs.FirstOrDefault(m => m.MonHocId == monHocId);
            }
        }

        // Kiểm tra trùng mã môn học trước khi thêm/sửa, bỏ qua chính bản ghi đang sửa
        public bool DaTonTaiMaMonHoc(string maMonHoc, int monHocIdBoQua = 0)
        {
            using (var db = new AppDbContext())
            {
                return db.MonHocs.Any(m => m.MaMonHoc == maMonHoc && m.MonHocId != monHocIdBoQua);
            }
        }

        public void Them(MonHoc monHoc)
        {
            using (var db = new AppDbContext())
            {
                db.MonHocs.Add(monHoc);
                db.SaveChanges();
            }
        }

        public void Sua(MonHoc monHoc)
        {
            using (var db = new AppDbContext())
            {
                db.MonHocs.Update(monHoc);
                db.SaveChanges();
            }
        }

        // Không cho xóa môn học nếu đã có sinh viên được nhập điểm cho môn này
        public bool CoTheXoa(int monHocId)
        {
            using (var db = new AppDbContext())
            {
                return !db.DiemSos.Any(d => d.MonHocId == monHocId);
            }
        }

        public void Xoa(int monHocId)
        {
            using (var db = new AppDbContext())
            {
                var monHoc = db.MonHocs.FirstOrDefault(m => m.MonHocId == monHocId);
                if (monHoc != null)
                {
                    db.MonHocs.Remove(monHoc);
                    db.SaveChanges();
                }
            }
        }
    }
}
