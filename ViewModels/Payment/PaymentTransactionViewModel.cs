using Online_Booking_System.Models.Payments;

namespace Online_Booking_System.ViewModels.Payment
{
    public class PaymentTransactionViewModel
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentStatus Status { get; set; }
        public string? GatewayTransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? FailureReason { get; set; }
    }
}
