using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Repositories.AuthenticationRepositories;
using InventoryManagementAPI.Repositories.CategoryRepositories;
using InventoryManagementAPI.Repositories.InvMovementRepositories;
using InventoryManagementAPI.Repositories.JWT;
using InventoryManagementAPI.Repositories.ProductRepositories;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using InventoryManagementAPI.Repositories.SupplierRepositories;
using InventoryManagementAPI.Repositories.UserRepositories;
using InventoryManagementAPI.Repositorys.ProductRepositories;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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

            // Reads the "Jwt" configuration section and registers it with ASP.NET's options system.
            // This is what lets JwtTokenService ask for IOptions<JwtSettings> and receive the configured
            // values from appsettings.json, appsettings.Development.json, user-secrets, environment variables, etc.
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            // Pull the same Jwt settings out immediately so Program.cs can configure JWT validation.
            // JwtTokenService uses these settings when it CREATES a token.
            // The AddJwtBearer setup below uses these settings when the API VALIDATES a token on later requests.
            //
            // If your secret key is stored in user-secrets, it still appears here as jwtSettings.Key
            // as long as the secret name is "Jwt:Key".
            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

            // The signing key is the shared secret used to prove the token was created by this API.
            // It should normally come from user-secrets or an environment variable, not from committed source code.
            // If Jwt:Key is missing or too short, token creation/validation will fail, so make sure it is set.

            builder.Services
                // Tells ASP.NET Core that this API uses authentication and that the default scheme is JWT Bearer.
                // "Bearer" means the client sends the token in the Authorization header:
                // Authorization: Bearer eyJhbGciOi...
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                // Configures the rules for accepting or rejecting JWT bearer tokens.
                // These rules run automatically when a request hits an endpoint protected by [Authorize].
                .AddJwtBearer(options =>
                {
                    // TokenValidationParameters defines what must be true for an incoming JWT to be trusted.
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // Checks that the token's issuer matches ValidIssuer.
                        // This confirms the token claims it came from the API/source we expect.
                        ValidateIssuer = true,

                        // Checks that the token's audience matches ValidAudience.
                        // This confirms the token was intended for this API/client combination.
                        ValidateAudience = true,

                        // Checks the token's expiry time.
                        // If the token is past its "expires" timestamp, ASP.NET rejects it.
                        ValidateLifetime = true,

                        // Checks that the token signature was created with the expected signing key.
                        // This is what prevents someone from editing the token payload and still being trusted.
                        ValidateIssuerSigningKey = true,

                        // The issuer and audience must match the values used when JwtTokenService creates the token.
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,

                        // Converts the configured secret key string into bytes and wraps it as a symmetric signing key.
                        // This must be the same key JwtTokenService used to sign the token.
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                    };
                });

            // Registers authorization services so [Authorize] and [Authorize(Roles = "...")] can be used on controllers/actions.
            // Adds AdminOrManager Policy
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOrManager", policy =>
                policy.RequireRole("Admin", "Manager"));
            });

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

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

            // Runs authentication for each request.
            // For JWT auth, this looks for an Authorization: Bearer <token> header,
            // validates the token using the rules above, and builds HttpContext.User from the token claims.
            app.UseAuthentication();

            // Runs authorization after authentication.
            // This checks endpoint rules such as [Authorize] or [Authorize(Roles = "Admin")]
            // against the user/claims created by UseAuthentication.
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
