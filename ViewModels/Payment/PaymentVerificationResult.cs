using Online_Booking_System.Models.Payments;

namespace Online_Booking_System.ViewModels.Payment
{
    /// <summary>
    /// Returned after verifying a payment with the gateway.
    /// </summary>
    public class PaymentVerificationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public PaymentStatus Status { get; set; }
        public string? GatewayTransactionId { get; set; }

        public static PaymentVerificationResult Fail(string error, PaymentStatus status = PaymentStatus.Failed) =>
            new() { Success = false, ErrorMessage = error, Status = status };

        public static PaymentVerificationResult Ok(string gatewayId) =>
            new() { Success = true, Status = PaymentStatus.Completed, GatewayTransactionId = gatewayId };
    }
}
