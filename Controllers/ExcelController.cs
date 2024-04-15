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
using Microsoft.AspNetCore.Mvc.Rendering;
using static NuGet.Packaging.PackagingConstants;
using System.IO;
using System.ComponentModel;
using ClosedXML.Excel;

namespace OnlineJwellery_Shopping.Controllers
{
    public class ExcelController : Controller
    {
        private readonly JwelleryShoppingContext _context;

        public ExcelController(JwelleryShoppingContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> ExportProductsToExcel()
        {
            var productList = await _context.Product.ToListAsync();
            return Json(productList);
        }
        [HttpGet]
        public async Task<IActionResult> ExportOrdersToExcel()
        {
            var orders = await _context.Order.ToListAsync();
            return Json(orders);
        }
        [HttpGet]
        public async Task<IActionResult> ExportCustomerToExcel()
        {
            var customers = await _context.User.ToListAsync();
            return Json(customers);
        }
        [HttpGet]
        public async Task<IActionResult> ExportBlogToExcel()
        {
            var blogs = await _context.Blog.ToListAsync();
            return Json(blogs);
        }
        [HttpGet]
        public async Task<IActionResult> ExportReviewToExcel(
    decimal? Price_from,
    decimal? Price_to,
    string ProductName,
    double? AvgRating)
        {
            var productsWithAvgRating = _context.Product
                .Select(p => new ProductWithAvgRating
                {
                    Product = p,
                    AvgRating = p.Reviews.Any() ? Math.Round(p.Reviews.Average(r => r.RatingValue), 1) : 0
                })
                .AsQueryable();

            // Áp dụng các tiêu chí lọc nếu chúng được cung cấp
            if (Price_from.HasValue)
            {
                productsWithAvgRating = productsWithAvgRating.Where(p => p.Product.Price >= Price_from);
            }

            if (Price_to.HasValue)
            {
                productsWithAvgRating = productsWithAvgRating.Where(p => p.Product.Price <= Price_to);
            }

            if (!string.IsNullOrEmpty(ProductName))
            {
                productsWithAvgRating = productsWithAvgRating.Where(p => p.Product.ProductName.Contains(ProductName));
            }

            if (AvgRating.HasValue)
            {
                productsWithAvgRating = productsWithAvgRating.Where(p => Math.Round(p.AvgRating, 1) == Math.Round(AvgRating.Value, 1));
            }

            var productList = productsWithAvgRating.ToList();

            // Trả về tập tin Excel
            return ExcelExport(productList);
        }

        public static class ExcelHelper
        {
            public static byte[] Export<T>(IEnumerable<T> data, string sheetName)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(sheetName);

                    // Ghi dữ liệu vào sheet Excel
                    worksheet.Cell(1, 1).InsertData(data);

                    // Chuyển workbook thành một mảng byte để trả về
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return stream.ToArray();
                    }
                }
            }
        }
        // Phương thức xuất Excel
        private IActionResult ExcelExport(IEnumerable<ProductWithAvgRating> productList)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Review");

                // Thêm tên cột
                worksheet.Cell(1, 1).Value = "Product Id";
                worksheet.Cell(1, 2).Value = "Thumbnail";
                worksheet.Cell(1, 3).Value = "Product Name";
                worksheet.Cell(1, 4).Value = "Price";
                worksheet.Cell(1, 5).Value = "Average Rating";

                // Dữ liệu bắt đầu từ dòng 2
                int row = 2;

                // Thêm dữ liệu từ danh sách sản phẩm
                foreach (var product in productList)
                {
                    worksheet.Cell(row, 1).Value = product.Product.ProductId;
                    worksheet.Cell(row, 2).Value = product.Product.Thumbnail;
                    worksheet.Cell(row, 3).Value = product.Product.ProductName;
                    worksheet.Cell(row, 4).Value = product.Product.Price;
                    worksheet.Cell(row, 5).Value = product.AvgRating;
                    row++;
                }

                // Lưu workbook vào MemoryStream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    // Trả về file Excel
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Review.xlsx");
                }
            }
        }
        public IActionResult ExportCanceledOrdersToExcel()
        {
            var canceledOrders = _context.OrderCancel
                .Include(oc => oc.Order)
                .Select(oc => new OrderCancelViewModel
                {
                    OrderCancelId = oc.OrderCancelId,
                    OrderId = oc.OrderId,
                    FullName = oc.Order.FullName,
                    Email = oc.Order.Email,
                    Telephone = oc.Order.Telephone,
                    TotalAmount = oc.Order.TotalAmount,
                    Reason = oc.Reason
                })
                .ToList();

            // Tạo workbook mới
            using (var workbook = new XLWorkbook())
            {
                // Tạo worksheet
                var worksheet = workbook.Worksheets.Add("CanceledOrders");

                // Thêm tên cột
                worksheet.Cell(1, 1).Value = "Order Cancel Id";
                worksheet.Cell(1, 2).Value = "Order Id";
                worksheet.Cell(1, 3).Value = "Full Name";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "Telephone";
                worksheet.Cell(1, 6).Value = "Total Amount";
                worksheet.Cell(1, 7).Value = "Reason";

                // Dữ liệu bắt đầu từ dòng 2
                int row = 2;

                // Thêm dữ liệu từ danh sách đơn hàng đã huỷ
                foreach (var order in canceledOrders)
                {
                    worksheet.Cell(row, 1).Value = order.OrderCancelId;
                    worksheet.Cell(row, 2).Value = order.OrderId;
                    worksheet.Cell(row, 3).Value = order.FullName;
                    worksheet.Cell(row, 4).Value = order.Email;
                    worksheet.Cell(row, 5).Value = order.Telephone;
                    worksheet.Cell(row, 6).Value = order.TotalAmount;
                    worksheet.Cell(row, 7).Value = order.Reason;
                    row++;
                }

                // Lưu workbook vào MemoryStream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    // Trả về file Excel
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CanceledOrders.xlsx");
                }
            }
        }

    }
}
