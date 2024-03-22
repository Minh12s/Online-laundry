using Microsoft.AspNetCore.Mvc;
using OnlineJwellery_Shopping.Models;
using OnlineJwellery_Shopping.Data;
using Microsoft.AspNetCore.Http;
using OnlineJwellery_Shopping.Models.Authentication;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System;
using System.Text;
using OnlineJwellery_Shopping.Heplers;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnlineJwellery_Shopping.Controllers
{
    public class PageController : BaseController
    {
        private readonly JwelleryShoppingContext db;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;


        public PageController(JwelleryShoppingContext context, IConfiguration configuration, IWebHostEnvironment env) : base(context)
        {
            db = context;
            _configuration = configuration;
            _env = env;
        }
        // GET: /<controller>/
        [Authentication]
        public async Task<IActionResult> Home()
        {
            // kế thừa các logic chung từ BaseController
            await SetCommonViewData();

            return View();
        }
        [Authentication]
        public async Task<IActionResult> Details(int id)
        {
            // kế thừa các logic chung từ BaseController
            await SetCommonViewData();
            // Lấy chi tiết sản phẩm từ cơ sở dữ liệu dựa trên ID được cung cấp
            var product = await db.Product.Include(p => p.Category)
                                           .Include(p => p.Brand)
                                           .Include(p => p.GoldAge)
                                           .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                // Xử lý trường hợp không tìm thấy sản phẩm với ID cụ thể
                return NotFound();
            }

            var relatedProducts = await db.Product
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id)
                .Take(4) // Số lượng sản phẩm liên quan bạn muốn hiển thị
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            // Đặt các thuộc tính ViewBag cho chi tiết sản phẩm
            ViewBag.ProductId = product.ProductId;
            ViewBag.ProductThumbnail = product.Thumbnail;  // Đường dẫn ảnh sản phẩm
            ViewBag.ProductName = product.ProductName; // Tên sản phẩm
            ViewBag.ProductPrice = product.Price;      // Giá sản phẩm
            ViewBag.ProductCategory = product.Category?.CategoryName;

            // Đặt các thuộc tính ViewBag cho sự có sẵn của sản phẩm
            ViewBag.ProductAvailability = product.Qty;

            // Đặt thông tin Brand vào ViewBag để sử dụng trong view
            ViewBag.BrandName = product.Brand?.BrandName; // Tên Brand

            // Đặt thông tin GoldAge vào ViewBag để sử dụng trong view
            ViewBag.GoldAge = product.GoldAge?.Age; // Tên GoldAge

            // Đặt các thuộc tính ViewBag cho thông tin khác của sản phẩm
            ViewBag.StoneType = product.StoneType;
            ViewBag.TotalWeight = product.TotalWeight;
            ViewBag.Color = product.Color;
            ViewBag.Size = product.Size;
            ViewBag.Material = product.Material;
            ViewBag.CertificationCode = product.CertificationCode;

            return View(product);
        }

        [Authentication]
        public async Task<IActionResult> Category(int page = 1, int pageSize = 9, decimal? minPrice = null, decimal? maxPrice = null)
        {
            // kế thừa các logic chung từ BaseController
            await SetCommonViewData();


            // Lấy danh sách sản phẩm với phân trang
            var query = db.Product
    .Include(p => p.Category) // Include the Category information
    .OrderBy(p => p.ProductId)
    .Skip((page - 1) * pageSize)
    .Take(pageSize);

            // Lọc theo giá
            if (minPrice != null)
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (maxPrice != null)
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            var productList = await query.ToListAsync();
            // Tính toán và chuyển thông tin phân trang vào ViewBag hoặc ViewModel
            ViewBag.TotalProductCount = await db.Product.CountAsync(); // Tổng số sản phẩm
            ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.TotalProductCount / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Categories = await db.Category.ToListAsync();

            return View(productList);
        }
        [Authentication]
        public async Task<IActionResult> Shop(int? id, string searchString, int? minPrice, int? maxPrice, int page = 1, int pageSize = 9)
        {

            // kế thừa các logic chung từ BaseController
            await SetCommonViewData();
            if (id == null)
            {
                return NotFound();
            }

            var category = db.Category.Find(id);

            if (category == null)
            {
                return NotFound();
            }

            ViewBag.SearchString = searchString;

            var productsInCategory = db.Product
                .Include(p => p.Category)
                .Where(p => p.CategoryId == id);

            if (!string.IsNullOrEmpty(searchString))
            {
                productsInCategory = productsInCategory.Where(p => p.ProductName.Contains(searchString));
            }

            // Lọc theo giá
            if (minPrice != null)
            {
                productsInCategory = productsInCategory.Where(p => p.Price >= minPrice);
            }

            if (maxPrice != null)
            {
                productsInCategory = productsInCategory.Where(p => p.Price <= maxPrice);
            }

            // Tính toán và chuyển thông tin phân trang vào ViewBag hoặc ViewModel
            ViewBag.CategoryId = id;
            ViewBag.TotalProductCount = productsInCategory.Count(); // Đếm số lượng sản phẩm sau khi áp dụng tìm kiếm
            ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.TotalProductCount / pageSize);
            ViewBag.CurrentPage = page;

            // Lấy danh sách sản phẩm với phân trang
            var paginatedProducts = productsInCategory
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.ProductsInCategory = paginatedProducts;
            ViewBag.Categories = db.Category.ToList();

            return View(paginatedProducts);
        }
        [Authentication]
        public async Task<IActionResult> Checkout()
        {
            // kế thừa các logic chung từ BaseController
            await SetCommonViewData();
            return View();
        }
        [Authentication]
        public async Task<IActionResult> Contact()
        {
            // kế thừa các logic chung từ BaseController
            await SetCommonViewData();
            return View();
        }
        [Authentication]
        public async Task<IActionResult> Blog()
        {
            // kế thừa các logic chung từ BaseController
            await SetCommonViewData();
            return View();
        }
        [Authentication]
        public async Task<IActionResult> BlogDetails()
        {
            // kế thừa các logic chung từ BaseController
            await SetCommonViewData();
            return View();
        }
        [Authentication]
        public async Task<IActionResult> About()
        {
            // kế thừa các logic chung từ BaseController
            await SetCommonViewData();
            return View();
        }
        [Authentication]
        public async Task<IActionResult> Thankyou()
        {
            // kế thừa các logic chung từ BaseController
            await SetCommonViewData();
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {

            if (HttpContext.Session.GetString("Username") == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Home", "Page");
            }
        }
        [HttpPost]
        public IActionResult Login(User user)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                User u = db.User.FirstOrDefault(x => x.Email.Equals(user.Email));

                if (u != null && BCrypt.Net.BCrypt.Verify(user.Password, u.Password))
                {
                    HttpContext.Session.SetString("Username", u.Username.ToString());
                    HttpContext.Session.SetString("Role", u.Role);
                    HttpContext.Session.SetString("UserId", u.UserId.ToString());

                    if (u.Role == "Admin" || u.Role == "User")
                    {
                        return RedirectToAction("Home", "Page");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tên người dùng hoặc mật khẩu không chính xác.");
                    return View();
                }
            }

            return RedirectToAction("Home", "Page");
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (true)
            {
                // Kiểm tra xem tên đăng nhập đã tồn tại chưa
                if (db.User.Any(x => x.Username.Equals(user.Username)))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(user);
                }
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                // Thiết lập vai trò mặc định là "User"
                user.Role = "User";

                // Thêm người dùng mới vào cơ sở dữ liệu
                db.User.Add(user);
                db.SaveChanges();

                // Lưu thông tin đăng nhập vào phiên làm việc
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                return RedirectToAction("Home", "Page");
            }

            return View(user);
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("Username");
            return RedirectToAction("login", "Page");
        }
    }
}

