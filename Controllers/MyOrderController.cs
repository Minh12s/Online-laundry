using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJwellery_Shopping.Data;
using OnlineJwellery_Shopping.Models.Authentication;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnlineJwellery_Shopping.Controllers
{
    public class MyOrderController : BaseController
    {
        private readonly JwelleryShoppingContext db;
        private readonly IConfiguration _configuration;

        public MyOrderController(JwelleryShoppingContext context, IConfiguration configuration) : base(context)
        {
            db = context;
            _configuration = configuration;
        }
        // GET: /<controller>/
        // GET: /<controller>/
        [Authentication]
        public async Task<IActionResult> MyOrder(int page = 1, int pageSize = 10)
        {
            // Kế thừa các logic chung từ BaseController
            await SetCommonViewData();

            // Lấy userId từ session
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // Truy vấn database để lấy thông tin người dùng có đơn hàng
            var user = db.User
                .Include(u => u.Orders)
                .ThenInclude(o => o.OrderProducts)
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            // Logic phân trang ở đây
            var totalOrders = user.Orders.Count;
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            // Kiểm tra xem trang yêu cầu có hợp lệ không

            user.Orders = user.Orders
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize; // Truyền giá trị pageSize vào ViewBag
            if (page < 1 || page > totalPages)
            {
                // Nếu trang yêu cầu không hợp lệ, chuyển về trang đầu tiên
                page = 1;
            }

            // Trả dữ liệu danh sách đơn hàng cho view
            return View(user);
        }
        public IActionResult OrderDetail(int? id)
        {

            if (id == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không có ID đơn hàng
            }

            // Lấy thông tin đơn hàng từ cơ sở dữ liệu dựa trên id và nạp thông tin User và OrderProducts
            var order = db.Order
                .Include(o => o.User)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product) // Nạp thông tin sản phẩm cho từng OrderProduct
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không tìm thấy đơn hàng
            }

            return View(order);
        }
        [Authentication]
        public async Task<IActionResult> ChangePassword()
        {
           
            return View();
        }
        [HttpPost]
        [Authentication]
        public async Task<IActionResult> ChangePassword(string current_password, string new_password, string new_password_confirmation)
        {
            if (new_password != new_password_confirmation)
            {
                TempData["Message"] = "Password confirmation does not match.";
                TempData["MessageColor"] = "alert-danger"; // Màu đỏ
                return RedirectToAction("ChangePassword", "MyOrder");
            }

            var userId = HttpContext.Session.GetString("UserId");
            var user = await db.User.FindAsync(int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            if (!BCrypt.Net.BCrypt.Verify(current_password, user.Password))
            {
                TempData["Message"] = "Current password is incorrect.";
                TempData["MessageColor"] = "alert-danger"; // Màu đỏ
                return RedirectToAction("ChangePassword", "MyOrder");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(new_password);
            db.Entry(user).State = EntityState.Modified;
            await db.SaveChangesAsync();
            TempData["Message"] = "Password changed successfully.";
            TempData["MessageColor"] = "alert-success"; // Màu xanh lá cây
            return RedirectToAction("ChangePassword", "MyOrder");
        }

        [Authentication]
        public IActionResult Profile()
        {
            return View();
        }
        [Authentication]
        public IActionResult EditProfile()
        {
            return View();
        }
    }
}

