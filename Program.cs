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

            // Validate that the Jwt settings are present and valid. If not, throw an exception to prevent the app from starting.
            // This is important because if the Jwt settings are missing or invalid, the API will not be able to validate tokens and will reject all requests.
            if (jwtSettings is null)
            {
                throw new InvalidOperationException("Missing Jwt settings in configuration. Please check appsettings.json or user-secrets.");
            }
            if (string.IsNullOrWhiteSpace(jwtSettings.Key))
            {
                throw new InvalidOperationException("Missing Jwt:Key in configuration. Please check appsettings.json or user-secrets.");
            }
            if(string.IsNullOrWhiteSpace(jwtSettings.Issuer))
            {
                throw new InvalidOperationException("Missing Jwt:Issuer in configuration. Please check appsettings.json or user-secrets.");
            }
            if(string.IsNullOrWhiteSpace(jwtSettings.Audience))
            {
                throw new InvalidOperationException("Missing Jwt:Audience in configuration. Please check appsettings.json or user-secrets.");
            }
            if(jwtSettings.ExpirationInMinutes <= 0)
            {
                throw new InvalidOperationException("Invalid Jwt:ExpirationInMinutes in configuration. Please check appsettings.json or user-secrets.");
            }


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
                // Create a scope to access the database context and check for test users
                // If they don't exist, create them
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<InvManDBContext>();

                    var testAdmin = context.Users.FirstOrDefault(u => u.Email == "TestAdmin@email.com");
                    var testManager = context.Users.FirstOrDefault(u => u.Email == "TestManager@email.com");
                    var testStaff = context.Users.FirstOrDefault(u => u.Email == "TestStaff@email.com");

                    if (testAdmin == null)
                    {
                        var newUser = new User
                        {
                            UserName = "TestAdmin",
                            Email = "TestAdmin@email.com",
                            Password_Hash = BCrypt.Net.BCrypt.EnhancedHashPassword("TestAdmin"),
                            Role = Models.Enums.UserRoles.Admin,
                        };
                        context.Users.Add(newUser);
                        context.SaveChanges();
                    }
                    if (testManager == null)
                    {
                        var newUser = new User
                        {
                            UserName = "TestManager",
                            Email = "TestManager@email.com",
                            Password_Hash = BCrypt.Net.BCrypt.EnhancedHashPassword("TestManager"),
                            Role = Models.Enums.UserRoles.Manager,
                        };
                        context.Users.Add(newUser);
                        context.SaveChanges();
                    }
                    if (testStaff == null)
                    {
                        var newUser = new User
                        {
                            UserName = "TestStaff",
                            Email = "TestStaff@email.com",
                            Password_Hash = BCrypt.Net.BCrypt.EnhancedHashPassword("TestStaff"),
                            Role = Models.Enums.UserRoles.Staff,
                        };
                        context.Users.Add(newUser);
                        context.SaveChanges();
                    }
                }



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
