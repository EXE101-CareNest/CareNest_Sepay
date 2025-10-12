using Microsoft.EntityFrameworkCore;
using CareNest_SePay.Domain.Entities;
using CareNest_SePay.Domain.Repositories;
using CareNest_SePay.Domain.Commons.Enums;
using CareNest_SePay.Infrastructure.Persistence;

namespace CareNest_SePay.Infrastructure.Persistence.Repositories
{
    public class SepayTransactionRepository : GenericRepository<SepayTransaction>, ISepayTransactionRepository
    {
        public SepayTransactionRepository(CareNestDbContext context) : base(context)
        {
        }

        public async Task<SepayTransaction?> GetByIdAsync(string id)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<SepayTransaction?> GetByTransactionIdAsync(long transactionId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.TransactionId == transactionId);
        }

        public async Task<IEnumerable<SepayTransaction>> GetByStatusAsync(TransactionStatus status)
        {
            return await _dbSet.Where(x => x.Status == status).ToListAsync();
        }

        public async Task<IEnumerable<SepayTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SepayTransaction>> GetByAccountNumberAsync(string accountNumber)
        {
            return await _dbSet
                .Where(x => x.AccountNumber == accountNumber)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public new async Task<SepayTransaction> AddAsync(SepayTransaction transaction)
        {
            await _dbSet.AddAsync(transaction);
            return transaction;
        }

        public new async Task<SepayTransaction> UpdateAsync(SepayTransaction transaction)
        {
            _dbSet.Update(transaction);
            return transaction;
        }

        public new async Task DeleteAsync(SepayTransaction transaction)
        {
            _dbSet.Remove(transaction);
        }

        public async Task<int> CountByStatusAsync(TransactionStatus status)
        {
            return await _dbSet.CountAsync(x => x.Status == status);
        }

        public async Task<bool> ExistsByTransactionIdAsync(long transactionId)
        {
            return await _dbSet.AnyAsync(x => x.TransactionId == transactionId);
        }
    }
}
