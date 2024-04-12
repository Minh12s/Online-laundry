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
using Microsoft.CodeAnalysis;

using Microsoft.AspNetCore.Hosting;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnlineJwellery_Shopping.Controllers
{
    public class MyOrderController : BaseController
    {
        private readonly JwelleryShoppingContext db;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public MyOrderController(JwelleryShoppingContext context, IConfiguration configuration, IWebHostEnvironment env) : base(context)
        {
            db = context;
            _configuration = configuration;
            _env = env;
        }

        [Authentication]
        public async Task<IActionResult> MyOrder(String OrderDate,String IsPaid, string searchTerm, int page = 1, int pageSize = 6)
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

            // Tìm kiếm theo tên nếu có giá trị searchTerm được cung cấp
            if (!string.IsNullOrEmpty(searchTerm))
            {
                user.Orders = user.Orders
                    .Where(o => o.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            if (!string.IsNullOrEmpty(IsPaid))
            {
                // Lọc theo trạng thái đã thanh toán (Paid/Unpaid)
                bool isPaid = IsPaid == "1";
                user.Orders = user.Orders
                    .Where(o => o.IsPaid == (isPaid ? "paid" : "unpaid"))
                    .ToList();
            }
            if (!string.IsNullOrEmpty(OrderDate))
            {
                // Chuyển đổi chuỗi OrderDate thành kiểu DateTime
                DateTime orderDateValue;
                if (DateTime.TryParse(OrderDate, out orderDateValue))
                {
                    // Lọc theo ngày đặt hàng (OrderDate)
                    user.Orders = user.Orders
                        .Where(o => o.OrderDate.Date == orderDateValue.Date)
                        .ToList();
                }
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
            // Lấy danh sách orderProducts từ đơn hàng
            var orderProducts = order.OrderProducts.ToList();
            if (status.ToLower() == "complete" && order.Status.ToLower() != "complete")
            {
                // Cập nhật trạng thái cho đơn hàng
                order.Status = status;
                _context.Update(order);
                await _context.SaveChangesAsync();
                // Gửi email thông báo cho khách hàng
                await SendThankYouEmail("dungprohn1409@gmail.com", order, orderProducts);

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
        private async Task SendThankYouEmail(string recipientEmail, Order order, List<OrderProduct> orderProducts)
        {
            // Đường dẫn tới mẫu email
            string emailTemplatePath = Path.Combine(_env.ContentRootPath, "Views", "Email", "MailThankYou.cshtml");

            // Đọc nội dung mẫu email từ file
            string emailContent = await System.IO.File.ReadAllTextAsync(emailTemplatePath);

            // Thêm thông tin sản phẩm vào email content
            string productList = "";
            foreach (var ordProduct in orderProducts)
            {
                productList += $"<div class='Order_list_product'><div class='Order_list_product1'><h5>{ordProduct.Product.ProductName}</h5></div><div class='quantity'><p>Qty: {ordProduct.Qty}</p></div><div class='total'><p>${ordProduct.Price}</p></div></div>";
            }
            emailContent = emailContent.Replace("{OrderProducts}", productList);

            // Thêm tổng đơn hàng vào email content
            decimal subtotal = orderProducts.Sum(op => op.Price * op.Qty);
            decimal shippingFee = order.ShippingMethod == "Express" ? 10.00m : 20.00m; // Giả sử phí vận chuyển là $10 cho Express, $20 cho các phương thức khác
            decimal totalAmount = subtotal + shippingFee;
            emailContent = emailContent.Replace("{Subtotal}", subtotal.ToString("0.00"));
            emailContent = emailContent.Replace("{ShippingFee}", shippingFee.ToString("0.00"));
            emailContent = emailContent.Replace("{TotalAmount}", totalAmount.ToString("0.00"));

            // Tạo đối tượng MailMessage
            var message = new MailMessage();
            message.To.Add(new MailAddress(recipientEmail)); // Địa chỉ email của khách hàng
            message.From = new MailAddress(_configuration["EmailSettings:Username"]);
            message.Subject = "Thank You for Your Purchase!";
            message.Body = emailContent;
            message.IsBodyHtml = true;

            // Tạo đối tượng SmtpClient để gửi email
            using (var smtp = new SmtpClient(_configuration["EmailSettings:SmtpServer"], int.Parse(_configuration["EmailSettings:Port"])))
            {
                var credentials = new NetworkCredential
                {
                    UserName = _configuration["EmailSettings:Username"],
                    Password = _configuration["EmailSettings:Password"]
                };
                smtp.Credentials = credentials;
                smtp.EnableSsl = true;

                // Gửi email
                await smtp.SendMailAsync(message);
            }
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

        [HttpGet]
        public async Task<IActionResult> ReasonCancel()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ReasonCancel(int id, string[] Reason, string otherReason)
        {
            // Lấy thông tin đơn hàng từ cơ sở dữ liệu
            var order = await _context.Order
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status.ToLower() == "pending")
            {
                // Cập nhật trạng thái của đơn hàng thành "cancel"
                order.Status = "cancel";

                // Trả lại số lượng sản phẩm đã mua
                foreach (var orderProduct in order.OrderProducts)
                {
                    // Kiểm tra xem số lượng trả lại có vượt quá số lượng đã mua không
                    int quantityToReturn = Math.Min(orderProduct.Qty, orderProduct.Product.Qty);

                    // Cập nhật số lượng trong kho
                    orderProduct.Product.Qty += quantityToReturn;
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.Update(order);

                // Lưu lý do huỷ đơn hàng vào bảng OrderCancel (nếu có)
                if (Reason != null && Reason.Any())
                {
                    foreach (var reason in Reason)
                    {
                        // Tạo một đối tượng OrderCancel và lưu vào cơ sở dữ liệu
                        var orderCancel = new OrderCancel
                        {
                            OrderId = id,
                            Reason = reason
                        };

                        _context.OrderCancel.Add(orderCancel);
                    }
                }

                // Lưu lý do khác (nếu có)
                if (!string.IsNullOrEmpty(otherReason))
                {
                    var orderCancel = new OrderCancel
                    {
                        OrderId = id,
                        Reason = otherReason
                    };

                    _context.OrderCancel.Add(orderCancel);
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                // Gửi email thông báo huỷ đơn hàng
                await SendCancellationEmail(order);

                TempData["Message"] = "Thank you for sharing the reason for your order";
                TempData["MessageType"] = "success";
            }

            // Redirect người dùng về trang chi tiết đơn hàng
            return RedirectToAction("OrderDetail", "MyOrder", new { id = id });
        }

        private async Task SendCancellationEmail(Order order)
        {
            string recipientEmail = order.Email;
            string emailContent = GenerateCancellationEmailContent(order);

            string smtpServer = _configuration["EmailSettings:SmtpServer"];
            int port = _configuration.GetValue<int>("EmailSettings:Port");
            string username = _configuration["EmailSettings:Username"];
            string password = _configuration["EmailSettings:Password"];

            using (var client = new SmtpClient(smtpServer))
            {
                client.Port = port;
                client.Credentials = new System.Net.NetworkCredential(username, password);
                client.EnableSsl = true;

                var message = new MailMessage(username, recipientEmail)
                {
                    Subject = "Your Order Cancellation Confirmation",
                    Body = emailContent,
                    IsBodyHtml = true
                };

                try
                {
                    await client.SendMailAsync(message);
                    ViewBag.Message = "Cancellation email sent successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Error = $"Failed to send cancellation email: {ex.Message}";
                }
            }
        }

        private string GenerateCancellationEmailContent(Order order)
        {
            // Đọc nội dung mẫu email từ file
            string emailTemplatePath = _env.ContentRootPath + "/Views/Email/CancelEmail.cshtml";
            string emailContent = System.IO.File.ReadAllText(emailTemplatePath);

            // Thay thế các placeholder trong mẫu email bằng thông tin đơn hàng
            emailContent = emailContent.Replace("{OrderNumber}", order.OrderId.ToString());
            emailContent = emailContent.Replace("{OrderDate}", order.OrderDate.ToString("dd/MM/yyyy HH:mm"));
            emailContent = emailContent.Replace("{CustomerName}", order.FullName);
            emailContent = emailContent.Replace("{CustomerEmail}", order.Email);
            emailContent = emailContent.Replace("{Status}", order.Status);

            return emailContent;
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
                    var uploadsFolder = Path.Combine("wwwroot", "images", "avatars");
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
        [Authentication]
        public async Task<IActionResult> Review(int productId)
        {
            // Lấy thông tin người dùng từ HttpContext
            var userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            ViewBag.ProductId = productId;
            ViewBag.UserId = userId;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Review(int productId, int ratingValue, string comment, string email)
        {
            // Kiểm tra xem sản phẩm có tồn tại không
            var product = await _context.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "The product does not exist or cannot be reviewed.";
                TempData["MessageType"] = "error";
                TempData["ProductId"] = productId;
                return RedirectToAction("Review", new { productId = productId });
            }

            // Lấy userId từ session
            var userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // Lưu đánh giá vào trạng thái "pending"
            var review = new Review
            {
                UserId = userId,
                ProductId = productId,
                RatingValue = ratingValue,
                Comment = comment,
                ReviewDate = DateTime.Now,
                Status = "pending"
            };

            // Thêm đánh giá vào cơ sở dữ liệu
            _context.Review.Add(review);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Thank you for your product review";
            TempData["MessageType"] = "success";
            return RedirectToAction("OrderComplete", "MyOrder");
        }

    }
}
