using Backend.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<OrderModel> Orders { get; set; }

        public DbSet<ProductModel> Products { get; set; }

        public DbSet<OrderItemModel> OrderItems { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}
