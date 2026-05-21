namespace Online_Booking_System.ViewModels.Payment
{
    public class RefundRequestViewModel
    {
        public int TransactionId { get; set; }

        public string PropertyTitle { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "USD";

        public string Reason { get; set; } = string.Empty;
    }
}