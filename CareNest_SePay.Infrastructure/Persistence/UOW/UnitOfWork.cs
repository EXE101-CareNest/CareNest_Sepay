using Microsoft.EntityFrameworkCore.Storage;
using CareNest_SePay.Application.Interfaces.UOW;
using CareNest_SePay.Domain.Entities;
using CareNest_SePay.Infrastructure.Persistence;
using CareNest_SePay.Infrastructure.Persistence.Repositories;

namespace CareNest_SePay.Infrastructure.Persistence.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CareNestDbContext _context;
        private IDbContextTransaction? _transaction;
        private IGenericRepository<SepayTransaction>? _sepayTransactionRepository;

        public UnitOfWork(CareNestDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<SepayTransaction> SepayTransactionRepository
        {
            get
            {
                _sepayTransactionRepository ??= new GenericRepository<SepayTransaction>(_context);
                return _sepayTransactionRepository;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
