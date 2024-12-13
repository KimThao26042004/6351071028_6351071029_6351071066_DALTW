using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Products

        public ActionResult Index(int? page)
        {
            int pageSize = 8; // Số sản phẩm mỗi trang
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1

            var products = db.Products.Where(x => x.IsActive).OrderBy(p => p.Title).ToPagedList(pageNumber, pageSize);
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = products.PageCount;
            return View(products);
        }

        public ActionResult Search(string Searchtext, int? page)
        {
            int pageSize = 8; // Số sản phẩm mỗi trang
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1

            // Lọc sản phẩm theo từ khóa tìm kiếm
            var products = db.Products
                             .Where(x => x.IsActive)  // Chỉ lấy các sản phẩm đang hoạt động
                             .AsQueryable();

            // Nếu có từ khóa tìm kiếm, lọc sản phẩm theo tên sản phẩm
            if (!string.IsNullOrEmpty(Searchtext))
            {
                products = products.Where(p => p.Title.Contains(Searchtext));
            }

            // Phân trang và sắp xếp sản phẩm theo tên
            var pagedProducts = products.OrderBy(p => p.Title)
                                        .ToPagedList(pageNumber, pageSize);

            // Cung cấp thông tin phân trang và từ khóa tìm kiếm cho View
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = pagedProducts.PageCount;
            ViewBag.Searchtext = Searchtext; // Để giữ lại từ khóa tìm kiếm trong view

            return View(pagedProducts); // Trả về View với danh sách sản phẩm tìm được
        }


        public ActionResult Detail(string alias,int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                db.Products.Attach(item);
                item.ViewCount = item.ViewCount + 1;
                db.Entry(item).Property(x => x.ViewCount).IsModified = true;
                db.SaveChanges();
            }
            var countReview = db.Reviews.Where(x => x.ProductId == id).Count();
            ViewBag.CountReview = countReview;
            return View(item);
        }
        public ActionResult ProductCategory(string alias, int id, int? page)
        {
            int pageSize = 8; // Số sản phẩm mỗi trang
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1

            var items = db.Products.Where(x => (id <= 0 || x.ProductCategoryId == id) && x.IsActive) 
                                   .OrderBy(p => p.Title)
                                   .ToPagedList(pageNumber, pageSize);

            var cate = db.ProductCategories.Find(id);
            ViewBag.CateName = cate != null ? cate.Title : "Danh mục sản phẩm";
            ViewBag.CateId = id;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = items.PageCount;

            return View(items);
        }


        public ActionResult SaleProductCategory(string alias, int id, int? page)
        {
            int pageSize = 8; // Số sản phẩm mỗi trang
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1

            var items = db.Products.Where(x => x.IsSale && x.IsActive && (id <= 0 || x.ProductCategoryId == id))
                                   .OrderBy(p => p.Title)
                                   .ToPagedList(pageNumber, pageSize);

            var cate = db.ProductCategories.FirstOrDefault(x => x.Id == id || x.Alias == alias);
            ViewBag.CateName = cate != null ? cate.Title : "Sản phẩm khuyến mãi";
            ViewBag.CateId = id;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = items.PageCount;

            return View(items);
        }


        public ActionResult Partial_ItemsByCateId()
        {
            var items = db.Products.Where(x => x.IsHome && x.IsActive).Take(15).ToList();
            return PartialView(items);
        }

        public ActionResult Partial_ProductSales()
        {
            var items = db.Products.Where(x => x.IsSale && x.IsActive).Take(15).ToList();
            return PartialView(items);
        }

        public ActionResult ProductSales(int? page)
        {
            int pageSize = 8; // Số sản phẩm mỗi trang
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1

            var items = db.Products.Where(x => x.IsSale && x.IsActive)
                                   .OrderBy(p => p.Title)
                                   .ToPagedList(pageNumber, pageSize);

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = items.PageCount;

            return View(items);
        }



    }
}