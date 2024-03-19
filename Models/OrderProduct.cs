using System;
using Microsoft.EntityFrameworkCore;
namespace OnlineJwellery_Shopping.Models
{
        [Keyless]
        public class OrderProduct
        {
            public int OrderId { get; set; }
            public int ProductId { get; set; }
            public int Qty { get; set; }
            public decimal Price { get; set; }

            // Composite primary key
            public Order Order { get; set; }
            public Product Product { get; set; }
        }
    }

