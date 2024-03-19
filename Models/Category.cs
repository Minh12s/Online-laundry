using System;
namespace OnlineJwellery_Shopping.Models
{
        public class Category
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
            public string Slug { get; set; }

        // Navigation property
        public ICollection<Product> Products { get; set; }
        }
    
}

