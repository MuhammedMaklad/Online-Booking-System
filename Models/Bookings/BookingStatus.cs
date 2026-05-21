namespace Online_Booking_System.Models.Bookings
{
    public enum BookingStatus
    {
        // Keep explicit numeric values to preserve DB enum mapping and allow safe migrations
        Pending = 0,
        Approved = 1,
        Paid = 2,

        // Old 'Cancelled' value was 3 in previous schema; map it to CancelledByUser by default
        CancelledByUser = 3,
        CancelledByOwner = 4,
        CancelledByAdmin = 5,

        RefundPending = 6,
        Refunded = 7
    }
}