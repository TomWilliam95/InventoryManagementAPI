using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.SupplierProductDTO_s;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.SupplierProductRepositories;

public class SupplierProductService : ISupplierProductService
{
    private readonly ISupplierProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SupplierProductService(ISupplierProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<IEnumerable<SupplierProductResponseDTO>>> GetAllBySupplierAsync(
        int supplierId,
        CancellationToken ct = default
    )
    {
        try
        {
            if (!await _repository.SupplierExistsAsync(supplierId, ct))
                return Err<IEnumerable<SupplierProductResponseDTO>>(
                    "Active supplier not found.",
                    404
                );
            var list = await _repository.GetAllBySupplierIdAsync(supplierId, ct);
            if (!list.Any())
                return Err<IEnumerable<SupplierProductResponseDTO>>(
                    "No products assigned to this supplier.",
                    404
                );
            return Ok<IEnumerable<SupplierProductResponseDTO>>(
                list.Select(Map).ToList(),
                "Supplier products retrieved successfully."
            );
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Err<IEnumerable<SupplierProductResponseDTO>>(
                "Internal error occurred, failed to load supplier products.",
                500
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<SupplierProductResponseDTO>>> GetAllByProductAsync(
        int productId,
        CancellationToken ct = default
    )
    {
        try
        {
            if (!await _repository.ProductExistsAsync(productId, ct))
                return Err<IEnumerable<SupplierProductResponseDTO>>(
                    "Active product not found.",
                    404
                );
            var list = await _repository.GetAllByProductIdAsync(productId, ct);
            if (!list.Any())
                return Err<IEnumerable<SupplierProductResponseDTO>>(
                    "No suppliers assigned to this product.",
                    404
                );
            return Ok<IEnumerable<SupplierProductResponseDTO>>(
                list.Select(Map).ToList(),
                "Product suppliers retrieved successfully."
            );
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Err<IEnumerable<SupplierProductResponseDTO>>(
                "Internal error occurred, failed to load product suppliers.",
                500
            );
        }
    }

    public async Task<ApiResponse<SupplierProductResponseDTO>> GetAsync(
        int supplierId,
        int productId,
        CancellationToken ct = default
    )
    {
        try
        {
            var item = await _repository.GetAsync(supplierId, productId, ct);
            return item == null
                ? Err("Supplier product assignment not found.", 404)
                : Ok(Map(item), "Supplier product retrieved successfully.");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Err("Internal error occurred, failed to load supplier product.", 500);
        }
    }

    public async Task<ApiResponse<SupplierProductResponseDTO>> AssignAsync(
        int supplierId,
        CreateSupplierProductRequestDTO dto,
        CancellationToken ct = default
    )
    {
        if (dto == null)
            return Err("Request body is required.", 400);
        var fields = Fields(dto.UnitCost, dto.LeadTimeDays, dto.MinimumOrderQuantity);
        if (fields != null)
            return fields;
        try
        {
            if (!await _repository.SupplierExistsAsync(supplierId, ct))
                return Err("Active supplier not found.", 404);
            if (!await _repository.ProductExistsAsync(dto.ProductID, ct))
                return Err("Active product not found.", 404);
            if (await _repository.AssignmentExistsAsync(supplierId, dto.ProductID, ct))
                return Err("This product is already assigned to the supplier.", 400);
            if (
                !string.IsNullOrWhiteSpace(dto.SupplierSku)
                && await _repository.SupplierSkuExistsAsync(supplierId, dto.SupplierSku, ct)
            )
                return Err("Supplier SKU is already used for another product.", 400);
            if (
                dto.IsPreferred
                && await _repository.GetPreferredSupplierForProductAsync(dto.ProductID, ct) != null
            )
                return Err("This product already has a preferred supplier.", 400);
            var now = DateTime.UtcNow;
            var item = new SupplierProduct
            {
                SupplierID = supplierId,
                ProductID = dto.ProductID,
                SupplierSku = dto.SupplierSku,
                UnitCost = dto.UnitCost,
                LeadTimeDays = dto.LeadTimeDays,
                MinimumOrderQuantity = dto.MinimumOrderQuantity,
                IsPreferred = dto.IsPreferred,
                IsActive = true,
                Created = now,
                Updated = now,
            };
            await _repository.AddAsync(item, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            item = await _repository.GetAsync(supplierId, dto.ProductID, ct) ?? item;
            return Ok(Map(item), "Product assigned to supplier successfully.", 201);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Err("Internal error occurred, failed to assign product to supplier.", 500);
        }
    }

    public async Task<ApiResponse<SupplierProductResponseDTO>> UpdateAsync(
        int supplierId,
        int productId,
        UpdateSupplierProductRequestDTO dto,
        CancellationToken ct = default
    )
    {
        if (dto == null)
            return Err("Request body is required.", 400);
        var rv = Valid(dto.RowVersion);
        if (rv != null)
            return rv;
        var fields = Fields(dto.UnitCost, dto.LeadTimeDays, dto.MinimumOrderQuantity);
        if (fields != null)
            return fields;
        try
        {
            var item = await _repository.GetAsync(supplierId, productId, ct);
            if (item == null)
                return Err("Supplier product assignment not found.", 404);
            var match = Match(item.RowVersion, dto.RowVersion);
            if (match != null)
                return match;
            if (
                !string.IsNullOrWhiteSpace(dto.SupplierSku)
                && await _repository.SupplierSkuExistsForOtherProductAsync(
                    supplierId,
                    productId,
                    dto.SupplierSku,
                    ct
                )
            )
                return Err("Supplier SKU is already used for another product.", 400);
            if (dto.IsPreferred && !item.IsPreferred)
            {
                var preferred = await _repository.GetPreferredSupplierForProductAsync(
                    productId,
                    ct
                );
                if (preferred != null && preferred.SupplierID != supplierId)
                    return Err("This product already has a preferred supplier.", 400);
            }
            item.SupplierSku = dto.SupplierSku;
            item.UnitCost = dto.UnitCost;
            item.LeadTimeDays = dto.LeadTimeDays;
            item.MinimumOrderQuantity = dto.MinimumOrderQuantity;
            item.IsPreferred = dto.IsPreferred;
            item.Updated = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
            return Ok(Map(item), "Supplier product updated successfully.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Err("Concurrency error occurred, failed to update supplier product.", 409);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Err("Internal error occurred, failed to update supplier product.", 500);
        }
    }

    public async Task<ApiResponse<SupplierProductResponseDTO>> SetPreferredAsync(
        int supplierId,
        int productId,
        UpdateSupplierProductPreferredRequestDTO dto,
        CancellationToken ct = default
    )
    {
        if (dto == null)
            return Err("Request body is required.", 400);
        var rv = Valid(dto.RowVersion);
        if (rv != null)
            return rv;
        try
        {
            var item = await _repository.GetAsync(supplierId, productId, ct);
            if (item == null)
                return Err("Supplier product assignment not found.", 404);
            var match = Match(item.RowVersion, dto.RowVersion);
            if (match != null)
                return match;
            if (item.IsPreferred == dto.IsPreferred)
                return Err($"Preferred status is already {dto.IsPreferred}.", 400);
            if (dto.IsPreferred)
            {
                var preferred = await _repository.GetPreferredSupplierForProductAsync(
                    productId,
                    ct
                );
                if (preferred != null && preferred.SupplierID != supplierId)
                    return Err("This product already has a preferred supplier.", 400);
            }
            item.IsPreferred = dto.IsPreferred;
            item.Updated = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
            return Ok(Map(item), "Preferred supplier status updated successfully.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Err("Concurrency error occurred, failed to update supplier product.", 409);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Err("Internal error occurred, failed to update preferred status.", 500);
        }
    }

    public Task<ApiResponse<SupplierProductResponseDTO>> ActivateAsync(
        int s,
        int p,
        UpdateSupplierProductStatusRequestDTO dto,
        CancellationToken ct = default
    ) => Status(s, p, dto, true, ct);

    public Task<ApiResponse<SupplierProductResponseDTO>> DeactivateAsync(
        int s,
        int p,
        UpdateSupplierProductStatusRequestDTO dto,
        CancellationToken ct = default
    ) => Status(s, p, dto, false, ct);

    private async Task<ApiResponse<SupplierProductResponseDTO>> Status(
        int s,
        int p,
        UpdateSupplierProductStatusRequestDTO dto,
        bool active,
        CancellationToken ct
    )
    {
        if (dto == null)
            return Err("Request body is required.", 400);
        var rv = Valid(dto.RowVersion);
        if (rv != null)
            return rv;
        if (dto.IsActive != active)
            return Err(
                $"IsActive must be {active.ToString().ToLowerInvariant()} for this operation.",
                400
            );
        try
        {
            var item = await _repository.GetAsync(s, p, ct);
            if (item == null)
                return Err("Supplier product assignment not found.", 404);
            var match = Match(item.RowVersion, dto.RowVersion);
            if (match != null)
                return match;
            if (item.IsActive == active)
                return Err($"Supplier product is already {(active ? "active" : "inactive")}.", 400);
            item.IsActive = active;
            item.Updated = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
            return Ok(
                Map(item),
                $"Supplier product {(active ? "activated" : "deactivated")} successfully."
            );
        }
        catch (DbUpdateConcurrencyException)
        {
            return Err("Concurrency error occurred, failed to update supplier product.", 409);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Err("Internal error occurred, failed to update supplier product status.", 500);
        }
    }

    private static SupplierProductResponseDTO Map(SupplierProduct x) =>
        new()
        {
            SupplierID = x.SupplierID,
            SupplierName = x.Supplier?.Name ?? string.Empty,
            ProductID = x.ProductID,
            ProductSku = x.Product?.Sku ?? string.Empty,
            ProductName = x.Product?.Name ?? string.Empty,
            SupplierSku = x.SupplierSku,
            UnitCost = x.UnitCost,
            LeadTimeDays = x.LeadTimeDays,
            MinimumOrderQuantity = x.MinimumOrderQuantity,
            IsPreferred = x.IsPreferred,
            IsActive = x.IsActive,
            Created = x.Created,
            Updated = x.Updated,
            RowVersion = x.RowVersion,
        };

    private static ApiResponse<T> Ok<T>(T d, string m, int c = 200) =>
        new()
        {
            Success = true,
            Data = d,
            Message = m,
            StatusCode = c,
        };

    private static ApiResponse<T> Err<T>(string m, int c) =>
        new()
        {
            Success = false,
            Message = m,
            StatusCode = c,
        };

    private static ApiResponse<SupplierProductResponseDTO> Err(string m, int c) =>
        Err<SupplierProductResponseDTO>(m, c);

    private static ApiResponse<SupplierProductResponseDTO>? Valid(byte[] r) =>
        r == null || r.Length == 0 ? Err("RowVersion is required for concurrency control.", 400)
        : r.Length != 8 ? Err("Invalid RowVersion length. Expected 8 bytes.", 400)
        : null;

    private static ApiResponse<SupplierProductResponseDTO>? Match(byte[] a, byte[] b) =>
        a.SequenceEqual(b)
            ? null
            : Err(
                "RowVersion mismatch. The supplier product has been modified by another process.",
                409
            );

    private static ApiResponse<SupplierProductResponseDTO>? Fields(
        decimal cost,
        int lead,
        int minimum
    ) =>
        cost < 0 ? Err("Unit cost cannot be negative.", 400)
        : lead < 0 ? Err("Lead time cannot be negative.", 400)
        : minimum < 1 ? Err("Minimum order quantity must be at least 1.", 400)
        : null;
}
