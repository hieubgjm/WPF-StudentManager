using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuanLySinhVien.Data;
using QuanLySinhVien.Models;

namespace QuanLySinhVien.Repositories
{
    /// <summary>
    /// Quản lý việc sinh viên đăng ký học môn nào, tách riêng khỏi bảng điểm (DiemSo):
    /// phải đăng ký môn trước thì mới được nhập điểm môn đó cho sinh viên.
    /// </summary>
    public class DangKyMonHocRepository
    {
        public List<int> LayDanhSachMonHocIdTheoSinhVien(int sinhVienId)
        {
            using (var db = new AppDbContext())
            {
                return db.DangKyMonHocs
                    .Where(d => d.SinhVienId == sinhVienId)
                    .Select(d => d.MonHocId)
                    .ToList();
            }
        }

        // Danh sách môn học sinh viên đã đăng ký, dùng để lọc combobox môn học ở màn hình Thêm điểm
        public List<MonHoc> LayMonHocDaDangKy(int sinhVienId)
        {
            using (var db = new AppDbContext())
            {
                return db.DangKyMonHocs
                    .Include(d => d.MonHoc)
                    .Where(d => d.SinhVienId == sinhVienId)
                    .Select(d => d.MonHoc)
                    .OrderBy(m => m.TenMonHoc)
                    .ToList();
            }
        }

        // Đồng bộ lại toàn bộ danh sách môn đăng ký của 1 sinh viên theo đúng danh sách
        // monHocIds truyền vào: thêm những môn mới được chọn, hủy những môn bị bỏ chọn.
        // Form ThemSuaSinhVienWindow đã khóa (disable) checkbox của môn đã có điểm nên
        // ở đây không cần kiểm tra lại việc đó.
        public void CapNhatDangKy(int sinhVienId, List<int> monHocIds)
        {
            using (var db = new AppDbContext())
            {
                var hienTai = db.DangKyMonHocs.Where(d => d.SinhVienId == sinhVienId).ToList();

                var canThem = monHocIds.Except(hienTai.Select(d => d.MonHocId));
                foreach (var monHocId in canThem)
                    db.DangKyMonHocs.Add(new DangKyMonHoc { SinhVienId = sinhVienId, MonHocId = monHocId });

                var canXoa = hienTai.Where(d => !monHocIds.Contains(d.MonHocId));
                db.DangKyMonHocs.RemoveRange(canXoa);

                db.SaveChanges();
            }
        }

        public int DemTongSoDangKy()
        {
            using (var db = new AppDbContext())
            {
                return db.DangKyMonHocs.Count();
            }
        }
    }
}
