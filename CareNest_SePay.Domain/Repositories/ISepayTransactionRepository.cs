using CareNest_SePay.Domain.Entities;
using CareNest_SePay.Domain.Commons.Enums;

namespace CareNest_SePay.Domain.Repositories
{
    public interface ISepayTransactionRepository
    {
        Task<SepayTransaction?> GetByIdAsync(string id);
        Task<SepayTransaction?> GetByTransactionIdAsync(long transactionId);
        Task<IEnumerable<SepayTransaction>> GetByStatusAsync(TransactionStatus status);
        Task<IEnumerable<SepayTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<SepayTransaction>> GetByAccountNumberAsync(string accountNumber);
        Task<SepayTransaction> AddAsync(SepayTransaction transaction);
        Task<SepayTransaction> UpdateAsync(SepayTransaction transaction);
        Task DeleteAsync(SepayTransaction transaction);
        Task<int> CountByStatusAsync(TransactionStatus status);
        Task<bool> ExistsByTransactionIdAsync(long transactionId);
    }
}
