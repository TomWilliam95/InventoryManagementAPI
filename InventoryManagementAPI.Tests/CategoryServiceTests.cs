using InventoryManagementAPI.Models.Contracts.Categories;
using InventoryManagementAPI.Models.Shared;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.CategoryDTO_s;
using InventoryManagementAPI.Repositories.CategoryRepositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Tests
{
    // CategoryService is the real class under test. ICategoryRepository is
    // mocked so these tests can simulate found, missing, duplicate, and error
    // cases without requiring a database.
    public class CategoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        // === GetSingleCategory Tests === \\
        [Fact]
        public async Task GetSingleCategory_WhenCategoryExists_Returns200()
        {
            // === Setup === \\

            //Create a mock of the ICategoryRepository interface
            var repository = new Mock<ICategoryRepository>();

            //Setup the mock to return a specific category when GetCategoryByIdAsync is called with a valid category ID
            repository
                .Setup(repo => repo.GetCategoryByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category { ID = 1, Name = "Electronics" });


            //=== Act === \\

            //Pass the mocked repository to the CategoryService
            var service = new CategoryService(repository.Object, _unitOfWork.Object);

            //Call the GetSingleCategory method with a valid category ID
            var result = await service.GetSingleCategory(1);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Category retrieved successfully.", result.Message);

            // Verify
            repository.Verify(repo => repo.GetCategoryByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
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
                .Setup(repo => repo.GetCategoryByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // === Act === \\

            // Creates an instance of the CategoryService, passing in the mocked repository
            var service = new CategoryService(repository.Object, _unitOfWork.Object);

            // Calls the GetSingleCategory method with a non-existent category ID
            var result = await service.GetSingleCategory(999);

            // === Assert === \\,

            // Asserts that the result indicates failure, has a 404 status code, and contains the expected error message
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Category not found.", result.Message);

            // Verify
            repository.Verify(repo => repo.GetCategoryByIdAsync(999, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetSingleCategory_WhenCategoryRepositoryThrowsException_Returns500()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            repository
                .Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // === Act === \\
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.GetSingleCategory(1);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to load category.", result.Message);

            // Verify
            repository.Verify(repo => repo.GetCategoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task GetCategories_ForwardsQueryAndToken_ReturnsMappedPageInRepositoryOrder()
        {
            var repository = new Mock<ICategoryRepository>();
            using var cancellation = new CancellationTokenSource();
            var query = new CategoryQueryParameters
            {
                Page = 2, PageSize = 2, Search = "tools", IsActive = false,
                SortBy = "name", SortDirection = "desc"
            };
            repository.Setup(repo => repo.GetCategoriesAsync(query, cancellation.Token))
                .ReturnsAsync(new PagedData<Category>
                {
                    Items = new[]
                    {
                        new Category { ID = 3, Name = "Tools Z", Description = "Heavy tools", IsActive = false },
                        new Category { ID = 2, Name = "Tools A", Description = null, IsActive = false }
                    },
                    TotalItems = 5
                });

            var result = await new CategoryService(repository.Object, _unitOfWork.Object)
                .GetCategories(query, cancellation.Token);

            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Categories retrieved successfully.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Page);
            Assert.Equal(2, result.Data.PageSize);
            Assert.Equal(5, result.Data.TotalItems);
            Assert.Equal(3, result.Data.TotalPages);
            Assert.True(result.Data.HasPreviousPage);
            Assert.True(result.Data.HasNextPage);
            Assert.Collection(result.Data.Items,
                item =>
                {
                    Assert.Equal("Tools Z", item.Name);
                    Assert.Equal("Heavy tools", item.Description);
                    Assert.False(item.IsActive);
                },
                item =>
                {
                    Assert.Equal("Tools A", item.Name);
                    Assert.Null(item.Description);
                    Assert.False(item.IsActive);
                });
            repository.Verify(repo => repo.GetCategoriesAsync(query, cancellation.Token), Times.Once);
            _unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData("name", "asc", null, null)]
        [InlineData("name", "desc", "tools", true)]
        [InlineData("id", "asc", "books", false)]
        [InlineData("id", "desc", null, true)]
        [InlineData("created", "asc", "electronics", null)]
        [InlineData("created", "desc", "  tools  ", false)]
        public async Task GetCategories_QueryOptions_ArePassedUnchangedToRepository(
            string sortBy, string sortDirection, string? search, bool? isActive)
        {
            var repository = new Mock<ICategoryRepository>(MockBehavior.Strict);
            var query = new CategoryQueryParameters
            {
                Page = 3, PageSize = 1, SortBy = sortBy,
                SortDirection = sortDirection, Search = search, IsActive = isActive
            };
            using var cancellation = new CancellationTokenSource();
            repository.Setup(repo => repo.GetCategoriesAsync(
                    It.Is<CategoryQueryParameters>(q =>
                        q.Page == 3 && q.PageSize == 1 && q.SortBy == sortBy &&
                        q.SortDirection == sortDirection && q.Search == search && q.IsActive == isActive),
                    cancellation.Token))
                .ReturnsAsync(new PagedData<Category>
                {
                    Items = new[] { new Category { Name = "Repository result", IsActive = isActive ?? true } },
                    TotalItems = 3
                });

            var result = await new CategoryService(repository.Object, _unitOfWork.Object)
                .GetCategories(query, cancellation.Token);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            var item = Assert.Single(result.Data.Items);
            Assert.Equal("Repository result", item.Name);
            Assert.Equal(isActive ?? true, item.IsActive);
            Assert.Equal(search, query.Search);
            Assert.Equal(sortBy, query.SortBy);
            Assert.Equal(sortDirection, query.SortDirection);
            Assert.Equal(isActive, query.IsActive);
            repository.VerifyAll();
            repository.Verify(repo => repo.GetCategoriesAsync(It.IsAny<CategoryQueryParameters>(), cancellation.Token), Times.Once);
            repository.VerifyNoOtherCalls();
            _unitOfWork.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(1, 2, 5, 2, 3, false, true)]
        [InlineData(3, 2, 5, 1, 3, true, false)]
        [InlineData(2, 2, 4, 2, 2, true, false)]
        [InlineData(1, 20, 1, 1, 1, false, false)]
        public async Task GetCategories_PageBoundaries_ReturnCorrectMetadataAndAllRepositoryItems(
            int page, int pageSize, int totalItems, int itemCount, int totalPages,
            bool hasPreviousPage, bool hasNextPage)
        {
            var repository = new Mock<ICategoryRepository>();
            var query = new CategoryQueryParameters { Page = page, PageSize = pageSize };
            var categories = Enumerable.Range(1, itemCount)
                .Select(id => new Category { ID = id, Name = $"Category {id}", IsActive = true })
                .ToArray();
            repository.Setup(repo => repo.GetCategoriesAsync(query, CancellationToken.None))
                .ReturnsAsync(new PagedData<Category> { Items = categories, TotalItems = totalItems });

            var result = await new CategoryService(repository.Object, _unitOfWork.Object).GetCategories(query);

            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal(page, result.Data.Page);
            Assert.Equal(pageSize, result.Data.PageSize);
            Assert.Equal(totalItems, result.Data.TotalItems);
            Assert.Equal(totalPages, result.Data.TotalPages);
            Assert.Equal(hasPreviousPage, result.Data.HasPreviousPage);
            Assert.Equal(hasNextPage, result.Data.HasNextPage);
            Assert.Equal(categories.Select(category => category.Name), result.Data.Items.Select(item => item.Name));
            repository.Verify(repo => repo.GetCategoriesAsync(query, CancellationToken.None), Times.Once);
            _unitOfWork.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public async Task GetCategories_EmptyPage_ReturnsNoCategoriesResponse(int totalItems)
        {
            var repository = new Mock<ICategoryRepository>();
            var query = new CategoryQueryParameters { Page = 4, PageSize = 2, Search = "missing" };
            repository.Setup(repo => repo.GetCategoriesAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedData<Category> { Items = Array.Empty<Category>(), TotalItems = totalItems });

            var result = await new CategoryService(repository.Object, _unitOfWork.Object).GetCategories(query);

            Assert.False(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("No categories found.", result.Message);
            Assert.Null(result.Data);
            repository.Verify(repo => repo.GetCategoriesAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetCategories_RepositoryThrows_Returns500()
        {
            var repository = new Mock<ICategoryRepository>();
            var query = new CategoryQueryParameters();
            repository.Setup(repo => repo.GetCategoriesAsync(query, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var result = await new CategoryService(repository.Object, _unitOfWork.Object).GetCategories(query);

            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to load categories.", result.Message);
            Assert.Null(result.Data);
            repository.Verify(repo => repo.GetCategoriesAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetCategories_RequestCancelled_PropagatesCancellation()
        {
            var repository = new Mock<ICategoryRepository>();
            var query = new CategoryQueryParameters();
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            repository.Setup(repo => repo.GetCategoriesAsync(query, cancellation.Token))
                .ThrowsAsync(new OperationCanceledException(cancellation.Token));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                new CategoryService(repository.Object, _unitOfWork.Object).GetCategories(query, cancellation.Token));
            repository.Verify(repo => repo.GetCategoriesAsync(query, cancellation.Token), Times.Once);
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
                .Setup(repo => repo.CreateCategoryAsync(It.Is<Category>(c => c.Name == "Toys" && c.Description == "Fun Stuff"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category { ID = 3, Name = "Toys", Description = "Fun Stuff" });

            // === Act === \\
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.AddCategory(newCategoryDto);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("Category added successfully.", result.Message);

            // Verify that the repository's CreateCategoryAsync method was called exactly once with the expected category
 repository.Verify(repo => repo.CreateCategoryAsync(It.Is<Category>(c => c.Name == "Toys" && c.Description == "Fun Stuff"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddCategory_WhenMissingRequestBody_Returns400()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            var service = new CategoryService(repository.Object, _unitOfWork.Object);

            // === Act === \\
            var result = await service.AddCategory(null!);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Request body is required.", result.Message);

            // Verify that the repository's CreateCategoryAsync method was never called since the request body was null
            repository.Verify(repo => repo.CreateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
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

            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.AddCategory(newCategoryDto);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Category name is required.", result.Message);

            repository.Verify(repo => repo.CreateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
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
                .Setup(repo => repo.CategoryNameExistsASync("Toys", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);


            // === Act === \\
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.AddCategory(newCategoryDto);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Category with the same name already exists.", result.Message);

            // Verify that the repository's CategoryNameExistsASync method was called exactly once with the expected category name
            repository.Verify(repo => repo.CategoryNameExistsASync("Toys", It.IsAny<CancellationToken>()), Times.Once);

            // Verify that the repository's CreateCategoryAsync method was never called since the category name already exists
            repository.Verify(repo => repo.CreateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
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
                .Setup(repo => repo.CreateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // === Act === \\
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.AddCategory(newCategoryDto);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to add category.", result.Message);

            // Verify
            repository.Verify(repo => repo.CreateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
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
                Description = "Updated Description",
                RowVersion = CreateRowVersion()
            };

            // Setup the mock to return false when OtherCategoryNameExistsAsync is called with the same category name, indicating no other category has the same name
            repository.Setup(repo => repo.OtherCategoryNameExistsAsync(It.IsAny<int>(), updateCategoryDto.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category { ID = It.IsAny<int>(), Name = It.IsAny<string>(), IsActive = true, RowVersion = CreateRowVersion() });

            // === Act === \\
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.UpdateCategoryDetails(999, updateCategoryDto);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Category updated successfully.", result.Message);

            // Verify that the repository's OtherCategoryNameExistsAsync method was called exactly once with the expected category ID and name
            repository.Verify(repo => repo.OtherCategoryNameExistsAsync(It.IsAny<int>(), "Updated Category", It.IsAny<CancellationToken>()), Times.Once);

            // Verify that the repository's UpdateCategoryAsync method was called exactly once with the expected category
            repository.Verify(repo => repo.UpdateCategoryAsync(It.Is<Category>(c => c.Name == "Updated Category" && c.Description == "Updated Description"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCategory_UpdateNonExistingCategory_Return404()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            var updateCategoryDto = new UpdateCategoryDetailsRequestDTO
            {
                Name = "Updated Category",
                Description = "Updated Description",
                RowVersion = CreateRowVersion()
            };

            // Setup the mock to return false when CategoryExistsAsync is called with a non-existing category ID
            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // === Act === \\
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.UpdateCategoryDetails(It.IsAny<int>(), updateCategoryDto);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Category not found.", result.Message);

            // Verify that the repository's UpdateCategoryAsync method was never called since the category does not exist
            repository.Verify(repo => repo.UpdateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCategory_UpdateRequestBodyNull_Return400()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            var service = new CategoryService(repository.Object, _unitOfWork.Object);

            // === Act === \\
            var result = await service.UpdateCategoryDetails(It.IsAny<int>(), null!);

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Request body is required.", result.Message);

            repository.Verify(repo => repo.UpdateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCategory_UpdateCategoryBlankName_Return400()
        {
            var repository = new Mock<ICategoryRepository>();

            var updateCategoryDto = new UpdateCategoryDetailsRequestDTO
            {
                Name = "", // Blank name
                Description = "Updated Description",
                RowVersion = CreateRowVersion()
            };

            var service = new CategoryService(repository.Object, _unitOfWork.Object);

            var result = await service.UpdateCategoryDetails(It.IsAny<int>(), updateCategoryDto);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Category name is required.", result.Message);

            repository.Verify(repo => repo.UpdateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCategory_UpdateCategoryNameAlreadyExists_Return400()
        {
            var repository = new Mock<ICategoryRepository>();
            var updateCategoryDto = new UpdateCategoryDetailsRequestDTO
            {
                Name = "Existing Category", // Name that already exists
                Description = "Updated Description",
                RowVersion = CreateRowVersion()
            };

            // Setup the mock to return true when OtherCategoryNameExistsAsync is called with the existing category name
            repository.Setup(repo => repo.OtherCategoryNameExistsAsync(It.IsAny<int>(), "Existing Category", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.UpdateCategoryDetails(It.IsAny<int>(), updateCategoryDto);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Category with the same name already exists.", result.Message);

            repository.Verify(repo => repo.OtherCategoryNameExistsAsync(It.IsAny<int>(), "Existing Category", It.IsAny<CancellationToken>()), Times.Once);
            repository.Verify(repo => repo.UpdateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCategory_RepositoryThrowException_Returns500()
        {
            var repository = new Mock<ICategoryRepository>();
            var updateCategoryDto = new UpdateCategoryDetailsRequestDTO
            {
                Name = "Updated Category",
                Description = "Updated Description",
                RowVersion = CreateRowVersion()
            };

            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category { ID = It.IsAny<int>(), Name = It.IsAny<string>(), IsActive = true, RowVersion = CreateRowVersion() });

            // Setup the mock to throw an exception when UpdateCategoryAsync is called
            repository.Setup(repo => repo.UpdateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var service = new CategoryService(repository.Object, _unitOfWork.Object);

            var result = await service.UpdateCategoryDetails(It.IsAny<int>(), updateCategoryDto);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to update category.", result.Message);

            // Verify
            repository.Verify(repo => repo.UpdateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        // === ActivateCategory Tests === \\
        [Fact]
        public async Task ActivateCategory_ActivateExistingCategory_Return200()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            // Setup the mock to return true when CategoryExistsAsync is called with the existing category ID
            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category { ID = It.IsAny<int>(), Name = It.IsAny<string>(), IsActive = false, RowVersion = CreateRowVersion() });

            // === Act === \\
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.ActivateCategory(1, new UpdateCategoryStatusRequestDTO { IsActive = false, RowVersion = CreateRowVersion() });

            // === Assert === \\
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Category activated successfully.", result.Message);


            // Verify that the repository's SaveChangesAsync method was called exactly once
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ActivateCategory_CategoryDoesntExist_Return404()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            //Setup the mock to return false when CategoryExistsAsync is called with a non-existing category ID
            repository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // === Act === \\
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.ActivateCategory(999, new UpdateCategoryStatusRequestDTO { IsActive = false, RowVersion = CreateRowVersion() });

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Category not found.", result.Message);

            //Verify that SaveChangesAsync was never called since the category does not exist
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ActivateCategory_CategoryAlreadyActive_Return400()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            // Setup the mock to return true when CategoryExistsAsync is called with the existing category ID
            repository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category { ID = 1, Name = "Electronics", IsActive = true, RowVersion = CreateRowVersion() }); // Simulate that the category is already active

            // === Act === \\
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.ActivateCategory(1, new UpdateCategoryStatusRequestDTO { IsActive = false, RowVersion = CreateRowVersion() });

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Category is already active.", result.Message);

            // Verify that SaveChangesAsync was never called since the category is already active
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ActivateCategory_RepositoryThrowsException_Return500()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();

            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category { ID = It.IsAny<int>(), Name = It.IsAny<string>(), IsActive = false, RowVersion = CreateRowVersion() });

            // Setup the mock to throw an exception when SaveChangesAsync is called
            _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // === Act === \\
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.ActivateCategory(1, new UpdateCategoryStatusRequestDTO { IsActive = false, RowVersion = CreateRowVersion() });

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to activate category.", result.Message);

            // Verify that SaveChangesAsync was never called since an exception occurred
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // === DeactivateCategory Tests === \\
        [Fact]
        public async Task DeactivateCategory_DeactivateExistingCategory_Return200()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();

            // Setup the mock to return true when CategoryExistsAsync is called with the existing category ID
            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category { ID = It.IsAny<int>(), Name = It.IsAny<string>(), IsActive = true, RowVersion = CreateRowVersion() });

            // === Act === \\
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.DeactivateCategory(1, new UpdateCategoryStatusRequestDTO { IsActive = true, RowVersion = CreateRowVersion() });

            // === Assert === \\
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Category deactivated successfully.", result.Message);

            // Verify that the repository's SaveChangesAsync method was called exactly once
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeactivateCategory_CategoryDoesntExist_Return404()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            // Setup the mock to return false when CategoryExistsAsync is called with a non-existing category ID
            repository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // === Act === \\
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.DeactivateCategory(999, new UpdateCategoryStatusRequestDTO { IsActive = true, RowVersion = CreateRowVersion() });

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Category not found.", result.Message);

            // Verify that the repository's SaveChangesAsync method was never called since the category does not exist
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeactivateCategory_CategoryAlreadyInactive_Return400()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();
            // Setup the mock to return true when CategoryExistsAsync is called with the existing category ID
            repository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            // Setup the mock to return a category that is already inactive
            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category { ID = 1, Name = "Electronics", IsActive = false, RowVersion = CreateRowVersion() });

            // === Act === \\
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.DeactivateCategory(1, new UpdateCategoryStatusRequestDTO { IsActive = true, RowVersion = CreateRowVersion() });

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Category is already inactive.", result.Message);

            // Verify that the repository's SaveChangesAsync method was never called since the category is already inactive
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeactivateCategory_RepositoryThrowsException_Return500()
        {
            // === Setup === \\
            var repository = new Mock<ICategoryRepository>();

            // Setup the mock to return true when CategoryExistsAsync is called with the existing category ID
            repository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category { ID = It.IsAny<int>(), Name = It.IsAny<string>(), IsActive = true, RowVersion = CreateRowVersion() });

            // Setup the mock to throw an exception when SaveChangesAsync is called
            _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // === Act === \\
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var result = await service.DeactivateCategory(1, new UpdateCategoryStatusRequestDTO { IsActive = true, RowVersion = CreateRowVersion() });

            // === Assert === \\
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to deactivate category.", result.Message);

            // Verify that the repository's SaveChangesAsync method was never called since an exception occurred
            _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        [Fact]
        public async Task UpdateCategory_InvalidRowVersion_Returns400()
        {
            var repository = new Mock<ICategoryRepository>();
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var request = new UpdateCategoryDetailsRequestDTO
            {
                Name = "Updated Category",
                Description = "Updated Description",
                RowVersion = [1, 2, 3, 4]
            };

            var result = await service.UpdateCategoryDetails(1, request);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            repository.Verify(repo => repo.GetCategoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            repository.Verify(repo => repo.UpdateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCategory_StaleRowVersion_Returns409()
        {
            var repository = new Mock<ICategoryRepository>();
            var category = new Category { ID = 1, Name = "Category", RowVersion = CreateRowVersion() };
            repository.Setup(repo => repo.GetCategoryByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var request = new UpdateCategoryDetailsRequestDTO
            {
                Name = "Updated Category",
                Description = "Updated Description",
                RowVersion = [8, 7, 6, 5, 4, 3, 2, 1]
            };

            var result = await service.UpdateCategoryDetails(1, request);

            Assert.False(result.Success);
            Assert.Equal(409, result.StatusCode);
            repository.Verify(repo => repo.UpdateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCategory_DatabaseConcurrencyException_Returns409()
        {
            var repository = new Mock<ICategoryRepository>();
            var category = new Category { ID = 1, Name = "Category", RowVersion = CreateRowVersion() };
            repository.Setup(repo => repo.GetCategoryByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);
            repository.Setup(repo => repo.UpdateCategoryAsync(category, It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateConcurrencyException());
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var request = new UpdateCategoryDetailsRequestDTO
            {
                Name = "Updated Category",
                Description = "Updated Description",
                RowVersion = CreateRowVersion()
            };

            var result = await service.UpdateCategoryDetails(1, request);

            Assert.False(result.Success);
            Assert.Equal(409, result.StatusCode);
            repository.Verify(repo => repo.UpdateCategoryAsync(category, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCategory_Success_ReturnsUpdatedRowVersion()
        {
            var repository = new Mock<ICategoryRepository>();
            var category = new Category { ID = 1, Name = "Category", RowVersion = CreateRowVersion() };
            byte[] updatedRowVersion = [9, 10, 11, 12, 13, 14, 15, 16];
            repository.Setup(repo => repo.GetCategoryByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);
            repository.Setup(repo => repo.UpdateCategoryAsync(category, It.IsAny<CancellationToken>()))
                .Callback((Category _, CancellationToken _) => category.RowVersion = updatedRowVersion)
                .Returns(Task.CompletedTask);
            var service = new CategoryService(repository.Object, _unitOfWork.Object);
            var request = new UpdateCategoryDetailsRequestDTO
            {
                Name = "Updated Category",
                Description = "Updated Description",
                RowVersion = CreateRowVersion()
            };

            var result = await service.UpdateCategoryDetails(1, request);

            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(updatedRowVersion, result.Data!.RowVersion);
            repository.Verify(repo => repo.UpdateCategoryAsync(category, It.IsAny<CancellationToken>()), Times.Once);
        }

        private static byte[] CreateRowVersion() => [1, 2, 3, 4, 5, 6, 7, 8];
    }
}
