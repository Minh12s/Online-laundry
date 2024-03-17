using Microsoft.AspNetCore.Mvc;

namespace OnlineJwellery_Shopping.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View("DashboardAdmin/Dashboard");
        }
    }
}
