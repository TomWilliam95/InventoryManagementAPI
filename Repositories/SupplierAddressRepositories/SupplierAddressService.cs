using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.SupplierAddressDTO_s;
using InventoryManagementAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.SupplierAddressRepositories;

public class SupplierAddressService : ISupplierAddressService
{
    private readonly ISupplierAddressRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SupplierAddressService(ISupplierAddressRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<IEnumerable<SupplierAddressResponseDTO>>> GetAllAsync(
        int supplierId,
        CancellationToken ct = default
    )
    {
        try
        {
            if (!await _repository.SupplierExistsAsync(supplierId, ct))
                return Err<IEnumerable<SupplierAddressResponseDTO>>(
                    "Active supplier not found.",
                    404
                );
            var list = await _repository.GetAllBySupplierIdAsync(supplierId, ct);
            if (!list.Any())
                return Err<IEnumerable<SupplierAddressResponseDTO>>(
                    "No supplier addresses found.",
                    404
                );
            return Ok<IEnumerable<SupplierAddressResponseDTO>>(
                list.Select(Map).ToList(),
                "Supplier addresses retrieved successfully."
            );
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Err<IEnumerable<SupplierAddressResponseDTO>>(
                "Internal error occurred, failed to load supplier addresses.",
                500
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<SupplierAddressResponseDTO>>> GetByTypeAsync(
        int supplierId,
        SupplierAddressType type,
        CancellationToken ct = default
    )
    {
        try
        {
            if (!await _repository.SupplierExistsAsync(supplierId, ct))
                return Err<IEnumerable<SupplierAddressResponseDTO>>(
                    "Active supplier not found.",
                    404
                );
            var list = await _repository.GetByTypeAsync(supplierId, type, ct);
            if (!list.Any())
                return Err<IEnumerable<SupplierAddressResponseDTO>>(
                    "No supplier addresses of this type found.",
                    404
                );
            return Ok<IEnumerable<SupplierAddressResponseDTO>>(
                list.Select(Map).ToList(),
                "Supplier addresses retrieved successfully."
            );
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Err<IEnumerable<SupplierAddressResponseDTO>>(
                "Internal error occurred, failed to load supplier addresses.",
                500
            );
        }
    }

    public async Task<ApiResponse<SupplierAddressResponseDTO>> GetByIdAsync(
        int supplierId,
        int addressId,
        CancellationToken ct = default
    )
    {
        try
        {
            var item = await _repository.GetByIdAsync(supplierId, addressId, ct);
            return item == null
                ? Err("Supplier address not found.", 404)
                : Ok(Map(item), "Supplier address retrieved successfully.");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Err("Internal error occurred, failed to load supplier address.", 500);
        }
    }

    public async Task<ApiResponse<SupplierAddressResponseDTO>> CreateAsync(
        int supplierId,
        CreateSupplierAddressRequestDTO dto,
        CancellationToken ct = default
    )
    {
        if (dto == null)
            return Err("Request body is required.", 400);
        var fields = Fields(dto.AddressLine1, dto.City, dto.PostalCode, dto.CountryCode);
        if (fields != null)
            return fields;
        try
        {
            if (!await _repository.SupplierExistsAsync(supplierId, ct))
                return Err("Active supplier not found.", 404);
            if (
                dto.IsPrimary
                && await _repository.GetPrimaryByTypeAsync(supplierId, dto.Type, ct) != null
            )
                return Err("The supplier already has a primary address of this type.", 400);
            var now = DateTime.UtcNow;
            var item = new SupplierAddress
            {
                SupplierID = supplierId,
                Type = dto.Type,
                AddressLine1 = dto.AddressLine1.Trim(),
                AddressLine2 = dto.AddressLine2,
                City = dto.City.Trim(),
                StateOrProvince = dto.StateOrProvince,
                PostalCode = dto.PostalCode.Trim(),
                CountryCode = dto.CountryCode.Trim().ToUpperInvariant(),
                IsPrimary = dto.IsPrimary,
                IsActive = true,
                Created = now,
                Updated = now,
            };
            await _repository.AddAsync(item, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return Ok(Map(item), "Supplier address created successfully.", 201);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Err("Internal error occurred, failed to create supplier address.", 500);
        }
    }

    public async Task<ApiResponse<SupplierAddressResponseDTO>> UpdateAsync(
        int supplierId,
        int addressId,
        UpdateSupplierAddressRequestDTO dto,
        CancellationToken ct = default
    )
    {
        if (dto == null)
            return Err("Request body is required.", 400);
        var rv = Valid(dto.RowVersion);
        if (rv != null)
            return rv;
        var fields = Fields(dto.AddressLine1, dto.City, dto.PostalCode, dto.CountryCode);
        if (fields != null)
            return fields;
        try
        {
            var item = await _repository.GetByIdAsync(supplierId, addressId, ct);
            if (item == null)
                return Err("Supplier address not found.", 404);
            var match = Match(item.RowVersion, dto.RowVersion);
            if (match != null)
                return match;
            if (dto.IsPrimary && (!item.IsPrimary || item.Type != dto.Type))
            {
                var primary = await _repository.GetPrimaryByTypeAsync(supplierId, dto.Type, ct);
                if (primary != null && primary.ID != addressId)
                    return Err("The supplier already has a primary address of this type.", 400);
            }
            item.Type = dto.Type;
            item.AddressLine1 = dto.AddressLine1.Trim();
            item.AddressLine2 = dto.AddressLine2;
            item.City = dto.City.Trim();
            item.StateOrProvince = dto.StateOrProvince;
            item.PostalCode = dto.PostalCode.Trim();
            item.CountryCode = dto.CountryCode.Trim().ToUpperInvariant();
            item.IsPrimary = dto.IsPrimary;
            item.Updated = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
            return Ok(Map(item), "Supplier address updated successfully.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Err("Concurrency error occurred, failed to update supplier address.", 409);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Err("Internal error occurred, failed to update supplier address.", 500);
        }
    }

    public async Task<ApiResponse<SupplierAddressResponseDTO>> SetPrimaryAsync(
        int supplierId,
        int addressId,
        UpdateSupplierAddressPrimaryRequestDTO dto,
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
            var item = await _repository.GetByIdAsync(supplierId, addressId, ct);
            if (item == null)
                return Err("Supplier address not found.", 404);
            var match = Match(item.RowVersion, dto.RowVersion);
            if (match != null)
                return match;
            if (item.IsPrimary == dto.IsPrimary)
                return Err($"Address primary status is already {dto.IsPrimary}.", 400);
            if (
                dto.IsPrimary
                && await _repository.GetPrimaryByTypeAsync(supplierId, item.Type, ct) != null
            )
                return Err("The supplier already has a primary address of this type.", 400);
            item.IsPrimary = dto.IsPrimary;
            item.Updated = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
            return Ok(Map(item), "Supplier address primary status updated successfully.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Err("Concurrency error occurred, failed to update supplier address.", 409);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Err("Internal error occurred, failed to update supplier address.", 500);
        }
    }

    public Task<ApiResponse<SupplierAddressResponseDTO>> ActivateAsync(
        int s,
        int a,
        UpdateSupplierAddressStatusRequestDTO dto,
        CancellationToken ct = default
    ) => Status(s, a, dto, true, ct);

    public Task<ApiResponse<SupplierAddressResponseDTO>> DeactivateAsync(
        int s,
        int a,
        UpdateSupplierAddressStatusRequestDTO dto,
        CancellationToken ct = default
    ) => Status(s, a, dto, false, ct);

    private async Task<ApiResponse<SupplierAddressResponseDTO>> Status(
        int s,
        int a,
        UpdateSupplierAddressStatusRequestDTO dto,
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
            var item = await _repository.GetByIdAsync(s, a, ct);
            if (item == null)
                return Err("Supplier address not found.", 404);
            var match = Match(item.RowVersion, dto.RowVersion);
            if (match != null)
                return match;
            if (item.IsActive == active)
                return Err($"Supplier address is already {(active ? "active" : "inactive")}.", 400);
            item.IsActive = active;
            item.Updated = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
            return Ok(
                Map(item),
                $"Supplier address {(active ? "activated" : "deactivated")} successfully."
            );
        }
        catch (DbUpdateConcurrencyException)
        {
            return Err("Concurrency error occurred, failed to update supplier address.", 409);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Err("Internal error occurred, failed to update supplier address status.", 500);
        }
    }

    private static SupplierAddressResponseDTO Map(SupplierAddress x) =>
        new()
        {
            ID = x.ID,
            SupplierID = x.SupplierID,
            Type = x.Type,
            AddressLine1 = x.AddressLine1,
            AddressLine2 = x.AddressLine2,
            City = x.City,
            StateOrProvince = x.StateOrProvince,
            PostalCode = x.PostalCode,
            CountryCode = x.CountryCode,
            IsPrimary = x.IsPrimary,
            IsActive = x.IsActive,
            Created = x.Created,
            Updated = x.Updated,
            RowVersion = x.RowVersion,
        };

    private static ApiResponse<T> Ok<T>(T data, string msg, int code = 200) =>
        new()
        {
            Success = true,
            Data = data,
            Message = msg,
            StatusCode = code,
        };

    private static ApiResponse<T> Err<T>(string msg, int code) =>
        new()
        {
            Success = false,
            Message = msg,
            StatusCode = code,
        };

    private static ApiResponse<SupplierAddressResponseDTO> Err(string m, int c) =>
        Err<SupplierAddressResponseDTO>(m, c);

    private static ApiResponse<SupplierAddressResponseDTO>? Valid(byte[] r) =>
        r == null || r.Length == 0 ? Err("RowVersion is required for concurrency control.", 400)
        : r.Length != 8 ? Err("Invalid RowVersion length. Expected 8 bytes.", 400)
        : null;

    private static ApiResponse<SupplierAddressResponseDTO>? Match(byte[] a, byte[] b) =>
        a.SequenceEqual(b)
            ? null
            : Err("RowVersion mismatch. The address has been modified by another process.", 409);

    private static ApiResponse<SupplierAddressResponseDTO>? Fields(
        string a,
        string c,
        string p,
        string country
    ) =>
        string.IsNullOrWhiteSpace(a)
        || string.IsNullOrWhiteSpace(c)
        || string.IsNullOrWhiteSpace(p)
        || string.IsNullOrWhiteSpace(country)
            ? Err("Address line 1, city, postal code and country code are required.", 400)
        : country.Trim().Length != 2 ? Err("Country code must be exactly 2 characters.", 400)
        : null;
}
