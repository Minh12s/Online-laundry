using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJwellery_Shopping.Data;
using OnlineJwellery_Shopping.Models.Authentication;
using OnlineJwellery_Shopping.Models;
using Microsoft.AspNetCore.Http;
using OnlineJwellery_Shopping.Heplers;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnlineJwellery_Shopping.Controllers
{
   
    public class FavoriteController : Controller
    {
        private readonly JwelleryShoppingContext _context;
        public FavoriteController(JwelleryShoppingContext context)
        {
            _context = context;
        }
        [Authentication]
        public IActionResult Index()
        {
            var cartItems = HttpContext.Session.Get<List<CartItem>>("cart");
            ViewData["CartItemCount"] = cartItems != null ? cartItems.Count : 0;

            // Lấy thông tin người dùng từ Session
            var usernameFromSession = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(usernameFromSession))
            {

                return RedirectToAction("login", "Page");
            }

            // Lấy thông tin người dùng từ database
            var currentUser = _context.User.SingleOrDefault(u => u.Username == usernameFromSession);

            if (currentUser == null)
            {

                return RedirectToAction("login", "Page");
            }

            // Lấy danh sách yêu thích của người dùng hiện tại
            var favorites = _context.Favorite
                .Include(f => f.User)
                .Include(f => f.Product)
                .Where(f => f.UserId == currentUser.UserId)
                .ToList();
            ViewBag.FavoriteCount = GetFavoriteCount(currentUser.UserId);
            // Truyền danh sách yêu thích đến view
            return View(favorites);
        }
        [Authentication]

        public IActionResult AddToFavorite(int productId)
        {
            var usernameFromSession = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(usernameFromSession))
            {
                TempData["Message"] = "Invalid user.";
                return RedirectToAction("login", "Page");
            }

            var currentUser = _context.User.SingleOrDefault(u => u.Username == usernameFromSession);

            if (currentUser == null)
            {
                return RedirectToAction("login", "Page");
            }

            // Kiểm tra xem sản phẩm đã tồn tại trong danh sách yêu thích của người dùng chưa
            var existingFavorite = _context.Favorite
                .Where(f => f.UserId == currentUser.UserId && f.ProductId == productId)
                .FirstOrDefault();

            if (existingFavorite != null)
            {
                // Nếu sản phẩm đã tồn tại trong danh sách yêu thích, xóa nó khỏi danh sách yêu thích
                _context.Favorite.Remove(existingFavorite);
                _context.SaveChanges();

                // Hiển thị thông báo sản phẩm đã bị xóa khỏi danh sách yêu thích
                TempData["Message"] = "Sản phẩm đã bị xóa khỏi danh sách yêu thích.";
                return RedirectToAction("details", "Page", new { id = productId });
            }

            // Lấy thông tin sản phẩm từ cơ sở dữ liệu
            var product = _context.Product.SingleOrDefault(p => p.ProductId == productId);

            if (product == null)
            {
                TempData["Message"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("details", "Page", new { id = productId });
            }

            // Nếu sản phẩm chưa tồn tại trong danh sách yêu thích, thêm mới
            var newFavorite = new Favorite
            {
                UserId = currentUser.UserId,
                ProductId = productId,
                ProductName = product.ProductName,
                Price = product.Price,
                Thumbnail = product.Thumbnail
            };

            _context.Favorite.Add(newFavorite);
            _context.SaveChanges();

            TempData["Message"] = "Đã thêm sản phẩm vào danh sách yêu thích.";
            return RedirectToAction("details", "Page", new { id = productId });
        }

        // xoá 1 sản phẩm

        [HttpPost]
        public IActionResult RemoveFromFavorites(int favoriteId)
        {
            var usernameFromSession = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(usernameFromSession))
            {
                return RedirectToAction("login", "Page");
            }

            var currentUser = _context.User.SingleOrDefault(u => u.Username == usernameFromSession);

            if (currentUser == null)
            {
                return RedirectToAction("login", "Page");
            }

            // Kiểm tra xem favoriteId thuộc về người dùng hiện tại không
            var favoriteToRemove = _context.Favorite
                .Where(f => f.UserId == currentUser.UserId && f.FavoriteId == favoriteId)
                .FirstOrDefault();

            if (favoriteToRemove != null)
            {
                // Xóa sản phẩm khỏi danh sách yêu thích
                _context.Favorite.Remove(favoriteToRemove);
                _context.SaveChanges();

                // Đặt TempData cho thông báo
                TempData["Message"] = "Sản phẩm đã bị xóa khỏi danh sách yêu thích.";
            }

            return RedirectToAction("Index", "Favorite");
        }

        // xoá hết sản phẩm

        [HttpPost]
        public IActionResult ClearFavorites()
        {
            var usernameFromSession = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(usernameFromSession))
            {

                return RedirectToAction("login", "Page");
            }

            var currentUser = _context.User.SingleOrDefault(u => u.Username == usernameFromSession);

            if (currentUser == null)
            {
                return RedirectToAction("login", "Page");
            }

            // Lấy danh sách yêu thích của người dùng và xoá hết
            var userFavorites = _context.Favorite
                .Where(f => f.UserId == currentUser.UserId)
                .ToList();

            _context.Favorite.RemoveRange(userFavorites);
            _context.SaveChanges();



            return RedirectToAction("Index", "Favorite");
        }

        // lấy số lượng 
        private int GetFavoriteCount(int userId)
        {
            return _context.Favorite
                .Where(f => f.UserId == userId)
                .Count();
        }


    }
}

