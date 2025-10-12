using Microsoft.Extensions.Logging;
using CareNest_SePay.Application.Interfaces.CQRS;
using CareNest_SePay.Application.Interfaces.UOW;
using CareNest_SePay.Application.Features.Queries;
using CareNest_SePay.Application.Common;
using CareNest_SePay.Domain.Entities;

namespace CareNest_SePay.Application.Features.Handlers
{
    public class GetTransactionsByDateRangeQueryHandler : IQueryHandler<GetTransactionsByDateRangeQuery, PageResult<SepayTransaction>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetTransactionsByDateRangeQueryHandler> _logger;

        public GetTransactionsByDateRangeQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetTransactionsByDateRangeQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PageResult<SepayTransaction>> HandleAsync(GetTransactionsByDateRangeQuery query)
        {
            try
            {
                var transactions = await _unitOfWork.SepayTransactionRepository
                    .FindAsync(x => x.CreatedAt >= query.StartDate && x.CreatedAt <= query.EndDate);

                var totalCount = transactions.Count();
                var items = transactions
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((query.PageIndex - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToList();

                return new PageResult<SepayTransaction>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageIndex = query.PageIndex,
                    PageSize = query.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving transactions from {query.StartDate} to {query.EndDate}");
                throw;
            }
        }
    }
}
