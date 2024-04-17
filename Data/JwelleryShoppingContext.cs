using System;
using Microsoft.EntityFrameworkCore;
using OnlineJwellery_Shopping.Models;

namespace OnlineJwellery_Shopping.Data
{
    public class JwelleryShoppingContext : DbContext
    {
        public JwelleryShoppingContext(DbContextOptions<JwelleryShoppingContext> options)
            : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Brand> Brand { get; set; }
        public DbSet<GoldAge> GoldAge { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderProduct> OrderProduct { get; set; }
        public DbSet<OrderCancel> OrderCancel { get; set; }
        public DbSet<OrderReturn> OrderReturn { get; set; }
        public DbSet<ReturnImages> ReturnImages { get; set; }
        public DbSet<Favorite> Favorite { get; set; }
        public DbSet<Review> Review { get; set; }
        public DbSet<Blog> Blog { get; set; }
      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderProduct>()
              .HasKey(op => new { op.OrderId, op.ProductId });

            modelBuilder.Entity<OrderProduct>()
                        .HasOne(op => op.Order)
                        .WithMany(o => o.OrderProducts)
                        .HasForeignKey(op => op.OrderId);

            modelBuilder.Entity<OrderProduct>()
                        .HasOne(op => op.Product)
                        .WithMany(p => p.OrderProducts)
                        .HasForeignKey(op => op.ProductId);

            modelBuilder.Entity<OrderReturn>()
               .HasOne(or => or.Order)
               .WithMany()
               .HasForeignKey(or => or.OrderId);
            modelBuilder.Entity<OrderReturn>()
            .HasOne(or => or.Product)
            .WithMany()
            .HasForeignKey(or => or.ProductId);



            // Thiết lập quan hệ giữa bảng Review và User
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Tuỳ chọn này sẽ xóa tất cả các đánh giá liên quan khi người dùng bị xóa

            base.OnModelCreating(modelBuilder);
        }
    }
}
