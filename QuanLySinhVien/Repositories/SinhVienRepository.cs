using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuanLySinhVien.Data;
using QuanLySinhVien.Models;

namespace QuanLySinhVien.Repositories
{
    /// <summary>
    /// CRUD cho bảng Sinh viên, kèm tìm kiếm/lọc theo tên, mã SV, lớp.
    /// </summary>
    public class SinhVienRepository
    {
        // tuKhoa: gõ tìm theo mã SV hoặc họ tên (không phân biệt hoa thường)
        // lopIdLoc: truyền 0 nghĩa là "Tất cả lớp", khác 0 thì chỉ lấy sinh viên của lớp đó
        public List<SinhVien> Tim(string tuKhoa, int lopIdLoc)
        {
            using (var db = new AppDbContext())
            {
                var query = db.SinhViens
                    .Include(sv => sv.Lop)
                    .Include(sv => sv.CaHoc)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    tuKhoa = tuKhoa.Trim().ToLower();
                    query = query.Where(sv =>
                        sv.MaSV.ToLower().Contains(tuKhoa) ||
                        sv.HoTen.ToLower().Contains(tuKhoa));
                }

                if (lopIdLoc > 0)
                {
                    query = query.Where(sv => sv.LopId == lopIdLoc);
                }

                return query.OrderBy(sv => sv.HoTen).ToList();
            }
        }

        public SinhVien LayTheoId(int sinhVienId)
        {
            using (var db = new AppDbContext())
            {
                return db.SinhViens
                    .Include(sv => sv.Lop)
                    .Include(sv => sv.CaHoc)
                    .FirstOrDefault(sv => sv.SinhVienId == sinhVienId);
            }
        }

        public bool DaTonTaiMaSV(string maSV, int sinhVienIdBoQua = 0)
        {
            using (var db = new AppDbContext())
            {
                return db.SinhViens.Any(sv => sv.MaSV == maSV && sv.SinhVienId != sinhVienIdBoQua);
            }
        }

        // Đếm sĩ số hiện tại của 1 lớp, dùng để cảnh báo khi thêm sinh viên vượt sĩ số tối đa
        public int DemSiSoLop(int lopId)
        {
            using (var db = new AppDbContext())
            {
                return db.SinhViens.Count(sv => sv.LopId == lopId);
            }
        }

        public void Them(SinhVien sinhVien)
        {
            using (var db = new AppDbContext())
            {
                db.SinhViens.Add(sinhVien);
                db.SaveChanges();
            }
        }

        public void Sua(SinhVien sinhVien)
        {
            using (var db = new AppDbContext())
            {
                db.SinhViens.Update(sinhVien);
                db.SaveChanges();
            }
        }

        public void Xoa(int sinhVienId)
        {
            using (var db = new AppDbContext())
            {
                var sv = db.SinhViens.FirstOrDefault(x => x.SinhVienId == sinhVienId);
                if (sv != null)
                {
                    db.SinhViens.Remove(sv);
                    db.SaveChanges();
                }
            }
        }

        // Dùng cho màn hình Thống kê: đếm số sinh viên theo từng lớp
        public List<ThongKeSiSoLop> ThongKeSiSoTheoLop()
        {
            using (var db = new AppDbContext())
            {
                return db.Lops
                    .Select(l => new ThongKeSiSoLop
                    {
                        TenLop = l.TenLop,
                        SiSoToiDa = l.SiSoToiDa,
                        SiSoHienTai = l.DanhSachSinhVien.Count
                    })
                    .OrderBy(x => x.TenLop)
                    .ToList();
            }
        }
    }

    // Lớp phụ trợ chỉ dùng để hiển thị lên DataGrid ở màn hình Thống kê,
    // không cần lưu xuống database nên để chung file cho gọn.
    public class ThongKeSiSoLop
    {
        public string TenLop { get; set; }
        public int SiSoToiDa { get; set; }
        public int SiSoHienTai { get; set; }
    }
}
