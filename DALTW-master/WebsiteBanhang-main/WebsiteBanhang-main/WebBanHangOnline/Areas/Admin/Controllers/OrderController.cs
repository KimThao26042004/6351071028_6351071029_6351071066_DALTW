using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using PagedList;
using System.Globalization;
using System.Data.Entity;
using WebBanHangOnline.Models.ViewModels;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Order
        public ActionResult Index(int? page)
        {
            var items = db.Orders.OrderByDescending(x => x.CreatedDate).ToList();

            if (page == null)
            {
                page = 1;
            }
            var pageNumber = page ?? 1;
            var pageSize = 10;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageNumber;
            return View(items.ToPagedList(pageNumber, pageSize));
        }

      

        public ActionResult View(int id)
        {
            var item = db.Orders.Find(id);
            return View(item);
        }

        public ActionResult Partial_SanPham(int id)
        {
            var items = db.OrderDetails.Where(x => x.OrderId == id).ToList();
            return PartialView(items);
        }

        [HttpPost]
        public ActionResult UpdateTT(int id, int trangthai)
        {
            // Tìm đơn hàng dựa trên ID
            var order = db.Orders.Include("OrderDetails.Product").FirstOrDefault(o => o.Id == id);

            if (order != null)
            {
                // Cập nhật trạng thái đơn hàng
                order.Status = trangthai;
                db.Entry(order).Property(x => x.Status).IsModified = true;
                db.SaveChanges();

                // Gửi email thông báo cho khách hàng
                try
                {
                    // Lấy danh sách sản phẩm trong đơn hàng
                    var productDetails = "";
                    foreach (var detail in order.OrderDetails)
                    {
                        productDetails += $"<tr>" +
                                          $"<td>{detail.Product?.Title ?? "Sản phẩm không tồn tại"}</td>" +
                                          $"<td>{detail.Quantity}</td>" +
                                          $"<td>{detail.Price:C}</td>" +
                                          $"</tr>";
                    }

                    string trangThaiText;
                    switch (trangthai)
                    {
                        case 1:
                            trangThaiText = "Chưa thanh toán";
                            break;
                        case 2:
                            trangThaiText = "Đã thanh toán";
                            break;
                        case 3:
                            trangThaiText = "Hoàn thành";
                            break;
                        case 4:
                            trangThaiText = "Đã hủy";
                            break;
                        default:
                            trangThaiText = "Không xác định";
                            break;
                    }

                    // Đọc template email
                    string emailTemplate = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/order_status.html"));
                    emailTemplate = emailTemplate.Replace("{{MaDon}}", order.Code)
                                                 .Replace("{{TenKhachHang}}", order.CustomerName)
                                                 .Replace("{{Email}}", order.Email ?? "Không có email")
                                                 .Replace("{{NgayCapNhat}}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
                                                 .Replace("{{TrangThai}}", trangThaiText)
                                                 .Replace("{{ChiTietSanPham}}", productDetails);

                    // Gửi email
                    WebBanHangOnline.Common.Common.SendMail(
                        "ShopOnline",
                        $"Cập nhật trạng thái đơn hàng #{order.Code}",
                        emailTemplate, order.Email
                    );
                }
                catch (Exception ex)
                {
                    // Ghi log hoặc xử lý lỗi gửi email
                    return Json(new { message = "Cập nhật thành công nhưng không thể gửi email.", Success = true, Error = ex.Message });
                }

                return Json(new { message = "Cập nhật thành công và email đã được gửi.", Success = true });
            }

            return Json(new { message = "Không tìm thấy đơn hàng.", Success = false });
            }



        //public void ThongKe(string fromDate, string toDate)
        //{
        //    var query = from o in db.Orders
        //                join od in db.OrderDetails on o.Id equals od.OrderId
        //                join p in db.Products on od.ProductId equals p.Id
        //                select new
        //                {
        //                    CreatedDate = o.CreatedDate,
        //                    Quantity = od.Quantity,
        //                    Price = od.Price,
        //                    OriginalPrice = p.Price
        //                };
        //    if (!string.IsNullOrEmpty(fromDate))
        //    {
        //        DateTime start = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
        //        query = query.Where(x => x.CreatedDate >= start);
        //    }
        //    if (!string.IsNullOrEmpty(toDate))
        //    {
        //        DateTime endDate = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
        //        query = query.Where(x => x.CreatedDate < endDate);
        //    }
        //    var result = query.GroupBy(x => DbFunctions.TruncateTime(x.CreatedDate)).Select(r => new
        //    {
        //        Date = r.Key.Value,
        //        TotalBuy = r.Sum(x => x.OriginalPrice * x.Quantity), // tổng giá bán
        //        TotalSell = r.Sum(x => x.Price * x.Quantity) // tổng giá mua
        //    }).Select(x => new RevenueStatisticViewModel
        //    {
        //        Date = x.Date,
        //        Benefit = x.TotalSell - x.TotalBuy,
        //        Revenues = x.TotalSell
        //    });
        //}
        public ActionResult ThongKe(string fromDate, string toDate)
{
    var query = from o in db.Orders
                join od in db.OrderDetails on o.Id equals od.OrderId
                join p in db.Products on od.ProductId equals p.Id
                join pc in db.ProductCategories on p.ProductCategoryId equals pc.Id // Kết nối với ProductCategory
                select new
                {
                    CreatedDate = o.CreatedDate,
                    Quantity = od.Quantity,
                    Price = od.Price,
                    OriginalPrice = p.Price,
                    CategoryName = pc.Title // Lấy tên của ProductCategory
                };

    if (!string.IsNullOrEmpty(fromDate))
    {
        DateTime start = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
        query = query.Where(x => x.CreatedDate >= start);
    }

    if (!string.IsNullOrEmpty(toDate))
    {
        DateTime endDate = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
        query = query.Where(x => x.CreatedDate < endDate);
    }

    var result = query.GroupBy(x => new { x.CategoryName, Date = DbFunctions.TruncateTime(x.CreatedDate) }) // Nhóm theo Category và Date
                      .Select(r => new
                      {
                          CategoryName = r.Key.CategoryName,
                          Date = r.Key.Date.Value,
                          TotalBuy = r.Sum(x => x.OriginalPrice * x.Quantity), // Tổng giá bán
                          TotalSell = r.Sum(x => x.Price * x.Quantity) // Tổng giá mua
                      })
                      .Select(x => new RevenueStatisticViewModel
                      {
                          ProductCategory = x.CategoryName, // Tên loại sản phẩm
                          Date = x.Date,
                          Benefit = x.TotalSell - x.TotalBuy,
                          Revenues = x.TotalSell
                      })
                      .ToList();

    return Json(result, JsonRequestBehavior.AllowGet); // Trả về JSON với kết quả thống kê
}

    }
}