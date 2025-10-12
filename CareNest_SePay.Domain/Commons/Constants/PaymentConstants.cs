namespace CareNest_SePay.Domain.Commons.Constants
{
    public static class PaymentConstants
    {
        public const string SEPAY_WEBHOOK_SIGNATURE_HEADER = "X-Sepay-Signature";
        public const string SEPAY_API_KEY_HEADER = "Authorization";
        public const string SEPAY_API_KEY_PREFIX = "Apikey";
        
        public const int MAX_RETRY_ATTEMPTS = 3;
        public const int WEBHOOK_TIMEOUT_SECONDS = 30;
        
        public static class TransactionTypes
        {
            public const string PAYMENT = "PAYMENT";
            public const string REFUND = "REFUND";
            public const string REVERSAL = "REVERSAL";
        }
    }
}
