using CareNest_SePay.Domain.Entities;

namespace CareNest_SePay.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<SepayTransaction> ProcessPaymentAsync(object webhookData);
        Task<SepayTransaction> UpdateTransactionStatusAsync(int transactionId, string status);
        Task<bool> ValidateWebhookSignatureAsync(string signature, string payload);
        Task<SepayTransaction> CreateTestTransactionAsync(decimal amount, string description = "Test Transaction");
        Task<bool> SendTestWebhookAsync(SepayTransaction transaction);
    }

    public interface ISepayAPIService
    {
        Task<string> GenerateQRCodeAsync(object vietQRData);
        Task<bool> SendWebhookAsync(string webhookUrl, object webhookData, string signature);
    }
}