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
        // search
        public async Task<IActionResult> Search(string searchString)
        {
            // Query all products
            var productsQuery = db.Product.Include(p => p.Category).AsQueryable();
            var distinctCategories = await db.Category.ToListAsync();
            // kế thừa các logic chung từ BaseController
            await SetCommonViewData();


            // Filter by category, price, and product name
            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Category.CategoryName.Contains(searchString) ||
                    p.ProductName.Contains(searchString) ||
                    p.Price.ToString().Contains(searchString)
                );
            }

            // Retrieve the filtered products
            var filteredProducts = await productsQuery.ToListAsync();

            // Pass the filtered products to the view
            ViewBag.SearchResults = filteredProducts;

            return View(filteredProducts);
        }

        [Authentication]
        public async Task<IActionResult> Details(string slug)
        {
            // Kế thừa các logic chung từ BaseController
            await SetCommonViewData();


            // Lấy chi tiết sản phẩm từ cơ sở dữ liệu dựa trên Slug được cung cấp
            var product = await db.Product
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.GoldAge)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (product == null)
            {
                // Xử lý trường hợp không tìm thấy sản phẩm với Slug cụ thể
                return NotFound();
            }

            // Lấy các sản phẩm liên quan
            var relatedProducts = await db.Product
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId)
                .Take(4) // Số lượng sản phẩm liên quan bạn muốn hiển thị
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            // Đặt các thuộc tính ViewBag cho chi tiết sản phẩm
            ViewBag.ProductId = product.ProductId;
            ViewBag.ProductThumbnail = product.Thumbnail;  // Đường dẫn ảnh sản phẩm
            ViewBag.ProductName = product.ProductName; // Tên sản phẩm
            ViewBag.ProductPrice = product.Price;      // Giá sản phẩm
            ViewBag.ProductCategory = product.Category?.CategoryName;
            ViewBag.ProductSlug = slug;

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


        public async Task<IActionResult> Category(
    int page = 1, 
    int pageSize = 9, 
    decimal? minPrice = null, 
    decimal? maxPrice = null, 
    int? brandId = null, 
    int? goldAgeId = null)
{
    // Kế thừa các logic chung từ BaseController
    await SetCommonViewData();

    // Truy xuất dữ liệu Brand và Gold Age từ cơ sở dữ liệu
    var brands = await _context.Brand.ToListAsync();
    var goldAges = await _context.GoldAge.ToListAsync();

    // Truyền dữ liệu vào view
    ViewBag.Brands = brands;
    ViewBag.GoldAges = goldAges;

    // Lấy danh sách sản phẩm với phân trang
    var query = _context.Product
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

    // Lọc theo BrandId
    if (brandId != null)
    {
        query = query.Where(p => p.BrandId == brandId);
    }

    // Lọc theo GoldAgeId
    if (goldAgeId != null)
    {
        query = query.Where(p => p.GoldAgeId == goldAgeId);
    }

    var productList = await query.ToListAsync();

    // Tính toán và chuyển thông tin phân trang vào ViewBag hoặc ViewModel
    ViewBag.TotalProductCount = await _context.Product.CountAsync(); // Tổng số sản phẩm
    ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.TotalProductCount / pageSize);
    ViewBag.CurrentPage = page;
    ViewBag.Categories = await _context.Category.ToListAsync();

    return View(productList);
}


        [Authentication]
        public async Task<IActionResult> Shop(
            string slug,
            string searchString,
            int? minPrice,
            int? maxPrice,
            int page = 1,
            int pageSize = 9,
            int? brandId = null,
            int? goldAgeId = null)
        {
            // kế thừa các logic chung từ BaseController
            await SetCommonViewData();

            // Truy xuất dữ liệu Brand và Gold Age từ cơ sở dữ liệu
            var brands = await _context.Brand.ToListAsync();
            var goldAges = await _context.GoldAge.ToListAsync();

            ViewBag.Brands = await _context.Brand.ToListAsync();
            ViewBag.GoldAges = await _context.GoldAge.ToListAsync();


            if (slug == null)
            {
                return NotFound();
            }

            var category = db.Category.FirstOrDefault(c => c.Slug == slug);

            if (category == null)
            {
                return NotFound();
            }

            ViewBag.SearchString = searchString;

            var productsInCategory = db.Product
                .Include(p => p.Category)
                .OrderBy(p => p.ProductId)
                .Where(p => p.CategoryId == category.CategoryId);

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
            // Lọc theo BrandId
            if (brandId != null)
            {
                productsInCategory = productsInCategory.Where(p => p.BrandId == brandId);
            }

            // Lọc theo GoldAgeId
            if (goldAgeId != null)
            {
                productsInCategory = productsInCategory.Where(p => p.GoldAgeId == goldAgeId);
            }

            // Tính toán và chuyển thông tin phân trang vào ViewBag hoặc ViewModel
            ViewBag.CategorySlug = slug;
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
        public async Task<IActionResult> Contact()
        {
            // kế thừa các logic chung từ BaseController
            await SetCommonViewData();
            return View();
        }
        [Authentication]
        [HttpPost]
        public async Task<IActionResult> Contact(string name, string email, string message)
        {

            string smtpServer = _configuration["EmailSettings:SmtpServer"];
            int port = _configuration.GetValue<int>("EmailSettings:Port");
            string username = _configuration["EmailSettings:Username"];
            string password = _configuration["EmailSettings:Password"];

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = port,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(email),
                Subject = "New Message from Your Website",
                Body = $"Name: {name}\nEmail: {email}\nMessage: {message}"
            };

            mailMessage.To.Add("dungprohn1409@gmail.com"); // Thay đổi địa chỉ email của người nhận

            try
            {
                await smtpClient.SendMailAsync(mailMessage);

                // Thông báo gửi email thành công
                TempData["Message"] = "Information has been sent successfully!!";
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu gửi email thất bại
                ViewBag.ErrorMessage = $"Failed to send message: {ex.Message}";
            }

            // Chuyển hướng về trang Contact
            return View();
        }
        [Authentication]
        public async Task<IActionResult> Blog(int page = 1, int pageSize = 5, string tag = null)
        {
            // Kế thừa các logic chung từ BaseController
            await SetCommonViewData();

            // Truy vấn danh sách các tag từ cơ sở dữ liệu
            var allTags = await _context.Blog.Select(b => b.Tag).Distinct().ToListAsync();

            // Gán danh sách các tag vào ViewBag
            ViewBag.AllTags = allTags;

            // Khởi tạo query với sắp xếp theo ngày giảm dần
            var query = _context.Blog.OrderByDescending(b => b.BlogDate);

            // Lọc bài viết theo tag (nếu có)
            if (!string.IsNullOrEmpty(tag))
            {
                query = query.Where(b => b.Tag == tag).OrderByDescending(b => b.BlogDate);
            }

            // Lấy danh sách bài viết cho trang hiện tại và kích thước trang
            var blogPosts = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Tổng số bài viết
            int totalPosts = await query.CountAsync();

            // Số lượng trang
            int totalPages = (int)Math.Ceiling((double)totalPosts / pageSize);

            // Truyền danh sách bài viết và thông tin phân trang vào ViewBag hoặc ViewModel
            ViewBag.BlogPosts = blogPosts;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            // Trả về View và truyền dữ liệu từ ViewBag
            return View(blogPosts);
        }


        [Authentication]
        public async Task<IActionResult> BlogDetails(int id)
        {
            // Lấy thông tin chi tiết của bài viết từ cơ sở dữ liệu dựa trên ID
            var blogPost = await _context.Blog.FirstOrDefaultAsync(b => b.Id == id);

            if (blogPost == null)
            {
                // Nếu không tìm thấy bài viết, có thể xử lý thông báo lỗi ở đây hoặc chuyển hướng đến trang lỗi
                return NotFound(); // Trả về trang 404 Not Found
            }

            // Kế thừa các logic chung từ BaseController
            await SetCommonViewData();

            // Truyền thông tin chi tiết của bài viết vào View để hiển thị
            ViewBag.BlogPost = blogPost;

            return View();
        }


        [Authentication]
        public async Task<IActionResult> About()
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
                    ModelState.AddModelError(string.Empty, "Email or password is incorrect");
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
            // Kiểm tra xem các trường bắt buộc đã được điền đầy đủ chưa
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin.");
                return View(user);
            }

            // Kiểm tra xem địa chỉ email đã được sử dụng chưa
            var existingUser = db.User.FirstOrDefault(x => x.Email.Equals(user.Email));
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "The email address has been used");
                return View(user);
            }

            // Hash mật khẩu trước khi lưu vào cơ sở dữ liệu
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



        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("Username");
            return RedirectToAction("login", "Page");
        }
    }
}

