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
        public IActionResult Product()
        {
            return View("ProductManagement/Product");
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
