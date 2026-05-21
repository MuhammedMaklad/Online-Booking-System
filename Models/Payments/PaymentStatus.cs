namespace Online_Booking_System.Models.Payments
{
    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        RefundPending,
        Failed,
        Refunded,
        Cancelled
    }
}
