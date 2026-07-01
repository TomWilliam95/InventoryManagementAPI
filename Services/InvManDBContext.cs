using InventoryManagementAPI.Models.CoreModels;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Services
{
    public class InvManDBContext: DbContext
    {
        public InvManDBContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Configure enum to string conversion for UserRole and Movement properties
            modelBuilder.Entity<User>().Property(u => u.Role).HasConversion<string>();
            modelBuilder.Entity<InventoryMovement>().Property(im => im.Movement).HasConversion<string>();

            //Setup price column without risk of truncation
            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
            //Sets Default value to current for Updated and Created properties when creating product and User.
            modelBuilder.Entity<Product>().Property(p => p.Updated).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Product>().Property(p => p.Created).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<User>().Property(u => u.Created).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<User>().Property(u => u.LastLogin).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<InventoryMovement>().Property(im => im.Created).HasDefaultValueSql("GETDATE()");


            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ID = 1,
                    Sku = "Test",
                    Name = "Test",
                    Description = "Test",
                    CategoryID = 1,
                    QuantityInStock = 420,
                    ReorderLevel = 69,
                    Price = 123,
                    SupplierID = 1,
                    IsActive = true
                },
                new Product
                {
                    ID = 2,
                    Sku = "Test2",
                    Name = "Test2",
                    Description = "Test2",
                    CategoryID = 1,
                    QuantityInStock = 420,
                    ReorderLevel = 69,
                    Price = 123.85m,
                    SupplierID = 2,
                    IsActive = true
                },
                new Product
                {
                    ID = 3,
                    Sku = "Test3",
                    Name = "Test3",
                    Description = "Test3",
                    CategoryID = 2,
                    QuantityInStock = 420,
                    ReorderLevel = 69,
                    Price = 123.85m,
                    SupplierID = 2,
                    IsActive = true
                }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    ID = 1,
                    Name = "CoolStuff",
                    Description = "Things that are cool",
                    IsActive = true,
                    Created = DateOnly.FromDateTime(DateTime.Now),
                    Updated = DateOnly.FromDateTime(DateTime.Now)
                },
                new Category
                {
                    ID = 2,
                    Name = "CoolerStuff",
                    Description = "Things that are cooler",
                    IsActive = true,
                    Created = DateOnly.FromDateTime(DateTime.Now),
                    Updated = DateOnly.FromDateTime(DateTime.Now)
                },
                new Category
                {
                    ID = 3,
                    Name = "CoolestStuff",
                    Description = "Things that are coolest",
                    IsActive = true,
                    Created = DateOnly.FromDateTime(DateTime.Now),
                    Updated = DateOnly.FromDateTime(DateTime.Now)
                }
            );

            modelBuilder.Entity<Supplier>().HasData(
                new Supplier
                {
                    ID = 1,
                    Name = "TestSupplier",
                    ContactName = "TestSupplier",
                    PhoneContact = "TestSupplier",
                    EmailContact = "TestSupplier",
                    Address = "TestSupplier",
                    IsActive = true
                },
                new Supplier
                {
                    ID = 2,
                    Name = "TestSupplier",
                    ContactName = "TestSupplier",
                    PhoneContact = "TestSupplier",
                    EmailContact = "TestSupplier",
                    Address = "TestSupplier",
                    IsActive = true
                },
                new Supplier
                {
                    ID = 3,
                    Name = "TestSupplier",
                    ContactName = "TestSupplier",
                    PhoneContact = "TestSupplier",
                    EmailContact = "TestSupplier",
                    Address = "TestSupplier",
                    IsActive = true
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
