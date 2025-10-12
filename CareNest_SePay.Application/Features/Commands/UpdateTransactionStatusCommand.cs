using CareNest_SePay.Application.Interfaces.CQRS;
using CareNest_SePay.Domain.Entities;
using CareNest_SePay.Domain.Commons.Enums;

namespace CareNest_SePay.Application.Features.Commands
{
    public class UpdateTransactionStatusCommand : ICommand<SepayTransaction>
    {
        public string TransactionId { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
