namespace InventoryManagementAPI.Repositories.UnitOfWorkRepositories
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
