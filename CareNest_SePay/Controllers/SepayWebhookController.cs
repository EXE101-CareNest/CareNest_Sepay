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

namespace CareNest_SePay.Controllers
{
    [ApiController]
    [Route("api/sepay")]
    public class SepayWebhookController : ControllerBase
    {
        private readonly ILogger<SepayWebhookController> _logger;
        private readonly IUseCaseDispatcher _dispatcher;
        private readonly IPaymentService _paymentService;

        public SepayWebhookController(ILogger<SepayWebhookController> logger, IUseCaseDispatcher dispatcher, IPaymentService paymentService)
        {
            _logger = logger;
            _dispatcher = dispatcher;
            _paymentService = paymentService;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveWebhook([FromBody] object webhookData)
        {
            try
            {
                var apiKey = Request.Headers["Authorization"].FirstOrDefault() ?? string.Empty;
                var signature = Request.Headers["X-Sepay-Signature"].FirstOrDefault();

                var command = new ProcessWebhookCommand
                {
                    WebhookData = webhookData,
                    ApiKey = apiKey,
                    Signature = signature
                };

                var transaction = await _dispatcher.DispatchAsync<ProcessWebhookCommand, SepayTransaction>(command);

                var response = BaseResponse<SepayTransaction>.SuccessResult(
                    transaction, 
                    MessageConstant.WebhookProcessed);

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized webhook request");
                var response = BaseResponse<object>.ErrorResult(ex.Message);
                return Unauthorized(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                var response = BaseResponse<object>.ErrorResult(MessageConstant.ErrorInternal);
                return StatusCode(500, response);
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


