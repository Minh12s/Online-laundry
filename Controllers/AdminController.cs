using Microsoft.AspNetCore.Mvc;

namespace OnlineJwellery_Shopping.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View("DashboardAdmin/Dashboard");
        }
        public IActionResult Customer()
        {
            return View("CustomerManagement/Customer");
        }
        public IActionResult Product()
        {
            return View("ProductManagement/Product");
        }
        public IActionResult Order()
        {
            return View("OrderManagement/Order");
        }
        public IActionResult Revenue()
        {
            return View("RevenueManagement/Revenue");
        }
    }
}
