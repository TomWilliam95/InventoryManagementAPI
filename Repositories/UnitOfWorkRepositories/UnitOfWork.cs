using InventoryManagementAPI.Services;

namespace InventoryManagementAPI.Repositories.UnitOfWorkRepositories
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly InvManDBContext _context;
        public UnitOfWork(InvManDBContext context)
        {
            _context = context;
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
