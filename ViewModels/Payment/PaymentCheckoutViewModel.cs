using Online_Booking_System.Models.Payments;

namespace Online_Booking_System.ViewModels.Payment
{
    /// <summary>
    /// Passed to the Stripe embedded-checkout view.
    /// </summary>
    public class PaymentCheckoutViewModel
    {
        public int TransactionId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentMethod Method { get; set; }
        public string? ClientSecret { get; set; }
        public string? PublishableKey { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
    }
}
