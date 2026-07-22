using System;
using System.Collections.Generic;

namespace QuanLySinhVien.Models
{
    /// <summary>
    /// Thông tin 1 sinh viên. Đây là bảng "trung tâm" của cả chương trình,
    /// mỗi sinh viên thuộc về 1 lớp và học theo 1 ca học.
    /// </summary>
    public class SinhVien
    {
        public int SinhVienId { get; set; }

        // Mã số sinh viên do trường cấp, không được trùng. VD: SV0001
        public string MaSV { get; set; }

        public string HoTen { get; set; }

        public DateTime NgaySinh { get; set; }

        // Lưu chuỗi "Nam"/"Nữ" luôn cho đơn giản, khỏi phải enum rồi convert qua lại
        // với ComboBox cho mệt
        public string GioiTinh { get; set; }

        public string DiaChi { get; set; }

        public string SoDienThoai { get; set; }

        public string Email { get; set; }

        // Trạng thái học tập hiện tại: "Đang học" / "Bảo lưu" / "Tốt nghiệp".
        // Lưu chuỗi cho đơn giản, giống cách làm với GioiTinh ở trên.
        public string TrangThai { get; set; } = "Đang học";

        // --- Khóa ngoại tới Lớp ---
        // Cho phép null vì lúc mới thêm sinh viên có thể chưa xếp lớp ngay
        public int? LopId { get; set; }
        public Lop Lop { get; set; }

        // --- Khóa ngoại tới Ca học ---
        public int? CaHocId { get; set; }
        public CaHoc CaHoc { get; set; }

        // 1 sinh viên có nhiều dòng điểm, mỗi dòng ứng với 1 môn học
        public List<DiemSo> DanhSachDiem { get; set; } = new List<DiemSo>();
    }
}
