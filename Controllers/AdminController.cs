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
using Microsoft.AspNetCore.Mvc.Rendering;
using static NuGet.Packaging.PackagingConstants;
using System.IO;

namespace OnlineJwellery_Shopping.Controllers
{
    public class AdminController : Controller
    {
        private readonly JwelleryShoppingContext _context;

        public AdminController(JwelleryShoppingContext context)
        {
            _context = context;
        }
        [Authentication]
        public IActionResult Dashboard()
        {
            return View("DashboardAdmin/Dashboard");
        }
        [Authentication]
        // Customer Management
        public async Task<IActionResult> Customer(int? page)
        {
            int pageSize = 1; // Số lượng người dùng mỗi trang
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1 nếu không có page được cung cấp

            if (_context.User != null)
            {
                // Lấy tổng số người dùng từ cơ sở dữ liệu
                int totalUsers = await _context.User.CountAsync();

                // Phân trang danh sách người dùng
                var userList = await _context.User.Skip((pageNumber - 1) * pageSize)
                                                  .Take(pageSize)
                                                  .ToListAsync();

                // Chuyển thông tin phân trang vào ViewBag
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
                ViewBag.TotalUsers = totalUsers;
                return View("CustomerManagement/Customer", userList);
            }
            else
            {
                return Problem("Entity set 'OgainShopContext.User' is null.");
            }
        }

        [Authentication]
        public IActionResult OrderUser(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không có ID người dùng
            }

            // Lấy thông tin người dùng từ cơ sở dữ liệu dựa trên id
            var user = _context.User.Include(u => u.Orders).FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không tìm thấy người dùng
            }

