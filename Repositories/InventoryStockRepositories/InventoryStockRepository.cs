using InventoryManagementAPI.Models.Contracts.InventoryStocks;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.Shared;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.InventoryStockRepositories
{
    public class InventoryStockRepository: IInventoryStockRepository
    {
        private readonly InvManDBContext _context;
        public InventoryStockRepository(InvManDBContext context)
        {
            _context = context;
        }
        public async Task<PagedData<InventoryStock>> GetStockAsync(InventoryStockQueryParameters query, CancellationToken cancellationToken)
        {
            var stockQuery = StockWithDetails().AsNoTracking();
            return await PaginationFilteringSorting(stockQuery, query, cancellationToken);
        }

        public async Task<InventoryStock?> GetStockByProductAndWarehouseIDAsync(int productId, int warehouseId, CancellationToken cancellationToken)
        {
            return await StockWithDetails().FirstOrDefaultAsync(stock => stock.ProductID == productId && stock.WarehouseID == warehouseId, cancellationToken);
        }

        public async Task<InventoryStock> CreateInventoryStockAsync(InventoryStock stock, CancellationToken cancellationToken)
        {
            await _context.InventoryStocks.AddAsync(stock, cancellationToken);
            return stock;
        }

        public async Task<InventoryStock?> GetStockByIdAsync(int stockId, CancellationToken cancellationToken)
        {
            return await StockWithDetails().FirstOrDefaultAsync(stock => stock.ID == stockId, cancellationToken);
        }

        /// <summary>
        /// Filters, sorts, and paginates the inventory stock query based on the provided query parameters.
        /// </summary>
        /// <param name="query">The query parameters for filtering, sorting, and pagination.</param>
        /// <param name="stockQuery">The initial inventory stock query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paged data object containing the filtered, sorted, and paginated inventory stock.</returns>
        private async Task<PagedData<InventoryStock>> PaginationFilteringSorting(IQueryable<InventoryStock> stockQuery, InventoryStockQueryParameters query, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLowerInvariant();
                stockQuery = stockQuery.Where(stock => stock.Product.Name.ToLower().Contains(search) || stock.Warehouse.Name.ToLower().Contains(search));
            }
            if (query.WarehouseId.HasValue)
            {
                stockQuery = stockQuery.Where(stock => stock.WarehouseID == query.WarehouseId.Value);
            }
            if (query.ProductId.HasValue)
            {
                stockQuery = stockQuery.Where(stock => stock.ProductID == query.ProductId.Value);
            }
            // Filters Query for Inventory Stock based on BelowReorderLevel and IsActive parameters
            if (query.BelowReorderLevel.HasValue && query.BelowReorderLevel.Value)
            {
                stockQuery = stockQuery.Where(stock => stock.Quantity <= stock.ReorderLevel);
            }
            if (query.IsActive.HasValue)
            {
                stockQuery = stockQuery.Where(stock => stock.IsActive == query.IsActive.Value);
            }

            var totalCount = await stockQuery.CountAsync(cancellationToken);

            var sortBy = query.SortBy?.Trim().ToLowerInvariant();
            var descending = query.SortDirection?.Trim().ToLowerInvariant() == "desc";

            stockQuery = sortBy switch
            {
                "id" when descending => stockQuery.OrderByDescending(stock => stock.ID),
                "id" => stockQuery.OrderBy(stock => stock.ID),
                "warehouse" when descending => stockQuery.OrderByDescending(stock => stock.Warehouse.Name).ThenBy(stock => stock.ID),
                "warehouse" => stockQuery.OrderBy(stock => stock.Warehouse.Name).ThenBy(stock => stock.ID),
                "created" when descending => stockQuery.OrderByDescending(stock => stock.Created).ThenBy(stock => stock.ID),
                "created" => stockQuery.OrderBy(stock => stock.Created).ThenBy(stock => stock.ID),
                "product" when descending => stockQuery.OrderByDescending(stock => stock.Product.Name).ThenBy(stock => stock.ID),
                _ => stockQuery.OrderBy(stock => stock.Product.Name).ThenBy(stock => stock.ID),
            };

            var skipRecords = (query.Page - 1) * query.PageSize;
            var pagedStock = await stockQuery.Skip(skipRecords).Take(query.PageSize).ToListAsync(cancellationToken);

            return new PagedData<InventoryStock>
            {
                Items = pagedStock,
                TotalItems = totalCount
            };
        }

        private IQueryable<InventoryStock> StockWithDetails() => _context.InventoryStocks
            .Include(stock => stock.Product)
            .Include(stock => stock.Warehouse);
    }
}
