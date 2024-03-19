using System;
using Microsoft.EntityFrameworkCore;
namespace OnlineJwellery_Shopping.Models
{
        public class Favorite
        {
            public int FavoriteId { get; set; }
            public string ProductName { get; set; }
            public decimal Price { get; set; }
            public string Thumbnail { get; set; }
            public int UserId { get; set; }
            public int ProductId { get; set; }

            public User User { get; set; }
            public Product Product { get; set; }
        }
    }


