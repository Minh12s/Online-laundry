using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace OnlineJwellery_Shopping.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Slug { get; set; }
        public decimal Price { get; set; }
        public string Thumbnail { get; set; }
        public int Qty { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public string StoneType { get; set; }
        public decimal TotalWeight { get; set; }
        public string Color { get; set; }
        public int GoldAgeId { get; set; }
        public string Size { get; set; }
        public string Material { get; set; }
        public string CertificationCode { get; set; }


        // Navigation properties
        public Category Category { get; set; }
        public Brand Brand { get; set; }
        public GoldAge GoldAge { get; set; }
        public ICollection<OrderProduct> OrderProducts { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}