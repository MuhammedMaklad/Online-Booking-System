namespace Online_Booking_System.ViewModels.Payment
{
    public class RefundResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? GatewayRefundId { get; set; }

        public static RefundResult Fail(string error) =>
            new() { Success = false, ErrorMessage = error };

        public static RefundResult Ok(string refundId) =>
            new() { Success = true, GatewayRefundId = refundId };
    }
}
