using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CareNest_SePay.Application.Services;
using CareNest_SePay.Application.Interfaces.Services;
using System.Text;
using System.Text.Json;
using System.Web;

namespace CareNest_SePay.Application.Services
{
    public class QRCodeService : IQRCodeService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<QRCodeService> _logger;
        private readonly ISepayAPIService _sepayAPIService;

        public QRCodeService(
            IConfiguration configuration, 
            ILogger<QRCodeService> logger,
            ISepayAPIService sepayAPIService)
        {
            _configuration = configuration;
            _logger = logger;
            _sepayAPIService = sepayAPIService;
        }

        public async Task<string> GenerateVietQRAsync(CreateQRRequest request)
        {
            try
            {
                _logger.LogInformation($"Generating VietQR for amount: {request.Amount}, order: {request.OrderId}");

                // Sử dụng URL Sepay để tạo QR code đơn giản
                var qrUrl = GenerateSepayQRUrl(request);

                _logger.LogInformation($"VietQR URL generated successfully for order: {request.OrderId}");
                return qrUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating VietQR for order: {request.OrderId}");
                throw;
            }
        }

        public Task<QRCodeInfo> GetQRCodeInfoAsync(string qrUrl)
        {
            try
            {
                // Parse QR URL để lấy thông tin
                var qrInfo = ParseSepayQRUrl(qrUrl);
                return Task.FromResult(qrInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error parsing QR URL: {qrUrl}");
                throw;
            }
        }

        private string GenerateSepayQRUrl(CreateQRRequest request)
        {
            // Sử dụng URL Sepay để tạo QR code đơn giản
            var baseUrl = "https://qr.sepay.vn/img";
            var accountNumber = request.AccountNumber ?? _configuration["Sepay:AccountNumber"] ?? "";
            var bankCode = GetBankCode(request.BankCode ?? _configuration["Sepay:BankCode"] ?? "VPBANK");
            var amount = request.Amount > 0 ? request.Amount.ToString("F0") : "";
            var description = Uri.EscapeDataString(request.Description ?? "");
            
            // Template: để trống (default), compact, qronly
            var template = request.Template ?? "";
            
            // Download: true để tải về, false hoặc để trống để hiển thị
            var download = request.Download ? "true" : "";

            var qrUrl = $"{baseUrl}?acc={accountNumber}&bank={bankCode}";
            
            if (!string.IsNullOrEmpty(amount))
                qrUrl += $"&amount={amount}";
                
            if (!string.IsNullOrEmpty(description))
                qrUrl += $"&des={description}";
                
            if (!string.IsNullOrEmpty(template))
                qrUrl += $"&template={template}";
                
            if (!string.IsNullOrEmpty(download))
                qrUrl += $"&download={download}";

            return qrUrl;
        }

        private QRCodeInfo ParseSepayQRUrl(string qrUrl)
        {
            try
            {
                // Parse QR URL để lấy thông tin
                var uri = new Uri(qrUrl);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                
                var qrInfo = new QRCodeInfo
                {
                    QRCode = qrUrl,
                    QRImageUrl = qrUrl, // URL này có thể dùng trực tiếp trong thẻ <img>
                    VietQRCode = qrUrl,
                    BankName = GetBankName(query["bank"] ?? ""),
                    BankCode = query["bank"] ?? "",
                    AccountNumber = query["acc"] ?? "",
                    AccountName = _configuration["Sepay:AccountName"] ?? "CareNest",
                    Amount = decimal.TryParse(query["amount"], out var amount) ? amount : 0,
                    Description = query["des"] ?? "",
                    OrderId = "", // Sẽ được set từ request
                    Template = query["template"] ?? "",
                    Download = query["download"] == "true",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15) // QR code hết hạn sau 15 phút
                };

                return qrInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Sepay QR URL");
                throw;
            }
        }

        private string GetBankName(string bankCode)
        {
            return bankCode.ToUpper() switch
            {
                "VPBANK" => "Ngân hàng TMCP Việt Nam Thịnh Vượng",
                "BIDV" => "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam",
                "VIETINBANK" => "Ngân hàng TMCP Công Thương Việt Nam",
                "ACB" => "Ngân hàng TMCP Á Châu",
                "OCB" => "Ngân hàng TMCP Phương Đông",
                "KIENLONGBANK" => "Ngân hàng TMCP Kiên Long",
                "MSB" => "Ngân hàng TMCP Hàng Hải",
                "TECHCOMBANK" => "Ngân hàng TMCP Kỹ Thương Việt Nam",
                "AGRIBANK" => "Ngân hàng Nông nghiệp và Phát triển Nông thôn Việt Nam",
                "MBBANK" => "Ngân hàng TMCP Quân đội",
                "SACOMBANK" => "Ngân hàng TMCP Sài Gòn Thương Tín",
                "VIETCOMBANK" => "Ngân hàng TMCP Ngoại Thương Việt Nam",
                _ => "Ngân hàng TMCP Việt Nam Thịnh Vượng"
            };
        }

        private string GetBankCode(string bankCode)
        {
            // Mapping bank codes theo chuẩn Sepay (sử dụng short_name hoặc code)
            return bankCode.ToUpper() switch
            {
                "VPBANK" or "VP" => "VPBANK",
                "BIDV" => "BIDV", 
                "VIETINBANK" or "VTB" => "VIETINBANK",
                "ACB" => "ACB",
                "OCB" => "OCB",
                "KIENLONGBANK" or "KLB" => "KIENLONGBANK",
                "MSB" => "MSB",
                "TECHCOMBANK" or "TCB" => "TECHCOMBANK",
                "AGRIBANK" or "AGB" => "AGRIBANK",
                "MBBANK" or "MB" => "MBBANK",
                "SACOMBANK" or "SCB" => "SACOMBANK",
                "VIETCOMBANK" or "VCB" => "VIETCOMBANK",
                _ => "VPBANK" // Default to VPBank
            };
        }
    }
}
