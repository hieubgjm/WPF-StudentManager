namespace QuanLySinhVien.Models
{
    /// <summary>
    /// 1 dòng đăng ký: sinh viên được phép học (và sau này nhập điểm) môn học này.
    /// Tách riêng khỏi DiemSo vì việc đăng ký môn có thể có trước khi có điểm.
    /// </summary>
    public class DangKyMonHoc
    {
        public int DangKyMonHocId { get; set; }

        public int SinhVienId { get; set; }
        public SinhVien SinhVien { get; set; }

        public int MonHocId { get; set; }
        public MonHoc MonHoc { get; set; }
    }
}
