using Microsoft.AspNetCore.Mvc;
//using OgainShop.Models;
//using OgainShop.Data;
using Microsoft.AspNetCore.Http;
//using OgainShop.Models.Authentication;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System;
using System.Text;
//using OgainShop.Heplers;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnlineJwellery_Shopping.Controllers
{
    public class PageController : Controller
    {
        // GET: /<controller>/
        public async Task<IActionResult> Home()
        {
            return View();
        }
        public async Task<IActionResult> Details()
        {
            return View();
        }
        public async Task<IActionResult> Category()
        {
            return View();
        }
        public async Task<IActionResult> Checkout()
        {
            return View();
        }
        public async Task<IActionResult> Contact()
        {
            return View();
        }
        public async Task<IActionResult> Blog()
        {
            return View();
        }
        public async Task<IActionResult> About()
        {
            return View();
        }
    }
}

