using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models.ViewModels;
using WebBanHangOnline.Models;
using PagedList;

namespace WebBanHangOnline.Controllers
{
    public class OrderDetailController : Controller
    {
        // GET: OrderDetail
        //public ActionResult Index()
        //{
        //    return View();
        //}
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
            var item = db.Orders.Find(id); // Tìm đơn hàng theo ID
            if (item == null)
            {
                return HttpNotFound(); // Nếu không tìm thấy đơn hàng, trả về lỗi 404
            }
            return View(item); // Trả về view với chi tiết đơn hàng
        }

        public ActionResult Partial_SanPham(int id)
        {
            var items = db.OrderDetails.Where(x => x.OrderId == id).ToList();
            return PartialView(items);
        }

        [HttpPost]
        public ActionResult UpdateTT(int id, int trangthai)
        {
            var item = db.Orders.Find(id); // Tìm đơn hàng dựa trên ID.
            if (item != null) // Nếu đơn hàng tồn tại.
            {
                db.Orders.Attach(item); // Gắn đối tượng vào context nếu chưa được track.
                item.Status = trangthai; // Cập nhật trạng thái đơn hàng.
                db.Entry(item).Property(x => x.Status).IsModified = true; // Đánh dấu thuộc tính này đã thay đổi.
                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu.
                return Json(new { message = "Success", Success = true }); // Trả về JSON báo thành công.
            }
            return Json(new { message = "Unsuccess", Success = false }); // Trả về JSON báo lỗi nếu không tìm thấy.
        }

    }
}