using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using YadgarCafe.Application.Interfaces;
using YadgarCafe.Application.Services;

namespace YadgarCafe.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IIngredientService, IngredientService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IProductRecipeService, ProductRecipeService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IDashboardService, DashboardService>();

            return services;
        }
    }
}
