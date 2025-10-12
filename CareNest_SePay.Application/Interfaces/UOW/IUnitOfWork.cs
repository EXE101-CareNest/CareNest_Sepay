using CareNest_SePay.Domain.Entities;

namespace CareNest_SePay.Application.Interfaces.UOW
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<SepayTransaction> SepayTransactionRepository { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
