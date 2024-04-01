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
    public class MyOrderController : BaseController
    {
        private readonly JwelleryShoppingContext db;
        private readonly IConfiguration _configuration;

        public MyOrderController(JwelleryShoppingContext context, IConfiguration configuration) : base(context)
        {
            db = context;
            _configuration = configuration;
        }
     
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string returnUrl)
        {
            var order = await _context.Order
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            if (status.ToLower() == "cancel" && order.Status.ToLower() == "pending")
            {
                // Cập nhật trạng thái cho đơn hàng
                order.Status = status;

                // Trả lại số lượng sản phẩm đã mua
                foreach (var orderProduct in order.OrderProducts)
                {
                    // Kiểm tra xem số lượng trả lại có vượt quá số lượng đã mua không
                    int quantityToReturn = Math.Min(orderProduct.Qty, orderProduct.Product.Qty);

                    // Cập nhật số lượng trong kho
                    orderProduct.Product.Qty += quantityToReturn;

                }

                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            // Cập nhật trạng thái cho đơn hàng
            order.Status = status;
            _context.Update(order);
            await _context.SaveChangesAsync();


            // Chuyển hướng đến returnUrl
            return Redirect(returnUrl);
        }

        [Authentication]
        public async Task<IActionResult> OrderPending(int page = 1, int pageSize = 5)
        {
            // Kế thừa các logic chung từ BaseController
            await SetCommonViewData();

            // Lấy userId từ session
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // Truy vấn database để lấy thông tin người dùng có đơn hàng có trạng thái là "Pending"
            var user = db.User
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderProducts)
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }
            // Logic phân trang ở đây
            var totalOrders = user.Orders.Count(o => o.Status == "pending");
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            user.Orders = user.Orders
                  .Where(o => o.Status == "pending")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
              .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;



            // Trả dữ liệu người dùng có đơn hàng Pending cho view
            return View(user);
        }

        [Authentication]
        public async Task<IActionResult> OrderConfirmed(int page = 1, int pageSize = 5)
        {
            // Kế thừa các logic chung từ BaseController
            await SetCommonViewData();

            // Lấy userId từ session
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // Truy vấn database để lấy thông tin người dùng có đơn hàng có trạng thái là "Pending"
            var user = db.User
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderProducts)
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }
            // Logic phân trang ở đây
            var totalOrders = user.Orders.Count(o => o.Status == "confirmed");
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            user.Orders = user.Orders
                  .Where(o => o.Status == "confirmed")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
              .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            // Lọc chỉ những đơn hàng có trạng thái là "Pending"


            // Trả dữ liệu người dùng có đơn hàng Pending cho view
            return View(user);
        }

        [Authentication]
        public async Task<IActionResult> OrderShipping(int page = 1, int pageSize = 5)
        {
            // Kế thừa các logic chung từ BaseController
            await SetCommonViewData();

            // Lấy userId từ session
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // Truy vấn database để lấy thông tin người dùng có đơn hàng có trạng thái là "Pending"
            var user = db.User
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderProducts)
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }
            // Logic phân trang ở đây
            var totalOrders = user.Orders.Count(o => o.Status == "shipping");
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            user.Orders = user.Orders
                  .Where(o => o.Status == "shipping")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
              .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;



            // Trả dữ liệu người dùng có đơn hàng Pending cho view
            return View(user);
        }

        [Authentication]
        public async Task<IActionResult> OrderShipped(int page = 1, int pageSize = 5)
        {
            // Kế thừa các logic chung từ BaseController
            await SetCommonViewData();

            // Lấy userId từ session
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // Truy vấn database để lấy thông tin người dùng có đơn hàng có trạng thái là "Pending"
            var user = db.User
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderProducts)
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }
            // Logic phân trang ở đây
            var totalOrders = user.Orders.Count(o => o.Status == "shipped");
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            user.Orders = user.Orders
                  .Where(o => o.Status == "shipped")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
              .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;


            // Trả dữ liệu người dùng có đơn hàng Pending cho view
            return View(user);
        }

        [Authentication]
        public async Task<IActionResult> OrderComplete(int page = 1, int pageSize = 5)
        {
            // Kế thừa các logic chung từ BaseController
            await SetCommonViewData();

            // Lấy userId từ session
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // Truy vấn database để lấy thông tin người dùng có đơn hàng có trạng thái là "Pending"
            var user = db.User
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderProducts)
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }
            // Logic phân trang ở đây
            var totalOrders = user.Orders.Count(o => o.Status == "complete");
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            user.Orders = user.Orders
                  .Where(o => o.Status == "complete")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
              .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;


            // Trả dữ liệu người dùng có đơn hàng Pending cho view
            return View(user);
        }

        [Authentication]
        public async Task<IActionResult> OrderCancel(int page = 1, int pageSize = 5)
        {
            // Kế thừa các logic chung từ BaseController
            await SetCommonViewData();

            // Lấy userId từ session
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // Truy vấn database để lấy thông tin người dùng có đơn hàng có trạng thái là "Pending"
            var user = db.User
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderProducts)
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }
            // Logic phân trang ở đây
            var totalOrders = user.Orders.Count(o => o.Status == "cancel");
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            user.Orders = user.Orders
                  .Where(o => o.Status == "cancel")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
              .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;


            // Trả dữ liệu người dùng có đơn hàng Pending cho view
            return View(user);
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
            // Lấy userId từ session
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // Truy vấn database để lấy thông tin người dùng
            var user = db.User.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user); // Truyền thông tin người dùng vào view
        }

        [HttpGet]
        [Authentication]
        public IActionResult EditProfile(int id)
        {
            // Truy vấn database để lấy thông tin người dùng
            var user = db.User.FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View("EditProfile", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User model, IFormFile thumbnail)
        {
            if (true)
            {
                // Lấy userId từ session
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

                // Truy vấn database để lấy thông tin người dùng
                var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return NotFound();
                }

                // Cập nhật các thông tin mới
                user.Username = model.Username;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;

                // Xử lý tệp tin ảnh và lưu đường dẫn
                if (thumbnail != null && thumbnail.Length > 0)
                {
                    var uploadsFolder = Path.Combine("wwwroot","images", "avatars");
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
                    user.Thumbnail = "/images/avatars/" + Path.GetFileName(imagePath);
                }
                else
                {
                    var existingUser = await _context.User.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == model.UserId);
                    if (existingUser != null)
                    {
                        user.Thumbnail = existingUser.Thumbnail;
                    }
                }

                // Cập nhật thông tin người dùng
                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Profile));
            }

            // Trả về View nếu dữ liệu không hợp lệ
            return View("EditProfile", model);
        }
    }
}

