using InventoryManagementAPI.Models.Contracts.Categories;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Repositories.CategoryRepositories;

namespace InventoryManagementAPI.Tests.RepositoryTests
{
    // This class contains integration tests for CategoryRepository.
    // It uses xUnit's IClassFixture<SqlServerFixture> to share a real SQL Server
    // container across all tests in this class.
    public class CategoryRepositoryTests : IClassFixture<SqlServerFixture>
    {
        // xUnit injects the shared SQL Server fixture through this constructor.
        private readonly SqlServerFixture _fixture;
        public CategoryRepositoryTests(SqlServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CreateCategoryAsync_ValidCategory_PersistsCategory()
        {
            // Arrange

            // Create a new instance of the database context using the fixture's CreateContext method.
            // This ensures that each test has its own isolated context for database operations.
            await using var context = _fixture.CreateContext();

            // Begin a new database transaction to ensure that any changes made during the test can be rolled back, maintaining test isolation and preventing side effects on other tests.
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            // Create a new instance of the CategoryRepository, passing in the database context.
            var repository = new CategoryRepository(context);

            // Create a new Category object with the necessary properties for testing.
            var category = CreateCategory("Repository Test Category");

            // Act

            // Call the CreateCategoryAsync method of the repository to persist the new category to the database.
            var createdCategory = await repository.CreateCategoryAsync(category, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);

            context.ChangeTracker.Clear(); // Clear the change tracker to ensure that the next retrieval is from the database and not from the in-memory context.

            // Retrieve the category from the database using its ID to verify that it was successfully persisted.
            var retrievedCategory = await context.Categories.FindAsync([createdCategory.ID], CancellationToken.None);

            // Assert
            Assert.NotNull(retrievedCategory);
            Assert.True(createdCategory.ID > 0); // Ensure that the created category has a valid ID assigned by the database.
            Assert.Equal(createdCategory.ID, retrievedCategory.ID);
            Assert.Equal("Repository Test Category", retrievedCategory.Name);
            Assert.Equal("This is a test category.", retrievedCategory.Description);
            Assert.True(retrievedCategory.IsActive);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ExistingCategory_ReturnsCategory()
        {
            // Arrange
            // Create a new instance of the database context using the fixture's CreateContext method.
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            // Create a new Category object with the necessary properties for testing.
            var category = CreateCategory("Repository Test Category");

            // Insert category directly into the database to set up the test scenario.
            // This is done to ensure that there is an existing category in the database for the test to retrieve.
            context.Categories.Add(category);
            await context.SaveChangesAsync(CancellationToken.None);

            context.ChangeTracker.Clear(); // Clear the change tracker to ensure that the next retrieval is from the database and not from the in-memory context.

            // Act

            // Create a new instance of the CategoryRepository, passing in the database context.
            var repository = new CategoryRepository(context);

            // Call the GetCategoryByIdAsync method of the repository to retrieve the category from the database using its ID.
            var retrievedCategory = await repository.GetCategoryByIdAsync(category.ID, CancellationToken.None);
            
            // Assert
            Assert.NotNull(retrievedCategory);
            Assert.Equal(category.ID, retrievedCategory.ID);
            Assert.Equal("Repository Test Category", retrievedCategory.Name);
            Assert.Equal("This is a test category.", retrievedCategory.Description);
            Assert.True(retrievedCategory.IsActive);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_MissingCategory_ReturnsNull()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
            // Act
            var repository = new CategoryRepository(context);
            var retrievedCategory = await repository.GetCategoryByIdAsync(int.MaxValue, CancellationToken.None);
            // Assert
            Assert.Null(retrievedCategory);
        }

        [Theory]
        [InlineData(1, 2, 2)]
        [InlineData(2, 2, 2)]
        [InlineData(3, 2, 1)]
        [InlineData(4, 2, 0)]
        public async Task GetCategoriesAsync_PaginatesSortedMatchesAndPreservesTotal(int page, int pageSize, int expectedCount)
        {
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();
            var categories = await AddQueryCategoriesAsync(context);
            context.ChangeTracker.Clear();

            var result = await new CategoryRepository(context).GetCategoriesAsync(new CategoryQueryParameters
            {
                Search = "query", Page = page, PageSize = pageSize
            });

            Assert.Equal(5, result.TotalItems);
            Assert.Equal(expectedCount, result.Items.Count());
            Assert.Equal(categories.Take(5).OrderBy(c => c.Name).Skip((page - 1) * pageSize).Take(pageSize).Select(c => c.ID),
                result.Items.Select(c => c.ID));
            Assert.Empty(context.ChangeTracker.Entries<Category>());
        }

        [Theory]
        [InlineData("name", "asc", new[] { 1, 3, 4, 2, 0 })]
        [InlineData("name", "desc", new[] { 0, 2, 4, 3, 1 })]
        [InlineData("id", "asc", new[] { 0, 1, 2, 3, 4 })]
        [InlineData("id", "desc", new[] { 4, 3, 2, 1, 0 })]
        [InlineData("created", "asc", new[] { 1, 2, 3, 4, 0 })]
        [InlineData("created", "desc", new[] { 0, 4, 3, 1, 2 })]
        [InlineData(" CREATED ", " DESC ", new[] { 0, 4, 3, 1, 2 })]
        [InlineData("unknown", "asc", new[] { 1, 3, 4, 2, 0 })]
        public async Task GetCategoriesAsync_SortsWithStableIdTieBreaker(string sortBy, string direction, int[] expectedIndexes)
        {
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();
            var categories = await AddQueryCategoriesAsync(context);
            context.ChangeTracker.Clear();

            var result = await new CategoryRepository(context).GetCategoriesAsync(new CategoryQueryParameters
            {
                Search = "query", SortBy = sortBy, SortDirection = direction
            });

            Assert.Equal(5, result.TotalItems);
            Assert.Equal(expectedIndexes.Select(index => categories[index].ID), result.Items.Select(c => c.ID));
        }

        [Theory]
        [InlineData("  QUERY ALPHA  ", null, new[] { 1 })]
        [InlineData("  SPECIAL DESCRIPTION  ", null, new[] { 2 })]
        [InlineData("query", true, new[] { 0, 2, 4 })]
        [InlineData("query", false, new[] { 1, 3 })]
        [InlineData("special description", false, new int[] { })]
        [InlineData("does-not-exist", null, new int[] { })]
        [InlineData(null, null, new[] { 0, 1, 2, 3, 4, 5 })]
        [InlineData("   ", null, new[] { 0, 1, 2, 3, 4, 5 })]
        public async Task GetCategoriesAsync_FiltersByNameDescriptionAndStatus(string? search, bool? isActive, int[] expectedIndexes)
        {
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();
            var categories = await AddQueryCategoriesAsync(context);
            context.ChangeTracker.Clear();

            var result = await new CategoryRepository(context).GetCategoriesAsync(new CategoryQueryParameters
            {
                Search = search, IsActive = isActive, SortBy = "id"
            });

            Assert.Equal(expectedIndexes.Length, result.TotalItems);
            Assert.Equal(expectedIndexes.Select(index => categories[index].ID), result.Items.Select(c => c.ID));
        }

        [Fact]
        public async Task GetCategoriesAsync_CombinedFiltersApplyBeforePagination()
        {
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();
            var categories = await AddQueryCategoriesAsync(context);
            context.ChangeTracker.Clear();

            var result = await new CategoryRepository(context).GetCategoriesAsync(new CategoryQueryParameters
            {
                Search = "query", IsActive = true, SortBy = "name", SortDirection = "desc", Page = 2, PageSize = 1
            });

            Assert.Equal(3, result.TotalItems);
            Assert.Equal(categories[2].ID, Assert.Single(result.Items).ID);
        }

        private static async Task<Category[]> AddQueryCategoriesAsync(InventoryManagementAPI.Services.InvManDBContext context)
        {
            var categories = new[]
            {
                CreateCategory("Query Zulu"), CreateCategory("Query Alpha"),
                CreateCategory("Query Echo"), CreateCategory("Query Bravo"),
                CreateCategory("Query Delta"), CreateCategory("Unrelated")
            };
            var created = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var days = new[] { 4, 0, 0, 1, 2, 3 };
            for (var index = 0; index < categories.Length; index++)
            {
                categories[index].Created = created.AddDays(days[index]);
                categories[index].IsActive = index % 2 == 0;
                categories[index].Description = index == 2 ? "Special description" : null;
                // Save separately so identity order is explicit for sorting assertions.
                context.Categories.Add(categories[index]);
                await context.SaveChangesAsync();
            }
            return categories;
        }
        [Fact]
        public async Task CategoryExistsAsync_ExistingCategory_ReturnsTrue()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var createdCategory = CreateCategory("Test Category");
            await context.Categories.AddAsync(createdCategory, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            // Act
            var repository = new CategoryRepository(context);
            var categoryExists = await repository.CategoryExistsAsync(createdCategory.ID, CancellationToken.None); // Use the ID of the created category.
            // Assert
            Assert.True(categoryExists);
        }
        [Fact]
        public async Task CategoryExistsAsync_MissingCategory_ReturnsFalse()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            // Act
            var repository = new CategoryRepository(context);
            var categoryExists = await repository.CategoryExistsAsync(int.MaxValue, CancellationToken.None); // Use a non-existent ID to check for category existence.
            // Assert
            Assert.False(categoryExists);
        }

        [Fact]
        public async Task CategoryNameExistsAsync_ExistingName_ReturnsTrue()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var testCategory = CreateCategory("Test Category");
            await context.Categories.AddAsync(testCategory, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            // Act
            var repository = new CategoryRepository(context);
            var nameExists = await repository.CategoryNameExistsASync("Test Category", CancellationToken.None);

            // Assert
            Assert.True(nameExists);
        }
        [Fact]
        public async Task CategoryNameExistsAsync_MissingName_ReturnsFalse()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            // Act
            var repository = new CategoryRepository(context);
            var nameExists = await repository.CategoryNameExistsASync("Random Category Name that doesnt exist", CancellationToken.None); // Use a non-existent category name to check for name existence.

            // Assert
            Assert.False(nameExists);
        }

