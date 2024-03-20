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
        [Authentication]
        public IActionResult MyOrder()
        {
            return View();
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

