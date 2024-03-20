using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineJwellery_Shopping.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OnlineJwellery_Shopping.Models;
using Microsoft.AspNetCore.Http;
using OnlineJwellery_Shopping.Models.Authentication;

using OnlineJwellery_Shopping.Heplers;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnlineJwellery_Shopping.Controllers
{
    public class BaseController : Controller
    {
        protected readonly JwelleryShoppingContext _context;

        public BaseController(JwelleryShoppingContext context)
        {
            _context = context;
        }

        protected int GetFavoriteCount(int userId)
        {
            return _context.Favorite
                .Where(f => f.UserId == userId)
                .Count();
        }
        //protected async Task SetCommonViewData()
        //{
        //    var cartItems = HttpContext.Session.Get<List<CartItem>>("cart");
        //    ViewData["CartItemCount"] = cartItems != null ? cartItems.Count : 0;

        //    var usernameFromSession = HttpContext.Session.GetString("Username");
        //    var currentUser = await _context.User.SingleOrDefaultAsync(u => u.Username == usernameFromSession);

        //    if (currentUser == null)
        //    {
        //        RedirectToAction("login", "Page");
        //    }

        //    ViewBag.FavoriteCount = GetFavoriteCount(currentUser.UserId);
        //}
    }
}

