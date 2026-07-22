using System;
using System.Security.Cryptography;
using System.Text;

namespace QuanLySinhVien.Helpers
{
    /// <summary>
    /// Hàm băm mật khẩu bằng SHA256. Không lưu mật khẩu dạng chữ thường (plain text)
    /// vào database, lỡ ai đó xem được database thì cũng không đọc được mật khẩu thật.
    /// </summary>
    public static class MaHoaMatKhau
    {
        public static string Bam(string matKhauGoc)
        {
            if (string.IsNullOrEmpty(matKhauGoc))
                return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                byte[] bytesDauVao = Encoding.UTF8.GetBytes(matKhauGoc);
                byte[] bytesDauRa = sha256.ComputeHash(bytesDauVao);

                var sb = new StringBuilder();
                foreach (byte b in bytesDauRa)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // So sánh mật khẩu người dùng gõ vào với mật khẩu (đã băm) lưu trong DB
        public static bool KiemTra(string matKhauGoc, string matKhauDaBam)
        {
            return string.Equals(Bam(matKhauGoc), matKhauDaBam, StringComparison.OrdinalIgnoreCase);
        }
    }
}
