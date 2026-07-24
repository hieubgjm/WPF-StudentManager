using System;
using System.Collections.Generic;
using QuanLySinhVien.Models;
using QuanLySinhVien.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QuanLySinhVien.Helpers
{
    /// <summary>
    /// Xuất PDF cho 2 màn hình liên quan tới điểm: phiếu điểm của 1 sinh viên
    /// (từ màn "Bảng điểm") và bảng điểm cả lớp theo 1 môn (từ màn "Nhập bảng điểm theo lớp").
    /// </summary>
    public static class PdfHelper
    {
        public static void XuatPhieuDiemCaNhan(string duongDanFile, SinhVien sinhVien,
            List<DiemSo> danhSachDiem, double? gpa, string xepLoai)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("PHIẾU ĐIỂM").FontSize(18).Bold();
                        col.Item().PaddingTop(10).Text($"Mã SV: {sinhVien.MaSV}      Họ và tên: {sinhVien.HoTen}");
                        col.Item().PaddingTop(2).Text(
                            $"Lớp: {sinhVien.Lop?.TenLop ?? "Chưa xếp lớp"}      Ngày sinh: {sinhVien.NgaySinh:dd/MM/yyyy}");
                    });

                    page.Content().PaddingVertical(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(OTieuDe).Text("Môn học");
                            header.Cell().Element(OTieuDe).Text("Tín chỉ");
                            header.Cell().Element(OTieuDe).Text("Giữa kỳ");
                            header.Cell().Element(OTieuDe).Text("Cuối kỳ");
                            header.Cell().Element(OTieuDe).Text("Tổng kết");
                        });

                        foreach (var diem in danhSachDiem)
                        {
                            table.Cell().Element(O).Text(diem.MonHoc?.TenMonHoc ?? "");
                            table.Cell().Element(O).AlignCenter().Text(diem.MonHoc?.SoTinChi.ToString() ?? "");
                            table.Cell().Element(O).AlignCenter().Text(diem.DiemGiuaKy?.ToString("N1") ?? "--");
                            table.Cell().Element(O).AlignCenter().Text(diem.DiemCuoiKy?.ToString("N1") ?? "--");
                            table.Cell().Element(O).AlignCenter().Text(diem.DiemTongKet?.ToString("N2") ?? "--");
                        }
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().Text(x =>
                        {
                            x.Span("GPA: ").Bold();
                            x.Span(gpa.HasValue ? gpa.Value.ToString("N2") : "--");
                            x.Span("      Xếp loại: ").Bold();
                            x.Span(xepLoai);
                        });
                        col.Item().PaddingTop(6).Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                        col.Item().AlignCenter().PaddingTop(4).Text(x =>
                        {
                            x.Span("Trang ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                    });
                });
            }).GeneratePdf(duongDanFile);
        }

        public static void XuatBangDiemLop(string duongDanFile, Lop lop, MonHoc monHoc,
            List<(string MaSV, string HoTen, double? DiemGiuaKy, double? DiemCuoiKy)> danhSach)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("BẢNG ĐIỂM").FontSize(18).Bold();
                        col.Item().PaddingTop(10).Text(
                            $"Lớp: {lop.TenLop}      Môn học: {monHoc.TenMonHoc} ({monHoc.SoTinChi} tín chỉ)");
                    });

                    page.Content().PaddingVertical(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(OTieuDe).Text("STT");
                            header.Cell().Element(OTieuDe).Text("Mã SV");
                            header.Cell().Element(OTieuDe).Text("Họ và tên");
                            header.Cell().Element(OTieuDe).Text("Giữa kỳ");
                            header.Cell().Element(OTieuDe).Text("Cuối kỳ");
                            header.Cell().Element(OTieuDe).Text("Tổng kết");
                        });

                        int stt = 1;
                        foreach (var dong in danhSach)
                        {
                            double? tongKet = XepLoaiHelper.TinhDiemTongKet(dong.DiemGiuaKy, dong.DiemCuoiKy);

                            table.Cell().Element(O).AlignCenter().Text(stt.ToString());
                            table.Cell().Element(O).Text(dong.MaSV);
                            table.Cell().Element(O).Text(dong.HoTen);
                            table.Cell().Element(O).AlignCenter().Text(dong.DiemGiuaKy?.ToString("N1") ?? "--");
                            table.Cell().Element(O).AlignCenter().Text(dong.DiemCuoiKy?.ToString("N1") ?? "--");
                            table.Cell().Element(O).AlignCenter().Text(tongKet?.ToString("N2") ?? "--");
                            stt++;
                        }
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                        col.Item().AlignCenter().PaddingTop(4).Text(x =>
                        {
                            x.Span("Trang ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                    });
                });
            }).GeneratePdf(duongDanFile);
        }

        public static void XuatDanhSachSinhVien(string duongDanFile, List<SinhVienHienThi> danhSach)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(28);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("DANH SÁCH SINH VIÊN").FontSize(16).Bold();
                        col.Item().PaddingTop(4).Text($"Tổng số: {danhSach.Count} sinh viên");
                    });

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(28);
                            columns.RelativeColumn(1.4f);
                            columns.RelativeColumn(2.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(0.9f);
                            columns.RelativeColumn(1.4f);
                            columns.RelativeColumn(1.8f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(0.8f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.4f);
                            columns.RelativeColumn(2.2f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(OTieuDe).Text("STT");
                            header.Cell().Element(OTieuDe).Text("Mã SV");
                            header.Cell().Element(OTieuDe).Text("Họ và tên");
                            header.Cell().Element(OTieuDe).Text("Ngày sinh");
                            header.Cell().Element(OTieuDe).Text("Giới tính");
                            header.Cell().Element(OTieuDe).Text("Lớp");
                            header.Cell().Element(OTieuDe).Text("Khoa");
                            header.Cell().Element(OTieuDe).Text("Trạng thái");
                            header.Cell().Element(OTieuDe).Text("GPA");
                            header.Cell().Element(OTieuDe).Text("Xếp loại");
                            header.Cell().Element(OTieuDe).Text("SĐT");
                            header.Cell().Element(OTieuDe).Text("Email");
                        });

                        int stt = 1;
                        foreach (var sv in danhSach)
                        {
                            table.Cell().Element(O).AlignCenter().Text(stt.ToString());
                            table.Cell().Element(O).Text(sv.MaSV);
                            table.Cell().Element(O).Text(sv.HoTen);
                            table.Cell().Element(O).AlignCenter().Text(sv.NgaySinh.ToString("dd/MM/yyyy"));
                            table.Cell().Element(O).AlignCenter().Text(sv.GioiTinh);
                            table.Cell().Element(O).Text(sv.TenLop ?? "--");
                            table.Cell().Element(O).Text(sv.Khoa ?? "--");
                            table.Cell().Element(O).Text(sv.TrangThai);
                            table.Cell().Element(O).AlignCenter().Text(sv.Gpa?.ToString("N2") ?? "--");
                            table.Cell().Element(O).Text(sv.XepLoai ?? "--");
                            table.Cell().Element(O).Text(sv.SoDienThoai ?? "--");
                            table.Cell().Element(O).Text(sv.Email ?? "--");
                            stt++;
                        }
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                        col.Item().AlignCenter().PaddingTop(4).Text(x =>
                        {
                            x.Span("Trang ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                    });
                });
            }).GeneratePdf(duongDanFile);
        }

        public static void XuatBaoCaoThongKe(string duongDanFile, BaoCaoThongKeDto baoCao)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("BÁO CÁO THỐNG KÊ").FontSize(18).Bold();
                        col.Item().AlignCenter().PaddingTop(4)
                            .Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                    });

                    page.Content().PaddingVertical(14).Column(col =>
                    {
                        col.Spacing(14);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Element(OThe).Column(c => TheSoLieu(c, "Tổng số sinh viên", baoCao.TongSinhVien));
                            row.RelativeItem().Element(OThe).Column(c => TheSoLieu(c, "Tổng số lớp", baoCao.TongLop));
                            row.RelativeItem().Element(OThe).Column(c => TheSoLieu(c, "Tổng số ca học", baoCao.TongCaHoc));
                        });

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Element(OThe).Column(c => TheSoLieu(c, "Sinh viên Nam", baoCao.SoNam));
                            row.RelativeItem().Element(OThe).Column(c => TheSoLieu(c, "Sinh viên Nữ", baoCao.SoNu));
                            row.RelativeItem().Element(OThe).Column(c => TheSoLieu(c, "Đang học", baoCao.DangHoc));
                            row.RelativeItem().Element(OThe).Column(c => TheSoLieu(c, "Bảo lưu", baoCao.BaoLuu));
                            row.RelativeItem().Element(OThe).Column(c => TheSoLieu(c, "Tốt nghiệp", baoCao.TotNghiep));
                        });

                        if (baoCao.AnhBieuDoTheoKhoa != null || baoCao.AnhBieuDoTheoKhoaHoc != null)
                        {
                            col.Item().Row(row =>
                            {
                                if (baoCao.AnhBieuDoTheoKhoa != null)
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text("Sinh viên theo Khoa").Bold();
                                        c.Item().PaddingTop(4).Image(baoCao.AnhBieuDoTheoKhoa);
                                    });

                                if (baoCao.AnhBieuDoTheoKhoaHoc != null)
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text("Sinh viên theo Khóa học").Bold();
                                        c.Item().PaddingTop(4).Image(baoCao.AnhBieuDoTheoKhoaHoc);
                                    });
                            });
                        }

                        col.Item().Text("Sĩ số theo từng lớp").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(OTieuDe).Text("Lớp");
                                header.Cell().Element(OTieuDe).Text("Sĩ số hiện tại");
                                header.Cell().Element(OTieuDe).Text("Sĩ số tối đa");
                            });

                            foreach (var lop in baoCao.SiSoTheoLop)
                            {
                                table.Cell().Element(O).Text(lop.TenLop);
                                table.Cell().Element(O).AlignCenter().Text(lop.SiSoHienTai.ToString());
                                table.Cell().Element(O).AlignCenter().Text(lop.SiSoToiDa.ToString());
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf(duongDanFile);
        }

        // Ô tiêu đề bảng: nền xám nhạt, chữ đậm, viền dưới
        private static IContainer OTieuDe(IContainer container) =>
            container.DefaultTextStyle(x => x.Bold())
                .Background(Colors.Grey.Lighten3)
                .BorderBottom(1).BorderColor(Colors.Grey.Darken1)
                .Padding(5);

        // Ô dữ liệu bình thường: chỉ có viền dưới mỏng để phân tách dòng
        private static IContainer O(IContainer container) =>
            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);

        // Khung 1 thẻ số liệu trong báo cáo thống kê: viền mỏng, nền xám rất nhạt
        private static IContainer OThe(IContainer container) =>
            container.Border(1).BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten5)
                .Padding(10);

        private static void TheSoLieu(ColumnDescriptor col, string tieuDe, int giaTri)
        {
            col.Item().Text(tieuDe).FontSize(9).FontColor(Colors.Grey.Darken1);
            col.Item().PaddingTop(2).Text(giaTri.ToString()).FontSize(18).Bold();
        }
    }

    // Toàn bộ số liệu cho báo cáo PDF của Dashboard (màn "Thống kê"). Ảnh biểu đồ được
    // chụp lại từ control WPF đang hiển thị (RenderTargetBitmap) rồi truyền vào dạng PNG,
    // vì QuestPDF không tự vẽ lại được LiveCharts.
    public class BaoCaoThongKeDto
    {
        public int TongSinhVien { get; set; }
        public int TongLop { get; set; }
        public int TongCaHoc { get; set; }
        public int SoNam { get; set; }
        public int SoNu { get; set; }
        public int DangHoc { get; set; }
        public int BaoLuu { get; set; }
        public int TotNghiep { get; set; }
        public List<ThongKeSiSoLop> SiSoTheoLop { get; set; }
        public byte[] AnhBieuDoTheoKhoa { get; set; }
        public byte[] AnhBieuDoTheoKhoaHoc { get; set; }
    }
}
