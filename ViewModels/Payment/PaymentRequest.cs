using Online_Booking_System.Models.Payments;

namespace Online_Booking_System.ViewModels.Payment
{
    /// <summary>
    /// Passed from PaymentService → IPaymentProvider when initiating a charge.
    /// </summary>
    public class PaymentRequest
    {
        public int BookingId { get; set; }
        public int TransactionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Description { get; set; } = string.Empty;

        /// <summary>Absolute URL the gateway should redirect to on success.</summary>
        public string SuccessUrl { get; set; } = string.Empty;

        /// <summary>Absolute URL the gateway should redirect to on cancel/failure.</summary>
        public string CancelUrl { get; set; } = string.Empty;

        /// <summary>Absolute URL for webhook notifications.</summary>
        public string WebhookUrl { get; set; } = string.Empty;
    }
}
