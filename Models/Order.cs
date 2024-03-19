using System;
namespace OnlineJwellery_Shopping.Models
{
        public class Order
        {
            public int OrderId { get; set; }
            public int UserId { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; }
            public string IsPaid { get; set; }

            // Thêm thông tin địa chỉ
            public string Province { get; set; } // tỉnh
            public string District { get; set; } // Quận/Huyện
            public string Ward { get; set; } // phường/xã
            public string AddressDetail { get; set; } // số nhà và tên đường

            // Phương thức trả về chuỗi địa chỉ đầy đủ
            public string GetFullAddress()
            {
                return $"{Province},{District},{Ward} - {AddressDetail}";
            }

            // Thêm thông tin người đặt hàng
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Telephone { get; set; }

            // Thêm thông tin phương thức thanh toán và vận chuyển
            public string PaymentMethod { get; set; }
            public string ShippingMethod { get; set; }


            // Navigation properties
            public User User { get; set; }
            public ICollection<OrderProduct> OrderProducts { get; set; }


        }
}

