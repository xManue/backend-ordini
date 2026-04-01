using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Core.Models
{
    public class ProductDTO
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
    }
}
