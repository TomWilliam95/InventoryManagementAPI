using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.CategoryDTO_s;
using InventoryManagementAPI.Repositories.CategoryRepositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Tests
{
    public class CategoryServiceTests
    {
        // === GetSingleCategory Tests === \\
        [Fact]
        public async Task GetSingleCategory_WhenCategoryExists_Returns200()
        {
            // === Setup === \\

            //Create a mock of the ICategoryRepository interface
            var repository = new Mock<ICategoryRepository>();

            //Setup the mock to return a specific category when GetCategoryByIdAsync is called with a valid category ID
            repository
                .Setup(repo => repo.GetCategoryByIdAsync(1))
                .ReturnsAsync(new Category { ID = 1, Name = "Electronics" });


            //=== Act === \\

            //Pass the mocked repository to the CategoryService
            var service = new CategoryService(repository.Object);

            //Call the GetSingleCategory method with a valid category ID
            var result = await service.GetSingleCategory(1);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Category retrieved successfully.", result.Message);
        }

        [Fact]
        public async Task GetSingleCategory_WhenCategoryDoesntExist_Returns404()
        {
            // === Setup === \\

            // Creates a mock of the ICategoryRepository interface
            // Using moq to simulate the behavior of the repository without needing a real database
            var repository = new Mock<ICategoryRepository>();

            // Sets up the mock to return null when GetCategoryByIdAsync is called with a non-existent category ID
            // This simulates the scenario where the category does not exist in the database
            repository
                .Setup(repo => repo.GetCategoryByIdAsync(999))
                .ReturnsAsync((Category?)null);

            // === Act === \\

            // Creates an instance of the CategoryService, passing in the mocked repository
            var service = new CategoryService(repository.Object);

            // Calls the GetSingleCategory method with a non-existent category ID
            var result = await service.GetSingleCategory(999);

            // === Assert === \\,

            // Asserts that the result indicates failure, has a 404 status code, and contains the expected error message
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Category not found.", result.Message);
        }

        [Fact]
        public async Task GetSingleCategory_WhenCategoryRepositoryThrowsException_Returns500()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            repository
                .Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database error"));

            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.GetSingleCategory(1);

            // === Assert === \\
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to load category.", result.Message);
        }


        // === GetAllCategories Tests === \\
        [Fact]
        public async Task GetAllCategories_WhenCategoriesExist_Returns200()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            repository
                .Setup(repo => repo.GetAllCategoriesAsync())
                .ReturnsAsync(new List<Category>
                {
                    new Category { ID = 1, Name = "Electronics" },
                    new Category { ID = 2, Name = "Books" }
                });
            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.GetAllCategories();

            // === Assert === \\
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Categories retrieved successfully.", result.Message);
        }

        [Fact]
        public async Task GetAllCategories_WhenNoCategoriesExist_Returns404()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            repository
                .Setup(repo => repo.GetAllCategoriesAsync())
                .ReturnsAsync(new List<Category>());

            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.GetAllCategories();

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No categories found.", result.Message);
        }

        [Fact]
        public async Task GetAllCategories_WhenCategoryRepositoryThrowsException_Returns500()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            repository
                .Setup(repo => repo.GetAllCategoriesAsync())
                .ThrowsAsync(new Exception("Database error"));
            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.GetAllCategories();
            // === Assert === \\
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to load categories.", result.Message);
        }


        // === AddCategory Tests === \\
        [Fact]
        public async Task AddCategory_WhenCategoryIsValid_Returns201()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();

            // Create a DTO for the new category to be added
            var newCategoryDto = new CreateCategoryRequestDTO
            {
                Name = "Toys",
                Description = "Fun Stuff"
            };

            // Setup the mock to return a new category when CreateCategoryAsync is called with a valid category
            repository
                .Setup(repo => repo.CreateCategoryAsync(It.Is<Category>(c => c.Name == "Toys" && c.Description == "Fun Stuff")))
                .ReturnsAsync(new Category { ID = 3, Name = "Toys", Description = "Fun Stuff" });

            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.AddCategory(newCategoryDto);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("Category added successfully.", result.Message);

            // Verify that the repository's CreateCategoryAsync method was called exactly once with the expected category
            repository.Verify(
                repo => repo.CreateCategoryAsync(
                    It.Is<Category>(c =>
                    c.Name == "Toys" &&
                    c.Description == "Fun Stuff")),
                Times.Once);
        }

        [Fact]
        public async Task AddCategory_WhenMissingRequestBody_Returns400()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            var service = new CategoryService(repository.Object);

            // === Act === \\
            var result = await service.AddCategory(null!);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Request body is required.", result.Message);

            // Verify that the repository's CreateCategoryAsync method was never called since the request body was null
            repository.Verify(repo => repo.CreateCategoryAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task AddCategory_WhenNameIsMissing_Returns400()
        {
            var repository = new Mock<ICategoryRepository>();
            var newCategoryDto = new CreateCategoryRequestDTO
            {
                Name = "", // Missing name
                Description = "Fun Stuff"
            };

            var service = new CategoryService(repository.Object);
            var result = await service.AddCategory(newCategoryDto);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Category name is required.", result.Message);

            repository.Verify(repo => repo.CreateCategoryAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task AddCategory_WhenNameAlreadyExists_Returns400()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            var newCategoryDto = new CreateCategoryRequestDTO
            {
                Name = "Toys", // Name that already exists
                Description = "Fun Stuff"
            };

            // Setup the mock to return true when CategoryNameExistsASync is called with the existing category name
            repository
                .Setup(repo => repo.CategoryNameExistsASync("Toys"))
                .ReturnsAsync(true);


            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.AddCategory(newCategoryDto);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Category with the same name already exists.", result.Message);

            // Verify that the repository's CategoryNameExistsASync method was called exactly once with the expected category name
            repository.Verify(repo => repo.CategoryNameExistsASync("Toys"), Times.Once);

            // Verify that the repository's CreateCategoryAsync method was never called since the category name already exists
            repository.Verify(repo => repo.CreateCategoryAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task AddCategory_RepositoryThrowsException_Returns500()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            var newCategoryDto = new CreateCategoryRequestDTO
            {
                Name = "Toys",
                Description = "Fun Stuff"
            };
            // Setup the mock to throw an exception when CreateCategoryAsync is called
            repository
                .Setup(repo => repo.CreateCategoryAsync(It.IsAny<Category>()))
                .ThrowsAsync(new Exception("Database error"));

            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.AddCategory(newCategoryDto);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to add category.", result.Message);
        }


        // === UpdateCategoryDetails Tests === \\
        [Fact]
        public async Task UpdateCategory_UpdateExistingCategory_Return200()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            var updateCategoryDto = new UpdateCategoryDetailsRequestDTO
            {
                Name = "Updated Category",
                Description = "Updated Description"
            };

            // Setup the mock to return false when OtherCategoryNameExistsAsync is called with the same category name, indicating no other category has the same name
            repository.Setup(repo => repo.OtherCategoryNameExistsAsync(It.IsAny<int>(), updateCategoryDto.Name))
                .ReturnsAsync(false);

            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Category { ID = It.IsAny<int>(), Name = It.IsAny<string>(), IsActive = true });

            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.UpdateCategoryDetails(999, updateCategoryDto);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Category updated successfully.", result.Message);

            // Verify that the repository's OtherCategoryNameExistsAsync method was called exactly once with the expected category ID and name
            repository.Verify(repo => repo.OtherCategoryNameExistsAsync(It.IsAny<int>(), "Updated Category"), Times.Once);

            // Verify that the repository's UpdateCategoryAsync method was called exactly once with the expected category
            repository.Verify(repo => repo.UpdateCategoryAsync(It.Is<Category>(c => c.Name == "Updated Category" && c.Description == "Updated Description")), Times.Once);
        }

        [Fact]
        public async Task UpdateCategory_UpdateNonExistingCategory_Return404()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            var updateCategoryDto = new UpdateCategoryDetailsRequestDTO
            {
                Name = "Updated Category",
                Description = "Updated Description"
            };

            // Setup the mock to return false when CategoryExistsAsync is called with a non-existing category ID
            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Category?)null);

            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.UpdateCategoryDetails(It.IsAny<int>(), updateCategoryDto);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Category not found.", result.Message);

            // Verify that the repository's UpdateCategoryAsync method was never called since the category does not exist
            repository.Verify(repo => repo.UpdateCategoryAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCategory_UpdateRequestBodyNull_Return400()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            var service = new CategoryService(repository.Object);

            // === Act === \\
            var result = await service.UpdateCategoryDetails(It.IsAny<int>(), null!);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Request body is required.", result.Message);

            repository.Verify(repo => repo.UpdateCategoryAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCategory_UpdateCategoryBlankName_Return400()
        {
            var repository = new Mock<ICategoryRepository>();

            var updateCategoryDto = new UpdateCategoryDetailsRequestDTO
            {
                Name = "", // Blank name
                Description = "Updated Description"
            };

            var service = new CategoryService(repository.Object);

            var result = await service.UpdateCategoryDetails(It.IsAny<int>(), updateCategoryDto);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Category name is required.", result.Message);

            repository.Verify(repo => repo.UpdateCategoryAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCategory_UpdateCategoryNameAlreadyExists_Return400()
        {
            var repository = new Mock<ICategoryRepository>();
            var updateCategoryDto = new UpdateCategoryDetailsRequestDTO
            {
                Name = "Existing Category", // Name that already exists
                Description = "Updated Description"
            };

            // Setup the mock to return true when OtherCategoryNameExistsAsync is called with the existing category name
            repository.Setup(repo => repo.OtherCategoryNameExistsAsync(It.IsAny<int>(), "Existing Category"))
                .ReturnsAsync(true);

            var service = new CategoryService(repository.Object);
            var result = await service.UpdateCategoryDetails(It.IsAny<int>(), updateCategoryDto);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Category with the same name already exists.", result.Message);

            repository.Verify(repo => repo.OtherCategoryNameExistsAsync(It.IsAny<int>(), "Existing Category"), Times.Once);
            repository.Verify(repo => repo.UpdateCategoryAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCategory_RepositoryThrowException_Returns500()
        {
            var repository = new Mock<ICategoryRepository>();
            var updateCategoryDto = new UpdateCategoryDetailsRequestDTO
            {
                Name = "Updated Category",
                Description = "Updated Description"
            };

            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Category { ID = It.IsAny<int>(), Name = It.IsAny<string>(), IsActive = true });

            // Setup the mock to throw an exception when UpdateCategoryAsync is called
            repository.Setup(repo => repo.UpdateCategoryAsync(It.IsAny<Category>()))
                .ThrowsAsync(new Exception("Database error"));

            var service = new CategoryService(repository.Object);

            var result = await service.UpdateCategoryDetails(It.IsAny<int>(), updateCategoryDto);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to update category.", result.Message);
        }


        // === ActivateCategory Tests === \\
        [Fact]
        public async Task ActivateCategory_ActivateExistingCategory_Return200()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            // Setup the mock to return true when CategoryExistsAsync is called with the existing category ID
            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Category { ID = It.IsAny<int>(), Name = It.IsAny<string>(), IsActive = false });

            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.ActivateCategory(1);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Category activated successfully.", result.Message);


            // Verify that the repository's SaveChangesAsync method was called exactly once
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ActivateCategory_CategoryDoesntExist_Return404()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            //Setup the mock to return false when CategoryExistsAsync is called with a non-existing category ID
            repository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.ActivateCategory(999);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Category not found.", result.Message);

            //Verify that SaveChangesAsync was never called since the category does not exist
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ActivateCategory_CategoryAlreadyActive_Return400()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            // Setup the mock to return true when CategoryExistsAsync is called with the existing category ID
            repository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Category { ID = 1, Name = "Electronics", IsActive = true }); // Simulate that the category is already active  

            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.ActivateCategory(1);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Category is already active.", result.Message);

            // Verify that SaveChangesAsync was never called since the category is already active
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ActivateCategory_RepositoryThrowsException_Return500()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();

            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Category { ID = It.IsAny<int>(), Name = It.IsAny<string>(), IsActive = false });

            // Setup the mock to throw an exception when SaveChangesAsync is called
            repository.Setup(repo => repo.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database error"));

            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.ActivateCategory(1);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to activate category.", result.Message);

            // Verify that SaveChangesAsync was never called since an exception occurred
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        // === DeactivateCategory Tests === \\
        [Fact]
        public async Task DeactivateCategory_DeactivateExistingCategory_Return200()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();

            // Setup the mock to return true when CategoryExistsAsync is called with the existing category ID
            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Category { ID = It.IsAny<int>(), Name = It.IsAny<string>(), IsActive = true });

            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.DeactivateCategory(It.IsAny<int>());

            // === Assert === \\
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Category deactivated successfully.", result.Message);

            // Verify that the repository's SaveChangesAsync method was called exactly once
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }
        
        [Fact]
        public async Task DeactivateCategory_CategoryDoesntExist_Return404()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            // Setup the mock to return false when CategoryExistsAsync is called with a non-existing category ID
            repository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.DeactivateCategory(It.IsAny<int>());

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Category not found.", result.Message);

            // Verify that the repository's SaveChangesAsync method was never called since the category does not exist
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeactivateCategory_CategoryAlreadyInactive_Return400()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            // Setup the mock to return true when CategoryExistsAsync is called with the existing category ID
            repository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            // Setup the mock to return a category that is already inactive
            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Category { ID = 1, Name = "Electronics", IsActive = false });

            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.DeactivateCategory(It.IsAny<int>());

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Category is already inactive.", result.Message);

            // Verify that the repository's SaveChangesAsync method was never called since the category is already inactive
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeactivateCategory_RepositoryThrowsException_Return500()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();

            // Setup the mock to return true when CategoryExistsAsync is called with the existing category ID
            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Category { ID = It.IsAny<int>(), Name = It.IsAny<string>(), IsActive = true });

            // Setup the mock to throw an exception when SaveChangesAsync is called
            repository.Setup(repo => repo.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database error"));

            // === Act === \\
            var service = new CategoryService(repository.Object);
            var result = await service.DeactivateCategory(It.IsAny<int>());

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to deactivate category.", result.Message);

            // Verify that the repository's SaveChangesAsync method was never called since an exception occurred
            repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }
    }
}
