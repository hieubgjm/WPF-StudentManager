using System.Collections.Generic;

namespace QuanLySinhVien.Models
{
    /// <summary>
    /// Môn học - dùng để nhập điểm cho sinh viên. VD: Toán cao cấp (3 tín chỉ).
    /// </summary>
    public class MonHoc
    {
        public int MonHocId { get; set; }

        // Mã môn học, gõ tay lúc thêm mới, không được trùng nhau (VD: TOAN01)
        public string MaMonHoc { get; set; }

        public string TenMonHoc { get; set; }

        // Số tín chỉ của môn, dùng để tính GPA có trọng số cho sinh viên
        public int SoTinChi { get; set; }

        public string GhiChu { get; set; }

        // 1 môn học có nhiều dòng điểm (mỗi dòng của 1 sinh viên)
        public List<DiemSo> DanhSachDiem { get; set; } = new List<DiemSo>();
    }
}
