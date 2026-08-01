using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YadgarCafe.Domain.Entities;

namespace YadgarCafe.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #region DbSets

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Ingredient> Ingredients => Set<Ingredient>();
        public DbSet<ProductRecipe> ProductRecipes => Set<ProductRecipe>();
        public DbSet<Inventory> Inventories => Set<Inventory>();

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Staff> Staffs => Set<Staff>();

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ConfigureCategory(builder);
            ConfigureProduct(builder);
            ConfigureIngredient(builder);
            ConfigureInventory(builder);
            ConfigureStaff(builder);
            ConfigureProductRecipe(builder);
            ConfigureOrder(builder);
            ConfigureOrderItem(builder);
        }

        #region Entity Configurations

        private static void ConfigureCategory(ModelBuilder builder)
        {
            builder.Entity<Category>(entity =>
            {
                entity.Property(x => x.Name)
                      .IsRequired()
                      .HasMaxLength(100);
            });
        }

        private static void ConfigureProduct(ModelBuilder builder)
        {
            builder.Entity<Product>(entity =>
            {
                entity.Property(x => x.Price)
                      .HasPrecision(18, 2);
            });
        }

        private static void ConfigureIngredient(ModelBuilder builder)
        {
            builder.Entity<Ingredient>(entity =>
            {
                entity.Property(x => x.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(x => x.CurrentStock)
                      .HasPrecision(18, 2);

                entity.Property(x => x.MinimumStock)
                      .HasPrecision(18, 2);
            });
        }

        private static void ConfigureInventory(ModelBuilder builder)
        {
            builder.Entity<Inventory>(entity =>
            {
                entity.Property(x => x.Quantity)
                      .HasPrecision(18, 2);

                entity.HasOne(x => x.Ingredient)
                      .WithMany()
                      .HasForeignKey(x => x.IngredientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureStaff(ModelBuilder builder)
        {
            builder.Entity<Staff>(entity =>
            {
                entity.Property(x => x.Name)
                      .IsRequired()
                      .HasMaxLength(100);
            });
        }

        private static void ConfigureProductRecipe(ModelBuilder builder)
        {
            builder.Entity<ProductRecipe>(entity =>
            {
                entity.HasKey(x => new
                {
                    x.ProductId,
                    x.IngredientId
                });

                entity.Property(x => x.Quantity)
                      .HasPrecision(18, 2);
            });
        }

        private static void ConfigureOrder(ModelBuilder builder)
        {
            builder.Entity<Order>(entity =>
            {
                entity.Property(x => x.TotalAmount)
                      .HasPrecision(18, 2);

                entity.HasOne(x => x.Customer)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(x => x.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureOrderItem(ModelBuilder builder)
        {
            builder.Entity<OrderItem>(entity =>
            {
                entity.Property(x => x.UnitPrice)
                      .HasPrecision(18, 2);

                entity.Property(x => x.TotalPrice)
                      .HasPrecision(18, 2);

                entity.HasOne(x => x.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(x => x.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        #endregion
    }
}

