using Online_Booking_System.Models.Payments;

namespace Online_Booking_System.ViewModels.Payment
{
    public class WebhookProcessingResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public PaymentStatus? NewStatus { get; set; }
        public string? GatewayTransactionId { get; set; }

        public static WebhookProcessingResult Fail(string error) =>
            new() { Success = false, ErrorMessage = error };

        public static WebhookProcessingResult Ok(PaymentStatus status, string gatewayId) =>
            new() { Success = true, NewStatus = status, GatewayTransactionId = gatewayId };
    }
}