        [Fact]
        public async Task OtherCategoryNameExistsAsync_OtherCategoryHasName_ReturnsTrue()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var existingCategory = CreateCategory("Test Category");
            var currentCategory = CreateCategory("Current Category");
            await context.Categories.AddRangeAsync([existingCategory, currentCategory], CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var repository = new CategoryRepository(context);
            // Act
            var otherNameExists = await repository.OtherCategoryNameExistsAsync(currentCategory.ID, existingCategory.Name, CancellationToken.None);

            // Assert
            Assert.True(otherNameExists);
        }
        [Fact]
        public async Task OtherCategoryNameExistsAsync_CurrentCategoryOwnsName_ReturnsFalse()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var testCategoryList = new List<Category>
            {
                CreateCategory("Test Category 1"),
                CreateCategory("Test Category 2"),
                CreateCategory("Test Category 3")
            };

            await context.Categories.AddRangeAsync(testCategoryList, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var repository = new CategoryRepository(context);

            // Act
            var otherNameExists = await repository.OtherCategoryNameExistsAsync(testCategoryList[0].ID, "Test Category 1", CancellationToken.None);

            // Assert
            Assert.False(otherNameExists);
        }



        [Fact]
        public async Task UpdateCategoryAsync_ValidChanges_PersistsUpdatedFields()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var testCategory = CreateCategory("Original Category");

            await context.Categories.AddAsync(testCategory, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var repository = new CategoryRepository(context);

            // Act
            testCategory.Name = "Updated Category";
            testCategory.Description = "Updated description.";
            testCategory.IsActive = false;

            await repository.UpdateCategoryAsync(testCategory, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);

            context.ChangeTracker.Clear(); // Clear the change tracker to ensure that the next retrieval is from the database and not from the in-memory context.

            var retrievedCategory = await context.Categories.FindAsync([testCategory.ID], CancellationToken.None);
            // Assert
            Assert.NotNull(retrievedCategory);
            Assert.Equal("Updated Category", retrievedCategory.Name);
            Assert.Equal("Updated description.", retrievedCategory.Description);
            Assert.False(retrievedCategory.IsActive);
        }


        // Creates a valid category used to arrange repository test data.
        private static Category CreateCategory(string name)
        {
            return new Category
            {
                Name = name,
                Description = "This is a test category.",
                IsActive = true,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
        }
    }
}
