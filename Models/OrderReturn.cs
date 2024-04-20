using System;

namespace OnlineJwellery_Shopping.Models
{
    public class OrderReturn
    {
        public int OrderReturnId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public decimal RefundAmount { get; set; }
        public string RejectReason { get; set; } // Thêm thuộc tính lý do từ chối hoàn trả

        public Order Order { get; set; }
        public User User { get; set; }
        public Product Product { get; set; }

        public ICollection<ReturnImages> ReturnImages { get; set; }
    }
}