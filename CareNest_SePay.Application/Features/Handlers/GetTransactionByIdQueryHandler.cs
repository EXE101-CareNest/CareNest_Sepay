using Microsoft.Extensions.Logging;
using CareNest_SePay.Application.Interfaces.CQRS;
using CareNest_SePay.Application.Interfaces.UOW;
using CareNest_SePay.Application.Features.Queries;
using CareNest_SePay.Domain.Entities;

namespace CareNest_SePay.Application.Features.Handlers
{
    public class GetTransactionByIdQueryHandler : IQueryHandler<GetTransactionByIdQuery, SepayTransaction?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetTransactionByIdQueryHandler> _logger;

        public GetTransactionByIdQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetTransactionByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<SepayTransaction?> HandleAsync(GetTransactionByIdQuery query)
        {
            try
            {
                var transaction = await _unitOfWork.SepayTransactionRepository.GetByIdAsync(query.Id);
                
                if (transaction == null)
                {
                    _logger.LogWarning($"Transaction with ID {query.Id} not found");
                }

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving transaction with ID: {query.Id}");
                throw;
            }
        }
    }
}
