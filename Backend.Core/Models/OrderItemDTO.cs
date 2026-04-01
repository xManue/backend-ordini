using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Core.Models
{
    public class OrderItemDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
