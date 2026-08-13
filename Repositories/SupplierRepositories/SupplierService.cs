using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.SupplierDTO_s;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.SupplierRepositories;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SupplierService(ISupplierRepository supplierRepository, IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<IEnumerable<SupplierResponseDTO>>> GetAllSuppliersAsync(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var suppliers = await _supplierRepository.GetAllSuppliersAsync(cancellationToken);
            if (!suppliers.Any())
                return Error<IEnumerable<SupplierResponseDTO>>("No suppliers found.", 404);
            return Success<IEnumerable<SupplierResponseDTO>>(
                suppliers.Select(MapToResponse).ToList(),
                "Suppliers retrieved successfully."
            );
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Error<IEnumerable<SupplierResponseDTO>>(
                "Internal error occurred, failed to load suppliers.",
                500
            );
        }
    }

    public async Task<ApiResponse<SupplierResponseDTO>> GetSupplierByIdAsync(
        int supplierId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var supplier = await _supplierRepository.GetSupplierByIdAsync(
                supplierId,
                cancellationToken
            );
            return supplier == null
                ? Error("Supplier not found.", 404)
                : Success(MapToResponse(supplier), "Supplier retrieved successfully.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Error("Internal error occurred, failed to load supplier.", 500);
        }
    }

    public async Task<ApiResponse<SupplierResponseDTO>> CreateSupplierAsync(
        CreateSupplierRequestDTO dto,
        CancellationToken cancellationToken = default
    )
    {
        if (dto == null)
            return Error("Request body is required.", 400);
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Error("Supplier name is required.", 400);
        try
        {
            if (await _supplierRepository.SupplierNameExistsAsync(dto.Name, cancellationToken))
                return Error("A supplier with the same name already exists.", 400);

            var supplier = new Supplier
            {
                Name = dto.Name.Trim(),
                TaxNumber = dto.TaxNumber,
                Website = dto.Website,
                IsActive = dto.IsActive,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
            };
            await _supplierRepository.CreateSupplierAsync(supplier, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Success(MapToResponse(supplier), "Supplier created successfully.", 201);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Error("Internal error occurred, failed to create supplier.", 500);
        }
    }

    public async Task<ApiResponse<SupplierResponseDTO>> UpdateSupplierAsync(
        int supplierId,
        UpdateSupplierRequestDTO dto,
        CancellationToken cancellationToken = default
    )
    {
        if (dto == null)
            return Error("Request body is required.", 400);
        var rowError = ValidateRowVersion(dto.RowVersion);
        if (rowError != null)
            return rowError;
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Error("Supplier name is required.", 400);
        try
        {
            var supplier = await _supplierRepository.GetSupplierByIdAsync(
                supplierId,
                cancellationToken
            );
            if (supplier == null)
                return Error("Supplier not found.", 404);
            var matchError = ValidateMatchingRowVersion(supplier.RowVersion, dto.RowVersion);
            if (matchError != null)
                return matchError;
            if (
                await _supplierRepository.SupplierNameExistsForOtherSupplierAsync(
                    supplierId,
                    dto.Name,
                    cancellationToken
                )
            )
                return Error("A supplier with the same name already exists.", 400);

            supplier.Name = dto.Name.Trim();
            supplier.TaxNumber = dto.TaxNumber;
            supplier.Website = dto.Website;
            supplier.Updated = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Success(MapToResponse(supplier), "Supplier updated successfully.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error("Concurrency error occurred, failed to update supplier.", 409);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Error("Internal error occurred, failed to update supplier.", 500);
        }
    }

    public Task<ApiResponse<SupplierResponseDTO>> ActivateSupplierAsync(
        int supplierId,
        UpdateSupplierStatusRequestDTO dto,
        CancellationToken cancellationToken = default
    ) => SetStatusAsync(supplierId, dto, true, cancellationToken);

    public Task<ApiResponse<SupplierResponseDTO>> DeactivateSupplierAsync(
        int supplierId,
        UpdateSupplierStatusRequestDTO dto,
        CancellationToken cancellationToken = default
    ) => SetStatusAsync(supplierId, dto, false, cancellationToken);

    private async Task<ApiResponse<SupplierResponseDTO>> SetStatusAsync(
        int supplierId,
        UpdateSupplierStatusRequestDTO dto,
        bool isActive,
        CancellationToken cancellationToken
    )
    {
        if (dto == null)
            return Error("Request body is required.", 400);
        var rowError = ValidateRowVersion(dto.RowVersion);
        if (rowError != null)
            return rowError;
        if (dto.IsActive != isActive)
            return Error(
                $"IsActive must be {isActive.ToString().ToLowerInvariant()} for this operation.",
                400
            );
        try
        {
            var supplier = await _supplierRepository.GetSupplierByIdAsync(
                supplierId,
                cancellationToken
            );
            if (supplier == null)
                return Error("Supplier not found.", 404);
            var matchError = ValidateMatchingRowVersion(supplier.RowVersion, dto.RowVersion);
            if (matchError != null)
                return matchError;
            if (supplier.IsActive == isActive)
                return Error($"Supplier is already {(isActive ? "active" : "inactive")}.", 400);
            supplier.IsActive = isActive;
            supplier.Updated = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Success(
                MapToResponse(supplier),
                $"Supplier {(isActive ? "activated" : "deactivated")} successfully."
            );
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error("Concurrency error occurred, failed to update supplier.", 409);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Error("Internal error occurred, failed to update supplier status.", 500);
        }
    }

    private static SupplierResponseDTO MapToResponse(Supplier supplier) =>
        new()
        {
            ID = supplier.ID,
            Name = supplier.Name,
            TaxNumber = supplier.TaxNumber,
            Website = supplier.Website,
            IsActive = supplier.IsActive,
            RowVersion = supplier.RowVersion,
        };

    private static ApiResponse<T> Success<T>(T data, string message, int statusCode = 200) =>
        new()
        {
            Success = true,
            Data = data,
            Message = message,
            StatusCode = statusCode,
        };

    private static ApiResponse<T> Error<T>(string message, int statusCode) =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
        };

    private static ApiResponse<SupplierResponseDTO> Error(string message, int statusCode) =>
        Error<SupplierResponseDTO>(message, statusCode);

    private static ApiResponse<SupplierResponseDTO>? ValidateRowVersion(byte[] rowVersion) =>
        rowVersion == null || rowVersion.Length == 0
            ? Error("RowVersion is required for concurrency control.", 400)
        : rowVersion.Length != 8 ? Error("Invalid RowVersion length. Expected 8 bytes.", 400)
        : null;

    private static ApiResponse<SupplierResponseDTO>? ValidateMatchingRowVersion(
        byte[] current,
        byte[] supplied
    ) =>
        current.SequenceEqual(supplied)
            ? null
            : Error("RowVersion mismatch. The supplier has been modified by another process.", 409);
}
