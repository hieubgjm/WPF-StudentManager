using System.Collections.Generic;
using QuanLySinhVien.Models;

namespace QuanLySinhVien.Helpers
{
    /// <summary>
    /// Gom hết công thức tính điểm tổng kết, GPA và xếp loại vào 1 chỗ duy nhất
    /// để Repository/Control nào cũng dùng chung, khỏi lặp lại công thức nhiều nơi.
    /// </summary>
    public static class XepLoaiHelper
    {
        // Trọng số điểm giữa kỳ/cuối kỳ khi tính điểm tổng kết 1 môn.
        // Đổi 2 số này nếu trường bạn quy định tỉ lệ khác.
        private const double TrongSoGiuaKy = 0.3;
        private const double TrongSoCuoiKy = 0.7;

        // Điểm tổng kết 1 môn = giữa kỳ * 30% + cuối kỳ * 70%.
        // Chỉ tính được khi cả 2 điểm đã có, thiếu điểm nào thì trả về null.
        public static double? TinhDiemTongKet(double? diemGiuaKy, double? diemCuoiKy)
        {
            if (diemGiuaKy == null || diemCuoiKy == null)
                return null;

            double tongKet = diemGiuaKy.Value * TrongSoGiuaKy + diemCuoiKy.Value * TrongSoCuoiKy;
            return System.Math.Round(tongKet, 2);
        }

        // GPA của 1 sinh viên = trung bình có trọng số theo số tín chỉ của điểm tổng kết
        // các môn đã có đủ điểm (môn chưa có đủ điểm giữa kỳ/cuối kỳ thì bỏ qua, không tính).
        public static double? TinhGpa(IEnumerable<DiemSo> danhSachDiem)
        {
            double tongDiemNhanTinChi = 0;
            int tongTinChi = 0;

            foreach (var diem in danhSachDiem)
            {
                double? diemTongKet = TinhDiemTongKet(diem.DiemGiuaKy, diem.DiemCuoiKy);
                if (diemTongKet == null || diem.MonHoc == null)
                    continue;

                tongDiemNhanTinChi += diemTongKet.Value * diem.MonHoc.SoTinChi;
                tongTinChi += diem.MonHoc.SoTinChi;
            }

            if (tongTinChi == 0)
                return null;

            return System.Math.Round(tongDiemNhanTinChi / tongTinChi, 2);
        }

        // Xếp loại học lực theo thang điểm 10 dựa trên GPA đã tính ở trên.
        public static string XepLoaiTuGpa(double? gpa)
        {
            if (gpa == null) return "Chưa có điểm";
            if (gpa >= 9) return "Xuất sắc";
            if (gpa >= 8) return "Giỏi";
            if (gpa >= 7) return "Khá";
            if (gpa >= 5) return "Trung bình";
            return "Yếu";
        }
    }
}
