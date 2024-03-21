using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnlineJwellery_Shopping.Heplers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineJwellery_Shopping.Data;
using OnlineJwellery_Shopping.Models;
using Microsoft.AspNetCore.Session;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnlineJwellery_Shopping.Controllers
{
    public class CartController : BaseController
    {

            private readonly JwelleryShoppingContext _context;

            public CartController(JwelleryShoppingContext context) : base(context)
            {
                _context = context;
            }


            public List<CartItem> Carts
            {
                get
                {
                    //var data = HttpContext.Session.Get<List<CartItem>>("cart");
                    var data = HttpContext.Session.Get<List<CartItem>>("cart");
                    if (data != null)
                    {
                        data = new List<CartItem>();
                    }
                    return data;
                }
            }

            public IActionResult AddToCart(int id, int quantity = 1)
            {
                List<CartItem> myCart;

                // Lấy giỏ hàng từ session nếu tồn tại
                if (HttpContext.Session.TryGetValue("cart", out byte[] cartBytes))
                {
                    myCart = JsonConvert.DeserializeObject<List<CartItem>>(Encoding.UTF8.GetString(cartBytes));
                }
                else
                {
                    myCart = new List<CartItem>();
                }

                var existingItem = myCart.FirstOrDefault(p => p.ProductId == id);

                if (existingItem != null)
                {
                    // Tính tổng số lượng bao gồm số lượng đang thêm vào
                    int totalQuantity = existingItem.Qty + quantity;

                    // Kiểm tra nếu tổng số lượng vượt quá số lượng có sẵn trong kho
                    var product = _context.Product.FirstOrDefault(p => p.ProductId == id);
                    if (product != null && totalQuantity > product.Qty)
                    {
                        // Số lượng vượt quá số lượng có sẵn, đặt TempData và chuyển hướng
                        TempData["Message"] = "Requested quantity exceeds available quantity!";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Details", "Page", new { id = id });
                    }

                    // Cập nhật số lượng của mặt hàng hiện có
                    existingItem.Qty = totalQuantity;
                }
                else
                {
                    // Mặt hàng không tồn tại trong giỏ hàng, thêm mặt hàng mới
                    var product = _context.Product.FirstOrDefault(p => p.ProductId == id);
                    if (product != null)
                    {
                        myCart.Add(new CartItem
                        {
                            ProductId = id,
                            ProductName = product.ProductName,
                            Qty = quantity,
                            Thumbnail = product.Thumbnail,
                            Price = product.Price
                        });

                        // Thêm thông báo vào TempData
                        TempData["Message"] = "Your product has been added to the cart!!";
                    TempData["MessageType"] = "success";
                }
                }

                // Lưu giỏ hàng mới vào session
                HttpContext.Session.Set("cart", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(myCart)));

                // Chuyển hướng về trang chi tiết sản phẩm
                return RedirectToAction("Details", "Page", new { id = id });
            }





            public IActionResult RemoveFromCart(int id)
            {
                List<CartItem> myCart;

                if (HttpContext.Session.TryGetValue("cart", out byte[] cartBytes))
                {
                    myCart = JsonConvert.DeserializeObject<List<CartItem>>(Encoding.UTF8.GetString(cartBytes));
                }
                else
                {
                    myCart = new List<CartItem>();
                }

                var item = myCart.SingleOrDefault(p => p.ProductId == id);

                if (item != null)
                {
                    myCart.Remove(item);
                    HttpContext.Session.Set("cart", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(myCart)));
                }

                return RedirectToAction("Index");
            }

            public async Task<IActionResult> Index()
            {
                // Lấy danh sách sản phẩm từ Session hoặc từ bất kỳ nguồn dữ liệu nào khác
                List<CartItem> cartItems = HttpContext.Session.Get<List<CartItem>>("cart");

                ViewData["CartItemCount"] = cartItems != null ? cartItems.Count : 0;
                var usernameFromSession = HttpContext.Session.GetString("Username");
                var currentUser = await _context.User.SingleOrDefaultAsync(u => u.Username == usernameFromSession);

                if (currentUser == null)
                {
                    RedirectToAction("login", "Page");
                }

                ViewBag.FavoriteCount = GetFavoriteCount(currentUser.UserId);
                return View(cartItems);
            }


            [HttpPost]
            public IActionResult UpdateQuantity(int productId, int quantity)
            {
                List<CartItem> myCart = HttpContext.Session.Get<List<CartItem>>("cart");
                CartItem item = myCart.FirstOrDefault(p => p.ProductId == productId);

                if (item != null)
                {
                    var product = _context.Product.SingleOrDefault(p => p.ProductId == productId);
                    if (product != null)
                    {
                        if (quantity <= product.Qty)
                        {
                            item.Qty = quantity;
                        }
                        else
                        {
                            // Số lượng yêu cầu vượt quá số lượng có sẵn

                            TempData["ProductId"] = productId; // Thiết lập ProductId để hiển thị thông báo cho sản phẩm này
                        }
                    }
                }

                HttpContext.Session.Set("cart", myCart);

                return RedirectToAction("Index");
            }



            [HttpPost]
          
            public IActionResult ClearCart()
            {
                HttpContext.Session.Remove("cart"); // Xóa giỏ hàng từ Session

                return RedirectToAction("Index");
            }



        }
    }


