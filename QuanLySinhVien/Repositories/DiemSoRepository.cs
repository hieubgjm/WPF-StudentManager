using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuanLySinhVien.Data;
using QuanLySinhVien.Helpers;
using QuanLySinhVien.Models;

namespace QuanLySinhVien.Repositories
{
    /// <summary>
    /// CRUD điểm số của sinh viên, kèm hàm tính GPA dựa trên toàn bộ điểm đã nhập.
    /// </summary>
    public class DiemSoRepository
    {
        // Lấy toàn bộ dòng điểm của 1 sinh viên, kèm thông tin Môn học để hiển thị
        // tên môn/số tín chỉ và để tính GPA có trọng số.
        public List<DiemSo> LayTheoSinhVien(int sinhVienId)
        {
            using (var db = new AppDbContext())
            {
                return db.DiemSos
                    .Include(d => d.MonHoc)
                    .Where(d => d.SinhVienId == sinhVienId)
                    .OrderBy(d => d.MonHoc.TenMonHoc)
                    .ToList();
            }
        }

        public DiemSo LayTheoId(int diemSoId)
        {
            using (var db = new AppDbContext())
            {
                return db.DiemSos
                    .Include(d => d.MonHoc)
                    .FirstOrDefault(d => d.DiemSoId == diemSoId);
            }
        }

        // Kiểm tra sinh viên đã có điểm môn này chưa, dùng để chặn nhập trùng môn.
        // diemSoIdBoQua: bỏ qua chính dòng điểm đang sửa (vì lúc sửa vẫn giữ nguyên môn cũ).
        public bool DaCoDiemMonHoc(int sinhVienId, int monHocId, int diemSoIdBoQua = 0)
        {
            using (var db = new AppDbContext())
            {
                return db.DiemSos.Any(d =>
                    d.SinhVienId == sinhVienId &&
                    d.MonHocId == monHocId &&
                    d.DiemSoId != diemSoIdBoQua);
            }
        }

        public void Them(DiemSo diemSo)
        {
            using (var db = new AppDbContext())
            {
                db.DiemSos.Add(diemSo);
                db.SaveChanges();
            }
        }

        public void Sua(DiemSo diemSo)
        {
            using (var db = new AppDbContext())
            {
                db.DiemSos.Update(diemSo);
                db.SaveChanges();
            }
        }

        public void Xoa(int diemSoId)
        {
            using (var db = new AppDbContext())
            {
                var diemSo = db.DiemSos.FirstOrDefault(d => d.DiemSoId == diemSoId);
                if (diemSo != null)
                {
                    db.DiemSos.Remove(diemSo);
                    db.SaveChanges();
                }
            }
        }

        // GPA của 1 sinh viên, tính có trọng số theo số tín chỉ (xem XepLoaiHelper).
        // Trả về null nếu sinh viên chưa có môn nào được nhập đủ điểm giữa kỳ + cuối kỳ.
        public double? TinhGpa(int sinhVienId)
        {
            return XepLoaiHelper.TinhGpa(LayTheoSinhVien(sinhVienId));
        }

        // Lấy điểm môn học "monHocId" của toàn bộ sinh viên thuộc lớp "lopId",
        // dùng cho màn hình nhập bảng điểm hàng loạt theo lớp.
        public List<DiemSo> LayTheoLopVaMon(int lopId, int monHocId)
        {
            using (var db = new AppDbContext())
            {
                return db.DiemSos
                    .Where(d => d.MonHocId == monHocId && d.SinhVien.LopId == lopId)
                    .ToList();
            }
        }

        // Lưu hàng loạt điểm môn "monHocId" cho nhiều sinh viên cùng lúc: sinh viên nào
        // đã có điểm môn này thì cập nhật, chưa có thì thêm mới. Dòng nào cả 2 cột điểm
        // đều để trống thì bỏ qua, không tạo dòng điểm rỗng.
        public void LuuHangLoat(int monHocId, List<DiemNhapDto> danhSach)
        {
            using (var db = new AppDbContext())
            {
                var daCo = db.DiemSos
                    .Where(d => d.MonHocId == monHocId)
                    .ToDictionary(d => d.SinhVienId);

                foreach (var dto in danhSach)
                {
                    if (dto.DiemGiuaKy == null && dto.DiemCuoiKy == null)
                        continue;

                    if (daCo.TryGetValue(dto.SinhVienId, out var diemCu))
                    {
                        diemCu.DiemGiuaKy = dto.DiemGiuaKy;
                        diemCu.DiemCuoiKy = dto.DiemCuoiKy;
                    }
                    else
                    {
                        db.DiemSos.Add(new DiemSo
                        {
                            SinhVienId = dto.SinhVienId,
                            MonHocId = monHocId,
                            DiemGiuaKy = dto.DiemGiuaKy,
                            DiemCuoiKy = dto.DiemCuoiKy
                        });
                    }
                }

                db.SaveChanges();
            }
        }
    }

    // 1 dòng trong màn "Nhập bảng điểm theo lớp": gộp thông tin sinh viên với điểm
    // giữa kỳ/cuối kỳ của môn đang chọn (điểm có thể lấy từ dữ liệu cũ hoặc đang gõ dở).
    public class DiemNhapDto
    {
        public int SinhVienId { get; set; }
        public string MaSV { get; set; }
        public string HoTen { get; set; }
        public double? DiemGiuaKy { get; set; }
        public double? DiemCuoiKy { get; set; }
    }
}
