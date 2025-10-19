using CareNest_SePay.Application.Interfaces.CQRS;
using CareNest_SePay.Domain.Entities;
using CareNest_SePay.Application.DTOs;

namespace CareNest_SePay.Application.Features.Commands
{
    public class ProcessWebhookCommand : ICommand<SepayTransaction>
    {
        public SepayWebhookPayload? WebhookPayload { get; set; }
        public object WebhookData { get; set; } = new();
        public string ApiKey { get; set; } = string.Empty;
        public string? Signature { get; set; }
    }
}
