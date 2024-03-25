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
        public IActionResult Customer()
        {
            return View("CustomerManagement/Customer");
        }
        [Authentication]
        public IActionResult OrderUser()
        {
            return View("CustomerManagement/OrderUser");
        }
        [Authentication]
        public IActionResult OrderDetailsUser()
        {
            return View("CustomerManagement/OrderDetailsUser");
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
        public async Task<IActionResult> editProduct(Product model, IFormFile thumbnail)
        {
            if (true)
            {
                // Xử lý tệp tin ảnh và lưu đường dẫn
                if (thumbnail != null && thumbnail.Length > 0)
                {
                    var uploadsFolder = Path.Combine("wwwroot", "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + thumbnail.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await thumbnail.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn vào trường Thumbnail của mô hình
                    model.Thumbnail = "/images/" + Path.GetFileName(imagePath);
                }

                else
                {
                    var existingProduct = _context.Product.AsNoTracking().FirstOrDefault(p => p.ProductId == model.ProductId);
                    if (existingProduct != null)
                    {
                        model.Thumbnail = existingProduct.Thumbnail;
                    }
                }
                // Tạo slug từ tên sản phẩm và gán cho thuộc tính Slug của model
                model.Slug = SlugHelper.GenerateSlug(model.ProductName);

                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Product));
            }

            // Trả về View nếu dữ liệu không hợp lệ
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

        [Authentication]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> addProduct(Product model, IFormFile thumbnail)
        {
            if (true)
            {
                // Xử lý khi dữ liệu hợp lệ

                // Xử lý tệp tin ảnh và lưu đường dẫn
                if (thumbnail != null && thumbnail.Length > 0)
                {
                    var uploadsFolder = Path.Combine("wwwroot", "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + thumbnail.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await thumbnail.CopyToAsync(stream);
                    }
                    // Lưu đường dẫn vào trường Thumbnail của mô hình
                    model.Thumbnail = "/images/" + Path.GetFileName(imagePath);
                }
                // Tạo slug từ tên sản phẩm và gán cho thuộc tính Slug của model
                model.Slug = SlugHelper.GenerateSlug(model.ProductName);

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Product));
            }
            ModelState.AddModelError(string.Empty, "Some required fields are missing.");
            // Trả về View nếu dữ liệu không hợp lệ
            return View(model);
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
        public IActionResult Order()
        {
            return View("OrderManagement/Order");
        }
        [Authentication]
        public IActionResult detailOrder()
        {
            return View("OrderManagement/detailOrder");
        }
        [Authentication]
        public IActionResult Blog()
        {
            return View("BlogManagement/Blog");
        }
    }
}
