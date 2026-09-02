
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.CoreModels.RolePermissions;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Services
{
    public class InvManDBContext: DbContext
    {
        public InvManDBContext(DbContextOptions options) : base(options)
        {

        }

        // Catalog
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        // Identity
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        //Inventory
        public DbSet<InventoryMovement> InventoryMovements { get; set; }
        public DbSet<InventoryStock> InventoryStocks { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }

        //Orders
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }

        //Suppliers
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierAddress> SupplierAddresses { get; set; }
        public DbSet<SupplierContact> SupplierContacts { get; set; }
        public DbSet<SupplierProduct> SupplierProducts { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite keys for explicit many-to-many join entities.
            modelBuilder.Entity<UserRole>()
                .HasKey(userRole => new { userRole.UserID, userRole.RoleID });
            modelBuilder.Entity<RolePermission>()
                .HasKey(rolePermission => new { rolePermission.RoleID, rolePermission.PermissionID });
            modelBuilder.Entity<SupplierProduct>()
                .HasKey(supplierProduct => new { supplierProduct.SupplierID, supplierProduct.ProductID });

            // Explicit relationships and conservative delete behaviour protect historical records.
            modelBuilder.Entity<Product>() // Configure the relationship between Product and Category with a foreign key constraint
                .HasOne(product => product.Category) // Specify that Product has one Category
                .WithMany(category => category.Products)// Specify that Category can have many Products
                .HasForeignKey(product => product.CategoryID) // Specify the foreign key property in Product that references Category
                .OnDelete(DeleteBehavior.Restrict); // Specify the delete behavior when a Category is deleted (Restrict means you cannot delete a Category if it has related Products)
            modelBuilder.Entity<InventoryStock>()
                .HasOne(stock => stock.Product)
                .WithMany(product => product.InventoryStocks)
                .HasForeignKey(stock => stock.ProductID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<InventoryStock>()
                .HasOne(stock => stock.Warehouse)
                .WithMany(warehouse => warehouse.InventoryStocks)
                .HasForeignKey(stock => stock.WarehouseID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<InventoryMovement>()
                .HasOne(movement => movement.InventoryStock)
                .WithMany(stock => stock.InventoryMovements)
                .HasForeignKey(movement => movement.InventoryStockID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<InventoryMovement>()
                .HasOne(movement => movement.User)
                .WithMany(user => user.InventoryMovements)
                .HasForeignKey(movement => movement.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupplierProduct>()
                .HasOne(supplierProduct => supplierProduct.Supplier)
                .WithMany(supplier => supplier.SupplierProducts)
                .HasForeignKey(supplierProduct => supplierProduct.SupplierID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupplierProduct>()
                .HasOne(supplierProduct => supplierProduct.Product)
                .WithMany(product => product.SupplierProducts)
                .HasForeignKey(supplierProduct => supplierProduct.ProductID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupplierAddress>()
                .HasOne(address => address.Supplier)
                .WithMany(supplier => supplier.SupplierAddresses)
                .HasForeignKey(address => address.SupplierID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupplierContact>()
                .HasOne(contact => contact.Supplier)
                .WithMany(supplier => supplier.SupplierContacts)
                .HasForeignKey(contact => contact.SupplierID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupplierContact>()
                .HasOne(contact => contact.SupplierAddress)
                .WithMany(address => address.SupplierContacts)
                .HasForeignKey(contact => contact.SupplierAddressID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserRole>()
                .HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.UserID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserRole>()
                .HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RolePermission>()
                .HasOne(rolePermission => rolePermission.Role)
                .WithMany(role => role.RolePermissions)
                .HasForeignKey(rolePermission => rolePermission.RoleID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RolePermission>()
                .HasOne(rolePermission => rolePermission.Permission)
                .WithMany(permission => permission.RolePermissions)
                .HasForeignKey(rolePermission => rolePermission.PermissionID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Customer>()
                .HasOne(customer => customer.User)
                .WithOne()
                .HasForeignKey<Customer>(customer => customer.UserID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(order => order.Supplier)
                .WithMany()
                .HasForeignKey(order => order.SupplierID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(order => order.Warehouse)
                .WithMany()
                .HasForeignKey(order => order.WarehouseID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(order => order.CreatedByUser)
                .WithMany()
                .HasForeignKey(order => order.CreatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(item => item.PurchaseOrder)
                .WithMany(order => order.PurchaseOrderItems)
                .HasForeignKey(item => item.PurchaseOrderID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesOrder>()
                .HasOne(order => order.Customer)
                .WithMany(customer => customer.SalesOrders)
                .HasForeignKey(order => order.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SalesOrder>()
                .HasOne(order => order.Warehouse)
                .WithMany()
                .HasForeignKey(order => order.WarehouseID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SalesOrder>()
                .HasOne(order => order.CreatedByUser)
                .WithMany()
                .HasForeignKey(order => order.CreatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SalesOrderItem>()
                .HasOne(item => item.SalesOrder)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.SalesOrderID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SalesOrderItem>()
                .HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            // Database constraints enforce invariants even when writes bypass API validation.
            // Use check constraints to enforce positive and non-negative values for relevant fields.
            // CK = Check Constraint, the naming convention is CK_<TableName>_<ColumnName>_<ConstraintDescription>
            // [] Syntax is used to escape column names in SQL Server, especially if they contain spaces or special characters.

            modelBuilder.Entity<Product>().ToTable(table =>
            // Ensure that the Price column in the Products table is always greater than 0
                table.HasCheckConstraint("CK_Products_Price_Positive", "[Price] > 0")); 
            
            modelBuilder.Entity<InventoryStock>().ToTable(table =>
            { // Ensure that the Quantity and ReorderLevel columns in the InventoryStock table are always greater than or equal to 0
                table.HasCheckConstraint("CK_InventoryStock_Quantity_NonNegative", "[Quantity] >= 0"); 
                table.HasCheckConstraint("CK_InventoryStock_ReorderLevel_NonNegative", "[ReorderLevel] >= 0");
            });
            
            modelBuilder.Entity<InventoryMovement>().ToTable(table =>
            {// Ensure that the Quantity column in the InventoryMovements table is always greater than 0,
             // and that the QuantityBefore and QuantityAfter columns are always greater than or equal to 0
                table.HasCheckConstraint("CK_InventoryMovements_Quantity_Positive", "[Quantity] > 0");
                table.HasCheckConstraint("CK_InventoryMovements_QuantityBefore_NonNegative", "[QuantityBefore] >= 0");
                table.HasCheckConstraint("CK_InventoryMovements_QuantityAfter_NonNegative", "[QuantityAfter] >= 0");
            });

            modelBuilder.Entity<SupplierProduct>().ToTable(table =>
            {// Ensure that the UnitCost and LeadTimeDays columns in the SupplierProducts table are always greater than or equal to 0,
                table.HasCheckConstraint("CK_SupplierProducts_UnitCost_NonNegative", "[UnitCost] >= 0");
                table.HasCheckConstraint("CK_SupplierProducts_LeadTimeDays_NonNegative", "[LeadTimeDays] >= 0");
                table.HasCheckConstraint("CK_SupplierProducts_MinimumOrderQuantity_Positive", "[MinimumOrderQuantity] > 0");
            });

            modelBuilder.Entity<PurchaseOrderItem>().ToTable(table =>
            {// Ensure that the QuantityOrdered, QuantityReceived, and UnitCost columns in the PurchaseOrderItems table are always greater than or equal to 0,
                table.HasCheckConstraint("CK_PurchaseOrderItems_QuantityOrdered_Positive", "[QuantityOrdered] > 0");
                table.HasCheckConstraint("CK_PurchaseOrderItems_QuantityReceived_Valid", "[QuantityReceived] >= 0 AND [QuantityReceived] <= [QuantityOrdered]");
                table.HasCheckConstraint("CK_PurchaseOrderItems_UnitCost_NonNegative", "[UnitCost] >= 0");
            });
            
            modelBuilder.Entity<SalesOrderItem>().ToTable(table =>
            {// Ensure that the QuantityOrdered, QuantityDispatched, UnitPrice, and DiscountAmount columns in the SalesOrderItems table are always greater than or equal to 0,
                table.HasCheckConstraint("CK_SalesOrderItems_QuantityOrdered_Positive", "[QuantityOrdered] > 0");
                table.HasCheckConstraint("CK_SalesOrderItems_QuantityDispatched_Valid", "[QuantityDispatched] >= 0 AND [QuantityDispatched] <= [QuantityOrdered]");
                table.HasCheckConstraint("CK_SalesOrderItems_UnitPrice_NonNegative", "[UnitPrice] >= 0");
                table.HasCheckConstraint("CK_SalesOrderItems_DiscountAmount_NonNegative", "[DiscountAmount] >= 0");
            });

            // Set the default collation for the database to be case-insensitive and accent-sensitive
            // This ensures that string comparisons and sorting are done in a case-insensitive manner.
            modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

            // Unique natural identifiers.
            // Creates unique indexes on the specified columns to enforce uniqueness at the database level.
            modelBuilder.Entity<Category>()
                .HasIndex(category => category.Name)
                .IsUnique();
            modelBuilder.Entity<Product>()
                .HasIndex(product => product.Sku)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();
            modelBuilder.Entity<Role>()
                .HasIndex(role => role.Name)
                .IsUnique();
            modelBuilder.Entity<Permission>()
                .HasIndex(permission => permission.Name)
                .IsUnique();
            modelBuilder.Entity<Warehouse>()
                .HasIndex(warehouse => warehouse.Name)
                .IsUnique();
            modelBuilder.Entity<Supplier>()
                .HasIndex(supplier => supplier.Name)
                .IsUnique();
            modelBuilder.Entity<Supplier>()
                .HasIndex(supplier => supplier.TaxNumber)
                .IsUnique();

            // Prevent duplicate relationship and order-line records.
            // Creates unique indexes on the specified columns to enforce uniqueness at the database level for relationships and order-line records.
            // Uses composite indexes to ensure that the combination of certain columns is unique, preventing duplicate entries in the database.
            modelBuilder.Entity<InventoryStock>()
                .HasIndex(stock => new { stock.ProductID, stock.WarehouseID })
                .IsUnique();
            modelBuilder.Entity<SupplierProduct>()
                .HasIndex(supplierProduct => new { supplierProduct.SupplierID, supplierProduct.SupplierSku })
                .IsUnique()
                .HasFilter("[SupplierSku] IS NOT NULL");

            // Enforce single-primary/preferred business rules atomically. The
            // filtered indexes only include rows where the flag is true, so
            // any number of non-primary/non-preferred rows remain valid.
            modelBuilder.Entity<SupplierContact>()
                .HasIndex(contact => contact.SupplierID)
                .IsUnique()
                .HasFilter("[IsPrimary] = 1");
            modelBuilder.Entity<SupplierAddress>()
                .HasIndex(address => new { address.SupplierID, address.Type })
                .IsUnique()
                .HasFilter("[IsPrimary] = 1");
            modelBuilder.Entity<SupplierProduct>()
                .HasIndex(supplierProduct => supplierProduct.ProductID)
                .IsUnique()
                .HasFilter("[IsPreferred] = 1");
            modelBuilder.Entity<PurchaseOrderItem>()
                .HasIndex(item => new { item.PurchaseOrderID, item.ProductID })
                .IsUnique();
            modelBuilder.Entity<SalesOrderItem>()
                .HasIndex(item => new { item.SalesOrderID, item.ProductID })
                .IsUnique();
            modelBuilder.Entity<PurchaseOrder>()
                .HasIndex(order => new { order.SupplierID, order.SupplierReference })
                .IsUnique();
            modelBuilder.Entity<SalesOrder>()
                .HasIndex(order => new { order.CustomerID, order.CustomerReference })
                .IsUnique();

            // Configure enum to string conversion
            // Specifies that the enum properties should be stored as strings in the database instead of their underlying integer values.
            modelBuilder.Entity<SupplierAddress>().Property(sa => sa.Type).HasConversion<string>();
            modelBuilder.Entity<InventoryMovement>().Property(im => im.Movement).HasConversion<string>();
            modelBuilder.Entity<PurchaseOrder>().Property(po => po.Status).HasConversion<string>();
            modelBuilder.Entity<SalesOrder>().Property(so => so.Status).HasConversion<string>();

            // Configure fixed precision for persisted monetary values.
            // This ensures that decimal values are stored with a fixed precision and scale in the database
            // Important for financial calculations to avoid rounding errors.
            modelBuilder.Entity<Product>()
                .Property(product => product.Price)
                .HasPrecision(18, 2);
            modelBuilder.Entity<SupplierProduct>()
                .Property(supplierProduct => supplierProduct.UnitCost)
                .HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(item => item.UnitCost)
                .HasPrecision(18, 2);
            modelBuilder.Entity<SalesOrderItem>()
                .Property(item => item.UnitPrice)
                .HasPrecision(18, 2);
            modelBuilder.Entity<SalesOrderItem>()
                .Property(item => item.DiscountAmount)
                .HasPrecision(18, 2);

            // Let SQL Server populate audit timestamps when values are omitted.
            // This ensures that the Created and Updated timestamps are automatically set to the current date and time when a new record is inserted into the database.
            // The GETDATE() function is used to get the current date and time from the SQL Server.
            modelBuilder.Entity<Category>().Property(category => category.Created).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Category>().Property(category => category.Updated).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Product>().Property(product => product.Created).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Product>().Property(product => product.Updated).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<User>().Property(user => user.Created).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<User>().Property(user => user.Updated).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<User>().Property(user => user.LastLogin).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<InventoryMovement>().Property(movement => movement.Created).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<InventoryStock>().Property(stock => stock.Created).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<InventoryStock>().Property(stock => stock.Updated).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Warehouse>().Property(warehouse => warehouse.Created).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Warehouse>().Property(warehouse => warehouse.Updated).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Supplier>().Property(supplier => supplier.Created).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Supplier>().Property(supplier => supplier.Updated).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<SupplierAddress>().Property(address => address.Created).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<SupplierAddress>().Property(address => address.Updated).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<SupplierContact>().Property(contact => contact.Created).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<SupplierContact>().Property(contact => contact.Updated).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<SupplierProduct>().Property(item => item.Created).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<SupplierProduct>().Property(item => item.Updated).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<PurchaseOrder>().Property(order => order.Created).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<PurchaseOrder>().Property(order => order.Updated).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<SalesOrder>().Property(order => order.Created).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<SalesOrder>().Property(order => order.Updated).HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Role>().HasData(
                new Role { 
                    ID = 1, 
                    Name = "Admin", 
                    Description = "Administrator role with full permissions", 
                    IsActive = true, 
                    Created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), 
                    Updated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), },
                
                new Role { 
                    ID = 2, 
                    Name = "Manager", 
                    Description = "Manager role with stock management permissions and limited administrative capabilities", 
                    IsActive = true, Created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), 
                    Updated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), }, 

                new Role { 
                    ID = 3, Name = "Staff", 
                    Description = "Standard staff role with limited permissions", 
                    IsActive = true, 
                    Created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), 
                    Updated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), });

            base.OnModelCreating(modelBuilder);
        }
    }
}
