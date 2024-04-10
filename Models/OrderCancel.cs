using System;
namespace OnlineJwellery_Shopping.Models
{
    public class OrderCancel
    {
        public int OrderCancelId { get; set; }
        public int OrderId { get; set; }
        public string Reason { get; set; }

        // Navigation property
        public Order Order { get; set; }
    }
}

