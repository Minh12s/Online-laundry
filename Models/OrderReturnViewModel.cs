using System;


namespace OnlineJwellery_Shopping.Models
{
    public class OrderReturnViewModel
    {
        public int OrderId { get; set; }
       
        public int UserId { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public decimal RefundAmount { get; set; }
        public List<IFormFile> ImagePath { get; set; }
    }
}

