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
        public async Task<IActionResult> Product(int? CategoryId,int? BrandId ,int? GoldAgeId, string ProductName, decimal? Price_from, decimal? Price_to, string search)
        {
            var categories = await _context.Category.ToListAsync();
            var brands = await _context.Brand.ToListAsync();
            var goldAges = await _context.GoldAge.ToListAsync();
            var products = _context.Product.AsQueryable();
            ViewBag.brands = brands;
            ViewBag.goldAges = goldAges;
            ViewBag.Categories = categories;
            // Trả về view với danh sách sản phẩm đã lọc
            return View("ProductManagement/Product", await products.ToListAsync());
        }

        [Authentication]
        public IActionResult editProduct()
        {
            return View("ProductManagement/editProduct");
        }
        [Authentication]
        public IActionResult addProduct()
        {
            return View("ProductManagement/addProduct");
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
