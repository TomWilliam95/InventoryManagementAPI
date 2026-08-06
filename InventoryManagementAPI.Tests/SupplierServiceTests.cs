using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.SupplierDTO_s;
using InventoryManagementAPI.Repositories.SupplierRepositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Tests
{
    // SupplierService is tested with a mocked ISupplierRepository. Each test
    // controls repository behaviour, calls the real service, checks its
    // ApiResponse, and verifies whether persistence was attempted.
    public class SupplierServiceTests
    {
        // === Get Supplier Tests === \\
        [Fact]
        public async Task GetSupplier_Success_Return200()
        {
            //Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.GetSupplierByIdAsync(It.IsAny<int>()))
                      .ReturnsAsync(CreateSupplier());
            //Act
            var service = new SupplierService(repository.Object);
            var result = await service.GetSupplierByIdAsync(1);
            //Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Supplier retrieved successfully.", result.Message);
            //Verify
            repository.Verify(repo => repo.GetSupplierByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetSupplier_NoSupplierFound_Return404()
        {
            //Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repository => repository.GetSupplierByIdAsync(It.IsAny<int>()))
                      .ReturnsAsync((Supplier?)null);
            //Act
            var service = new SupplierService(repository.Object);
            var result = await service.GetSupplierByIdAsync(1);
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Supplier not found", result.Message);
            //Verify
            repository.Verify(repo => repo.GetSupplierByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetSupplier_ThrowException_Return500()
        {
            //Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.GetSupplierByIdAsync(It.IsAny<int>()))
                      .ThrowsAsync(new Exception("Database connection failed."));
            //Act
            var service = new SupplierService(repository.Object);
            var result = await service.GetSupplierByIdAsync(1);
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to retrieve supplier.", result.Message);
            //Verify
            repository.Verify(repo => repo.GetSupplierByIdAsync(It.IsAny<int>()), Times.Once);
        }

        // === GET ALL SUPPLIERS TESTS === \\
        [Fact]
        public async Task GetAllSuppliers_Success_Returns200()
        {
            //Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.GetAllSuppliersAsync())
                      .ReturnsAsync(new List<Supplier> {
                          CreateSupplier(),
                          CreateSupplier(),
                          CreateSupplier()
                      });
            //Act
            var service = new SupplierService(repository.Object);
            var result = await service.GetAllSuppliersAsync();
            //Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Suppliers successfully retrieved", result.Message);
            //Verify
            repository.Verify(repo => repo.GetAllSuppliersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllSuppliers_NoSuppliersFound_Returns404()
        {
            //Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.GetAllSuppliersAsync())
                      .ReturnsAsync([]);
            //Act
            var service = new SupplierService(repository.Object);
            var result = await service.GetAllSuppliersAsync();
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No Suppliers Found", result.Message);
            //Verify
            repository.Verify(repo => repo.GetAllSuppliersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllSuppliers_ThrowException_Returns500()
        {
            //Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.GetAllSuppliersAsync())
                      .ThrowsAsync(new Exception("Database connection failed."));
            //Act
            var service = new SupplierService(repository.Object);
            var result = await service.GetAllSuppliersAsync();
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to load suppliers.", result.Message);
            //Verify
            repository.Verify(repo => repo.GetAllSuppliersAsync(), Times.Once);
        }


        // === Create Supplier Tests === \\
        [Fact]
        public async Task CreateSupplier_Success_Return201()
        {
            //Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.CreateSupplierAsync(It.IsAny<Supplier>()))
                      .ReturnsAsync(CreateSupplier());
            //Act
            var service = new SupplierService(repository.Object);
            var result = await service.CreateSupplierAsync(CreateNewSupplierDTO());
            //Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("Supplier successfully created", result.Message);
            //Verify
            repository.Verify(repo => repo.SupplierNameExistsAsync(It.IsAny<string>()), Times.Once);
            repository.Verify(repo => repo.SupplierEmailExistsAsync(It.IsAny<string>()), Times.Once);
            repository.Verify(repo => repo.CreateSupplierAsync(It.IsAny<Supplier>()), Times.Once);
        }

        
        [Fact]
        public async Task CreateSupplier_NullDto_Return400()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.CreateSupplierAsync(null!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid supplier object model", result.Message);

            // Verify
            repository.Verify(repo => repo.CreateSupplierAsync(It.IsAny<Supplier>()), Times.Never);
        }

        [Fact]
        public async Task CreateSupplier_InvalidName_Return400()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            var dto = CreateNewSupplierDTO();
            dto.Name = " ";
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.CreateSupplierAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Provide a supplier Name", result.Message);
            // Verify
            repository.Verify(repo => repo.CreateSupplierAsync(It.IsAny<Supplier>()), Times.Never);
        }

        [Fact]
        public async Task CreateSupplier_ThrowException_Return500()
        {
            //Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.CreateSupplierAsync(It.IsAny<Supplier>()))
                      .ThrowsAsync(new Exception("Database connection failed."));
            //Act
            var service = new SupplierService(repository.Object);
            var result = await service.CreateSupplierAsync(CreateNewSupplierDTO());
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to create supplier.", result.Message);
            //Verify
            repository.Verify(repo => repo.CreateSupplierAsync(It.IsAny<Supplier>()), Times.Once);
        }

        [Fact]
        public async Task CreateSupplier_DuplicateName_Return400()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.SupplierNameExistsAsync("New Supplier")).ReturnsAsync(true);
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.CreateSupplierAsync(CreateNewSupplierDTO());

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Supplier Name already exists", result.Message);
            repository.Verify(repo => repo.CreateSupplierAsync(It.IsAny<Supplier>()), Times.Never);
        }

        [Fact]
        public async Task CreateSupplier_DuplicateEmail_Return400()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.SupplierEmailExistsAsync("test@example.com")).ReturnsAsync(true);
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.CreateSupplierAsync(CreateNewSupplierDTO());

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Supplier Email already exists", result.Message);
            repository.Verify(repo => repo.CreateSupplierAsync(It.IsAny<Supplier>()), Times.Never);
        }

        // === Update Supplier Tests === \\
        [Fact]
        public async Task UpdateSupplier_ValidRequest_Returns200()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.GetSupplierByIdAsync(1)).ReturnsAsync(CreateSupplier());
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.UpdateSupplierAsync(1, CreateUpdateSupplierDTO());

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Supplier details successfully updated", result.Message);
            Assert.Equal("Updated Supplier", result.Data!.Name);

            // It.Is<Supplier> inspects the actual entity sent to the repository.
            // The verification proves both the ID and updated name were mapped.
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateSupplier_NullRequest_Returns400()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.UpdateSupplierAsync(1, null!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid supplier object model", result.Message);

            // Verify
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateSupplier_SupplierNotFound_Returns404()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.GetSupplierByIdAsync(99)).ReturnsAsync((Supplier?)null);
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.UpdateSupplierAsync(99, CreateUpdateSupplierDTO());

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Supplier not found", result.Message);
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateSupplier_DuplicateName_Returns400()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.GetSupplierByIdAsync(1)).ReturnsAsync(CreateSupplier());
            repository.Setup(repo => repo.SupplierNameExistsForOtherSupplierAsync(1, "Updated Supplier"))
                .ReturnsAsync(true);
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.UpdateSupplierAsync(1, CreateUpdateSupplierDTO());

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Supplier Name already exists", result.Message);
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateSupplier_DuplicateEmail_Returns400()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.GetSupplierByIdAsync(1)).ReturnsAsync(CreateSupplier());
            repository.Setup(repo => repo.SupplierEmailExistsForOtherSupplierAsync(1, "updated@example.com"))
                .ReturnsAsync(true);
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.UpdateSupplierAsync(1, CreateUpdateSupplierDTO());

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Supplier Email already exists", result.Message);
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateSupplier_RepositoryThrows_Returns500()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.GetSupplierByIdAsync(1)).ReturnsAsync(CreateSupplier());
            repository.Setup(repo => repo.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database error"));
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.UpdateSupplierAsync(1, CreateUpdateSupplierDTO());

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to update supplier.", result.Message);

            // Verify
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        // === Supplier Status Tests === \\
        [Fact]
        public async Task ActivateSupplier_InactiveSupplier_Returns200()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            var supplier = CreateSupplier();
            supplier.IsActive = false;
            repository.Setup(repo => repo.GetSupplierByIdAsync(1)).ReturnsAsync(supplier);
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.ActivateSupplierAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data!.IsActive);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Supplier successfully activated", result.Message);

            // Verify
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ActivateSupplier_MissingSupplier_Returns404()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.GetSupplierByIdAsync(99)).ReturnsAsync((Supplier?)null);
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.ActivateSupplierAsync(99);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Supplier not found", result.Message);

            // Verify
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ActivateSupplier_AlreadyActive_Returns400()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.GetSupplierByIdAsync(1)).ReturnsAsync(CreateSupplier());
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.ActivateSupplierAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Supplier is already active", result.Message);

            // Verify
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeactivateSupplier_ActiveSupplier_Returns200()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.GetSupplierByIdAsync(1)).ReturnsAsync(CreateSupplier());
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.DeactivateSupplierAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data!.IsActive);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Supplier successfully deactivated", result.Message);

            // Verify
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeactivateSupplier_MissingSupplier_Returns404()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            repository.Setup(repo => repo.GetSupplierByIdAsync(99)).ReturnsAsync((Supplier?)null);
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.DeactivateSupplierAsync(99);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Supplier not found", result.Message);

            // Verify
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeactivateSupplier_AlreadyInactive_Returns400()
        {
            // Arrange
            var repository = new Mock<ISupplierRepository>();
            var supplier = CreateSupplier();
            supplier.IsActive = false;
            repository.Setup(repo => repo.GetSupplierByIdAsync(1)).ReturnsAsync(supplier);
            var service = new SupplierService(repository.Object);

            // Act
            var result = await service.DeactivateSupplierAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Supplier is already inactive", result.Message);

            // Verify
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        // === Helper Methods === \\
        private Supplier CreateSupplier()
        {
            // Valid existing supplier used as a baseline by retrieval, update,
            // activation, and deactivation tests.
            return new Supplier
            {
                ID = 1,
                Name = "Test Supplier",
                ContactName = "Test Contact",
                PhoneContact = "1234567890",
                EmailContact = "test@example.com",
                Address = "123 Test St",
                IsActive = true
            };
        }

        private CreateSupplierRequestDTO CreateNewSupplierDTO()
        {
            // Valid creation request. Validation tests modify one field at a time.
            return new CreateSupplierRequestDTO
            {
                Name = "New Supplier",
                ContactName = "New Contact",
                PhoneContact = "0987654321",
                EmailContact = "test@example.com",
                Address = "456 New St",
            };
        }

        private UpdateSupplierRequestDTO CreateUpdateSupplierDTO()
        {
            // Valid update request shared by tests that simulate different
            // repository outcomes such as missing IDs or duplicate values.
            return new UpdateSupplierRequestDTO
            {
                Name = "Updated Supplier",
                ContactName = "Updated Contact",
                PhoneContact = "0400000000",
                EmailContact = "updated@example.com",
                Address = "789 Updated St",
                IsActive = true
            };
        }
    }
}
            // Verify
            // Verify
            // Verify
            // Verify
            // Verify
            // Verify
