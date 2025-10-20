using CareNest_SePay.Application.Services;

namespace CareNest_SePay.Application.Interfaces.Services
{
    public interface IWebhookBackgroundService
    {
        Task QueueWebhookAsync(WebhookProcessingRequest request);
    }
}
