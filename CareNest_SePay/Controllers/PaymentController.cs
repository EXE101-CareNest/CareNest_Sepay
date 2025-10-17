using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CareNest_SePay.Application.Services;
using CareNest_SePay.Application.Common;
using static CareNest_SePay.Application.Common.MessageConstant;

namespace CareNest_SePay.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IQRCodeService _qrCodeService;

        public PaymentController(ILogger<PaymentController> logger, IQRCodeService qrCodeService)
        {
            _logger = logger;
            _qrCodeService = qrCodeService;
        }

        /// <summary>
        /// Tạo mã QR cho khách hàng quét thanh toán
        /// </summary>
        /// <param name="request">Thông tin tạo QR code</param>
        /// <returns>Mã QR code và thông tin thanh toán</returns>
        [HttpPost("create-qr")]
        public async Task<IActionResult> CreateQRCode([FromBody] CreateQRRequest request)
        {
            try
            {
                _logger.LogInformation($"Creating QR code for order: {request.OrderId}, amount: {request.Amount}");

                // Validate request
                if (request.Amount <= 0)
                {
                    var errorResponse = BaseResponse<object>.ErrorResult("Số tiền phải lớn hơn 0");
                    return BadRequest(errorResponse);
                }

                if (string.IsNullOrEmpty(request.OrderId))
                {
                    var errorResponse = BaseResponse<object>.ErrorResult("Mã đơn hàng không được để trống");
                    return BadRequest(errorResponse);
                }

                // Tạo QR code URL
                var qrUrl = await _qrCodeService.GenerateVietQRAsync(request);
                
                // Lấy thông tin QR code
                var qrInfo = await _qrCodeService.GetQRCodeInfoAsync(qrUrl);
                
                // Set OrderId vào QR info
                qrInfo.OrderId = request.OrderId;

                var response = BaseResponse<QRCodeInfo>.SuccessResult(qrInfo, "Tạo mã QR thành công");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating QR code for order: {request.OrderId}");
                var response = BaseResponse<object>.ErrorResult("Có lỗi xảy ra khi tạo mã QR");
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Lấy thông tin QR code theo mã đơn hàng
        /// </summary>
        /// <param name="orderId">Mã đơn hàng</param>
        /// <returns>Thông tin QR code</returns>
        [HttpGet("qr-info/{orderId}")]
        public Task<IActionResult> GetQRCodeInfo(string orderId)
        {
            try
            {
                _logger.LogInformation($"Getting QR code info for order: {orderId}");

                if (string.IsNullOrEmpty(orderId))
                {
                    var errorResponse = BaseResponse<object>.ErrorResult("Mã đơn hàng không được để trống");
                    return Task.FromResult<IActionResult>(BadRequest(errorResponse));
                }

                // Trong thực tế, bạn sẽ lấy QR code từ database dựa trên orderId
                // Ở đây tôi sẽ tạo một QR code mẫu
                var qrInfo = new QRCodeInfo
                {
                    QRCode = $"QR_{orderId}_{DateTime.UtcNow:yyyyMMddHHmmss}",
                    VietQRCode = $"VietQR_{orderId}",
                    BankName = "SePay",
                    AccountNumber = "1234567890",
                    AccountName = "CareNest",
                    Amount = 0,
                    Description = $"Thanh toán đơn hàng {orderId}",
                    OrderId = orderId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                };

                var response = BaseResponse<QRCodeInfo>.SuccessResult(qrInfo, "Lấy thông tin QR code thành công");
                return Task.FromResult<IActionResult>(Ok(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting QR code info for order: {orderId}");
                var response = BaseResponse<object>.ErrorResult("Có lỗi xảy ra khi lấy thông tin QR code");
                return Task.FromResult<IActionResult>(StatusCode(500, response));
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái thanh toán của đơn hàng
        /// </summary>
        /// <param name="orderId">Mã đơn hàng</param>
        /// <returns>Trạng thái thanh toán</returns>
        [HttpGet("payment-status/{orderId}")]
        public Task<IActionResult> GetPaymentStatus(string orderId)
        {
            try
            {
                _logger.LogInformation($"Getting payment status for order: {orderId}");

                if (string.IsNullOrEmpty(orderId))
                {
                    var errorResponse = BaseResponse<object>.ErrorResult("Mã đơn hàng không được để trống");
                    return Task.FromResult<IActionResult>(BadRequest(errorResponse));
                }

                // Trong thực tế, bạn sẽ lấy trạng thái từ database
                var paymentStatus = new
                {
                    OrderId = orderId,
                    Status = "Pending", // Pending, Completed, Failed, Cancelled
                    Amount = 0m,
                    PaidAt = (DateTime?)null,
                    TransactionId = (string?)null,
                    Message = "Đang chờ thanh toán"
                };

                var response = BaseResponse<object>.SuccessResult(paymentStatus, "Lấy trạng thái thanh toán thành công");
                return Task.FromResult<IActionResult>(Ok(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payment status for order: {orderId}");
                var response = BaseResponse<object>.ErrorResult("Có lỗi xảy ra khi lấy trạng thái thanh toán");
                return Task.FromResult<IActionResult>(StatusCode(500, response));
            }
        }
    }
}
