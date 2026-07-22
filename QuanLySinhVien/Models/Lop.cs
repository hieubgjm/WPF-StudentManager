using System.Collections.Generic;

namespace QuanLySinhVien.Models
{
    /// <summary>
    /// Đại diện cho 1 lớp học (VD: lớp DHKTPM17A).
    /// Mỗi lớp có thể có nhiều sinh viên theo học.
    /// </summary>
    public class Lop
    {
        public int LopId { get; set; }

        // Mã lớp, gõ tay lúc thêm mới, không được trùng nhau (VD: DHKTPM17A)
        public string MaLop { get; set; }

        public string TenLop { get; set; }

        // Lớp này thuộc khoa nào, ví dụ "Công nghệ thông tin"
        public string Khoa { get; set; }

        // Sĩ số tối đa cho phép của lớp, dùng để cảnh báo khi thêm sinh viên vượt quá
        public int SiSoToiDa { get; set; }

        public string GhiChu { get; set; }

        // 1 lớp - nhiều sinh viên. EF Core tự tạo khóa ngoại SinhVien.LopId dựa vào navigation này
        public List<SinhVien> DanhSachSinhVien { get; set; } = new List<SinhVien>();
    }
}
