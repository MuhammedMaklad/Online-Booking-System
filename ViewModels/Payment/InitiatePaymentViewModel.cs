using System.ComponentModel.DataAnnotations;
using Online_Booking_System.Models.Payments;

namespace Online_Booking_System.ViewModels.Payment
{
    /// <summary>
    /// Posted from the "Choose payment method" page.
    /// </summary>
    public class InitiatePaymentViewModel
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        public string Currency { get; set; } = "USD";
    }
}
