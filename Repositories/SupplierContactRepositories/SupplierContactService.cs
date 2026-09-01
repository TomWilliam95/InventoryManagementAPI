using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.SupplierContactDTO_s;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.SupplierContactRepositories;

public class SupplierContactService : ISupplierContactService
{
    private readonly ISupplierContactRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SupplierContactService(ISupplierContactRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<IEnumerable<SupplierContactResponseDTO>>> GetAllAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _repository.SupplierExistsAsync(supplierId, cancellationToken))
                return ApiResponseHelper.Failure<IEnumerable<SupplierContactResponseDTO>>("Active supplier not found.", 404);
            var contacts = await _repository.GetAllBySupplierIdAsync(supplierId, cancellationToken);
            if (!contacts.Any())
                return ApiResponseHelper.Failure<IEnumerable<SupplierContactResponseDTO>>("No supplier contacts found.", 404);
            return ApiResponseHelper.Success<IEnumerable<SupplierContactResponseDTO>>(contacts.Select(Map).ToList(), "Supplier contacts retrieved successfully.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return ApiResponseHelper.Failure<IEnumerable<SupplierContactResponseDTO>>("Internal error occurred, failed to load supplier contacts.", 500);
        }
    }

    public async Task<ApiResponse<SupplierContactResponseDTO>> GetByIdAsync(int supplierId, int contactId, CancellationToken cancellationToken = default)
    {
        try
        {
            var contact = await _repository.GetByIdAsync(supplierId, contactId, cancellationToken);
            return contact == null
                ? ApiResponseHelper.Failure<SupplierContactResponseDTO>("Supplier contact not found.", 404)
                : ApiResponseHelper.Success(Map(contact), "Supplier contact retrieved successfully.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return ApiResponseHelper.Failure<SupplierContactResponseDTO>("Internal error occurred, failed to load supplier contact.", 500);
        }
    }

    public async Task<ApiResponse<SupplierContactResponseDTO>> GetPrimaryAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _repository.SupplierExistsAsync(supplierId, cancellationToken))
                return ApiResponseHelper.Failure<SupplierContactResponseDTO>("Active supplier not found.", 404);

            var contact = await _repository.GetPrimaryBySupplierIdAsync(supplierId, cancellationToken);

            return contact == null
                // If no primary contact is found, return a 404 response with a message indicating that the primary supplier contact was not found.
                ? ApiResponseHelper.Failure<SupplierContactResponseDTO>("Primary supplier contact not found.", 404)
                // If a primary contact is found, return a success response with the mapped contact data
                // and a message indicating that the primary supplier contact was retrieved successfully.
                : ApiResponseHelper.Success(Map(contact), "Primary supplier contact retrieved successfully.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return ApiResponseHelper.Failure<SupplierContactResponseDTO>("Internal error occurred, failed to load primary supplier contact.", 500);
        }
    }

    public async Task<ApiResponse<SupplierContactResponseDTO>> CreateAsync(int supplierId, CreateSupplierContactRequestDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return ApiResponseHelper.Failure<SupplierContactResponseDTO>("Request body is required.", 400);
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ApiResponseHelper.Failure<SupplierContactResponseDTO>("Contact name is required.", 400);
        try
        {
            if (!await _repository.SupplierExistsAsync(supplierId, cancellationToken))
                return ApiResponseHelper.Failure<SupplierContactResponseDTO>("Active supplier not found.", 404);

            if (!string.IsNullOrWhiteSpace(dto.Email) && await _repository.EmailExistsForSupplierAsync(supplierId, dto.Email, cancellationToken))
                return ApiResponseHelper.Failure<SupplierContactResponseDTO>("This email is already used by another contact for the supplier.", 400);

            if (dto.IsPrimary && await _repository.GetPrimaryBySupplierIdAsync(supplierId, cancellationToken) != null)
                return ApiResponseHelper.Failure<SupplierContactResponseDTO>("The supplier already has a primary contact.", 400);

            var now = DateTime.UtcNow;
            var contact = new SupplierContact
            {
                SupplierID = supplierId,
                SupplierAddressID = dto.SupplierAddressID,
                Name = dto.Name.Trim(),
                JobTitle = dto.JobTitle,
                Email = dto.Email,
                Phone = dto.Phone,
                IsPrimary = dto.IsPrimary,
                IsActive = true,
                Created = now,
                Updated = now,
            };
            await _repository.AddAsync(contact, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ApiResponseHelper.Success<SupplierContactResponseDTO>(Map(contact), "Supplier contact created successfully.", 201);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Error("Internal error occurred, failed to create supplier contact.", 500);
        }
    }

    public async Task<ApiResponse<SupplierContactResponseDTO>> UpdateAsync(int supplierId, int contactId, UpdateSupplierContactRequestDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Error("Request body is required.", 400);
        var rv = Validate(dto.RowVersion);
        if (rv != null)
            return rv;
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Error("Contact name is required.", 400);
        try
        {
            var contact = await _repository.GetByIdAsync(supplierId, contactId, cancellationToken);
            if (contact == null)
                return Error("Supplier contact not found.", 404);
            var match = Match(contact.RowVersion, dto.RowVersion);
            if (match != null)
                return match;
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _repository.EmailExistsForOtherContactAsync(supplierId, contactId, dto.Email, cancellationToken))
                return Error("This email is already used by another contact for the supplier.", 400);
            if (dto.IsPrimary && !contact.IsPrimary && await _repository.GetPrimaryBySupplierIdAsync(supplierId, cancellationToken) != null)
                return Error("The supplier already has a primary contact.", 400);
            contact.SupplierAddressID = dto.SupplierAddressID;
            contact.Name = dto.Name.Trim();
            contact.JobTitle = dto.JobTitle;
            contact.Email = dto.Email;
            contact.Phone = dto.Phone;
            contact.IsPrimary = dto.IsPrimary;
            contact.Updated = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Success(Map(contact), "Supplier contact updated successfully.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error("Concurrency error occurred, failed to update supplier contact.", 409);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Error("Internal error occurred, failed to update supplier contact.", 500);
        }
    }

    public Task<ApiResponse<SupplierContactResponseDTO>> SetPrimaryAsync(int supplierId, int contactId, UpdateSupplierContactPrimaryRequestDTO dto, CancellationToken cancellationToken = default) => SetPrimaryInternalAsync(supplierId, contactId, dto, cancellationToken);

    private async Task<ApiResponse<SupplierContactResponseDTO>> SetPrimaryInternalAsync(int supplierId, int contactId, UpdateSupplierContactPrimaryRequestDTO dto, CancellationToken ct)
    {
        if (dto == null)
            return Error("Request body is required.", 400);
        var rv = Validate(dto.RowVersion);
        if (rv != null)
            return rv;
        try
        {
            var contact = await _repository.GetByIdAsync(supplierId, contactId, ct);
            if (contact == null)
                return Error("Supplier contact not found.", 404);
            var match = Match(contact.RowVersion, dto.RowVersion);
            if (match != null)
                return match;
            if (contact.IsPrimary == dto.IsPrimary)
                return Error($"Contact primary status is already {dto.IsPrimary}.", 400);
            if (dto.IsPrimary && await _repository.GetPrimaryBySupplierIdAsync(supplierId, ct) != null)
                return Error("The supplier already has a primary contact.", 400);
            contact.IsPrimary = dto.IsPrimary;
            contact.Updated = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
            return Success(Map(contact), "Supplier contact primary status updated successfully.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error("Concurrency error occurred, failed to update supplier contact.", 409);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Error("Internal error occurred, failed to update supplier contact.", 500);
        }
    }

    public Task<ApiResponse<SupplierContactResponseDTO>> ActivateAsync(int supplierId, int contactId, UpdateSupplierContactStatusRequestDTO dto, CancellationToken cancellationToken = default) => SetStatusAsync(supplierId, contactId, dto, true, cancellationToken);

    public Task<ApiResponse<SupplierContactResponseDTO>> DeactivateAsync(int supplierId, int contactId, UpdateSupplierContactStatusRequestDTO dto, CancellationToken cancellationToken = default) => SetStatusAsync(supplierId, contactId, dto, false, cancellationToken);

    private async Task<ApiResponse<SupplierContactResponseDTO>> SetStatusAsync(int supplierId, int contactId, UpdateSupplierContactStatusRequestDTO dto, bool active, CancellationToken ct)
    {
        if (dto == null)
            return Error("Request body is required.", 400);
        var rv = Validate(dto.RowVersion);
        if (rv != null)
            return rv;
        if (dto.IsActive != active)
            return Error($"IsActive must be {active.ToString().ToLowerInvariant()} for this operation.", 400);
        try
        {
            var contact = await _repository.GetByIdAsync(supplierId, contactId, ct);
            if (contact == null)
                return Error("Supplier contact not found.", 404);
            var match = Match(contact.RowVersion, dto.RowVersion);
            if (match != null)
                return match;
            if (contact.IsActive == active)
                return Error($"Supplier contact is already {(active ? "active" : "inactive")}.", 400);
            contact.IsActive = active;
            contact.Updated = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
            return Success(Map(contact), $"Supplier contact {(active ? "activated" : "deactivated")} successfully.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error("Concurrency error occurred, failed to update supplier contact.", 409);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Error("Internal error occurred, failed to update supplier contact status.", 500);
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int supplierId, int contactId, DeleteSupplierContactRequestDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Error<bool>("Request body is required.", 400);
        if (dto.RowVersion == null || dto.RowVersion.Length != 8)
            return Error<bool>("A valid 8-byte RowVersion is required.", 400);
        try
        {
            var contact = await _repository.GetByIdAsync(supplierId, contactId, cancellationToken);
            if (contact == null)
                return Error<bool>("Supplier contact not found.", 404);
            if (!contact.RowVersion.SequenceEqual(dto.RowVersion))
                return Error<bool>("RowVersion mismatch. The contact has been modified by another process.", 409);
            _repository.Remove(contact);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Success(true, "Supplier contact deleted successfully.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error<bool>("Concurrency error occurred, failed to delete supplier contact.", 409);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Error<bool>("Internal error occurred, failed to delete supplier contact.", 500);
        }
    }

    private static SupplierContactResponseDTO Map(SupplierContact x) =>
        new()
        {
            ID = x.ID,
            SupplierID = x.SupplierID,
            SupplierAddressID = x.SupplierAddressID,
            Name = x.Name,
            JobTitle = x.JobTitle,
            Email = x.Email,
            Phone = x.Phone,
            IsPrimary = x.IsPrimary,
            IsActive = x.IsActive,
            Created = x.Created,
            Updated = x.Updated,
            RowVersion = x.RowVersion,
        };

    private static ApiResponse<T> Success<T>(T data, string message, int code = 200) =>
        ApiResponseHelper.Success(data, message, code);

    private static ApiResponse<T> Error<T>(string message, int code) =>
        ApiResponseHelper.Failure<T>(message, code);

    private static ApiResponse<SupplierContactResponseDTO> Error(string message, int code) =>
        ApiResponseHelper.Failure<SupplierContactResponseDTO>(message, code);

    private static ApiResponse<SupplierContactResponseDTO>? Validate(byte[] rowVersion) =>
        RowVersionHelper.ValidateFormat<SupplierContactResponseDTO>(rowVersion);

    private static ApiResponse<SupplierContactResponseDTO>? Match(byte[] current, byte[] supplied) => RowVersionHelper.Validate<SupplierContactResponseDTO>(current, supplied);
}
