using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Helpers;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Api.Configurations;

public static class DomainServicesConfigurationExtension
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<Dsw2025TpiContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            
            options.UseSeeding((context, _) =>
            {
                ((Dsw2025TpiContext)context).Seedwork<Customer>("Sources/customers.json");
                ((Dsw2025TpiContext)context).Seedwork<Product>("Sources/products.json");
                
            });
        });

        
        services.AddScoped<IRepository, EfRepository>();
        services.AddTransient<ProductService>();
        services.AddTransient<OrderService>();

        return services;
    }
}
