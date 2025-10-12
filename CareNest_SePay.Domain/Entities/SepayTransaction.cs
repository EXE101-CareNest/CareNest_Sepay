using System;

namespace CareNest_SePay.Domain.Entities
{
    public class SepayTransaction
    {
        public int Id { get; set; }
        public long TransactionId { get; set; }
        public string Gateway { get; set; }
        public long TransactionDate { get; set; }
        public string AccountNumber { get; set; }
        public string SubAccount { get; set; }
        public decimal AmountIn { get; set; }
        public decimal AmountOut { get; set; }
        public decimal Accumulated { get; set; }
        public string Code { get; set; }
        public string TransactionContent { get; set; }
        public string ReferenceNumber { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}


