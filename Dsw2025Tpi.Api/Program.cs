using Dsw2025Tpi.Api.Configurations;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Helpers;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Dsw2025Tpi.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Desarrollo de Software TPI",
                Version = "v1",
            });
            o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Ingresar el token",
                Type = SecuritySchemeType.ApiKey
            });
            o.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
        });

        builder.Services.AddHealthChecks();

        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password = new PasswordOptions
            {
                RequiredLength = 8,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true
            };

        })
               .AddEntityFrameworkStores<AuthenticateContext>()
               .AddDefaultTokenProviders();

        var jwtConfig = builder.Configuration.GetSection("Jwt");
        var keyText = jwtConfig["Key"] ?? throw new ArgumentNullException("JWT Key");
        var key = Encoding.UTF8.GetBytes(keyText);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig["Issuer"],
                    ValidAudience = jwtConfig["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        builder.Services.AddDomainServices(builder.Configuration);

        builder.Services.AddDbContext<Dsw2025TpiContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("Dsw2025TpiEntities"));
        });

        builder.Services.AddDbContext<AuthenticateContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("Dsw2025TpiEntities"));
        });

        builder.Services.AddScoped<JwtTokenService>();

        builder.Services.AddAuthorization();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("PermitirFrontend", policy =>
                policy.WithOrigins("http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });

        var app = builder.Build();

        // middleware de manejo de errores
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage(); 
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseExceptionHandler("/error"); 
        }

        
        app.Map("/error", (HttpContext httpContext) =>
        {
            var exception = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
            
            return Results.Problem(
                title: "Ha ocurrido un error inesperado.",
                detail: exception?.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        });

        app.UseHttpsRedirection();

        app.UseCors("PermitirFrontend");

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.MapHealthChecks("/healthcheck");

        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { "Admin", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
            context.Seedwork<Product>("Sources/products.json");
            context.Seedwork<Customer>("Sources/customers.json");
        }

        await app.RunAsync();
    }
}
