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
        public IActionResult Product()
        {
            return View("ProductManagement/Product");
        }
        [Authentication]
        public IActionResult Order()
        {
            return View("OrderManagement/Order");
        }
        [Authentication]
        public IActionResult Revenue()
        {
            return View("RevenueManagement/Revenue");
        }
    }
}
