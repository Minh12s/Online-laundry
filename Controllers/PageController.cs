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
            return View();
        }
        [Authentication]
        public async Task<IActionResult> Details()
        {
            return View();
        }
        [Authentication]
        public async Task<IActionResult> Category()
        {
            return View();
        }
        [Authentication]
        public async Task<IActionResult> Checkout()
        {
            return View();
        }
        [Authentication]
        public async Task<IActionResult> Contact()
        {
            return View();
        }
        [Authentication]
        public async Task<IActionResult> Blog()
        {
            return View();
        }
        [Authentication]
        public async Task<IActionResult> BlogDetails()
        {
            return View();
        }
        [Authentication]
        public async Task<IActionResult> About()
        {
            return View();
        }
        [Authentication]
        public async Task<IActionResult> Thankyou()
        {
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
                User u = db.User.FirstOrDefault(
                    x => x.Email.Equals(user.Email));

                if (u != null && BCrypt.Net.BCrypt.Verify(user.Password, u.Password))
                {
                    HttpContext.Session.SetString("Username", u.Username.ToString());
                    HttpContext.Session.SetString("Role", u.Role);
                    HttpContext.Session.SetString("UserId", u.UserId.ToString());

                    if (u.Role == "Admin")
                    {
                        return RedirectToAction("Home", "Page");
                    }
                    else if (u.Role == "User")
                    {
                        return RedirectToAction("Home", "Page");
                    }
                }
            }

            return View();
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

