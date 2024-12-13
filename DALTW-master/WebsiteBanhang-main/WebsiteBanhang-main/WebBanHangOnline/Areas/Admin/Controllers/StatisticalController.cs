using System;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using ClosedXML.Excel;
using System.IO;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using System.IO;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    public class StatisticalController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Statistical
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetStatistical(string fromDate, string toDate)
        {
            var query = from o in db.Orders
                        join od in db.OrderDetails
                        on o.Id equals od.OrderId
                        join p in db.Products
                        on od.ProductId equals p.Id
                        select new
                        {
                            CreatedDate = o.CreatedDate,
                            Quantity = od.Quantity,
                            Price = od.Price,
                            OriginalPrice = p.OriginalPrice
                        };
            if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime startDate = DateTime.ParseExact(fromDate, "dd/MM/yyyy", null);
                query = query.Where(x => x.CreatedDate >= startDate);
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                DateTime endDate = DateTime.ParseExact(toDate, "dd/MM/yyyy", null);
                query = query.Where(x => x.CreatedDate < endDate);
            }

            var result = query.GroupBy(x => DbFunctions.TruncateTime(x.CreatedDate)).Select(x => new
            {
                Date = x.Key.Value,
                TotalBuy = x.Sum(y => y.Quantity * y.OriginalPrice),
                TotalSell = x.Sum(y => y.Quantity * y.Price),
            }).Select(x => new
            {
                Date = x.Date,
                DoanhThu = x.TotalSell,
                LoiNhuan = x.TotalSell - x.TotalBuy
            });
            return Json(new { Data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ExportToExcel()
        {
            var data = db.Orders
                .GroupBy(o => DbFunctions.TruncateTime(o.CreatedDate))
                .Select(g => new
                {
                    Ngay = g.Key, // Trả về giá trị ngày mà không định dạng ngay tại truy vấn
                    DoanhThu = g.Sum(o => o.TotalAmount),
                    LoiNhuan = g.Sum(o => o.TotalAmount * 0.2m) // Giả sử lợi nhuận = 20% doanh thu
                })
                .ToList(); // Lấy dữ liệu về trước

            // Sau khi lấy về dữ liệu, thực hiện việc định dạng ngày

            var formattedData = data.Select(d => new
            {
                Ngay = d.Ngay.HasValue ? d.Ngay.Value.ToString("dd/MM/yyyy") : "", // Định dạng ngày sau khi lấy dữ liệu
                DoanhThu = d.DoanhThu.ToString("C"),
                LoiNhuan = d.LoiNhuan.ToString("C")
            }).ToList();

            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("DoanhThu");
            ws.Cell(1, 1).Value = "Ngày";
            ws.Cell(1, 2).Value = "Doanh thu";
            ws.Cell(1, 3).Value = "Lợi nhuận";

            for (int i = 0; i < formattedData.Count; i++)
            {
                ws.Cell(i + 2, 1).Value = formattedData[i].Ngay;
                ws.Cell(i + 2, 2).Value = formattedData[i].DoanhThu;
                ws.Cell(i + 2, 3).Value = formattedData[i].LoiNhuan;
            }

            using (var stream = new MemoryStream())
            {
                wb.SaveAs(stream);
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DoanhThu.xlsx");
            }
        }

    }
}