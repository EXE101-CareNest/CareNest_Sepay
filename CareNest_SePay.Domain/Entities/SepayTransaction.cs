using System;
using CareNest_SePay.Domain.Commons;
using CareNest_SePay.Domain.Commons.Enums;

namespace CareNest_SePay.Domain.Entities
{
    public class SepayTransaction : BaseEntity
    {
        public long TransactionId { get; set; }
        public PaymentGateway Gateway { get; set; }
        public long TransactionDate { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string SubAccount { get; set; } = string.Empty;
        public decimal AmountIn { get; set; }
        public decimal AmountOut { get; set; }
        public decimal Accumulated { get; set; }
        public string Code { get; set; } = string.Empty;
        public string TransactionContent { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; } = 0;
        public DateTime? ProcessedAt { get; set; }
    }
}


