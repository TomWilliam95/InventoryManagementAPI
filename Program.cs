using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Repositories.AuthenticationRepositories;
using InventoryManagementAPI.Repositories.CategoryRepositories;
using InventoryManagementAPI.Repositories.InvMovementRepositories;
using InventoryManagementAPI.Repositories.ProductRepositories;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using InventoryManagementAPI.Repositories.SupplierRepositories;
using InventoryManagementAPI.Repositories.UserRepositories;
using InventoryManagementAPI.Repositorys.ProductRepositories;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("SSMS");
            builder.Services.AddDbContext<InvManDBContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            // Grabs the JwtSettings section from appsettings.json and binds it to the JwtSettings class, making it available for dependency injection.
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();

            builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
            builder.Services.AddScoped<ISupplierService, SupplierService>();

            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
            builder.Services.AddScoped<IInventoryMovementService, InventoryMovementService>();
            

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
