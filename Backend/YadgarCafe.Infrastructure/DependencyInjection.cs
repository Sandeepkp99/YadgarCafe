using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YadgarCafe.Application.Services.Interfaces;
using YadgarCafe.Infrastructure.Persistence;
using YadgarCafe.Infrastructure.Repositories;
using YadgarCafe.Infrastructure.Services;

namespace YadgarCafe.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")
                ));

            // Register Category services
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();

            // Register Product services
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();

            // Register Ingredient services
            services.AddScoped<IIngredientRepository, IngredientRepository>();
            services.AddScoped<IIngredientService, IngredientService>();

            // Register Inventory services
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IInventoryService, InventoryService>();

            // Register Product Recipe services
            services.AddScoped<IProductRecipeRepository, ProductRecipeRepository>();
            services.AddScoped<IProductRecipeService, ProductRecipeService>();

            // Register Customer services
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICustomerService, CustomerService>();

            // Register Order services
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderService, OrderService>();

            return services;
        }
    }
}
