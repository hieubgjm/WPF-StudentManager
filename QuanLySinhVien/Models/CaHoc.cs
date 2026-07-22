using System;
using System.Collections.Generic;

namespace QuanLySinhVien.Models
{
    /// <summary>
    /// Ca học - khung giờ học trong ngày mà sinh viên đăng ký theo học.
    /// VD: Ca sáng (07:00 - 11:00), Ca chiều (13:00 - 17:00), Ca tối (18:00 - 21:00).
    /// </summary>
    public class CaHoc
    {
        public int CaHocId { get; set; }

        public string TenCa { get; set; }

        // Dùng TimeSpan vì đây là giờ trong ngày, không cần lưu ngày tháng
        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public string GhiChu { get; set; }

        // 1 ca học có thể có nhiều sinh viên đăng ký học
        public List<SinhVien> DanhSachSinhVien { get; set; } = new List<SinhVien>();
    }
}
