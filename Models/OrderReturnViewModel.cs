using System;


namespace OnlineJwellery_Shopping.Models
{
    public class OrderReturnViewModel
    {
        public int OrderReturnId { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public decimal RefundAmount { get; set; }
        public List<IFormFile> ImagePath { get; set; }

        public List<ReturnImages> ReturnImages { get; set; }
     


    }
}


