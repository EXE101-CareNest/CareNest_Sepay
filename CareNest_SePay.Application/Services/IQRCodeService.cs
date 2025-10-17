using CareNest_SePay.Domain.Entities;

namespace CareNest_SePay.Application.Services
{
    public interface IQRCodeService
    {
        Task<string> GenerateVietQRAsync(CreateQRRequest request);
        Task<QRCodeInfo> GetQRCodeInfoAsync(string qrCode);
    }

    public class CreateQRRequest
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty; // VPBank, BIDV, etc.
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string Template { get; set; } = string.Empty; // "", "compact", "qronly"
        public bool Download { get; set; } = false; // true để tải về, false để hiển thị
    }

    public class QRCodeInfo
    {
        public string QRCode { get; set; } = string.Empty; // URL của QR code
        public string QRImageUrl { get; set; } = string.Empty; // URL để hiển thị ảnh QR
        public string VietQRCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string Template { get; set; } = string.Empty;
        public bool Download { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
