namespace InventoryManagementAPI.Repositories.SupplierContactRepositories
{
    public interface ISupplierContactRepository
    {
        Task<SupplierContact?> GetByIdAsync(int supplierId, int contactId, CancellationToken cancellationToken = default);
        Task<IEnumerable<SupplierContact>> GetAllBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default);
        Task<SupplierContact?> GetPrimaryBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default);
        Task<bool> SupplierExistsAsync(int supplierId, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsForSupplierAsync(int supplierId, string email, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsForOtherContactAsync(int supplierId, int contactId, string email, CancellationToken cancellationToken = default);
        Task<SupplierContact> AddAsync(SupplierContact contact, CancellationToken cancellationToken = default);
        void Remove(SupplierContact contact);
    }
}
