using System.ComponentModel.DataAnnotations.Schema;
using QuanLySinhVien.Helpers;

namespace QuanLySinhVien.Models
{
    /// <summary>
    /// 1 dòng điểm của 1 sinh viên ứng với 1 môn học: điểm giữa kỳ, điểm cuối kỳ.
    /// Điểm tổng kết không lưu xuống DB mà tự tính lại từ 2 điểm trên (xem DiemTongKet),
    /// để tránh trường hợp dữ liệu lưu sẵn bị lệch khi công thức tính thay đổi sau này.
    /// </summary>
    public class DiemSo
    {
        public int DiemSoId { get; set; }

        public int SinhVienId { get; set; }
        public SinhVien SinhVien { get; set; }

        public int MonHocId { get; set; }
        public MonHoc MonHoc { get; set; }

        // Thang điểm 10, cho phép null vì có thể chưa nhập đủ 2 cột điểm
        public double? DiemGiuaKy { get; set; }

        public double? DiemCuoiKy { get; set; }

        // [NotMapped]: không tạo cột trong DB, chỉ tính toán khi cần dùng
        [NotMapped]
        public double? DiemTongKet => XepLoaiHelper.TinhDiemTongKet(DiemGiuaKy, DiemCuoiKy);
    }
}
