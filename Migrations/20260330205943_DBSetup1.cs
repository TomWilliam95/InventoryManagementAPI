using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventoryManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class DBSetup1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "ID", "Description", "Name" },
                values: new object[,]
                {
                    { 2, "Things that are cooler", "CoolerStuff" },
                    { 3, "Things that are coolest", "CoolestStuff" }
                });

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "ID", "Address", "ContactName", "EmailContact", "IsActive", "Name", "PhoneContact" },
                values: new object[,]
                {
                    { 2, "TestSupplier", "TestSupplier", "TestSupplier", true, "TestSupplier", "TestSupplier" },
                    { 3, "TestSupplier", "TestSupplier", "TestSupplier", true, "TestSupplier", "TestSupplier" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ID", "CategoryID", "Description", "IsActive", "Name", "Price", "QuantityInStock", "ReorderLevel", "Sku", "SupplierID" },
                values: new object[,]
                {
                    { 2, 1, "Test2", true, "Test2", 123.85m, 420, 69, "Test2", 2 },
                    { 3, 2, "Test3", true, "Test3", 123.85m, 420, 69, "Test3", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "ID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "ID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "ID",
                keyValue: 2);
        }
    }
}
