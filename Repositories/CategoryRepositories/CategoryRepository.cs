using InventoryManagementAPI.Models.Contracts.Categories;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.Shared;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.CategoryRepositories
{
    public class CategoryRepository: ICategoryRepository
    {
        private readonly InvManDBContext _context;
        public CategoryRepository(InvManDBContext context)
        {
            _context = context;
        }

        // === GET ===
        public async Task<Category?> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Categories.FindAsync(categoryId, cancellationToken);
        }
        public async Task<PagedData<Category>> GetCategoriesAsync(CategoryQueryParameters query, CancellationToken cancellationToken = default)
        {
            var categoryQuery = _context.Categories
                .AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();

                categoryQuery = categoryQuery.Where(c => c.Name.ToLower().Contains(search) || c.Description!.ToLower().Contains(search));
            }
            if (query.IsActive.HasValue)
            {
                categoryQuery = categoryQuery.Where(c => c.IsActive == query.IsActive.Value);
            }

            var totalCount = await categoryQuery.CountAsync(cancellationToken);
            var sortBy = query.SortBy.Trim().ToLowerInvariant();
            var descending = query.SortDirection.Trim().ToLowerInvariant() == "desc";

            categoryQuery = sortBy switch
            {
                "id" when descending => categoryQuery.OrderByDescending(c => c.ID),
                "id" => categoryQuery.OrderBy(c => c.ID),
                "created" when descending => categoryQuery.OrderByDescending(c => c.Created).ThenBy(c => c.ID),
                "created" => categoryQuery.OrderBy(c => c.Created).ThenBy(c => c.ID),
                "name" when descending => categoryQuery.OrderByDescending(c => c.Name).ThenBy(c => c.ID),
                _ => categoryQuery.OrderBy(c => c.Name).ThenBy(c => c.ID)
            };

            var recordsToSkip = (query.Page - 1) * query.PageSize;
            var categoriesList = await categoryQuery.Skip(recordsToSkip).Take(query.PageSize).ToListAsync(cancellationToken);

            return new PagedData<Category>
            {
                Items = categoriesList,
                TotalItems = totalCount,
            };
        }

        // === POST ===
        public async Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default)
        {
            await _context.Categories.AddAsync(category, cancellationToken);
            return category;
        }

        // === PUT ===
        public Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default)
        {
            _context.Categories.Update(category);
            return Task.CompletedTask;
        }

        // === CHECK EXISTENCE ===
        public async Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Categories.AnyAsync(c => c.ID == categoryId, cancellationToken);
        }

        public async Task<bool> OtherCategoryNameExistsAsync(int categoryId, string categoryName, CancellationToken cancellationToken = default)
        {
            return await _context.Categories.AnyAsync(c => c.Name == categoryName && c.ID != categoryId, cancellationToken);
        }
        public async Task<bool> CategoryNameExistsASync(string categoryName, CancellationToken cancellationToken = default)
        {
            return await _context.Categories.AnyAsync(c => c.Name == categoryName, cancellationToken);
        }

    }
}
