using System;
namespace OnlineJwellery_Shopping.Models
{
  
        public class User
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string? Thumbnail { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            public string Email { get; set; }
            public string Role { get; set; } 

            // Navigation property
            public ICollection<Order> Orders { get; set; }
        }
    
}

