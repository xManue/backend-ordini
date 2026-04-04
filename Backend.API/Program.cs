using Backend.API.Middleware;
using Backend.Core.Models;
using Backend.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Backend.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var port = Environment.GetEnvironmentVariable("PORT") ?? "5225";
            builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

            // Add services to the container.
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                var dbPath = Path.Combine(AppContext.BaseDirectory, "ristorante.db");
                options.UseSqlite($"Data Source={dbPath}");
            });

            var app = builder.Build();

            // ✅ Swagger sempre attivo
            app.UseSwagger();
            app.UseSwaggerUI();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
                if (!db.Products.Any())
                {
                    db.Products.AddRange(new List<ProductModel>
        {
            new ProductModel { Name = "Pizza Margherita", Price = 8.5m, Category = "pizza", IsAvailable = true },
            new ProductModel { Name = "Coca Cola", Price = 3m, Category = "bevande", IsAvailable = true },
            new ProductModel { Name = "Tiramisu", Price = 5m, Category = "dessert", IsAvailable = true },
            new ProductModel { Name = "Pizza Pepperoni", Price = 9.5m, Category = "pizza", IsAvailable = true },
            new ProductModel { Name = "Fanta", Price = 3m, Category = "bevande", IsAvailable = true },
            new ProductModel { Name = "Gelato", Price = 4m, Category = "dessert", IsAvailable = true },
            new ProductModel { Name = "Pizza Veggie", Price = 9m, Category = "pizza", IsAvailable = true },
            new ProductModel { Name = "Sprite", Price = 3m, Category = "bevande", IsAvailable = true },
        });

                    db.SaveChanges();
                }
            }

            // CORS
            app.UseCors();

            // API Key protection for admin endpoints
            app.UseMiddleware<ApiKeyMiddleware>();

            // Frontend (wwwroot)
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
