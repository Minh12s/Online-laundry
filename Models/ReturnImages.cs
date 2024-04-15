using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineJwellery_Shopping.Models
{
    public class ReturnImages
    {

        public int ReturnImagesId { get; set; }

        [ForeignKey("OrderReturn")]
        public int OrderReturnId { get; set; }

        public string ImagePath { get; set; }

        public OrderReturn OrderReturn { get; set; }
    }
}