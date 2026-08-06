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
            await using var transaction = await context.Database.BeginTransactionAsync();

            // Create a new instance of the CategoryRepository, passing in the database context.
            var repository = new CategoryRepository(context);

            // Create a new Category object with the necessary properties for testing.
            var category = CreateCategory("Repository Test Category");

            // Act

            // Call the CreateCategoryAsync method of the repository to persist the new category to the database.
            var createdCategory = await repository.CreateCategoryAsync(category);

            context.ChangeTracker.Clear(); // Clear the change tracker to ensure that the next retrieval is from the database and not from the in-memory context.

            // Retrieve the category from the database using its ID to verify that it was successfully persisted.
            var retrievedCategory = await context.Categories.FindAsync(createdCategory.ID);

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
            await using var transaction = await context.Database.BeginTransactionAsync();

            // Create a new Category object with the necessary properties for testing.
            var category = CreateCategory("Repository Test Category");

            // Insert category directly into the database to set up the test scenario.
            // This is done to ensure that there is an existing category in the database for the test to retrieve.
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear(); // Clear the change tracker to ensure that the next retrieval is from the database and not from the in-memory context.

            // Act

            // Create a new instance of the CategoryRepository, passing in the database context.
            var repository = new CategoryRepository(context);

            // Call the GetCategoryByIdAsync method of the repository to retrieve the category from the database using its ID.
            var retrievedCategory = await repository.GetCategoryByIdAsync(category.ID);
            
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
            await using var transaction = await context.Database.BeginTransactionAsync();
            // Act
            var repository = new CategoryRepository(context);
            var retrievedCategory = await repository.GetCategoryByIdAsync(int.MaxValue);
            // Assert
            Assert.Null(retrievedCategory);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_CategoryExists_ReturnsCategory()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var category = CreateCategory($"Get-all category {Guid.NewGuid():N}");
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
  
            // Act
            var repository = new CategoryRepository(context);
            var retrievedCategories = (await repository.GetAllCategoriesAsync()).ToList();

            // Assert
            Assert.Contains(retrievedCategories, c =>
                c.ID == category.ID &&
                c.Name == category.Name);
        }


        [Fact]
        public async Task CategoryExistsAsync_ExistingCategory_ReturnsTrue()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var createdCategory = CreateCategory("Test Category");
            await context.Categories.AddAsync(createdCategory);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Act
            var repository = new CategoryRepository(context);
            var categoryExists = await repository.CategoryExistsAsync(createdCategory.ID); // Use the ID of the created category.
            // Assert
            Assert.True(categoryExists);
        }
        [Fact]
        public async Task CategoryExistsAsync_MissingCategory_ReturnsFalse()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            // Act
            var repository = new CategoryRepository(context);
            var categoryExists = await repository.CategoryExistsAsync(int.MaxValue); // Use a non-existent ID to check for category existence.
            // Assert
            Assert.False(categoryExists);
        }

        [Fact]
        public async Task CategoryNameExistsAsync_ExistingName_ReturnsTrue()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var testCategory = CreateCategory("Test Category");
            await context.Categories.AddAsync(testCategory);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Act
            var repository = new CategoryRepository(context);
            var nameExists = await repository.CategoryNameExistsASync("Test Category");

            // Assert
            Assert.True(nameExists);
        }
        [Fact]
        public async Task CategoryNameExistsAsync_MissingName_ReturnsFalse()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            // Act
            var repository = new CategoryRepository(context);
            var nameExists = await repository.CategoryNameExistsASync("Random Category Name that doesnt exist"); // Use a non-existent category name to check for name existence.

            // Assert
            Assert.False(nameExists);
        }

        [Fact]
        public async Task OtherCategoryNameExistsAsync_OtherCategoryHasName_ReturnsTrue()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            // Create two categories with the same name to test the OtherCategoryNameExistsAsync method.
            var testCategory1 = CreateCategory("Test Category");
            await context.Categories.AddAsync(testCategory1);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var testCategory2 = CreateCategory("Test Category");
            await context.Categories.AddAsync(testCategory2);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new CategoryRepository(context);
            // Act
            var otherNameExists = await repository.OtherCategoryNameExistsAsync(testCategory2.ID, "Test Category");

            // Assert
            Assert.True(otherNameExists);
        }
        [Fact]
        public async Task OtherCategoryNameExistsAsync_CurrentCategoryOwnsName_ReturnsFalse()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var testCategoryList = new List<Category>
            {
                CreateCategory("Test Category 1"),
                CreateCategory("Test Category 2"),
                CreateCategory("Test Category 3")
            };

            await context.Categories.AddRangeAsync(testCategoryList);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new CategoryRepository(context);

            // Act
            var otherNameExists = await repository.OtherCategoryNameExistsAsync(testCategoryList[0].ID, "Test Category 1");

            // Assert
            Assert.False(otherNameExists);
        }



        [Fact]
        public async Task UpdateCategoryAsync_ValidChanges_PersistsUpdatedFields()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var testCategory = CreateCategory("Original Category");

            await context.Categories.AddAsync(testCategory);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new CategoryRepository(context);

            // Act
            testCategory.Name = "Updated Category";
            testCategory.Description = "Updated description.";
            testCategory.IsActive = false;

            await repository.UpdateCategoryAsync(testCategory);

            context.ChangeTracker.Clear(); // Clear the change tracker to ensure that the next retrieval is from the database and not from the in-memory context.

            var retrievedCategory = await context.Categories.FindAsync(testCategory.ID);
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
