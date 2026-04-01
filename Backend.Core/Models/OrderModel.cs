using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Core.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public List<OrderItemModel> OrderItems { get; set; } = new();
        public OrderStatus Status { get; set; }
        public int? TableNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PaidAt { get; set; }
        public DateTime PreparingAt { get; set; }
        public DateTime ReadyAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public DateTime CancelledAt { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
