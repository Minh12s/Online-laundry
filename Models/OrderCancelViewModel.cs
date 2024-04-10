using System;
namespace OnlineJwellery_Shopping.Models
{
    public class OrderCancelViewModel
    {
        public int OrderCancelId { get; set; }
        public int OrderId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public decimal TotalAmount { get; set; }
        public string Reason { get; set; }
    }
}