            return View("CustomerManagement/OrderUser", user);
        }
        [Authentication]
        public IActionResult OrderDetailsUser(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không có ID đơn hàng
            }

            // Lấy thông tin đơn hàng từ cơ sở dữ liệu dựa trên id và nạp thông tin User và OrderProducts
            var order = _context.Order
                .Include(o => o.User)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product) // Nạp thông tin sản phẩm cho từng OrderProduct
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không tìm thấy đơn hàng
            }

            return View("CustomerManagement/OrderDetailsUser", order);
        }
        [Authentication]
        // Product Management
        public async Task<IActionResult> Product(
     int? CategoryId,
     int? BrandId,
     int? GoldAgeId,
     string ProductName,
     decimal? Price_from,
     decimal? Price_to,
     string? StoneType,
     decimal? TotalWeight_from,
     decimal? TotalWeight_to,
     string? Color,
     string? Size,
     string? Material,
     string? CertificationCode,
     string search,
     int page = 1,
     int pageSize = 10)
        {
            var categories = await _context.Category.ToListAsync();
            var brands = await _context.Brand.ToListAsync();
            var goldAges = await _context.GoldAge.ToListAsync();
            var products = _context.Product.AsQueryable();
            // Áp dụng các tiêu chí lọc nếu chúng được cung cấp
            if (CategoryId.HasValue)
            {
                // Lọc theo category_id
                products = products.Where(o => o.CategoryId == CategoryId.Value);
            }
            if (BrandId.HasValue)
            {
                // Lọc theo Brand_id
                products = products.Where(o => o.BrandId == BrandId.Value);
            }

            if (GoldAgeId.HasValue)
            {
                // Lọc theo GoldAge_id
                products = products.Where(o => o.GoldAgeId == GoldAgeId.Value);
            }

            if (!string.IsNullOrEmpty(ProductName))
            {
                // Lọc theo productName
                products = products.Where(o => o.ProductName.Contains(ProductName));
            }

            if (Price_from.HasValue)
            {
                // Lọc theo giá từ Price_from
                products = products.Where(o => o.Price >= Price_from.Value);
            }

            if (Price_to.HasValue)
            {
                // Lọc theo giá đến Price_to
                products = products.Where(o => o.Price <= Price_to.Value);
            }
            if (!string.IsNullOrEmpty(StoneType))
            {
                // Lọc theo StoneType
                products = products.Where(o => o.StoneType.Contains(StoneType));
            }
            if (TotalWeight_from.HasValue)
            {
                // Lọc theo giá từ TotalWeight_from
                products = products.Where(o => o.TotalWeight >= TotalWeight_from.Value);
            }

            if (TotalWeight_to.HasValue)
            {
                // Lọc theo giá đến TotalWeight_to
                products = products.Where(o => o.TotalWeight <= TotalWeight_to.Value);
            }
            if (!string.IsNullOrEmpty(Color))
            {
                // Lọc theo Color
                products = products.Where(o => o.Color.Contains(Color));
            }
            if (!string.IsNullOrEmpty(CertificationCode))
            {
                // Lọc theo CertificationCode
                products = products.Where(o => o.CertificationCode.Contains(CertificationCode));
            }
            if (!string.IsNullOrEmpty(Size))
            {
                // Lọc theo Size
                products = products.Where(o => o.Size.Contains(Size));
            }
            if (!string.IsNullOrEmpty(Material))
            {
                // Lọc theo Material
                products = products.Where(o => o.Material.Contains(Material));
            }
            if (!string.IsNullOrEmpty(search))
            {
                // Lọc theo từ khóa tìm kiếm trong tên sản phẩm hoặc mô tả
                products = products.Where(o => o.ProductName.Contains(search)
                                            || o.CertificationCode.Contains(search));

            }

            // Phân trang
            var totalProducts = await products.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            var productList = await products.Skip((page - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToListAsync();

            ViewBag.brands = brands;
            ViewBag.goldAges = goldAges;
            ViewBag.Categories = categories;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            // Trả về view với danh sách sản phẩm đã lọc và thông tin phân trang
            return View("ProductManagement/Product", productList);
        }



        [Authentication]
        [HttpGet]
        public async Task<IActionResult> editProduct(int id)
        {
            // Lấy thông tin sản phẩm cần chỉnh sửa từ cơ sở dữ liệu
            var product = await _context.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Lấy danh sách các danh mục để hiển thị trong dropdownlist
            var categories = _context.Category.OrderBy(c => c.CategoryName).ToList();
            var brands = _context.Brand.OrderBy(c => c.BrandName).ToList();
            var goldAges = _context.GoldAge.OrderBy(c => c.Age).ToList();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            ViewBag.Brands = new SelectList(brands, "BrandId", "BrandName");
            ViewBag.GoldAges = new SelectList(goldAges, "GoldAgeId", "Age");

            return View("ProductManagement/editProduct", product);
        }

        [Authentication]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> editProduct(Product model, IFormFile thumbnail,
           IFormFile smallThumbnail1, IFormFile smallThumbnail2,
           IFormFile smallThumbnail3, IFormFile smallThumbnail4)
        {
            if (true)
            {
                try
                {
                    // Xử lý tệp tin ảnh chính (thumbnail) và lưu đường dẫn
                    if (thumbnail != null && thumbnail.Length > 0)
                    {
                        model.Thumbnail = await SaveImage(thumbnail);
                    }
                    else
                    {
                        // Giữ hình ảnh hiện tại nếu không có hình mới được cung cấp
                        var existingProduct = _context.Product.AsNoTracking().FirstOrDefault(p => p.ProductId == model.ProductId);
                        if (existingProduct != null)
                        {
                            model.Thumbnail = existingProduct.Thumbnail;
                        }
                    }

                    // Xử lý các tệp tin ảnh smallThumbnail và lưu đường dẫn tương ứng
                    if (smallThumbnail1 != null && smallThumbnail1.Length > 0)
                    {
                        model.SmallThumbnail1 = await SaveImage(smallThumbnail1);
                    }

                    if (smallThumbnail2 != null && smallThumbnail2.Length > 0)
                    {
                        model.SmallThumbnail2 = await SaveImage(smallThumbnail2);
                    }

                    if (smallThumbnail3 != null && smallThumbnail3.Length > 0)
                    {
                        model.SmallThumbnail3 = await SaveImage(smallThumbnail3);
                    }

                    if (smallThumbnail4 != null && smallThumbnail4.Length > 0)
                    {
                        model.SmallThumbnail4 = await SaveImage(smallThumbnail4);
                    }

                    // Tạo slug từ tên sản phẩm và gán cho thuộc tính Slug của model
                    model.Slug = SlugHelper.GenerateSlug(model.ProductName, model.ProductId);

                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Product));
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Xử lý ngoại lệ xảy ra trong quá trình cập nhật dữ liệu
                    ModelState.AddModelError("", "Error occurred while saving data.");
                }
            }

            // Nếu dữ liệu không hợp lệ, trả về view với model và thông tin danh mục
            var categories = _context.Category.OrderBy(c => c.CategoryName).ToList();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View("ProductManagement/editProduct", model);
        }



        [Authentication]
        public IActionResult addProduct()
        {
            var categories = _context.Category.OrderBy(c => c.CategoryName).ToList();
            var brands = _context.Brand.OrderBy(c => c.BrandName).ToList();
            var goldages = _context.GoldAge.OrderBy(c => c.Age).ToList();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            ViewBag.Brands = new SelectList(brands, "BrandId", "BrandName");
            ViewBag.Goldages = new SelectList(goldages, "GoldAgeId", "Age");

            return View("ProductManagement/addProduct");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> addProduct(Product model,
            IFormFile thumbnail, IFormFile smallThumbnail1,
            IFormFile smallThumbnail2, IFormFile smallThumbnail3,
            IFormFile smallThumbnail4)
        {
            if (true)
            {
                if (thumbnail != null && thumbnail.Length > 0)
                {
                    var thumbnailPath = await SaveImage(thumbnail);
                    model.Thumbnail = thumbnailPath;
                }

                if (smallThumbnail1 != null && smallThumbnail1.Length > 0)
                {
                    var smallThumbnail1Path = await SaveImage(smallThumbnail1);
                    model.SmallThumbnail1 = smallThumbnail1Path;
                }

                if (smallThumbnail2 != null && smallThumbnail2.Length > 0)
                {
                    var smallThumbnail2Path = await SaveImage(smallThumbnail2);
                    model.SmallThumbnail2 = smallThumbnail2Path;
                }

                if (smallThumbnail3 != null && smallThumbnail3.Length > 0)
                {
                    var smallThumbnail3Path = await SaveImage(smallThumbnail3);
                    model.SmallThumbnail3 = smallThumbnail3Path;
                }

                if (smallThumbnail4 != null && smallThumbnail4.Length > 0)
                {
                    var smallThumbnail4Path = await SaveImage(smallThumbnail4);
                    model.SmallThumbnail4 = smallThumbnail4Path;
                }

                model.Slug = SlugHelper.GenerateSlug(model.ProductName, model.ProductId);

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Product));
            }
            ModelState.AddModelError(string.Empty, "Some required fields are missing.");
            return View(model);
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine("wwwroot", "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + imageFile.FileName);
            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return "/images/" + Path.GetFileName(imagePath);
        }


        public async Task<IActionResult> detailsProduct(int? ProductId)
        {
            if (ProductId == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.GoldAge)
                .FirstOrDefaultAsync(p => p.ProductId == ProductId);

            if (product == null)
            {
                return NotFound();
            }

            return View("ProductManagement/detailsProduct", product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> deleteProduct(int id)
        {
            var product = await _context.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Xóa ảnh sản phẩm khỏi thư mục
            if (!string.IsNullOrEmpty(product.Thumbnail))
            {
                var imagePath = Path.Combine("wwwroot", product.Thumbnail.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            // Chuyển hướng về trang ProductManagement/Product
            return RedirectToAction("Product", "Admin");
        }

        [Authentication]
        public async Task<IActionResult> Order(int page = 1, int pageSize = 10, string TotalAmount = null, string ShippingMethod = null, string PaymentMethod = null, string IsPaid = null, string Status = null, string search = null)
        {
            // Lấy danh sách đơn hàng từ cơ sở dữ liệu
            var orders = _context.Order.AsQueryable();

            // Áp dụng các tiêu chí lọc nếu chúng được cung cấp
            if (!string.IsNullOrEmpty(TotalAmount))
            {
                decimal totalAmountValue;
                if (decimal.TryParse(TotalAmount, out totalAmountValue))
                {
                    // Lọc theo TotalAmount
                    orders = orders.Where(o => o.TotalAmount == totalAmountValue);
                }
            }

            if (!string.IsNullOrEmpty(ShippingMethod))
            {
                // Lọc theo ShippingMethod
                orders = orders.Where(o => o.ShippingMethod == ShippingMethod);
            }

            if (!string.IsNullOrEmpty(PaymentMethod))
            {
                // Lọc theo PaymentMethod
                orders = orders.Where(o => o.PaymentMethod == PaymentMethod);
            }

            if (!string.IsNullOrEmpty(IsPaid))
            {
                // Lọc theo trạng thái đã thanh toán (Paid/Unpaid)
                bool isPaid = IsPaid == "1";
                orders = orders.Where(o => o.IsPaid == (isPaid ? "paid" : "unpaid"));
            }

            if (!string.IsNullOrEmpty(Status))
            {
                // Lọc theo Status
                orders = orders.Where(o => o.Status == Status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                // Lọc theo từ khóa tìm kiếm trong tên người đặt hàng, email hoặc các trường khác
                orders = orders.Where(o => o.FullName.Contains(search)
                                        || o.Email.Contains(search)
                                        || o.TotalAmount.ToString().Contains(search)
                                        || o.ShippingMethod.Contains(search)
                                        || o.PaymentMethod.Contains(search)
                                        || o.IsPaid.Contains(search)
                                        || o.Status.Contains(search));
            }
            // Sắp xếp theo OrderDate mới nhất
            orders = orders.OrderByDescending(o => o.OrderDate);
            // Tính toán số lượng đơn hàng và số trang
            int totalOrders = await orders.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            // Phân trang
            orders = orders.Skip((page - 1) * pageSize).Take(pageSize);

            // Truyền thông tin phân trang vào ViewBag hoặc ViewModel
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            // Trả về view với danh sách đơn hàng đã lọc và phân trang
            return View("OrderManagement/Order", await orders.ToListAsync());
        }


        [Authentication]
        public async Task<IActionResult> detailOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var order = _context.Order
                .Include(o => o.User)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View("OrderManagement/detailOrder", order);
        }
        [Authentication]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string returnUrl)
        {
            var order = await _context.Order.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái cho đơn hàng
            order.Status = status;
            _context.Update(order);
            await _context.SaveChangesAsync();

            // Chuyển hướng đến returnUrl
            return Redirect(returnUrl);
        }
        public async Task<IActionResult> Blog(int? page)
        {
            int pageSize = 10; // Số lượng bài đăng mỗi trang
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1 nếu không có page được cung cấp

            // Lấy tổng số bài đăng từ cơ sở dữ liệu
            int totalBlogs = await _context.Blog.CountAsync();

            // Phân trang danh sách bài đăng và sắp xếp theo thời gian gần nhất
            var blogs = await _context.Blog.OrderByDescending(b => b.BlogDate)
                                           .Skip((pageNumber - 1) * pageSize)
                                           .Take(pageSize)
                                           .ToListAsync();

            // Chuyển thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);
            ViewBag.TotalBlogs = totalBlogs;

            return View("BlogManagement/Blog", blogs);
        }




        [Authentication]
        [HttpGet]
        public async Task<IActionResult> editBlog(int id)
        {
            var blog = await _context.Blog.FindAsync(id);

            if (blog == null)
            {
                return NotFound();
            }

            return View("BlogManagement/editBlog", blog);
        }

        [Authentication]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> editBlog(Blog model, IFormFile thumbnail)
        {
            if (true)
            {
                if (thumbnail != null && thumbnail.Length > 0)
                {
                    // Nếu có ảnh đại diện mới được cung cấp, hãy cập nhật nó
                    var uploadsFolder = Path.Combine("wwwroot", "images", "Blogs");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + thumbnail.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await thumbnail.CopyToAsync(stream);
                    }

                    model.Thumbnail = "/images/Blogs/" + Path.GetFileName(imagePath);
                }
                else
                {
                    // Nếu không có ảnh đại diện mới được cung cấp, giữ giá trị hiện tại
                    var existingBlog = await _context.Blog.AsNoTracking().FirstOrDefaultAsync(b => b.Id == model.Id);
                    if (existingBlog != null)
                    {
                        model.Thumbnail = existingBlog.Thumbnail;
                    }
                }

                model.BlogDate = DateTime.Now;

                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Blog));
            }

            return View(model);
        }

        public IActionResult addBlog()
        {
            return View("BlogManagement/addBlog");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> addBlog(Blog model, IFormFile thumbnail)
        {
            if (true)
            {
                if (thumbnail != null && thumbnail.Length > 0)
                {
                    var uploadsFolder = Path.Combine("wwwroot", "images","Blogs");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + thumbnail.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await thumbnail.CopyToAsync(stream);
                    }

                    model.Thumbnail = "/images/Blogs/" + Path.GetFileName(imagePath);

                }

                model.BlogDate = DateTime.Now;
               

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Blog));
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> deleteBlog(int id)
        {
            var blog = await _context.Blog.FindAsync(id);

            if (blog == null)
            {
                return NotFound();
            }

            // Xóa ảnh đại diện của blog khỏi thư mục
            if (!string.IsNullOrEmpty(blog.Thumbnail))
            {
                var imagePath = Path.Combine("wwwroot", blog.Thumbnail.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Blog.Remove(blog);
            await _context.SaveChangesAsync();

            // Chuyển hướng về trang BlogManagement/Blog
            return RedirectToAction("Blog", "Admin");
        }
        [Authentication]
        public IActionResult Review()
        {
            return View("ReviewManagement/Review");
        }

        [Authentication]
        public IActionResult detailReview()
        {
            return View("ReviewManagement/detailReview");
        }


    }
}
