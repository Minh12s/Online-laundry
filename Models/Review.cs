using System;
namespace OnlineJwellery_Shopping.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
        public int RatingValue { get; set; }

        public Product Product { get; set; }
        public User User { get; set; }
    }
}

