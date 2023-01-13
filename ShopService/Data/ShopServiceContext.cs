using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopServiceDA.Models;

namespace ShopServiceDA.Data
{
    public class ShopServiceContext : DbContext
    {
        public ShopServiceContext(DbContextOptions<ShopServiceContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Product { get; set; } = default!;
        public DbSet<Material> Material { get; set; } = default!;

        public DbSet<Order> Order { get; set; } = default!;
    }
}
