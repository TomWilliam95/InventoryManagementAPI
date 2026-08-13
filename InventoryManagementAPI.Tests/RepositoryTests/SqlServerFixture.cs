using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;

namespace InventoryManagementAPI.Tests.RepositoryTests
{
    public sealed class SqlServerFixture : IAsyncLifetime
    {
        // The MsSqlContainer is a disposable resource that manages the lifecycle of a SQL Server container for testing purposes.
        // The MsSqlBuilder is used to configure the container, specifying the image to use (in this case, SQL Server 2022 on Ubuntu 20.04).
        private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-20.04").Build();

        // The CreateContext method creates a new instance of the InvManDBContext, which is the Entity Framework Core database context for the application.
        public InvManDBContext CreateContext()
        {
            // The DbContextOptionsBuilder is used to configure the context to use SQL Server with the connection string provided by the container.
            // The connection string is obtained from the _container instance, which provides the necessary details to connect to the SQL Server instance running in the container.
            var options = new DbContextOptionsBuilder<InvManDBContext>()
                .UseSqlServer(_container.GetConnectionString())
                .Options;

            // The method returns a new instance of InvManDBContext configured to connect to the SQL Server container.
            return new InvManDBContext(options);
        }

        // The InitializeAsync method is part of the IAsyncLifetime interface, which is used to perform asynchronous initialization tasks before tests are run.
        public async Task InitializeAsync()
        {
            // The StartAsync method is called on the _container instance to start the SQL Server container.
            // This ensures that the database server is running and ready to accept connections.
            await _container.StartAsync();

            // The CreateContext method is called to create a new instance of InvManDBContext, which is used to interact with the database.
            await using var context = CreateContext();

            // The reshaped model does not have a replacement migration yet, so create
            // the test schema directly from the current EF model. Switch this back to
            // MigrateAsync once a new baseline migration has been generated.
            await context.Database.EnsureCreatedAsync();

            // Migrations also insert the application's demonstration seed data. Repository tests
            // arrange their own records, so remove that data to keep every assertion independent
            // from IDs and values defined in InvManDBContext.OnModelCreating.
            await context.InventoryMovements.ExecuteDeleteAsync();
            await context.Products.ExecuteDeleteAsync();
            await context.Users.ExecuteDeleteAsync();
            await context.Suppliers.ExecuteDeleteAsync();
            await context.Categories.ExecuteDeleteAsync();
        }

        // The DisposeAsync method is part of the IAsyncLifetime interface, which is used to perform asynchronous cleanup tasks after tests are run.
        public Task DisposeAsync()
        {
            // The DisposeAsync method is called on the _container instance to stop and dispose of the SQL Server container.
            // This ensures that any resources used by the container are released and that the container is properly cleaned up after tests are completed.
            return _container.DisposeAsync().AsTask();
        }
    }
}
