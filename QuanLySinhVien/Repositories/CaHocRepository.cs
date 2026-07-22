using System.Collections.Generic;
using System.Linq;
using QuanLySinhVien.Data;
using QuanLySinhVien.Models;

namespace QuanLySinhVien.Repositories
{
    /// <summary>
    /// CRUD cho bảng Ca học.
    /// </summary>
    public class CaHocRepository
    {
        public List<CaHoc> LayTatCa()
        {
            using (var db = new AppDbContext())
            {
                return db.CaHocs
                    .OrderBy(c => c.GioBatDau)
                    .ToList();
            }
        }

        public CaHoc LayTheoId(int caHocId)
        {
            using (var db = new AppDbContext())
            {
                return db.CaHocs.FirstOrDefault(c => c.CaHocId == caHocId);
            }
        }

        public void Them(CaHoc caHoc)
        {
            using (var db = new AppDbContext())
            {
                db.CaHocs.Add(caHoc);
                db.SaveChanges();
            }
        }

        public void Sua(CaHoc caHoc)
        {
            using (var db = new AppDbContext())
            {
                db.CaHocs.Update(caHoc);
                db.SaveChanges();
            }
        }

        // Không cho xóa ca học nếu đang có sinh viên học theo ca đó
        public bool CoTheXoa(int caHocId)
        {
            using (var db = new AppDbContext())
            {
                return !db.SinhViens.Any(sv => sv.CaHocId == caHocId);
            }
        }

        public void Xoa(int caHocId)
        {
            using (var db = new AppDbContext())
            {
                var caHoc = db.CaHocs.FirstOrDefault(c => c.CaHocId == caHocId);
                if (caHoc != null)
                {
                    db.CaHocs.Remove(caHoc);
                    db.SaveChanges();
                }
            }
        }
    }
}
