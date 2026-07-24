using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuanLySinhVien.Data;
using QuanLySinhVien.Helpers;
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

        // Thêm hàng loạt sinh viên cùng lúc, dùng cho màn hình "Nhập sinh viên từ file".
        public void ThemHangLoat(List<SinhVien> danhSach)
        {
            using (var db = new AppDbContext())
            {
                db.SinhViens.AddRange(danhSach);
                db.SaveChanges();
            }
        }

        // Lấy toàn bộ mã SV hiện có (không phân biệt hoa thường) để kiểm tra trùng
        // hàng loạt khi nhập từ file, tránh phải gọi DaTonTaiMaSV riêng từng dòng.
        public HashSet<string> LayTatCaMaSV()
        {
            using (var db = new AppDbContext())
            {
                return new HashSet<string>(
                    db.SinhViens.Select(sv => sv.MaSV),
                    StringComparer.OrdinalIgnoreCase);
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

        // Tìm kiếm nâng cao: lọc theo nhiều tiêu chí cùng lúc (mã SV/tên, lớp, khoa,
        // giới tính, trạng thái, khoảng ngày sinh, khoảng GPA) và trả về dữ liệu đã
        // gộp sẵn (kèm GPA + xếp loại) để đổ thẳng lên DataGrid.
        public List<SinhVienHienThi> TimNangCao(BoLocSinhVien boLoc)
        {
            using (var db = new AppDbContext())
            {
                var query = db.SinhViens
                    .Include(sv => sv.Lop)
                    .Include(sv => sv.CaHoc)
                    .Include(sv => sv.DanhSachDiem)
                        .ThenInclude(d => d.MonHoc)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(boLoc.TuKhoa))
                {
                    string tuKhoa = boLoc.TuKhoa.Trim().ToLower();
                    query = query.Where(sv =>
                        sv.MaSV.ToLower().Contains(tuKhoa) ||
                        sv.HoTen.ToLower().Contains(tuKhoa));
                }

                if (boLoc.LopId.HasValue && boLoc.LopId.Value > 0)
                {
                    query = query.Where(sv => sv.LopId == boLoc.LopId.Value);
                }

                if (!string.IsNullOrWhiteSpace(boLoc.Khoa))
                {
                    query = query.Where(sv => sv.Lop != null && sv.Lop.Khoa == boLoc.Khoa);
                }

                if (!string.IsNullOrWhiteSpace(boLoc.GioiTinh))
                {
                    query = query.Where(sv => sv.GioiTinh == boLoc.GioiTinh);
                }

                if (!string.IsNullOrWhiteSpace(boLoc.TrangThai))
                {
                    query = query.Where(sv => sv.TrangThai == boLoc.TrangThai);
                }

                if (boLoc.NgaySinhTu.HasValue)
                {
                    query = query.Where(sv => sv.NgaySinh >= boLoc.NgaySinhTu.Value);
                }

                if (boLoc.NgaySinhDen.HasValue)
                {
                    query = query.Where(sv => sv.NgaySinh <= boLoc.NgaySinhDen.Value);
                }

                var danhSach = query.OrderBy(sv => sv.HoTen).ToList();

                // GPA là trung bình có trọng số theo tín chỉ, khó dịch gọn sang câu lệnh SQL
                // nên tính ở bộ nhớ sau khi đã lọc thô các tiêu chí còn lại ở trên.
                var ketQua = danhSach.Select(sv =>
                {
                    double? gpa = XepLoaiHelper.TinhGpa(sv.DanhSachDiem);
                    return new SinhVienHienThi
                    {
                        SinhVienId = sv.SinhVienId,
                        MaSV = sv.MaSV,
                        HoTen = sv.HoTen,
                        NgaySinh = sv.NgaySinh,
                        GioiTinh = sv.GioiTinh,
                        SoDienThoai = sv.SoDienThoai,
                        Email = sv.Email,
                        TenLop = sv.Lop?.TenLop,
                        Khoa = sv.Lop?.Khoa,
                        TenCaHoc = sv.CaHoc?.TenCa,
                        TrangThai = sv.TrangThai,
                        Gpa = gpa,
                        XepLoai = XepLoaiHelper.XepLoaiTuGpa(gpa)
                    };
                }).AsEnumerable();

                if (boLoc.GpaTu.HasValue)
                {
                    ketQua = ketQua.Where(sv => sv.Gpa.HasValue && sv.Gpa.Value >= boLoc.GpaTu.Value);
                }

                if (boLoc.GpaDen.HasValue)
                {
                    ketQua = ketQua.Where(sv => sv.Gpa.HasValue && sv.Gpa.Value <= boLoc.GpaDen.Value);
                }

                return ketQua.ToList();
            }
        }

        // Đếm số sinh viên Nam/Nữ, dùng cho Dashboard
        public Dictionary<string, int> DemTheoGioiTinh()
        {
            using (var db = new AppDbContext())
            {
                return db.SinhViens
                    .GroupBy(sv => sv.GioiTinh)
                    .Select(g => new { GioiTinh = g.Key, SoLuong = g.Count() })
                    .ToDictionary(x => x.GioiTinh ?? "Khác", x => x.SoLuong);
            }
        }

        // Đếm số sinh viên theo trạng thái (Đang học/Bảo lưu/Tốt nghiệp), dùng cho Dashboard
        public Dictionary<string, int> DemTheoTrangThai()
        {
            using (var db = new AppDbContext())
            {
                return db.SinhViens
                    .GroupBy(sv => sv.TrangThai)
                    .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
                    .ToDictionary(x => x.TrangThai ?? "Chưa xác định", x => x.SoLuong);
            }
        }

        // Số sinh viên theo từng Khoa, dùng vẽ biểu đồ Dashboard
        public List<ThongKeNhom> ThongKeTheoKhoa()
        {
            using (var db = new AppDbContext())
            {
                return db.SinhViens
                    .Where(sv => sv.Lop != null && sv.Lop.Khoa != null)
                    .GroupBy(sv => sv.Lop.Khoa)
                    .Select(g => new ThongKeNhom { TenNhom = g.Key, SoLuong = g.Count() })
                    .OrderBy(x => x.TenNhom)
                    .ToList();
            }
        }

        // Số sinh viên theo từng Khóa học, dùng vẽ biểu đồ Dashboard
        public List<ThongKeNhom> ThongKeTheoKhoaHoc()
        {
            using (var db = new AppDbContext())
            {
                return db.SinhViens
                    .Where(sv => sv.Lop != null && sv.Lop.KhoaHoc != null)
                    .GroupBy(sv => sv.Lop.KhoaHoc)
                    .Select(g => new ThongKeNhom { TenNhom = g.Key, SoLuong = g.Count() })
                    .OrderBy(x => x.TenNhom)
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

    // Lớp phụ trợ dùng chung cho các biểu đồ theo nhóm (theo Khoa, theo Khóa học...)
    public class ThongKeNhom
    {
        public string TenNhom { get; set; }
        public int SoLuong { get; set; }
    }

    // Gói toàn bộ tiêu chí của màn hình Tìm kiếm nâng cao vào 1 chỗ, truyền vào
    // TimNangCao thay vì phải khai báo 1 danh sách tham số dài dòng.
    // Để trống (null) tiêu chí nào nghĩa là không lọc theo tiêu chí đó.
    public class BoLocSinhVien
    {
        public string TuKhoa { get; set; }
        public int? LopId { get; set; }
        public string Khoa { get; set; }
        public string GioiTinh { get; set; }
        public string TrangThai { get; set; }
        public DateTime? NgaySinhTu { get; set; }
        public DateTime? NgaySinhDen { get; set; }
        public double? GpaTu { get; set; }
        public double? GpaDen { get; set; }
    }

    // Dòng dữ liệu hiển thị lên DataGrid ở màn hình Sinh viên: gộp thông tin cơ bản
    // của SinhVien với tên Lớp/Khoa/Ca học (thay vì phải bind qua nhiều cấp Lop.TenLop)
    // và GPA/Xếp loại đã tính sẵn.
    public class SinhVienHienThi
    {
        public int SinhVienId { get; set; }
        public string MaSV { get; set; }
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string TenLop { get; set; }
        public string Khoa { get; set; }
        public string TenCaHoc { get; set; }
        public string TrangThai { get; set; }
        public double? Gpa { get; set; }
        public string XepLoai { get; set; }
    }
}
