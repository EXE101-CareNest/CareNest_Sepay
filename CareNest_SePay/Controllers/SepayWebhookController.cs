using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CareNest_SePay.Infrastructure.Persistence;
using CareNest_SePay.Domain.Entities;

namespace CareNest_SePay.Controllers
{
    [ApiController]
    [Route("api/sepay")]
    public class SepayWebhookController : ControllerBase
    {
        private readonly ILogger<SepayWebhookController> _logger;
        private readonly IConfiguration _configuration;
        private readonly CareNestDbContext _dbContext;

        public SepayWebhookController(ILogger<SepayWebhookController> logger, IConfiguration configuration, CareNestDbContext dbContext)
        {
            _logger = logger;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveWebhook([FromBody] object webhookData)
        {
            try
            {
                var apiKey = Request.Headers["Authorization"].FirstOrDefault();
                var expectedKey = $"Apikey {_configuration["Sepay:ApiKey"]}";

                if (apiKey != expectedKey)
                {
                    _logger.LogWarning("Invalid API Key received");
                    return Unauthorized("Invalid API Key");
                }

                _logger.LogInformation($"Webhook received: {JsonConvert.SerializeObject(webhookData)}");

                // Lưu bản ghi tối thiểu để audit
                var entity = new SepayTransaction
                {
                    Body = JsonConvert.SerializeObject(webhookData),
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.SepayTransactions.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}


