using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YadgarCafe.Domain.Common;
using YadgarCafe.Domain.Entities;

namespace YadgarCafe.Infrastructure.Persistence.Configurations
{
    internal static class ConfigurationExtensions
    {
        public const string NotDeleted = "[IsDeleted] = 0";

        /// <summary>Common column rules shared by every BaseEntity.</summary>
        public static void ConfigureBase<T>(this EntityTypeBuilder<T> b) where T : BaseEntity
        {
            b.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            b.Property(x => x.ModifiedBy).HasMaxLength(100);
        }
    }

    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> b)
        {
            b.ConfigureBase();
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.HasIndex(x => x.Name).IsUnique().HasFilter(ConfigurationExtensions.NotDeleted);
        }
    }

    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> b)
        {
            b.ConfigureBase();
            b.Property(x => x.Name).IsRequired().HasMaxLength(150);
            b.Property(x => x.Price).HasPrecision(18, 2);
            b.HasIndex(x => x.Name).IsUnique().HasFilter(ConfigurationExtensions.NotDeleted);
            b.HasIndex(x => x.CategoryId);

            b.HasOne(x => x.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
    {
        public void Configure(EntityTypeBuilder<Ingredient> b)
        {
            b.ConfigureBase();
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Unit).IsRequired().HasMaxLength(20);
            b.Property(x => x.CurrentStock).HasPrecision(18, 3);
            b.Property(x => x.MinimumStock).HasPrecision(18, 3);
            b.Property(x => x.RowVersion).IsRowVersion();
            b.Ignore(x => x.IsLowStock);
            b.HasIndex(x => x.Name).IsUnique().HasFilter(ConfigurationExtensions.NotDeleted);
        }
    }

    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> b)
        {
            b.ConfigureBase();
            b.Property(x => x.Quantity).HasPrecision(18, 3);
            b.Property(x => x.BalanceAfter).HasPrecision(18, 3);
            b.Property(x => x.Remarks).HasMaxLength(500);
            b.HasIndex(x => new { x.IngredientId, x.StockDate });
            b.HasIndex(x => x.OrderId);

            b.HasOne(x => x.Ingredient)
                .WithMany()
                .HasForeignKey(x => x.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ProductRecipeConfiguration : IEntityTypeConfiguration<ProductRecipe>
    {
        public void Configure(EntityTypeBuilder<ProductRecipe> b)
        {
            b.ConfigureBase();
            b.Property(x => x.Quantity).HasPrecision(18, 3);
            b.HasIndex(x => new { x.ProductId, x.IngredientId }).IsUnique();

            b.HasOne(x => x.Product)
                .WithMany(p => p.Recipes)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Ingredient)
                .WithMany(i => i.ProductRecipes)
                .HasForeignKey(x => x.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Recipes are hard deleted, so they follow their product/ingredient instead of an IsDeleted flag.
            b.HasQueryFilter(x => !x.Product.IsDeleted && !x.Ingredient.IsDeleted);
        }
    }

    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> b)
        {
            b.ConfigureBase();
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);
            b.Property(x => x.Address).HasMaxLength(300);

            // Walk-in customers may have no phone, so only real numbers must be unique.
            b.HasIndex(x => x.PhoneNumber).IsUnique()
                .HasFilter("[IsDeleted] = 0 AND [PhoneNumber] <> ''");

            b.HasIndex(x => x.UserId).IsUnique()
                .HasFilter("[IsDeleted] = 0 AND [UserId] IS NOT NULL");
        }
    }

    public class StaffConfiguration : IEntityTypeConfiguration<Staff>
    {
        public void Configure(EntityTypeBuilder<Staff> b)
        {
            b.ConfigureBase();
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.PhoneNumber).HasMaxLength(20);
            b.Property(x => x.Designation).HasMaxLength(100);
        }
    }

    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> b)
        {
            b.ConfigureBase();
            b.Property(x => x.TotalAmount).HasPrecision(18, 2);

            // Domain methods guard transitions, so EF fills these through the private setters.
            b.Property(x => x.Status);
            b.Property(x => x.PaymentStatus);

            b.HasIndex(x => new { x.CustomerId, x.CreatedOn });
            b.HasIndex(x => new { x.Status, x.CreatedOn });

            b.HasOne(x => x.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Navigation(x => x.OrderItems).AutoInclude(false);
        }
    }

    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> b)
        {
            b.ConfigureBase();
            b.Property(x => x.ProductName).IsRequired().HasMaxLength(150);
            b.Property(x => x.UnitPrice).HasPrecision(18, 2);
            b.Property(x => x.TotalPrice).HasPrecision(18, 2);
            b.HasIndex(x => x.OrderId);

            b.HasOne(x => x.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> b)
        {
            b.ConfigureBase();
            b.Property(x => x.TokenHash).IsRequired().HasMaxLength(128);
            b.Ignore(x => x.IsActive);
            b.HasIndex(x => x.TokenHash).IsUnique();
            b.HasIndex(x => x.UserId);

            b.HasOne(x => x.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> b)
        {
            b.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        }
    }
}
