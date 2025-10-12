using Microsoft.Extensions.Logging;
using CareNest_SePay.Application.Interfaces.CQRS;
using CareNest_SePay.Application.Interfaces.UOW;
using CareNest_SePay.Application.Features.Commands;
using CareNest_SePay.Domain.Entities;

namespace CareNest_SePay.Application.Features.Handlers
{
    public class UpdateTransactionStatusCommandHandler : ICommandHandler<UpdateTransactionStatusCommand, SepayTransaction>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateTransactionStatusCommandHandler> _logger;

        public UpdateTransactionStatusCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdateTransactionStatusCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<SepayTransaction> HandleAsync(UpdateTransactionStatusCommand command)
        {
            try
            {
                var transaction = await _unitOfWork.SepayTransactionRepository.GetByIdAsync(command.TransactionId);
                if (transaction == null)
                {
                    throw new ArgumentException($"Transaction with ID {command.TransactionId} not found");
                }

                // Update transaction status
                transaction.Status = command.Status;
                transaction.ErrorMessage = command.ErrorMessage;
                transaction.UpdatedBy = command.UpdatedBy;
                transaction.ProcessedAt = DateTime.UtcNow;

                await _unitOfWork.SepayTransactionRepository.UpdateAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Transaction {command.TransactionId} status updated to {command.Status}");

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating transaction status for ID: {command.TransactionId}");
                throw;
            }
        }
    }
}
