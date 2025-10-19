using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CareNest_SePay.Application.Interfaces.CQRS;
using CareNest_SePay.Application.Interfaces.Services;
using CareNest_SePay.Application.Features.Commands;
using CareNest_SePay.Application.Features.Queries;
using CareNest_SePay.Application.Common;
using CareNest_SePay.Domain.Entities;
using CareNest_SePay.Application.DTOs;
using NewtonsoftJson = Newtonsoft.Json;
using SystemTextJson = System.Text.Json;

namespace CareNest_SePay.Controllers
{
    [ApiController]
    [Route("api/sepay")]
    public class SepayWebhookController : ControllerBase
    {
        private readonly ILogger<SepayWebhookController> _logger;
        private readonly IUseCaseDispatcher _dispatcher;
        private readonly IPaymentService _paymentService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public SepayWebhookController(ILogger<SepayWebhookController> logger, IUseCaseDispatcher dispatcher, IPaymentService paymentService, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _logger = logger;
            _dispatcher = dispatcher;
            _paymentService = paymentService;
            _configuration = configuration;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveWebhook([FromBody] SystemTextJson.JsonElement webhookData)
        {
            try
            {
                _logger.LogInformation("Received SePay webhook");
                
                // Validate API Key
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                _logger.LogInformation($"Raw Authorization header: '{authHeader}'");
                
                if (string.IsNullOrEmpty(authHeader))
                {
                    _logger.LogWarning("Missing Authorization header");
                    return Unauthorized(BaseResponse<object>.ErrorResult("Missing Authorization header"));
                }

                var receivedApiKey = authHeader
                    .Replace("Apikey ", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase)
                    .Trim();

                var expectedApiKey = _configuration["Sepay:ApiKey"];
                _logger.LogInformation($"Expected API Key: '{expectedApiKey}'");
                _logger.LogInformation($"Received API Key: '{receivedApiKey}'");
                _logger.LogInformation($"API Key match: {receivedApiKey == expectedApiKey}");

                if (receivedApiKey != expectedApiKey)
                {
                    _logger.LogWarning($"Invalid API Key - Expected: '{expectedApiKey}', Received: '{receivedApiKey}'");
                    return Unauthorized(BaseResponse<object>.ErrorResult("Invalid API Key"));
                }

                _logger.LogInformation("API Key validated successfully");

                // Log raw webhook data để debug
                var rawJson = webhookData.GetRawText();
                _logger.LogInformation($"Webhook raw data: {rawJson}");

                // Parse webhook data ngay ở controller
                var webhookPayload = SystemTextJson.JsonSerializer.Deserialize<SepayWebhookPayload>(
                    rawJson, 
                    new SystemTextJson.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (webhookPayload == null)
                {
                    _logger.LogWarning("Failed to parse webhook payload");
                    return BadRequest(BaseResponse<object>.ErrorResult("Invalid webhook data format"));
                }

                _logger.LogInformation(
                    $"Parsed webhook - ID: {webhookPayload.Id}, Amount: {webhookPayload.TransferAmount}, Type: {webhookPayload.TransferType}"
                );

                // Optional signature validation: only validate if signature header is provided
                var signature = Request.Headers["X-Sepay-Signature"].FirstOrDefault();
                if (!string.IsNullOrEmpty(signature))
                {
                    var isValidSignature = await _paymentService.ValidateWebhookSignatureAsync(signature, rawJson);
                    if (!isValidSignature)
                    {
                        _logger.LogWarning("Invalid webhook signature");
                        var errorResponse = BaseResponse<object>.ErrorResult("Invalid webhook signature");
                        return Unauthorized(errorResponse);
                    }
                }

                // Truyền object đã parse vào command
                var command = new ProcessWebhookCommand
                {
                    WebhookPayload = webhookPayload,  // Truyền object đã parse
                    ApiKey = receivedApiKey,
                    Signature = signature
                };

                var transaction = await _dispatcher.DispatchAsync<ProcessWebhookCommand, SepayTransaction>(command);

                _logger.LogInformation($"Webhook processed successfully - Transaction ID: {transaction.TransactionId}, Status: {transaction.Status}");

                var response = BaseResponse<SepayTransaction>.SuccessResult(
                    transaction, 
                    "Webhook processed successfully");

                return Ok(response);
            }
            catch (SystemTextJson.JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON format in webhook");
                return BadRequest(BaseResponse<object>.ErrorResult("Invalid JSON format"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized webhook request");
                return Unauthorized(BaseResponse<object>.ErrorResult("Unauthorized"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return StatusCode(500, BaseResponse<object>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("transactions/{id}")]
        public async Task<IActionResult> GetTransactionById(string id)
        {
            try
            {
                var query = new GetTransactionByIdQuery { Id = id };
                var transaction = await _dispatcher.DispatchQueryAsync<GetTransactionByIdQuery, SepayTransaction?>(query);

                if (transaction == null)
                {
                    var notFoundResponse = BaseResponse<object>.ErrorResult(MessageConstant.ErrorNotFound);
                    return NotFound(notFoundResponse);
                }

                var response = BaseResponse<SepayTransaction>.SuccessResult(transaction, MessageConstant.SuccessGet);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving transaction with ID: {id}");
                var response = BaseResponse<object>.ErrorResult(MessageConstant.ErrorInternal);
                return StatusCode(500, response);
            }
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactionsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = new GetTransactionsByDateRangeQuery
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };

                var result = await _dispatcher.DispatchQueryAsync<GetTransactionsByDateRangeQuery, PageResult<SepayTransaction>>(query);
                var response = BaseResponse<PageResult<SepayTransaction>>.SuccessResult(result, MessageConstant.SuccessGet);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving transactions from {startDate} to {endDate}");
                var response = BaseResponse<object>.ErrorResult(MessageConstant.ErrorInternal);
                return StatusCode(500, response);
            }
        }

        [HttpPut("transactions/{id}/status")]
        public async Task<IActionResult> UpdateTransactionStatus(
            string id,
            [FromBody] UpdateTransactionStatusRequest request)
        {
            try
            {
                var command = new UpdateTransactionStatusCommand
                {
                    TransactionId = id,
                    Status = request.Status,
                    ErrorMessage = request.ErrorMessage,
                    UpdatedBy = "System" // In real app, this would come from authentication context
                };

                var transaction = await _dispatcher.DispatchAsync<UpdateTransactionStatusCommand, SepayTransaction>(command);
                var response = BaseResponse<SepayTransaction>.SuccessResult(transaction, MessageConstant.SuccessUpdate);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Transaction not found: {id}");
                var response = BaseResponse<object>.ErrorResult(ex.Message);
                return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating transaction status for ID: {id}");
                var response = BaseResponse<object>.ErrorResult(MessageConstant.ErrorInternal);
                return StatusCode(500, response);
            }
        }

        [HttpPost("test/sandbox")]
        public async Task<IActionResult> TestSandbox([FromBody] TestSandboxRequest request)
        {
            try
            {
                _logger.LogInformation("Starting sandbox test");

                // Create test transaction
                var testTransaction = await _paymentService.CreateTestTransactionAsync(
                    request.Amount, 
                    request.Description ?? "Sandbox Test Transaction"
                );

                // Send test webhook
                var webhookSent = await _paymentService.SendTestWebhookAsync(testTransaction);

                var result = new
                {
                    Transaction = testTransaction,
                    WebhookSent = webhookSent,
                    Message = "Sandbox test completed successfully"
                };

                var response = BaseResponse<object>.SuccessResult(result, "Sandbox test completed");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sandbox test");
                var response = BaseResponse<object>.ErrorResult($"Sandbox test failed: {ex.Message}");
                return StatusCode(500, response);
            }
        }

        [HttpPost("test/webhook")]
        public async Task<IActionResult> TestWebhook([FromBody] TestWebhookRequest request)
        {
            try
            {
                _logger.LogInformation("Testing webhook processing");

                // Create test webhook data
                var testWebhookData = new
                {
                    transactionId = request.TransactionId ?? new Random().Next(100000, 999999),
                    transactionDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    accountNumber = request.AccountNumber ?? "TEST_ACCOUNT",
                    subAccount = "TEST",
                    amountIn = request.Amount,
                    amountOut = 0,
                    accumulated = request.Amount,
                    code = "TEST_CODE",
                    transactionContent = request.Description ?? "Test Webhook Transaction",
                    referenceNumber = $"TEST_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
                    status = "Pending"
                };

                // Process the webhook
                var transaction = await _paymentService.ProcessPaymentAsync(testWebhookData);

                var response = BaseResponse<SepayTransaction>.SuccessResult(transaction, "Webhook test completed");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during webhook test");
                var response = BaseResponse<object>.ErrorResult($"Webhook test failed: {ex.Message}");
                return StatusCode(500, response);
            }
        }
    }

    public class UpdateTransactionStatusRequest
    {
        public Domain.Commons.Enums.TransactionStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class TestSandboxRequest
    {
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    public class TestWebhookRequest
    {
        public int? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string? AccountNumber { get; set; }
        public string? Description { get; set; }
    }
}


